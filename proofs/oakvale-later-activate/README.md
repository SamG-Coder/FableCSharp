# Who later `00CB5AD0`s / constructs `Q_NewOakValeIntro` after no-save Leave

Investigation only. No production `src/` edits.

Do **not** start `S_QNOVI` as New Game.
Do **not** invent `ActivateQuest("Q_NewOakValeIntro")` as New Game.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Authority: `Fable.exe` complete `.text` dump
`tools/Fable.ExeIndex/out/01-sections/text-map/`
(`listing-*.txt`, `e8.tsv`, `functions.tsv`) and
`out/00-index/` (`xrefs.tsv`, `strings.tsv`).
Existing C# / docs are **not** authority.

---

## Verdict

After no-save Leave, **no first-seen site** `00CB5AD0`s or constructs
`Q_NewOakValeIntro`.

`00CB5AD0` has **one** `E8` in the whole `.text`: `004B42E8` inside
`004B4260`. First `004B4260` after Leave is `0049F24E` on
`world+172` (`Init Quests`). That vector is `AddQuest(..., TRUE)`
plus WLD initial names. `Q_NewOakValeIntro` is `AddQuest(..., FALSE)`
and `AddTestQuest` → `world+196`. It is **not** on `world+172`.

Bind `00CD6E27` / `S_QNOVI` / `00DBEF70` is factory **register**
(`00CB5C90`), not construct. `00DBEF70` and `00DABAC0` have **zero**
`E8` sites. They run only after a later `004B4260` / `004B4A10` of
this name succeeds and `004B3CE0` does `call [eax+4]` /
`call [edx+8]`.

The dump-proven **consumer of `world+196` that `004B4A10`s** is
debug test-quest UI `0061A8A0` / `0061AB30`, gated by `[this+343]`.
That is **not** New Game.

| Claim | Class |
|---|---|
| First `004B4260` (`0049F24E` `world+172`) includes this name | **DISPROVEN** |
| Bind `00CD6E27` constructs `S_QNOVI` | **DISPROVEN** |
| New Game `ActivateQuest("Q_NewOakValeIntro")` | **DISPROVEN** |
| `00DBEF70` / `00DABAC0` `E8` from Leave / Init Game | **DISPROVEN** |
| `00CB5AD0` unique `E8` = `004B42E8` in `004B4260` | **PROVEN** |
| `AddQuest FALSE` → `world+184` only; `TRUE` also `world+172` | **PROVEN** |
| `AddTestQuest` → `world+196` stride 28 | **PROVEN** |
| `world+196` activate consumer = `0061AB30` (`004B4A10` / `004B4C50`) | **PROVEN** |
| `0061AB30` is New Game / no-save Leave | **DISPROVEN** |
| `00CE7670` starts the quest | **DISPROVEN** (wait / card only) |
| Who first-seen no-save eventually `004B4A10`s this name without debug UI | **UNREAD** |

---

## Timeline (no-save Leave)

```
Init World 004A6xxx
  004A6677  00CB5D80 "Registering Scripts"
    00CB5E12  00CD52D0
      00CD6E27 bind "Q_NewOakValeIntro" / "S_QNOVI" / [rec]=00DBEF70
      00CB5C90 register into script-def map     // BIND only
004A1840 Load world
  004A0D90 FinalAlbion.qst
    AddQuest("Q_NewOakValeIntro", FALSE)
      always world+184
      TRUE only → world+172                    // FALSE: skip
    AddTestQuest(...)
      world+196 28-byte record                 // not 004B4260
[0x13B8648]==0
  00416BCA 0049F180 "Init Quests"
    0049F247 lea edx, [esi+172]
    0049F24E 004B4260                          // FIRST walk
      00CB5AD0 per name in world+172
      Q_NewOakValeIntro not in that vector
  00416BF0 world+90584
    0099E960 vs empty 0x122D70E
    je skip 00416C11 004B4A10
user.ini ActivateQuest("Gameflow")
  00419CE0 → [vtbl+1104] 00892E80 → 004B4A10("Gameflow")
00CE7670 Gameflow Main
  push "Q_NewOakValeIntro"
  [vtbl+100] wait-until-active                 // not 00CB5AD0
```

