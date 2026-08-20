# Who later `00CB5AD0`s / constructs `Q_FireHeart`

Investigation only. Production `src/` was not edited.

Lookout `Q_FireHeart` is a **TNG section name** that holds the
first `AICreature`. That is **not** quest activate.

Do **not** treat first `004B4260` / `0049F24E` as this name.
Do **not** treat `00D35090` / `00CB8230("FH_Villager")` as the
first villager create.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Authority: TLC `data\Levels\FinalAlbion.qst` `Q_FireHeart`;
dump `00CB5AD0` xrefs (`e8.tsv`, `xrefs.tsv`,
`listing-00c80000.txt` / `listing-00480000.txt` /
`listing-00cc0000.txt` / `listing-00d00000.txt`);
`proofs/creature-ai-first`, `npc-first-create`,
`quest-activate-gate`, `qst-first-load`,
`addtestquest-token`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| When is `Q_FireHeart` `00CB5AD0`’d after no-save Leave? | **Not first-seen.** Unique `E8` of `00CB5AD0` is `004B42E8` inside `004B4260`. First `004B4260` is `0049F24E` on `world+172`. This name is **not** in that vector. | **PROVEN** skip |
| `AddQuest` TRUE or FALSE? | **FALSE.** `FinalAlbion.qst` line 100: `AddQuest("Q_FireHeart", FALSE);` | **PROVEN** |
| First Lookout `AICreature` vs this activate? | **Independent.** `LookoutPoint.tng` section `Q_FireHeart` `FH_Villager` is `0051FD80` / `005272E0`. It does **not** wait for `00CB5AD0`. | **PROVEN** |
| Does Gameflow `00CB5AD0` this name? | **No.** Later `00CE7670` gives `OBJECT_QUEST_CARD_FIRE_HEART` (`vtbl+1180`) and **waits** (`vtbl+100`). | **DISPROVEN** as activate |
| Who first-seen no-save eventually `004B4A10`s this name? | **UNREAD** outside debug `0061AB30` / save reload. | **UNREAD** |

---

## Verdict

After no-save Leave, **no first-seen site** `00CB5AD0`s or
constructs `Q_FireHeart`.

`00CB5AD0` has **one** `E8` in the whole `.text`: `004B42E8`
inside `004B4260`. First `004B4260` after Leave is `0049F24E`
on `world+172` (`Init Quests`). That vector is `AddQuest(...,
TRUE)` only. `Q_FireHeart` is `AddQuest(..., FALSE)` plus two
`AddTestQuest` cards → `world+184` / `world+196`, **not**
`world+172`.

Bind `00CD5FCF` / `S_QFHT` / `00D3AFC0` is factory
**register** (`00CB5C90`), not construct. `00D3AFC0` has
**one** callee (`00D34470` ctor). `00D35090` (name table,
includes `FH_Villager`) has **zero** `E8` sites. Both run
only after a later `004B4260` / `004B4A10` of this name
hits and `004B3CE0` constructs.

The dump-proven **consumer of `world+196` that `004B4A10`s**
is debug test-quest UI `0061A8A0` / `0061AB30`. That is
**not** New Game.

| Claim | Class |
|---|---|
| First `004B4260` (`0049F24E` `world+172`) includes this name | **DISPROVEN** |
| Bind `00CD5FCF` constructs `S_QFHT` / `00D35090` | **DISPROVEN** |
| New Game `ActivateQuest("Q_FireHeart")` | **DISPROVEN** |
| `00D3AFC0` / `00D35090` `E8` from Leave / Init Game | **DISPROVEN** |
| `00CB5AD0` unique `E8` = `004B42E8` in `004B4260` | **PROVEN** |
| `AddQuest FALSE` → `world+184` only; `TRUE` also `world+172` | **PROVEN** |
| `AddTestQuest` ×2 → `world+196` stride 28 | **PROVEN** |
| `world+196` activate consumer = `0061AB30` (`004B4A10`) | **PROVEN** |
| `0061AB30` is New Game / no-save Leave | **DISPROVEN** |
| `00CE7670` `00CED95E` starts the quest | **DISPROVEN** (card + wait) |
| First `AICreature` needs this `00CB5AD0` | **DISPROVEN** |
| Who first-seen no-save eventually `004B4A10`s this name without debug UI | **UNREAD** |

---

## Timeline (no-save Leave)

