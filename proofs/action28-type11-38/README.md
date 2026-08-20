# Type 11/38 action 28 at `0055ADDE`: armed `vtbl+588` → `0055ACF0` posts `+380`

Investigation only. No production `src/` edits.

Question: `0055AD60` action **28** at `0055ADDE` — if
`[inner+364]` call outer `vtbl+588`. Is first-seen type 11/38
`vtbl+588` **`0055ACF0`** (or a `jmp` to it)? After action 26
arms, does LMB-up type **6** post **`+380`**?

Authority: dump `Fable.exe` `0055AD60` / `0055ADDE` / table
`0x55AE88` / `0055ACF0` / `0055AF60` / `0055AEB0` / `0054DC30` /
`0054DDB0` / `0054E0B0` / `00558B90` / `0055A660` / `00557AF0`
in `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
`e8.tsv` dest `0x0055ACF0`; `listing-00400000.txt` (`0042E3EE`);
`proofs/0055A726-plus228-jmp/README.md`,
`proofs/action27-release/README.md`,
`proofs/0055ACF0-first-caller/README.md`,
`proofs/action28-after-26/README.md`,
`proofs/type6-action28/README.md`,
`proofs/plus224-payloads/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE** / **LEFTOVER**.

Do not re-prove type 4 → action 26, type 6 = LMB up, CRC
`0x53C644E4` → def `+228`, or that action 26 `0055AF60` posts
`[+372]` / `[def+224]`. Do not invent Lionhead names.

`.rdata` slot dwords were **not** printed this pass
(`listing-01200000.txt` is still `.text`; type 11/38 tables live
at `01249554` / `0124B04C`). Callees are from ABI + unique
bodies. Vtbl **pointers** stay **PARTIAL**.

---

## Verdict

**`0055ADDE` is action 28.** `ecx` is inner (`widget+4`). If
`[inner+364]` is set, it `lea ecx,[esi-4]` and
`call [outer.vtbl+588]`, then writes `[inner+364]=0`. If the
byte is 0 it skips the call and only stamps `+396`.

First-seen type **11** / **38** `vtbl+588` is the **bare**
`0055ACF0` body, not the type-35 tail-`jmp`s. Those `jmp`s
(`0055A726` / `0055A73B`) live in `0055A660`, which is type-35
`+588` only (`0055A726-plus228-jmp`). Press Start / New Profile /
Main Menu have no type 35/39/41.

`0055ACF0` is the unique 0-arg inverse of proven `+584` /
`0055AF60`: SelectState, **unmap** 28, `push [this+380]`,
`call [vtbl+524]`. That list is ctor `[def+228]`. Type 11 slim
`0054DDB0` also pushes `+380` but is **not** that inverse (no
`vtbl+192([+364])`, no `inner.vtbl+16(28)`).

After a **selected** action 26 (`[inner+348]` / `widget+352` ≠ 0)
the click sets `[inner+364]=1` and `0055AF60` locally maps 28.
The next type **6** (LMB up) is action 28. That apply now takes
the armed arm and `vtbl+588` **does** walk `+380`.

| Claim | Status |
| --- | --- |
| `0x55AE88[2] = 0055ADDE` (action 28) | **PROVEN** |
| `0055ADDE`: if `[inner+364]` then `call [outer.vtbl+588]`, else stamp | **PROVEN** |
| Type 6 → `0042E498` `push 28` | **PROVEN** (`type6-action28`) |
| `0055ACF0` `push [this+380]` / `vtbl+524` = `[def+228]` list | **PROVEN** |
| Type-35 `0055A660` tails `jmp 0055ACF0` are first-seen type 11/38 `+588` | **DISPROVEN** (no type 35 on those trees) |
| Type-39 `00557AF4` `E8 0055ACF0` is type 11/38 `+588` | **DISPROVEN** |
| Type 11/38 `01249554+588` / `0124B04C+588` dword is `0055ACF0` | **PARTIAL** (unique 0-arg inverse; no rdata) |
| Type 11 `0054DDB0` is that `+588` dword | **DISPROVEN** as inverse of `0055AF60`; rdata **PARTIAL** |
| Action 26 / `vtbl+584` / `0055AF60` posts `+380` | **DISPROVEN** |
| First-seen LMB-up before a selected 26 posts `+380` | **DISPROVEN** (`[+364]=0` skips `+588`) |
| After selected 26, LMB-up type 6 posts `+380` via `+588` | **PROVEN** call + body; slot dword **PARTIAL** |
| Accept / New Game / INVISIBLE `+380` holds `0x126` / 15 / `0xE5` | **PROVEN** store (`plus224-payloads`) |

