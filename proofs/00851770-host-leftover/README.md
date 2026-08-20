# Host leftover on first-seen `00851770` seed

Investigation only. No production `src/` edits.

Do **not** start Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. This is the
New Profile bind after Press Start `0xE5`,
not Leave / Init Game.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Question: `00851770` default seed. What does
native first-seen write? Does host **MATCH**
or leftover? First leftover field?

Authority: `proofs/00851770-default-seed`
(three-way `004069E0`, retail first-seen);
`Fable.exe` dump
`listing-00840000.txt` (`00851700` /
`00851770`);
`listing-00400000.txt` (`004069E0` /
`00406A20` / `00406A5D`);
`listing-00540000.txt` (`00540180` /
`005407B0`);
`listing-009c0000.txt` (`009C95E0`);
`listing-00580000.txt` (`00596917`);
`src/Fable.Game/EngineLifecycle.cs`
`BindNewProfileFromArmedTick`;
`src/Fable.Game/FrontendMessages.cs`
`State.EditName`;
`EngineLifecycleTests.Frontend_00851770_seeds_Default_then_0x126_is_0059697A_main_menu`.

Do not re-prove `0x126` / `00851920` / Leave.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Native first-seen write? | Type-37 `vtbl+572` (`00540180`): **`widget+356`** ← dest from `004069E0` retail `TEXT_GUI_PROFILE_DEFAULT` (`[0x13B871C]+96` `009C95E0`); **`+364=0`**; **`+360=len`**. Same fn also `[ui96+12]=widget`, then `+380=1`. **Not** `0x122DE80` `"Default"`. | **PROVEN** path + fields; dest letters **UNREAD** (text.big) |
| Host **MATCH** or leftover? | Bind / `00851700` `+4=+5=0` **MATCH**. Seed value **LEFTOVER**. | **PROVEN** |
| First leftover field? | **`FrontendEditBoxName`** (`"Default"` / `0x122DE80`). Maps to native **`widget+356`**. | **PROVEN** leftover, still present |

---

## Verdict

**LEFTOVER. First leftover field: `FrontendEditBoxName`.**

Native first-seen `00851770` seeds the type-37
box through `004069E0` **retail** arm
(`[0x13B86A0]==0`, `[0x13B871C]≠0`) into
`vtbl+572` → `00540180`:

```
widget+356  CString  TEXT_GUI_PROFILE_DEFAULT payload
widget+364  0
widget+360  wchar length
```

`0x122DE80` is only the **both-null** arm
(`proofs/00851770-default-seed`). First-seen
does not take it.

Host `BindNewProfileFromArmedTick` still does

```
FrontendEditBoxName = FrontendProfileDefaultFallback;  // "Default"
```

and asserts that string. `FrontendMessages.State.EditName`
defaults to the same leftover. The Note line
`004069E0 [0x13B86A0]=0 TEXT_GUI_PROFILE_DEFAULT else 0x122DE80 Default`
is leftover commentary: first-seen never reaches
the `else`.

Host leftover **side effect** is that assign.
`+4` / `+5` / bind name / type 37 are not it.

Return this leftover only: **`FrontendEditBoxName`**.

---

## 1. Native first-seen stores (`00851770`)

`00596917` (`listing-00580000.txt`) allocs 16,
`00851700` (`+4=0`, `+5=0`, `+12=0`, `+8=menu`),
`[ui+96]=obj`, then `00851770`.

`00851770` (`listing-00840000.txt`):

```
008517B0  mov [esi+12], edi          ; ui96+12 = type-37 widget
008517B3  mov edi, [edi]             ; widget vtbl
008517B5  push eax                   ; dest local
008517B6  call 0040D2A0
008517BB  mov ecx, eax
008517BD  call 004069E0              ; fill dest; ret dest
008517C2  mov ecx, [esi+12]
008517C5  push eax
008517C6  call [edi+572]             ; 00540180
…
0085180A  mov [eax+380], 0x01
00851834  push 33
00851842  push 34
0085184C  push 1
0085184E  call [eax+600]
```

