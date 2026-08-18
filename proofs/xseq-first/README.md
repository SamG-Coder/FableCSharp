# First XSEQ / type-6 open after Leave Frontend

Investigation only. No production `src` edits.
Do **not** start at Oakvale / `CS_WAKING_UP_LOOP` / `3420` /
`PlayAnimation` `00CC15DA`. That path is later
`Q_NewOakValeIntro` / `CS_OAKVALE_INTRO_FATHER`,
not Leave / Init World / first no-save 3D Present.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER**.

Sources: `docs/runtime/FORWARD_TREE.md` §§7, 14–15;
`docs/status/investigations/2026-08-18-resource-manager.md`;
`docs/status/investigations/E-player-palskin.md`;
`XSeqFile.cs` / `MeshBank.cs` / `EngineLifecycle.cs`;
`XSeqFormatTests` / `EngineLifecycleTests`;
listings `00A27030` / `00AA4710` / `00A999B0` / `00AA4680` /
`00A26D40` / `004A6E30` / `006FAA90`.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  PlayAVI / frontend 2D   // no MBANK, no 3DAF/XSEQ
  Init Engine 0042E204    // render cam only
0042F2A2 Leave frontend
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
    … cameras …
    0049E620 Init Mesh Bank          // FIRST 3DAF/XSEQ objects
      00A09F20 MBANK_ALLMESHES
        miss → 00A27030 size 0x460
          alloc 0xC0 → 00AA5C80 XBDS     bank+356
          alloc 92   → 00AA4710          bank+360
                         └─ 00A999B0 3DAF + nested XSEQ tag
          alloc 76   → 00AA0F60          bank+960 (world+68)
          009D56C0 directory only
      004BBFD0 [0x13B8A04]
    00AEAA90 / 00AEAA80 particle banks
    006FAA90 Init Animation Event Managers   // not XSEQ
    006FABF0 / 006F5C10 Init Animation Events
  00416953 Load world FinalAlbion.wld
004189C2 first pumps
  FirstSeenPlaysAnim=false
  no 00A26D40 type-6 payload
  no 00A4C5E0 / 00A4CDD0 clip unpack
  first PALSKIN dest = bind locals (00AA0090 on bank+960)
```

`3420` `CS_OAKVALE_DREAM_INTRO_YOUNG_HERO_WAKING_UP_LOOP` and
`Hero.PlayAnimation CS_WAKING_UP_LOOP` are **not** on this list.
**PROVEN**.

---

## 1. XSEQ / type-6 during frontend?

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D UI | **PROVEN** | FORWARD_TREE §4; `0042DF9E` / type `0x22` |
| Retail banks at bootstrap include `MBANK_ALLMESHES` | **DISPROVEN** | `009A8150` is GBANK/PARTICLE pairs only |
| `00A27030` / `00AA4710` / `00A999B0` during `0042EC7C` | **DISPROVEN** | Mesh bank is `004A6E30` after Leave |
| Frontend / Leave starts `PlayAnimation` / `00CBFB7D` | **DISPROVEN** | camera-after-leave; `FirstSeenCallsPlayAnimationDispatcher=false` |
| Frontend.big type-6 widgets are XSEQ | **DISPROVEN** | UI type 6 is `0054EF00` glyphs, not `3DAF` |

**Answer:** no type-6 clip and no `3DAF`/`XSEQ` object during frontend.

---

## 2. First 3DAF / XSEQ object after Leave

Not a graphics.big payload. First *constructed* objects are empty
persist helpers inside `MBANK_ALLMESHES` ctor `00A27030`:

| Order | VA | Object | Class |
|---|---|---|---|
| 1 | `00AA5C80` | XBDS `bank+356` size `0xC0` vtbl `0129E450` | **PROVEN** construct. Not a clip. |
| 2 | `00AA4710` → `00A999B0` | 3DAF-derived `bank+360` size **92**. Tags `"3DAF"` then `"XSEQ"`. Vtbl `0129E060` → `0129E1E4`; nested tag vtbl `0129E194` | **PROVEN** first `3DAF`/`XSEQ` ctor. Empty children (`ANRT` alloc inside `00A999B0`). No BIG read. |
| 3 | `00AA4680` | Small nested XSEQ size **28** (`+16/+20/+24=0`). **0 E8** sites | **PROVEN** as the in-place ctor `XSeqFile.CtorXseq`. First-seen call is **UNREAD** (vtbl / persist `00A98C80` from `00AA4710`, not a direct E8). |
| 4 | `00AA0F60` | Mixer `bank+960` size 76 vtbl `0129E134`. Two `0x1000` scratch blocks | **PROVEN**. Copied to `world+68`. Used later by `00AA0090`. |

`00AA49D0` only writes the RTTI string `"Compressed Animation Sequence"`
into a `CString`. It is **not** an open.

Directory `009D56C0` / `009CFBC0` then binds `graphics.big` MESH
and **indexes** type 6 into `bank+932`. **PROVEN** bucket
(`00A26660`: types 6–10). **DISPROVEN** as parse: `ParsedCount=0`
at open; `00A26D40` is later vtbl+48.

**Answer:** first XSEQ *object* is the empty `00AA4710` helper on
the mesh bank at Init World. First *file* for type-6 is the MESH
directory only.

---

## 3. Init Animation Event Managers — not a clip open

After `0049E620` / particle bank hooks:

```
004A75A7  006FAA90   Init Animation Event Managers  (ecx = bank)
004A75FC  006FABF0   Init Animation Events  (cl=1)
004A7601  006F5C10   event table 0x14C
```

| Site | What | Class |
|---|---|---|
| `006FAA90` | Two 64-byte managers (`vtbl 01264974`) via `006FD460`. Names `"Loading Game Animation Events"` / `"Loading Sound Animation Events"` live in `006FABF0` | **PROVEN** construct |
| `006FABF0` | `006FA4E0` name bind on those managers | **PROVEN** as event lists. **DISPROVEN** as `00A999B0` / type-6 |
| `006F5C10` | alloc `0x14C` → `008C2530` | **PARTIAL** (event object). 0 E8 to XSEQ ctors |

`EngineLifecycle.InitWorldInitStages` lists Mesh Bank then jumps
to UI Manager. Native has particle + animation-event stages in
between. **LEFTOVER** (Note table only; host does not run those
ctors).

---

## 4. First type-6 *payload* (graphics.big clip)

`XSeqFile.Parse` is the host stand-in for:

```
00A243B0  vtbl+52 get-or-load
  miss → 00A26D40  vtbl+48
    type 1/2/4/5 → 96-byte C3D record
    type 3       → 16-byte
    type 6/7/8/9/10 → 40-byte record (00A2D200 / 00A25970)
