# `005952C3` `vtbl+192`(5) vs scale init `0052C730`

Investigation only. No production `src/` edits.

Question: `0042F015` `005952C3` `vtbl+192`(5) on Press
Start after `00598A1C`. Exact order vs scale init
`0052C730` (zeros `+332`)? Does `0052C730` run after
`005952C3` and wipe `+332=5`?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0042EF6F` / `0042F00A` / `0042F015` / `0040F0E0`);
`listing-00580000.txt` (`005952C3` / `00598A1C` /
`0059672A` / `00599CDA` / `00599738` / `00595ACC` /
`0059B039`);
`listing-00500000.txt` (`0052C730` / `005339B0` /
`0052CF40` / `0052C7E0` / `00531EC0` / `00530260` /
`005334A0` / `0052CAF0`);
`listing-00540000.txt` (`0054E4B0`);
`proofs/0059A119-state5/README.md`;
`proofs/0052C730-host-state/README.md`;
`proofs/0054DC30-first-call/README.md`;
`docs/runtime/FORWARD_TREE.md`.

Do not re-prove persist Type=10 on
`UI_FRONTEND_PRESS_START_MENU`, `00598EE6` `0xE5`,
or `00596763` as later `+192`(6).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| `0042F015` `005952C3` `vtbl+192`(5) on Press Start after `00598A1C`? | **Yes.** `"Init frontend"` tail: `0042F00A 0042DED5` then `0042F015 call 005952C3`. Body is `[ui+32].back()` `push 5` / `call [eax+192]`. First-seen back is slot `0x14` from `00599CDA 0059672A`. | **PROVEN** |
| Exact order vs `0052C730` (zeros `+332`)? | `00598A1C` (factory + current push) → Init Engine → `005952C3` (`+332=5`) → `0062F800` / `0062F8B0` / `0040F0E0`. `0052C730` is the type-10 `vtbl+172` body (`0054E4B0`), **not** an Init-frontend sibling. | **PROVEN** order of `0042EF6F`; `+172` on this root **UNREAD** as a first-seen site |
| Does `0052C730` run after `005952C3` and wipe `+332=5`? | **No.** `005952C3` is only `+192` → `0052CF40` `mov [this+332], 5`. Next `0042EF6F` calls are fade clocks / `0040F0E0` viewport floats. First-frame `0052C7E0` / `00531EC0` / `00530260` do not `E8 0052C730` and do not `vtbl+172`. Later `+192`(6) replaces 5; that is not `0052C730`. | **DISPROVEN** |

---

## Verdict

**`005952C3` sets Press Start `+332=5` after attach.
`0052C730` does not run after that write and does not
wipe it.**

| Claim | Status |
| --- | --- |
| `0042F015` is `"Init frontend"` `call 005952C3` after `0042DED5(0)` | **PROVEN** |
| `005952C3` is `add ecx, 32` / `0059B039` / `push 5` / `vtbl+192` / `ret` | **PROVEN** |
| First-seen `[ui+32].back()` is slot `0x14` Press Start | **PROVEN** (`00599CAE` / `0059672A`) |
| Type-10 `+192` store is `0052CF40` `+332=arg` | **PROVEN** body; `.rdata` `012497E4+192` **UNREAD** |
| `0052C730` is `005339B0` then `+324/+328/+332=0` | **PROVEN** |
| Type-10 `vtbl+172` `0054E4B0` starts with `call 0052C730` | **PROVEN** |
| `.text` `E8 0054E4B0` / `E8 005339B0` except from `0052C730` | **DISPROVEN** (empty / only `0052C733`) |
| `00598A1C` slot-`0x14` factory / `0041DB1D` / `005331A0` call root `+172` | **DISPROVEN** |
| `00599738` `+172` is after `005952C3` | **DISPROVEN** (still inside `00598A1C`) |
| `005952C3` / `0062F800` / `0040F0E0` / first-frame tick+draw call `0052C730` | **DISPROVEN** |
| `0052C730` after `005952C3` wipes Press Start `+332=5` | **DISPROVEN** |
| First-seen Press Start **root** `vtbl+172` site | **UNREAD** (Main Menu `00595ACC` is later / other root) |
| Host `ApplyFrontendScaleInit` note after `SelectState(5)` is this native tail | **DISPROVEN** leftover (scale init is `+172`, not `0042F015`) |

---

## 1. `0042EF6F` then `005952C3`

`listing-00400000.txt`:

```
0042EF6F  push "Init frontend"
          …
