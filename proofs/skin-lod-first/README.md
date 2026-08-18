# First skin / PALSKIN LOD after Leave Frontend

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / kid **4300** / father /
`MESH_HERO_HAIR_YOUNG_01` **4275–4277** / `CAM_OVIF_SHOT2`.
Those are leftover `Q_NewOakValeIntro` or unread attachments,
not Leave / Init Game / first no-save 3D Present.

Do **not** invent a second C3D LOD mesh for Graphic **4299**.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `docs/PARITY.md` §19; `docs/render/FIRST_SCENE_WORLD_PARITY.md`
VISIBILITY; `docs/status/investigations/E-player-palskin.md`;
`2026-08-18-palskin.md`; `WorldShading.cs` (`MeshLodInfoReady*`);
`FirstSceneWorld` visibility row `"lod"`;
`MeshFormatTests` / `WorldGeometryTests` / `WorldPipelineTests`;
`proofs/palskin-open/`; `proofs/c3d-first-submit/`;
`proofs/hero-inventory-first/`;
ExeIndex `c3dmeshlodinfo-ready-00a23de0-00a23de0.md`,
`vtbl-c3dmeshlodinfo-vtbl-0129cdb4-0129cdb4.md`,
`resource-ready-009d54e0-009d54e0.md`,
`calls-resource-ready-009d54e0-009d54e0.md`,
listings `00A23DE0` / `00A23C20` / `00A24430` / `00A24520` /
`00A25470` / `00A254A0` / `00A26D40` / `00A243B0` /
`0057A43E` / `0057FB86`.

Siblings: `proofs/palskin-open` (first type-5 **open**),
`proofs/c3d-first-submit` (static DIP),
`proofs/landscape-first-draw` (`009D54E0` on tiles).

---

## Verdict

**First skin / PALSKIN LOD after Leave is a ready-or-not gate
on the stored C3D. It does not swap the mesh id.**

First no-save PALSKIN is Lookout adult **4299** `MESH_HERO`.
That id stays 4299. Frontend never runs this gate.

| Layer | First native site | After Leave? | Class |
|---|---|---|---|
| `C3DMeshLODInfo` ready `00A23DE0` | vtbl `0x0129CDB4` slot 0; **zero `E8`** | only after a C3D exists (Lookout) | **PROVEN** body. First virtual caller **UNREAD** |
| Null `+36` → `al=1` | first-seen | **PROVEN** (`FirstSeenLodInfoNullReturnsReady`) |
| Non-null `+36` → `009D54E0` | later / landscape | **PROVEN** as resource-ready, **DISPROVEN** as mesh swap |
| Load-if-set `00A25470` → `00A24520` | only when `+36≠0` | **DISPROVEN** as first-seen |
| Type-5 branch inside `00A24520` | `and eax,15; cmp 5` | **PROVEN** can load PALSKIN later. **DISPROVEN** as Leave first |
| LOD predicate `00A23C20` | `cmp arg,1; setg` (no `this`) | **PROVEN** body. First-seen use **DISPROVEN** |
| Appearance-modifier siblings 4275/4276/4277 | def **11656** on disk | **PROVEN** ids. **DISPROVEN** as first Present |
| Landscape `009D54E0` `00BF3BEC` / `00BDC6D9` | tile / patch | **PROVEN** first *E8* ready checks. **DISPROVEN** as skin LOD |

**Answer:** after Leave, do not pick a LOD mesh. Draw 4299 when
the stored C3D is ready. Host `MeshLodInfoReady_00A23DE0(0)==1`
matches first-seen.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  0042DF9E  2D UI                         // no C3D, no LODInfo
  Init Engine 0042E204
    00B3B6D0 3, "SHADERS_PALSKIN"         // name only (palskin-open)
0042F2A2 Leave frontend
  009BE420 + 009BEEB0 Present
0042F491 Init Game → 004184BD
  Init World 004A6E30
    0049E620 MBANK_ALLMESHES              // directory; ParsedCount=0
00416953 FinalAlbion.wld
004189C2 first pumps                      // no region; no C3D
later 00501450 LookoutPoint
  006C2170 Loading objects
    Graphic HANDLE 009AD410               // no parse
  0051FD80 / 006AC910 CREATURE_HERO
    Graphic 4299 MESH_HERO
then first 00A243B0(id=4299) miss
  00A26D40 type 5 → 96-byte record vtbl 0129CD3C
    FIRST PALSKIN payload                 // palskin-open
C3DMeshLODInfo (if attached) +36 == 0
  00A23DE0 → mov al,1                     // FIRST skin LOD gate
  00A25470 no-ops (eax=[this+36]==0)
  no 00A24520 type-5 load
  no mesh-id rewrite
then 00BD71B0 / 00BD3070 draw 4299
  19 prims; submit set [4299] only