`S_QNOVI` ctor `00DAAC00` / run `00DABAC0` / `00DBDE40` are
**not** on this walk.

---

## 1. `00CB5AD0` callers

`e8.tsv`: **one** site.

| site | dest | parent |
|---|---|---|
| `004B42E8` | `00CB5AD0` | `004B4260` |

`listing-00c80000.txt` `00CB5AD0`: lookup by name in the map
filled by `00CB5C90`. Hit → `lea eax, [edi+4]` (factory record).
Miss → `xor eax, eax`. It does **not** alloc `S_QNOVI`.

`004B4260` (`listing-00480000.txt`):

```
ecx = quest manager [0x13B89FC]
arg0 = vector of CString names (begin/end at [ebp]/[ebp+4])
for each name:
  004B00C0 already-present?  je skip
  mov ecx, [edi+120]         // script-def map
  push name
  call 00CB5AD0              // 004B42E8 UNIQUE
  store 12-byte {name, factory, flags}
004B3CE0(that list)
```

`ret 12`. Later `00CB5AD0` of this name **is** a later
`004B4260` (or `004B4A10` which builds a 1-name vector and
`E8`s `004B4260` at `004B4A5A`).

### `004B4260` `E8` sites (`e8.tsv`)

| site | parent | list |
|---|---|---|
| `0049EAD1` | `0049EAC0` | `this+0xAC` (172) on **ecx**, not world |
| `0049F24E` | `0049F180` Init Quests | **`world+172`** first after Leave |
| `004B4A5A` | `004B4A10` | 1-name wrapper |
| `004B5B84` | save `START_ACTIVE_QUESTS` | load-game, not no-save |
| `00892EAF` | `00892EA0` | thunk `push 1,1` then `004B4260` |
| `00892EEF` | `00892EE0` | thunk `push 0,1` then `004B4260` |

First after Leave: **`0049F24E`**. **PROVEN**.

---

## 2. QST: `world+172` vs `world+184` vs `world+196`

`004A0D90` (`listing-00480000.txt`).

`AddQuest` (`004A0E7D` / `004A0EAB`):

- always append name to `lea esi, [ebp+184]` (`004A1080`)
- if persistent TRUE (`bl`): also `lea esi, [ebp+172]` (`004A10C4`)
- `004B2850` register def with `[0x13B89FC]`

`AddTestQuest` (`004A0E92` / `004A113B`):

- `004A16E4` `mov ecx, [ebp+200]`
- `004A16EA` `lea esi, [ebp+196]`
- grow stride **28** (`add [esi+4], 28` / `004ADB50`)

No `004B4260` / `00CB5AD0` in this parser. **PROVEN**.

`Q_NewOakValeIntro` is `AddQuest(..., FALSE)` in the master table
plus `AddTestQuest`. It lands in `world+184` and `world+196`,
**not** `world+172`. First `004B4260` therefore does not
`00CB5AD0` it. **PROVEN** from parser; the FALSE token in the
QST file itself is data, not `.text`.

---

## 3. Bind `00CD6E27` / `S_QNOVI` / `00DBEF70`

`strings.tsv`: `Q_NewOakValeIntro` `0x012C5D14`; `S_QNOVI`
`0x012F789C`.

`xrefs.tsv` / `listing-00cc0000.txt` — **five** code pushes of
`Q_NewOakValeIntro`:

| VA | function | role |
|---|---|---|
| `00CD6E27` | `00CD52D0` registrar | bind name |
| `00CD6E86` | same | bind cleanup / `00CBFAB8` |
| `00CE791D` | `00CE7670` Gameflow | card + wait |
| `00CE7977` | same | `[vtbl+100]` is-active |
| `00CE79C9` | same | loop `[vtbl+100]` |

