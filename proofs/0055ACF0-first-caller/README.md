# First-seen caller of `0055ACF0` after frontend attach

Investigation only. No production `src/` edits.

Question: after frontend attach, which recovered site first
enters `0055ACF0` — `00557AF0` (`E8` at `00557AF4`) or the
type-35 tails `0055A726` / `0055A73B`? Which action / vtbl on
Press Start / New Profile / Main Menu type 11/38 reaches it?
Does type 4 / action 26 ever reach `0055ACF0`?

Authority: `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
(`0055ACF0`); `listing-00540000.txt` (`0055ACF0` / `00557AF0` /
`0055A640` / `0055A660` / `0055A9C0` / `0055AD60` / `0055AF60` /
`0055B040` / `00557850`); `listing-00580000.txt` (`00598A1C` /
`00598EE6`); `00-index/xrefs.tsv` / `rtti.txt`;
`proofs/0055B9D0-post-dword/README.md`;
`proofs/plus224-payloads/README.md`;
`proofs/00557AF0-caller/README.md`;
`proofs/type7-action35/README.md`;
`proofs/type6-action28/README.md`;
`proofs/action27-release/README.md`;
`implementer/frontend/17-press-start-frame.txt`;
`implementer/frontend/01-widget-construction.md`;
`FrontendUiDefTests` Accept / New Game / INVISIBLE.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**
/ **STALE**.

Do not re-prove `0055ACF0` `push [esi+380]` / `vtbl+524` =
`[def+228]` list, or action 26 `vtbl+584` / `0055AF60` posts
`[+372]` / `[def+224]`. Do not invent Lionhead names.

`plus224-payloads` “first-seen apply never takes `00557AF4` /
`0055A726` / `0055A73B`; attach **UNREAD**” is now **DISPROVEN**
as unread: attach is walked. Those three sites still do **not**
run on the first-seen trees.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| First-seen `.text` caller after attach | **Neither** recovered site | **PROVEN** |
| `00557AF0` vs `0055A726` / `0055A73B` | Type-39 wrap vs type-35 tails. **No** type 39 / 35 / 41 on Press Start / New Profile / Main Menu | **PROVEN** |
| Attach `00598EE6` / slot `0x14` `vtbl+284` | Type-10 `0054E4F0` (`+352` = `0xE5`). **Not** `0055ACF0` | **PROVEN** |
| Type 11/38 action **26** / outer `vtbl+584` | `0055AF60` (or `00557850` `jmp` / type-35 `0055A5D0` wrap). **Never** `0055ACF0` | **PROVEN** |
| Type 4 / action 26 ever reach `0055ACF0` | **No** | **PROVEN** |
| Type 11/38 action that *could* enter the **bare** body | Action **28** / `vtbl+588` if that dword **is** `0055ACF0` | **PARTIAL** (ABI; no rdata) |
| First-seen action 28 already takes that hop | **DISPROVEN** — ctor `[+364]=0` skips `+588`; type 38 enable has **no** 28 until after a 26 click | **PROVEN** skip; slot **PARTIAL** |

**Answer:** after frontend attach, `0055ACF0` is **not** entered
by `00557AF0` or by `0055A726` / `0055A73B`. Type 4 / action 26
on type 11/38 is `vtbl+584` / `0055AF60` and **does not** reach
it. First-seen `0xE5` / `0x126` / 15 therefore still do **not**
leave `+380` on that click.

---

## 1. Recovered entries into `0055ACF0`

`e8.tsv` dest `0x0055ACF0`: **one** row.

```
0x00557AF4	0x0055ACF0
```

`listing-00540000.txt` `jmp 0055ACF0`: **two** tails, both in
type-35 `0055A660`.

```
0055A726  jmp 0055ACF0          ; 13B8AD4 walk found no type 35/41
0055A73B  jmp 0055ACF0          ; input+184 == 0, or found type 35/41
```

No other `E8` / `jmp`. `e8.tsv` dest `0x00557AF0` and dest
`0x0055A660`: **empty**. Those two owners are **vtbl methods**.

Body (`ecx` = outer type-34-family widget, 0-arg `ret`):

```
0055ACF0  mov ecx, [esi+364]
          call [vtbl+192]            ; SelectState(stored +364)
          lea ecx, [esi+4]
          push 28
          call [inner.vtbl+16]       ; unsubscribe 28
          push [esi+380]
          call [vtbl+524]            ; post +228 list
          ret