```

`CREATURE_HERO_CHILD` / **4300** / father / **4275–4277** /
`005B37F7` DEFAULT / `HeroWear` are **not** on this list.
**PROVEN**.

---

## 1. Frontend / Leave — no skin LOD

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D (`0042DF9E` / type `0x22`) | **PROVEN** | `proofs/camera-after-leave`; `palskin-open` |
| Frontend opens `MBANK_ALLMESHES` / `00A26D40` | **DISPROVEN** | first directory is Init World after Leave |
| Frontend constructs `C3DMeshLODInfo` | **DISPROVEN** | no mesh bank; no type-5 record |
| `00A23DE0` / `00A25470` / `00A24520` on retail pump | **DISPROVEN** | zero C3D this |
| Leave teardown is a LOD pick | **DISPROVEN** | `0042F2A2` is fade / clear / `FinalAlbion.wld` |

**Answer:** no skin LOD during frontend. Do not
`Meshes.Get(4299|4300|4275)` or invent a LOD id on
`PumpFrontendFrame`.

---

## 2. What `C3DMeshLODInfo` is (and is not)

RTTI `0x01397304` `.?AVC3DMeshLODInfo@@`. Vtbl **`0x0129CDB4`**:

| Slot | VA | Body |
|---|---|---|
| +0 | `00A23DE0` | `ecx=[this+36]; test; je → al=1; else jmp 009D54E0` |
| +4 | `00A25470` | if `+36==0` ret; else `00A24520(0,1)` then maybe `00A249A0` |
| +8 | `00A23C20` | `cmp arg,1; setg eax` — **does not read `this`** |
| +12 | `00A254A0` | release `+36` then `00A249A0([this+48],[this+44])` |
| +16/+20 | `009FBF00` / `009FBF10` | shared helpers |
| +24 | `013571F0` | **UNREAD** (rdata) |
| +28 | `00A2B6D0` | deleting dtor → `00A2B6F0` |

`00A23DE0` has **zero `E8` callers**. Reach is vtbl only.

Sibling ctor `00A24430` writes vtbl **`0x0129CDA4`** (16 bytes
before `CDB4`), zeros `+28/+32/+36/+40`, stores bank at `+44`
and type byte at `+48`. Inline twin at `00A24B7F` sets
`+48=4` (static). First-seen `+36` is therefore **0**.

`00A26D40` type 1/2/4/**5** allocates the **96-byte C3D record**
(`vtbl 0129CD3C` at bank+908), not a second LOD mesh. Type 5
is the same record as static. **PROVEN**.

### Ready is not a swap

```
00A23DE0  mov ecx, [ecx+36]
          test ecx, ecx
          je  00A23DEC          ; al = 1  (draw stored C3D)
          jmp 009D54E0          ; lock [this+40]+0x14C; return [this+68]
