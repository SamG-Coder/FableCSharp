# CCutsceneDef / script.bin format

Statuses: **PROVEN**, **PARTIAL**, **UNREAD**.

Executable persist is `00F2A1D0`. The host parser is
`ScriptBank.TryReadCutsceneVectors`. Printable ASCII
scrape (`ExtractCommands`) is **DISPROVEN** as the
command list `00CBFB7D` walks.

| Field | Value | Evidence | Status |
|---|---|---|---|
| Type | `CCutsceneDef` | GameBin `TypeName`; ctor `00F29D00` vtbl `0x012FB6E0` | PROVEN |
| Runtime size | `0x9C` | ctor getter `00F29D90` | PROVEN |
| Command-list pointer | `this+60` | persist first `004331F9`; runner `add ebx, 60` then `00432EE9` | PROVEN |
| Skip / vector 1 | `this+72` | persist second vector; `00CC017C` copy when skip | PROVEN |
| Vector 2 | `this+108` | persist third; `[ebp+120]==1` uses this instead of +60 | PROVEN |
| Vectors 3–7 | `+84 +96 +120 +132 +0x90` | persist remaining `004331F9` | PROVEN |
| Vector count | 8 | persist has eight reads | PROVEN |
| Vector encoding | skip u32 + count + NUL CStrings | `004331F9` / `00433273` | PROVEN |
| String ownership | persist-owned CString vector | not a borrowed ASCII scrape | PROVEN |
| Argument representation | command line text; comma args | `00CBFB7D` token match + parse helpers | PROVEN |
| Delimiters | space after verb/actor; comma args | `ScriptCommand.Parse` | PROVEN |
| Terminators | NUL per string; count bounds the list | `00433273` | PROVEN |
| Condition encoding | `IsTrue` / `IsFalse` / `null` strcmp | `00CBEDBA` / `00CBEE0C` / `00CBEE5E` | PROVEN |
| Nested blocks | none on first-seen list | no block opcodes recovered | UNREAD |
| Labels / jumps | none on first-seen list | increment `[ebp-72]` only | UNREAD as first-seen |
| Command ordering | persist order = run order | `00CD17FD` inc then next | PROVEN |
| Instance-name lookup | GameBin `InstanceName` | `ScriptBank.Find` | PROVEN |
| Factory / name table | `00CB8230` + `00DABAC0` factories | `ScriptFactoryTable` | PROVEN |
| script.bin type | GameBin entries; cutscenes are `CCutsceneDef` | `DataCatalogTests` | PROVEN |

Runner `00CBFB7D` copies `+60` (or `+108` when `[ebp+120]==1`)
into a working CString vector and walks it. First-seen
`00DB86B0` pushes 0 so `+60` is used. Skip vector 1 does
not auto-run (`FirstSeenCutsceneVector1AutoRuns=false`).
