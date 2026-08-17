using System.Diagnostics;
using System.Text;

namespace Fable.Core;

/// <summary>
/// Observation-only Receive vs WaitEx vs Present
/// log. Does not pace playback.
/// </summary>
public static class PlayAviWave
{
    private static readonly object Gate = new();
    private static readonly List<string> Lines = [];
    private static readonly long Start = Stopwatch.GetTimestamp();
    private static int _recv;
    private static int _set;
    private static int _waitSignaled;
    private static int _waitTimeout;
    private static int _present;
    private static int _recvSincePresent;
    private static int _maxRecvBetweenPresent;
    private static int _collapsedSets;
    private static int _lastSetSerial;
    private static int _setsSinceWait;

    public static void Reset()
    {
        lock (Gate)
        {
            Lines.Clear();
            _recv = 0;
            _set = 0;
            _waitSignaled = 0;
            _waitTimeout = 0;
            _present = 0;
            _recvSincePresent = 0;
            _maxRecvBetweenPresent = 0;
            _collapsedSets = 0;
            _lastSetSerial = 0;
            _setsSinceWait = 0;
        }
    }

    public static void Site(string name, string phase, long extra = 0)
    {
        Add($"{name} {phase} t={Ms():F1} tid={Environment.CurrentManagedThreadId} extra={extra}");
    }

    public static void Recv(int n, long startHns, int serial, string phase)
    {
        if (phase == "enter")
        {
            Interlocked.Increment(ref _recv);
            Interlocked.Increment(ref _recvSincePresent);
        }

        Add($"recv={n} {phase} t={Ms():F1} pts={startHns} serial={serial} sincePresent={Volatile.Read(ref _recvSincePresent)}");
    }

    public static void Set(int serial)
    {
        lock (Gate)
        {
            _set++;
            _setsSinceWait++;
            if (_setsSinceWait > 1)
                _collapsedSets++;
            _lastSetSerial = serial;
            Lines.Add($"set serial {serial} t={Ms():F1} setsSinceWait={_setsSinceWait}");
        }
    }

    public static void Wait(bool signaled, int serialBefore, int serialAfter, int recvDuringWait)
    {
        lock (Gate)
        {
            var recvSince = _recvSincePresent;
            if (recvSince > _maxRecvBetweenPresent)
                _maxRecvBetweenPresent = recvSince;
            if (signaled)
                _waitSignaled++;
            else
                _waitTimeout++;
            _setsSinceWait = 0;
            var tag = signaled ? "signaled" : "timeout";
            Lines.Add($"wait {tag} serial {serialBefore}->{serialAfter} recvSince={recvSince} recvDuringWait={recvDuringWait} t={Ms():F1}");
        }
    }

    public static void Present(int serial)
    {
        lock (Gate)
        {
            _present++;
            _recvSincePresent = 0;
            Lines.Add($"present {serial} t={Ms():F1}");
        }
    }

    public static string Report()
    {
        lock (Gate)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"recv={_recv} set={_set} waitSignaled={_waitSignaled} waitTimeout={_waitTimeout} present={_present}");
            sb.AppendLine($"maxRecvBetweenPresent={_maxRecvBetweenPresent} collapsedSets={_collapsedSets} lastSet={_lastSetSerial}");
            var extraSets = _set - _waitSignaled;
            sb.AppendLine($"setMinusSignaled={extraSets} (AutoReset coalesces extra Sets)");
            sb.AppendLine();
            var n = Math.Min(Lines.Count, 80);
            for (var i = 0; i < n; i++)
                sb.AppendLine(Lines[i]);
            if (Lines.Count > 80)
            {
                sb.AppendLine("...");
                for (var i = Math.Max(n, Lines.Count - 40); i < Lines.Count; i++)
                    sb.AppendLine(Lines[i]);
            }

            return sb.ToString();
        }
    }

    private static void Add(string line)
    {
        lock (Gate)
        {
            if (Lines.Count < 20_000)
                Lines.Add(line);
        }
    }

    private static double Ms() =>
        (Stopwatch.GetTimestamp() - Start) * 1000.0 / Stopwatch.Frequency;
}
