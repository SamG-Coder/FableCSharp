# `00456A5A` persist of compiled `EXPRESSION+120` (not Oakvale)

Investigation only. No production `src/` or `tests/` edits.

Question: `00456A5A` is persist load of nested
`CTCExpression` / `EXPRESSION` `+120` via `0045228F`.
Recover enclosing function start/end, record size
(`add esi, 0x8C`), field `+120` type, whether first-seen
New Game persist ever writes `Q_NewOakValeIntro` here,
and who first **writes** `+120` at runtime after Lookout
(not persist parse). Next unread site for the Oakvale
activator name source.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** invent WASD / dest numbers / physics.
Do **not** wire `007EF200` as Oakvale.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: ExeIndex
`listing-00440000.txt` `00456A5A` / `0045228F` /
`00456903` / `00456964` / `004569A1` / `004569A7` /
`00456AD6` / `0045D637` / `0045D70B` / `0044D6C5`;
`listing-00400000.txt` `00404500` / `0044FC00` /
`00431061` / `00431102` / `0043314A` / `00431020` /
`00431143`;
`listing-007c0000.txt` `007EF070` / `007EF200` /
`007EF3A1`;
`functions.tsv` `0x00456903`;
`e8.tsv` dest `0x004569A7` **0** rows;
`vtbl.tsv` `0x01233D1C` slot 18 / `0x012401F4` /
`0x0124026C`;
`rtti.txt` `CExpressionDef` `0x01376DCC` /
`CTCExpression` `0x0137A424`;
`strings.tsv` / `xrefs.tsv` `0x012C5D14`;
`compiled-defs/game/entries.tsv` `EXPRESSION` (39) /
`names.tsv` (no `Q_NewOakValeIntro`);
TLC `game.bin` inflate via `tools/Fable.Dump bin`;
`src/Fable.Game/EngineLifecycle.cs`
`PrepareDefinitionManager`;
siblings `proofs/007EF200-first-plus120`,
`proofs/007EEF60-activate`,
`proofs/ctcexpression-quest-names`,
`proofs/q-novi-activator-callers`,
`proofs/cactivatequestdef-payloads`,
`proofs/0044C72B-compile`.

There is **no** `listing-00450000.txt`. The site lives
in `listing-00440000.txt` (`0x00440000`–`0x0047FFFF`).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Enclosing function? | **`004569A7`–`00456AD6` `ret 4`.** `functions.tsv` parent `00456903` (172 callees) over-merges the getter / dtor / ctor / size / persist. **0** `E8` of `004569A7`; dispatch is vtbl `0x01233D1C` slot **18**. | **PROVEN** |
| What class? | Compiled bank type **`EXPRESSION`**. Factory `0044D6C5` `"EXPRESSION"` → `0045D70B` `push 0x90` → ctor `00456964` vtbl `0x01233D1C`. **Not** `CTCExpression` (`0x0124026C`) and **not** the 16-byte component-def (`0x012401F4` / `004DB050`). Nested object `007EF200` reads as `esi = [[CTCExpression+12]]`. | **PROVEN** |
| Record size vs `add esi, 0x8C`? | **`0x90` in-memory** (`004569A1` / alloc `0045D70B`). `add esi, 0x8C` is **offset 140**, last persist (`00456C21` u32). Serialized `game.bin` raw **187** = 3-byte header + u16 `0` + 182 field bytes. | **PROVEN** |
| `+120` persist type? | **`0045228F` → `0044FC00` raw u32.** File payload is **names.bin offset** or **`-1`**. Same field CRC **`0x1FB35A1B`** as `CActivateQuestDef+40` intern. **Not** variable-length CString helper `00431143`. Tick `007EF200` still **reads** the slot as a CString intern. | **PROVEN** file u32 / names.bin offset; **PARTIAL** Lionhead field name (CRC only) |
| New Game persist writes `Q_NewOakValeIntro` here? | **No.** All 39 `EXPRESSION` rows parsed. 36 have `+120 = -1`. Three write names.bin offsets: `Expression_Pickpocket` / `Expression_Picklock` / `Expression_Steal`. No intern `0x012C5D14` in any 187-byte body. `names.bin` has **no** `Q_NewOakValeIntro`. | **DISPROVEN** |
| Who first **writes** `+120` after Lookout (not persist)? | **Nobody recovered.** First write is ctor `-1` then persist `00456A5A` on Init Definition Manager (`0044C72B` / `009B08C0`), **before** Lookout. After that, copy ctor `0045D6A4` is the only other recovered store. `007EF200` **reads**. CTC persist `00686960` is a `ret 8` stub; `007EF070` only looks up `"ExpressionDef"` into `+12`. | **PROVEN** persist-before-Lookout; **DISPROVEN** as a post-Lookout writer |
| Wire `ActivateQuest("Q_NewOakValeIntro")` from this VA? | **No.** | **DISPROVEN** |

