# FableCrc `.text` imm of intern `0x012C5D14` `Q_NewOakValeIntro`

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** collapse CString intern xrefs (`xrefs.tsv`) with
the non-image u32-imm table (`crc.tsv`).
Do **not** collapse catalog (`world+184` / `QM+44`) with a
Thing CString (`007EF200+120` / `00843FC0+168` / `0x6C+40`).

Question: is intern `0x012C5D14` used as a FableCrc
immediate in `.text`? Who first copies that intern onto a
live Thing CString after a region exists? What is the
`004B5080` `START_NEW_QUEST` operand?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **UNKNOWN**.

Authority: ExeIndex `listing-00cc0000.txt` `00CD6E27` /
`00CE791D`; `listing-00480000.txt` `004AF450` / `004B4A10` /
`004B4AA0` / `004B4B5F` / `004B5080` / `004B54FC` /
`004B5500` / `004B58F3` / `004B5B84` / `004BBB40`;
`listing-007c0000.txt` `007EF200` / `007EF3A1`;
`listing-00840000.txt` `00843F50` / `00843FC0` /
`0084407E`; `listing-00880000.txt` `008969A0` /
`008969B1`; `listing-00980000.txt` `0099F690` /
`0099F600`; `00-index/strings.tsv` / `xrefs.tsv` /
`xrefs-by-string.tsv`; `01-sections/text-map/crc.tsv`
(non-image u32 imm) / `abs.tsv` / `calls-by-dest.tsv` /
`e8.tsv` / `functions.tsv`; `compiled-defs/names.tsv`;
`src/Fable.Formats/Defs/FableCrc.cs` (`0xEDB88320`,
init 0); `GameBinFormatTests`
`CActivateQuestDef_payloads_are_16_bytes_and_do_not_intern_Q_NewOakValeIntro`
/ `Script_bin_payloads_do_not_intern_Q_NewOakValeIntro`
/ `Expression_plus120_persist_is_not_Q_NewOakValeIntro`;
siblings `oakvale-activate-unread-audit`,
`q-novi-activator-callers`,
`00CB5AD0-remaining-presenters`,
`007EF200-first-plus120`,
`00456A5A-expression-plus120`,
`cactivatequestdef-payloads`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Is intern `0x012C5D14` a FableCrc `.text` imm? | **No.** Image-VA CString. Listing annotates `68 14 5D 2C 01` as `push "Q_NewOakValeIntro"`. `crc.tsv` has **0** `0x12C5D14`. `FableCrc("Q_NewOakValeIntro")` `0x02C878A8` has **0** `crc.tsv` / listing / `names.tsv` hits. | **DISPROVEN** |
| Different table from CString xrefs? | **Yes.** Five `xrefs.tsv` sites are bind + Gameflow wait. `crc.tsv` is non-image u32 imm (`val >= 0x10000` and not in-image). Intern never lands there. | **PROVEN** |
| Who first copies that intern onto a Thing CString after a region exists? | **Nobody recovered.** Five PE pushes never write `+120` / `+168` / `0x6C+40`. Those slots copy a **runtime** CString. Persist / TNG / `game.bin` / `script.bin` do not hold the intern. First live Thing is still **UNKNOWN**. | **DISPROVEN** as PE copy; live Thing **UNKNOWN** |
| `004B5080` save operand? | Streamed CString via `009BA4A0` between `START_NEW_QUEST` / `END_NEW_QUEST`, stored as 12-byte record **`+4`**. **Not** `push 0x012C5D14`. Writer `004AF450` emits prefix `0x012393CC` + `[record+4]`. Unique `E8` is `004B58F3` inside `004B5500`. 0 no-save inbound. Live save bytes **UNKNOWN**. | **PROVEN** slot; **DISPROVEN** as intern PE; live name **UNKNOWN** |

---

## Verdict

**`0x012C5D14` is not a FableCrc immediate.** It is only
the PE intern of `"Q_NewOakValeIntro"`. `.text` uses it
as a CString pointer at **five** sites. The FableCrc
table (`crc.tsv`) has neither that VA nor the name hash
`0x02C878A8`.

No recovered site **copies** that intern onto a Thing
CString. `007EF200` / `00843FC0` / `004B4AA0` pass
whatever already sits at nested `+120` / action `+168` /
component `0x6C+40`. Those writers are persist, def
`+40`, or a caller CString — none is a PE Oakvale push.
After a region exists the first live slot equal to the
intern stays **UNKNOWN**.

`004B5080` is save **parse**, not New Game activate.
Its `START_NEW_QUEST` operand is a **streamed** name,
not intern `0x012C5D14`. Do **not** invent
`ActivateQuest("Q_NewOakValeIntro")`.

