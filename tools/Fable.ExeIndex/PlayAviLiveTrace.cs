using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Fable.Core;

namespace Fable.ExeIndex;

/// <summary>
/// Attaches to Steam <c>Fable.exe</c> and timestamps
/// the PlayAVI sites. Observation only — does not
/// change WaitEx or DirectShow.
/// </summary>
internal static class PlayAviLiveTrace
{
    public static int Run(string exePath, string outDir, string[] args)
    {
        var seconds = 30;
        var secTok = args.SkipWhile(a => a is not "--seconds").Skip(1).FirstOrDefault();
        if (secTok is not null && int.TryParse(secTok, out var parsed) && parsed > 0)
            seconds = parsed;
        var waitTok = args.SkipWhile(a => a is not "--wait").Skip(1).FirstOrDefault();
        var waitForCopyS = 240;
        if (waitTok is not null && int.TryParse(waitTok, out var waitParsed) && waitParsed > 0)
            waitForCopyS = waitParsed;
        var filter = args.SkipWhile(a => a is not "--filter").Skip(1).FirstOrDefault()
                     ?? "dream_sequence_comp";
        // Never launch Fable.exe or send keys unless the
        // operator passes --launch / --keys. Fullscreen
        // D3D + injected Enter locked the host desktop.
        var mayLaunch = args.Any(a => a is "--launch");
        var noKeys = !args.Any(a => a is "--keys");
        var useInt3 = args.Any(a => a is "--int3");
        var family = Path.Combine(outDir, "01-sections", "playavi-timeline");
        Directory.CreateDirectory(family);

        var pe = PeImage.Load(exePath);
        PlayAviTimeline.Reset("exe");
        Console.WriteLine($"live Fable.exe  {exePath}");
        Console.WriteLine($"image base 0x{pe.ImageBase:X8}  filter `{filter}`  seconds {seconds}");

        Process? launched = null;
        var attached = false;
        try
        {
            var pid = FindFablePid(exePath);
            if (pid == 0 && mayLaunch)
            {
                Console.WriteLine("launching Fable.exe (--launch). Do not use on a live desktop.");
                launched = Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = Path.GetDirectoryName(exePath) ?? ".",
                    UseShellExecute = true,
                });
                for (var i = 0; i < 60 && FindFablePid(exePath) == 0; i++)
                    Thread.Sleep(500);
                pid = FindFablePid(exePath);
                if (pid == 0)
                    pid = launched?.Id ?? 0;
                if (!noKeys)
                    _ = Task.Run(() => NudgeNewGame(pid, waitForCopyS));
                // SteamStub must unpack before any breakpoint write.
                // Hardware BPs are per-thread and need a live game thread.
                Console.WriteLine("waiting 20s for unpack before attach");
                Thread.Sleep(20_000);
                var live = FindFablePid(exePath);
                if (live != 0)
                    pid = live;
            }

            if (pid == 0)
            {
                Console.Error.WriteLine("Fable.exe is not running. Attach only — will not launch the game.");
                PlayAviTimeline.Write(family, "exe");
                return 2;
            }