**No** other `.text` push. **PROVEN**.

Bind body (`00CD6E0F`–`00CD6E6D`):

```
push "S_QNOVI"
00CB5AC0                 // CString dtor stub, not construct
push "Q_NewOakValeIntro"
mov [esp+32], 0xDBEF70   // factory pointer into the record
mov [esp+36], ebp        // 0xCDBD20
call 00CB5C90            // register
```

`00CB5C90` appends to `[ebx+4]` map. Same pattern as
`Q_SunnyvaleMaster` at `00CD5307` (`[esp+32]=0xCDD550`).

`00CD52D0` is called **once** from `00CB5E12` inside
`00CB5D80` (`"Registering Scripts"`). That is called from
`004A6677` while the world object is built (`lea edi, [esi+88]`
script-def holder). **Before** Init Quests. Bind ≠ start.
**PROVEN**.

---

## 4. `00DBEF70` / `00DABAC0` / `00DBDE40` `E8` sites

| dest | `e8.tsv` sites |
|---|---|
| `00DBEF70` | **0** |
| `00DABAC0` | **0** |
| `00DAAC00` | **0** (called from `00DBEF70` listing; `e8` row may omit this island) |
| `00DBDE40` | **1**: `00DAC295` |

`listing-00d80000.txt` `00DBEF70`: `push 0x10C` / `00BFEA1A` /
`00DAAC00` ctor. Factory, not a direct call from Leave.

`00DABAC0`: vtbl slot 2 of `S_QNOVI` (`0x012D7A28+8`). Registers
`NOVI_*` names then:

```
00DAC293  mov ecx, esi
00DAC295  call 00DBDE40
```

`00DBDE40` is **after** construct+run, not New Game. **PROVEN**.

Indirect construct (`listing-00480000.txt` `004B3CE0`):

```
004B3EE4  cmp [edi+4], 0          // factory from 00CB5AD0
004B3EED  mov al, [0x1375454]
004B3EF4  je 004B4063             // stub object, no 00DBEF70
004B3F0B  mov eax, [edi+4]
004B3F17  call [eax+4]            // 00DBEF70
004B3F1C  mov edx, [esi]
004B3F20  call [edx+8]            // 00DABAC0
```

`[0x1375454]` has **no** other `.text` xref in this dump
(**UNREAD** as a stored PE byte). The call is still **not** an
`E8` from Leave. Without a prior `00CB5AD0` hit for this name,
`[edi+4]` is 0 and this block is skipped (`je 004B4063` from
null factory, or the name never enters the 12-byte list).

---

## 5. `world+196` AddTestQuest consumer

Copy / clear / dtor only (not activate):

| VA | role |
|---|---|
| `004A16EA` | `AddTestQuest` append |
| `004A08D0` `004A090D` | clear on reload |
| `004A6BC7` | world dtor |

Activate consumer (`listing-00600000.txt`):

```
0061A8A0
  call 00686A80                 // eax = [[0x13B8A1C]+36] world
  add eax, 0xC4                 // +196
  00624A30 copy vector
  filter 004AF610 (already active)

00686A80  mov eax, [0x13B8A1C]
          mov eax, [eax+36]
          ret
```

`0061AB30` (same GUI object, `[edi+343]!=0`):

```
pick record at index [+344] * 28
optional Data\Levels\Ini\ + 009EC890
004B43D0 / 004B39B0
if [record+24] nonempty: 004B4C50   // holy-site / second AddTestQuest arg
else: push 1,1 ; push record ; 004B4A10   // 0061AC28
```

`call 004B4C50` is **only** `0061AC1C` in the listings.
`0061AB30` is `E8`d from `0061B59D` when `[esi+343]!=0`
(same widget). `0061B530` / `0061B560` jump to `0061AA80` /
`0061A9D0` (cycle the list) under the same byte.

