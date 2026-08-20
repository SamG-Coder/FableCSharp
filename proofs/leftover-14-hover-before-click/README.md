# Leftover #14 — first-seen `0055BF10` on Accept / New Game before the click

Investigation only. Production `src/` and `tests/` were
not edited. Do **not** invent dest fill. Do **not**
plant `512,384`. Do **not** re-enable `Key.N` /
`ActivateNewGame`. Do **not** invent `TryChromeHit`
dest.

Question: first-seen `0055BF10` call on Accept /
New Game **before** the click. Who ticks hover so
type-11/38 `+352` is **1** when Type4 runs?

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055ACB0` / `0055AC90` / `0055B880` / `0055B890` /
`0055B8F0` / `0055BF10` / `0055C0DE` / `0055B9D0` /
`0055AD60` / `0054DB50` / `0054DC30` / `00558C70` /
`0055AEB0` / `0055C650`),
`listing-00500000.txt` (`0052D900` / `0052DA20` /
`0052C7E0` / `0052C730`),
`listing-00580000.txt` (`00599E3F` `0059A0C4`),
`listing-00400000.txt` (`0042EC7C` / `0042E3EE` /
`0042E5DC` / `0042DC94` `0042DD15`);
ExeIndex `vtbl 0x01249554 160`, `vtbl 0x0124B04C 160`,
`vtbl 0x01249530 8`, `vtbl 0x0124B024 8`;
`e8.tsv` (empty for `0055BF10` / `0055ACB0`);
siblings `proofs/leftover-14-dest-aabb`,
`proofs/leftover-14-dest-numbers`,
`proofs/leftover-14-present-dest`,
`proofs/type11-plus352-select`,
`proofs/leftover-48-native-hit`,
`proofs/00599E3F-walk-slots`,
`proofs/type4-current-inner-apply`,
`proofs/0052DA20-subscribe-25`;
`src/Fable.Game/FrontendInputMap.cs`
(`Type4InnerHoverTick` / `Leftover14OpenForDestPresentNotes`);
`src/Fable.Game/EngineLifecycle.cs`
(`TickType11Type38Hover`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH** / **STALE**.

Do not re-prove LMB type 4 / 6, Type4 current-inner
`0055CB10`, dest **formula**, dest **numbers**, or
`TryChromeHit`. Native dest 4-tuple stays **UNREAD**.

`FrontendInputMap.Leftover14OpenForDestPresentNotes = true`.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Who **calls** `0055BF10` on Accept / New Game before the click? | Frontend tick `0042DC94` → `00599E3F` `[ui+84] [node+20].vtbl+4(dt)`. Type 11 `01249554+4` and type 38 `0124B04C+4` are both **`0055ACB0`**. That jmp `0055B890` → `vtbl+580` **`0055BF10`**. No `.text` `E8` | **PROVEN** dispatcher; `0055B890` take **PARTIAL** |
| Same-frame Type4 vs that tick? | `0042E3EE` Type4 **before** `0042DC94`. Click-frame Type4 reads `+352` from a **prior** hover, or from same-harvest type 13 `0055B9D0` | **PROVEN** order |
| Does Type4 itself write `+352=1`? | **No.** `0055CB10(26)` **reads** `u8 [inner+348]`. Only store is `0055C0DE` | **DISPROVEN** |
| Ctor / attach write Accept / New Game `+352=1`? | **No.** Type-33 `0055BA20` writes **0**. Persist id is `+372` | **DISPROVEN** (`type11-plus352-select`) |
| Type 11 tick is `0054DB50` / `00558770`? | **No.** Those are not `vtbl+4`. `+172` = `0054DB50`. `vtbl+4` = `0055ACB0` | **DISPROVEN** / **STALE** vs `type38-on-off-first` |
| `inner.vtbl+8(25)` on Accept / New Game first-seen? | Inner `+8` is `0052D900` contains. Type 11 activate maps **26,31,28,27,32,29** (**no 25**). Type 38 enable maps **26,31,27,32** (**no 25**). Type 38 `vtbl+192` `00558C70` has a case `vtbl+12(25)` | **PROVEN** ids; first-seen **25** **UNREAD** |
| First-seen `0055C0DE` take on Accept / New Game before click? | Call site recovered. Take still needs contains(25) **and** `0055B8F0` AABB. Extra numbers / 25-map hit **UNREAD** | **UNREAD** take |
| Host `TickType11Type38Hover` is that store? | Notes `0055ACB0`. Writes `Hovered`, not `+352`. Type4 still `HitIndex` dest AABB | **LEFTOVER** |
| Close leftover #14? | **No.** Hover dispatcher MATCH; take / dest numbers / Type4 apply / Present Notes still open | **LEFTOVER** open |

---

## Verdict

**Hover so `+352` is 1 when Type4 runs is the
prior `00599E3F` `vtbl+4` tick (`0055ACB0` →
`0055B890` → `0055BF10`), not Type4 and not
ctor. Leave leftover #14 open.**

`0055BF10` has **no** `.text` `E8`. Dispatch is
`vtbl+580`. ExeIndex rdata:

| Type | Vtbl | `+4` tick | `+172` layout | `+580` hover |
| ---: | --- | --- | --- | --- |
| 11 New Game | `01249554` | **`0055ACB0`** | `0054DB50` | **`0055BF10`** |
| 38 Accept | `0124B04C` | **`0055ACB0`** | `0055AC90` | **`0055BF10`** |

`00599E3F` walks every `[ui+84]` slot and calls
that `vtbl+4(dt)`. After Press Start / after
`0xE5` New Profile / Main Menu, Accept and New
Game **are** on that walk. Every later frontend
frame ticks them **after** input.

Same `0042EC7C` frame:

```
0042E3EE  type 13 → 0055CB10(25) → 0055B9D0 → vtbl+580
          type  4 → 0055CB10(26)  reads +352
          type  6 → 0055CB10(28)
