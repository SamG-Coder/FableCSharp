# Who calls `00557AF0` (after `0055ACF0` posts `+380` / `+228`)

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listing
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`00557AF0` / `00557BD0` / `00557EB0` / `00558540` / `0055ACF0` /
`0055AD60` / `0055AF60` / `0055AFD0` / `0055B040` / `0055B5B0`);
`e8.tsv`; `00-index/xrefs.tsv` / `rtti.txt` / `strings.tsv`;
`proofs/0055B9D0-post-dword/README.md`;
`proofs/plus224-payloads/README.md`;
`proofs/action27-release/README.md`;
`proofs/type7-action35/README.md`;
`implementer/frontend/17-press-start-frame.txt`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove `0055ACF0` `push [esi+380]` / `vtbl+524` or
`0055B040` `[def+228]` → `vtbl+320` → `widget+380`.
Do not re-prove type 4 → action 26 / `0055AF60` posts `+372`.

---

## Verdict

**No `.text` `E8` calls `00557AF0`.** `e8.tsv` has zero rows with
dest `0x00557AF0`. The body is a **0-arg vtbl method** on type **39**
`CKeyRedefiner@NUISystem` (`0124ADBC`). It is **not** the
subscriber apply (`00557EB0` is `ret 4`).

First-seen Press Start / New Profile / Main Menu: **never**.
Those trees have no type 39. It is **not** hover (`vtbl+592` /
action 27) and **not** select/click (`vtbl+584` / `0055AF60` /
`00557850`).

`TEXT_GUI_PRESS_CONTROL` is **only** pushed inside this function
(`xrefs.tsv` one site `00557B19`). That string is the remapper
prompt after `0055ACF0`, **not** Press Start
`TEXT_GUI_MENU_PRESS_BUTTON`.

| Claim | Status |
| --- | --- |
| `00557AF0` `E8` `0055ACF0` then maybe arm singleton / `TEXT_GUI_PRESS_CONTROL` / subscribe 33,26,27,35,38–42 | **PROVEN** |
| Any `.text` `E8` / `jmp` to `00557AF0` | **DISPROVEN** (`e8.tsv`; listing has none) |
| `00557AF0` is `0055CB10` apply / inner `vtbl+4` | **DISPROVEN** (0-arg `ret`; apply is `00557EB0`) |
| Owner is type 39 `CKeyRedefiner` `0124ADBC` / inner `0124AD98` | **PROVEN** ctor + dtor `13B8AC8` |
| Slot dword `0124ADBC+?` = `00557AF0` | **PARTIAL** (no `.rdata` listing) |
| Same slot family as `0055ACF0` (0-arg; type 39 wraps it) | **PROVEN** ABI / unique wrap |
| That family is `vtbl+584` click / select | **DISPROVEN** (`00557850` `jmp 0055AF60` is the click thunk) |
| That family is hover-in `vtbl+592` / `0055AFD0` | **DISPROVEN** (`0055ACF0` unsubscribes **28** and posts **`+380`**, not sub 29 / post `+392`) |
| Strongest remaining slot: `vtbl+588` (action 28 unarm, inverse of `+584`) | **PARTIAL** (ABI match; rdata undumped) |
| First-seen Press Start / New Profile / Main Menu invokes it | **DISPROVEN** (no type 39; singleton `13B8AC8` stays 0) |
| `TEXT_GUI_PRESS_CONTROL` has another `.text` pusher | **DISPROVEN** (`xrefs.tsv`) |

**Answer:** nobody in `.text` calls it directly. A type-39 vtbl
slot (not first-seen hover/select) does. First-seen frontend:
**never**.

---

## 1. Dump `00557AF0` (`listing-00540000.txt`)

0-arg (`ret`, not `ret 4`). `ecx` = outer widget.

```
00557AF0  push ecx
00557AF1  push esi
00557AF2  mov esi, ecx
00557AF4  call 0055ACF0              ; post [this+380]
00557AF9  mov eax, [0x13B8AC8]
00557AFE  test eax, eax
00557B00  jne 00557BCB               ; already capturing → skip
00557B06  mov [0x13B8AC8], esi
00557B0C  mov eax, [esi+408]
          test eax, eax
          je  00557B69               ; no text child
00557B16  push -1
00557B18  push "TEXT_GUI_PRESS_CONTROL"
00557B1D  lea ecx, [esp+12]
00557B21  call 0099EBF0
          ; [+408].vtbl+580(1, 1, &string)
          ; then vtbl+52 colour 0x00FFFFFF
00557B69  add esi, 4                 ; inner
          push 33 / 26 / 27 / 35 / 38 / 39 / 40 / 41 / 42
          call [inner.vtbl+12]       ; subscribe
00557BBE  call 0041E5F2
          push esi
          call [input.vtbl+12]       ; register inner
00557BCD  ret
```