`00540180` (`listing-00540000.txt`) is the
type-37 string assign (ctor plants vtbl
`0x1246B8C`; this is the `0099B7D0` into
`+356` `ret 4`):

```
00540182  mov edi, [esp+12]          ; dest
00540189  lea ecx, [esi+356]
0054018F  mov [esi+364], 0x0
00540199  call 0099B7D0              ; +356 ← dest
005401A0  call 0099B220
005401A6  mov [esi+360], eax         ; length
```

Ctor `005407B0` already zeroed `+356` /
`+360` / `+364` / **`+380`**. First-seen
`00851770` then overwrites the string triple
and sets **`+380=1`**.

`004069E0` first-seen dest
(`00851770-default-seed` §2–3):

| Global | First-seen | Arm |
|---|---|---|
| `[0x13B86A0]` | 0 | skip game `+20` |
| `[0x13B871C]` | retail, `+96` loaded | **`TEXT_GUI_PROFILE_DEFAULT` `009C95E0`** |

Hit: `009C95E0` `0099B720` from `[entry+40]`
into dest. Miss: `[0x13BCA24]`, **not**
`0x122DE80`. Both-null `push 0x122DE80` is
**DISPROVEN** on this tick.

Dest letters live in `TEXT_ENGLISH_MAIN`
(`lang/English/text.big`). Exe dump does not
contain them. **UNREAD** here. Do not treat
host `"Default"` as a proven payload.

---

## 2. Host site (`BindNewProfileFromArmedTick`)

`EngineLifecycle` ~3655:

```
Note(00851700, "+4=0 +5=0");
Note(00851770, "UI_NEW_PROFILE_EDIT_BOX type 37");
Note(004069E0, "[0x13B86A0]=0 TEXT_GUI_PROFILE_DEFAULT
                 else 0x122DE80 Default");
FrontendMenuRoot = UI_FRONTEND_NEW_PROFILE_SCREEN;
FrontendUi96Present = true;
FrontendUi96Accept = false;     // +4
FrontendUi96Armed = false;      // +5
FrontendEditBoxBound = true;    // +12 set
FrontendEditBoxName = "Default";
```

`FrontendMessages.State.EditName` starts as
`"Default"` and `ApplyTick` re-seeds that
fallback if empty.

| Host field | Native first-seen | Class |
|---|---|---|
| `FrontendMenuRoot` New Profile | `00596763` slot `0x17` | **MATCH** |
| `FrontendUi96Present` | `[ui+96]` after `00851700` | **MATCH** |
| `FrontendUi96Accept=false` | `+4=0` | **MATCH** |
| `FrontendUi96Armed=false` | `+5=0` | **MATCH** |
| `FrontendEditBoxBound` | `[ui96+12]=widget` | **MATCH** |
| **`FrontendEditBoxName="Default"`** | **`+356` = bank dest, not `0x122DE80`** | **LEFTOVER** |
| (none) | `+364=0`, `+360=len` | host **omit** (not leftover) |
| (none) | `+380=1` | host **omit** |
| Note `else 0x122DE80 Default` | both-null only | leftover **comment** |

Tests lock the leftover:
`Frontend_00851770_seeds_Default_then_0x126_…`
and `Frontend_type4_posts_stored_0xE5_…`
`Assert.Equal("Default", FrontendEditBoxName)`.

`docs/PARITY.md` / `FORWARD_TREE` §4 still say
`[0x13B86A0]=0 → 0x122DE80 "Default"`. That
doc leftover tracks the host field.

---

## 3. What this does **not** say

- English on-screen word is Default.
  **UNREAD** (text.big). Coincidence with
  `"Default"` would still be the **wrong
  source**.
- Host leftover is `+4` / `+5` / actions
  33/34. **DISPROVEN** (those **MATCH**).
- First leftover field is `widget+380`.
  **DISPROVEN** (host does not write it;
  leftover is the seed **value** it *does*
  write).
- `004069E0` game arm runs first-seen.
  **DISPROVEN**.
- `00851770` posts `0x126`. **DISPROVEN**.