0042DC94  00599E3F vtbl+4          writes +352 for the *next* Type4
```

Click with no move uses the **previous** tick’s
`0055C0DE`. Move+click in one harvest can take
in the type-13 apply **before** Type4.

The **call** of `0055BF10` on those widgets
before the click is this tick (and optional
type 13). The **take** (`+352=1`) still needs
`inner.vtbl+8(25)` and dest AABB extra. Those
first-seen values stay **UNREAD**. Do **not**
plant dest. Do **not** invent `TryChromeHit`.

**Answer:** `00599E3F` → type 11/38 `vtbl+4`
`0055ACB0` → `0055B890` → `0055BF10`. Type4
only reads the byte.

---

## 1. Evidence — `0055ACB0` / `0055B890` / `0055BF10`

`listing-00540000.txt`.

Type-34-family **tick** (now rdata `vtbl+4` on
type 11 **and** 38):

```
0055ACB0  mov al, [ecx+352]
          xor dl, dl
          cmp al, dl
          jne 0055ACDC
          … if +352==0, clear +368 / +388 …
0055ACDC  jmp 0055B890
```

`0055B890` `ret 4` (dt):

```
0055B890  push dt
          mov  esi, ecx
          call 0052C7E0             ; style / layout tick
          fabs(+60 − +52) vs [0x129BA3C]
          jp   0055B8DD             ; has area → vtbl+580
          fabs(+64 − +56) vs eps
          jp   0055B8DD
          (+48 − dt) vs 0
          jp   skip                 ; point dest + dt changed
0055B8DD  call [edx+580]            ; 0055BF10
```

Has persist-target area → always `vtbl+580`.
Point dest (Accept / New Game persist size 0)
depends on `+48` vs dt. That take is **PARTIAL**.
Do **not** treat persist width as dest AABB
(`leftover-48-native-hit`: AABB is origin +
scale × `+176` extra).

`0055BF10` 0-arg `ret`:

```
0055BF19  call 0041E5F2
          test [input+164]; jne leave
          lea ecx, [esi+4]
          push 25
          call [inner.vtbl+8]       ; 0052D900 contains(25)
          je  leave
          ; if [input+184] type-32: vtbl+64 / +92 → [esp+32]
0055C00C  test [esi+352]
          jne already
          call [vtbl+568]           ; 0055B8F0 AABB of [esp+32]
          je  fail
          call 0055BB40             ; lose to 0x13B8AD4 peer
          …
0055C0DE  mov [esi+352], 0x01
          call [vtbl+532]           ; 0055BBF0
          insert self on 0x13B8AD4
