# Parity audit: no-save construct of `Q_NewOakValeIntro`

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** collapse catalog (`world+184` / `QM+44`) with
auto-start (`world+172`). Do **not** collapse construct
(`004B3CE0` kind `0x37`) with Gameflow wait (kind `0x33`
Give). Do **not** treat `00CD6E27` bind as construct.

Question: prove the host **wrong** that no-save New Game
does not activate `Q_NewOakValeIntro`, **or** confirm the
remaining UNKNOWN list of who could still be the
**construct presenter**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH** / **INFERRED**.

Authority: ExeIndex listings
`listing-00400000.txt` (`00416BCF` / `00419CE0`),
`listing-00480000.txt` (`0049F24E` / `004A1080` /
`004B2890` / `004B3CE0` / `004B4260` / `004B42E8` /
`004B4A10` / `004B5080`),
`listing-00600000.txt` (`0061AB30` / `0061AC28`),
`listing-007c0000.txt` (`007EF3A1`),
`listing-00840000.txt` (`00843FC0` / `0084407E`),
`listing-00880000.txt` (`00892E80` / `00892EA0` /
`00892EC0`),
`listing-00cc0000.txt` (`00CD6E27` / `00CE7670`);
`xrefs.tsv` intern `0x012C5D14`;
`EngineLifecycleTests`
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`
/ `Type1_resume_00CB8220_is_00A44880_then_00893570_yield`
/ `No_save_does_not_activate_Q_NewOakValeIntro`
/ `Init_quests_004B4260_activates_wld_initial_list`
/ `UserIni_009EC890_RunScript_joystick_is_00999230_miss`;
`GameBinFormatTests`
`CActivateQuestDef_payloads_are_16_bytes_and_do_not_intern_Q_NewOakValeIntro`
/ `Script_bin_payloads_do_not_intern_Q_NewOakValeIntro`
/ `Expression_plus120_persist_is_not_Q_NewOakValeIntro`;
siblings `q-novi-activator-callers`,
`cactivatequestdef-payloads`,
`gameflow-type33-give`,
`004B2890-empty-first`,
`qst-autostart-list`,
`ini-activate-quest`,
`addtestquest-token`,
`oakvale-later-activate`,
`007EF200-first-plus120`,
`00456A5A-expression-plus120`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Is the host **wrong** that no-save does not activate this name? | **No.** Unique `00CB5AD0` `E8` is `004B42E8` inside `004B4260`. First no-save walk is `0049F24E` `[world+172]`. That vector omits the name. Later `user.ini` is `"Gameflow"`. Type-1 Gameflow **waits**. | **PROVEN** host **MATCH** |
| MATCH construct presenter on no-save? | **None.** Do not invent one. | **DISPROVEN** as recovered |
| `world+172` vs `world+184`? | `+184` always `AddQuest`. `+172` TRUE only. `Q_NewOakValeIntro` is FALSE → catalog `+184` / `QM+44`, not auto-start. | **PROVEN** |
| `user.ini`? | One `ActivateQuest("Gameflow")` via `00419CE0` → vtbl+1104 `00892E80` → `004B4A10`. Not Oakvale. | **PROVEN** |
| `script.bin` intern `0x012C5D14`? | **0 hits** across every inflated payload. `names.bin` has no that string. | **PROVEN** |
| `CActivateQuestDef` 16-byte names? | NULLDEF / `Global_OpenChest` ×2 / `Global_GiveHeroItemsFromRewardChest` / `Global_TeleportToHeroGuild` / `Global_ToggleTimeDisplay`. No intern dword `0x012C5D14`. | **PROVEN** (closes sibling UNREAD hex) |
| `0061AB30`? | Debug picker `[this+343]`. `004B4A10` at `0061AC28` from `world+196`. **Not** New Game. | **PROVEN** leftover; **DISPROVEN** as no-save |

---

## Verdict

**The host is not wrong.** No recovered no-save site
`004B4260`s / `004B4A10`s / `00CB5AD0`s
`Q_NewOakValeIntro`. There is **no MATCH presenter**
on this walk.

Gameflow `00CE7670` state 0 **waits forever** on a
type-`0x33` Give named that intern (`00893570`
vtbl+100). Construct of other TRUE names posts
`0x37`. Inventing `ActivateQuest("Q_NewOakValeIntro")`
would still miss the wait (`ActivateQuestSatisfiesGameflowWait=false`).

Remaining **UNKNOWN** is only **later** construct:
which live Thing / copied CString first supplies intern
`0x012C5D14` to `004B4A10` **after** a region exists.
That is off the no-save Type-1 walk (`CurrentRegion=null`).

Stale ExeIndex `03-pseudo/newgame.md` “`START_NEW_QUEST`
/ `AddTestQuest` start `Q_NewOakValeIntro`” is
**DISPROVEN**. That is not a host DIVERGE.

---

## KNOWN / INFERRED / UNKNOWN

| Item | Class | Evidence |
|---|---|---|
| Unique `00CB5AD0` `E8` = `004B42E8` in `004B4260` | **KNOWN** | listings: **one** `call 00CB5AD0` |
| First no-save `004B4260` = `0049F24E` `[world+172]` | **KNOWN** | `lea edx, [esi+172]` |
| `world+172` nine TRUE names; no Oakvale | **KNOWN** | `Init_quests_*`; `DoesNotContain` |
| `world+184` / `QM+44` **contain** Oakvale | **KNOWN** | `AddQuest FALSE`; `Contains(WorldPlus184)` |
| `ActivatedQuests` = nine TRUE + `"Gameflow"` | **KNOWN** | same test `[9]=="Gameflow"` |
| `user.ini` `ActivateQuest("Gameflow")` only | **KNOWN** | TLC file; `00419CE0` `[edx+1104]` |
| Five `.text` pushes of intern `0x012C5D14` | **KNOWN** | bind `00CD6E28`/`00CD6E87`; card+wait `00CE791E`/`00CE7978`/`00CE79CA` |
| Bind `00CD6E27` is `00CB5C90` factory register | **KNOWN** | `[esp+32]=0xDBEF70`; not `00CB5AD0` |
| `00CE7670` is wait `vtbl+100`, not construct | **KNOWN** | `call [edx+100]` then `006E7410` yield |
| First `004B2890` empty `QM+112` skip | **KNOWN** | `0049F259`; no `004B4A10` |
| `+90584` empty → skip `00416C11` | **KNOWN** | `0099E960` vs `0x122D70E` |
| Eight `E8` of `004B4A10`; none push `0x012C5D14` | **KNOWN** | table below |
| Six `E8` of `004B4260`; first-seen lists omit Oakvale | **KNOWN** | table below |
| `AddTestQuest` → `world+196` only | **KNOWN** | `004A113B`; no `004B2850` |
| `0061AB30` `[+343]` leftover picker | **KNOWN** | `0061AC28` / `004B4C50` |
| `CActivateQuestDef` six payloads ≠ Oakvale | **KNOWN** | hex + `names.Get` |
| `script.bin` intern 0 hits | **KNOWN** | every `Raw` dword |
| `EXPRESSION+120` persist ≠ Oakvale | **KNOWN** | 36× `-1`; three `Expression_Pick*` |
| Lookout TNG 0 Oakvale / 0 `StartCTCExpression` | **KNOWN** | `007EF200-first-plus120` |
| `StartOakValeWest` TNG = `XXXSectionStart` bucket | **KNOWN** | not a CTC field |
| Childhood TNG queues `ActivateQuest` | **KNOWN** false | `ChildhoodTngQueuesActivateQuest=false` |
| `004B5080` 0 no-save inbound `E8` | **KNOWN** | save parse only |
| `00DBEF70` / `00DABAC0` 0 `E8` | **KNOWN** | run only after construct |
| Type-1 yield; resume still miss; `CurrentRegion=null` | **KNOWN** | Type-1 + resume + No_save tests |
| Later construct **must** still go through `004B4260` | **INFERRED** | unique lookup is inside that fn; `004B4A10` is the 1-name wrapper (`004B4A5A`) |
| Later name is a **copied CString**, not a PE push | **INFERRED** | five intern xrefs already classified; remaining callers copy `[+120]` / `[+168]` / `[+40]` / picker record |
| First live Thing whose copied name **is** `0x012C5D14` | **UNKNOWN** | after a region; not Lookout; not no-save Type-1 |
| `FableCrc("Q_NewOakValeIntro")` `.text` imm | **UNKNOWN** | would be a **different** table, not `004B4A10` |
| `004B5080` save `START_NEW_QUEST` operand | **UNKNOWN** | off no-save |
| `+90584` contents if ever nonempty | **UNKNOWN** | no-save skip **PROVEN** |
| `004B4B5F` live `[comp 0x6C +40]` as this intern | **UNKNOWN** | use-item / debug; not first no-save |

No row is a MATCH no-save presenter. Do not invent one.

---

## 1. Host claim under audit

`docs/status/README.md` / `docs/PARITY.md` /
`No_save_does_not_activate_Q_NewOakValeIntro`:

- `00CD6E27` bind-only
- WLD / `+90584` / `004B5080` / `AddTestQuest` /
  `00896A30` are not the activator
- `user.ini` is `Gameflow`
- Type-1 `00CE7670` yields on the name; no Oakvale
  in `ActivatedQuests` / `Runtime.Quests`

Type-1 extras that the host also MATCH:

- `GameflowWaitsForeverOnNoSave=true`
- `ActivateQuestSatisfiesGameflowWait=false`
- `QuestConstructEventKind=0x37` ≠ `QuestGiveEventKind=0x33`
- `EventPosts=10` all construct
- `004167DA` not called on this pump

To prove the host **wrong** would need a no-save
listing site that `004B4260`s this intern before
first region. **None recovered.**

---

## 2. `world+172` vs `world+184` (catalog ≠ start)

`listing-00480000.txt` `AddQuest` after TRUE/FALSE parse
(`bl`):

```
004A1080  lea esi, [ebp+184]       ; ALWAYS
004A10B2  test bl, bl
004A10B4  je  004A10F6             ; FALSE skips +172
004A10C4  lea esi, [ebp+172]       ; TRUE only
004A10F6  mov ecx, [0x13B89FC]
          call 004B2850            ; ALWAYS → QM+44
