# MBANK_ALLMESHES first open after Leave — frontend?

Investigation only. Production `src/` was not edited.

Question: is `MBANK_ALLMESHES` (`graphics.big` MESH / `MeshBank`)
first opened **after Leave only**, or does frontend already open
it?

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE** / **LEFTOVER**.

Sources: `MeshBank.cs`, `EngineLifecycle.cs` (`OpenMeshBank`,
`PumpFrontendFrame`, `RequestNewGame`, `EnterGame`); ExeIndex
`listing-00480000.txt` / `listing-00a00000.txt` /
`listing-00b00000.txt` / `xrefs.tsv` / `strings.tsv`;
`docs/runtime/FORWARD_TREE.md` §§4, 6–7, 14; resource-manager /
F-load-performance; siblings `texture-library-open`,
`palskin-open`, `xseq-first`, `audit-creature-leftovers`.

---

## Verdict

**Yes. First-seen no-save: `MBANK_ALLMESHES` first open is after
Leave only.** Frontend never opens it.

| What | First native site | Relative to Leave |
|---|---|---|
| `frontend.big` `GBANK_FRONT_END` | `0042DDB3` / `009F83D0` | **before** Leave **PROVEN** |
| `textures.big` `GBANK_MAIN` | Init Graphics `00416C8A` / `009F83D0` | **after** Leave **PROVEN** (sibling) |
| `graphics.big` `MBANK_ALLMESHES` directory | `004A6E30` → `0049E620` → `00A09F20` miss | **after** Leave **PROVEN** |
| C3D / type-5 / type-6 parse | later `00A243B0` / `00A26D40` | **after** Leave + Lookout; **DISPROVEN** as this open |

Bootstrap `009A8150` is GBANK/PARTICLE name pairs only.
**DISPROVEN** that it registers or opens `MBANK_ALLMESHES`.

The only `.text` push of `"MBANK_ALLMESHES"` is `0049E655`.
The only `E8` to `0049E620` is `004A750B` (Init World Init).
The only `E8` to `00A09F20` is `0049E679` (inside that open).
**PROVEN** one first-seen site.

---

## Timeline (no-save New Game)

```
00402510 bootstrap
  009A8150  GBANK_* / PARTICLE_* pairs     // no MBANK_ALLMESHES
0042EC7C retail
  PlayAVI / 0042E98F frontend.big
  Init Engine 0042E204
    00B3CB30 shader manager
      intern "MBANK_ENGINE" @ 00B3D749     // NOT ALLMESHES; not 00A09F20
  0042DF9E 2D UI  (type 0x22)
    009AD410 UI def Type=10                // GameBin, not MESH
0042F2A2 Leave frontend
0042F491 Init Game → 00418DCA → 004184BD
  Init Graphics 00416C8A                   // textures.big first
  Init World 0041735A → 004A6E30
    "Init Mesh Bank Manager"               // log only
    "Init Mesh Bank" 0049E620              // FIRST graphics.big MESH
      "Opening Mesh Bank"
      push "MBANK_ALLMESHES"
      009A4EC0 engine
      00A09F20([engine+116], name)
        miss → 00BFEA1A(0x460) → 00A27030
        vtbl 0129CE94; tables empty
        [bank].vtbl+4 009D56C0 → 009A7F80 / 009CFBC0 directory
      world+60 / +64 = handle
      world+68 = [bank+960]
      "Setting Mesh Bank"
      004BBFD0  mov [0x13B8A04], ecx
    00AEAA90 particle mesh-bank hook
  00416953 Load world FinalAlbion.wld
004189C2 first pumps                       // ParsedCount still 0
later Lookout 00A243B0 miss                // first C3D, not this open
```

---

## 1. Frontend must not open `MBANK_ALLMESHES`

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D (`0042DF9E` / type `0x22`) | **PROVEN** | FORWARD_TREE §4; `PumpFrontendFrame` |
| `MBANK_ALLMESHES` during `0042EC7C` | **DISPROVEN** | first `0049E620` is Init World after Leave |
| `009A8150` includes the mesh bank | **DISPROVEN** | GBANK/PARTICLE pairs only; string not in that table |
| A second `.text` call of `0049E620` / `00A09F20` | **DISPROVEN** | one `E8` each in the listings |
| Press Start `009AD410` is a mesh handle | **DISPROVEN** | `0041D21B` `[def+60]` Type=**10** UI |
| Frontend UI type 6 is a C3D / XSEQ | **DISPROVEN** | `0054EF00` glyphs |
| `00A27030` / `009D56C0` / `004BBFD0` on frontend | **DISPROVEN** | no mesh bank object yet |
| `InitFrontendUi` / `PumpFrontendFrame` call `OpenMeshBank` | **PROVEN** absence | frontend body; `OpenMeshBank` is EnterGame / Present / Submit |
| Leave `0042F2A2` opens the bank | **DISPROVEN** | Leave is audio/UI teardown + `FinalAlbion.wld` record |

**Answer:** frontend may intern `MBANK_ENGINE` on the shader
manager (`00B3CB30` @ `00B3D749`, Init Engine, still retail).
That is **not** `00A09F20`, **not** `0049E620`, and **not**
`graphics.big` MESH. Do not `MeshBank.Open` / `Meshes.Get`
during `PumpFrontendFrame`.

---

## 2. What “open” is (and is not)