---

## Verdict

`00456A5A` is **compiled-def persist** of bank
`EXPRESSION+120`, the same nested dword `007EF200`
copies into `004B4A10` when `[+116]==0` and the
slot is not the empty intern.

First-seen New Game **does** run this persist (39
rows, `009B08C0` Compile leftover). It **never**
stores `Q_NewOakValeIntro`. Empty is `-1`; the only
non-empty names are the three `Expression_Pick*`
offsets.

Host already skips inventing the Oakvale activate.
`PrepareDefinitionManager` Notes `009B08C0` as
leftover body and does **not** parse these 187-byte
rows. **MATCH** skip. **LEFTOVER** compile persist.

Until a **later** live `0x8F` tick sees intern
`0x012C5D14` in this slot, the no-save Oakvale
activator stays **UNKNOWN**.

---

## Status table

| Claim | Class | Evidence |
|---|---|---|
| Site `00456A5A` is `call 0045228F` of `lea eax,[esi+120]` | **PROVEN** | `listing-00440000.txt` |
| Enclosing persist is `004569A7`–`00456AD6` | **PROVEN** | `push esi` / `mov esi,ecx` / `ret 4`; next fn `00456AD9` |
| `functions.tsv` `00456903` is that persist | **DISPROVEN** | `00456903`–`00456945` is a different `00404500` getter. Blob also holds dtor `00456948` and ctor `00456964` |
| `e8.tsv` dest `004569A7` | **PROVEN** none | vtbl slot 18 only |
| vtbl `0x01233D1C` slot 18 = `004569A7`, slot 20 = `004569A1` size `0x90`, slot 19 = copy `0045D637` | **PROVEN** | `vtbl.tsv` |
| Factory `"EXPRESSION"` → `0045D70B` | **PROVEN** | `0044D6C5` `push "EXPRESSION"`; `0044D6E4` `mov [ebp-16], 0x45D70B` |
| `CTCExpression` persist is this function | **DISPROVEN** | CTC vtbl `0x0124026C` slot 1 `00686960` `ret 8`; def vtbl `0x012401F4` slot 1 `007EF070` `004109A0("ExpressionDef")` into `+12` |
| `+120` file helper is CString intern-from-stream `00431143` | **DISPROVEN** | `0045228F` file arm is `0044FC00` 4-byte copy after `00404500` skips 4-byte field CRC |
| `+120` CRC `0x1FB35A1B` == `FableCrc("QuestName")` | **DISPROVEN** | sibling `GameBinFormatTests` / `cactivatequestdef-payloads` |
| 39×187 bodies contain intern `0x012C5D14` | **DISPROVEN** | inflate hex; no `145D2C01` |
| `names.bin` can yield `Q_NewOakValeIntro` | **DISPROVEN** | `names.tsv` 0 hits |
| `xrefs.tsv` `0x012C5D14` includes persist | **DISPROVEN** | five sites: bind `00CD6E28`/`00CD6E87` + Gameflow wait `00CE791E`/`00CE7978`/`00CE79CA` |
| Host invents Oakvale activate from this | **DISPROVEN** | `EngineLifecycle.InitCharactersAndQuests` Notes skip; test `No_save_does_not_activate_Q_NewOakValeIntro` |

---

## 1. Enclosing function

`listing-00440000.txt` (no `listing-00450000`):

```
00456900  ret 4
00456903  push ebp                    ; different getter (00404500)
          …
00456945  ret 4
00456948  push esi                    ; deleting dtor (00430300 / 00BFE9BC)
          …
00456961  ret 4
00456964  push esi                    ; ctor
00456967  call 00430370
0045696C  mov [esi], 0x1233D1C        ; vtbl EXPRESSION
          or eax, -1
          mov [esi+60] … [esi+88], eax
          mov [esi+100] … [esi+108], eax
          and [esi+112], 0
0045699A  mov [esi+120], eax          ; +120 = -1 until persist
004569A0  ret
004569A1  mov eax, 0x90               ; size getter, vtbl slot 20
          ret
004569A7  push esi                    ; persist, vtbl slot 18
004569A8  mov esi, ecx                ; EXPRESSION*
004569AA  push edi
004569AB  mov edi, [esp+12]           ; persist context
          …
00456A54  lea eax, [esi+120]
00456A57  push eax
00456A58  mov ecx, edi
00456A5A  call 0045228F               ; THIS SITE
          …
00456AC6  add esi, 0x8C               ; +140
00456ACC  push esi
00456ACD  mov ecx, edi
00456ACF  call 00456C21
00456AD4  pop edi
00456AD5  pop esi
00456AD6  ret 4
00456AD9  push ebp                    ; +116 helper, not this fn
```

