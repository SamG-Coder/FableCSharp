# First `0055ACF0` / `+380` post of `0x126` after New Profile attach

Investigation only. No production `src/` edits.

Authority: dump `Fable.exe` `0055AD60` / `0055ACF0` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055AE88` / `0055AF60` / `0055B040` / `0055B460` / `00558B90` /
`0055AEB0` / `0055C0DE` / `0055BF10` / `00558DE0` / `0055A660` /
`00557AF0`); `e8.tsv`;
`listing-00400000.txt` (`0042E3EE` / `0041E6D3`);
`listing-00580000.txt` (`0059A238` / `00599D5C`);
`UI_ACCEPT_NEW_PROFILE` file persist (`FrontendUiDefTests`);
`proofs/plus224-payloads/README.md`,
`proofs/0055A726-plus228-jmp/README.md`,
`proofs/type11-plus352-select/README.md`,
`proofs/action27-release/README.md`,
`proofs/type6-action28/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE** / **LEFTOVER**.

Do not re-prove CRC `0x230364D6` → def `+224`, `0x53C644E4` →
def `+228`. Do not invent a DIK for type 4 / 6 / 13. Return
(DIK 28) is type 1 / action 33, **not** this hop.

`.rdata` vtbl dwords were **not** printed this pass. Slot
**bodies** are from ABI + unique `.text`. Slot **pointers** stay
**PARTIAL**.

---

## Verdict

**Attach after `0xE5` does not post `0x126`.** `UI_ACCEPT_NEW_PROFILE`
(type 38) already holds `0x126` on the **`+380`** list
(`[def+228]`). Action 26 on that widget posts the **empty**
`+372` list (`[def+224]==0`) and **never** loads `+380`.
The first body that can deliver `0x126` is **`0055ACF0`**.

| Claim | Status |
| --- | --- |
| After `0xE5` → `00599D5C` / slot `0x17`, Accept ctor copies `0x126` onto `widget+380` | **PROVEN** |
| That attach / `00851770` / type-38 `00558C70` calls `0055ACF0` | **DISPROVEN** |
| Action 26 `0055AD7B` → `vtbl+584` / `0055AF60` posts `[+372]` | **PROVEN** |
| On Accept that `[+372]` is **NULL** (`+224==0`) → no `0059A238` | **PROVEN** |
| `0055ACF0` `push [this+380]` / `vtbl+524` is the `+228` / `0x126` poster | **PROVEN** |
| Hover action 27 `0055AE01` calls `vtbl+592`, not `0055ACF0` / not `+524` | **PROVEN** |
| Type-11/38 `+352` is the selected **u8 gate**, not the id | **PROVEN** |
| Ctor / attach writes Accept `+352=1` | **DISPROVEN** (ctor **0**; only `0055C0DE`) |
| First `0055ACF0` on this tree is type-35 `0055A660` / type-39 `00557AF0` | **DISPROVEN** (no type 35/39 on New Profile) |
| Type-34/38 `vtbl+588` body is **`0055ACF0`** | **PARTIAL** (unique 0-arg unmap-28 + `+380`; no rdata) |
| Action 28 `0055ADDE` is `call [outer.vtbl+588]` if `[inner+364]` armed | **PROVEN** |
| First `0x126` is therefore **later** `0055ACF0` after a **selected** action 26 | **PARTIAL** (needs the `+588` dword) |

**Trigger:** not hover. Not “Accept selected `+352`” as the
poster (that byte only **allows** action 26 to enter
`0055AF60`). The recovered chain is:

```
pointer take-selection → widget+352 = 1          ; 0055C0DE
type 4 → action 26 → 0055AF60([+372]==0)         ; empty; map 28; arm
type 6 → action 28 → vtbl+588 → 0055ACF0([+380]) ; 0x126
```

Type 4 is LMB **down**. Type 6 is LMB **up**. Not a keyboard
DIK.

---

## 1. Dump `0055ACF0` — the `+380` poster

0-arg (`ret`). `ecx` = **outer** widget.

```
0055ACF0  push esi
0055ACF1  mov esi, ecx
0055ACF3  mov ecx, [esi+364]      ; outer+364 (saved select-state)
0055ACF9  mov eax, [esi]
0055ACFB  push ecx
0055ACFC  mov ecx, esi
0055ACFE  call [eax+192]          ; SelectState
0055AD04  mov edx, [esi+4]
0055AD07  lea ecx, [esi+4]
0055AD0A  push 28
0055AD0C  call [edx+16]           ; unmap local action 28
0055AD0F  mov ecx, [esi+380]
0055AD15  mov eax, [esi]
0055AD17  push ecx
0055AD18  mov ecx, esi
0055AD1A  call [eax+524]          ; walk +380
0055AD20  pop esi
0055AD21  ret
```