```
Init World 004A6E30
  00CB5D80 "Registering Scripts"
    00CD52D0
      00CD5FBC "S_QFHT"
      00CD5FCF "Q_FireHeart" / [rec]=00D3AFC0 / persist bl=0
      00CB5C90 register                         // BIND only
004A1840 Load Quests
  004A0D90 FinalAlbion.qst
    AddQuest("Q_FireHeart", FALSE)              // line 100
      always world+184
      TRUE only → world+172                    // FALSE: skip
      004B2850 QM+44                           // gate table only
    AddTestQuest ×2
      world+196 28-byte records                 // not 004B4260
[0x13B8648]==0
  0049F180 "Init Quests"
    0049F247 lea edx, [esi+172]
    0049F24E 004B4260                          // FIRST walk
      00CB5AD0 per name in world+172
      Q_FireHeart not in that vector
  user.ini ActivateQuest("Gameflow")           // not this name
later 00501450 Lookout objects
  section Q_FireHeart
    0051FD80 CREATURE_BS_VILLAGER_MALE FH_Villager "0"
      005272E0 / 00831F80                      // FIRST AICreature
      00833A70 CAIBrain                        // creature-ai-first
      no 00D35090 / no 00D373D0
later 00CE7670 (not first type-1)
  00CED95E "Q_FireHeart" + "OBJECT_QUEST_CARD_FIRE_HEART"
    [vtbl+1180] 00896A30                       // card, not 004B4A10
  00CED9CD / 00CEDA19 [vtbl+100]               // wait-until-active
```

`00D35090` / `00CB8230("FH_Villager")` / `00D373D0` are
**not** on this walk.

---

## 1. `00CB5AD0` callers

`e8.tsv`: **one** site.

| site | dest | parent |
|---|---|---|
| `004B42E8` | `00CB5AD0` | `004B4260` |

`listing-00c80000.txt` `00CB5AD0`: lookup by name in the map
filled by `00CB5C90`. Hit → `lea eax, [edi+4]` (factory
record). Miss → `xor eax, eax`. It does **not** alloc
`S_QFHT`.

`004B4260` (`listing-00480000.txt`):

```
ecx = quest manager [0x13B89FC]
arg0 = vector of CString names
for each name:
  004B00C0  QM+44 membership / "NULL"
  al=0 → skip
  mov ecx, [edi+120]
  call 00CB5AD0              // 004B42E8 UNIQUE
  004BB720 12-byte {name, factory, flags}
004B3CE0(that list)
```

`ret 12`. A later `00CB5AD0` of this name **is** a later
`004B4260` (or `004B4A10` which builds a 1-name vector and
`E8`s `004B4260` at `004B4A5A`).

`004B00C0` on this name would return **1** if someone
walked it: `AddQuest` always `004B2850`s into `QM+44`.
FALSE only omits `world+172`. Gate skip is **DISPROVEN**
as the reason first Init Quests misses; the name is simply
**not on the walked list**.

### `004B4260` `E8` sites (`e8.tsv`)

| site | parent | list |
|---|---|---|
| `0049EAD1` | `0049EAC0` | `this+0xAC` (172) on **ecx**, not world |
| `0049F24E` | `0049F180` Init Quests | **`world+172`** first after Leave |
| `004B4A5A` | `004B4A10` | 1-name wrapper |
| `004B5B84` | save `START_ACTIVE_QUESTS` | load-game, not no-save |
| `00892EAF` | `00892EA0` | thunk `push 1,1` then `004B4260` |
| `00892EEF` | `00892EE0` | thunk `push 0,1` then `004B4260` |

First after Leave: **`0049F24E`**. **PROVEN.**

---

## 2. QST: `AddQuest FALSE`

TLC `FinalAlbion.qst`:

```
AddQuest("Q_FireHeart",				FALSE);          // line 100
```

`004A0D90` (`listing-00480000.txt`):

- `AddQuest` always appends to `lea esi, [ebp+184]`
- TRUE (`bl`) also `lea esi, [ebp+172]`
- then `004B2850` → `QM+44`
- `AddTestQuest` → `world+196` stride **28** only

No `004B4260` / `00CB5AD0` in this parser. **PROVEN**.

Host `QuestFile` / `quests-qst.md`: `Q_FireHeart`
persistent **False**. Same token. Bind persist
`[esp+48]=bl` with `xor ebx,ebx` at `00CD52E9` (Sunnyvale
overrides to `0x01`; this row keeps **0**). **PROVEN**.

`world+172` TRUE names (`qst-first-load`):

1. `Q_SunnyvaleMaster` … 8. `CS_PlayCutscene`
9. `Global_WatchForHeroDeath`

`Q_FireHeart` is **not** in that nine. **PROVEN**.

Two `AddTestQuest` rows (same file):