```

Init Quests:

```
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260
0049F259  call 004B2890            ; empty +112; NOT activate
```

`world+172` (host `WorldPlus172`):

1. `Q_SunnyvaleMaster`
2. `ChapterAndSceneManager`
3. `PersonalScriptMain`
4. `PersonalScript_GlobalThings`
5. `NPCDeath`
6. `HeroBoasts`
7. `V_HeroDolls`
8. `CS_PlayCutscene`
9. `Global_WatchForHeroDeath`

`Q_NewOakValeIntro` is `AddQuest(..., FALSE)` plus one
`AddTestQuest` card. It is on `+184` / `QM+44` / `+196`.
It is **absent** from the `0049F24E` walk. Walking `+184`
here would **DIVERGE**.

WLD `START_INITIAL_QUESTS` is a **subset** of TRUE and
is **not** the writer.

---

## 3. Unique construct lookup

`listing-00480000.txt`:

```
004B42A2  push "QuestManager: Activate Quest"
004B42D7  call 004B00C0            ; QM+44 membership
004B42DE  je  004B4363
004B42E8  call 00CB5AD0            ; UNIQUE E8
          004BB720 factory or 0
004B4386  call 004B3CE0            ; once; kind 0x37 or stub
```

Six `call 004B4260`:

| Site | Parent | No-save Oakvale? |
|---|---|---|
| `0049EAD1` | stub `0049EAC0` `+0xAC` | 0 inbound **PROVEN** skip |
| `0049F24E` | Init Quests `[world+172]` | **DISPROVEN** (name absent) |
| `004B4A5A` | `004B4A10` 1-name wrapper | first-seen `"Gameflow"` / empty skip |
| `004B5B84` | save `START_ACTIVE_QUESTS` | not no-save |
| `00892EAF` | vtbl 277 `00892EA0` | 0 first-seen FF |
| `00892EEF` | vtbl 279 `00892EE0` | 0 first-seen FF |

`00DBEF70` / `00DABAC0` have **zero** `E8`. They run
only after this lookup hits and `004B3CE0` does
`call [eax+4]` / `call [edx+8]`.

---

## 4. Eight `004B4A10` sites — none a PE Oakvale push

```
004B4A10  sub esp, 12
          push 1; push 1
          mov ecx, [esp+36]        ; arg0 CString*
          call 00433530            ; 12-byte name list
