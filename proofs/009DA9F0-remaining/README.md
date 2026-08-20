# Remaining `009DA9F0`: first Present dummy/null skip

Investigation only. Production `src/` and `tests/`
were not edited.

Question: recover the **first-seen empty skip**
of `009DA9F0`. Is first Present dummy / null
region? Host leftover Notes vs **MATCH** skip?
Does `00435530` flush `ScenePasses` via
`009DA9F0(1)` DIP vtbl+332? Frontend
`0042DF9E` also `009DA9F0(1)` twice — is that
a world submit?

Do **not** invent world submit on frontend
Present. Do **not** start at Oakvale /
`Q_NewOakValeIntro` / `00DBDE40`. Do **not**
re-prove dest writers (leftover #36) or the
`00BAE2D0` DIPUP path (`proofs/009DA9F0-vs-0042DF9E`).

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Authority: `proofs/009DA9F0-vs-0042DF9E`;
`proofs/fog-first-present`;
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-009c0000.txt`
(`009DA9F0` / `009DAA42` / `009DB6E6` /
`009DB365` / `009DB700`);
`listing-00400000.txt` (`0042DF9E`
`0042E136`/`0042E15A`, `00435530`
`00435D4D`);
`src/Fable.Game/EngineLifecycle.cs`
(`DisplayFlushShouldDip`,
`SubmittedLayerBits`,
`FlushFrontendDisplay`,
`ApplyDisplayCamera`,
`FlushSubmittedLayers`,
`DisplaySubmitStages`,
`PumpFrontendFrame`,
`SubmitCurrentWorld`);
`src/Fable.Formats/Scene/ScenePass.cs`
(`ScenePasses.Registration`);
`docs/status/README.md` ScenePasses row;
`docs/PARITY.md` first-seen `00435530` dest;
`EngineLifecycleTests.Frontend_009DA9F0_first_seen_is_empty_skip_not_type_22`;
`EngineLifecycleTests.After_004AEA70_eq_1_00417001_is_00435F70_Present`;
`EngineLifecycleTests.Game_00435530_Presents_009BEEB0_and_pumps_input`.
Siblings: `proofs/dx9-3d-submit`,
`proofs/c3d-first-submit`,
`proofs/hud-first-present-skip`,
`proofs/current-region-no-save`,
`docs/status/investigations/A-dx9-submit.md`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First-seen empty skip condition? | `[this+16020]==[this+16024]` → count 0 (`(end-begin)*0x88888889` / 60-byte) → `je 009DB6E6`. No DIP. | **PROVEN** |
| First *game* Present region? | Dummy index **0** / `CurrentRegion==null`. Not Lookout. Not Oakvale. | **PROVEN** skip |
| That Present’s `009DA9F0(1)`? | Empty dest → `009DB6E6`. `SubmittedLayerBits` empty. | **MATCH** skip |
| `00435530` flushes `ScenePasses` via `009DA9F0` bits `0x4`→`0x40`→`0x20`→`0x100`→`0x2000`? | Host leftover pairing. Native `009DA9F0` is 2D `+16020`. 3D bits are `00B25950` inside `00B27D90`. | **DISPROVEN** as native. Status row **LEFTOVER** |
| Nonempty `009DA9F0` DIP? | `00A058C0` then `[device+88].vtbl+332` (stride 32, VB `+16008`, prim 2 or 4). First-seen **does not take**. | **PROVEN** tail; first-seen **PROVEN** skip |
| Frontend `0042DF9E` `009DA9F0(1)`? | **Twice** (`0042E136` / `0042E15A`). Same empty skip. Not world. | **PROVEN** call sites; **MATCH** skip |
| Invent world submit on frontend Present? | **No.** `PumpFrontendFrame` does not `SubmitCurrentWorld`. Native frontend 3D walk `0042E0BB` `00B27D90` is empty. | **DISPROVEN** |

---

## Verdict

**First-seen `009DA9F0` is the empty skip.
First Present is dummy / null region.
Host leftover Notes name it `FlushLayers`.
The skip itself is MATCH. Do not invent
world submit on frontend Present.**

Recovered condition (listing
`009DA9F0`…`009DAA42`):

```
edx = [this+16020]
ecx = [this+16024]
ecx -= edx
count = ecx * 0x88888889  (signed / 60)
je 009DB6E6               ; count == 0
```

Zero records → `009DB6E6` (`009F9F70` ×2,
`ret 4`). No `00A058C0`. No vtbl+332.

First no-save game Present
(`004AEA70=1` → `00435F70` jmp
`00435530`) runs that skip **once**.
Region is dummy `005066E0` index 0;
`CurrentRegion` is null; no
`00501450`; `WorldSubmitted` is false.

Frontend `0042DF9E` runs the same skip
**twice** (each after `009D9C80`).
Widgets drain via `00BAE2D0` DIPUP
vtbl+336, not `+16020`.

Host `DisplayFlushShouldDip(0, 0)` is
always false (never stores
`[this+16020]`). Notes
`009DA9F0(1) [+16020] empty` /
`empty dest` / `skip DIP 009DB6E6`.
`SubmittedLayerBits` stays empty.
`LayerFlushCount==0`. That **skip** is
**MATCH**. The **label**
`FlushLayers` / “layer bits come from
`ScenePasses.Registration`” is
**LEFTOVER**. Closing leftover by
flushing land/C3D/sky on this VA, or
by submitting world from
`0042DF9E`, is **DISPROVEN**.

---

## 1. Recovered skip (`listing-009c0000.txt`)

```
009DA9F0  sub esp, 104
009DA9F5  mov ebp, ecx
009DA9F7  mov edx, [ebp+16020]
009DA9FD  mov ecx, [ebp+16024]
009DAA03  sub ecx, edx
009DAA05  mov eax, 0x88888889
009DAA0A  imul ecx
          … sar edx, 5 …
009DAA42  je 009DB6E6
```

Empty tail:

```
009DB6E6  lea ecx, [esp+60]
          call 009F9F70
          lea ecx, [esp+68]
          call 009F9F70
009DB6FD  ret 4
009DB700  ; enqueue starts here
```

Nonempty tail (not first-seen):

```
009DB33F  call 00A058C0
          mov eax, [device+88]
          push 32
          add edx, [ebp+16008]
          push esi
          push 2   or  push 4
009DB365  call [ecx+332]
```

| Item | Value | Class |
|---|---|---|
| Queue | `[this+16020, +16024)` | **PROVEN** |
| Record size | **60** (`0x88888889`) | **PROVEN** |
| Empty | `je 009DB6E6` | **PROVEN** first-seen |
| Nonempty DIP | vtbl+**332**, stride 32, VB `+16008`, prim 2 or 4 | **PROVEN** tail |
| Enqueue | `009DB700` only (`009DC00E` / `009DD93D`) | **PROVEN** |
| `cmp …,0x22` | none in this body | **DISPROVEN** as type-`0x22` switch |
| ScenePasses bits | none in this body | **DISPROVEN** pairing |

Host formula **MATCH**es the count:

```4188:4189:src/Fable.Game/EngineLifecycle.cs
    public static bool DisplayFlushShouldDip(int begin, int end) =>
        DisplayQueueCount(begin, end) != 0;
```

Call sites always pass `(0, 0)`. That is a
recovered **skip**, not a recovered queue
read. Later nonempty `+16020` is **UNREAD**
on no-save first Present (`A-dx9-submit`
§5: which `009DD8F0` gate first opens).

Constants locked by
`Frontend_009DA9F0_first_seen_is_empty_skip_not_type_22`:
`DisplayQueueBeginOffset=16020`,
`DisplayQueueRecordSize=60`,
`DisplayQueueEnqueueFn=009DB700`,
`FirstSeenFrontendE8Enqueue=false`,
`DrawIndexedPrimitiveVtbl=332`,
`DisplayFlushShouldDip(0,0)==false`,
`Frontend2dDipIssued==false`.

---

## 2. First Present is dummy / null region

No-save after Leave (`fog-first-present`,
`PARITY.md`, `current-region-no-save`):

```
0042F2A2 Leave                  ; 0042DF9E stops
004189C2 first pumps
  WorldFrame<=1: skip 00435530
  dummy 004FC180 index 0
    WorldMap+156 = 0
    record+36 null
later 004AEA70=1
  00435F70 jmp 00435530         ; FIRST game Present
    +232>0 → 00434CD0
      +216=0 / [0x1375CDC]=0
      009D8250 ret dest empty
    00435000 skip 00639E40
    00435070 skip 0057B43F
    009D9C80
    009DA9F0(1) → 009DB6E6      ; no DIP
    009BEF50 / 009BEEB0
  no region / not 00501450
```

`After_004AEA70_eq_1_00417001_is_00435F70_Present`
locks:

- Note `009DA9F0` **empty dest**
- `SubmittedLayerBits` **empty**
- `LayerFlushCount==0`
- `CurrentRegion==null`
- `WorldSubmitted==false`
- no `LoadFromFirstRealRegion`
- no `00501450`

`Game_00435530_Presents_009BEEB0_and_pumps_input`
locks the same empty bits on the first
`009BEEB0` after `WorldFrame=2`.

`00435530` has **no** `E8` `00B25950` /
`00B27D90` / `00B6B0B0` / `00BF4570` /
`00BB2540`. Overlay/interface first-seen
**skip**. Invented always-`00639E40` /
invented layer bits **DISPROVEN**
(`PARITY.md`).

Dummy Present is **not** fog-lit Lookout
and **not** Oakvale (`fog-first-present`).
Empty `009DA9F0` is not a reason to
`SubmitCurrentWorld` or to call
`00501450` from `Pump`.

---

## 3. `00435530` vs `0042DF9E` — same helper, not ScenePasses

Same device helpers. Different envelope.
**PROVEN** listings.

| Step | Frontend `0042DF9E` | Game `00435530` |
|---|---|---|
| `009D9C80` / `009DA9F0(1)` | **twice** (`0042E136`, `0042E15A`) | **once** (`00435D4D`) |
| Arg | `push 1` | `push 1` |
| `this` | `[0x13B8384]` | `[0x13B8384]` |
| First-seen queue | empty → `009DB6E6` | empty → `009DB6E6` |
| 3D layer walk | `[retail+88].vtbl+32` `0042E0BB` = `00B27D90` (lists empty) | **no** `E8` / no `[reg+32]` to `00B27D90` |
| Widget DIP | `00BAE2D0` DIPUP vtbl+336 during `00595222` | none first-seen |

Frontend recovered (`listing-00400000.txt`):

```
0042E129  call 009D9C80
0042E134  push 1
0042E136  call 009DA9F0
0042E13B  call 00404A80
0042E142  call 00404C00
0042E14D  call 009D9C80
0042E158  push 1
0042E15A  call 009DA9F0
0042E165  call 009BEF50
0042E170  call 009BEEB0
```

Game recovered:

```
00435D40  call 009D9C80
00435D4B  push 1
00435D4D  call 009DA9F0
00435D58  call 009BEF50
00435F50  call 009BEEB0
```

`009DA9F0` is **2D** `+16020`. Pairing it as
the 3D layer walker is **DISPROVEN**
(`dx9-3d-submit`, `c3d-first-submit`).
ScenePasses registration
(`0x4` landscape BG, `0x40` FG, `0x20`
static, `0x100` PALSKIN, `0x2000` sky)
walks inside `00B25950` ← `00B27D90`.
Those DIPs are vtbl+**328**, not 332.

---

## 4. Host leftover Notes vs MATCH skip

### MATCH skip (keep)

| Host | Native | Class |
|---|---|---|
| `DisplayFlushShouldDip(0, 0)` | `[+16020]==[+16024]` | **MATCH** first-seen |
| Note `009DA9F0(1) [+16020] empty` (frontend ×2) | `je 009DB6E6` twice | **MATCH** skip |
| Note `009DA9F0(1) [+16020] empty dest` (game ×1) | `je 009DB6E6` once | **MATCH** skip |
| `Frontend2dDipIssued==false` | no vtbl+332 | **MATCH** |
| `SubmittedLayerBits` empty / `LayerFlushCount==0` | no `00B25950` in `00435530`; empty `+16020` | **MATCH** skip of invented bits |
| `DisplayFlushQueueIsNoteOnly==true` | first-seen no DIP | **MATCH** |
| `WorldSubmitted==false` on first Present | dummy region; no `006C2170` | **MATCH** |
| `PumpFrontendFrame` does not `SubmitCurrentWorld` | frontend is 2D + empty `00B27D90` | **MATCH** omit of world |
| `IssueFrontendFramePresent` no `DrawIndexedPrimitive` | sprites are DIPUP vtbl+336 | **MATCH** API family |

`FlushSubmittedLayers` is gated:

```10845:10855:src/Fable.Game/EngineLifecycle.cs
        var shouldDip = DisplayFlushShouldDip(0, 0);
        Note(..., "009DA9F0(1) [+16020] empty dest");
        ...
        if (shouldDip)
            FlushSubmittedLayers();
```

First-seen never enters. Tests lock empty
bits. **Do not** flip `(0, 0)` to force
layer Notes on dummy Present.

### LEFTOVER Notes / comments (do not close as MATCH)

| Host | Why leftover |
|---|---|
| `DisplaySubmitStages` name `"FlushLayers"` on `0x009DA9F0` | Native name is 2D `+16020` drain, not ScenePasses |
| Comment “Layer bits come from `ScenePasses.Registration`” on `DisplaySubmitStages` | Bits belong to `00B25950` |
| Comment “layer flush `009DA9F0(1)` DrawIndexedPrimitive vtbl+332” on overlay consts | vtbl+332 is the **nonempty** 2D tail. First-seen skips it. Not land/C3D |
| `ApplyDisplayCamera` “Layer bits are `ScenePasses` flushed by `009DA9F0`” | **DISPROVEN** pairing |
| `FlushSubmittedLayers` “`009DA9F0` draws the queued layer bits” | Would emit `bit 0x4` / `0x40` / `0x20` / `0x100` / `0x2000` **if** `shouldDip`. First-seen does not. Invented bits on nonempty host DIP would still be the wrong walker |
| `docs/status/README.md` row “`00435530` flushes ScenePasses via `009DA9F0(1)` … (`SubmittedLayerBits`)” marked PROVEN | The cited test **asserts empty** `SubmittedLayerBits`. Status over-claims the leftover comment as native |
| Host never stores `[this+16020]` | Empty Note is stand-in, not a queue read. Later HUD/`009DD8F0` producer **UNREAD** (`hud-first-present-skip`) |
| `DrawFrontendWidgets` comment “DIP is later `009DA9F0` empty skip” | **STALE** vs `IssueRecoveredDraws` (`009DA9F0-vs-0042DF9E`) |
| `PumpFrontendFrame` omits `0042E0BB` | Native always calls empty `00B27D90`. Omit is leftover vs listing; first-seen **no extra DIP** still MATCH |
| Client `_frontendReady` skips mesh/gizmos | Host gate. Native frontend also has no world DIP. Do not “fix” by submitting world |

`FlushSubmittedLayers` body (unread on
first-seen):

```10867:10879:src/Fable.Game/EngineLifecycle.cs
    private void FlushSubmittedLayers()
    {
        _submittedLayers.Clear();
        foreach (var pass in ScenePasses.Registration)
        {
            if (!ScenePasses.Draws(pass.Submit))
                continue;
            _submittedLayers.Add(pass.Bit);
            Note(DisplayFlushLayersFn, "GamePump", "Layer",
                $"bit 0x{pass.Bit:X} {pass.Submit}");
        }
        LayerFlushCount++;
    }
```

That walk is the leftover **ScenePasses**
pairing the status row treats as PROVEN.
It is **not** the listing of `009DA9F0`.
First-seen MATCH is that it **does not
run**.

---

## 5. Do not invent world submit on frontend Present

Native `0042DF9E` first-seen:

```
Clear / Begin
00595222 → 00BAE2D0 DIPUP / 00AB7C20 UP   ; 2D
0042E0BB 00B27D90                         ; empty lists; no cell / C3D DIP
009DA9F0(1) ×2 empty                      ; this skip
End / Present
```

Host `PumpFrontendFrame`:

```
Note 0042DF9E
Tick / Draw widgets
FlushFrontendDisplay ×2                   ; Note empty 009DA9F0
IssueFrontendFramePresent                 ; Clear / DIPUP / Present
```

No `SubmitCurrentWorld`. That call is
only in `PumpGameUpdate` after
`HeroSpawned`. Frontend stage
`PresentToHost` is skipped when
`Dx9OwnsFrontendPresent`. NativeSemantic
swapchain is the 2D batch.

**DISPROVEN** as first-seen frontend
Present:

- `SubmitCurrentWorld` / Concat land+C3D+sky
- `FlushSubmittedLayers` bits on `009DA9F0`
- nonempty `+16020` DIP vtbl+332 from widget dest
- Lookout `00BF4570` / `00BB2540`
- Oakvale house 6909 / `StartOakVale`
- `HeroSpawned` / `WorldSubmitted` gate on this Present

`c3d-first-submit`: frontend **does**
reach `00B27D90` (empty type `0x18`).
That is **not** `009DA9F0`. Host omit of
the empty walk must not be “fixed” by
pushing world meshes into
`IssueFrontendFramePresent` or into
`FlushFrontendDisplay`.

Leave teardown `0042EBB6` is
`009BE420` + `009BEEB0` only —
**DISPROVEN** as 3D / as `009DA9F0`.

---

## 6. Split that is easy to mix

```
009DA9F0(1)
  listing          +16020 2D drain; empty → 009DB6E6
  nonempty         vtbl+332 prim 2/4 VB +16008
  first-seen       SKIP  (frontend ×2, first game ×1)

00B27D90
  listing          engine vtbl+32 → 00B25950 ScenePasses
  first-seen FE    called, lists empty, no 3D DIP
  first game       00435530 does not call it

Host leftover
  DisplaySubmitStages "FlushLayers"
  FlushSubmittedLayers ScenePasses bits
  status row PROVEN bits 0x4 → 0x40 → …

Host MATCH
  DisplayFlushShouldDip(0,0)
  SubmittedLayerBits empty
  no SubmitCurrentWorld on 0042DF9E
```

`proofs/009DA9F0-vs-0042DF9E` answered
**which DIP** on frontend (recovered
`00BAE2D0`, not `+16020`). This file
answers the **remaining skip**: dummy
Present empty queue, leftover
ScenePasses label, do not invent world
on frontend Present.

---

## Classification

| Claim | Status |
|---|---|
| First-seen skip is `[+16020]==[+16024]` → `009DB6E6` | **PROVEN** / **MATCH** |
| First game Present is dummy / null region | **PROVEN** |
| First game `009DA9F0(1)` empty dest | **MATCH** |
| Frontend `009DA9F0(1)` twice empty | **PROVEN** sites / **MATCH** skip |
| `00435530` flushes ScenePasses via `009DA9F0` | **DISPROVEN** native; **LEFTOVER** host Notes / status |
| Nonempty DIP is vtbl+332 of `+16020` | **PROVEN** tail; first-seen **skip** |
| `009DA9F0` is the 3D layer walker | **DISPROVEN** |
| Invent world submit on `0042DF9E` | **DISPROVEN** |
| Host `DisplayFlushShouldDip(0,0)` is a live queue read | **DISPROVEN** (stand-in); skip **MATCH** |
| First nonempty `+16020` producer after New Game | **UNREAD** (`009DD8F0` gates) |
| Game caller of `012A0F3C+32` after Leave | **UNREAD** (`dx9-3d-submit`) |
| Dest 4-tuples / leftover #36 | **UNREAD** (out of scope) |

**Remaining leftover:** ScenePasses name on
`009DA9F0`, status row, `FlushSubmittedLayers`
pairing. Do not close it by submitting world
on frontend Present or by emitting layer bits
on dummy Present. First-seen empty skip stays
**MATCH**.