Pair with click `0055AF60` (`listing` `0055AF60`…`0055AFCA`):

| Body | List pushed | Local 28 | Typical slot |
| --- | --- | --- | --- |
| `0055AF60` | `[this+372]` (`[def+224]`) | inner **`vtbl+12(28)`** map | type-34 **`+584`** |
| `0055ACF0` | `[this+380]` (`[def+228]`) | inner **`vtbl+16(28)`** unmap | type-34 **`+588`** (**PARTIAL**) |

`0055AF60` never loads `+380`. `0055ACF0` never loads `+372`.

`vtbl+524` walker is `00558DE0` (1-arg; NULL head `je` empty).
Frontend `[0x13B86A0]==0` → `0041E6D3` → UI `vtbl+32`
`0059A238`. Rdata dword **PARTIAL**.

---

## 2. Dump `0055AD60` — 26 / 27 / 28

`ecx` = **inner** (`widget+4`). `lea eax,[edi-26]` /
`cmp eax,6` / `jmp [0x55AE88+eax*4]`. Table dwords
(`action27-release`; **not** code order):

| `eax` | Action | Dest | Outer call |
| ---: | ---: | --- | --- |
| 0 | **26** | `0055AD7B` | `vtbl+584` then `[inner+364]=1` |
| 1 | **27** | `0055AE01` | `vtbl+592`; `[inner+384]=1` |
| 2 | **28** | `0055ADDE` | if armed: **`vtbl+588`**, `[inner+364]=0` |

```
0055AD7B  mov al, [esi+348]       ; widget+352 u8
          test al, al
          je  0055AE3D            ; no +584
          lea ecx, [esi-4]
          call [eax+584]          ; 0055AF60
          [esi+364] = 1
          call 0055B9D0           ; nop for 26

0055AE01  test [esi+348]          ; same +352 gate
          je  0055AE70
          call [outer.vtbl+592]   ; hover-in; no +524
          [esi+384] = 1

0055ADDE  test [esi+364]          ; armed by 26, not +352
          je  0055AE70
          call [outer.vtbl+588]
          [esi+364] = 0
```

Zero `E8 0055ACF0` / `E8 0055AF60` / `E8 0059A238` in
`0055AD60`. Case 26 does **not** jump to `0055ACF0`.

`0042E3EE`: type **4** → `push 26`; type **6** → `push 28`;
type **10** (RMB) → `push 27`. Do not map Return here.

---

## 3. `UI_ACCEPT_NEW_PROFILE` stores `0x126` on `+380`

Type 38 ctor `00558B90`:

```
00558B98  call 0055B460           ; type 34; +352=0; +372/+380=0
          [esi]    = 0124B04C
          [esi+4]  = 0124B024
          [esi+24] = 0124B01C
          ret 4
```

`0055B040` first arm `[def+224]` → `vtbl+284` → list **`+372`**.
Second arm `[def+228]` → `vtbl+320` → list **`+380`**
(`0055B5B0`). File (`plus224-payloads`):

| Widget | `0x230364D6` (`+224`) | `0x53C644E4` (`+228`) |
| --- | ---: | ---: |
| `UI_ACCEPT_NEW_PROFILE` | **0** | **`0x126`** |

So first-seen Accept: `+372 == 0`, `+380` holds boxed `0x126`.
Action 26 `push [+372]` is the empty walker. `0055ACF0`
`push [+380]` is the `0x126` walker.

`type6-action28` “action 28 posts no UI message” is **STALE**
if `+588` is `0055ACF0`: that body **does** `vtbl+524([+380])`.

---

## 4. After `0xE5` attach: no `0055ACF0`

`0xE5` → `0059A238` → `00599D5C` → first-seen empty
`005955AB` → `00596917` slot `0x17`
`UI_FRONTEND_NEW_PROFILE_SCREEN`. `00851770` binds the type-37
edit box and seeds `"Default"`. It does **not** post `0x126`
(`type38-msg126`).

Type-38 select `00558C70` is `0052CF40` then maybe
`inner.vtbl+12(25)`. No `0055ACF0`. Enable `0055AEB0`
subscribes **26 / 31 / 27 / 32** — **not 28**.

`.text` callers of `0055ACF0` (`e8.tsv` + listing):

| Site | Owner | On New Profile? |
| --- | --- | --- |
| `E8 00557AF4` | type-39 `00557AF0` (`CKeyRedefiner`) | **no** |
| `jmp 0055A726` / `0055A73B` | type-35 `0055A660` (`vtbl+588` wrap) | **no** |

New Profile recovered types: 10 root, 12 menu, 37 edit, 38
accept, 11 helpers (`0055A726-plus228-jmp`). No 35 / 39 / 41.

