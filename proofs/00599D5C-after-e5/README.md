# After `0xE5` → `00599D5C`: order vs `005955AB` / `00595845` / `00596917`

Investigation only. No production `src/` edits.

Question: after `0059A238` consumes `0xE5` and jumps to
`00599D5C`, what is the first-seen order vs `00595845` /
`00596917` / `005955AB`? Does `00599D5C` write `[ui+160]`
then return so the same `00599E3F` tick binds New Profile?

Authority: `Fable.exe` complete dump
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`0059A238` / `00599D5C` / `005955AB` / `00595845` /
`00596917` / `00599E3F` / `00595422`),
`listing-00400000.txt` (`0042EC7C` / `0042E3EE` /
`0042DC94` / `00412450`),
`listing-00540000.txt` (`0054E2FA`),
`listing-00840000.txt` (`00851700`),
`e8.tsv`.
Host `EngineLifecycle.DispatchFrontendMessage` /
`BindNewProfileFromArmedTick` is **not** authority.

Do not re-prove type 4 → action 26, attach
`00598EE6` `0xE5`, or `0054E4F0` → widget+352.
Do not start Oakvale.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER**.

---

## Verdict

**Empty-vector arm (recovered first-seen, no names):**
`005955AB` → `00595845` (second `005955AB` inside) →
`00599D5C` returns → **same `0042EC7C` frame**
`0042DC94` → `00599E3F` → `00596917` **only if**
`[ui+160]≠0` and the `[ui+32]` predicates pass.

`00599D5C` does **not** write `[ui+160]`. The only
`[ui+160]=1` store in this UI object is `00595845`.
`00596917` is **not** a callee of `00599D5C`. Its only
`E8` site is `00599ED2` inside `00599E3F`.

Same-frame `00599E3F` after the poll that ran `0059A238`
is **PROVEN**. That tick **unconditionally** binding New
Profile is **PARTIAL**: the `+160` write is in place,
but `00599E3F` still requires `[ui+32]` `vtbl+196` or
`vtbl+56` `[+3]==0`. Those returns are **UNREAD**.