```

`009D54E0` is the generic resource-ready byte. Landscape tile
expand `00BF3BEC` and patch submit `00BDC6D9` call it on the
**resource object**, not through `00A23DE0`. Skip expand when
`al==0`. That is visibility of an already-chosen blob.

`FirstSeenLodInfoSwapsMesh=false`. House **6909/6911** and
(leftover) kid **4300** stay those ids. Lookout hero stays
**4299**. **PROVEN**.

### Later load can be PALSKIN — not first-seen

`00A25470` only runs `00A24520` when `+36≠0`. `00A24520`:

```
[ebp+44] bank
and eax, 15
sete  type==5          ; PALSKIN
sete  type==2
sete  type==4
… alloc 0xEC → 00A89190
```

So a **non-null** LODInfo can later pull a type-5 C3D. First-seen
`+36==0` never enters that path. **PROVEN** skip.

`00A23C20` returns `(lodLevel > 1)`. No first-seen caller on
Leave / Init Game / `006AC910` / `00BD71B0`. **DISPROVEN** as
this site.

---

## 3. First PALSKIN id after Leave is not an LOD sibling

From `proofs/palskin-open` + first-scene dump:

| Item | First no-save | Class as *LOD pick* |
|---|---|---|
| Creature | `CREATURE_HERO` after `PLAYER_HERO` miss | **PROVEN** id. Not a LOD |
| Graphic | **4299** `MESH_HERO` | **PROVEN**. No rewrite |
| Kid 4300 `MESH_YOUNGHERO_02` | Oakvale leftover | **DISPROVEN** |
| Father | Oakvale leftover | **DISPROVEN** |
| Submit palskin set | **`[4299]` only** | **PROVEN** |
| Prim0 | stride 36 / flags 22 / group 9 | **PROVEN** file |
| `FirstSeenPlaysAnim` | false | **PROVEN** (bind dest) |

There is no `MESH_HERO_LOD*` header enum and no second Graphic
on `CREATURE_HERO` for this Present. Do not pair 4299 with 4300
as near/far LOD.

---

## 4. Appearance-modifier “LOD” records — not first-seen

`CAppearanceModifierDef` factory `004563E8` → ctor `004546AF`
(size **112**, vtbl `0x012330DC`). Live hair:

| Script | Graphic trap | Modifier | Real PALSKIN |
|---|---|---|---|
| `OBJECT_HERO_HAIR_YOUNG_01` | **4126** folded hat | **11656** | **4275** + morph **4276 / 4277** |

`2026-08-18-palskin.md` reads modifier 11656 as count **3** then
records `(lod?, unk, meshId, 1.0, lo, hi)`. That is a **def**
LOD / morph table, not `C3DMeshLODInfo`.

`0057A43E` (next to the `CAppearanceModifierDef` name intern):

```
if [this+80] != 0 → 0
[this+64] == 16  → 1
[this+64] == 128 → 2
[this+64] == 256 → 3
else → 0
```

Callers `0057FB86` / `0057FBBB` / `0057FBF0`: if a 72-byte
vector has **5** entries, index it by that 0..3 and load
floats at `+60/+64/+68`. **PROVEN** as a later scale/pick
helper. **DISPROVEN** as Leave / first Present.

First-seen attach is off:

| Site | First no-save | Class |
|---|---|---|
| `006AC910` / `006A9DD0` | only `CTCPhysicsControlled` | **DISPROVEN** as modifier apply |
| `005B37F7` DEFAULT | clothing GUI / `PC_UI_FRAME` only | **DISPROVEN** |
| `HeroWear` / `HeroHair` | need `00CBFB7D` | **DISPROVEN** (`hero-inventory-first`) |
| Submit 4126 | hat trap | **DISPROVEN** |
| Submit 4275–4277 | unread sockets | **DISPROVEN** as this Present |

**Answer:** modifier LOD siblings exist on disk. First Present
does not select among them.

---

## 5. Host vs native

| Host | Native first-seen | Class |
|---|---|---|
| `MeshLodInfoReady_00A23DE0(0)==1` | null `+36` → `al=1` | **MATCH** |
| `MeshLodInfoReady_00A23DE0(non0)==0` | would be `009D54E0` → `[res+68]` | **DIVERGE** stub (not first-seen) |
| `FirstSeenLodInfoSwapsMesh=false` | 4299 stays 4299 | **MATCH** |
| `FirstSceneWorld` visibility `"lod"` | ready-or-not | **MATCH** wording |
| `SubmittedPalskinMeshIds==[4299]` | one Graphic | **MATCH** set |
| Flatten 4299 through `MeshBatches` | not `00BD71B0` | **DISPROVEN** as PALSKIN draw (E) |
| `TracePalskin` father / kid 4300 | leftover Oakvale | **LEFTOVER** vs Leave |
| Invent 4275 as first LOD | attachments omitted | **DISPROVEN** |

---

## Classification table

| Claim | Status |
|---|---|
| Frontend runs skin / PALSKIN LOD | **DISPROVEN** |
| First gate after Leave is `00A23DE0` ready-or-not | **PROVEN** body / **PARTIAL** first virtual caller |
| First-seen `+36==0` → draw stored C3D | **PROVEN** |
| `00A23DE0` / `00A25470` swap 4299 → another id | **DISPROVEN** |
| `009D54E0` on landscape is skin LOD | **DISPROVEN** (tile resource-ready) |
| `00A24520` type-5 can load PALSKIN when `+36` set | **PROVEN** later path |
| That load runs on first no-save Present | **DISPROVEN** |
| `00A23C20` is `(level>1)` | **PROVEN** |
| First-seen uses `00A23C20` | **DISPROVEN** |
| First PALSKIN id is 4299, set `[4299]` | **PROVEN** |
| Kid 4300 / father / 4126 are first LOD | **DISPROVEN** |
| Modifier 11656 → 4275/4276/4277 | **PROVEN** def / **DISPROVEN** first Present |
| `0057A43E` is first-seen LOD pick | **DISPROVEN** |
| Host ready-or-not helper on null | **MATCH** |

---

## Do not

- Swap Graphic **4299** for 4300 / 4275 / a made-up `MESH_HERO_LOD`.
- Treat landscape `009D54E0` as character LOD.
- Call `00A24520` / `00A25470` on first Present (`+36` is 0).
- Submit **4126** or **4275–4277** as the first skin LOD.
- Run any of this on frontend frames.
- Use `PalskinPipelineTests` father/kid fixtures as Leave LOD.
- Pretend `MeshLodInfoReady_00A23DE0(non0)` is native `[res+68]`.

Next recoverable slice is still the first virtual caller of
`0129CDB4+0` on 4299 (thing construct vs `00BD71B0`), not a
second mesh.