Attach therefore **constructs** the `+380` list and does **not**
walk it.

---

## 5. Hover? **No**

Action 27 is `0055AE01` → `vtbl+592` → `[inner+384]=1`.
Sibling hover-out is action 29 → `vtbl+596`. Hover click-shape
`0055AFD0` posts **`+392`** / `[def+236]`, then maps **29**.
None of those push `+380` or enter `0055ACF0`.

---

## 6. Accept selected `+352`? Gate only

`[inner+348]` / `widget+352` on type 38 is a **u8**. Ctor
`0055BA20` writes **0**. Persist does not touch it. The only
`mov [esi+352], 1` in this listing is `0055C0DE` inside
0-arg `0055BF10` (hit-test / take selection; vtbl slot
**PARTIAL**).

If the byte is still 0:

- action 26 skips `vtbl+584` → no empty `+372` post, **no**
  local map of 28, **no** `[inner+364]=1`
- action 28 then takes `je 0055AE70` → **no** `vtbl+588`

So `+352=1` is required **before** the click/unclick pair.
It is **not** the stored id (type-10 `0054E4F0` packet* is a
different object). First-seen attach does **not** arm it
(`type11-plus352-select`).

Whether a type-13 / action-25 pointer tick has already run
`0055BF10` on Accept by the first type 4 is **UNREAD**.

---

## 7. Action 26 empty `+372`, then later `0055ACF0`

Yes, that is the only recovered **Accept** path that can
reach `0055ACF0`.

1. `+352 ≠ 0`.
2. Type 4 / `0055CB10(26)` / type-38 `0055AD60` case 0 /
   `vtbl+584` = `0055AF60`:
   - `SelectState([def+524])`
   - `vtbl+524([+372])` → **empty**
   - `inner.vtbl+12(28)` — 28 becomes locally mapped
   - `[inner+364]=1`
3. Type 6 / `0055CB10(28)` / case 2 / `vtbl+588`:
   - if that slot is `0055ACF0` (ABI unique; rdata **PARTIAL**):
     `vtbl+524([+380])` → **`0x126`** → `00851920`

Enable never inserts 28. The **only** first-seen map of 28 on
Accept is step 2. A lone type 6 before a selected 26 cannot
unarm and cannot post.

`0124B04C+588` / `0124BD2C+588` should read `0055ACF0` (or a
5-byte `jmp`). Type 35 already wraps that body as `+588`
(`0055A660`). **PARTIAL** until the dword is printed.

---

## 8. C# leftover (do not apply here)

`FrontendInputMap.MessageFromWidgets` posts factory
`MessageId` (`+228` / `0x126`) on action **26**. Native action
26 on Accept posts **nothing**. The native id rides action
**28** / `0055ACF0` after the selected click.

Do **not** invent Return / Enter / a screen-name fork. Do
**not** store the id at C# offset 352.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0055ACF0` | 0-arg; unmap 28; `vtbl+524([+380])` | **PROVEN** poster |
| `0055AD7B` | action 26: `+352` gate then `+584` | **PROVEN** |
| `0055AF60` | `+584` body; posts empty `+372` on Accept | **PROVEN** |
| `0055AE01` | action 27 hover-in `+592` | **PROVEN**; **DISPROVEN** poster |
| `0055ADDE` | action 28 → `+588` if armed | **PROVEN** |
| `0055ACF0` = type-38 `+588` | — | **PARTIAL** |
| `00558B90` / `0055B040` | ctor; `0x126` onto `+380` | **PROVEN** |
| `0055C0DE` | only `+352=1` | **PROVEN** store; slot **PARTIAL** |
| `00557AF0` / `0055A660` | other `0055ACF0` entries | **DISPROVEN** on this tree |
| Attach-time `0x126` post | — | **DISPROVEN** |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0055ACF0`, `0055AD60`, `0055AE88`, `0055AF60`, `0055B040`,
  `0055B460`, `00558B90`, `00558C70`, `0055AEB0`, `0055BF10`,
  `0055C0DE`, `00558DE0`, `0055A660`, `00557AF0`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
  (`00557AF4` → `0055ACF0` only)
- `listing-00400000.txt` (`0042E3EE` type 4 / 6)
- `listing-00580000.txt` (`0059A238` / `00851920` consume)
- `proofs/plus224-payloads/README.md`
- `proofs/0055A726-plus228-jmp/README.md`
- `proofs/00557AF0-caller/README.md`
- `proofs/type11-plus352-select/README.md`
- `proofs/action27-release/README.md`
- `proofs/type6-action28/README.md` (action-28 “no post” **STALE**)
- `proofs/type38-msg126/README.md`
- `tests/Fable.Formats.Tests/FrontendUiDefTests.cs`
