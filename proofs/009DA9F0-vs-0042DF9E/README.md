# `009DA9F0` vs `0042DF9E` Present body

Investigation only. Production `src/` and `tests/` were not
edited.

Question: with `FrontendPresentBodyIsLive=true`, is the
frontend Present body a live **MATCH** for sprites, or only
Clear / Present wrapping an empty `+16020` DIP queue
(leftover #36)? Recover what `0042DF9E` first-seen actually
draws vs host `CompositeFrontendPresent`.

Authority: `Fable.exe`
`assembly/exe/01-sections/text-map/listing-00400000.txt`
(`0042DF9E`), `listing-009c0000.txt` (`009D9C80` /
`009DA9F0` / `009DB6E6` / `009DB700`);
`implementer/frontend/06-dx9-submit.md`, `15-submit.md`,
`fn-00BAE2D0-exact.txt`;
`src/Fable.Game/EngineLifecycle.cs`
(`FrontendPresentBodyIsLive`, `PumpFrontendFrame`,
`IssueFrontendFramePresent`, `FlushFrontendDisplay`,
`CompositeFrontendPresent`, `CollectFrontendRecords`);
`src/Fable.Game/FrontendDx9Submit.cs`
(`IssueRecoveredDraws`);
`src/Fable.Dx9/IDirect3DDevice9.cs`;
`tests/Fable.Formats.Tests/Dx9DeviceRecordTests.cs`;
`proofs/frontend-0042DF9E-status`,
`proofs/issue-36-verify`,
`proofs/0041AC20-dest-formula`,
`proofs/dx9-3d-submit`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Do not re-prove dest writers (`0041AC20` / `0052FFD0` /
`0052F5C0`). Do not treat recovered DIPUP as a live
`009DA9F0` queue read.

---

## Verdict

**Live Present body MATCH for the `00BAE2D0` sprite path.
Not a live `009DA9F0` DIP. Leftover #36 dest numbers stay
open. Empty `+16020` is native first-seen, not “no UI.”**

`0042DF9E` first-seen draws:

1. Clear `009D8CF0` (`0xFF000000`, flags 0 → 7)
2. BeginScene `009BEF20`
3. `[ui+84]` `00595222` vtbl+8. Type-0 packs `0x22` and
   dest vtbl+92 → `00BAE2D0` → `00A0AEA0` **DIPUP
   vtbl+336**. Type-6 packs `0x27` → `00AB7C20` /
   `00A0ABE0` **DrawPrimitive vtbl+324**. Zero dest is
   `00BADB36`, not enqueue.
4. `[retail+88].vtbl+32` `00B27D90` (empty landscape;
   no cell DIP)
5. `009D9C80` / `009DA9F0(1)` twice. `+16020` empty →
   `009DB6E6`. No type-`0x22` switch. No `009DB700`.
6. EndScene `009BEF50` / Present `009BEEB0`

Host with `Device` set: `IssueFrontendFramePresent` issues
Clear / BeginScene / recovered DIPUP+UP / EndScene /
Present. `FlushFrontendDisplay` always
`DisplayFlushShouldDip(0, 0)` → Note-only
`009DA9F0(1) [+16020] empty`. `Frontend2dDipIssued` stays
false. Sprites in `LastBatch` come from
`CollectFrontendRecords` dests (`00BAE2D0` analog), not
from `+16020`.

| Claim | Status |
| --- | --- |
| `FrontendPresentBodyIsLive` means device Clear / recovered DIPUP / Present | **PROVEN** |
| `009DA9F0(1)` ×2 is live `+16020` DIP | **DISPROVEN** (Note + empty skip) |
| First-seen native `009DA9F0` is `009DB6E6` | **MATCH** |
| Widget sprites drain via `009DA9F0` / type `0x22` switch | **DISPROVEN** |
| Widget sprites drain via `00BAE2D0` DIPUP vtbl+336 | **PROVEN** / host **MATCH** API |
| Host Present is only Clear / Present (no sprite draws) | **DISPROVEN** on install |
| No-install Present is Clear / Begin / SetViewport / End / Present | **PROVEN** (`Bootstrap(null)`) |
| `CompositeFrontendPresent` dest 4-tuples are native-dumped | **UNREAD** / **LEFTOVER** #36 |
| Type-6 `512,384,512,384` is a listing dest | **DISPROVEN** (host analog point) |
| Empty `+16020` means leftover #36 “no DIP at all” | **DISPROVEN** (wrong queue) |

**Overall: MATCH** for live Present wrapper + recovered
sprite DIPUP vs `00BAE2D0`. **MATCH** for empty
`009DA9F0`. **LEFTOVER** #36 for dest numbers. Do not
close #36.

---

## 1. Evidence — listing `0042DF9E`

`listing-00400000.txt` `0042DF9E`…`0042E179` `ret 4`.
Complete `E8` list (`functions.tsv`):

`00415A60, 009E1BC0, 00A0B560, 009BECE0, 009D8CF0,
009BEF20, 00595582, 00595222, 0041E5F2, 0041D03C,
009D9C80, 009DA9F0, 00404A80, 00404C00, 009D9C80,
009DA9F0, 009BEF50, 009BEEB0`.

One vtbl: `0042E0BB` `call [eax+32]` on `[esi+88]`
(`012A0F3C` engine = `00B27D90`).

```
0042E063  push ebx                  ; ebx=0 flags
0042E071  mov [ebp-1], 0xFF         ; colour 00 00 00 FF
0042E075  call 009D8CF0             ; Clear
0042E080  call 009BEF20             ; BeginScene
0042E08A  call 00595582             ; UI get [esi+88]
0042E091  call 00595222             ; [ui+84] vtbl+8
0042E0BB  call [eax+32]             ; 00B27D90
0042E129  call 009D9C80
0042E134  push 1
0042E136  call 009DA9F0
0042E13B  call 00404A80
0042E142  call 00404C00
0042E14D  call 009D9C80
0042E158  push 1
0042E15A  call 009DA9F0
0042E165  call 009BEF50             ; EndScene
0042E170  call 009BEEB0             ; Present
```

Viewport is **before** Clear: `fldz` origin, qword
`0x13961E8` / `0x13961F0` size, `00A0B560` then
`009BECE0`. Clear colour **PROVEN** `0xFF000000`.
Flush pair count **2** **PROVEN**.

No `E8 009DB700`. No `E8 00BAE2D0` (vtbl from the UI
walk). No `cmp …,0x22` in this body.

---

## 2. Evidence — listing `009DA9F0` / `009D9C80`

### `009DA9F0` (`listing-009c0000.txt`)

```
009DA9F0  sub esp, 104
009DA9F7  mov edx, [ebp+16020]
009DA9FD  mov ecx, [ebp+16024]
009DAA03  sub ecx, edx
009DAA05  mov eax, 0x88888889
009DAA0A  imul ecx                 ; count = bytes/60
009DAA42  je 009DB6E6              ; empty → skip DIP
```

Nonempty tail (`009DB33F` / `009DB370`):

```
call 00A058C0
mov eax, [device+88]
push 32                            ; stride
add edx, [ebp+16008]               ; VB
push esi                           ; count
push 2   or  push 4                ; prim
call [ecx+332]                     ; DrawIndexedPrimitive
```

Empty tail:

```
009DB6E6  lea ecx, [esp+60]
          call 009F9F70 ×2
009DB6FD  ret 4
009DB700  ; enqueue starts here, not a 0042DF9E callee
```

`009DB700` callers in this listing are later
(`009DC00E` / `009DD93D` only). Frontend widgets do not
call it (`06-dx9-submit.md`). First-seen frontend
`[this+16020]==[this+16024]` → `009DB6E6`. **PROVEN.**

vtbl+332 is **buffered** DIP of the display VB. Sprite
DIPUP is vtbl+336 (`00A0AEA0`). Mixing those is
**DISPROVEN**.

### `009D9C80`

Prologue is dirty-list + `[0x13BC800]` / `[0x13CB508]+10248`
bump (same family as `009DA9F0` after the count check).
Later DIP (`009DA45D`) uses VB **`+15960`**, not `+16020`.
No `cmp …,0x22` in `009D9C80–009DB000`. Type-`0x22` is
not this drain. First-seen frontend enqueue into this
queue is **UNREAD**; widgets still do not `009DB700`.

---

## 3. Original — what first-seen `0042DF9E` draws

Two 2D families, one empty 3D walk, one empty display
queue.

| Family | Insert | First-seen drain | DX9 |
| --- | --- | --- | --- |
| Type `0x22` sprite | `0041AFA0` / `0041BEB0` dest `+0x15C` `call [edx+92]` | `00BAD8A0`; draw `00BAE2D0` | DIPUP vtbl+336 prim **4** INDEX16 stride **32** indices `0,1,2,1,3,2` |
| Type `0x27` glyph | `0054EF00` / `00543910` size 64 | `00AB7C20` | DrawPrimitive vtbl+324 prim **4** 6×28 XYZRHW |
| Display `+16020` | `009DB700` only | `009DA9F0` | vtbl+332 prim 2 or 4. First-seen **skip** |
| Engine layers | `0042E0BB` `00B27D90` | `00B6B0B0` on empty `[0x1436E8C]+44` | no cell DIP |

Zero dest: `00BAD8A0` copies rec+12 → instance+72 then
`00BADB36 ret 8`. **No** `009DB700`. Native **ctor** dest
is `0,0,0,0`. Native **first Present** dest after
`0052C7E0` / `00531EC0` layout is **UNREAD** (no stack /
`+248` dump). Formula recovered in
`proofs/0041AC20-dest-formula`. Do not invent the
4-tuple.

`00595222` itself is **not** a DIP (**DISPROVEN** as DIP
in the ledger). It is the walk that **can** DIPUP.

---

## 4. Host — `PumpFrontendFrame` / `CompositeFrontendPresent`

```357:366:src/Fable.Game/EngineLifecycle.cs
    public const uint FrontendDrawFn = 0x0042DF9E;
    /// With Device attached,
    /// IssueFrontendFramePresent issues
    /// Clear / recovered DIPUP / Present.
    /// 009DA9F0(1) twice is still
    /// Note-only empty skip.
    public const bool FrontendPresentBodyIsLive = true;
    public const bool DisplayFlushQueueIsNoteOnly = true;
```

Frame still `Note`s `0042DF9E`, then walks, then issues:

```
PumpFrontendFrame
  Note 0042E3EE / 0042DC94 / 0042DF9E / 009D8CF0 / 009BEF20
  TickFrontendWidgets            // 00599E3F + 00531EC0 dest
  DrawFrontendWidgets            // 00595222 analog
    CollectFrontendRecords
    CompositeFrontendPresent     // store records; NativeSemantic drops FrontendBatch
  FlushFrontendDisplay ×2        // Note 009D9C80 / 009DA9F0 empty
  ApplyFrontendDisplay           // 00404C00 [+8]==0 skip
  IssueFrontendFramePresent      // live device
```

### Live Present body

```4350:4367:src/Fable.Game/EngineLifecycle.cs
    private void IssueFrontendFramePresent()
    {
        var device = Device;
        if (device is null)
            return;
        var frame = FrontendDx9Submit.FrontendFrame();
        device.Clear(Dx9Clear.WhenArgZero, frame.ClearColorArgb, 1f, 0);
        device.BeginScene();
        // BindFrontendTextures / SetViewport
        FrontendDx9Submit.IssueRecoveredDraws(device, _frontendDx9Records);
        device.EndScene();
        device.Present();
    }
```

`IssueRecoveredDraws` skips `DestX1<=DestX0` (`00BADB36`
analog). Sprites: `DrawIndexedPrimitiveUP` TriangleList,
4 verts, 2 prims, INDEX16 `0,1,2,1,3,2`, stride 32.
Glyphs: `DrawPrimitiveUP` 6×28. **No**
`DrawIndexedPrimitive` (vtbl+332).

Install + sprites/glyphs:

`Dx9DeviceRecordTests.Native_semantic_frontend_present_builds_device_batch`
locks `FrontendPresentBodyIsLive`,
`DisplayFlushQueueIsNoteOnly`, `FrontendBatch==null`,
`LastBatch` nonempty (`IndexCount==6` sprite and
`IndexCount==0 && VertexCount==6` glyph), host Present
unchanged.

Shadow install:
`Shadow_frontend_pump_records_nonempty_sprite_and_glyph_draws`
locks DIPUP between Begin/End, **no** buffered DIP,
`Frontend2dDipIssued==false`.

No-install `Bootstrap(null)`:
`First_frontend_present_is_clear_begin_end_present`
locks names
`Clear, BeginScene, SetViewport, EndScene, Present`
on the `0042DF9E` pump (after the after-AVI Clear/Present).
No DIPUP: no widget textures.

### Empty `009DA9F0` is hard-coded

```4627:4649:src/Fable.Game/EngineLifecycle.cs
    private void FlushFrontendDisplay()
    {
        // Notes 009D9C80 dirty-list / +10248
        var shouldDip = DisplayFlushShouldDip(0, 0);
        Note(..., shouldDip ? "... DIP vtbl+332"
                            : "009DA9F0(1) [+16020] empty");
        Frontend2dDipIssued = shouldDip;
    }
```

`begin=0,end=0` always. Host never stores `[this+16020]`.
That empty Note is a recovered skip, not a queue read.
`FrontendEnqueueRan` can still be true (nonempty dest
Note on `00BAE2D0`). That flag does **not** flip
`shouldDip` on HEAD.

Older `proofs/issue-36-verify` “host Notes DIP vtbl+332
when `FrontendEnqueueRan`” is **STALE** vs this
`FlushFrontendDisplay`. Ledger “leave #36 open” remains
for **dest**, not for a live `009DA9F0` claim.

### What `CompositeFrontendPresent` submits

`CollectFrontendRecords` walks `IsPresented` widgets:

- Sprite: `DestX1>DestX0` and a loaded texture → one
  `0x22` record. Host dest comes from
  `LayoutFrontendWidgets` / leftover-from-graphic
  (`GraphicId!=0` → frame size). Forest tiles dump as
  `0,0,410,410` (`+204=256`, `256*(1024/640)≈410`).
- Type-6: dest dump `512,384,512,384` (point) → sprite
  skip; glyphs from `Type6Pen` + `FrontendTextDraw.Layout`.
- `LeafDipSkipped` colour 0 still a host gate.

NativeSemantic: `FrontendBatch=null`; device Present
owns the swapchain. Shadow: `BuildBatch` **and**
`IssueRecoveredDraws`.

`export/frontend/press-start-dests.txt` is a **host**
dump (`batch=True`). Not a native `+248` / stack dump.

---

## 5. Gap

```
Native 0042DF9E device order
  00A0B560 viewport
  009D8CF0 Clear 0xFF000000
  009BEF20 BeginScene
  00595222 → 00BAE2D0 DIPUP / 00AB7C20 UP   // during walk
  00B27D90 empty
  009D9C80 / 009DA9F0(1) empty
  00404A80 / 00404C00 skip
  009D9C80 / 009DA9F0(1) empty
  009BEF50 / 009BEEB0

Host IssueFrontendFramePresent
  Clear 0xFF000000                          MATCH
  BeginScene                                MATCH
  SetViewport                               PARTIAL (after Begin, not 00A0B560 before Clear)
  IssueRecoveredDraws DIPUP/UP              MATCH API; dest LEFTOVER
  (no 00B27D90)                             GAP, first-seen empty → no extra DIP
  (009DA9F0 Note only)                      MATCH skip
  EndScene / Present                        MATCH
```

| Gap | Class |
| --- | --- |
| Dest 4-tuple vs native first Present | **UNREAD** / leftover #36 |
| `512,384,512,384` PRESS START text | **LEFTOVER** analog point; listing has no `512`/`384` immediates |
| Forest `0,0,410,410` | host analog of leftover `+204` × remap; native dump **UNREAD** |
| Viewport after Begin | **PARTIAL** vs `00A0B560` before Clear |
| `[retail+88].vtbl+32` | host skip. First-seen empty list **MATCH** no landscape DIP |
| `009D9C80` body (5331 insns) | host Note dirty-list only. Queue `+15960` first-seen **UNREAD** |
| DIPUP during walk vs after flush Notes | device order still Begin → DIPUP → End **MATCH** |
| Blend / RS / `VSHADER_2D_SPRITE` bind | recovered subset; rest **UNREAD** / TEMPORARY in `15-submit.md` |
| `DrawFrontendWidgets` comment “DIP is later `009DA9F0` empty skip” | **STALE** vs `IssueRecoveredDraws` (src not edited here) |

Leftover #36 stays open for dest-lock. It is **not** “Present
is Clear-only.” Empty `009DA9F0` is the native first-seen
queue. Sprites are the other family.

---

## 6. Split that is easy to mix

```
0042DF9E
  Clear / Begin                         live MATCH
  00595222
    00BAE2D0 DIPUP vtbl+336             live MATCH API (host IssueRecoveredDraws)
    dest numbers                        leftover #36 UNREAD
  00B27D90                              native empty; host omit
  009DA9F0(1) ×2 +16020                 Note MATCH empty skip
  End / Present                         live MATCH
```

`proofs/frontend-0042DF9E-status` answered “Note-only vs
live.” This file answers **which DIP**: recovered
`00BAE2D0`, not `009DA9F0`.