004B4A5A  call 004B4260
          ret 12
```

| `E8` | Real fn | Name | No-save Oakvale? |
|---|---|---|---|
| `00416C11` | `00416953` tail | `[game+90584]` | empty skip **PROVEN** |
| `004B4B5F` | `004B49E0` | `[comp 0x6C +40]` | not this intern as PE; live **UNKNOWN** |
| `004B4D45` | `004B4C50` | copy of same +40, flags (1,1) | debug / `0061AC1C` only |
| `0061AC28` | `0061AB30` | `world+196` record | leftover `[+343]` |
| `007EF3A1` | `007EF200` | nested `[esi+120]` copy | persist **DISPROVEN**; live Thing **UNKNOWN** |
| `0084407E` | `00843FC0` | `[this+168]` | ctor literals `Expression_*`; def `+40` **DISPROVEN** Oakvale |
| `00892E8F` | `00892E80` | ini/script CString (1,1) | `"Gameflow"` |
| `00892ECF` | `00892EC0` | same CString (1,0) | 0 first-seen FF |

`0061AB30` head (`listing-00600000.txt`):

```
0061AB30  sub esp, 24
          mov edi, ecx
          mov al, [edi+343]
          test al, al
          je  0061AC50             ; skip whole body
          …
0061AC28  call 004B4A10            ; empty card → activate record+0
```

**DISPROVEN** as New Game. **PROVEN** as the
`world+196` consumer.

---

## 5. `user.ini` Gameflow — not Oakvale

`listing-00400000.txt` handler `00419CE0`:

```
00419CE4  mov ecx, [0x13B86A0]
          call [eax+36]            ; 004197B0 xor al,al
