# `004FDBC0` vs host `LoadGlobalThingsFile`

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`. No-save
New Game is Leave `0042F2A2` → `FinalAlbion.wld` → Loading world
`004A1840` → `00507C30`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE**.

Question: host `LoadGlobalThingsFile` opens proximity TNG during
`00507C30`. Native `004FDBC0` first file is `LookoutPoint.tng` —
**parse only or construct?** Host mismatch?

Authority: `proofs/tng-first-after-leave`, `proofs/004FDBC0-open`;
`EngineLifecycle.LoadGlobalThingsFile`.
Siblings: `tng-spawn`, `tng-after-leave`, `first-0051FD80-file`,
`wld-map-index-0`, `lookout-tng-walk`, `lev-first-after-leave`,
`stb-first-open`.

Sources: `listing-004c0000.txt` (`004FDBC0` / `004FBF60` /
`004FAFF0`), `listing-00500000.txt` (`00507C30` / `00509859` /
`005223F0` / `00521AE0` / `0051FD80`),
`src/Fable.Game/EngineLifecycle.cs` (`LoadWorldMap` /
`LoadGlobalThingsFile` / `LoadRegionMapThings` /
`LoadSingleThing` / `EnsureLevels`),
`src/Fable.Game/LevelLibrary.cs` (`TryLoadThings`),
`src/Fable.Formats/Tng/ThingFile.cs`.

---

## Verdict

**Parse / open only. Not construct.** First file on both sides
is **`LookoutPoint.tng`**. Host is **not** a construct mismatch.

| Claim | Class |
|---|---|
| First `004FDBC0` file is `LookoutPoint.tng` (native map index 1) | **PROVEN** |
| That open happens **inside** `00507C30`, still Loading world | **PROVEN** |
| Host `LoadGlobalThingsFile` is that site (`004FDBC0` arm) | **PROVEN** |
| Host first `TryLoadThings` is `"LookoutPoint"` | **PROVEN** |
| Host **constructs** CThings here (`LoadSingleThing` / `InsertThing`) | **DISPROVEN** |
| Native this open **is** `0051FD80` / `00A371C0` | **DISPROVEN** host; live `[manager+128]` **UNREAD** |
| Lookout CThings are built later as ContainsMap[1] | **PROVEN** |
| First CThing after dummy is Bridge `TRACK_NODE_BASIC` | **PROVEN** (`first-0051FD80-file`) |
| First file / filter / timing mismatch | **MATCH** |
| Token-walk depth (`ThingFile.Parse` vs open-and-drop) | **PARTIAL** |
| `EnsureLevels` WAD + `_RT.stb` during this call | **DIVERGE** |

**Open ≠ construct.** Host concatenates parsed `ThingInstance`s
into `GlobalThings` and leaves `RegionThings` empty. Native
always opens the `.tng`; `00521AE0` / `0051FD80` run only if
`005223F0` sees `[thing_manager+128]==1`. First-seen
`00416392==0` after Init Game is **PARTIAL** evidence that
countable list stayed empty.

---

## Host site (`LoadWorldMap` → `LoadGlobalThingsFile`)

`LoadWorldMap` is `004A1840` after Startup WAD:

```
WorldFile.Load(FinalAlbion.wld)     // 00507C30 tokens
LoadGtngFile()                      // 0050959F stem+.gtng  TLC miss
LoadGlobalThingsFile()              // 00509859 → 004FDBC0
LoadRegionGraphFile()               // 00509982
SetStaticMapFileForUse()            // AFTER the TNG open
```

`SingleGlobalThingsFile` default is false (`[0x13B8609]=0`).
The `.gtg` arm `004FE2A0` is **not** no-save.

```csharp
public void LoadGlobalThingsFile()
{
    // Note 004FDBC0 (not 004FE2A0)
    EnsureLevels();
    var loaded = new List<ThingInstance>();
    foreach (var map in World.Maps)              // Maps[0] = NewMap 1
    {
        if (!map.LoadedOnPlayerProximity)
            continue;
        var tng = _levels?.TryLoadThings(map.ScriptName);
        if (tng is null)
            continue;
        loaded.AddRange(tng.Things);
        GlobalThingMapsLoaded++;
    }
    GlobalThings = new ThingFile {
        Version = 2,
        Sections = [new ThingSection { Name = "GLOBAL", Things = loaded }]
    };
}
```

No `LoadSingleThing`. No `InsertThing`. No `_regionThings`.
`TryLoadThings` is `ThingFile.Parse` (loose then WAD). TLC has
no loose `LookoutPoint.tng`; bytes are `FinalAlbion.wad`.

First increment: `Maps[0].ScriptName=="LookoutPoint"`,
`.Index==1`. Dummy native slot 0 is **absent** from the C#
list (`wld-map-index-0`). Do not start at `Maps[1]`.

Census (host parse, TLC): **151** prox maps / **21746** things.
Lookout file: **288**, first `NewThing` `MARKER_BASIC`
`M_Maze`.

---

## Native first file (`004FDBC0` during `00507C30`)

`00509859` `"Load global things"` → `[0x13B8609]==0` →
`call 004FDBC0` (`ecx=CWorldMap`, **no** stack args).

```
004FDBC5  esi = ecx
004FDBC7  begin = [esi+32], end = [esi+36]
          count = (end-begin)/72
