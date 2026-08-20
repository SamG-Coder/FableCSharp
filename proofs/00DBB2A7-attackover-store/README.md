# `00DBB2A7` writes `[quest+80]=1` (`AttackOver`)

Investigation + host constant lock. Production does **not**
invent `ActivateQuest("Q_NewOakValeIntro")`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Authority: `listing-00d80000.txt` `00DBB218`–`00DBB2A7`;
`PersistTable.AttackOverStore`;
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`.

---

## Verdict

The `AttackOver` / `[this+80]` store is **`00DBB2A7`**
`mov [ecx+80], 1` **after** `CS_OAKVALE_INTRO_THERESA`
(`00CBFB7D` at `00DBB238`) and PlayAVI
`Data\Video\1_raid_on_oak_vale_comp.xmv` (`vtbl+1476`
at `00DBB260`). It is **not** a `mov` inside
`00DBDE40` / `00DBDE00–00DBF000` wait spin.

`00DAADA0` remains the persist **bind**
(`004045C0("AttackOver", this+80)`). First-seen value
is still **false** until this store runs.

| Claim | Class |
|---|---|
| Store VA is `00DBB2A7` | **PROVEN** |
| Store is after Theresa CS + raid AVI | **PROVEN** |
| Store is inside `00DBDE40` map-wait | **DISPROVEN** |
| `00DAADA0` is the `+80=1` write | **DISPROVEN** (bind only) |
| No-save New Game reaches this store | **DISPROVEN** (quest never constructed) |
| Host `AttackOverWriterKnown` | **PROVEN** as the store VA |

Do **not** set persist true from `StartNewGame` / dummy Lookout.
The writer is later `S_QNOVI` body, after childhood deeds + raid.
