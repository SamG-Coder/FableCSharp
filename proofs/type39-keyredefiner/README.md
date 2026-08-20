# Type 39 `CKeyRedefiner` (`00557AF0` / `0124ADBC`) — first construct after PE

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listing
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041D21B` / `0041D37C` / `0041DB1D` / `0042EA62`);
`listing-00540000.txt` (`00558540` / `005585D0` / `00558660` /
`00557AF0` / `0055B460`);
`listing-00580000.txt` (`00598A1C` / `00598CA8` / `00598F4E` /
`0058CA1E`);
`e8.tsv`; `00-index/rtti.txt` / `strings.tsv` / `xrefs.tsv`;
inflated `frontend.bin` + `names.bin` (`FrontendUiDef.TypeCrc`
`0x0DA8270B` → `[def+60]`);
`implementer/frontend/17-press-start-frame.txt`;
`implementer/frontend/persist-scan.txt`;
`FrontendUiDefTests.Factory_builds_press_start_then_main_menu_from_the_same_walk`;
`proofs/00557AF0-caller/README.md`;
`proofs/00598A1C-only-e5/README.md`;
`proofs/type7-action35/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not treat `00557AF0` as the ctor. Do not re-prove type 4 →
action 26 or `TEXT_GUI_PRESS_CONTROL` as Press Start.

---

## Verdict

**Ctor is `00558540`, not `00557AF0`.** `0124ADBC` is the outer
vtbl written by that ctor. The only `.text` `E8` of `00558540`
is the type-**39** arm of `0041D21B` (`0041D392`).

First construct after PE is **not** a user click on Options.
It is the first `0041DB1D` of a tree whose persist `Type` is 39.
That factory sits **inside the same first `00598A1C`** that
builds Press Start (after AVI, `0042EA62` arg 0).

Order inside that one call:

| Slot | Root | `0041DB1D` site | Type 39 in that persist tree? |
| ---: | --- | --- | --- |
| `0x14` | `UI_FRONTEND_PRESS_START_MENU` | `00598BD2` | **DISPROVEN** |
| `0x1` | `UI_FRONTEND_OPTIONS_MENU` | `00598CD6` | **DISPROVEN** as remapper (buttons / sliders) |
| `0x16` (22) | `UI_FRONTEND_SCREEN_REDEFINE_KEYS_PC` | `00598F7C` | **PROVEN** home (controls remapper) |
| `0x17` | `UI_FRONTEND_NEW_PROFILE_SCREEN` | `00598FFE` | **DISPROVEN** |

So: **not** the Press Start / New Profile / Main Menu *trees*.
**Yes** the Options / controls *family*, specifically the
**Redefine Keys** screen (slot `0x16` = `00595B24` menu id
**22** `UI_TEXT_REDEFINE_KEYS_MENU_TITLE`), not the Options
list itself.

Native **pre-builds** that slot during first `00598A1C`,
**before** New Profile factory and **before** Main Menu
`0059697A`. The widget exists while Press Start is on screen.
`00557AF0` (capture / `TEXT_GUI_PRESS_CONTROL`) is a later
vtbl call on that live object, not construction.

| Claim | Status |
| --- | --- |
| Type 39 factory arm is `0041D37C` `push 0x1C0` / `E8 00558540` | **PROVEN** |
| Ctor writes outer vtbl `0124ADBC`, inner `0124AD98`, `+24` `0124AD90` | **PROVEN** |
| RTTI `0137C444` `CKeyRedefiner@NUISystem` | **PROVEN** |
| `00557AF0` is the ctor | **DISPROVEN** (0-arg wrap of `0055ACF0`) |
| Any other `.text` `E8` of `00558540` | **DISPROVEN** (`e8.tsv` only `0041D392`) |
| First-seen Press Start / New Profile / Main Menu persist trees contain type 39 | **DISPROVEN** |
| Options list `UI_FRONTEND_OPTIONS_MENU` is the remapper class | **DISPROVEN** (`UI_OPTIONS_BUTTON_REDEFINE_KEYS`) |
| Remapper screen is `UI_FRONTEND_SCREEN_REDEFINE_KEYS_PC` slot `0x16` | **PROVEN** attach |
| First type-39 ctor after PE is that `00598F7C` `0041DB1D` (child walk) | **PROVEN** site; exact child Type i32 list **PARTIAL** until `Dump.csx` |
| User must open Options / Redefine Keys before the object exists | **DISPROVEN** (prebuilt in `00598A1C`) |
| In-game `0058AF10` / `PC_UI_REDEFINER_LIST` is first construct | **DISPROVEN** (after Leave frontend) |

---

## 1. Dump type 39 ctor `00558540`

`0041D21B` (`listing-00400000.txt`):