004FDBDE  ebx = 1                       // skip dummy 0
004FDBF2  edi = 0x48
          if [slot+36] && [slot+40]:    // filled + prox
004FDC71    push ebx
            call 004FBF60               // one map file
```

First hit: `ebx=1`, WLD `NewMap 1` `LevelScriptName
"LookoutPoint"`. `004FAFF0` appends `0x12442C4` `".tng"`.
Stream: first-seen `[map+168]==0` → `00BFEA1A(28)` +
`0099AD80` / `CreateFileW`. Then `005223F0`.

```
005223F7  eax = [esi+128]
005223FF  cmp eax, 1
00522407  jne 00522502                  // drop shared_ptr
0052249F  call 00521AE0                 // token walk
005224AB  call 0051E2F0
```

| Layer | Always? |
|---|---|
| Path `LookoutPoint.tng` | **yes** |
| Open `0099AD80` / `00A39D80` | **yes** |
| `00521AE0` `NewThing` walk | **only if** `[+128]==1` |
| `0051FD80` / `00A371C0` | same gate |
| Else | open + drop; no CThing |

`004FDBC0` does **not** call Allocate Class. Construct is
later `006C2170` ContainsMap: Bridge → Lookout → Guild
(`proofs/first-0051FD80-file`). Lookout is **re-opened** as
ContainsMap[1].

---

## Parse only or construct?

Three senses. Do not collapse them.

| Sense | This `004FDBC0` / host call | Later `006C2170` |
|---|---|---|
| **File I/O** | `LookoutPoint.tng` first | Bridge first, then Lookout again |
| **Token parse** | host **yes** (`ThingFile.Parse`); native **iff** `+128==1` | **yes** (`00521AE0`) |
| **CThing construct** | host **no**; native gated **UNREAD** | **yes** (`0051FD80`) |

Answer for “does `004FDBC0` construct Lookout?”:

- **Host: no.** Parse + store only. **PROVEN.**
- **Native live New Game: unread gate, parse-only is the
  working model.** `first-0051FD80-file` classifies “Lookout
  is constructed in `00507C30`” as **DISPROVEN**. First
  constructed file after dummy pumps is
  `BowerstoneBridge.tng` `TRACK_NODE_BASIC` `GuardTrack`.
- If the unread `+128==1` branch were live, first `0051FD80`
  would be Lookout `M_Maze` **during** `00416953`, before
  dummy pumps. First-seen `00416392==0` argues it is not.

`GuildArrivalHSP` is parsed as Lookout holy-site #1 (file
order ~35). It is **not** constructed here on the host.
Hero `006AC910` is later still.

---

## Host mismatch?

### MATCH (this question)

| Site | Host | Native |
|---|---|---|
| When | inside `00507C30`, before Set Static Map | same |
| Switch | `SingleGlobalThingsFile==false` → per-map | `[0x13B8609]==0` → `004FDBC0` |
| `.gtng` | miss note, skip | `0050959F` miss |
| First name | `TryLoadThings("LookoutPoint")` | `004FBF60(1)` → `.tng` |
| Filter | `LoadedOnPlayerProximity` (TLC writes token) | `[slot+36] && [slot+40]` |
| Walk order | `Maps[0]…` = NewMap 1… | `ebx=1…` skip dummy 0 |
| Count | 151 / ~21746 | same census |
| Construct | none | gated; host skip **MATCH** vs off |
| First `0051FD80` | Bridge after `00501450` | same if gate off |
| `GlobalThings` → `RegionThings` | no leak | first Present is ContainsMap + hero |

### DIVERGE (side effects of the same call, not construct)

| Site | Host | Native | Class |
|---|---|---|---|
| `EnsureLevels` | `LevelLibrary` ctor opens WAD + `FinalAlbion_RT.stb` | `004FDBC0` opens `.tng` only; STB later / different name | **DIVERGE** (`lev-first-after-leave`, `stb-first-open`) |
| Token walk if `+128!=1` | always `ThingFile.Parse` 288 then 150 more | open + drop; no `00521AE0` | **PARTIAL** |
| Stream | WAD `Read` (no loose TLC file) | `CreateFileW` on resolved path | **PARTIAL** (same bytes, different FS) |
| Store | one `ThingFile` section `GLOBAL` | no host-like concat; *use* **UNREAD** | **PARTIAL** |
| Missing `.tng` | skip, no increment | still `004FBF60` | TLC both 151 |

`EnsureLevels` is the real host extra during this VA, not a
false construct. Do not treat WAD+STB as native `004FDBC0`.

### Not a mismatch

- Starting at Picnic / Bridge / Oakvale for this **open**.
- Calling `LoadGlobalThingsFile` `0051FD80`.
- Inserting the 21k `GlobalThings` into the first scene.

---

## Path (no-save)

```
00416953  Loading world
  004A1840
    00507C30
      NewMap 1  LookoutPoint
      0050959F  .gtng miss
      00509859  Load global things
        004FDBC0                         ← THIS
          LookoutPoint.tng               first open
            host: ThingFile.Parse 288, concat GLOBAL
            native: open; 0051FD80 iff +128==1   UNREAD
          … 150 more prox maps …
      00509982  region graph
    Set Static Map                       AFTER
004189C2  dummy pumps  0 things
00501450 → 00500540(1,0,0) Lookout region
  006C2170  ContainsMap
    BowerstoneBridge.tng                 first 0051FD80
    LookoutPoint.tng                     reopen, construct
    GuildExterior.tng
```

---

## UNREAD / PARTIAL

- Live `[thing_manager+128]` on the first `005223F0`.
- Native *use* of the 21k set (UID table, PersonalScript,
  later cache). **DISPROVEN** as first-Present C3Ds.
- Whether `CreateFileW` hits bytes identical to WAD
  `Find("LookoutPoint.tng")`.
- Any TLC prox miss that would desync the 151 count.

---

## Do not

- Report `004FDBC0` / `LoadGlobalThingsFile` as construct.
- Collapse first **open** (Lookout) with first **CThing**
  (Bridge).
- “Fix” host by calling `LoadSingleThing` here to match an
  unread `+128==1` branch.
- Start this walk at `Maps[1]` to skip a dummy the C# list
  does not have.
- Bind Oakvale / `00DBDE40` / kid `CREATURE_HERO_CHILD`.
- Treat `EnsureLevels` WAD+STB as native TNG construct.