`e8.tsv` `0x00557AF4 → 0x0055ACF0` is the only `E8` in this
function besides string/helper calls.

If `13B8AC8` is already set, the function still ran `0055ACF0`
(the `+380` post) and then returns without changing the
prompt or the subscribe set.

---

## 2. Callers: `e8.tsv` + listing

`e8.tsv` dest `0x00557AF0`: **empty**.

`listing-00540000.txt` has `call 00557AF0` / `jmp 00557AF0`:
**none**.

`e8.tsv` around the cluster:

| Site | Dest | Role |
| --- | --- | --- |
| `00557AF4` | `0055ACF0` | this wrap |
| `00557C68` | `00557BD0` | type-39 state case → teardown |
| `00557F42` | `00557BD0` | captured apply, last-key==1 |
| `0055A5D3` | `0055AF60` | type-35 click wrap |

`0055ACF0` itself: one `E8` (`00557AF4`) and two **tails**
`0055A726` / `0055A73B` (`jmp 0055ACF0` from type-35
`0055A660`). Those tails never enter `00557AF0`.

So every live entry to `00557AF0` is `call [this.vtbl+k]` with
`k` **PARTIAL**.

It is **DISPROVEN** as `0055CB10` apply: that path is 1-arg
`ret 4`. Type 39’s apply is `00557EB0` (inner `0124AD98`).
When `13B8AC8 == this`, `00557EB0` binds; else it
`E8 0055AD60`. Neither `E8`s `00557AF0`.

---

## 3. Owner is type 39 `CKeyRedefiner`, not first menus

Ctor `00558540` (factory type **39**, size `0x1C0`):

```
00558549  call 0055B460              ; type 34
00558550  mov [esi],     0x124ADBC   ; outer vtbl
00558556  mov [esi+4],   0x124AD98   ; inner
0055855D  mov [esi+24],  0x124AD90
          ; +404…+444 zero; +408 = text child later
```

Dtor `00558660` is the only other `13B8AC8` write besides
`00557AF0` / `00557BD0` / `00558020`:

```
00558677  cmp [0x13B8AC8], esi
          jne skip
          mov [0x13B8AC8], 0
          jmp 0055B760               ; type-34 dtor
```

RTTI `0x0137C444` `CKeyRedefiner@NUISystem` (`rtti.txt` /
`strings.tsv`). Same object already named in `type7-action35`.

Type 38 Accept is the **next** factory slot (`00558B90`), not
this class. Type 39 as `UI_CANCEL` stays **DISPROVEN**
(`ui-cancel-message`).

Click thunk on the same object is **not** `00557AF0`:

```
00557850  jmp 0055AF60               ; 0-arg +584 shape
```

`vtbl584-post-hop` already used that 5-byte `jmp` as a type-40
example; the bytes sit in the type-39 method cluster before
`00557AF0`.

---

## 4. `0055ACF0` is the `+228` poster, not hover/select

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

`0055B040` (type-34 ctor `0055B4B5`): `[def+228]` boxed →
`vtbl+320`. `0055B5B0` appends that pair onto `widget+380`
(`0055B9D0-post-dword`).

Sibling 0-arg bodies (do not collapse):

| VA | SelectState | Post | Local map | Recovered slot |
| --- | --- | --- | --- | --- |
| `0055AF60` | `def+524` | `[+372]` (`+224`) | **sub** 28 | **`vtbl+584`** / action 26 click |
| `0055ACF0` | `[+364]` | `[+380]` (`+228`) | **unsub** 28 | inverse of click → **`vtbl+588`** **PARTIAL** |
| `0055AFD0` | `def+528` | `[+392]` (`+236`) | **sub** 29 | **`vtbl+592`** ABI / action 27 hover-in |

`action27-release` table `0x55AE88`:

| Action | Dest | Outer slot |
| ---: | --- | --- |
| 26 | `0055AD7B` | `vtbl+584` |
| **27** | `0055AE01` | **`vtbl+592`** (`[+384]=1`) |
| 28 | `0055ADDE` | `vtbl+588` (`[+364]=0`) |
| 29 | `0055AE53` | `vtbl+596` (`[+384]=0`) |

