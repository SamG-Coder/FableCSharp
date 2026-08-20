# UseCamera first-seen spline: SNAP vs PLAY

Investigation only. No production `src` edits.

Question: when intro fiber leftover `UseCamera CAM_OVIF_SHOT2`
runs, does the engine **SNAP** the helper pose or **PLAY**
the TNG spline? Host `ScriptedCamera.Playing` on `Bind`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

Sources:

- ExeIndex listings
  `tools/Fable.ExeIndex/out/01-sections/script-runtime/usecamera-token-00cc9f39-00cc9f39.md`
  (token `00CC9F39`; activate label `00CC9F3A` is mid-`push`),
  `usecamera-bind-00cca109-00cca109.md`,
  `usecamera-name-bind-00cca1aa-00cca1aa.md`,
  `usecamera-yield-00cca22c-00cca22c.md`,
  `tools/Fable.ExeIndex/out/01-sections/newgame-trace/bind-camera-00b23b50-00b23b50.md`,
  `camera-536-helper-gate-00b314e0-00b314e0.md`,
  `camera-update-helper-fov-00b314e0-00b314e0.md`,
  `camera-spline-update-00b31160-00b31160.md`,
  `spline-enable-536-00b2fc10-00b2fc10.md`,
  `camera-ctor-00b31700-00b31700.md`,
  `calls-camera-update-00b314e0-00b314e0.md`,
  `text-map/listing-00cc0000.txt` (`00CCA166`–`00CCA266`),
  `text-map/listing-00b00000.txt` (`00B23B50` / `00B2FC10` / `00B31160` / `00B314E0` / `00B31700`)
- TNG dump `tools/Fable.ExeIndex/out/startoak-tng.txt` (repo
  `export/` and `dump/` have **no** SHOT2 TNG)
- Host `src/Fable.Game/ScriptedCamera.cs` (`Bind` /
  `Playing=false`), `LandscapeFrustum.cs`
  (`FirstSeenSplineEnabled=false`), `RegionTravel.cs`
  (`TryCameraFromThing` key 0)
- Tests `WorldSceneTests` (SHOT2 type + FOV +
  `FirstSeenSplineEnabled`), `ScriptRuntimeArchitectureTests`
  (`UseCamera_snap_then_WaitForCamera_continues`),
  `DataCatalogTests` (`UseCamera CAM_OVIF_SHOT2` in
  `CS_OAKVALE_INTRO_FATHER`)
- `docs/runtime/COMMAND_MAP.md` / generated (`UseCamera`
  apply `00B23B50`; `WaitForCamera` snap idle)

---

## Verdict

**SNAP.** First-seen intro `UseCamera CAM_OVIF_SHOT2` binds
the TNG helper and updates through `00B314E0` with
**`+536==0`**. That `je 00B31502` helper path copies
`+0/+12/+24`. It does **not** call spline update
`00B31160`.

`CAM_OVIF_SHOT2` **is** `CAMERA_POINT_SCRIPTED_SPLINE`.
The type name is **not** play. Play requires
`00B2FC10` (`[cam+536]=1`). That fn has **zero**
`call 00B2FC10` sites and no vtbl listing. Bind
`00B23B50` never writes `+536`. Ctor `00B31700`
zeros it.

Host `ScriptedCamera.Bind` sets **`Playing=false`**.
Do not invent spline play on first-seen while `+536`
stays 0.

| Claim | Status |
| --- | --- |
| TNG `CAM_OVIF_SHOT2` is `CAMERA_POINT_SCRIPTED_SPLINE` | **PROVEN** (`startoak-tng.txt` + `WorldSceneTests`) |
| Type name ⇒ `00B31160` play on `UseCamera` | **DISPROVEN** |
| `UseCamera` token / activate `00CC9F39` / `00CC9F3A` | **PROVEN** |
| Bind apply is `00B23B50` (`00B2FBF0` + `00B314E0(1)`) | **PROVEN** |
| `00B23B50` writes `+536` or calls `00B2FC10` | **DISPROVEN** |
| `00B314E0` `+536==0` → helper `00B31502`, skip `00B31160` | **PROVEN** |
| `00B2FC10` is the `+536=1` enable | **PROVEN** |
| `00B2FC10` static `E8` xrefs | **PROVEN** zero (`call 00B2FC10` absent) |
| First-seen `+536` starts 0 (`00B31700`) and stays 0 | **PROVEN** (no enable on this path) |
| Host `Bind` leaves `Playing=false` | **PROVEN** |
| `WaitForCamera` after snap continues (`vtbl+1672` idle) | **PROVEN** host + command map |
| `CameraPath` / rig / rotate `BeginTransition` (`Playing=true`) | **PROVEN** (different verbs) |
| Repo `export/` / `dump/` SHOT2 property dump | **UNREAD** (not present) |
| `vtbl+1648` / `+1656` bodies → `00B23B50` instruction walk | **PARTIAL** (map names apply `00B23B50`; no E8) |
| Multi-key spline sample if someone later sets `+536` | **UNREAD** (not first-seen) |

