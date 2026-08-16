# Fable.ExeIndex

Turns a local `Fable.exe` into a searchable dump, then splits that dump into
section packets an agent can rewrite as C-like pseudocode.

The x86 decoder must consume a full instruction. Unknown `0F` / `F6` / x87 used
to emit `db` and then a fake `ret`. It now covers 0F (jcc/movzx/setcc/imul),
F6/F7, C0/C1/D0–D3 shifts, D8–DF x87, A0–A3 moffs, and 66/F2/F3 prefixes.

```
dotnet run --project tools/Fable.ExeIndex -- all
```

```
dotnet run --project tools/Fable.ExeIndex -- disasm 0x00B25950 80
dotnet run --project tools/Fable.ExeIndex -- calls 0x00B25950
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x012A2B54 16
dotnet run --project tools/Fable.ExeIndex -- imm 0x013962A0
dotnet run --project tools/Fable.ExeIndex -- disp 348 0x00B20000 0x00B80000
dotnet run --project tools/Fable.ExeIndex -- scanff 16
```

Steps (`index` / `split` / `translate` / `all` / `disasm` / `calls` / `trace-render` / `imm` / `vtbl` / `disp` / `scanff`):

| Dir | What |
|---|---|
| `out/00-index` | PE sections, imports, strings, RTTI, string xrefs, DXT FourCCs |
| `out/01-sections` | Per-topic packets (texture, sky, landscape, render, water, world, shaders) |
| `out/02-translate` | AI prompt wrapping each packet |
| `out/03-pseudo` | C-like pathway + pseudocode (write these from the packets; do not invent) |

`out/` is gitignored at the repo root **and** in this folder. Do not commit it.
Pass `--out <dir>` only if that directory is also ignored.

Do not invent formats or constants that the listing does not show.