| Claim | Status |
| --- | --- |
| `0059A238` `sub ecx, 0xE5` / `je` → `call 00599D5C` | **PROVEN** `0059A6BE` / `0059A781` |
| First call inside `00599D5C` is `005955AB` | **PROVEN** `00599D79` |
| Empty `005955AB` (begin==end) → `00595845` | **PROVEN** `00599D84` / `00599D8A` |
| `00599D5C` calls `00596917` | **DISPROVEN** (no `E8`; `e8.tsv` dest `00596917` is only `00599ED2`) |
| First-seen order of the three: `005955AB`, then `00595845`, then `00596917` | **PROVEN** as code order on the empty arm; disk emptiness **UNREAD** |
| `00599D5C` writes `[ui+160]` | **DISPROVEN** (no `mov [esi+160]`) |
| `[ui+160]=1` is `00595845` | **PROVEN** `00595873`; only `=1` store in `listing-00580000.txt` |
| `00599D5C` then `ret` to `0059A238` | **PROVEN** `00599E3E` / `jmp 0059A7FF` |
| Same `0042EC7C` iteration: `0042E3EE` then `0042DC94` → `00599E3F` | **PROVEN** `0042F0AC` / `0042F0B3` |
| Same `00599E3F` tick always binds New Profile | **PARTIAL** (`+160` set; `vtbl+196` / `vtbl+56[+3]` **UNREAD**) |
| Host `DispatchFrontendMessage` / `BindNewProfileFromArmedTick` as native order | **not authority** (C# skips the `[ui+32]` gates) |

---

## 1. `0xE5` is `call 00599D5C`, then return

`0059A238` (`listing-00580000.txt`):

```
0059A281  mov eax, [ebp+8]
0059A284  mov eax, [eax]
0059A286  mov ecx, [eax]          ; id
…
0059A6BE  sub ecx, 0xE5
0059A6C4  je 0059A77F
…
0059A77F  mov ecx, esi            ; UI this
0059A781  call 00599D5C
0059A786  jmp 0059A7FF            ; epilogue
```

`e8.tsv`: `0x0059A781 → 0x00599D5C` is the only
`.text` `E8` to `00599D5C`.

The listing at `00599D5C` is **misaligned**
(`functions.tsv` has no function start). Recovered
prologue from the leftover decode (`add [ebp-117], dl` /
`db 0xEC` = `55 8B EC`) plus the live body:

```
00599D5C  push ebp
00599D5D  mov ebp, esp
00599D5F  sub esp, 24
          push ebx / esi
          xor ebx, ebx
          push edi
          lea eax, [ebp-24]
          push eax
          mov esi, ecx
          ; [ebp-24]/[ebp-20]/[ebp-16] = 0
00599D79  call 005955AB
00599D7E  mov edi, [ebp-24]
00599D81  mov eax, [ebp-20]
00599D84  cmp edi, eax
00599D86  jne 00599D94
00599D88  mov ecx, esi
00599D8A  call 00595845
00599D8F  jmp 00599E1B
…
00599E3E  ret
```

No `call 00596917`. No `mov [esi+160], …`.

---

## 2. First-seen order of the three

### `005955AB` first

`005955AB` is a thiscall that **fills** the 12-byte
vector pushed at `[ebp-24]` (begin / end / cap). It
starts by `00412450` (vector reset: `[vec+4] = [vec]`,
so begin==end), then walks a directory (`0041A540` /
`00999760` / `004128A0`). Empty means **no names
appended**.

Whether a given install has save names is **UNREAD**
(no directory listing in this pass). The recovered
first-seen path used everywhere else in this tree is
the empty arm (`cmp edi, eax` / `je` → `00595845`).

Non-empty `00599D5C` (`00599D94+`) is `005957D9` /
`0059899A` / `00596763` or `00597B20`. That arm does
**not** call `00595845` and does **not** write
`[ui+160]`.

`00595845` is reached from only two `E8`s
(`e8.tsv`): `00599D8A` (this empty arm) and
`0059A70A` (a **different** `0059A238` id, not `0xE5`).

### Then `00595845` (empty only)

```
00595845  mov esi, ecx
00595850  call 0040D2A0
00595855  mov [eax+12], 1
          ; zero a local 12-byte vector
0059586A  call 005955AB            ; second enum
0059586F  mov [esi+100], 1
00595873  mov [esi+160], 1
          ; 0041E5F2 / [ui+32] 0059B039 / vtbl+192(6)
005958F4  ret
```

`functions.tsv` `0x00595845` callees:
`0040D2A0`, `005955AB`, `0041E5F2` ×3, `0059B039`,
`00412130`. **No** `00596917`.

`[esi+160]` stores in `listing-00580000.txt` on this
object:

| VA | Insn | Role |
| --- | --- | --- |
| `00595468` | `mov [esi+160], bl` | UI ctor zero |
| `00595873` | `mov [esi+160], 1` | **only** set-1 |
| `00599E88` | `cmp [esi+160], bl` | tick test |
| `00599ECC` | `mov [esi+160], bl` | tick clear before bind |

### Then `00596917` — later, in `00599E3F`

`e8.tsv` dest `0x00596917`: **one** site,
`0x00599ED2`.

```
00599E88  cmp [esi+160], bl
00599E8E  je 00599F04
          ; [ui+32] 0059B039; [ptr]==0 → skip
          ; [ptr].vtbl+196 != 0 → bind
          ; else [ptr].vtbl+56 ; [eax+3]!=0 → skip
00599ECC  mov [esi+160], bl
00599ED2  call 00596917
```

`00596917` looks up slot `0x17`, `00596763` switch,
`00851700` (`[ui+96]`, `+4=+5=0`), `00851770`.
It does **not** run inside `00599D5C`.

After that bind, same `00599E3F` continues to
`[ui+96]`. `00851700` left `+5=0`, so
`cmp [eax+5], bl` / `je 0059A0C4` **skips** the later
`005955AB` (`00599F4D`) and `0059697A`. First-seen
after `0xE5` does **not** hit those.

**First-seen order (empty arm):**
`005955AB` → `00595845` → (`00599D5C` `ret`) →
`00599E3F` → `00596917`.

---

## 3. Same-frame `00599E3F`, not same call

`0042EC7C` loop (`listing-00400000.txt`):

```
0042F20F  call 009A4EC0
0042F216  call 009A6460
0042F21B  cmp eax, 2
0042F21E  jne 0042F041          ; per-frame
…
0042F0AA  mov ecx, esi
0042F0AC  call 0042E3EE         ; poll (0059A238 lives here)
0042F0B1  mov ecx, esi
0042F0B3  call 0042DC94         ; tick
```

`0042DC94` ends with `call 00599E3F` (`0042DD15`).
No branch between `0042E3EE` and `0042DC94`.

`00599E3F` does **not** call `0059A238`
(`functions.tsv` `0x00599E3F` callees include
`00596917` / `005955AB` / `0059697A`, not
`0059A238`).

So: consume `0xE5` on the poll, **return** out of
`00599D5C` / `0059A238` / `0042E3EE`, then the
**same iteration** calls `00599E3F`. That is the
“same tick” in frame order, not a nested call.

`00599E3F` bind is **not** “`00599D5C` wrote `+160`
and fell through.” `+160` is written by `00595845`.
Bind still depends on `[ui+32]`:

- `00595845` already does `0059B039` / `vtbl+192(6)`
  with no null check, so a surviving empty arm has a
  current widget. That part of the tick gate is
  **PROVEN** non-null if `00595845` returned.
- `vtbl+196` / `vtbl+56[+3]` after that `vtbl+192(6)`
  are **UNREAD**. Host `BindNewProfileFromArmedTick`
  skips them (**LEFTOVER** vs dump; not used as
  authority).

---

## Do not invent

- `00599D5C` writing `[ui+160]`.
- `00596917` as a callee of `00599D5C` or of
  `0059A238` on `0xE5`.
- `00595845` on a non-empty `005955AB` from `0xE5`.
- Oakvale / `StartOakValeSetup` from this message.
- Host `DispatchFrontendMessage` order as the dump.