```

`.text` `E8 0055BF10` empty. Pointer `input+184`
is the type-32 widget (`0055C650` `mov [eax+184], esi`).
Ctor of `UI_MOUSE_POINTER` writes it. AABB arg is
that pointer dest, **not** a planted `512,384`.

`0055B8F0` (`vtbl+568` rdata on 11/38): dest
**origin** `vtbl+488` + dest **scale** `vtbl+492`
× `vtbl+96` extra of `+176` children. Empty
`+176` → extra `0,0,0,0` → miss. Accept’s type-0
`UI_HELPER_BUTTON_MOUSE_AREA` leftover fills extra
(`leftover-48-native-hit`). New Game has type-0
`UI_BUTTON_MOUSE_AREA`. Extra **numbers** **UNREAD**
(do not invent dest).

---

## 2. Evidence — who ticks `vtbl+4`

`0042DC94` (`listing-00400000.txt`) ends
`call 00599E3F` (`0042DD15`). Same `0042EC7C`
as `0042E3EE` / `0042DF9E` (`0042F0AC` /
`0042F0B3`).

`00599E3F` `0059A0C4` (`00599E3F-walk-slots`):

```
[ui+84] sentinel → [head+8] leftmost
[node+20].vtbl+4(dt) if widget* ≠ 0
004292C0 successor until sentinel
```

No type filter. New Profile slot `0x17` and
Main Menu live on that walk with Press Start
`0x14`. Host `TickFrontendWidgets` Notes the
VA then layouts **one** tree (**LEFTOVER** list).

ExeIndex `vtbl` (this pass):

```
01249554+4   = 0055ACB0     ; type 11 New Game
0124B04C+4   = 0055ACB0     ; type 38 Accept
01249554+580 = 0055BF10
0124B04C+580 = 0055BF10
01249554+568 = 0055B8F0
0124B04C+568 = 0055B8F0
01249554+172 = 0054DB50     ; layout, not tick
0124B04C+172 = 0055AC90     ; call 0055B880 → jmp 0052C730
```

`type38-on-off-first` “type 11 tick `00558770`”
is **STALE**. `0054DB50` is `vtbl+172`: if
`[def+545]` then `0055AC90`, else `0052C730`.
Neither is `0055BF10`.

---

## 3. Evidence — Type4 reads `+352`; type 13 can write it earlier

`0055AD60` action 26 (`listing-00540000.txt`):

```
0055AD7B  mov al, [esi+348]      ; widget+352 u8
          test al, al
          je  0055AE3D           ; skip vtbl+584
          call [eax+584]
          [esi+364] = 1
```

No store. Type4 **cannot** arm Accept / New Game
until hover already took.

Action 25 (not in `26..32`) `ja 0055AE79` →
`0055B9D0`:

```
0055B9D0  cmp [esp+4], 25
          jne  ret 4
          add ecx, -4            ; outer
          call [eax+580]         ; 0055BF10
```

`0042E3EE` type 13 → `push 25` → `0055CB10`.
`0055CB10` still requires `inner.vtbl+8(25)`
before apply. Same contains gate as the tick
body. Type 13 in the harvest is FIFO **before**
a later type 4 (`type4-current-inner-apply`).
Click with no move does **not** produce type 13;
it needs the **prior** `00599E3F`.

Inner vtbl (this pass):

| Type | Inner | `+4` apply | `+8` accept | `+12` insert |
| ---: | --- | --- | --- | --- |
| 11 | `01249530` | `0054DBC0` | **`0052D900`** | `0052DA20` |
| 38 | `0124B024` | `0055AD60` | **`0052D900`** | `0052DA20` |

`0052D900`: if `[inner+16]≠0` return 0; else BST
contains. Empty set → **0**. `0055BF10` then
**leaves without `+352=1`**.

Local-map first-seen:

| Site | Widget | Ids |
| --- | --- | --- |
| `0054DC30` `vtbl+572` | type 11 activate | **26, 31, 28, 27, 32, 29** — **no 25** |
| `0055AEB0` `vtbl+572` | type 38 enable | **26, 31, 27, 32** — **no 25** |
| `00558C70` `vtbl+192` | type 38 SelectState | one case `push 0x19` / `jmp [inner+12]` = **25** |
| `0055C650` | type 32 mouse only | **25** on **that** inner, not Accept / New Game |

Ctor `0055BA20` maps **no** action ids. First-seen
`0054DC30` / `00558C70(25)` on New Game / Accept
stay **UNREAD** (`0054DC30-first-call`). Until 25
is on that inner’s set, every `0055BF10` **call**
still **returns** at `je leave`.

---

## 4. Original — before the click

```
ctor 0055BA20                 +352 = 0
attach / enable               +372 persist; +352 still 0
each 0042EC7C until LMB:
  0042E3EE  type 13? 0055B9D0 → 0055BF10   ; if contains(25)
  0042DC94  00599E3F
            Accept/NewGame.vtbl+4 = 0055ACB0
            0055B890 → 0055BF10
            0055C0DE +352=1 iff contains(25) && AABB