| Line | HSP | Group | Title | Card |
|---:|---|---|---|---|
| 250 | `MemorialGardenHSP` | `0` | `Q Claiming the FireHeart` | `OBJECT_QUEST_CARD_FIRE_HEART` |
| 257 | `LookoutPointHSP` | `0` | `Q Reclaiming the FireHeart` | empty |

Those land in `world+196` only. **PROVEN**.

---

## 3. Bind `00CD5FCF` / `S_QFHT` / `00D3AFC0`

`strings.tsv`: `Q_FireHeart` `0x012C4818`; `S_QFHT`
`0x012F795C`.

`xrefs.tsv` / `listing-00cc0000.txt` — **five** code
pushes of `Q_FireHeart`:

| VA | recovered fn | role |
|---|---|---|
| `00CD5FCF` | `00CD52D0` registrar | bind name |
| `00CD602E` | same | bind cleanup / `00CBFAB8` |
| `00CED95E` | `00CE7670` (`xrefs` split `00CECC07`) | card |
| `00CED9CD` | same | `[vtbl+100]` is-active |
| `00CEDA19` | same | loop `[vtbl+100]` |

**No** other `.text` push. **PROVEN**.

Bind body (`00CD5FB7`–`00CD6015`):

```
push "S_QFHT"
00CB5AC0                 // name map, not construct
push "Q_FireHeart"
mov [esp+32], 0xD3AFC0   // factory pointer
mov [esp+48], bl         // 0
call 00CB5C90            // register
```

`00CD52D0` is called **once** from `00CB5E12` inside
`00CB5D80` (`"Registering Scripts"`), during Init World,
**before** Init Quests. Bind ≠ start. **PROVEN**.

---

## 4. `00D3AFC0` / `00D35090` / `FH_Villager`

| dest | `e8.tsv` sites |
|---|---|
| `00D3AFC0` | **0** (immediate stored at bind; called via `004B3CE0` `[eax+4]`) |
| `00D34470` | **1**: `00D3AFDB` (factory body) |
| `00D35090` | **0** (object `vtbl+8` after construct) |
| `00D373D0` | **0** (stored at name-record `+16`) |

`listing-00d00000.txt` `00D3AFC0`: `push 0x1A4` /
`00BFEA1A` / `00D34470` ctor, vtbl `0x12CCB90`. Factory,
not a Leave `E8`.

`00D35090`: `00CB8230` name table (`DF_Fireheart`,
`FH_Prisoner`, `FH_Scythe`, **`FH_Villager`**, …).

`FH_Villager` record (`00D353D9`):

```
push "FH_Villager"
[edi+16] = 0xD373D0
call 00CB8230
```

That registrar runs **only if** `Q_FireHeart` is
constructed. Leave does not. **PROVEN**.

Indirect construct (`004B3CE0`): factory from a prior
`00CB5AD0` hit, then `[0x1375454]` (`.data` 1) →
`call [eax+4]` `00D3AFC0` → `call [edx+8]` `00D35090`.
Without that hit, `[edi+4]==0` → stub `004B4063`.
**PROVEN** as the only construct path; **DISPROVEN** as
Leave.

---

## 5. Relation to first `FH_Villager` create

`LookoutPoint.tng` (`creature-ai-first` / `npc-first-create`):

```
XXXSectionStart Q_FireHeart;
NewThing AICreature;
DefinitionType "CREATURE_BS_VILLAGER_MALE";
ScriptName FH_Villager;
ScriptData "0";
```

Bridge has **0** `AICreature`. Gameflow / NULL have **0**.
This block is the first `CThingAICreature` after Leave.

| Layer | Native | Needs `00CB5AD0`? |
|---|---|---|
| TNG section string `Q_FireHeart` | grouping in the file; `0051FD80` walks it | **no** |
| Kind factory `AICreature` | `005272E0` / `00831F80` | **no** |
| Script name `FH_Villager` on the Thing | persist string; `00CB8960` table empty at Leave | **no** for create |
| `00D35090` / `00D373D0` | `S_QFHT` name row | **yes** (later) |
| `CAIBrain` `0088C160` | `00833A70` Initial Activate | **no** |

First create is **TNG**, not quest factory, not
`00CB8230`. **PROVEN**. Whether first `004C97B0` then
misses `00CB8960` is **PARTIAL** (`npc-first-create`).

Section census 3 = three `FH_Villager` Things. GuildExterior
later has more `Q_FireHeart` Things (`FH_Scythe`, …). Same
rule: TNG construct does not start the quest.

---

## 6. `world+196` / `004B4A10` later sites