**PROVEN.** Packed thiscalls, no `int3` between them.
`functions.tsv` start `0x00456903` swallows all of
the above (callee list includes every `0045228F` in
the persist). Same over-merge class as
`007EEF60` vs `007EF200`.

Factory (`listing-00440000` `0044C72B` Compile
table):

```
0044D6C5  push "EXPRESSION"
          …
0044D6E4  mov [ebp-16], 0x45D70B
          call 009B0AC0
```

```
0045D70B  push 0x90
          call 00BFEA1A
          test eax, eax
          je  0045D721
          mov ecx, eax
0045D71C  jmp 00456964
```

Copy ctor `0045D637` / `0045D6A4` `mov [esi+120],eax`
from `[edi+120]`. **PROVEN** second writer; not
first-seen New Game (defs are bank-loaded, not copied
on Lookout).

---

## 2. Persist field map and `+120` type

`0045228F` (`listing-00440000`):

```
0045228F  push esi
          push 0x122D70E              ; empty intern (write-mode name only)
          mov esi, ecx                ; persist context
          call 00404500               ; mode 2: skip 4-byte field CRC
          mov eax, [esi+24]
          dec eax / dec eax
          je  004522B6                ; mode 2 file
          dec eax
          jne 004522C2                ; not mode 3
          push [dest]                 ; mode 3 write dword
          call 00993EB0
004522B6  push [esi+36]
          mov ecx, dest
          call 0044FC00               ; copy 4 payload bytes → [dest]
          ret 4
```

`0044FC00` is a 4-byte stream read (`mov [esi],edi`).
**PROVEN** u32. Contrast `00431143` (real CString:
`0099E4B0` / stream vtbl+24 / `0099EFB0`).

`004569A7` field order vs `007EF200` use:

| Off | Helper | File payload | `007EF200` |
|---|---|---|---|
| `+60`…`+88` | `0045228F` | u32 / `-1` | unused here |
| `+92` | `00431061` | float (`fld`) | unused |
| `+96` | `00431102` | u32 | unused |
| `+100`…`+108` | `0045228F` | u32 / `-1` | unused |
| `+112` | `004522F4` | u32; ctor **0** | unused |
| `+116` | `00456AD9` | u32; ctor 0 | `test` → camera `0041649C` if set |
| **`+120`** | **`0045228F`** | **names.bin offset or `-1`** | CString vs `0x122D70E` → `004B4A10` |
| `+124`/`+125`/`+126` | `0043314A` | **u8** (`00403EB0` `setne`) | arg3 / follow-on |
| `+128` | `00456B7D` | u32 | unused |
| `+132`/`+133`/`+134` | `0043314A` | u8 | unused |
| `+136` | `00431020` | u32 (`0040F8A0`) | unused |
| `+140` (`0x8C`) | `00456C21` | u32 | unused |

`add esi, 0x8C` is **not** the object size. Size is
**`0x90`**. Serialized 187 matches 3 header + u16 `0`
+ the table (12×8 `0045228F` + float 8 + int 8 +
`004522F4` 8 + `00456AD9` 8 + 6×5 bools + `00456B7D`
8 + `00431020` 8 + `00456C21` 8 = 182).

Field CRC at `+120` is **`0x1FB35A1B`** (little-endian
`1B5AB31F`), identical to `CActivateQuestDef+40`.
Lionhead **name** of that CRC is **UNREAD** (not
`QuestName`).

---

## 3. First-seen New Game persist: 39 rows, no Oakvale

`0044C72B` registers the factory, then
`"Game Definition Manager: Compile"` / `009B08C0`
loads `game.bin`. That is first-seen persist of these
objects. **Before** Lookout TNG / first Present.

`entries.tsv`: 39 `EXPRESSION`, raw **187**, 0 named
ASCII fields. Instance names are social
(`EXPRESSION_FOLLOW` … `EXPRESSION_A`). **No** `Q_*`.

Inflate (`Fable.Dump bin EXPRESSION*`): every row is
`01 00 01 00 00` (NULLDEF `00 00 00 00 00`) then
`(crc, payload)` in persist order.

`+120` dword after CRC `1B5AB31F`:

| Instance | Payload | `names.bin` |
|---|---|---|
| `NULLDEF_EXPRESSION` + 35 other social rows | `FFFFFFFF` (`-1`) | none |
| `EXPRESSION_PICKPOCKET` | `0x00059A88` | **`Expression_Pickpocket`** |
| `EXPRESSION_PICKLOCK` | `0x00059AA2` | **`Expression_Picklock`** |
| `EXPRESSION_STEAL` | `0x00059ABA` | **`Expression_Steal`** |