            if (!Dbg.DebugActiveProcess(pid))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "DebugActiveProcess");
            attached = true;
            Dbg.DebugSetProcessKillOnExit(false);

            var session = new Session(pe, filter, useInt3);
            var firstCopy = 0L;
            var deadline = Environment.TickCount64 + waitForCopyS * 1000L;
            if (!noKeys)
                _ = Task.Run(() => NudgeNewGame(pid, waitForCopyS));
            while (true)
            {
                if (!Dbg.WaitForDebugEvent(out var ev, 500))
                {
                    if (firstCopy != 0 && Environment.TickCount64 >= firstCopy + seconds * 1000L)
                        break;
                    if (firstCopy == 0 && Environment.TickCount64 >= deadline)
                    {
                        Console.WriteLine("timeout waiting for 00A3B730 (start New Game)");
                        break;
                    }

                    continue;
                }

                var cont = Dbg.DbgContinue;
                if (ev.dwDebugEventCode == Dbg.CreateProcess)
                {
                    session.OnCreate(ev.dwProcessId, ev.u.CreateProcessInfo);
                    session.ArmAllThreads(pid);
                }
                else if (ev.dwDebugEventCode == Dbg.CreateThread)
                {
                    session.OnCreateThread(ev.u.CreateThread);
                }
                else if (ev.dwDebugEventCode == Dbg.LoadDll)
                {
                    session.OnLoadDll(ev.u.LoadDll);
                }
                else if (ev.dwDebugEventCode == Dbg.Exception)
                {
                    var code = ev.u.Exception.ExceptionRecord.ExceptionCode;
                    var addr = ev.u.Exception.ExceptionRecord.ExceptionAddress;
                    if (code == Dbg.Breakpoint || code == Dbg.SingleStep)
                    {
                        if (session.OnException(ev.dwThreadId, addr, code, out var hitCopy))
                        {
                            cont = Dbg.DbgContinue;
                            if (hitCopy && firstCopy == 0)
                            {
                                firstCopy = Environment.TickCount64;
                                Console.WriteLine("first 00A3B730 — collecting {0}s", seconds);
                            }
                        }
                    }
                    else if (ev.u.Exception.dwFirstChance != 0)
                    {
                        cont = Dbg.DbgExceptionNotHandled;
                    }
                }
                else if (ev.dwDebugEventCode is Dbg.ExitProcess or Dbg.Rip)
                {
                    Dbg.ContinueDebugEvent(ev.dwProcessId, ev.dwThreadId, cont);
                    break;
                }

                if (!Dbg.ContinueDebugEvent(ev.dwProcessId, ev.dwThreadId, cont))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "ContinueDebugEvent");

                if (firstCopy != 0 && Environment.TickCount64 >= firstCopy + seconds * 1000L)
                    break;
            }

            session.Flush();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            PlayAviTimeline.Note("error", 0, extra: ex.Message);
        }
        finally
        {
            if (attached)
            {
                try { Dbg.DebugActiveProcessStop((uint)(launched?.Id ?? FindFablePid(exePath))); }
                catch { /* already gone */ }
            }
        }

        var md = PlayAviTimeline.Write(family, "exe");
        Console.WriteLine(md);
        var csharpJsonl = Path.Combine(family, "csharp.jsonl");
        if (File.Exists(csharpJsonl))
        {
            var compare = PlayAviTimeline.WriteComparison(
                family,
                PlayAviTimeline.Snapshot(),
                PlayAviTimeline.LoadJsonl(csharpJsonl));
            Console.WriteLine(compare);
        }
        else
        {
            Console.WriteLine("no csharp.jsonl yet — run PlayAviArch --timeline then re-compare");
        }

        return PlayAviTimeline.Snapshot().Any(e => e.Kind == "copy") ? 0 : 1;
    }

    private static int FindFablePid(string exePath)
    {
        foreach (var p in Process.GetProcessesByName("Fable"))
        {
            try
            {
                if (string.Equals(p.MainModule?.FileName, exePath, StringComparison.OrdinalIgnoreCase))
                    return p.Id;
            }
            catch
            {
                return p.Id;
            }
        }

        return 0;
    }

    private static void NudgeNewGame(int pid, int seconds)
    {
        try
        {
            var end = Environment.TickCount64 + Math.Max(30, seconds) * 1000L;
            var n = 0;
            while (Environment.TickCount64 < end)
            {
                Thread.Sleep(2000);
                var live = FindFablePidFromPid(pid);
                if (live == 0)
                    continue;
                Process p;
                try { p = Process.GetProcessById(live); }
                catch { continue; }
                if (p.HasExited)
                    return;
                var hwnd = p.MainWindowHandle;
                if (hwnd == IntPtr.Zero)
                    hwnd = Dbg.FindWindow(null, "Fable");
                if (hwnd == IntPtr.Zero)
                    continue;
                Dbg.SetForegroundWindow(hwnd);
                // Legal / skip / New Game are all Enter.
                // Space skips some logo movies.
                var vk = (n % 5 == 4) ? (byte)0x20 : (byte)0x0D;
                Dbg.keybd_event(vk, 0, 0, UIntPtr.Zero);
                Thread.Sleep(40);
                Dbg.keybd_event(vk, 0, 2, UIntPtr.Zero);
                n++;
            }
        }
        catch
        {
            // Menu click is best-effort.
        }
    }

    private static int FindFablePidFromPid(int pid)
    {
        var found = FindFablePid(
            @"C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\Fable.exe");
        return found != 0 ? found : pid;
    }

    private sealed class Session
    {
        private readonly PeImage _pe;
        private readonly string _filter;
        private readonly bool _useInt3;
        private readonly Dictionary<ulong, Site> _int3 = new();
        private readonly Dictionary<int, byte> _saved = new();
        private readonly Site[] _hw = new Site[4];
        private IntPtr _process;
        private ulong _base;
        private bool _armed;
        private bool _inWindow;
        private string _path = "";
        private int _copySerial;
        private int _presentSerial;
        private int _probePresents;
        private long _lastGetStart;
        private long _lastGetEnd;
        private Site? _stepSite;

        public Session(PeImage pe, string filter, bool useInt3)
        {
            _pe = pe;
            _filter = filter;
            _useInt3 = useInt3;
        }

        public void OnCreate(uint pid, Dbg.CREATE_PROCESS_DEBUG_INFO info)
        {
            _ = pid;
            _process = info.hProcess;
            _base = (ulong)info.lpBaseOfImage;
            Console.WriteLine($"module base 0x{_base:X}  preferred 0x{_pe.ImageBase:X8}");
            EnsureHw(info.hThread);
        }

        public void OnCreateThread(Dbg.CREATE_THREAD_DEBUG_INFO info)
        {
            EnsureHw(info.hThread);
        }

        public void OnLoadDll(Dbg.LOAD_DLL_DEBUG_INFO info)
        {
            if (_process == IntPtr.Zero || info.lpImageName == IntPtr.Zero)
                return;
            try
            {
                if (!Dbg.ReadProcessMemory(_process, info.lpImageName, out var namePtr, (nuint)IntPtr.Size, out _)
                    || namePtr == IntPtr.Zero)
                    return;
                var buf = new byte[520];
                if (!Dbg.ReadProcessMemory(_process, namePtr, buf, (nuint)buf.Length, out _))
                    return;
                var name = info.fUnicode != 0
                    ? Encoding.Unicode.GetString(buf).TrimEnd('\0')
                    : Encoding.ASCII.GetString(buf).TrimEnd('\0');
                if (name.EndsWith("d3d9.dll", StringComparison.OrdinalIgnoreCase))
                    PlayAviTimeline.Note("d3d9", 0, extra: $"base=0x{(ulong)info.lpBaseOfDll:X}");
            }
            catch
            {
                // name is optional
            }
        }

        public bool OnException(uint threadId, IntPtr addr, uint code, out bool hitCopy)
        {
            hitCopy = false;
            var hThread = Dbg.OpenThread(Dbg.ThreadAllAccess, false, threadId);
            if (hThread == IntPtr.Zero)
                return false;
            try
            {
                if (!_armed)
                {
                    Arm(hThread);
                    _armed = true;
                    ArmAllThreads((int)Dbg.GetProcessId(_process));
                    return true;
                }

                EnsureHw(hThread);

                var va = (ulong)addr;
                if (code == Dbg.Breakpoint && _int3.TryGetValue(va - 1, out var int3Site))
                {
                    Handle(int3Site, hThread, isInt3: true, out hitCopy);
                    return true;
                }

                if (code == Dbg.SingleStep)
                {
                    if (_stepSite is { } step)
                    {
                        Replant(step);
                        _stepSite = null;
                    }

                    var ctx = ReadCtx(hThread);
                    for (var i = 0; i < _hw.Length; i++)
                    {
                        var site = _hw[i];
                        if (site is null)
                            continue;
                        if (va == Map(site.Rva) || (ctx.Dr6 & (1u << i)) != 0)
                        {
                            Handle(site, hThread, isInt3: false, out hitCopy);
                            ctx.Dr6 = 0;
                            WriteCtx(hThread, ctx);
                            return true;
                        }
                    }

                    return true;
                }

                return false;
            }
            finally
            {
                Dbg.CloseHandle(hThread);
            }
        }

        public void Flush() { }

        private void EnsureHw(IntPtr hThread)
        {
            if (hThread == IntPtr.Zero || _hw[0] is null)
                return;
            try
            {
                ApplyHw(hThread);
            }
            catch
            {
                // Thread may already have exited.
            }
        }

        private void ApplyHw(IntPtr hThread)
        {
            var ctx = ReadCtx(hThread);
            ctx.Dr7 = 0x00000055;
            ctx.Dr0 = (uint)_hw[0].Addr;
            ctx.Dr1 = (uint)_hw[1].Addr;
            ctx.Dr2 = (uint)_hw[2].Addr;
            ctx.Dr3 = (uint)_hw[3].Addr;
            WriteCtx(hThread, ctx);
        }

        private void Arm(IntPtr hThread)
        {
            Site[] hw =
            [
                new("copy", PlayAviTimeline.SiteCopy, 0x00A3B730),
                new("wait-leave", PlayAviTimeline.SiteWaitLeave, 0x00628AAC),
                new("present-leave", PlayAviTimeline.SitePresentLeave, 0x009BEF10),
                new("advise", PlayAviTimeline.SiteAdviseTime, 0x00CA4B07),
            ];
            Site[] soft =
            [
                new("setevent", PlayAviTimeline.SiteSetEvent, 0x00A3B8EB),
                new("wait-enter", PlayAviTimeline.SiteWaitEnter, 0x00628A9E),
                new("beginscene", PlayAviTimeline.SiteBeginScene, 0x009BEF20),
                new("blit", PlayAviTimeline.SiteBlit, 0x009DC870),
                new("endscene", PlayAviTimeline.SiteEndScene, 0x009BEF50),
                new("present-enter", PlayAviTimeline.SitePresentEnter, 0x009BEEB0),
                new("advise", PlayAviTimeline.SiteAdviseTime, 0x00CA4B07),
                new("dorender", PlayAviTimeline.SiteDoRender, 0x00A3BCF0),
                new("open", PlayAviTimeline.SiteOpen, 0x00A3B9D0),
                new("schedule", PlayAviTimeline.SiteSchedule, 0x00CA4AA0),
            ];

            var ctx = ReadCtx(hThread);
            ctx.Dr7 = 0x00000055;
            for (var i = 0; i < hw.Length; i++)
            {
                var addr = Map(hw[i].Rva);
                hw[i].Addr = addr;
                _hw[i] = hw[i];
                switch (i)
                {
                    case 0: ctx.Dr0 = (uint)addr; break;
                    case 1: ctx.Dr1 = (uint)addr; break;
                    case 2: ctx.Dr2 = (uint)addr; break;
                    case 3: ctx.Dr3 = (uint)addr; break;
                }
            }

            WriteCtx(hThread, ctx);

            if (_useInt3)
            {
                foreach (var site in soft)
                {
                    site.Addr = Map(site.Rva);
                    PlantInt3(site);
                    _int3[site.Addr] = site;
                }
            }

            Console.WriteLine($"armed 4 hardware + {_int3.Count} int3 sites at base 0x{_base:X}");
        }

        public void ArmAllThreads(int pid)
        {
            if (_hw[0] is null || pid == 0)
                return;
            var snap = Dbg.CreateToolhelp32Snapshot(Dbg.SnapThread, 0);
            if (snap == IntPtr.Zero || snap == new IntPtr(-1))
                return;
            try
            {
                var te = new Dbg.THREADENTRY32 { dwSize = (uint)Marshal.SizeOf<Dbg.THREADENTRY32>() };
                if (!Dbg.Thread32First(snap, ref te))
                    return;
                var n = 0;
                do
                {
                    if (te.th32OwnerProcessID != (uint)pid)
                        continue;
                    var ht = Dbg.OpenThread(Dbg.ThreadAllAccess, false, te.th32ThreadID);
                    if (ht == IntPtr.Zero)
                        continue;
                    try
                    {
                        ApplyHw(ht);
                        n++;
                    }
                    catch
                    {
                        // skip
                    }
                    finally
                    {
                        Dbg.CloseHandle(ht);
                    }
                } while (Dbg.Thread32Next(snap, ref te));
                Console.WriteLine($"hardware BPs on {n} threads");
            }
            finally
            {
                Dbg.CloseHandle(snap);
            }
        }

        private void Handle(Site site, IntPtr hThread, bool isInt3, out bool hitCopy)
        {
            hitCopy = false;
            var ctx = ReadCtx(hThread);
            if (isInt3)
            {
                Restore(site);
                ctx.Eip = (uint)site.Addr;
                ctx.EFlags |= 0x100;
                _stepSite = site;
                WriteCtx(hThread, ctx);
            }

            switch (site.Kind)
            {
                case "open":
                    _path = ReadOpenPath(ctx);
                    PlayAviTimeline.NotePath(_path);
                    _inWindow = _path.Contains(_filter, StringComparison.OrdinalIgnoreCase);
                    Console.WriteLine($"open `{_path}` window={_inWindow}");
                    break;
                case "gettime":
                    if (TryReadGetTime(ctx, out var start, out var end))
                    {
                        _lastGetStart = start;
                        _lastGetEnd = end;
                    }

                    if (_inWindow)
                    {
                        PlayAviTimeline.Note(
                            "gettime",
                            site.Rva,
                            _copySerial + 1,
                            _lastGetStart,
                            _lastGetEnd,
                            threadId: unchecked((int)Dbg.GetThreadId(hThread)));
                    }

                    break;
                case "copy":
                    _copySerial++;
                    hitCopy = _inWindow || _copySerial == 1;
                    if (_inWindow || string.IsNullOrEmpty(_path))
                    {
                        if (string.IsNullOrEmpty(_path))
                            _inWindow = true;
                        PlayAviTimeline.Note(
                            "copy",
                            site.Rva,
                            _copySerial,
                            _lastGetStart,
                            _lastGetEnd,
                            threadId: unchecked((int)Dbg.GetThreadId(hThread)));
                    }

                    break;
                case "wait-leave":
                    if (_inWindow)
                    {
                        PlayAviTimeline.Note(
                            "wait-leave",
                            site.Rva,
                            _copySerial,
                            _lastGetStart,
                            _lastGetEnd,
                            waitResult: unchecked((int)ctx.Eax),
                            threadId: unchecked((int)Dbg.GetThreadId(hThread)));
                    }

                    break;
                case "present-leave":
                case "present-enter":
                    _presentSerial = _copySerial;
                    if (_inWindow || _probePresents < 8)
                    {
                        if (!_inWindow)
                            _probePresents++;
                        PlayAviTimeline.Note(
                            site.Kind,
                            site.Rva,
                            _presentSerial,
                            _lastGetStart,
                            _lastGetEnd,
                            threadId: unchecked((int)Dbg.GetThreadId(hThread)),
                            extra: _inWindow ? _path : "pre-playavi");
                    }

                    break;
                default:
                    if (_inWindow)
                    {
                        PlayAviTimeline.Note(
                            site.Kind,
                            site.Rva,
                            _copySerial,
                            _lastGetStart,
                            _lastGetEnd,
                            threadId: unchecked((int)Dbg.GetThreadId(hThread)));
                    }

                    break;
            }
        }

        private bool TryReadGetTime(Dbg.WOW64_CONTEXT ctx, out long start, out long end)
        {
            start = 0;
            end = 0;
            // 00CA49F0: [esp+12]=sample after push ebx. At entry
            // stdcall thiscall: ecx=this, [esp+4]=sample.
            var sample = ReadU32(_process, ctx.Esp + 4);
            if (sample == 0)
                return false;
            // IMediaSample times are not at a fixed offset.
            // After the call [eax+20] GetTime writes the two
            // out pointers pushed as start/end. We cannot see
            // them at function entry. Record the sample pointer.
            PlayAviTimeline.Note("gettime-sample", PlayAviTimeline.SiteGetSampleTimes, extra: $"sample=0x{sample:X8}");
            return false;
        }

        private string ReadOpenPath(Dbg.WOW64_CONTEXT ctx)
        {
            // 00A3B9D0: [esp+8] is often the CString / wchar path.
            foreach (var off in new uint[] { 4, 8, 12, 16 })
            {
                var p = ReadU32(_process, ctx.Esp + off);
                if (p == 0)
                    continue;
                var s = ReadWString(_process, p, 260);
                if (s.Contains(".wmv", StringComparison.OrdinalIgnoreCase) ||
                    s.Contains(".xmv", StringComparison.OrdinalIgnoreCase) ||
                    s.Contains("Video", StringComparison.OrdinalIgnoreCase))
                    return s;
                var inner = ReadU32(_process, p);
                if (inner != 0)
                {
                    s = ReadWString(_process, inner, 260);
                    if (s.Contains(".wmv", StringComparison.OrdinalIgnoreCase) ||
                        s.Contains(".xmv", StringComparison.OrdinalIgnoreCase) ||
                        s.Contains("Video", StringComparison.OrdinalIgnoreCase))
                        return s;
                }
            }

            return "";
        }

        private void PlantInt3(Site site)
        {
            var buf = new byte[1];
            if (!Dbg.ReadProcessMemory(_process, (IntPtr)site.Addr, buf, 1, out _))
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"read {site.Kind}");
            _saved[(int)site.Rva] = buf[0];
            buf[0] = 0xCC;
            if (!Dbg.WriteProcessMemory(_process, (IntPtr)site.Addr, buf, 1, out _))
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"int3 {site.Kind}");
            Dbg.FlushInstructionCache(_process, (IntPtr)site.Addr, 1);
        }

        private void Restore(Site site)
        {
            if (!_saved.TryGetValue((int)site.Rva, out var b))
                return;
            var buf = new[] { b };
            Dbg.WriteProcessMemory(_process, (IntPtr)site.Addr, buf, 1, out _);
            Dbg.FlushInstructionCache(_process, (IntPtr)site.Addr, 1);
        }

        private void Replant(Site site)
        {
            var buf = new byte[] { 0xCC };
            Dbg.WriteProcessMemory(_process, (IntPtr)site.Addr, buf, 1, out _);
            Dbg.FlushInstructionCache(_process, (IntPtr)site.Addr, 1);
        }

        private ulong Map(uint va) => _base + (va - _pe.ImageBase);

        private static Dbg.WOW64_CONTEXT ReadCtx(IntPtr thread)
        {
            var ctx = new Dbg.WOW64_CONTEXT { ContextFlags = Dbg.Wow64All };
            if (!Dbg.Wow64GetThreadContext(thread, ref ctx))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Wow64GetThreadContext");
            return ctx;
        }

        private static void WriteCtx(IntPtr thread, Dbg.WOW64_CONTEXT ctx)
        {
            ctx.ContextFlags = Dbg.Wow64All;
            if (!Dbg.Wow64SetThreadContext(thread, ref ctx))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Wow64SetThreadContext");
        }

        private static uint ReadU32(IntPtr process, ulong addr)
        {
            var buf = new byte[4];
            if (!Dbg.ReadProcessMemory(process, (IntPtr)addr, buf, 4, out _))
                return 0;
            return BitConverter.ToUInt32(buf, 0);
        }

        private static string ReadWString(IntPtr process, uint addr, int maxChars)
        {
            var buf = new byte[maxChars * 2];
            if (!Dbg.ReadProcessMemory(process, (IntPtr)addr, buf, (nuint)buf.Length, out _))
                return "";
            return Encoding.Unicode.GetString(buf).TrimEnd('\0');
        }
    }

    private sealed class Site
    {
        public Site(string kind, uint rva, uint va)
        {
            Kind = kind;
            Rva = rva;
            Va = va;
        }

        public string Kind { get; }
        public uint Rva { get; }
        public uint Va { get; }
        public ulong Addr { get; set; }
    }
}

