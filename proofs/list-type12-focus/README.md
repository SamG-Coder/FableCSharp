# Type-12 list focus (`005403D2`) vs action 26

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `0054C3A0` / `0053B63E` / `00540320` /
`0052C730` / `0052CF40` / `0054E280` / `0054DBC0` / `0055AD60` /
`0055CB10` / `0042E3EE`; inflated `frontend.bin` + `names.bin`;
`implementer/frontend/14-container.md`, `05-input.md`,
`persist-scan.txt`; `proofs/who-posts-15/README.md`;
`FrontendWidgetType.List` / `FirstSeenState`.

Status: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

| Claim | Class |
| --- | --- |
| Type 12 ctor `0054C3A0` calls type 8 `0053B63E` then vtbl `01249224` / inner `012491FC` | **PROVEN** |
| `00540320` is a **key** handler (`[esp+4]`). `005403D2` is the `cmp edi, 1` arm | **PROVEN** |
| Action 26 is switched on type 10 (`0054E2A2`) and type 11/38 (`0055AD66`). **No** `lea [x-26]` on the list | **PROVEN** |
| First-seen `+324/+328/+332=0` (`0052C730`). Type 12 does **not** exclusive-hide siblings | **PROVEN** |
| List highlight index is `+348` into dword vector `+356`, not `+332` | **PROVEN** |
| Main-menu list first persist child is `UI_FRONTEND_BUTTON_NEW_GAME` type 11, persist **15** | **PROVEN** |
| Press Start list only child is `UI_FRONTEND_BUTTON_INVISIBLE` type 11, persist **`0xE5` / 229** | **PROVEN** |
| Action 26 goes to `0055CB10` listeners (type 10 / type 11), **not** through `005403D2` or `+332` | **PROVEN** |
| `005403D2` first-seen posts a UI message | **DISPROVEN** (empty circular `+352`) |

**Does action 26 go to the selected list child?** Not via the list.
It is delivered to subscribed type-11 (and type-10) inner vtbls.
The list only changes which child is highlighted (`+348` / `vtbl+192`
states 3 vs 4). First-seen highlight is child **0** if `+348` starts
at 0 (**PARTIAL** — ctor zero of `+348` on type 8/12 is **UNREAD**
in this dump; type-33 ctor `0055BA20` does zero `+348`).

---

## 1. Type 12 object

```
0054C3A0  push def
          call 0053B63E          ; type 8, size 0x1FC
          mov [esi],     01249224
          mov [esi+4],   012491FC
          mov [esi+24],  012491F4
```

`vtbl+8` is `00530260` (same as type 5/10/18). Persist children
append to `+176` (`005331A0`).

`0052C730` after layout:

```
+324 = +328 = +332 = 0
+320 = -1
+340 = 1
```