| Layer | VA | Parses C3D? | This first-open? |
|---|---|---|---|
| Bootstrap name pairs | `009A8150` | **No** | **DISPROVEN** (name absent) |
| `MBANK_ENGINE` intern | `00B3D749` in `00B3CB30` | **No** | **DISPROVEN** (other string) |
| Named lookup | `00A09F20` | **No** | first-seen **miss** **PROVEN** |
| Object ctor | `00A27030` size `0x460` vtbl `0129CE94` | **No** (`009D5230` is `ret 4`) | **PROVEN** construct |
| Directory bind | `vtbl+4` `009D56C0` → `009CFBC0` | **No** (ids/types/offsets) | **PROVEN** file bind |
| Global | `004BBFD0` `[0x13B8A04]` | **No** | **PROVEN** (`mov [0x13B8A04], ecx`; only `E8` is `0049E72F`) |
| Def → 12-byte handle | `009AD410` | **No** | later TNG / UI |
| Get-or-load | `00A243B0` / `00A26D40` | on miss | **DISPROVEN** as directory open |

`00A09F20` miss: `00BFEA1A(0x460)` → `00A27030`, then 12-byte
handle `{refcount=1, dtor=0x428AE7, object}`, then
`[bank].vtbl+4(name)` (`009D56C0`). Hit would copy the existing
handle. First-seen list at `[engine+116]+8` is empty → miss.

Host `MeshBank.Open`: `graphics/graphics.big` + first sub-bank
whose name contains `"MESH"`. `Opened=true`, `EntryCount` ~6729,
`ParsedCount=0`. **MATCH** directory-only.

---

## 3. First MESH directory after Leave — **PROVEN**

`004A6E30` (`world.vtbl+36`) logs `"Init Mesh Bank Manager"`
then `"Init Mesh Bank"` and `call 0049E620` at `004A750B`.
That is after Leave `0042F2A2` → Init Game `0042F491` →
`004184BD` `"Init World"` `0041735A`.

`0049E620` (`ecx` = world):

```
"Opening Mesh Bank"
push "MBANK_ALLMESHES"
009A4EC0 → [engine+116]
00A09F20(...)
[world+60]/[+64] = {object, handle}   // release old if any
[world+68] = [bank+960]
"Setting Mesh Bank"
004BBFD0
```

Init Graphics (`00416C8A` / `textures.big`) runs **before** this
on the same `004184BD` list. Mesh bank is not the first BIG after
Leave; it is the first **MESH** named bank.

`00AEAA90` (“Setting Particle Engine Mesh Bank”) is a hook on
the already-open handle at `world+60`. **DISPROVEN** as a second
`00A09F20`.

---

## 4. Host vs native

| Host | Native first-seen | Class |
|---|---|---|
| `PumpFrontendFrame` does not `OpenMeshBank` / `Get` | no MESH | **MATCH** |
| `RequestNewGame` does not open banks | Leave teardown | **MATCH** |
| `EnterGame` Init World stage → `OpenMeshBank` | `004A750B` → `0049E620` | **MATCH** |
| `MeshBank.Open` directory, `ParsedCount=0` | `009D56C0` / `009CFBC0` | **MATCH** |
| One `Meshes` on `EngineLifecycle` | one 0x460 + `[0x13B8A04]` | **MATCH** |
| `PresentWorld` / `SubmitCurrentWorld` re-call `OpenMeshBank` | no-op if `Opened` | **MATCH** once |
| Frontend `009AD410` via `ResolveFrontendDef` / GameBin | UI Type 10 | **MATCH** (not MESH) |
| `WorldGeometry.Build` `meshes ??= new MeshBank()` then `Open` | native has one world bank | **LEFTOVER** if called without `Meshes`; production `PresentWorld` passes `Meshes` |
| Test `new MeshBank()` / `LoadCreature` | format lock, not New Game | test-only; must not run on frontend |

`OpenMeshBank` sites: `EnterGame` (`InitMeshBankFn`),
`SubmitCurrentWorld`, `PresentWorld`, `ExpandPresentedWorld`.
All after Leave. Guard `Meshes.Opened || Install is null`.

`Fable.Client` does not construct `MeshBank`.

---

## Classification table

| Claim | Status |
|---|---|
| First `graphics.big` MESH open is `0049E620` after Leave | **PROVEN** |
| That open is Init World `004A6E30` / `004A750B` | **PROVEN** |
| First-seen `00A09F20` is a miss → `00A27030` 0x460 | **PROVEN** |
| Directory only (`ParsedCount=0`); no C3D walk | **PROVEN** |
| `004BBFD0` stores `[0x13B8A04]` | **PROVEN** |
| `MBANK_ALLMESHES` during frontend / AVI / Leave | **DISPROVEN** |
| `009A8150` opens or names `MBANK_ALLMESHES` | **DISPROVEN** |
| Frontend `009AD410` is MBANK | **DISPROVEN** (UI Type 10) |
| `MBANK_ENGINE` is this open | **DISPROVEN** |
| `MBANK_ENGINE` file I/O at `00B3D76C` vtbl+4 | **UNREAD** (not `00A09F20`) |
| Skip-frontend / `[0x13B8648]` first bank | **UNREAD** (not no-save) |
| Host `WorldGeometry` auto-`new MeshBank` | **LEFTOVER** helper; not frontend |

---

## Do not

- Open `graphics.big` / `MeshBank` / `Meshes.Get` on frontend frames.
- Treat Press Start `009AD410` or type-6 glyphs as MESH ids.
- Parse every C3D at `0049E620`.
- Invent a second `graphics.big` dump beside `MeshBank`.
- Collapse `frontend.big` / `textures.big` / `graphics.big` into one library.
- Call `MBANK_ENGINE` registration an `MBANK_ALLMESHES` open.

Next slice is still first C3D payload after this directory
(`palskin-open` / `c3d-first-submit`), not a frontend mesh bank.
