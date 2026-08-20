# Leftover #14 remaining — dest AABB / Present Notes

Investigation only. No `src/` or `tests/` edits.
Do **not** re-enable `Key.N` / `ActivateNewGame`.

Question: what still makes leftover #14 open after native
LMB **MATCH** (type 4 / 6)? Recover Type4 **current-inner
apply** vs hover **dest AABB**. Dump `0042DF9E` Present
body and `00595222` walk.

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0042DF9E` / `0042E3EE` / `0042E085`),
`listing-00540000.txt`
(`0055CB10` / `0055AD60` / `0055AF60` / `0055ACB0` /
`0055B890` / `0055BF10` / `0055B8F0` / `0054E2FA`),
`listing-00580000.txt` (`00595222` / `00595582`);
`proofs/leftover-14-native-key/README.md`;
`proofs/frontend-0042DF9E-status/README.md`;
`proofs/00595222-first-node/README.md`;
`proofs/type11-plus352-select/README.md`;
`proofs/action26-subscribers/README.md`;
`proofs/leftover-48-dest/README.md`;
`src/Fable.Game/EngineLifecycle.cs`
(`PumpFrontendFrame`, `IssueFrontendFramePresent`,
`FlushFrontendDisplay`, `MaybeActivateNewGameFromInput`,
`ArmType34Widgets`, `TickType11Type38Hover`);
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Game/FrontendHitTest.cs`;
`src/Fable.Game/FrontendDx9Submit.cs`;
`src/Fable.Game/FrontendLayout.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Do not re-prove LMB type 4 / 6 → actions 26 / 28, persist
CRC `0x53C644E4` → `0x126` / 15, or Leave `0042F2A2`.
Do **not** invent dest numbers (including `512,384`).

---

## Verdict

**LMB MATCH does not close leftover #14.** Native poster
of Press Start `0xE5` / New Profile `0x126` / New Game 15
is already **MATCH** (`leftover-14-native-key`). Remaining
#14 is dest / Present Notes:

| Slice | Native | Host | Class |
| --- | --- | --- | --- |
| LMB type 4 / 6 translator | `00A03C80` / `00A03D60` | `SilkNativeInput` queues Type4 / Type6 | **MATCH** |
| Type4 **apply** | `0055CB10(26)` on **current inners** (`widget+4`) | dest AABB `HitIndex` / `Hovered` then first visible type-10 / first armed 11/38 | **LEFTOVER** |
| Hover dest AABB | tick `0055BF10` → `vtbl+568` `0055B8F0` writes type-11/38 `+352` u8 | `Contains \|\| HitIndex`; `TryChromeHit` invents point-dest hit | **LEFTOVER** vs current-inner |
| `0042DF9E` Present body | Clear / BeginScene / `00595222` / `009DA9F0(1)×2` / EndScene / Present | `IssueFrontendFramePresent` live when `Device` set; `FlushFrontendDisplay` **Note-only** | **PARTIAL** |
| `00595222` | `[ui+84]` `[node+20].vtbl+8` walk, not a DIP | `Note` + `DrawFrontendWidgets` `ResidentSlotTrees` | **MATCH** walk shape; **LEFTOVER** dest skip |

`FrontendInputMap.Leftover14OpenForDestPresentNotes = true`.
`FrontendPresentBodyIsLive = true`.
`DisplayFlushQueueIsNoteOnly = true`.

**Answer:** leftover #14 stays open because Type4 still
applies through **host dest AABB hover**, not the recovered
**current-inner** `0055CB10` site, and because `0042DF9E`
still **Notes** `009DA9F0` (empty `+16020`) while
`IssueRecoveredDraws` skips dests with no area. Closing
the LMB translator does not dest-lock hover or flush the
native display queue.

---

## 1. Evidence — `0042DF9E` Present body

`listing-00400000.txt`:

```
0042DF9E  push ebp
          mov esi, ecx
0042E075  call 009D8CF0          ; Clear (flags 0 → 7, 0xFF000000)
0042E07A  mov ecx, [0x13B8390]
0042E080  call 009BEF20          ; BeginScene
0042E085  mov eax, [esi+88]
          push ebx               ; ebx = 0
          push eax
          call 00595582          ; UI*
          mov ecx, eax
0042E091  call 00595222          ; [ui+84] vtbl+8 walk, ret 8
0042E096  call 0041E5F2
          cmp [eax+156], bl
          je  0042E0B2
          … 0041D03C …
0042E0B2  mov ecx, [esi+88]
          mov eax, [ecx]
          push ebx
          push [ebp+8]
          call [eax+32]          ; engine vtbl+32