`14-container.md`: type 5/10/**12** do not exclusive-select.
`+332=0` is the style key (`vtbl+192` `0052CF40`). Sibling type-11
rows stay in the `+176` walk. Type **18** is the only
`SelectsChild` case.

---

## 2. `00540320` / `005403D2` — keys, not action 26

```
00540320  edi = arg0
          cmp edi, 111          ; EngineInput.KeyMove0 0x6F
          cmp edi, 112          ; KeyMove1 0x70
          call 00404A80 / 00404C50
          test al; jne skip
005403CC  cmp edi, 1            ; DIK_ESCAPE
005403D2  mov eax, [esi+352]    ; circular list sentinel
          mov edi, [eax]
          cmp edi, eax
          je  empty
          loop:
            call 0041E5F2
            lea ecx, [edi+8]
            push ecx
            call [edx+56]       ; 0041E6D3 → UI vtbl+32 if game==0
            mov edi, [edi]
            cmp edi, [esi+352]
          jmp empty
          cmp edi, 117 / 108 / 113
005405C9  cmp edi, 28           ; DIK_RETURN; same walk on +348 list
```

`111`/`112` bump `[esi+360]` (scroll), not `+332`.

Type 10 `+352` is a stored **i32** (attach `0xE5`). Type 12 `+352`
is an STL-style circular list. First-seen sentinel `[head]==head`
→ the `005403D2` loop never calls `0041E6D3`.

`05-input.md` already names this site “List-widget `005403D2`
key==1 … (different widget)” from type-10 `0054E280`.

---

## 3. Where action 26 actually goes

`0042E3EE` type 4 → `push 26`. `0055CB10` walks listeners:
filter `vtbl+8(action)`, then `vtbl+4(action)`.

| Widget | Inner action fn | Action 26 |
| --- | --- | --- |
| Type 10 menu | `0054E280` (`lea eax,[ebx-26]`, case 0 `0054E2FA`) | `push &widget+352` → UI vtbl+32 |
| Type 11 button | `0054DBC0` → `0055AD60` (`lea eax,[edi-26]`) | debounce + `[obj+545]`; press path `vtbl+584` / `+364=1` |
| Type 38 accept | `0055AD60` | persist id `0x126` |
| Type 12 list | **none** | no `lea [x-26]` in `0054C3A0`–`0054Dxxx` |

Type 11 subscribe (`0054DC7E`): `push 26; call [eax+12]` (and 31,
28, 27, 32, 29) after `vtbl+192(3)`. Unsubscribe is `+16` with
`vtbl+192(4)`.

List navigation that *looks* like focus (`0054C59E`):

```
child = [[esi+356] + [esi+348]*4]
vtbl+192(4)   ; old
dec / wrap +348
vtbl+192(3)   ; new
```

That is the **highlight** index, not `+332`. Action 26 is not
passed as an argument into that walk.

---

## 4. Persist trees

### Press Start

`UI_FRONTEND_PRESS_START_MENU` child `#624`
`UI_FRONTEND_LIST_PRESS_START_MENU` type **12**, Children **1**
→ `#625` `UI_FRONTEND_BUTTON_INVISIBLE`.

| Widget | Type | Persist message |
| --- | ---: | ---: |
| `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 | `Action` `0xF1A22807` = **0**; `0x53C644E4` = **0** |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | `0x53C644E4` **`0xE5`**; `Action` **229** |

Hex: `E444C653 E5000000` on INVISIBLE (`persist-scan.txt` nested
`#625`). Dest is a point (`14-container.md`). Visible first-seen.

First-seen Press Start **0xE5** is still type-10 attach
`00598EE6` + `0054E4F0` + `0054E280`. INVISIBLE holds the **same**
id in persist. Whether the type-11 also fires on first-seen type 4
depends on subscribe/focus (**PARTIAL**). `005403D2` does **not**
post it first-seen.

C# leftover: `AttachFrontendTree` writes root `MessageId=0xE5`
when persist is 0. That is the type-10 attach analog, not the
list child.

### Main Menu

`UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` type 10.
List `UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` type 12.

First persist child: `UI_FRONTEND_BUTTON_NEW_GAME` type **11**,
CRC `0x53C644E4` / `Action` **15** (`who-posts-15`,
`FrontendUiDefTests`).

Root type 10 persist id is **0** (not 15). Type-10 `0054E280` is
**DISPROVEN** as the New Game poster. Type-11 `0054DBC0` is the
recovered 15 poster once that child is a listener.

Further rows in `names.bin` after NEW_GAME (LOAD / OPTIONS / …)
are siblings, not first-seen highlight 0.

---

## 5. C# vs native

| Site | Native | C# |
| --- | --- | --- |
| `FrontendWidgetType.SelectsChild` | type 18 only | **MATCH** |
| List `+332` | style 0; all type-11 children visible | factory does not hide list kids | **MATCH** |
| Action 26 | `0055CB10` → type 10/11/38 | `MessageFromWidgets` first visible 10/11/38 with `MessageId≠0` | **LEFTOVER** (no `+348` gate) |
| `005403D2` key 1 | empty `+352` first-seen | unmapped (`TryMapEvent` type 1 is null) | **MATCH** as no-op |
| INVISIBLE `0xE5` | persist | factory copies `MessageId`; root also forced `0xE5` on Press Start | **PARTIAL** (two 0xE5 sources) |

Do **not** treat `+332=0` as “selected list child” for input.
Do **not** route action 26 through `005403D2`.

---

## 6. UNREAD

- Type 8/12 ctor write of `+348` / `+356` first-seen (highlight 0
  is the working assumption, not a recovered `mov [esi+348],0`).
- Whether type 11 `0055AD60` action 26 posts immediately
  (`vtbl+584`) or only arms `+364` until action 27 / `vtbl+524`.
- Physical device that synthesizes type 4.
- CRC `0x53C644E4` field name.