Oakvale-audit leftover “FableCrc `.text` imm” is
**closed (DISPROVEN)**. Leftover “first live Thing
CString” and “live save `START_NEW_QUEST` name” stay
**UNKNOWN**.

---

## Status table

| Claim | Class | Evidence |
|---|---|---|
| Five `.text` uses of intern `0x012C5D14` | **PROVEN** | `xrefs.tsv`; listing bytes `68 14 5D 2C 01` only at `00CD6E27` / `00CD6E86` / `00CE791D` / `00CE7977` / `00CE79C9` |
| Those five are bind + card + wait | **PROVEN** | `00CB5C90` / `00CBFAB8`; `vtbl+1180`; `vtbl+100` `00893570` |
| Listing `14 5D 2C 01` elsewhere | **DISPROVEN** | 5 hits, all the intern pushes |
| Intern in `crc.tsv` / `abs.tsv` | **DISPROVEN** | string-annotated; `AbsValues` sees no `0x012C5D14`; `crc.tsv` 0 `0x12C5D14` |
| `FableCrc("Q_NewOakValeIntro")` `0x02C878A8` | **PROVEN** | `FableCrc.cs`; check `names.tsv` `UI` `0xC8CC5025` / `ENGINE` `0xA9927CA8` |
| That hash as `.text` imm | **DISPROVEN** | `crc.tsv` 0 `0x2C878A8`; listing 0 `A8 78 C8 02`; `names.tsv` 0 |
| `names.bin` row for the quest | **DISPROVEN** | `names.Find` null; `names.tsv` 0 |
| `names.tsv` `0xFA5557F6` is this quest | **DISPROVEN** | that CRC is `CActivateQuestDef` |
| Field CRC `0x1FB35A1B` is this quest | **DISPROVEN** | persist `EXPRESSION+120` / `CActivateQuestDef+40` field id, not the name |
| `007EF200` pushes the intern | **DISPROVEN** | copies nested `[esi+120]` |
| `00843FC0` pushes the intern | **DISPROVEN** | `[this+168]` from ctor arg / `def+40` |
| `004B4B5F` pushes the intern | **DISPROVEN** | `add eax, 40` then `004B4A10` |
| 39×187 `EXPRESSION` bodies hold intern | **DISPROVEN** | sibling inflate; no `145D2C01` |
| Six `CActivateQuestDef` 16-byte rows hold intern | **DISPROVEN** | every 4-byte window ≠ `0x012C5D14` |
| `script.bin` intern dword | **DISPROVEN** | 0 hits |
| Lookout TNG copies intern onto a CTC field | **DISPROVEN** | 0 Oakvale / 0 `StartCTCExpression` |
| `StartOakValeWest` TNG `+120` / `+168` / `+40` | **DISPROVEN** | only `XXXSectionStart` → `ThingInstance.Section` |
| First live Thing after a region whose copied CString **is** the intern | **UNKNOWN** | needs a live `+145` dump; not Lookout; not no-save Type-1 |
| `004B5080` PE operand is intern `0x012C5D14` | **DISPROVEN** | only `START_NEW_QUEST` / `END_NEW_QUEST` / empty intern `0x122D70E` |
| `004B5080` unique `E8` | **PROVEN** | `004B58F3` in `004B5500` (`int3` `004B54FF`) |
| `004B5080` on no-save | **DISPROVEN** | 0 external inbound |
| Live save `START_NEW_QUEST` name equals Oakvale | **UNKNOWN** | streamed; no save dump in scope |
| Host invents `ActivateQuest(Q_NewOakValeIntro)` | **DISPROVEN** | do not add |

---

## 1. Two tables

`strings.tsv`:

```
0x012C5D14	0xEC5D14	Q_NewOakValeIntro
```

`xrefs.tsv` / `xrefs-by-string.tsv` (CString, **five**):

| Site | Parent | Role |
|---|---|---|
| `00CD6E28` | `00CD5170` blob / bind `00CD6E27` | `00CB5C90` factory `0xDBEF70` / `S_QNOVI` |
| `00CD6E87` | same | `00CBFAB8` second bind arm |
| `00CE791E` | `00CE7670` | `vtbl+1180` card `OBJECT_QUEST_CARD_OAKVALE_INTRO` |
| `00CE7978` | `00CE7670` | `vtbl+100` Give-wait |
| `00CE79CA` | `00CE7670` | same wait, loop |

`listing-00cc0000.txt` raw bytes are the intern:

```
00CD6E27  68 14 5D 2C 01            push "Q_NewOakValeIntro"
00CD6E86  68 14 5D 2C 01            push "Q_NewOakValeIntro"
00CE791D  68 14 5D 2C 01            push "Q_NewOakValeIntro"
00CE7977  68 14 5D 2C 01            push "Q_NewOakValeIntro"
00CE79C9  68 14 5D 2C 01            push "Q_NewOakValeIntro"
```

Whole `text-map/` `14 5D 2C 01`: **those five only**.

Indexer `GrepFacts.AbsValues` only extracts `0x…` tokens.
String-annotated pushes therefore appear in **neither**
`abs.tsv` nor `crc.tsv`. `crc.tsv` is the other table:
non-image u32 imm `>= 0x10000` (`Program.cs` map-text).
Intern `0x012C5D14` is in-image, so it could never be a
`crc.tsv` row even if listed as hex.

**PROVEN** different table.

---

## 2. `FableCrc("Q_NewOakValeIntro")` is not a `.text` imm

Hasher: `FableCrc` (`0xEDB88320`, init 0, no xorout).

```
FableCrc("Q_NewOakValeIntro") == 0x02C878A8
```

Check against `names.tsv` (same hasher):

| Seed | CRC | `names.tsv` |
|---|---|---|
| `UI` | `0xC8CC5025` | match |
| `ENGINE` | `0xA9927CA8` | match |

Hunt of `0x02C878A8` / `0x2C878A8` / LE `A8 78 C8 02`:

| Table | Hits |
|---|---|
| `crc.tsv` | **0** |
| all `listing-*.txt` | **0** |
| `names.tsv` | **0** |
| repo `0x02C878A8` | **0** |

`names.tsv` `0xFA5557F6` is **`CActivateQuestDef`**, not
the quest. Persist field CRC `0x1FB35A1B` on
`EXPRESSION+120` / `CActivateQuestDef+40` is **not**
`FableCrc("QuestName")` and **not** this quest.

Activation recovered for this name is a **CString intern**,
not a name CRC. A CRC hit would have been a different
consumer than `004B4A10`. There is no such imm.

**DISPROVEN.**

---

## 3. No PE copy of the intern onto a Thing CString

Thing slots that can later `004B4A10` a copied name:

| Slot | Copier | Source | PE Oakvale? |
|---|---|---|---|
| `CTCExpression` nested `+120` | `007EF200` `00415DD0` | `[esi+120]` vs empty `0x122D70E` | **No** |
| `CCreatureAction_ActivateQuest+168` | `00843FC0` | ctor `00843F50` arg / `def+40` | **No** |
| component `0x6C` record `+40` | `004B4B5F` | `add eax, 40` after `0040F020` id `0x6C` | **No** |

`008969A0` **writes** `0x6C+40` (`0099EFB0` from
`[esp+24]`) then `008969B1` `004B4AA0`. That source is
the caller CString, **not** `push 0x012C5D14`. HUD
follow-on is `HUD_ORB_QUEST_CORE` /
`TEXT_QST_078_GM_MSG_NEW_QUEST_CARD` — leftover card,
not no-save Init.

Closed file-side copies of the intern dword:

- 39×187 `EXPRESSION` bodies: no `145D2C01`
- six `CActivateQuestDef` 16-byte rows: every window ≠ intern
- `script.bin` inflated `Raw`: 0
- `names.bin`: no string, so no names offset can resolve to it
- Lookout TNG: 0 Oakvale, 0 `StartCTCExpression`
- `StartOakValeWest` TNG: `XXXSectionStart Q_NewOakValeIntro` only (`ThingInstance.Section`)

`world+184` / `QM+44` / `world+196` **do** hold the
catalog CString (`AddQuest FALSE` / `AddTestQuest`).
Those are QuestManager / world vectors, **not** the
three Thing slots.

After a region exists (`006B3FF0` Lookout first), a
later Thing with `+145` live could still tick
`007EF200` / queue `00843FC0` / hit `004B4AA0` with a
**copied** CString equal to the intern. No PE writer
fills that. First such Thing is **UNKNOWN**.

Do **not** treat `StartOakValeWest` section buckets as
that copy.

---

## 4. `004B5080` `START_NEW_QUEST` operand

`004B5080` (`listing-00480000.txt`) starts after `int3`
`004B507D`–`004B507F`, ends `004B54FC` `ret 4`.
`functions.tsv` size 1674 over-merges later save arms
(`004B5B84` is **not** this fn).