This is the test-quest UI, **not** no-save Leave / `user.ini`.
**PROVEN** as the `world+196` → `004B4A10` consumer.
**DISPROVEN** as New Game.

---

## 6. `004B4A10` later sites (`e8.tsv`)

`004B4A10`: build 1-element name vector, `004B4A5A` → `004B4260`
→ unique `00CB5AD0`.

| site | after first Init Quests? | this name? |
|---|---|---|
| `00416C11` | yes, `world+90584` if nonempty | no-save empty skip (`0099E960` vs `0x122D70E`) **PROVEN** skip; contents if set **UNREAD** |
| `004B4B5F` | thing `004B4AA0` (`[thing+40]` name) | **UNREAD** as this string |
| `004B4D45` | `004B4C50` (only from `0061AC1C`) | debug UI **PROVEN** |
| `0061AC28` | test UI `0061AB30` | debug **PROVEN** |
| `007EF3A1` | action `[obj+120]` | **UNREAD** as this string |
| `0084407E` | creature `[+168]` / `[+172]` flag | **UNREAD** as this string |
| `00892E8F` | `00892E80` `push 1,1` | generic ActivateQuest |
| `00892ECF` | `00892EC0` `push 0,1` | generic |

`00892E80` is `[script-manager vtbl+1104]` from `00419CE0`.
TLC `user.ini` argument is `"Gameflow"`, not this name.
`.text` never pushes `Q_NewOakValeIntro` into `00892E80`.
Inventing `ActivateQuest("Q_NewOakValeIntro")` as New Game is
**DISPROVEN**.

A later native/script could still pass a **copied** CString
into `00892E80` / `004B4AA0` without a second exe string xref.
That is **UNREAD** for first-seen no-save. It is **not** the
Init Quests / ini / bind path.

`004B5B84` `004B4260` is save `START_ACTIVE_QUESTS`
(`004B5B54`). `004B5080` `START_NEW_QUEST` is `E8`d only from
`004B58F3` (save parser `004B5500`). **DISPROVEN** as no-save
Leave.

---

## 7. Gameflow `00CE7670` is not construct

`listing-00cc0000.txt`:

```
00CE7914  call [eax+1524]
00CE791D  push "Q_NewOakValeIntro"
00CE7930  push "OBJECT_QUEST_CARD_OAKVALE_INTRO"
00CE7957  call [edx+1180]          // card, not 004B4A10
00CE7977  push "Q_NewOakValeIntro"
00CE7995  call [edx+100]           // is-active
          yield 00CB7940 until true
```

`00892F40` is `jmp 004AF610` (is-active). That matches a wait,
not `00CB5AD0`. **DISPROVEN** as the later constructor.

---

## Classifications

1. **`00CB5AD0` unique `E8` — PROVEN.** `004B42E8` only.
2. **First `004B4260` after Leave — PROVEN.** `0049F24E`
   `world+172`. Name not on that list (`AddQuest FALSE`).
3. **Bind constructs `S_QNOVI` — DISPROVEN.** `00CB5C90` map
   insert of `00DBEF70`.
4. **New Game `ActivateQuest(Q_NewOakValeIntro)` — DISPROVEN.**
   No exe push into `00892E80`. Ini is `Gameflow`.
5. **`00DBEF70` / `00DABAC0` as Leave `E8` — DISPROVEN.** Zero
   sites. `00DBDE40` only `00DAC295`.
6. **`world+196` later `004B4A10` — PROVEN as `0061AB30`.**
   **DISPROVEN** as no-save New Game (`[+343]` test UI).
7. **First-seen no-save constructor of this name — UNREAD**
   beyond “not Init Quests, not bind, not ini, not Gameflow
   start”. Do not fill that gap with `S_QNOVI` as New Game.