---

## Recovered path (intro leftover, not Leave Present)

```
CS_OAKVALE_INTRO_FATHER   // persist vector 0
  CameraPause FALSE       // [ebp-37]=0
  UseCamera CAM_OVIF_SHOT2
    00CC9F39  push "UseCamera"
    00CC9F3A  … 00BFEAF8 match
    lookup TNG name
    thing ready? 004AB150 / 004AB130==1
      yes → [0x143E8F8] vtbl+1656(thing, name, …)
      no  → [0x143E8F8] vtbl+1648(name, 0, 0, …)
    00CCA22C  [ebp-37]==0 → skip vtbl+28
  engine bind 00B23B50
    00B2FBF0  [0x1436EA0]+12 = helper
    push 1
    00B314E0
      [esi+536] == 0          // ctor 00B31742 bl=0
      je 00B31502             // SNAP helper
      // not taken: call 00B31160 PLAY
      helper +0/+12/+24 → pos / look / up
      00B30B50
```

No-save first Present is still Lookout / WorldCamera
`006B4900` (`FirstSeenCallsUseCamera=false`). This
file is the Oakvale intro `UseCamera` leftover only.

---

## 1. TNG: type is spline; dump/export empty

`tools/Fable.ExeIndex/out/startoak-tng.txt`:

```
Thing  CAMERA_POINT_SCRIPTED_SPLINE  CAM_OVIF_SHOT2  (40.091, 130.258, 15.756)
Thing  CAMERA_POINT_SCRIPTED_SPLINE  CAM_OVIF_SHOT6  (39.853, 129.899, 15.754)
```

`WorldSceneTests` (live install TNG, not `export/`):

- `DefinitionType == CAMERA_POINT_SCRIPTED_SPLINE`
- `HeroIsSubject=FALSE` (`FirstSeenHeroIsSubject=false`)
- `CoordAxisUp` → `(0,0,1)`
- `CTCCameraPointScriptedSpline.FOV` = `0.2`
- `KeyCameras[0].FOV` = `0.2` → 72°
- no `CTCCameraPointScripted.FOV` (non-spline key)

`export/` is fonts / frontend / native screenshots.
No `dump/` tree. No SHOT2 property file there.

Host pose uses **key 0 only**
(`RegionTravel.TryCameraFromThing`). That is a snap
sample of the first key, not `00B31160` lerp.

---

## 2. UseCamera `00CC9F3A`

Token at `00CC9F39` is `push "UseCamera"`. Listing
`usecamera-activate-00cc9f3a` starts one byte late
(`lodsb`) — use the token listing +
`listing-00cc0000.txt`.

Activate:

1. `00BFEAF8` verb match; fail → `00CCA26B` next token
   (`PlayAVI`).
2. Required name at `[ebp+40]`; empty / IsFalse →
   `00CD17FD` (continue, no bind).
3. Optional time at `[ebp+44]` → `[ebp-172]` else
   `0xBF800000` (−1).
4. Optional thing at `[ebp+48]`.
5. `00CCA148`–`00CCA164`: if the looked-up object is
   ready (`004AB150` / `004AB130==1`) call
   **`[eax+1656]`** (`00CCA19A`). Else
   **`[eax+1648]`** (`00CCA1E3`) with two `push 0`.
6. Optional `"CAMERA:"` log (`00CCA1EF`).
7. Yield `00CCA22C`: `[ebp-37]==0` (CameraPause FALSE)
   **skips** `vtbl+28`. Father command 2 is
   `CameraPause FALSE`. First SHOT2 bind does not
   wait on spline end.

No `call 00B2FC10`. No `+536` write. No `00B31160`.

Apply site in the command map is **`00B23B50`**
(engine helper bind). Context `vtbl+1648/+1656`
bodies are **PARTIAL** (zero `E8` of `00B23B50`;
engine slot is vtbl+16).

---