```
0041D223  mov edi, [ebp+8]          ; def name / handle
0041D241  call 0042AEDA             ; resolve CUIDef
0041D249  mov eax, [ebx+60]         ; persist Type
0041D24C  cmp eax, 43
0041D24F  ja  0041D7A7
0041D255  jmp [0x41D7F8+eax*4]
```

Type 38 then type 39 then type 40:

```
0041D35C  push 0x194
          call 00BFEA1A
          call 00558B90             ; type 38 Accept
0041D37C  push 0x1C0
0041D381  call 00BFEA1A
0041D392  call 00558540             ; type 39
0041D39C  push 0x190
          call 00556350             ; type 40
```

Ctor (`listing-00540000.txt`):

```
00558540  mov eax, [esp+4]          ; def
00558547  mov esi, ecx
00558549  call 0055B460             ; type 34 base
0055854E  xor edi, edi
00558550  mov [esi],     0x124ADBC  ; outer vtbl
00558556  mov [esi+4],   0x124AD98  ; inner
0055855D  mov [esi+24],  0x124AD90
00558564  mov [esi+404], edi
0055856A  mov [esi+408], edi        ; text child later
          push 12
          call 00BFEA0E             ; list head @ +412
          ; +416…+444 = 0
005585BF  ret 4
```

`0055B460` is type 34: `0055BA20` then vtbl `0124BD2C`, zeros
`+364…+392`, `0055B040` copies persist `+224` / `+228`.

Sibling `005585D0` is the same stores via `0055B4C0` (copy /
second ctor). `e8.tsv` dest `0x005585D0`: **empty**.

Dtor `00558660` restores the same three vtbls, clears
`13B8AC8` if it owns the remapper singleton, frees `+412`,
`jmp 0055B760`.

`00557AF0` is **not** this:

```
00557AF0  mov esi, ecx
00557AF4  call 0055ACF0             ; post [this+380]
          ; maybe [0x13B8AC8]=this
          ; TEXT_GUI_PRESS_CONTROL on [+408]
          ; subscribe 33,26,27,35,38–42
          ret                       ; 0-arg
```

That is a live type-39 vtbl method. Construction already
happened.

`FrontendWidgetType.Table[39]` = ctor `00558540`, size
`0x1C0`. Vtbl column is still 0 in that table (**PARTIAL**
vs listing `0124ADBC`).

---

## 2. Factory after PE: `00598A1C`, not a later screen click

`0042E98F` bind after AVI (`listing-00400000.txt`):

```
0042EA62  call 00598A1C             ; first-seen arg 0
```

`00598A1C` (`listing-00580000.txt`) factories many roots into
the UI slot table via `0041E5F2` + `0041DB1D`. `0041DB1D`:

```
0041DB41  call 009AD410             ; lookup def
0041DB49  call 0041D21B             ; type switch / ctor
0041DB57  call [eax+332]            ; 005331A0 child walk
```

Child walk is persist `Children` (`0x3DC30C85`). Each child
with `[def+60]==39` hits `00558540`.

Relevant factories in **that first call**, in order:

```
00598BA2  "UI_FRONTEND_PRESS_START_MENU"     slot 0x14
00598BD2  call 0041DB1D

00598CA8  "UI_FRONTEND_OPTIONS_MENU"         slot 0x1
00598CD6  call 0041DB1D

00598F4E  "UI_FRONTEND_SCREEN_REDEFINE_KEYS_PC"  slot 0x16
00598F7C  call 0041DB1D

00598FD0  "UI_FRONTEND_NEW_PROFILE_SCREEN"   slot 0x17
00598FFE  call 0041DB1D
```

`00595B24` label table (Main Menu registrar, later):

| Label | id |
| --- | ---: |
| `UI_TEXT_OPTIONS_MENU_TITLE` | 24 then 1 |
| `UI_TEXT_REDEFINE_KEYS_MENU_TITLE` | **22** (`0x16`) |

Slot `0x16` **is** the Redefine Keys / controls screen.
Showing it later does **not** re-run `00558540` for the
template already built at `00598F7C`.

Main Menu root `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`
is **`0059697A` / `00595A06`**, after New Profile accept
`0x126`. That is **after** type 39 already exists.

In-game pause options `0058AF10` (`0062C797` only `E8`) looks
up `PC_UI_REDEFINER_LIST` at `0058CA1E` and `call 00556570`.
That is post-Leave, **DISPROVEN** as first construct.

---

## 3. `frontend.bin` type 39 defs

Persist Type is CRC `0x0DA8270B` then i32 → `[def+60]`.
`FrontendUiDef.TryParse` is the reader. Run:

```
dotnet script proofs/type39-keyredefiner/Dump.csx
```

### Trees that do **not** contain type 39 (**PROVEN**)

`FrontendWidgetFactory.Build` + Press Start frame:

| Root | Observed types |
| --- | --- |
| `UI_FRONTEND_PRESS_START_MENU` | 10, 5, 18, 0, 6, 12, 11, 32 |
| `UI_FRONTEND_NEW_PROFILE_SCREEN` | 10, 6, 12, 38, … |
| `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` | 10, 12, 11, 6 |

`17-press-start-frame.txt`: no type 39 row. Prompt is
`TEXT_GUI_MENU_PRESS_BUTTON`, not `TEXT_GUI_PRESS_CONTROL`.

### Options list vs controls remapper

`names.bin` / `persist-scan.txt` (frontend UI names):

| Name | Role |
| --- | --- |
| `UI_FRONTEND_OPTIONS_MENU` | slot `0x1` root (`00598CA8`) |
| `UI_FRONTEND_LIST_OPTIONS_MENU` | list |
| `UI_OPTIONS_BUTTON_REDEFINE_KEYS` | list entry that **opens** slot `0x16` |
| `UI_REDEFINE_BUTTON` / `UI_OPTIONS_BUTTON_REDEFINE_KEYS_TEXT` | chrome / label |
| `UI_FRONTEND_SCREEN_REDEFINE_KEYS_PC` | slot `0x16` root (`00598F4E`) |
| `UI_FRONTEND_LIST_REDEFINE_KEYS_MENU` | remapper list |
| `UI_KEY_REDEFINER_BASE` | row template — type **39** candidate |
| `UI_KEY_REDEFINER_ACTION_TEXT` | `TEXT_GUI_ACTION_*` child |
| `UI_KEY_REDEFINER_KEY_TEXT` | bound-key child (`[+408]` in `00557AF0`) |
| `UI_REDEFINER_MOUSE_AREA` | hit target |
| `UI_HELPERS_REDEFINE` | helpers group |
| `PC_UI_REDEFINER_LIST` | in-game / `0058AF10` sibling |
| `UI_OPTIONS_REDEFINE_TABLES` | table chrome |
| `UI_UNDEFINED_CONTROLS_WARNING` | warning, not the redefiner class |

`UI_KEY_REDEFINER_BASE` is the only persist name that matches
the RTTI class. Exact `Type==39` i32 on that entry (and
whether any other UI name also stores 39) is **PARTIAL**
until `Dump.csx` is run against TLC `frontend.bin`.

`UI_FRONTEND_OPTIONS_MENU` children are the Options *list*
(game / video / redefine / audio / …). That tree is
**DISPROVEN** as `CKeyRedefiner`. It is still factory'd
**before** slot `0x16` in the same `00598A1C`.

---

## 4. Answer

**When first constructed after PE?** After AVI, inside first
`00598A1C` (`0042EA62`), at `00598F7C` `0041DB1D`
`UI_FRONTEND_SCREEN_REDEFINE_KEYS_PC`. The first
`[def+60]==39` child of that walk calls `00558540` and
stores `0124ADBC`.

**Press Start / New Profile / Main Menu?** Those persist
trees: **never**. Same `00598A1C` also builds Press Start
**earlier**, but that walk has no type 39.

**Options / controls?** Controls / Redefine Keys screen
**yes**. Options list **no** (it only has the button that
selects slot `0x16`). The object is already constructed
before the user can open either screen.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `00558540` | type 39 ctor | **PROVEN** |
| `0124ADBC` | outer vtbl | **PROVEN** |
| `00557AF0` | capture / remapper arm | **PROVEN** method; **DISPROVEN** ctor |
| `0041D37C` / `0041D392` | factory type 39 | **PROVEN** |
| `0041DB1D` | lookup + ctor + `+332` children | **PROVEN** |
| `00598A1C` | first frontend slot fill | **PROVEN** |
| `00598F7C` | first redefine-keys factory | **PROVEN** |
| `00598CD6` | Options list factory | **PROVEN**; not type 39 |
| `0x13B8AC8` | remapper singleton | **PROVEN** (set later in `00557AF0`) |
| `UI_KEY_REDEFINER_BASE` Type i32 | 39 | **PARTIAL** (`Dump.csx`) |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
  (`0041D21B`, `0041D37C`, `0041DB1D`, `0042EA62`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`00558540`, `005585D0`, `00558660`, `00557AF0`, `0055B460`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
  (`00598A1C`, `00598CA8`, `00598F4E`, `0058CA1E`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
- `tools/Fable.ExeIndex/out/00-index/rtti.txt` /
  `strings.tsv` / `xrefs.tsv`
- `src/Fable.Formats/Defs/FrontendWidgetType.cs`
- `src/Fable.Formats/Defs/FrontendUiDef.cs`
- `implementer/frontend/17-press-start-frame.txt`
- `implementer/frontend/persist-scan.txt`
- `proofs/00557AF0-caller/README.md`
- `proofs/00598A1C-only-e5/README.md`
- `proofs/type39-keyredefiner/Dump.csx`
