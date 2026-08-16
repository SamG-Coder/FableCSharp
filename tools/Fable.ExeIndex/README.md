# Fable.ExeIndex

Turns a local `Fable.exe` into a searchable dump, then splits that dump into
section packets an agent can rewrite as C-like pseudocode.

```
dotnet run --project tools/Fable.ExeIndex -- all
```

Steps (`index` / `split` / `translate` / `all`):

| Dir | What |
|---|---|
| `out/00-index` | PE sections, imports, strings, RTTI, string xrefs, DXT FourCCs |
| `out/01-sections` | Per-topic packets (texture, sky, landscape, render, water, world, shaders) |
| `out/02-translate` | AI prompt wrapping each packet |
| `out/03-pseudo` | C-like pathway + pseudocode (write these from the packets; do not invent) |

`out/` is gitignored at the repo root **and** in this folder. Do not commit it.
Pass `--out <dir>` only if that directory is also ignored.

Do not invent formats or constants that the listing does not show.