internal static class Dbg
{
    public const uint CreateProcess = 3;
    public const uint CreateThread = 2;
    public const uint ExitProcess = 5;
    public const uint LoadDll = 6;
    public const uint Exception = 1;
    public const uint Rip = 9;
    public const uint Breakpoint = 0x80000003;
    public const uint SingleStep = 0x80000004;
    public const uint DbgContinue = 0x00010002;
    public const uint DbgExceptionNotHandled = 0x80010001;
    public const uint ThreadAllAccess = 0x1F03FF;
    public const uint Wow64All = 0x0001003F;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool DebugActiveProcess(int pid);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool DebugActiveProcessStop(uint pid);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool DebugSetProcessKillOnExit(bool kill);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WaitForDebugEvent(out DEBUG_EVENT ev, int ms);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ContinueDebugEvent(uint pid, uint tid, uint status);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr process, IntPtr addr, [Out] byte[] buf, nuint size, out nuint read);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr process, IntPtr addr, out IntPtr value, nuint size, out nuint read);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr process, IntPtr addr, byte[] buf, nuint size, out nuint written);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FlushInstructionCache(IntPtr process, IntPtr addr, nuint size);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenThread(uint access, bool inherit, uint tid);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Wow64GetThreadContext(IntPtr thread, ref WOW64_CONTEXT ctx);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Wow64SetThreadContext(IntPtr thread, ref WOW64_CONTEXT ctx);

    [DllImport("kernel32.dll")]
    public static extern uint GetThreadId(IntPtr thread);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hwnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string? cls, string? name);

    [DllImport("kernel32.dll")]
    public static extern uint GetProcessId(IntPtr process);

    public const uint SnapThread = 0x00000004;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint pid);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Thread32First(IntPtr snap, ref THREADENTRY32 te);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Thread32Next(IntPtr snap, ref THREADENTRY32 te);

    [StructLayout(LayoutKind.Sequential)]
    public struct THREADENTRY32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ThreadID;
        public uint th32OwnerProcessID;
        public int tpBasePri;
        public int tpDeltaPri;
        public uint dwFlags;
    }

    [DllImport("user32.dll")]
    public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);

    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_EVENT
    {
        public uint dwDebugEventCode;
        public uint dwProcessId;
        public uint dwThreadId;
        public DEBUG_UNION u;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DEBUG_UNION
    {
        [FieldOffset(0)] public EXCEPTION_DEBUG_INFO Exception;
        [FieldOffset(0)] public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo;
        [FieldOffset(0)] public CREATE_THREAD_DEBUG_INFO CreateThread;
        [FieldOffset(0)] public LOAD_DLL_DEBUG_INFO LoadDll;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EXCEPTION_DEBUG_INFO
    {
        public EXCEPTION_RECORD ExceptionRecord;
        public uint dwFirstChance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EXCEPTION_RECORD
    {
        public uint ExceptionCode;
        public uint ExceptionFlags;
        public IntPtr ExceptionRecord;
        public IntPtr ExceptionAddress;
        public uint NumberParameters;
        public IntPtr ExceptionInformation0;
        public IntPtr ExceptionInformation1;
        public IntPtr ExceptionInformation2;
        public IntPtr ExceptionInformation3;
        public IntPtr ExceptionInformation4;
        public IntPtr ExceptionInformation5;
        public IntPtr ExceptionInformation6;
        public IntPtr ExceptionInformation7;
        public IntPtr ExceptionInformation8;
        public IntPtr ExceptionInformation9;
        public IntPtr ExceptionInformation10;
        public IntPtr ExceptionInformation11;
        public IntPtr ExceptionInformation12;
        public IntPtr ExceptionInformation13;
        public IntPtr ExceptionInformation14;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CREATE_PROCESS_DEBUG_INFO
    {
        public IntPtr hFile;
        public IntPtr hProcess;
        public IntPtr hThread;
        public IntPtr lpBaseOfImage;
        public uint dwDebugInfoFileOffset;
        public uint nDebugInfoSize;
        public IntPtr lpThreadLocalBase;
        public IntPtr lpStartAddress;
        public IntPtr lpImageName;
        public ushort fUnicode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CREATE_THREAD_DEBUG_INFO
    {
        public IntPtr hThread;
        public IntPtr lpThreadLocalBase;
        public IntPtr lpStartAddress;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LOAD_DLL_DEBUG_INFO
    {
        public IntPtr hFile;
        public IntPtr lpBaseOfDll;
        public uint dwDebugInfoFileOffset;
        public uint nDebugInfoSize;
        public IntPtr lpImageName;
        public ushort fUnicode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WOW64_FLOATING_SAVE_AREA
    {
        public uint ControlWord;
        public uint StatusWord;
        public uint TagWord;
        public uint ErrorOffset;
        public uint ErrorSelector;
        public uint DataOffset;
        public uint DataSelector;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
        public byte[] RegisterArea;
        public uint Cr0NpxState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WOW64_CONTEXT
    {
        public uint ContextFlags;
        public uint Dr0;
        public uint Dr1;
        public uint Dr2;
        public uint Dr3;
        public uint Dr6;
        public uint Dr7;
        public WOW64_FLOATING_SAVE_AREA FloatSave;
        public uint SegGs;
        public uint SegFs;
        public uint SegEs;
        public uint SegDs;
        public uint Edi;
        public uint Esi;
        public uint Ebx;
        public uint Edx;
        public uint Ecx;
        public uint Eax;
        public uint Ebp;
        public uint Eip;
        public uint SegCs;
        public uint EFlags;
        public uint Esp;
        public uint SegSs;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] ExtendedRegisters;
    }
}