**Answer:** yes to the `0055ADDE` test/call. First-seen type
11/38 `+588` is **`0055ACF0` itself**, not a `jmp`. After
action 26 **arms**, LMB-up type 6 **does** post `+380` (the
`[def+228]` list). The rdata dword is still **PARTIAL**.

---

## 1. Dump `0055AD60` case 28 = `0055ADDE`

`ecx` = inner = `widget+4`. `ret 4`.

```
0055AD60  push esi / edi
0055AD62  mov edi, [esp+12]          ; action
0055AD66  lea eax, [edi-26]
0055AD69  cmp eax, 6
0055AD6C  mov esi, ecx
0055AD6E  ja  0055AE79               ; 0055B9D0 only
0055AD74  jmp [0x55AE88+eax*4]
```

Table dwords (`action27-release`; listing junk-decode of
`0055AE88`; **not** `.text` layout):

| `eax` | Action | Dest | Outer call |
| ---: | ---: | --- | --- |
| 0 | **26** | `0055AD7B` | `vtbl+584` then `[inner+364]=1` |
| 1 | 27 | `0055AE01` | `vtbl+592` hover-in |
| 2 | **28** | **`0055ADDE`** | if armed: **`vtbl+588`** |
| 3 | 29 | `0055AE53` | `vtbl+596` hover-out |
| 4 | 30 | `0055AE79` | `0055B9D0` only |
| 5 | 31 | `0055ADB2` | debounce + `vtbl+524([+372])` |
| 6 | 32 | `0055AE20` | hover persist `+388` |

`type6-action28` §6 code-order map is **STALE** for 27/29–32.
Case **28** is still `0055ADDE`.

```
0055ADDE  mov al, [esi+364]          ; inner+364 (armed u8)
          test al, al
          je  0055AE70               ; unarmed → stamp only
0055ADEC  mov edx, [esi-4]           ; outer vtbl
0055ADEF  lea ecx, [esi-4]           ; outer this
0055ADF2  call [edx+588]             ; 0-arg
0055ADF8  mov [esi+364], 0
          jmp 0055AE70
0055AE70  [esi+396] = [esi+44]
0055AE79  push edi
          call 0055B9D0              ; 28 ≠ 25 → ret 4
          ret 4
```

No `E8 0055ACF0`. No `E8 0055AF60`. No `push [+372]` /
`push [+380]`. No `call [vtbl+524]`. The post, if any, is
inside the `+588` callee.

Type 11 inner `0054DBC0` only reaches this switch when parent
`[def+545]` is set (`0054DC1C`). Type 38 apply **is**
`0055AD60`.

---

## 2. `0055ACF0` is the `+380` / unmap-28 body

0-arg (`ret`). `ecx` = **outer** widget.

```
0055ACF0  push esi
          mov esi, ecx
          push [esi+364]
          call [vtbl+192]            ; SelectState(stored +364)
          lea ecx, [esi+4]
          push 28
          call [inner.vtbl+16]       ; ERASE 28
          push [esi+380]
          call [this.vtbl+524]       ; walk +228 list
          pop esi
          ret
```

Pair with click `0055AF60` (action 26 `+584`):

| | `0055AF60` (`+584`) | `0055ACF0` (candidate `+588`) |
| --- | --- | --- |
| SelectState | `[def+524]` | `[this+364]` |
| Local 28 | inner **`vtbl+12` insert** | inner **`vtbl+16` erase** |
| Post | `[+372]` / `[def+224]` | `[+380]` / `[def+228]` |