0042E129  call 009D9C80
          push 1
          call 009DA9F0          ; flush +16020
          call 00404A80
          call 00404C00
          call 009D9C80
          push 1
          call 009DA9F0          ; second flush
0042E165  call 009BEF50          ; EndScene
0042E170  call 009BEEB0          ; Present
          ret 4
```

`00595222` (`listing-00580000.txt`) is the walk only:

```
00595222  mov ebx, ecx
          mov eax, [ebx+84]      ; sentinel
          mov esi, [eax+8]       ; leftmost
          cmp esi, eax
          je  empty
00595230  mov ecx, [esi+20]      ; widget*
          test ecx, ecx
          je  next               ; null value skip
          mov eax, [ecx]
          push 0, 0, 0, arg, arg
          call [eax+8]           ; vtbl+8
0059524A  call 004292C0          ; in-order successor
          cmp esi, [ebx+84]
          jne 00595230
          ret 8
```

No dest AABB. No type filter. No `[ui+32]` current.
Null `[node+20]` is the only skip. First-seen nonempty
draw is slot `0x1`, Press Start lives at slot `0x14`
(`00595222-first-node`). DIP is later `009DA9F0`, not
this walk.

`009DA9F0(1)` twice drains `[this+16020, +16024)`.
First-seen empty → `009DB6E6` skip. Widget dest submit
is `00BAE2D0` / `00AB7C20` (DIPUP / PrimitiveUP), not
buffered `DrawIndexedPrimitive` of `+16020`. Empty dest
is `00BADB36` skip.

---

## 2. Evidence — Type4 current-inner apply vs dest AABB hover

### Current-inner apply (Type4 / action 26)

`0042E3EE` type 4 → `push 26` → `call [edx]` =
`0055CB10` (`input.vtbl+0`). Listener object is
**`widget+4`** (inner).

```
0055CB10  if [this+8] != 0:           ; focused inner*
              if inner.vtbl+8(26):    ; accept
                  inner.vtbl+4(26)    ; apply
              return
          else broadcast [this+12] else [this+4]
              listener = [node+8]
              if inner.vtbl+8(26): inner.vtbl+4(26)
```

| Type | Inner apply (`vtbl+4`) | Action 26 |
| ---: | --- | --- |
| 10 | `0054E280` / `0054E2FA` | if `[inner+348]` packet* ≠ 0, UI `vtbl+32` `&widget+352` (`0xE5` attach) |
| 11 | `0054DBC0` → `0055AD60` | parent `+545` then same switch |
| 38 | `0055AD60` | `mov al,[esi+348]` (`widget+352` **u8**). 0 → skip `vtbl+584`. Else `0055AF60`, `[inner+364]=1` |

```
0055AD7B  mov al, [esi+348]      ; inner this: widget+352 u8
          test al, al
          je  0055AE3D           ; 0055B9D0 only
          lea ecx, [esi-4]
          call [eax+584]         ; 0055AF60
          mov [esi+364], 1       ; arm
          call 0055B9D0
```

Type4 **does not** load dest origin / dest size.
Press Start `0xE5` is type-10 `+352` packet*, posted
if that inner is on the `0055CB10` list. Type 11/38
persist id is **not** posted on 26; 26 only arms
`+364` after the selected u8 is 1. Type 6 / action 28
then `vtbl+588` / `0055ACF0` posts `+380` / `[def+228]`.

Type-10 first-seen as a `0055CB10` node is **UNREAD**
(ctor does not `input.vtbl+8`). Type 11/38 **PROVEN**
registered (`0055BA20`).

### Hover dest AABB (tick, not Type4)

Type-34 tick `0055ACB0` (`vtbl+4` on the **outer**):

```
0055ACB0  mov al, [ecx+352]      ; selected u8
          … clear +368 / +388 if 0 …
          jmp 0055B890
```

`0055B890` (dt): `0052C7E0` then, when dest width/height
are ~0 **or** dt changed, `call [vtbl+580]` =
`0055BF10`.

```
0055BF10  call 0041E5F2
          test [input+164]; jne leave
          inner.vtbl+8(25); je leave
          ; if [input+184] type-32 pointer: vtbl+64 / +92 → point
0055C00C  mov al, [esi+352]
          test al, al
          jne already
          call [vtbl+568]        ; 0055B8F0 dest AABB
          je  fail
          call 0055BB40          ; lose to a higher 0x13B8AD4 widget
          …
0055C0DE  mov [esi+352], 0x01    ; only recovered +352=1 store
```

`0055B8F0` AABB (`listing-00540000.txt`):

```
0055B8F0  call [vtbl+488]        ; dest origin
          call [vtbl+492]        ; dest size
          call [vtbl+96]         ; extra
          left  = origin.x + extra.x
          top   = origin.y + extra.y
          right = origin.x + size.x * extra
          bot   = origin.y + size.y * extra
          contains: x in [left, right), y in [top, bot)