**PROVEN** complete 39/39. Sibling
`00843F50` already interned `"Expression_Follow"` /
`"Expression_Wait"` as **generic** action names, not
always-quest. These three `Expression_Pick*` offsets
are the same kind of name, **not** `Q_NewOakValeIntro`.

No row contains intern `0x012C5D14`. `names.bin` has
no that string, so a names.bin-offset persist **cannot**
yield it.

When (if) compile later converts names.bin offset →
CString intern, that write is still **inside**
`009B08C0`, still **before** Lookout, and still not
Oakvale. Converter VA **UNREAD**.

---

## 4. Writers of `EXPRESSION+120` (not the tick)

| Site | When | Value |
|---|---|---|
| ctor `0045699A` | alloc `0045D70B` | `-1` |
| persist `00456A5A` | `009B08C0` Compile | names.bin offset or `-1` |
| copy `0045D6A4` | vtbl slot 19 | copy dword |
| `007EF200` `lea ebx,[esi+120]` | Thing tick | **read** (`005FA740` / `00415DD0`) |
| CTC persist `00686960` | — | stub, no store |
| `007EF070` | component persist | `"ExpressionDef"` → `[this+12]` only |

After Lookout on no-save: bank objects already exist.
No recovered store. First Present leftover **#4** /
Lookout TNG leftover **#50** have **zero**
`StartCTCExpression` (sibling
`ctcexpression-quest-names`). Do **not** collapse
those leftovers into this persist.

---

## Original (no-save New Game)

```
0041601D  [vtbl+8] 0044C72B Init Definition Manager
  0044D6C5  register "EXPRESSION" factory 0045D70B
  009B08C0  Compile leftover
    ×39  0045D70B alloc 0x90 → 00456964 (+120=-1)
         vtbl+18 004569A7
           00456A5A 0045228F [+120]
             36× -1
             3× Expression_Pickpocket/Picklock/Steal
             0× Q_NewOakValeIntro
004A0D90  AddQuest FALSE Q_NewOakValeIntro     // +184 only
00416BCF  +90584 empty skip 004B4A10
004FDBC0  LookoutPoint.tng                     // 0 CTCExpression
00501450  first region Lookout
006B3FF0  first Present
007EF200  needs live 0x8F + nested +120 intern // not here
00CE7670  wait Q_NewOakValeIntro == 0
```

`xrefs.tsv` of intern `0x012C5D14`: bind + wait only.

---

## Host

`EngineLifecycle.PrepareDefinitionManager` Notes
`0044C72B` / `009B08C0` **"Compile leftover body"**
and does **not** inflate `EXPRESSION` rows or write
`+120`. `InitCharactersAndQuests` Notes
`"004B4A10 not Q_NewOakValeIntro"` and the `+90584`
empty skip. `ActivateNamedQuest` walks `world+172`
only. No `ActivateQuest("Q_NewOakValeIntro")`.
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`.

**MATCH** skip. Compile persist is **LEFTOVER**
(already noted). Do **not** add a Note that pretends
`00456A5A` stored Oakvale. Do **not** treat leftover
`#4` / `#50` as this writer.

---

## Gap / next unread site

Oakvale activator name source is **not** this persist.

| Item | Class | Next |
|---|---|---|
| Lionhead name of CRC `0x1FB35A1B` | **UNREAD** | not `QuestName`; same CRC as `CActivateQuestDef+40` |
| names.bin offset → CString intern converter | **UNREAD** | likely inside `009B08C0` **before** Lookout; would still not be Oakvale |
| First live `007EF200` with non-empty intern after a region | **UNREAD**; **DISPROVEN** as no-save Lookout | needs `[thing+145]` + component `0x8F`; sibling `007EF200-first-plus120` |
| `CActivateQuestDef` 16-byte names | **PROVEN** not Oakvale | `Global_OpenChest` / `Global_GiveHeroItemsFromRewardChest` / `Global_TeleportToHeroGuild` / `Global_ToggleTimeDisplay` |
| Who first `004B4A10`s intern `0x012C5D14` after Lookout | **UNREAD** | not persist `00456A5A`; not TNG section `XXXSectionStart` |

**Next unread site:** first Thing after a **later**
region (not Lookout TNG parse) that actually ticks
`CTCExpression` vtbl+28 with nested `+120` intern
equal to `0x012C5D14` — or prove no such Thing and
keep hunting a different `004B4A10` name source.
Do **not** invent it from this VA.

Until that live intern equals `Q_NewOakValeIntro`,
the no-save activator stays **UNKNOWN**.
