# Fable.ExeIndex

Turns a local `Fable.exe` into a searchable dump, then splits that dump into
section packets an agent can rewrite as C-like pseudocode.

The x86 decoder must consume a full instruction. Unknown `0F` / `F6` / x87 used
to emit `db` and then a fake `ret`. It now covers 0F (jcc/movzx/setcc/imul),
F6/F7, ADC/SBB `10–1D`, C0/C1/D0–D3 shifts, D8–DF x87, A0–A3 moffs, and 66/F2/F3
prefixes. `00DBDE40` (`sbb bl, bl`) needs ADC/SBB or the new-game setup looks like
a 12-instruction stub. `map-newgame` walks real prologues in New Game code
ranges (`WalkFunction` stops at INT3 / next `push ebp`) and does not BFS into
CRT or later-town callees.

```
dotnet run --project tools/Fable.ExeIndex -- all
dotnet run --project tools/Fable.ExeIndex -- trace-newgame
dotnet run --project tools/Fable.ExeIndex -- --force trace-landscape
dotnet run --project tools/Fable.ExeIndex -- map-newgame
```

```
dotnet run --project tools/Fable.ExeIndex -- disasm 0x00B25950 80
dotnet run --project tools/Fable.ExeIndex -- fn 0x00BD71B0
dotnet run --project tools/Fable.ExeIndex -- calls 0x00B25950
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x012A2B54 16
dotnet run --project tools/Fable.ExeIndex -- imm 0x013962A0
dotnet run --project tools/Fable.ExeIndex -- disp 348 0x00B20000 0x00B80000
dotnet run --project tools/Fable.ExeIndex -- scanff 16
dotnet run --project tools/Fable.ExeIndex -- floats 0x0139C5D8 16
dotnet run --project tools/Fable.ExeIndex -- calldisp 0xE4
dotnet run --project tools/Fable.ExeIndex -- calldisp 0x1C
dotnet run --project tools/Fable.ExeIndex -- trace-landscape
```

Each dump family lives in `out/01-sections/<family>/` as one markdown file per VA, plus `INDEX.md` that links them. A stub `01-sections/<family>.md` points at that index. `out/manifest.json` stores the exe identity (`TimeDateStamp-SizeOfImage-fileLength`) and a **recipe version** per family. Re-running the same command skips a family unless the exe changed, the version constant in `DumpStore.cs` was bumped, or you pass `--force`.

`calldisp` finds both `FF 5x disp8` and `FF 9x disp32` vtbl calls. `fn` walks one function past early `ret` and stops at INT3 or the next `push ebp`. Steps (`index` / `split` / `translate` / `all` / `disasm` / `fn` / `calls` / `trace-render` / `trace-landscape` / `trace-newgame` / `map-newgame` / `imm` / `vtbl` / `disp` / `scanff` / `floats` / `calldisp`):

| Dir | What |
|---|---|
| `out/manifest.json` | Exe id + per-family dump versions |
| `out/00-index` | PE sections, imports, strings, RTTI, string xrefs, DXT FourCCs |
| `out/01-sections/<family>/` | One part file per VA + `INDEX.md` |
| `out/02-translate` | Prompt wrapping each family's INDEX |
| `out/03-pseudo` | C-like pathway + pseudocode (write these from the packets; do not invent) |

`out/` is gitignored at the repo root **and** in this folder. Do not commit it.
Pass `--out <dir>` only if that directory is also ignored.

Do not invent formats or constants that the listing does not show.
