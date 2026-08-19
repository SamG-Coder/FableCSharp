# Type 11/38 ctor zeros `inner+364` (`widget+368`)

Investigation only. No production `src/` edits.

Question: does the type 11/38 ctor zero `inner+364`
(`widget+368`)? First-seen unarmed so action 28 skips
`0055ACF0` until action 26?

Authority: dump type 11 ctor `0054E0B0`, type 38 ctor
`00558B90`, type 34 ctor `0055B460` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055BA20` / `0055B040` / `0055AD60` / `0055ADDE` /
`0055ADA1` / `0055ACB0` / `0055AF30` / `0054DC30` /
`0055AEB0` / `0054DF50`); `e8.tsv` dest `0x0055B460`;
`proofs/action27-release/README.md`;
`proofs/action28-after-26/README.md`;
`proofs/0055ACF0-first-caller/README.md`;
`proofs/type11-plus352-select/README.md`;
`proofs/0055B9D0-post-dword/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**
/ **STALE**.

Do not re-prove `0055ACF0` `push [esi+380]` / `vtbl+524`,
action 26 `vtbl+584` / `0055AF60` posts `[+372]` /
`[def+224]`, or table `0x55AE88` dwords. Do not invent
Lionhead names. Do not collapse **outer** `+364` (dword
SelectState backup) with **inner** `+364` (armed `u8`).

---

## Verdict

**Yes.** Type 11 `0054E0B0` and type 38 `00558B90` both
`call 0055B460`. That type-34 ctor `xor eax,eax` then
`mov [esi+368], al`. `esi` is the **outer** widget, so
`widget+368` = **`inner+364`** (`inner` = `widget+4`).

First-seen that byte is **0**. Action 28 `0055ADDE`
`mov al,[esi+364]` / `test al` / `je 0055AE70` therefore
**never** `call [outer.vtbl+588]`. The hop that *could*
be `0055ACF0` stays unreached until a later action **26**
writes `[inner+364]=1` at `0055ADA1`.

| Claim | Status |
| --- | --- |
| Type 11 ctor `0054E0B0` `E8` `0055B460` | **PROVEN** `0054E0B8` |
| Type 38 ctor `00558B90` `E8` `0055B460` | **PROVEN** `00558B98` |
| `0055B460` `mov [esi+368], al` after `xor eax,eax` | **PROVEN** `0055B48B` |
| That byte is `inner+364` / armed `u8` | **PROVEN** (same field action 26/28 use) |
| Type 11/38 tails after `0055B460` rewrite `+368` | **DISPROVEN** |
| `0055B040` refills `+368` from persist | **DISPROVEN** (lists `+372` / `+380` only) |
| Type 33 `0055BA20` already wrote `+368` | **DISPROVEN** (stops at `+360`) |
| First-seen `[inner+364]=0` | **PROVEN** |
| Action 28 unarmed → `je 0055AE70` (no `+588`) | **PROVEN** |
| First-seen action 28 therefore skips `0055ACF0` | **PROVEN** skip; `+588` dword **PARTIAL** |
| Action 26 is the recovered `=1` writer | **PROVEN** `0055ADA1` |

**Answer:** ctor zeros `inner+364` (`widget+368`). First-seen
action 28 is unarmed and skips `0055ACF0` until action 26
arms the byte.

---

## 1. Two `+364`s on the same object

`0055AD60` `ecx` is **inner** (`widget+4`). `0055B460` /
`0055ACF0` / `0055AF60` `ecx` is **outer**.

| Field | Outer disp | Inner disp | Width | Role |
| --- | ---: | ---: | --- | --- |
| SelectState backup | **`+364`** | `+360` | dword | `0055AF60` store; `0055ACF0` `vtbl+192` arg |
| Armed | **`+368`** | **`+364`** | **u8** | action 26 `=1`; action 28 gate |
| `+224` list | `+372` | `+368` | ptr | **not** the armed byte |

Action 28 reads **`[inner+364]`** (`mov al`). That is
**`widget+368`**, not the dword `0055ACF0` loads at
`[outer+364]`.

`0055B460` zeros **both**:

```
0055B46D  xor eax, eax
0055B485  mov [esi+364], eax      ; outer+364 dword = 0
0055B48B  mov [esi+368], al       ; outer+368 / inner+364 u8 = 0
```

The dword store does **not** cover `+368`. The next
instruction is the armed-byte zero this note is about.

---

## 2. Dump `0055B460` (type 34 ctor)

`ecx` = newly allocated widget. `ret 4`. `e8.tsv` dest
`0x0055B460`: seven sites. Type 11 / 38 are `0054E0B8` /
`00558B98`.

```
0055B460  mov eax, [esp+4]
          push esi
          push eax
          mov esi, ecx
0055B468  call 0055BA20           ; type 33; zeros through +360
          xor eax, eax
          mov ecx, esi
          [esi]     = 0124BD2C
          [esi+4]   = 0124BD08
          [esi+24]  = 0124BD00
0055B485  mov [esi+364], eax      ; dword
0055B48B  mov [esi+368], al       ; u8  ← inner+364
0055B491  mov [esi+372], eax      ; +224 list head
0055B497  mov [esi+376], eax
0055B49D  mov [esi+380], eax      ; +228 list head
0055B4A3  mov [esi+384], eax
0055B4A9  mov [esi+388], al
0055B4AF  mov [esi+392], eax
0055B4B5  call 0055B040           ; copy def +224…+236 onto lists
          mov eax, esi
          pop esi
          ret 4
```

Sister `0055B4C0` (copy-ctor) repeats the same
`[esi+364]` / `[esi+368]` zeros then `0055B040`.

Type 33 `0055BA20` (always called first):

```
0055BA29  call 0052CC50
          xor eax, eax
          [esi]     = 0124BFB4
          [esi+4]   = 0124BF90
          [esi+24]  = 0124BF88
0055BA46  mov [esi+348], eax
0055BA4C  mov [esi+352], al       ; selected u8 (type11-plus352-select)
          [esi+356] = 0
          [esi+360] = 0
          input.vtbl+8(inner)
          ret 4
```

No store at `+364` or `+368`. Type 34 owns those fields.

`0055B040` then `test [def+224]` / `test [def+228]` and
`vtbl+284` / `vtbl+320` into **`+372` / `+380`**. No
`mov […+368]`. Accept / New Game / INVISIBLE refill
`+380` when `[def+228]` is nonzero (`plus224-payloads`);
the armed byte stays 0.

---

## 3. Type 11 / 38 ctors inherit the zero

Type 11 `0054E0B0` (`01249554`, alloc `0x1B4`):

```
0054E0B8  call 0055B460
          xor eax, eax
          [esi]     = 01249554
          [esi+4]   = 01249530
          [esi+24]  = 01249528
          [esi+408]…[esi+432] = 0     ; extra lists
          call 0054DF50               ; [def+196] → +408
          ret 4
```

`0054DF50` `mov ecx,[eax+196]` / `lea ecx,[ebp+408]`.
No `+364` / `+368`.

Type 38 `00558B90` (`0124B04C`, alloc `0x194`):

```
00558B98  call 0055B460
          [esi]     = 0124B04C
          [esi+4]   = 0124B024
          [esi+24]  = 0124B01C
          ret 4
```

No store after the base ctor except the three vtbls.

Activate / enable also leave the byte alone:

| Site | Maps | Writes `+368`? |
| --- | --- | --- |
| Type 11 `0054DC30` | `vtbl+12(26,31,28,27,32,29)` if parent `+545` | **no** |
| Type 38 `0055AEB0` | `vtbl+12(26,31,27,32)` — **no 28** | **no** (`0055BAE0` copies onto `+348`) |

So construct + first enable leave `[widget+368]=0`.

---

## 4. Action 28 skips `+588` while that byte is 0

`0055AD60` `ecx` = inner. Table `0x55AE88[2]` =
`0055ADDE` (`action27-release`).

```
0055ADDE  mov al, [esi+364]          ; inner+364 = widget+368
          test al, al
          je  0055AE70               ; stamp +44 → +396; 0055B9D0
          lea ecx, [esi-4]
          call [outer.vtbl+588]      ; 0-arg; ABI of 0055ACF0
          mov [esi+364], 0
          jmp 0055AE70
```