00419D11  mov eax, [edx+36]        ; world
00419D14  mov ecx, [eax+56]        ; script manager
00419D1E  call [edx+1104]          ; 00892E80
```

```
00892E80  mov eax, [esp+4]
          mov ecx, [0x13B89FC]
          push 1; push 1; push eax
00892E8F  call 004B4A10
          ret 4
```

TLC `user.ini` (install-wide grep: **one**
`ActivateQuest` line):

```
ActivateQuest("Gameflow");
```

Direct `00CB5AD0` from the ini walker is **DISPROVEN**.
`userst.ini` has **zero** `ActivateQuest` and is applied
at command line, before the command is registered.

---

## 6. Bind + Gameflow wait are not construct

Five intern xrefs (`xrefs.tsv` `0x012C5D14`):

```
00CD6E27  push "Q_NewOakValeIntro"
          mov [esp+32], 0xDBEF70
          call 00CB5C90            ; BIND

00CE791D  push "Q_NewOakValeIntro"
          push "OBJECT_QUEST_CARD_OAKVALE_INTRO"
          call [edx+1180]          ; card bind; NOT Give
00CE7977  push "Q_NewOakValeIntro"
          call [edx+100]           ; 00893570 type 0x33
          je  skip wait
00CE79B0  call [edx+28]            ; 006E7410 yield
00CE79C9  push "Q_NewOakValeIntro"
          call [eax+100]
          jne 00CE79B0             ; FOREVER on miss