```

Point dest (size 0) → empty AABB → `al=0` → `+352`
stays ctor 0 → Type4 current-inner `0055AD60` skips
`0055AF60`. Hover **writes the gate**; Type4 **reads
it**. They are not the same site.

`0055BF10` is 0-arg, vtbl dispatch (`e8.tsv` empty).
It is **not** TypeMouse-only: pointer `input+184` is
optional; the AABB test uses dest of `this`. First-seen
call on Accept / New Game before the click is **UNREAD**.

Do **not** invent dest 4-tuples. Native dest size is
`0041AFA0` persist `+360` else leftover `+204`. GraphicIndex
0 → leftover 0 → dest is a **point**. Point dest has no
`0055B8F0` area.

---

## 3. Original (native order, frontend frame)

```
0042EC7C
  0042E3EE  type 4 → 0055CB10(26)     current-inner apply
            type 6 → 0055CB10(28)     armed +380 post
  0042DC94 / 00599E3F
            [ui+84] vtbl+4 tick
            type 11/38: 0055ACB0 → 0055B890 → 0055BF10 dest AABB
            +352 u8 then next Type4 can arm
  0042DF9E
            009D8CF0 / 009BEF20
            00595582 / 00595222 [ui+84] vtbl+8
            009D9C80 / 009DA9F0(1)
            00404A80 / 00404C00
            009D9C80 / 009DA9F0(1)
            009BEF50 / 009BEEB0
```

Type4 on Press Start: type-10 `0054E2FA` posts attach
`0xE5` **without** dest AABB. Type4 on New Profile /
Main Menu: only the **current inner** whose `+352` u8
is already 1 arms; dest AABB ran on a **prior tick**.

---

## 4. Host

`PumpFrontendFrame` still `Note`s every recovered VA,
then:

```
PumpInput
MaybeActivateNewGameFromInput     ; hover + Type4/Type6
TickFrontendWidgets               ; 00599E3F analog
DrawFrontendWidgets               ; 00595222 analog
FlushFrontendDisplay ×2           ; 009DA9F0 Note-only
IssueFrontendFramePresent         ; live when Device set
```

### Present Notes

```358:366:src/Fable.Game/EngineLifecycle.cs
    public const uint FrontendDrawFn = 0x0042DF9E;
    /// With Device attached,
    /// IssueFrontendFramePresent issues
    /// Clear / recovered DIPUP / Present.
    /// 009DA9F0(1) twice is still
    /// Note-only empty skip.
    public const bool FrontendPresentBodyIsLive = true;
    public const bool DisplayFlushQueueIsNoteOnly = true;
```

`IssueFrontendFramePresent`: if `Device` is null, **return**.
Else Clear / BeginScene / viewport / `IssueRecoveredDraws` /
EndScene / Present. Tests lock both flags
(`Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present`,
`Native_semantic_frontend_present_builds_device_batch`).

`FlushFrontendDisplay` always `DisplayFlushShouldDip(0, 0)`
→ false. `Frontend2dDipIssued` stays false. That is the
**Note-only** half of leftover #14 / #36.

`IssueRecoveredDraws` skips `DestX1 <= DestX0 || DestY1 <= DestY0`
(`00BADB36`). Point dests produce **no** DIPUP even when the
Present body is live.

`DrawFrontendWidgets` Notes `00595222` then walks
`ResidentSlotTrees`. Walk shape **MATCH**. Dest/DIP of
inactive slots **UNREAD**.

### Type4 dest AABB hover (not current-inner)

```3950:3978:src/Fable.Game/EngineLifecycle.cs
    TickType11Type38Hover(_frontendWidgets);
    … ActionType4 → ArmType34Widgets()
       MessageFromWidgets(act, _frontendWidgets)
    … ActionType6 → MessageFromPlus228List
       UnarmType34Widgets