0042E3EE  type 4 0055CB10(26)
          reads +352; arms +364; does not write +352
```

Press Start Type4 posts type-10 `0xE5` **without**
this gate. New Profile Accept / Main Menu New Game
**need** the prior take.

---

## 5. Host

```
PumpInput
MaybeActivateNewGameFromInput   ; hover then Type4/Type6
TickFrontendWidgets             ; 00599E3F analog *after* Type4
```

`TickType11Type38Hover` Notes `0055ACB0 vtbl+580
0055BF10` then `Hovered = Contains || HitIndex`.
No `+352` u8. No `contains(25)`. Type4
`ArmType34Widgets` still `HitIndex` dest AABB
(`NativeType4UsesDestAabb = false` is the native
flag; host apply is still dest). `TryChromeHit`
invents type-16/37 size (**leftover #48**). Accept
does not use it (child dest walk). Do **not**
invent a dest 4-tuple to “fix” this tick.

`Type4InnerHoverTick = 0x0055ACB0` **MATCH**es
rdata `+4`. Host still does not store the byte.

---

## Gap

```
Evidence           Original                      Host                         Gap
0055ACB0 vtbl+4    00599E3F every slot           TickType11Type38Hover Note   MATCH dispatcher
0055BF10           vtbl+580; contains(25)+AABB   Hovered stand-in             take UNREAD; no +352
0055C0DE           only +352=1                   never written                leftover #14
0055CB10(26)       reads +352                    HitIndex dest AABB           leftover #14 apply
0055B8F0 extra     +176 child leftover           dest / TryChromeHit          #48; do not invent
type 13 0055B9D0   same 0055BF10 before Type4    no Queue TypeMouse           leftover producer
```

| Claim | Class |
| --- | --- |
| Type 11/38 `vtbl+4` = `0055ACB0` | **PROVEN** rdata |
| `00599E3F` is the tick that reaches that slot | **PROVEN** |
| `0055BF10` = `vtbl+580`; no `E8` | **PROVEN** |
| `0055C0DE` only `+352=1` | **PROVEN** |
| Type4 writes `+352` | **DISPROVEN** |
| Attach writes `+352=1` | **DISPROVEN** |
| Type 11/38 first-seen maps **25** | **UNREAD** |
| First-seen `0055B890` takes `vtbl+580` on point dest | **PARTIAL** |
| First-seen AABB extra numbers | **UNREAD** — do not invent dest |
| First-seen `+352=1` before the click | **UNREAD** take; **PROVEN** writer/site |
| Host `Hovered` / `HitIndex` is `+352` | **LEFTOVER** |
| `TryChromeHit` dest is native Accept hover | **DISPROVEN** |
| Close leftover #14 dest / Present Notes | **DISPROVEN** — stays open |

**Overall: PARTIAL** (dispatcher **PROVEN**; take
**UNREAD**). **Leave #14 open.**

**Proposed (do not apply here):** keep LMB
Type4/Type6. Do not restore `Key.N`. Point Type4
at `0055CB10` current inners. Keep dest AABB on
the **tick** that writes `+352` (`0055ACB0` →
`0055BF10` → `0055B8F0` origin + scale × extra).
Do not plant `512,384`. Do not invent
`TryChromeHit` dest for Accept. 25-map and AABB
extra still need a dump, not a dest fill.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00500000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00580000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\proofs\leftover-14-dest-aabb\README.md`
- `C:\FableCSharp\proofs\leftover-14-dest-numbers\README.md`
- `C:\FableCSharp\proofs\leftover-14-present-dest\README.md`
- `C:\FableCSharp\proofs\type11-plus352-select\README.md`
- `C:\FableCSharp\proofs\leftover-48-native-hit\README.md`
- `C:\FableCSharp\proofs\00599E3F-walk-slots\README.md`
- `C:\FableCSharp\proofs\type4-current-inner-apply\README.md`
- `C:\FableCSharp\src\Fable.Game\FrontendInputMap.cs`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
