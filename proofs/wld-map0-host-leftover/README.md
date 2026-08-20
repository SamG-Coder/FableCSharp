# Dummy map index 0 vs host on Init World Map

Investigation only. No production `src/` or `tests/` edits.

Do **not** treat C# `Maps[0]` as native index 0.
Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
First authored map is LookoutPoint (`NewMap 1`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **DIVERGE** / **MATCH** / **LEFTOVER**.

Question: WLD map index **0** dummy vs host. **MATCH** or
**LEFTOVER**? First leftover on Init World Map?

Authority: `proofs/wld-map-index-0`, `proofs/wld-map0-dummy`,
`proofs/init-world-004A6E30`, `proofs/wld-parse`;
dump `listing-00480000.txt` (`004A6E30` / `004A6EC0`);
`listing-00500000.txt` (`005066E0` / `00515AD0` /
`00506738` / `0050674B`);
`listing-004c0000.txt` (`004FDDE0` / `004FDBC0`);
host notes only: `EngineLifecycle.InitWorldInitStages` /
`EnterGame` `"Init World"` arm / `LoadWorldMap`.
Siblings: `wld-00507C30-switch`, `004FDBC0-vs-host`,
`00419D90-hoist`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Dummy map slot 0 vs host `Maps[0]`? | **LEFTOVER** plant. Host list has **no** index-0 dummy. `Maps[0]` is Lookout (native slot **1**). | **PROVEN** leftover / **DIVERGE** slot |
| First leftover on Init World Map? | **Yes.** Named site `005066E0` is `InitWorldInitStages[0]`. Host **Note-only**. First table write in the ctor is `00515AD0(1)` → 72-byte **map** dummy 0. | **PROVEN** leftover |
| Host name / apply VA? | `"Init World Map"` `0x005066E0` | **MATCH** |
| Host runs the `0xD8` ctor / dummy plant? | **No.** `Note(worldApply, worldName)` only. Next real work in the table is Mesh Bank `OpenMeshBank()`. | **DISPROVEN** body |
| First authored map / first `004FDBC0` name? | Lookout on both sides | **MATCH** |
| Insert dummy `Maps[0]` to close this leftover? | **No.** Leftover **theater**. Breaks `Maps[0]==Lookout`. | **DISPROVEN** as needed |
| Oakvale / `00DBDE40` as map 0? | **No.** | **DISPROVEN** |

---

## Verdict

**LEFTOVER on Init World Map. MATCH first authored map.**

`InitWorldInitStages[0]` is `"Init World Map"` `005066E0`.
That is the **first** leftover in the world-init table:
host notes the name; native allocates `0xD8`, stores
`[world+20]`, and plants dummy **map** slot 0.

Dummy 0 is **not** Lookout and **not** Oakvale. Host
`WorldFile.Parse` never inserts it. `Maps[0].Index==1`
`ScriptName=="LookoutPoint"` **MATCH**es native slot 1
and first `004FDBC0` `ebx=1`. Comparing `Maps[0]` to
native index 0 is the leftover.

Region dummy (88-byte `[+44]`) is the **second** table
write in the same ctor. Host already treats
`CurrentRegionIndex==0` as that dummy (`RegionTableDummyCount=1`).
Do not fold the two dummies.

---

## Path (this leftover only)

```
0041735A  Init World
  004A67D0  CWorld
  [world].vtbl+36  004A6E30
    "Init World Map"                    ← InitWorldInitStages[0]
      alloc 0xD8
      004A6EC0  005066E0                ← THIS SITE
        0099A2F0 / vtbl 01244AEC
        [+20] 24-byte list
        push 1; [esi+32] 00515AD0       ← FIRST leftover table
          1×72  004FDDE0  [+36]=0       ← dummy MAP 0
        push 88; [esi+44] 006BC410      ← SECOND leftover table
          dummy REGION 0
        [esi+156]=0
    "Init Environment" …
00416953  Loading world
  00507C30  NewMap 1 Lookout → slot 1
  004FDBC0  ebx=1  skip dummy 0
```

Host:

```
EnterGame
  "Init World"
    Note(004A6E30)
    foreach InitWorldInitStages
      Note(005066E0, "Init World Map")  ← leftover Note-only
      … later OpenMeshBank() only
    InitWorldCameras()
  LoadWorld → LoadWorldMap
    WorldFile.Load                     ← no dummy row
    LoadGlobalThingsFile foreach Maps  ← Maps[0] Lookout
```

---

## 1. Init World Map is the first leftover in the table

`EngineLifecycle.InitWorldInitStages`:

```
("Init World Map",            0x005066E0),   // first
("Init Environment",          0x006BBC30),
("Init Navigation Manager",   0x00A15670),
…
("Init Mesh Bank",            OpenFn),       // first real work
("Init UI Manager",           0x0041D198),
```

`EnterGame` `"Init World"` arm (`EngineLifecycle` ~3870):

```
Note(InitWorldInitFn, "Init World Init", … "004A6E30");
foreach (var (worldName, worldApply) in InitWorldInitStages)
{
    Note(worldApply, worldName, "World", worldName);
    if (worldApply == InitMeshBankFn)
        OpenMeshBank();
}
```

| Host | Native `004A6E30` child 1 | Class |
|---|---|---|
| Stage name `"Init World Map"` | `004A6E3C` / `004A6E64` | **MATCH** |
| Apply `0x005066E0` | `004A6EC0 call 005066E0` | **MATCH** |
| Size `WorldMapObjectSize=0xD8` | `004A6E8C push 0xD8` | **MATCH** (const only) |
| Shift 5 / bound `0x2000` | `004A6EAD push 5` / `ecx=0x2000` | **MATCH** (const only) |
| Alloc + ctor + `[world+20]` | `00BFEA1A` then store | **LEFTOVER** omit |
| Dummy map 0 | `00515AD0(1)` | **LEFTOVER** omit |
| Dummy region 0 / `+156=0` | `006BC410` / `[esi+156]=ebx` | **PARTIAL** later (`CurrentRegionIndex`) |

Name is present. Body is omitted. Same leftover class as
later Note-only world stages. This row is **first** because
it is `InitWorldInitStages[0]`.

`init-world-004A6E30` already classed the host table as a
**PARTIAL** name subset. This file only ranks the first hole
on the **named** Init World Map site.

---

## 2. First leftover *inside* `005066E0` is dummy map 0

`listing-00500000` `005066E0` (`ecx` = new `0xD8`):

```
005066E8  call 0099A2F0
005066FC  [esi]    = 0x1244AEC          // WorldMapVtbl
00506702  [esi+4]  = 0x1244AB4
00506711  [esi+16] = 0
00506714  push 24                       // circular list [+20]
…
00506738  push 1
0050673A  lea ecx, [esi+32]             // map vector
00506740  call 00515AD0                 // FIRST table
00506745  [esi+44/48/52] = 0
0050674B  push 88
00506750  call 00BFEA0E
00506768  call 006BC410                 // SECOND table (region)
0050682F  [esi+156] = 0                 // current = dummy
00506847  [esi+172] = 1                 // MapUIDCount default
```

`00515AD0(1)`: `lea eax,[edi+edi*8]; shl 3` → 72 bytes;
`004FDDE0` default into slot 0; `[begin,end,cap]=[p,p+72,p+72]`.

`004FDDE0` dummy map record:

```
[+24] empty intern (0x122D70E)
[+36..39] = 0                         // not filled
[+40] = 1                             // prox default
[+64] = 0                             // IsSea
```

`004FDBC0` starts `ebx=1` and tests `[slot+36]`. Dummy 0 is
never opened. File never emits `NewMap 0`; `eax==0` is
rejected (`005083AF`). **PROVEN** (`wld-map-index-0`).

Sibling `wld-00507C30-switch` (“dummy slot 0 is the 88-byte
region row, **not** this 72-byte map table”) is **DISPROVEN**
as a map-table claim. Both vectors have an index-0 dummy.
Map dummy is **first** in the ctor.

Host comments `RegionTableDummyCount` / `ActivateCurrentRegion`
describe the **region** dummy. That is not this leftover.

---

## 3. Host `Maps[0]` is not dummy 0

```csharp
// WorldFile.Parse
if (StartsWithToken(line, "NewMap", out var newMapRest))
    current = new WorldMapBuilder { Index = ParseInt(newMapRest) };
if (EndMap)
    maps.Add(current.Build());   // append; no dummy
```

| Native `[+32]` stride 72 | C# `World.Maps` | Class |
|---|---|---|
| size **399** (0..398) | count **398** | **DIVERGE** |
| slot 0 dummy (`[+36]=0`) | **absent** | **LEFTOVER** omit |
| slot 1 LookoutPoint | `Maps[0]`, `.Index==1` | **MATCH** |
| first `004FDBC0` | `ebx=1` Lookout `.tng` | **MATCH** |

`TlcInstallTests.World_starts_at_lookout_point`:
`Maps[0].Index==1`, `ScriptName=="LookoutPoint"`.

`FindRegionContaining` comment (“New-game Oakvale is
`StartOakVale`, not `Maps[0]`”) is **LEFTOVER** vs no-save:
`Maps[0]` **is** Lookout; Oakvale is `NewRegion 4` /
`NewMap 203`.

---

## 4. Close this leftover? No dummy `Maps[0]`

| Move | First `004FDBC0` name | Native slot 0 | Class |
|---|---|---|---|
| Keep `Maps[0]` = Lookout | **MATCH** | omit | **MATCH** name |
| Insert dummy `Maps[0]` | Picnic / skip Lookout | **MATCH** layout | leftover theater |
| Start load at `Maps[1]` | Picnic | still omit 0 | **DIVERGE** |
| Bind Oakvale as map 0 | Oakvale | **DISPROVEN** | leftover |

`wld-map0-dummy`: host does **not** need a dummy row to
match the first open name. Implementing `00515AD0(1)` as
`Maps[0]` would track native 0 and **break** every
`Maps[0]==Lookout` test.

The leftover to *implement* later is the `0xD8` object
(`[world+20]`, vtbl, `+156`), not a C# dummy map.

---

## Classification table

| Claim | Class |
|---|---|
| Init World Map is `InitWorldInitStages[0]` `005066E0` | **PROVEN** |
| Host Note-only that apply | **PROVEN** **LEFTOVER** |
| First ctor table write is dummy **map** slot 0 | **PROVEN** |
| Dummy 0 is Lookout / Oakvale | **DISPROVEN** |
| Dummy 0 is *only* the 88-byte region row | **DISPROVEN** |
| Host `Maps[0]` == native slot 0 | **DISPROVEN** / **DIVERGE** |
| Host `Maps[0]` == native slot 1 Lookout | **MATCH** |
| First leftover on this named site is dummy map 0 | **PROVEN** leftover |
| Insert dummy `Maps[0]` | leftover theater |
| Live `005223F0` `[+128]` on first `004FBF60(1)` | **UNREAD** (`wld-map-index-0`) |

**Answer:** dummy map index 0 vs host is **LEFTOVER**
(omitted plant / Note-only ctor), not a **MATCH** of
slot 0. It **is** the first leftover on Init World Map.
First authored map stays **MATCH** at `Maps[0]` /
native slot 1. Do not start Oakvale.

---

## Host notes (no edit)

- Keep `WorldMap.Index` as the WLD integer.
- Do not insert dummy `Maps[0]`.
- Do not start `LoadGlobalThingsFile` at `Maps[1]`.
- Do not treat `RegionTableDummyCount` as a map dummy.
- Next named leftover after this site is `"Init Environment"`
  `006BBC30` (still Note-only).