```

`TickType11Type38Hover`: type 11/38 `Hovered = Contains || HitIndex == i`.
Comment claims `0055ACB0` / `vtbl+580` `0055BF10`. Host never
writes a `+352` u8. `Hovered` is the stand-in.

`ArmType34Widgets`: `HitIndex(_frontendWidgets, PointerX, PointerY)`
then require `Hovered` before `Armed = true`. **Not**
`0055CB10` inner accept/apply.

`MessageFromType10Attach`: first visible type-10 `Type10Packet`
(any Type4, **no dest**). Native type-10 apply is inner
`0054E2FA` if that inner is on the listener list.

`MessageFromPlus228List`: first armed type 11/38 `MessageId`.
Native is the **same widget** that action 26 armed, via
inner `vtbl+588`, not a host list scan.

`FrontendHitTest.HitRect` prefers pre-assigned `Hit*` when
it has area. `AssignHitRects` / `TryChromeHit` invents
type-16/37 hit from rightmost type-2 table dest
(`TryChromeHitIsNativeHit = false`). That is leftover **#48**
size invent feeding leftover **#14** hover.

Tests: `ClickNamed` uses `TryDestPoint` midpoint + TypeMouse
then Type4/Type6. `Main_Menu_Type4_Type6_posts_15_from_current_pointer_without_TypeMouse`
sets the pointer to that midpoint. Empty space `(12,12)`
does not Accept. Click `(700,300)` is a host lock, **not**
a recovered dest tuple. Do not treat those as native dest.

Client LMB still queues Type4/Type6 (`SilkNativeInput`).
Enter stays TypeKey. No `Key.N`.

---

## 5. Gap (why #14 stays open)

```
Evidence          Original                         Host                          Gap
0042DF9E body     Clear, BeginScene, 00595222,     IssueFrontendFramePresent     Device==null still Note.
                  009DA9F0×2, EndScene, Present    live DIPUP when Device set    009DA9F0 still Note
                                                                                 (DisplayFlushQueueIsNoteOnly).
00595222          all-slot [ui+84] vtbl+8          ResidentSlotTrees + Note      MATCH walk. Dest/DIP of
                                                                                 inactive slots UNREAD.
IssueRecovered    00BAE2D0 nonempty dest;          skip DestX1<=DestX0           Point dest → no submit.
                  00BADB36 empty
Type4 apply       0055CB10 current inner vtbl+4    HitIndex dest AABB + first    Host apply is hover dest,
                  (type-10 +352 packet /           visible type-10 / first       not current-inner.
                  type-11/38 +352 u8 then arm)     armed 11/38
Hover AABB        0055BF10 tick, 0055B8F0 size     Contains||HitIndex;           TryChromeHit invents
                  0 → no +352=1                    Hovered stand-in for +352     point-dest hit (#48).
                                                                                 Dest 4-tuples UNREAD.
LMB translator    type 4/6 device 3                Queue Type4/Type6             MATCH. Not this leftover.
Key.N / Enter     no native New Game poster        absent / TypeKey skip         DISPROVEN cheat. Do not restore.
```

| Claim | Class |
| --- | --- |
| Native LMB type 4 / 6 posts `0xE5` / `0x126` / 15 | **MATCH** (`leftover-14-native-key`) |
| Leftover #14 remaining is dest / Present Notes | **PROVEN** (`Leftover14OpenForDestPresentNotes`) |
| `0042DF9E` is only a `Note` when `Device` is set | **DISPROVEN** (`FrontendPresentBodyIsLive`) |
| `009DA9F0(1)×2` is live DIP of `+16020` | **DISPROVEN** (`DisplayFlushQueueIsNoteOnly`) |
| `00595222` is a DIP | **DISPROVEN** (walk only) |
| Type4 apply is dest AABB `0055B8F0` | **DISPROVEN** — apply is `0055CB10` inner |
| Dest AABB `0055BF10` / `0055B8F0` writes type-11/38 `+352` u8 | **PROVEN** store; first-seen call **UNREAD** |
| Host `Hovered` / `HitIndex` is that store | **LEFTOVER** stand-in |
| Host `TryChromeHit` is `0055B8F0` | **DISPROVEN** (`leftover-48-dest`) |
| Type4 current-inner list first-seen includes type 10 | **UNREAD** |
| Dest 4-tuple dump (`512,384` or any invented pair) | **UNREAD** — do not invent |
| `Key.N` / Enter as New Game | **DISPROVEN** |

**Proposed (do not apply here):** keep LMB Type4/Type6.
Do not restore `Key.N`. Point Type4/Type6 at `0055CB10`
current inners; keep dest AABB on the **tick** that writes
the selected u8. Do not invent dest size for `0055B8F0`.
Keep `0042DF9E` Notes as a trace; leftover Present is the
empty `009DA9F0` queue and dest-area skip, not a missing
LMB device.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00580000.txt`
- `C:\FableCSharp\proofs\leftover-14-native-key\README.md`
- `C:\FableCSharp\proofs\frontend-0042DF9E-status\README.md`
- `C:\FableCSharp\proofs\00595222-first-node\README.md`
- `C:\FableCSharp\proofs\type11-plus352-select\README.md`
- `C:\FableCSharp\proofs\action26-subscribers\README.md`
- `C:\FableCSharp\proofs\leftover-48-dest\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendInputMap.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendHitTest.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendDx9Submit.cs`