00A4C5E0  UnpackFn     stream → buffer
00A4CDD0  PersistLoadFn  44-byte clip records (add 44 in 00A4DFF8 loop)
00A4EFC0  CompressFn   write path
```

| Claim | Class |
|---|---|
| `00A26D40` type-6 branch exists | **PROVEN** (`cmp ebx, 6` / `00A26ED3`) |
| First no-save pump / create / first Present calls that branch | **DISPROVEN** as first-seen. `FirstSeenPlaysAnim=false`. Create `006AC910` has no PlayAnimation / STAND / `005B37F7`. `009AD410` is handle-only. First C3D parse is type 1/2/4/5 (hero 4299), not type 6 |
| `00A4C5E0` / `00A4CDD0` on Leave / Init World | **DISPROVEN**. Callers are inner persist (`00A4DFF8`, `00A503AE`), not `004A6E30` / `0049E620` |
| First clip id / name after Leave | **UNREAD**. Appearance+52 `00662A00` body UNREAD. Do not invent Lookout `DEFAULT` / `STAND` |
| Wake `3420` is that first clip | **DISPROVEN**. Oakvale dream leftover; `XSeqFormatTests` fixture only |

**Answer:** first *payload* type-6 open is **not** on the Leave /
Init Game / first Present spine. Host `MeshBank.GetAnim` is the
right later slot; nothing on `EngineLifecycle` New Game calls it.

---

## 5. First *use* of the XSEQ helper (still not a clip)

| Site | When | Class |
|---|---|---|
| `00AAF1E0` from `00A8AD08` | C3D bone-local copy (`+160` 48-byte locals). First hero/static C3D parse | **PROVEN** as mesh serialize helper. **DISPROVEN** as clip open |
| `00AA0090` from `00BD2E35` | PALSKIN packer: `ecx = [mesh+80]+4` then `[that+960]` (the `00AA0F60` object) | **PROVEN** first hierarchy call once dest is packed. Input is bind locals when no clip (`FirstSeenPlaysAnim=false`) |
| Time interp inside `00AA0090` | leftover until a clip plays | **UNREAD** |

**Answer:** first animation-system *use* after Leave is hierarchy
on the empty bank mixer, driven by C3D locals, not a type-6 sample.

---

## 6. C# vs native

| Site | What | Class |
|---|---|---|
| `MeshBank.Open` directory, `ParsedCount=0`, type 3 dropped | MESH open | **MATCH** directory. Type-3 skip is **PARTIAL** vs native `bank+920` |
| `00A27030` helpers `00AA5C80` / `00AA4710` / `00AA0F60` | not constructed in C# | **LEFTOVER**. Host has no 92-byte 3DAF prototype |
| `MeshBank.GetAnim` / `XSeqFile.Parse` | on-demand type-6 | **EQUIVALENT** slot to `00A26D40` type 6 + `00A4C5E0`. **Not** first-seen New Game |
| `XSeqFile.CtorXseq=00AA4680` | nested 28-byte ctor | **PROVEN** addr. First live object is **`00AA4710`**, which *calls* `00A999B0` |
| `WakeLoopId=3420` | Oakvale dream | **LEFTOVER** vs Leave. Keep as format fixture |
| `PaletteForPose(..., sequence)` | first-key sample | **PROVEN** format. Unused on first Present |
| `AnimationRuntime.Clips` / `GetAnim` from lifecycle | never | **MATCH** skip |
| `InitWorldInitStages` omits `006FAA90` / `006FABF0` | Note table | **LEFTOVER** |

---

## Classifications (short)

1. **Frontend XSEQ / type-6 open — DISPROVEN.** No mesh bank, no
   `00A999B0`. UI type 6 is glyphs.
2. **First XSEQ after Leave — empty `00AA4710`/`00A999B0` helper
   on `00A27030` at Init Mesh Bank. PROVEN.** Directory then
   indexes type 6; does not parse.
3. **Init Animation Events — event managers. PROVEN construct.
   DISPROVEN as clip open.**
4. **First graphics.big type-6 payload — not on this spine.
   DISPROVEN as first-seen.** Id/name **UNREAD**. `3420` wake
   **DISPROVEN** as this site.
5. **C# `GetAnim` during frontend / New Game — LEFTOVER if called;
   MATCH if left on-demand.** Native first Present stays bind-pose.

Do not treat `XSeqFile.Parse(3420)` or `PlayAnimation` apply as
the first open after Leave.