```

Sibling `0055AF60` (action 26 click) is the inverse: SelectState
`[def+524]`, **subscribe** 28, post `[+372]` / `+224`. It never
loads `[+380]` (`0055B9D0-post-dword`, `0055AF60-callee`).

---

## 2. `00557AF4` is type 39 only

`00557AF0` (`00557AF0-caller`):

```
00557AF0  mov esi, ecx
00557AF4  call 0055ACF0
          ; if [0x13B8AC8]==0: capture, TEXT_GUI_PRESS_CONTROL,
          ; subscribe 33/26/27/35/38–42, input.vtbl+12(inner)
          ret
```

Owner ctor `00558540` (factory type **39**, size `0x1C0`):

```
call 0055B460
mov [esi],   0x124ADBC
mov [esi+4], 0x124AD98
```

RTTI `CKeyRedefiner@NUISystem`. `TEXT_GUI_PRESS_CONTROL` has
**one** `.text` pusher (`xrefs.tsv` `00557B19`).

Press Start dump types: 10, 5, 18, 0, 6, 12, 11, 32.
**No 39.** Prompt is `TEXT_GUI_MENU_PRESS_BUTTON`.

New Profile / Main Menu: type 10 root, type 12 list, type 38
Accept / type 11 New Game (`FrontendUiDefTests` Type **38** /
**11**). **No type 39.** `13B8AC8` stays 0.

So `00557AF4` is **not** the first-seen caller. It is not a
type 11/38 method.

---

## 3. `0055A726` / `0055A73B` are type 35 only

Factory type **35** `0055A9C0` (size `0x1AC`):

```
call 0055B460
mov [esi],   0x124BA94
mov [esi+4], 0x124BA70
; +404…+425 zero; +412 = 0
call 0055A890
```

Type **41** `00559830` calls `0055A9C0` then overrides to
`0124B7E4` (size `0x1DC`).

`0055A660` (0-arg; uses `+412` / `+416` / `+420` — type-35
fields; type 38 object is only `0x194` = 404, so this body
**cannot** be type 38):

```
if [this+412]:
    vtbl+524([+416]); input.vtbl+0(30); vtbl+524([+420])
ecx = [input+184]
if ecx == 0:  jmp 0055ACF0          ; 0055A73B
walk [0x13B8AD4]:
    widget.vtbl+260() == 35 or 41 → bl = 1
if bl == 0: [input+184].vtbl+604(); jmp 0055ACF0   ; 0055A726
else:       [input+184].vtbl+596(); jmp 0055ACF0   ; 0055A73B
```

`vtbl+260` `cmp eax, 35` at `0055A6ED` is the **widget type**
getter, not action 35 (`type7-action35`).

Press Start / New Profile / Main Menu trees have **no** type 35
or 41 (`17-press-start-frame.txt`; factory table in
`01-widget-construction.md`; tests lock Accept = 38, New Game /
INVISIBLE = 11). `0055A660` is never `this` on those screens.

Type 35’s **click** wrap is a different function:

```
0055A5D0  call 0055AF60              ; not 0055ACF0
          ; type-35 +412 latch
          jmp [input+184].vtbl+600
```

So even a live type 35 action 26 stays in `0055AF60`.

---

## 4. Attach does not call `0055ACF0`

`00598A1C` builds Press Start into UI slot `0x14`, then:

```
00598EE6  mov [packet], 0xE5
          call 0059B5D7              ; slot 0x14 widget
          call [widget.vtbl+284]     ; type-10 0054E4F0 → +352