`0055AF60` never loads `+380`. `0055ACF0` never loads `+372`.

Walker of `vtbl+524` is `00558DE0` (ABI / unique list walk;
rdata **PARTIAL**). Empty head `je` returns. Nonempty
`&node+8` → input `vtbl+56` `0041E6D3` → frontend UI
`vtbl+32` `0059A238`. Boxed dword0 is `[def+228]`.

---

## 3. First-seen type 11/38 `+588` is bare `0055ACF0`, not a `jmp`

`.text` entries into `0055ACF0`:

| Site | Kind | Owner |
| --- | --- | --- |
| `00557AF4` | sole `e8.tsv` `E8` | type-39 `00557AF0` (`CKeyRedefiner`) |
| `0055A726` / `0055A73B` | tail `jmp` | type-35 `0055A660` |

`0055A660` is the type-35 **override** of this unarm slot
(`0055A726-plus228-jmp`). Both tails restore `ecx = this` and
`jmp 0055ACF0`. That is type-35 `+588`, not type 11/38.

First-seen trees have **no** type 35 / 39 / 41
(`17-press-start-frame.txt`; Accept = 38, New Game /
INVISIBLE = 11). Those `E8` / `jmp` sites **do not run**.

Type 11 ctor `0054E0B0` / type 38 ctor `00558B90` call type-34
`0055B460` then overwrite:

```
0054E0B8  call 0055B460
          mov [esi],   0x1249554     ; type 11 outer
          mov [esi+4], 0x1249530

00558B98  call 0055B460
          mov [esi],   0x124B04C     ; type 38 outer
          mov [esi+4], 0x124B024
```

Type 38 has **no** local clone of `0055ACF0` and **no**
`0054DDB0`. Its 0-arg unarm slot is the inherited type-34
body.

Type 11 cluster next to apply:

| Fn | If `def+545` | Shape |
| --- | --- | --- |
| `0054DD50` | `push [esi+372]; vtbl+524` | post half of `0055AF60` |
| `0054DDB0` | `push [esi+380]; vtbl+524` | post half of `0055ACF0` |
| `0054DE10` | `push [esi+392]; vtbl+524` | hover list |

`0054DDB0` is **not** the inverse of `0055AF60`: it does not
`vtbl+192([+364])` and does not `inner.vtbl+16(28)`. Action 28
unarm matches the **full** `0055ACF0` pair (26 maps 28; 28
erases 28). So type 11 `+588` is still `0055ACF0`; `0054DDB0`
is another **caller** of `vtbl+524([+380])`, not the action-28
slot.

Expected rdata (not printed):

| Table | VA of `+588` | Expected dword |
| --- | --- | --- |
| type 11 `01249554` | `012497A0` | `0055ACF0` |
| type 38 `0124B04C` | `0124B298` | `0055ACF0` |
| type 34 `0124BD2C` | `0124BF78` | `0055ACF0` |
| type 35 `0124BA94` | `0124BCE0` | `0055A660` (then `jmp 0055ACF0`) |

Dump: `Fable.ExeIndex vtbl 0x01249554 160`,
`vtbl 0x0124B04C 160`. Until then the **pointer** is
**PARTIAL**. The **body** first-seen type 11/38 would enter is
`0055ACF0`, not `0055A660`.

---

## 4. After action 26 arm, type 6 posts `+380`

`0042E3EE`: type **4** (LMB down) `push 26`; type **6** (LMB
up) `push 28` (`0042E49D`). Same `0055CB10` walk.

Action 26 (`0055AD7B`):

```
test [esi+348]                   ; widget+352 u8
je  skip +584
call [outer.vtbl+584]            ; 0055AF60
mov [esi+364], 1
call 0055B9D0                    ; nop for 26
```

`0055AF60` posts empty `[+372]` on Accept / New Game /
INVISIBLE (`+224==0`) and `inner.vtbl+12(28)`.