```
004B50C0  push "START_NEW_QUEST"          ; 0x012394B0
          0099EBF0 / 009BA330
004B50F5  push "START_NEW_QUEST"
          009B9E00  →  je 004B52AB        ; no tag → skip
004B5124  009B9C60 / 009BA4A0 / 0099EFB0  ; STREAM token
004B516B  push "END_NEW_QUEST"
          009BA330                         ; read until end
          0099EC30 ×3                      ; 12-byte record
          store at [esi] / 004BB400
004B527C  push "START_NEW_QUEST"
          009B9E00  →  jne 004B5124        ; next block
004B54A2  004BBB40([esi],[esi+4], [esi+12])
004B54FC  ret 4
```

`004BBB40` is a 12-byte-stride range helper
(`0x2AAAAAAB` ÷12) then `004BBA40` / `004BB520`.
**Not** `004B4260`. Save `004B4260` is
`004B5B84` `START_ACTIVE_QUESTS` inside **`004B5500`**.

`START_NEW_QUEST` xrefs: **only** those three parse
sites. Spaced intern `0x012393CE` `"START_NEW_QUEST "`
has **0** xrefs.

Writer is `004AF450` (`END_NEW_QUEST` xref `004AF4AB`):

```
004AF463  push 0x012393E0                 ; next intern (BoastCompleted)
          lea eax, [edi+4]
          push eax                        ; record+4 CString*
          mov edx, 0x012393CC             ; 2 bytes before spaced tag
          call 0099F690                   ; prefix + append name; ret 4
          call 0099F600                   ; ret 4, consumes 0x012393E0
          call 0099F0A0                   ; write line
          0099F0A0([edi+8])
          push "END_NEW_QUEST"
          call 0099F100
```

`0099F690`: `edx` is a C-string (`cmp [edi], 0`), copy
`009A0590`, append stdcall CString via `0099F0A0`.
Name on disk is **`[record+4]`**, formatted with the
spaced-tag prefix at `0x012393CC`. **Not** intern
`0x012C5D14`.

Unique `E8` of `004B5080`: `004B58F3` inside
`004B5500` (`START_SAVED_QUESTS` nested parse).
`004B5500` itself is only `E8`d from later save
`004B646E` / `004B64C7` (same over-merge). **0**
no-save inbound.

Whether a later save’s `START_NEW_QUEST` block text
equals `Q_NewOakValeIntro` is **UNKNOWN** (needs a
save dump). The PE operand is **DISPROVEN**.

---

## Timeline (no-save) — still no FableCrc / Thing copy

```
00CD6E27  push intern 0x012C5D14     ; BIND only
004A0D90  AddQuest FALSE             ; +184 / QM+44 catalog CString
          AddTestQuest               ; +196 only
0049F24E  004B4260([world+172])      ; name absent
00416BCF  +90584 empty skip
user.ini  00892E80 "Gameflow"
00CE7670  push intern                ; WAIT vtbl+100
007EF200 / 00843FC0 / 004B4AA0       ; no PE intern; live Thing UNKNOWN
004B5080                             ; 0 E8
crc.tsv  0x02C878A8                  ; 0 rows
```

---

## Host

`EngineLifecycle` Notes `00416BCF` skip and
`"004B4A10 not Q_NewOakValeIntro"`.
`ActivateNamedQuest` walks `world+172` only.
`OakvaleQuestIntern = 0x012C5D14` is the **CString**
intern, not a FableCrc. `ExpressionPlus120Crc =
0x1FB35A1B` is a persist **field** CRC.
`No_save_does_not_activate_Q_NewOakValeIntro`.
**MATCH.**

Do **not** add `FableCrc("Q_NewOakValeIntro")` as a
`.text` gate. Do **not** invent
`ActivateQuest("Q_NewOakValeIntro")`.

---

## Remaining UNKNOWN

1. First live Thing after a region whose
   `CTCExpression+120` / action `+168` / `0x6C+40`
   CString **equals** intern `0x012C5D14`.
2. Live save `START_NEW_QUEST` `[record+4]` text
   (off no-save).
3. `[game+90584]` if a later writer fills it —
   no-save skip **PROVEN**.

Until (1) dumps a live name, the later construct
presenter stays **UNKNOWN**. The FableCrc-imm leftover
is **not** that presenter.

---

## Sources (absolute)

- `C:\FableCSharp\assembly\exe\00-index\strings.tsv`
- `C:\FableCSharp\assembly\exe\00-index\xrefs.tsv`
- `C:\FableCSharp\assembly\exe\00-index\xrefs-by-string.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\crc.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\abs.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\calls-by-dest.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-007c0000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00840000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00880000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00cc0000.txt`
- `C:\FableCSharp\assembly\compiled-defs\names.tsv`
- `C:\FableCSharp\proofs\oakvale-activate-unread-audit\README.md`
- `C:\FableCSharp\proofs\q-novi-activator-callers\README.md`
- `C:\FableCSharp\proofs\00CB5AD0-remaining-presenters\README.md`