```

(`press-start-e5-attach`). No `E8 0055ACF0`. Type-10 `+284` is
**not** the type-34 list poster. New Profile / Main Menu attaches
are **not** `00598EE6` and do not invent a type 35/39.

Ctor order on type 11/38 (`0054E0B0` / `00558B90` → `0055B460` →
`0055B040`) boxes `[def+228]` onto `+380` when nonzero. That is a
**store**, not a post. `0055ACF0` is the later **reader**.

---

## 5. Type 11/38: which action / vtbl

Shared apply `0055AD60` (`ecx` = inner = `widget+4`), table
`0x55AE88` (`action27-release`; do not use the stale
code-order map in `type6-action28` §6):

| Action | Event | Outer slot | Body |
| ---: | --- | --- | --- |
| **26** | type **4** LMB down | **`vtbl+584`** | `0055AF60` (post `+372`) |
| 27 | type 10 RMB down | `vtbl+592` | hover-in (`0055AFD0` ABI) |
| **28** | type **6** LMB up | **`vtbl+588`** | unarm; 0-arg |
| 29 | — | `vtbl+596` | unhover |

Action 26 (`0055AD7B`):

```
if [inner+348]==0: skip click
lea ecx, [esi-4]
call [outer.vtbl+584]          ; 0055AF60 / 00557850 jmp
[inner+364] = 1
call 0055B9D0                  ; nop for 26
```

No `E8` / `jmp` to `0055ACF0`. Type 39 click thunk on the same
family is `00557850` `jmp 0055AF60`, **not** `00557AF0`.

Type 11 enable `0054DC7E`: 26, 31, **28**, 27, 32, 29.
Type 38 / type 34 enable `0055AEB0`: 26, 31, 27, 32 — **no 28**.
`0055AF60` then `inner.vtbl+12(28)` only **after** a successful
26 click.

Action 28 (`0055ADDE`) is the only first-seen-family slot whose
0-arg callee has the same ABI as `0055ACF0` (inverse of `+584`).
Exact `01249554+588` / `0124B04C+588` / `0124BD2C+588` dwords
are **PARTIAL** (no `.rdata`). If they **are** `0055ACF0`, that
entry is **direct**, not via `00557AF4` or `0055A726`/`73B`.

Ctor `0055B460` zeros `+364`. First-seen action 28 therefore
takes `je 0055AE70` and **never** calls `+588`. A later LMB up
**after** a 26 that armed Accept / New Game / INVISIBLE is the
earliest type 11/38 path that *could* hit the bare body. That
is **not** type 4 / action 26, and it is **not** either named
site.

`type6-action28` “action 28 posts no `0x126` / 15” assumed
`+588` is not a list poster. If `+588` = `0055ACF0`, that
sentence is **STALE** for the `+228` list. Slot still
**PARTIAL**; do not treat first-seen LMB up as proven
`0x126` / 15.

---

## 6. Type 4 / action 26 never reaches `0055ACF0`

```
type 4 → 0042E3EE push 26 → 0055CB10
  type 10  0054E280 / 0054E2FA     ; +352 if nonzero; no 0055ACF0
  type 11  0054DBC0 → 0055AD60[0]  ; vtbl+584 → 0055AF60
  type 38  0055AD60[0]             ; same
  type 39  00557EB0 → 0055AD60     ; same click; 00557AF0 is 0-arg
  type 35  0055A510 → 0055AD60     ; click wrap is 0055A5D0 → 0055AF60
```

Broadcast first-seen (`action26-subscribers`): every subscribed
inner gets 26. None of those applies `E8`s `0055ACF0`.

Press Start `0xE5` is type-10 attach `+352` or type-11
`+228` sitting on `+380` **unread** by action 26.
Accept `0x126` / New Game 15 live on `+228` / `+380`.
Action 26 posts `+372` / `+224` = **0** on those defs
(`plus224-payloads`).

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0055ACF0` | post `[+380]` / unsub 28 | **PROVEN** body |
| `00557AF4` | sole `E8`; type-39 wrap | **PROVEN**; first-seen **DISPROVEN** |
| `0055A726` / `0055A73B` | type-35 `0055A660` tails | **PROVEN**; first-seen **DISPROVEN** |
| `0055A660` | type-35 0-arg; `+412` fields | **PROVEN** owner; slot **PARTIAL** |
| `0055A5D0` | type-35 click wrap of `0055AF60` | **PROVEN**; **DISPROVEN** as `0055ACF0` caller |
| `00557850` | `jmp 0055AF60` | **PROVEN** click thunk |
| `0055AD7B` | action 26 → `vtbl+584` | **PROVEN** |
| `0055ADDE` | action 28 → `vtbl+588` | **PROVEN** call; callee = `0055ACF0` **PARTIAL** |
| `00598EE6` | attach `0xE5` → type-10 `+284` | **PROVEN**; **DISPROVEN** as `0055ACF0` |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`00557AF0`, `00557850`, `00558540`, `00559830`, `0055A5D0`,
  `0055A660`, `0055A9C0`, `0055ACF0`, `0055AD60`, `0055AE88`,
  `0055AEB0`, `0055AF60`, `0055B040`, `0055B460`)
- `listing-00580000.txt` (`00598A1C`, `00598EE6`)
- `tools/Fable.ExeIndex/out/00-index/xrefs.tsv` /
  `rtti.txt` (`TEXT_GUI_PRESS_CONTROL`,
  `CKeyRedefiner@NUISystem`)
- `proofs/0055B9D0-post-dword/README.md`
- `proofs/plus224-payloads/README.md`
- `proofs/00557AF0-caller/README.md`
- `proofs/type7-action35/README.md`
- `proofs/type6-action28/README.md` (event 6 → 28 **PROVEN**;
  action-table rows **STALE** vs `action27-release`)
- `proofs/action27-release/README.md`
- `implementer/frontend/17-press-start-frame.txt`
- `implementer/frontend/01-widget-construction.md`
- `tests/Fable.Formats.Tests/FrontendUiDefTests.cs`