Hover-in is `+592` / `0055AFD0`, **DISPROVEN** for
`00557AF0`. Select/click is `+584` / `00557850` →
`0055AF60`, **DISPROVEN** for `00557AF0`.

`00557AF0` = type-39 wrap of `0055ACF0`, so it is the same
0-arg family as `0055ACF0`. If that family is `+588`, the
recovered caller is `0055AD60` `0055ADDE` after an armed
action 26 (`[+364]=1`, LMB-up type 6 / action 28). Exact
`0124ADBC+588` dword is **PARTIAL**.

---

## 5. First-seen frontend: never (not hover, not select)

Press Start dump (`17-press-start-frame.txt`): types 10, 5, 18,
0, 6, 12, 11, 32. **No 39.** Prompt text is
`TEXT_GUI_MENU_PRESS_BUTTON`, not `TEXT_GUI_PRESS_CONTROL`.

New Profile / Main Menu (`type7-action35`, `type11-msg15`):
type 10 root, type 12 list, type 38 accept / type 11 New Game.
**No `00557AF0` capture.** `13B8AC8` stays 0, so `00557EB0`
always falls through to `0055AD60`.

Hover on those screens (action 27 / `vtbl+592`) cannot reach
`00557AF0` even if a type 11/38 is selected: that slot is
`0055AFD0`, not this wrap.

Select/click (action 26 / `vtbl+584`) on those screens is
`0055AF60` / type-11/38 `+372`. It never `E8`s `00557AF0`.

So first-seen: **never**.

---

## 6. Relation to `TEXT_GUI_PRESS_CONTROL`

`xrefs.tsv`:

```
0x0124ABFC  0x00557B19  fn=0x00557AF0  TEXT_GUI_PRESS_CONTROL
```

`strings.tsv` `0x0124ABFC`. Sole `.text` immediate.

It runs **after** `0055ACF0`, only when:

- `13B8AC8 == 0` (this instance wins the singleton), and
- `[this+408] != 0` (text child exists).

Then `0099EBF0` builds the Lionhead string and
`[+408].vtbl+580` / `vtbl+52` apply it.

That is the options remapper “press a control” label
(`type7-action35`). It is **DISPROVEN** as Press Start
`TEXT_GUI_MENU_PRESS_BUTTON` / `UI_PRESS_START_TEXT`.

Teardown (`00557BD0`, also 0-arg) clears `13B8AC8`, calls
`00557A10` (restore bound-key text), unsubscribes 33, and
does **not** push `TEXT_GUI_PRESS_CONTROL`.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `00557AF0` | type-39 0-arg wrap of `0055ACF0` + capture | **PROVEN** body; slot **PARTIAL** |
| `0055ACF0` | post `[+380]` / unsub 28 | **PROVEN**; =`+588` **PARTIAL** |
| `00557850` | `jmp 0055AF60` | **PROVEN** click thunk; **DISPROVEN** as this caller |
| `0055AFD0` | hover-in post `[+392]` | **PROVEN** ABI; **DISPROVEN** as this body |
| `00557EB0` | type-39 inner apply | **PROVEN**; **DISPROVEN** as caller of `00557AF0` |
| `00557BD0` | capture teardown | **PROVEN** sibling |
| `00558540` | type 39 ctor `0124ADBC` | **PROVEN** |
| `0x13B8AC8` | remapper singleton | **PROVEN** |
| `TEXT_GUI_PRESS_CONTROL` | only `00557B18` | **PROVEN** |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`00557AF0`, `00557BD0`, `00557EB0`, `00557850`, `00558540`,
  `00558660`, `0055ACF0`, `0055AD60`, `0055AE88`, `0055AF60`,
  `0055AFD0`, `0055B040`, `0055B460`, `0055B5B0`, `0055A660`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
- `tools/Fable.ExeIndex/out/00-index/xrefs.tsv`
- `tools/Fable.ExeIndex/out/00-index/rtti.txt` /
  `strings.tsv` (`CKeyRedefiner@NUISystem`,
  `TEXT_GUI_PRESS_CONTROL`)
- `proofs/0055B9D0-post-dword/README.md`
- `proofs/plus224-payloads/README.md`
- `proofs/action27-release/README.md`
- `proofs/type7-action35/README.md`
- `implementer/frontend/17-press-start-frame.txt`