## 3. Bind `00B23B50`

```
00B23B50  push esi / push edi
00B23B52  mov edi, [esp+12]        // helper
00B23B58  mov ecx, [0x1436EA0]
00B23B5F  call 00B2FBF0            // [cam+12] = helper
00B23B64  mov ecx, [0x1436EA0]
00B23B6A  push 1
00B23B6C  call 00B314E0
00B23B71  mov [esi+20], edi        // last helper
00B23B76  ret 4
```

`00B2FBF0` is three insns: store arg at `ecx+12`.
`E8` callers: `00B23B5F`, `00B2798F` only.

Neighbour `00B23B80` is `jmp 00B2FC00` (store
`cam+16`), **not** `00B2FC10`.

---

## 4. Update `00B314E0` / spline `00B31160`

```
00B314E0  sub esp, 0x94
00B314E7  mov esi, ecx
00B314E9  mov al, [esi+536]
00B314EF  test al, al
00B314F1  je 00B31502              // SNAP
00B314F3  call 00B31160            // PLAY
00B314FF  ret 4
00B31502  … helper +0/+12/+24 …
00B316E8  call 00B30B50
```

`00B314E0` `E8` callers: `00B23B6C`, `00B2799D`.
Neither writes `+536`.

`00B31160` only runs when `+536!=0`. It walks
`[esi+520]`…`[esi+524]` records (stride 32),
advances `[esi+532]` / `[esi+544]` with
`009E1BC0` dt, lerps pos/look/FOV, then
`00B30B50`. First-seen never enters it.

---

## 5. Enable `00B2FC10` — zero static xrefs

```
00B2FC10  push esi
00B2FC13  mov [esi+536], 0x01
00B2FC1A  mov [esi+532], 0x0
00B2FC24  call 009E1BC0
00B2FC29  fstp [esi+544]
00B2FC30  ret
          int3 pad
```

Repo-wide `call 00B2FC10`: **no hits**. No vtbl
listing names it. `functions.tsv` row at
`0x00B2FC10` is a **1607-insn merge** into later
`00B2FC50` / frustum — not callers of this stub.

Ctor `00B31700`:

```
00B3170C  xor ebx, ebx
00B3173C  mov [esi+12], ebx
00B31742  mov [esi+536], bl        // 0
00B31748  mov [esi+532], ebx
```

`LandscapeFrustum.FirstSeenSplineEnabled = false`.
`SplineFlagOffset = 536`. `SplineEnable = 00B2FC10`.

Without an enable, first-seen `+536` stays 0. Do
not invent play.

---

## 6. Host `Playing` stays false on Bind

`ScriptedCamera.Bind`:

```
ScriptCameraActive = true;
Playing = false;
```

Comment: snap `UseCamera` arrives immediately.
`BeginTransition` is Path / Rig / Rotate /
look-between only.

`UseCamera_snap_then_WaitForCamera_continues`:
after `UseCamera`, `Playing==false`,
`WaitForCamera` continues, `ResetCamera` runs.
`WaitForCamera` (`00CCA41F` / `00CCA58F`) polls
`vtbl+1672`; `al==0` → `00CD17FD`.

`CS_OAKVALE_INTRO_FATHER` contains
`UseCamera CAM_OVIF_SHOT2`. It does not need
`WaitForCamera` for that snap to hold.

---

## SNAP vs PLAY (VAs)

| Role | VA | First-seen intro |
| --- | --- | --- |
| UseCamera token | `00CC9F39` | runs |
| UseCamera activate | `00CC9F3A` | TNG name bind |
| Thing bind | `00CCA19A` `vtbl+1656` | if object ready |
| Name bind | `00CCA1E3` `vtbl+1648` | else |
| Yield | `00CCA22C` `vtbl+28` | **skipped** after CameraPause FALSE |
| Engine bind | `00B23B50` | SNAP store + update(1) |
| Store helper | `00B2FBF0` | `[cam+12]` |
| Update gate | `00B314E0` | **`+536==0` → `00B31502`** |
| Helper FOV / axes | `00B31502`…`00B30B50` | SNAP |
| Spline play | `00B31160` | **not called** |
| Spline enable | `00B2FC10` | **no xref; +536 stays 0** |
| Ctor zero +536 | `00B31742` | first-seen start |
| Host Playing | `ScriptedCamera.Bind` | **false** |

**Answer: SNAP (`00B23B50` → `00B314E0` `je 00B31502`).
Not PLAY (`00B31160`).**