`004B4A10`: 1-name vector → `004B4A5A` → unique
`00CB5AD0`.

| site | this name on no-save? |
|---|---|
| `00416C11` `world+90584` | empty skip (`0x122D70E`) **PROVEN** |
| `004B4B5F` thing `004B4AA0` (`[+40]`) | **UNREAD** as this string |
| `004B4D45` / `0061AC28` | debug UI `0061AB30` **PROVEN** not New Game |
| `007EF3A1` action `[obj+120]` | **UNREAD** as this string |
| `0084407E` creature `[+168]` / `[+172]` | **UNREAD** as this string |
| `00892E8F` / `00892ECF` | generic; ini is `"Gameflow"` **PROVEN** |

`.text` never pushes `Q_FireHeart` into `00892E80`.
Inventing `ActivateQuest("Q_FireHeart")` as New Game is
**DISPROVEN**.

A later native/script could still pass a **copied**
CString into `00892E80` / `004B4AA0` without a second exe
string xref. That is **UNREAD** for first-seen no-save.

`004B5B84` is save `START_ACTIVE_QUESTS`. **DISPROVEN** as
no-save Leave.

`0061AB30` (`[+343]!=0`) can `004B4A10` either FireHeart
`AddTestQuest` row. **PROVEN** as the `+196` consumer.
**DISPROVEN** as New Game.

---

## 7. Gameflow `00CED95E` is not construct

`00CED95E` sits in the giant `00CE7670` frame
(`sub esp, 0x824`). `xrefs.tsv` labels the island
`00CECC07`; there is no new `E8` / `ret` that splits
activate off. Same `esi+64` script-interface, same
`00CEF016` abort.

Immediately before the card (`listing-00cc0000.txt`):
`LOOKOUT_POINT_DEMON_DOOR_READY`, then a region wait on
`"OakvaleMemorialGarden"`, then:

```
00CED95E  push "Q_FireHeart"
00CED971  push "OBJECT_QUEST_CARD_FIRE_HEART"
00CED998  call [eax+1180]          // 00896A30 → 004B0D30
00CED9B9  mov [eax+4], 0x76C       // next Gameflow state
00CED9CD  push "Q_FireHeart"
00CED9E5  call [eax+100]           // is-active 00893610
          invert; je skip
00CEDA00  [vtbl+28] / 00CB7940     // yield
00CEDA19  push "Q_FireHeart"
          loop until active
00CEDA5A  push "V_GuildMaster"
00CEDA78  call [edx+1104]          // 00892E80 ActivateQuest
                                   //   that name, not FireHeart
```

`00896A30` (`listing-00880000.txt`) calls `004B0D30` then
`004AF610` (already active). Card path needs the quest
**already** constructed. **DISPROVEN** as the
`00CB5AD0`.

`vtbl+100` is `00892F40` → `004AF610`. Wait, not lookup.
**PROVEN**.

This site is **not** first type-1 (`00CE7670` state 0
yields earlier; see `gameflow-main-first-tick`). It is a
late Main beat after the demon-door / memorial-garden
waits. **PROVEN** as later; **DISPROVEN** as Leave
activate.

---

## Classifications

1. **`00CB5AD0` unique `E8` — PROVEN.** `004B42E8` only.
2. **First `004B4260` after Leave — PROVEN.** `0049F24E`
   `world+172`. Name not on that list (`AddQuest FALSE`).
3. **`AddQuest` — PROVEN FALSE.** QST line 100. Persist
   bind `bl=0`. Two `AddTestQuest` rows → `+196` only.
4. **Bind constructs `S_QFHT` — DISPROVEN.** `00CB5C90`
   map insert of `00D3AFC0`.
5. **New Game `ActivateQuest(Q_FireHeart)` — DISPROVEN.**
   No exe push into `00892E80`. Ini is `Gameflow`.
6. **`00D3AFC0` / `00D35090` as Leave `E8` — DISPROVEN.**
   Zero sites on those VAs. Ctor only `00D3AFDB`.
7. **First `FH_Villager` — PROVEN TNG.** Lookout section
   `Q_FireHeart` `0051FD80` / `005272E0`. Does **not**
   need this activate. `00D35090` is a later name table.
8. **`world+196` later `004B4A10` — PROVEN as `0061AB30`.**
   **DISPROVEN** as no-save New Game.
9. **First-seen no-save constructor of this name — UNREAD**
   beyond “not Init Quests, not bind, not ini, not
   Gameflow card, not first villager”. Do not fill that
   gap by activating the quest to spawn Lookout NPCs.
