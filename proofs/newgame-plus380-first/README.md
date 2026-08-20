# First `0055ACF0` on New Game after Main Menu attach

Investigation only. No production `src/` edits.

Question: first `0055ACF0` of `UI_FRONTEND_BUTTON_NEW_GAME`
(`+228` = **15**) after Main Menu attach. Same hop as Accept?
Type-12 highlight required?

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055ACF0` / `0055B040` / `0055B460` / `0055B5B0` / `0055AF60` /
`0055AD60` / `00558DE0` / `0054E0B0` / `0054DDB0` / `0054DC30` /
`0054DB50` / `0054C3A0` / `0054CBF0` / `0054D056` / `0054D660`),
`listing-00580000.txt` (`0059899A` / `00595A06` / `0059A238`);
`tools/Fable.ExeIndex/out/01-sections/newgame-trace/ui-frontend-main-menu-0059899a.md`;
inflated `frontend.bin` + `FrontendUiDefTests`;
`proofs/type11-msg15/README.md`;
`proofs/list-type12-focus/README.md`;
`proofs/type12-highlight-plus348/README.md`;
`proofs/plus224-payloads/README.md`;
`proofs/messageid-plus228/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE** / **LEFTOVER**.

Do not re-prove type 4 → action 26, CRC `0x53C644E4` = persist
`+228`, or `0059A238` consume of 15.

---

## Verdict

| Claim | Class |
| --- | --- |
| File `UI_FRONTEND_BUTTON_NEW_GAME` `+224` / `0x230364D6` is **0**; `+228` / `0x53C644E4` is **15** | **PROVEN** |
| Ctor `0054E0B0` → `0055B460` → `0055B040` boxes **15** onto the `+228` list | **PROVEN** |
| That list head is **`widget+380`** (`0055B5B0`); `+372` stays ctor **0** | **PROVEN** store body; `vtbl+320` dword **PARTIAL** |
| `0055ACF0` is `vtbl+192([+364])`, unsub 28, `push [+380]`, `vtbl+524` | **PROVEN** |
| Main Menu attach `0059899A` → `00595A06` calls `0055ACF0` | **DISPROVEN** |
| Type-11 tick `0054DB50` / activate `0054DC30` call `0055ACF0` | **DISPROVEN** |
| Type-12 highlight `0054D056` / nav `0054C59E` call `0055ACF0` | **DISPROVEN** |
| Action 26 `0055AF60` posts that **15** | **DISPROVEN** (posts empty `+372`) |
| `.text` `E8`/`jmp` of `0055ACF0` on this screen | **DISPROVEN** (only redefiner `00557AF4` + type-35 tails) |
| Type 11 has slim sibling `0054DDB0` (`push [+380]` / `vtbl+524`) | **PROVEN** body; `=01249554` slot **PARTIAL** |
| Accept uses the same `+228` → `+380` → `vtbl+524` / `00558DE0` hop | **PROVEN** ABI; wrapper **PARTIAL** |
| Type-12 highlight is required to fill or post `+380` | **DISPROVEN** fill; **DISPROVEN** as a recovered caller |
| First-seen Main Menu invoke of `0055ACF0` on New Game | **UNREAD** (no recovered caller) |

**Answer:** after attach the boxed **15** is already on
`NEW_GAME+380`. Attach does **not** run `0055ACF0`. The first
recovered `+380` post ABI is the same as Accept
(`vtbl+524` / `00558DE0` / `&node+8`). Type 11 may wrap that
post in `0054DDB0` instead of `0055ACF0` (no rdata). Type-12
highlight is **not** required: ctor wrote the list; list
`vtbl+192(3)` never calls this poster. First-seen apply of the
wrapper on this screen stays **UNREAD**.

---

## 1. File: New Game `+228` is 15, `+224` is 0

`00631C60` writes adjacent tail i32s (`messageid-plus228`):

| Def | CRC | NEW_GAME | ACCEPT |
| --- | --- | ---: | ---: |
| `+224` | `0x230364D6` | **0** | **0** |
| `+228` | `0x53C644E4` | **15** | **`0x126`** |

Tests: `FrontendUiDefTests` `newGame.Plus224==0`,
`newGame.MessageId==15`; same pair on Accept (`0` / `0x126`).

Hex (`plus224-payloads`):

```
D6640323 00000000   ; +224 = 0
E444C653 0F000000   ; +228 = 15
```

`type11-msg15` still calling persist 15 “`[def+224]`” is
**STALE** for this field (`messageid-plus228`).

---

## 2. Attach fills `+380`, does not post

Main Menu (`newgame-trace` / `0059899A`):

```
005989E1  push "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE"
005989F8  call 00595A06          ; factory + root vtbl+172
00598A0A  call 00595B24          ; label list (id 0, not msg 15)
```

Factory type 11 (`type11-msg15`):

```
0054E0B8  call 0055B460          ; type 34; zeros +364…+392
          mov [esi],   01249554
          mov [esi+4], 01249530
          call 0054DF50          ; +196 Action vector, not +228
```

`0055B040` during that ctor (type-34 vtbl `0124BD2C` still live):

```
test [def+224]; je 0055B15A      ; NEW_GAME 0 → skip vtbl+284 / +372
mov  eax, [def+228]              ; 15
test eax; je 0055B24B
box 15 (0042BE50 / 0042AA29)
call [vtbl+320]                  ; 0055B21F
```

`0055B5B0` is the only type-34 `ret 4` that writes **`+380`**.
Identity `0124BD2C+320 == 0055B5B0` stays **PARTIAL**. Host
`Plus228ListOffset=380` matches that body.