No `E8 0055ACF0`. The only recovered first-seen-family
callee with that ABI is `0055ACF0` (`0055ACF0-first-caller`).
Exact `01249554+588` / `0124B04C+588` dwords stay
**PARTIAL**. The **skip** does not need the dword: first-seen
`al==0` never issues the call.

Type 38 enable has **no** local 28 until a later
`0055AF60` `vtbl+12(28)`. Type 11 already maps 28 on
activate. Either way, a first-seen type 6 apply still
takes `je 0055AE70`.

---

## 5. Action 26 is the first recovered `=1`

```
0055AD7B  mov al, [esi+348]          ; widget+352 selected u8
          test al, al
          je  0055AE3D               ; no +584; no arm
          lea ecx, [esi-4]
          call [outer.vtbl+584]      ; 0055AF60
          [esi+396] = [esi+44]
0055ADA1  mov [esi+364], 0x01        ; arm inner+364
          call 0055B9D0
```

`0055AF60` `mov [esi+364], eax` is **outer+364** (saved
`[this+328]`), not this `u8`. It does not arm action 28.

A successful selected 26 is therefore the earliest type
11/38 write of `[widget+368]=1`. After that, a later LMB
up (type 6 / action 28) can take `+588`. That is **not**
first-seen.

Sibling keep-zero sites (not ctor; listed so they are not
confused with arm):

| VA | When | Effect on `widget+368` |
| --- | --- | --- |
| `0055ACB0` | type-34 tick; `+352==0` | if nonzero, `mov [ecx+368], dl` (`dl=0`) |
| `0055AF30` | type-34 disable | `xor al,al` / `mov [esi+368], al` then erase 28/29 |
| `0055ADF8` | action 28 after `+588` | `[inner+364]=0` |

First-seen `+352` is also ctor 0 (`type11-plus352-select`).
Until hit-test `0055C0DE` sets it, action 26 never reaches
`0055ADA1` either.

---

## 6. C# leftover (do not apply here)

| Site | Native | Host |
| --- | --- | --- |
| ctor `widget+368=0` | `0055B48B` | `FrontendWidget.Armed` default **false** **MATCH** |
| action 26 `=1` | `0055ADA1` after `+352` and `+584` | `ArmType34Widgets` sets `Armed` on every type 11/38 **LEFTOVER** (no `+352` gate) |
| action 28 skip if 0 | `je 0055AE70` | `MessageFromPlus228List` requires `Armed` **MATCH** |
| `0055ACF0` = `+588` | ABI **PARTIAL** | `Plus228PostFn = 0055ACF0` on action 28 **PARTIAL** same |

Do **not** treat ctor `+364` dword zero as the armed flag.
Do **not** treat first-seen LMB up as a `+228` post.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0055B48B` | ctor `widget+368=0` | **PROVEN** |
| `0055B485` | ctor `widget+364` dword = 0 (other field) | **PROVEN** |
| `0054E0B8` / `00558B98` | type 11 / 38 enter `0055B460` | **PROVEN** |
| `0055B4B5` | `0055B040` after the zeros | **PROVEN**; no `+368` write |
| `0055ADA1` | action 26 `[inner+364]=1` | **PROVEN** |
| `0055ADDE` | action 28 gate on that byte | **PROVEN** |
| `0055AE70` | unarmed join (no `+588`) | **PROVEN** |
| `vtbl+588` = `0055ACF0` | unique 0-arg unmap-28 + `+380` | **PARTIAL** |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0054E0B0`, `00558B90`, `0055B460`, `0055B4C0`,
  `0055BA20`, `0055B040`, `0055AD60`, `0055ADDE`,
  `0055ADA1`, `0055ACB0`, `0055AF30`, `0055AF60`,
  `0054DC30`, `0055AEB0`, `0054DF50`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
  (`0x0054E0B8` / `0x00558B98` → `0x0055B460`)
- `proofs/action27-release/README.md`
- `proofs/action28-after-26/README.md`
- `proofs/0055ACF0-first-caller/README.md`
- `proofs/type11-plus352-select/README.md`
- `proofs/0055B9D0-post-dword/README.md`
- `src/Fable.Game/FrontendInputMap.cs`
  (`Plus228PostFn`, `MessageFromPlus228List`)