| Object | 28 mapped before first 26 | After selected 26 |
| --- | --- | --- |
| Type 11 (`0054DC30` if parent `+545`) | **yes** (26, 31, **28**, 27, 32, 29) | insert again (idempotent) |
| Type 38 (`0055AEB0`) | **no** (26, 31, 27, 32 only) | **first** local 28 |

Ctor `0055B460` / `0055BA20` leaves `[inner+364]=0` and
`[widget+352]=0`. First-seen type 6 therefore takes
`je 0055AE70` and **does not** call `+588`.

Once a **selected** 26 has run:

```
type 4 → 26 → +584 / 0055AF60([+372]==0); map 28; [+364]=1
type 6 → 28 → +588 / 0055ACF0([+380])     ; 0x126 / 15 / 0xE5
```

That is the first type 11/38 path that posts the persist
`+228` list. It is **not** action 26. It is **not**
`00557AF4` / `0055A726`.

`+352=1` is still required **before** that 26
(`type11-plus352-select`; only store is `0055C0DE` in
`0055BF10`). Attach does not write it. Whether a pointer tick
has already taken selection by the first LMB down is
**UNREAD** here.

`type6-action28` “action 28 posts no `0x126` / 15” is
**STALE**: that assumed `+588` is not a list poster. If
`+588` is `0055ACF0`, LMB-up **does** `vtbl+524([+380])`.

---

## 5. C# leftover (do not apply here)

| Site | Native | Host |
| --- | --- | --- |
| type 6 → 28 | `0042E49D` | `ActionType6=28` **MATCH** |
| 28 → `+380` after arm | `0055ADDE` → `+588` / `0055ACF0` | `MessageFromWidgets` type 6 + `Armed` **MATCH** shape |
| `+588` dword printed | no | `Plus228PostFn=0055ACF0` assumes the ABI **MATCH** / rdata **PARTIAL** |
| type 11 `0054DDB0` as 28 | no (not the unarm inverse) | unused **MATCH** |
| First LMB-up with `Armed=false` | skip `+588` | `MessageFromPlus228List` requires `Armed` **MATCH** |

Do **not** post `+228` from action 26. Do **not** treat the
type-35 `jmp`s as the first-seen type 11/38 slot.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0055ADDE` | action 28 → `vtbl+588` if `[inner+364]` | **PROVEN** |
| `0055ACF0` | unmap 28; `vtbl+524([+380])` | **PROVEN** body; `=type 11/38 +588` **PARTIAL** |
| `0055AF60` | action 26 `+584`; map 28; post `+372` | **PROVEN** |
| `0055A726` / `0055A73B` | type-35 `jmp 0055ACF0` | **PROVEN**; first-seen 11/38 **DISPROVEN** |
| `00557AF4` | type-39 `E8 0055ACF0` | **PROVEN**; first-seen 11/38 **DISPROVEN** |
| `0054DDB0` | type-11 slim `+380` poster | **PROVEN** body; **DISPROVEN** as `+588` inverse |
| `01249554+588` / `0124B04C+588` | expected `0055ACF0` | **PARTIAL** |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0055ACF0`, `0055AD60`, `0055ADDE`, `0x55AE88`, `0055AEB0`,
  `0055AF60`, `0055A660`, `0055A726`, `00557AF0`, `0054DBC0`,
  `0054DC30`, `0054DDB0`, `0054E0B0`, `00558B90`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
  (`00557AF4` → `0055ACF0` only)
- `listing-00400000.txt` (`0042E498` `push 28`)
- `proofs/0055A726-plus228-jmp/README.md`
- `proofs/action27-release/README.md`
- `proofs/0055ACF0-first-caller/README.md`
- `proofs/action28-after-26/README.md`
- `proofs/type6-action28/README.md` (event 6 → 28 **PROVEN**;
  “28 posts nothing” **STALE** if `+588`=`0055ACF0`)
- `proofs/plus224-payloads/README.md`
- `src/Fable.Game/FrontendInputMap.cs`