```

No `call […+1152]`, no `004B1D30`, no `00CB5AD0` in
`00CE7670`. Later Give of this **name** is `00DBE295`
after `StartOakVale` map-wait **and** `AttackOver`,
and only while S_QNOVI is `QM+136` — which itself
needs the unread construct. **Blocked** on no-save.

---

## 7. Closed UNREADs (were presenter candidates)

### `CActivateQuestDef` 16-byte intern

`GameBinFormatTests.CActivateQuestDef_payloads_*`:

| Id | Hex | `names.Get(+7)` |
|---:|---|---|
| 61 | `0000001B5AB31FFFFFFFFF784B39BF01` | empty (`-1`) |
| 9241 | `0100011B5AB31F8D1F0500784B39BF00` | `Global_OpenChest` |
| 9248 | same | `Global_OpenChest` |
| 12277 | `…EFA10500…` | `Global_GiveHeroItemsFromRewardChest` |
| 12857 | `…51A60500…` | `Global_TeleportToHeroGuild` |
| 12874 | `…D0A60500…` | `Global_ToggleTimeDisplay` |

Every 4-byte window **≠** `0x012C5D14`. Sibling
`cactivatequestdef-payloads` hex UNREAD is **closed**.

`00843F50` six `E8`: `Expression_Follow` / `Wait` /
`Fish` / `Dig` literals plus two `[def+40]` copies.
**None** push `0x012C5D14`. Sibling next-dump (4)
**closed**.

### `script.bin` intern 0 hits

`Script_bin_payloads_do_not_intern_Q_NewOakValeIntro`
walks every inflated `Raw` for LE `0x012C5D14`.
**Zero.** `names.Find("Q_NewOakValeIntro")==null`, so
a names.bin offset cannot resolve to that string
either.

### `EXPRESSION+120` persist

39 rows. 36 store `-1`. Three store
`Expression_Pickpocket` / `Picklock` / `Steal`.
No intern dword. `007EF200` still only copies
whatever nested `+120` holds at tick time.

### TNG / childhood

Lookout (first no-save TNG): **0** Oakvale strings,
**0** `StartCTCExpression`. `StartOakValeWest`:
`XXXSectionStart Q_NewOakValeIntro` →
`ThingInstance.Section` only (chicken-egg vs
first Lookout Present). `ChildhoodTngQueuesActivateQuest=false`.

---

## 8. Remaining UNREAD that could still be the construct presenter

Enumerate **every** leftover that can still feed
`004B4260` of intern `0x012C5D14`. None of these
is on the no-save Type-1 walk.

1. **First live Thing after a region exists** whose
   runtime CString equals the intern, via:
   - `007EF200` nested `[esi+120]` (`[esi+116]==0`)
   - `00843FC0` `[this+168]` from `CActivateQuestDef+40`
     (payloads themselves are **not** Oakvale, so the
     def must be a **later** instance or a copied
     CString, not the six `game.bin` rows)
   - `004B4B5F` component `0x6C` record `+40`
   - `00892E80` / `00892EC0` / `00892EA0` with a
     **copied** CString (no second PE xref)
2. **Which TNG / region first hosts that Thing.**
   Not Lookout. Not `StartOakValeWest` section
   buckets. Not `00501450` first Present.
3. **`FableCrc("Q_NewOakValeIntro")` `.text` immediate.**
   Activation recovered here is CString intern. A CRC
   hit would be a **different** table, not `004B4A10`.
4. **`004B5080` `START_NEW_QUEST` operand** — 0 no-save
   `E8`; save parse only.
5. **`[game+90584]` if a later writer fills it** —
   no-save compare vs empty intern **skips**.

Until (1) dumps a live name equal to `Q_NewOakValeIntro`,
the construct presenter stays **UNKNOWN**. Do **not**
fill the gap with `ActivateQuest("Q_NewOakValeIntro")`.

---

## Timeline (no-save) — still no Oakvale construct

```
00CD6E27  00CB5C90 bind Q_NewOakValeIntro / S_QNOVI / 00DBEF70
004A0D90  FinalAlbion.qst
  AddQuest FALSE Q_NewOakValeIntro → +184, QM+44
  AddTestQuest → +196 only
0049F24E  004B4260([world+172])     // nine TRUE names
004B42E8  00CB5AD0                  // unique; not Oakvale
          004B3CE0 → 00687540(55,50)
0049F259  004B2890                  // empty +112 skip
00416BCF  +90584 empty skip 004B4A10
user.ini  00892E80 004B4A10 "Gameflow"
first type-1 00CB8220
  00CE7670  00893570("Q_NewOakValeIntro") = 0 → yield
resume    same miss
          CurrentRegion=null
007EF200 / 00843FC0 / 0061AB30      // not here
```

---

## Host

| Host | Native | Class |
|---|---|---|
| `ActivateNamedQuest` walks `_worldPlus172` only | `0049F24E` | **MATCH** |
| `WorldPlus184` has Oakvale; `ActivatedQuests` does not | catalog vs walk | **MATCH** |
| `ActivateQuest("Gameflow")` from `user.ini` | `00419CE0` → `00892E80` | **MATCH** |
| Note `00CD6E27` bind not `00CB5AD0` | five intern xrefs | **MATCH** |
| Note `00416BCF` skip `004B4A10` | empty `+90584` | **MATCH** |
| Note `004A113B` store not `004B4A10` | `world+196` | **MATCH** |
| Note `0061AB30` not New Game | `[+343]` | **MATCH** |
| Type-1 yield; not in `Runtime.Quests` | `00893570` miss | **MATCH** |
| Invent `ActivateQuest("Q_NewOakValeIntro")` | none on no-save | **DISPROVEN** if added |

No src DIVERGE. Do not add a recovered-caller Note that
pretends `007EF200` / `00843FC0` / `0061AB30` ran Oakvale
on no-save.

---

## Classifications (short)

1. **Host no-save omit — MATCH / PROVEN.** Unique
   `00CB5AD0` never sees this name on this walk.
2. **No MATCH construct presenter — PROVEN absent.**
   Do not invent `ActivateQuest("Q_NewOakValeIntro")`.
3. **`script.bin` intern 0 / `CActivateQuestDef` six
   names / `EXPRESSION+120` persist — PROVEN not
   Oakvale.** Closes prior payload UNREADs.
4. **Remaining UNKNOWN — later live copied CString
   into `004B4A10` after a region exists; FableCrc
   table; save `004B5080` operand.** Off no-save.
