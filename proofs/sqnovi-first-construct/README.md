# Who first constructs `S_QNOVI` on retail no-save New Game

Investigation only. No production `src/` invent of
`ActivateQuest("Q_NewOakValeIntro")`.

Status: **UNKNOWN** as first-seen constructor.
Every recovered `004B4260` list on this walk
**excludes** that name.

## Verdict

Unique `00CB5AD0` `E8` is `004B42E8` inside
`004B4260`. Init Quests walks `world+172`
(`AddQuest` TRUE). `Q_NewOakValeIntro` is FALSE
→ catalog `world+184` / `QM+44` only.

| Site | List | Oakvale on no-save |
|---|---|---|
| `0049F24E` Init Quests | `world+172` 9 TRUE names | **DISPROVEN** |
| `004B4A5A` | 1-name wrapper | first-seen `"Gameflow"` / empty skip |
| `004B5B84` | save `START_ACTIVE_QUESTS` | not no-save |
| `00892EAF` / `00892EEF` | vtbl 277 / 279 | 0 first-seen FF |
| `0049EAD1` | stub `+172` | 0 inbound |

`004B5080` `START_NEW_QUEST` has one `E8`
(`004B58F3`, save parse). UI New Game is msg 15
→ Leave `0042F2A2`. **DISPROVEN**.

Gameflow seed `00CE6CF0` only inserts state
names. `00CE7670` before the wait binds the
Oakvale **card** (`vtbl+1180`) then waits
`vtbl+100` `00893570` for type-`0x33`. First
`vtbl+1104` in that fn is later
`"GameflowAssistance"`. **DISPROVEN** as
construct.

TNG `XXXSectionStart Q_NewOakValeIntro` is a
section bucket. `004C97B0` / `00CB8960` are
thing scripts, not `00CB5AD0`.

`00DBDE40` Give `vtbl+1152` at `00DBE295` is
**after** `AttackOver`. Unblocks Gameflow wait
only if childhood already ran.

## Next

Inflated `CActivateQuestDef` 16-byte intern
dwords; later live `[CExpressionDef+120]` /
`[action+168]` equal to `0x012C5D14`.
Do not collapse Give (`0x33`) with construct
(`0x37`).