0042F00A  call 0042DED5          ; fade 0; audio
0042F00F  mov  ecx, [esi+180]    ; UI*
0042F015  call 005952C3
0042F01A  lea  ecx, [esi+256]
0042F020  call 0062F800
0042F02B  call 0062F8B0
0042F037  call 0040F0E0          ; [0x13B876C/+70] = viewport
0042F03C  jmp  0042F20F
```

This is **after** bind `0042E98F` → `00598A1C(0)`
(`0042EA62`) and Init Engine `0042E204`.

`listing-00580000.txt`:

```
005952C3  add  ecx, 32           ; [ui+32] deque
          call 0059B039          ; back()
          mov  ecx, [eax]
          mov  eax, [ecx]
          push 5
          call [eax+192]
          ret
```

No `E8 0052C730`. No `vtbl+172`.

`00598A1C` tail when `[ui+192]==0` (first-seen):

```
00599C94  cmp  [ebx+192], 0
          jne  00599CB7
          …
00599CAE  mov  [ebp+124], 0x14
          …
00599CDA  call 0059672A          ; input vtbl+8(+4); 0059B61C push_back
```

`0059672A` does **not** `+192` / `+172`. Current back
is the Press Start widget stored at slot `0x14`.

Type-10 `+192` body (`0052CF40-selectstate-6`):

```
0052CF49  cmp  [esi+332], ebp
          je   0052D35E
0052CF58  mov  [esi+332], ebp    ; +332 = 5
          ; clear +312/+308 / +316 list; vtbl+540; child vtbl+188
```

No `call 0052C730`. Child `+188` candidate `0052CAF0`
writes **child** `+332`, not the Press Start root.

---

## 2. What `0052C730` is

```
0052C730  push esi
          mov  esi, ecx
          call 005339B0          ; dest / inherit; child +172 recurse
          xor  eax, eax
          mov  [esi+324], eax
          mov  [esi+328], eax
          mov  [esi+344], eax
          mov  [esi+332], eax    ; wipe
          mov  [esi+320], 0xBF800000
          …
          ret
```

`005339B0` is **only** `E8`’d from `0052C733`. Type-10
`vtbl+172`:

```
0054E4B0  mov  esi, ecx
          call 0052C730
          ; [+48] → +348; optional "UI_ACCEPT"
```

No `.text` `E8 0054E4B0`. Dispatch is vtbl-only.

Ctor `005334A0` zeros through `+303` / lists; it does
**not** store `+324/+328/+332`. Those dwords stay 0
until `0052C730` or `0052CF40` (heap leftover
**PARTIAL** if neither ran).

---

## 3. `0052C730` is not after `005952C3`

| After `005952C3` | Calls `0052C730` / root `+172`? |
| --- | --- |
| `0062F800` / `0062F8B0` | **no** — pump fade clocks |
| `0040F0E0` | **no** — `009BEDC0` → two floats |
| First-frame `00599E3F` `[ui+84]` `vtbl+4` `0052C7E0` | **no** — `+196` / `+540` / `00531EC0` dest |
| `00531EC0` | **no** — uses existing `+272/+276`; child `+72/+456` |
| Draw `00595222` → `vtbl+8` `00530260` | **no** — child `+8` only |
| Empty `0xE5` `00595845` / `00596763` | **no** — `+192`(6) **replaces** 5; not a zero |

`00598A1C` has one `+172` walk at `00599738`, **before**
`0059672A` / return / `0042F015`. `esi` is still the UI
object (`0059B5D7` `ecx`). That is **not** a post-select
wipe. Slot-`0x14` factory / `0041DB1D` / `005331A0` do
not call root `+172` (`0054DC30-first-call`).

`FORWARD_TREE.md` nesting `0054E4B0` under `00598A1C`
is **not** a recovered `E8` on slot `0x14`. Main Menu
`00595ACC call [eax+172]` is the first recovered **root**
`+172`, after `0x126` / `00595A06`, on a **different**
widget.

---

## 4. Host leftover

`InitFrontendUi` notes `005952C3` / `SelectFrontendState(5)`
then `ApplyFrontendScaleInit` (`0052C730` / `005339B0`).
Native Init-frontend tail has **no** `0052C730`. Factory
`ApplyFirstSeenState` (`+332=0`) runs inside
`AttachPressStartWidgets` **before** the select-5 note —
that direction **MATCH**es “zeros first, then `+192`(5)”
if `+172` ever ran on attach (site still **UNREAD**).

Do **not** re-apply `0052C730` after `005952C3`. That
would invent the wipe this listing set **DISPROVES**.

---

## Do not invent

- Lionhead names for style keys 0–6.
- `.rdata` `012497E4+172/+192` dwords.
- First-seen Press Start root `+172` as `00595ACC`.
- `0052C730` as a `0042EF6F` sibling of `005952C3`.