After `00595A06` returns:

| Slot | First-seen New Game |
| --- | --- |
| `+372` (`+224`) | **0** (skipped) |
| `+380` (`+228`) | list head, boxed **15** |
| `+352` (click u8) | **0** (`type11-plus352-select`) |

Root layout `vtbl+172` → type 10 `0052C730` → type 12
`0054D660` (`list-type12-focus` / `type12-highlight-plus348`).
That walk does **not** `E8 0055ACF0` and does **not** call
child `vtbl+192(3)`.

---

## 3. Dump `0055ACF0` — `+380` poster, not attach

```
0055ACF0  push esi
          mov esi, ecx
          mov ecx, [esi+364]
          call [vtbl+192]            ; SelectState(+364)
          lea ecx, [esi+4]
          push 28
          call [inner+16]            ; unsubscribe 28
          push [esi+380]
          call [vtbl+524]            ; 00558DE0
          ret
```

Sibling `0055AF60` (action 26 `vtbl+584`) is the same shape on
**`+372`** and **subscribes** 28. Action 26 therefore still
posts the empty `+224` list on this widget
(`plus224-payloads`).

`00558DE0`: `test edi,edi; je ret`. Non-null `+380` walks
`&node+8` → input `vtbl+56` `0041E6D3` → frontend UI
`vtbl+32` `0059A238`. Boxed dword0 is the 15 that
`0059A238` switches on.

`.text` callers of `0055ACF0` in `listing-00540000.txt`:

| Site | Object |
| --- | --- |
| `00557AF4` | key redefiner (`TEXT_GUI_PRESS_CONTROL`) |
| `0055A726` / `0055A73B` | type-35 tails (`vtbl+260` cmp 35/41) |

None of those widgets exist on first-seen Main Menu
(`type7-action35`). Dispatch onto type 11/38 is a **vtbl**
call. Slot id **PARTIAL** (no `01249554` / `0124B04C` dump).

---

## 4. Type 11 slim clone — not the same wrapper as Accept

Type-11 cluster next to apply/activate:

| Fn | If `def+545` | Posts |
| --- | --- | --- |
| `0054DD50` | `push [esi+372]; vtbl+524` | `+224` (empty here) |
| **`0054DDB0`** | `push [esi+380]; vtbl+524` | **`+228` / 15** |
| `0054DE10` | `push [esi+392]; vtbl+524` | sibling list |

`0054DDB0` is the **post half** of `0055ACF0` without
`vtbl+192(+364)` / unsub 28.

Type 38 (`00558B90`) overwrites vtbl to `0124B04C` and has
**no** `0054DDB0` clone. Accept’s recovered `+380` wrapper is
therefore type-34 **`0055ACF0`**. New Game may keep that slot
or replace it with `0054DDB0`. Same walker, same boxed 15.

**Same hop as Accept?** The persist → `+380` → `vtbl+524` →
`00558DE0` → `0059A238`(15 / `0x126`) hop **yes**. The 0-arg
wrapper **not proven identical**.

---

## 5. Type-12 highlight is not this post

Tree (`list-type12-focus`, `type11-msg15`):

```
UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE     type 10
└── UI_FRONTEND_LIST_MAIN_MENU_…                   type 12
    └── [0] UI_FRONTEND_BUTTON_NEW_GAME            type 11  +228=15
        … LOAD / OPTIONS siblings
```

List `+348` is **0** at ctor `0053B662` and again at attach
`0054D6F1`. That names `+356[0]` = persist child 0 = New Game
**as an index**, not as a UI message.

Highlight `vtbl+192(3)` `0054D056`:

```
every +356 inner:  input.vtbl+8; inner+12(25)
+356[+348] inner:  +12(22) then +12(4)
0052CF40(3)
```

Inner `+12` is local-map insert (`0052DA20`), not
`0055ACF0`. Actions 25 / 22 / 4 are outside type-11
`0055AD60` (`26…32`). Nav `0054C59E` / `0054C95E` only
calls child `vtbl+192(4/3)`.

Type-11 activate `0054DC30` (`vtbl+192(3)` then local
26/31/28/27/32/29) is **not** on the attach layout path
(`type12-highlight-plus348` §7). Even if it ran, it would
not call `0055ACF0`.

Tick `0054DB50`: `def+545` → `0055AC90`; else `0052C730`.
Neither posts `+380`.

**Highlight required?** **No** for the boxed 15 (ctor).
**No** as a recovered trigger of `0055ACF0` / `0054DDB0`.
List index 0 is already the first-seen row; that is a
different slot from the type-11 click gate `+352`.

---

## 6. C# leftover (do not apply here)

`FrontendInputMap.Plus228PostFn = 0055ACF0`,
`Plus228ListOffset = 380`,
`MessageFromPlus228List` returns the first visible type
11/38 `MessageId` on action **26**.

Native action 26 on this widget posts **`+372`**, which is
empty. The host therefore delivers 15 on the **wrong**
action relative to `0055AF60`, even though the integer
matches `+228`. `plus380-poster` already records that
mapper. Do not treat attach or list `+348` as a C# gate.

---

## 7. UNREAD

- `01249554+?` dword for `0055ACF0` vs `0054DDB0`.
- `0124B04C` still `0055ACF0` for Accept (body recovered;
  slot **PARTIAL**).
- First caller of either wrapper after `00595A06` on
  Main Menu (pointer tick / later `vtbl+192(3)` / other
  action).
- `def+545` first-seen on New Game vs the Main Menu list.
- Lionhead name for `0x53C644E4`.
