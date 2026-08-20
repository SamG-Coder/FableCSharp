# Childhood Oakvale objectives — live kill, not a smash heuristic

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent a barrel smash heuristic (radius,
physics contact, anim event, health, LMB-near-barrel,
action 26/27/28, `CKickableDef`). Smash is a **live
Thing kill**. `WatchBarrels` only **polls**
`[quest+116]`.

Do **not** auto-complete quests, objectives, gold,
deeds, or `AttackOver`. Do **not** invent
`ActivateQuest("Q_NewOakValeIntro")`. Do **not**
write `+116=1` from Pump. Do **not** poke
`HeroGold` to skip `WatchForGotGold`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: dump `Fable.exe`
`listing-00d80000.txt` (`00DABAC0` /
`00DBDE40` / `00DBE890` / `00DBE2E0` /
`00DB7DB0` / `00DB7E10` / `00DAEA70` /
`00DB0660` / `00DB4095` / `00DB97A0` /
`00DBE3C0`); `listing-00c80000.txt`
`00CB7950` / `00CB7940`;
`listing-00f00000.txt` `00F35A00` /
`00F35A30`; `listing-004c0000.txt`
`004C9B80`;
`tools/Fable.ExeIndex/out/01-sections/script-bank/quests-qst.md`
(no repo `dump/scripts/quests` tree);
`xrefs-by-string.tsv` `TEXT_QUEST_OAKVALE_INTRO_OBJECTIVE_*`;
`src/Fable.Game/RegionTravel.cs` `WatchBarrels*`;
`ScriptFactoryTable.Barrel*`;
siblings `proofs/watchbarrels-00DBE890`,
`proofs/watchbarrels-smash-vtbl20`,
`proofs/novi-factory-starts`,
`proofs/raid-avi-attackover-live`,
`proofs/004F67BA-gold-kickable`,
`proofs/action27-release`,
`proofs/type6-action28`,
`proofs/q-novi-activator-callers`,
`proofs/qst-autostart-list`,
`proofs/00DBDE40-host-gap`.

There is **no** `dump/scripts/quests` folder in this
workspace. QST names below are the ExeIndex dump of
shipped `FinalAlbion.qst`. `S_QNOVI` is **native**,
not a `script.bin` entry (`native-sqnovi.md`).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| What completes a childhood barrel smash? | Live kill of the bound `NOVI_Barrel` Thing (`004C9B80`). Then `00F35A00` gone → `00CB7950` `vtbl+20` `00DB7DB0` writes `[quest+116]`. `WatchBarrels` polls that byte. | **PROVEN** writer + poller; dispatcher **MATCH** host const |
| May the host smash on radius / LMB / kickable? | **No.** | **DISPROVEN** |
| Childhood objective order? | `01` names → deeds (`02` good / `03` gold>2 / `04` sweets) → `05` Theresa MEET_YES **last**. `06` is PostAttack. | **PROVEN** sites |
| Auto-complete any of those? | **No.** | **DISPROVEN** |
| Is `CGoldDef` / `CKickableDef` this gold / kick? | **No.** Init Thing Components. Last-barrel gold is `WatchBarrels` `vtbl+2340("OBJECT_GOLD_1")`. | **DISPROVEN** (`004F67BA-gold-kickable`) |
| Is action 27 / 28 smash or kick? | **No.** 27 = RMB hover-in. 28 = LMB-up unarm. | **DISPROVEN** |
| No-save Pump runs this? | **No.** Activator **UNREAD**. `AddQuest(..., FALSE)`. | **DISPROVEN** live |

---

## Verdict

**Native childhood objectives are a live chain on
`Q_NewOakValeIntro` / `S_QNOVI`. Host data MATCH.
Live run DISPROVEN. Do not fake the chain.**

Barrel smash is **not** a host test. The Thing must
die. `00DB7E10` radius `2.0` is instruction text,
not smash. Action 26/27/28 are frontend widgets,
not `+116`. `WatchBarrels` never walks barrel
physics.

Objectives **01–05** run **while** `00DBDE40`
spins on `[quest+80]` (`AttackOver` still 0).
Objective **06** is `00DBE3C0` PostAttack **after**
`00DBB2A7`. Do not collapse 05 into 06. Do not
write `AttackOver=1` to skip deeds.

---

## 1. QST childhood list (dump)

`script-bank/quests-qst.md` / shipped
`FinalAlbion.qst`. All **FALSE** except noted.

| Name | Persistent | Childhood? |
|---|---|---|
| `Q_NewOakValeIntro` | False | **yes** — main `S_QNOVI` |
| `Q_NewOakValeIntro_PreAttack` | False | **yes** — named from `00DBDE40` after watchers |
| `Q__OakValeIntro_PostAttack` | False | **no** — after `AttackOver` |
| `Q_HerosOldHouse` | False | **no** as activate — `00DBDE40` **lookup** string |
| `Q_OakValeBanditRaid` | False | **no** — adult raid family |
| `V_ChickenKicking` | False | **no** — adult minigame |
| `CS_OakValeRevisited` | False | **no** |

`AddQuest("Q_NewOakValeIntro", FALSE)` → `world+184` /
`QM+44` only. **Not** `world+172` auto-start
(`qst-autostart-list`). `AddTestQuest` card
(`addtestquest-token`):

```
AddTestQuest("Q_NewOakValeIntro", "NOVStartHSP", 2,
  "Q Oak Vale Introduction", "", "OakValeIntro.end",
  "OBJECT_QUEST_CARD_OAKVALE_INTRO");
```

That card is `world+196` only. **DISPROVEN** as
activate. `ChildhoodTngQueuesActivateQuest=false`
(West TNG has no `CActivateQuestDef`). Who later
`00CB5AD0`s the name remains **UNREAD**
(`q-novi-activator-callers` / `oakvale-later-activate`).

Do **not** `ActivateQuest` from Pump / Leave /
`user.ini` (that file is `Gameflow`).

---

## 2. Objective texts — sites and order

`vtbl+2620` then `vtbl+1184` is the HUD set.
Same shape at every site.

| # | String | Writer | When it is legal |
|--:|---|---|---|
| 01 | `TEXT_QUEST_OAKVALE_INTRO_OBJECTIVE_01` | `00DABAC0` `00DAC1BA` (also father `00DB91A9`) | After 16× `00CB8230`, **before** `StartBarrelTimer` / `00DBDE40` |
| 02 | `…_02` | `00DB0660` `00DB080A` | Good-deed fn after teddy `00DB0600`; only if `vtbl+508 < 3` **and** `[quest+148]==0` |
| 03 | `…_03` | `WatchForGotGold` `00DBE2E0` `00DBE34F` | Inventory `vtbl+508 > 2` |
| 04 | `…_04` | BookTrader `00DB4095` `00DB4A93` | After sweets / `OBJECT_CHOCOLATE_BOX_UNGIVEABLE`; then `[quest+148]=1` |
| 05 | `…_05` | Theresa `00DB97A0` `00DB9DE6` (also `00DBA278` / `00DBA767`) | After `CS_OAKVALE_INTRO_THERESA_MEET_YES` — **last childhood** |
| 06 | `…_06` | `00DBE3C0` `00DBE478` | PostAttack env. **Not** a childhood deed |

**Order that must stay live** (do not shuffle, do not skip):

```
activate Q_NewOakValeIntro                    // UNREAD who
00DABAC0
  16× NOVI_* name+factory
  OBJECTIVE_01
  StartBarrelTimer 00DB4F70
  00DBDE40
    map-wait StartOakVale
    CREATURE_HERO_CHILD
    WatchBarrels     00DBE890
    WatchForGotGold  00DBE2E0
    ManageQuestCoreMarkers 00DBE4E0
    Q_NewOakValeIntro_PreAttack
    vtbl+2584(12.0); HerosOldHouse; spin [+80]
NOVI_LiveFather 00DB86B0  CS_OAKVALE_INTRO_FATHER
  // first-seen: in-house, FirstSeenHandsPlayerControl=false
player control (later)
PreAttack TNG living NPCs + named NOVI_Barrel Things
  // IntroQuestTngHasNoviNames=false
live kill barrels → +116 → WatchBarrels edi
  edi==1     00DAEA70(0)     // bad deed +84/+88
  edi==N-1   OBJECT_GOLD_1
gold pickup until vtbl+508 > 2
  WatchForGotGold → OBJECTIVE_03
teddy give 00DB0600 OBJECT_TEDDY_BEAR_UNGIVEABLE
  00DB0660 inc +84 → FIRST_GOOD_DEED → OBJECTIVE_02
BookTrader sweets → OBJECTIVE_04, [+148]=1
Theresa MEET_YES chocolate → OBJECTIVE_05
  // STOP childhood here
00DB97A0 r=2.0 then CS_OAKVALE_INTRO_THERESA
  raid AVI 1_raid_on_oak_vale_comp.xmv
  00DBB2A7 AttackOver=1
00DBE3C0 OBJECTIVE_06 + ENVIRONMENT_OV_POSTATTACK
```

`02` / `03` / `04` are **parallel** wander/watchers
on the same quest object while the `+80` spin holds.
`05` is last. `06` is after the raid store.
**PROVEN** sites. Host runs **none**.

---

## 3. WatchBarrels live kill (not a heuristic)

### 3a. Poller — **PROVEN**

`00DBDE40` after `CREATURE_HERO_CHILD` attaches
three `00CDD450` (0.1f / 64 / 1). First is
`"WatchBarrels"` callback `00DBE890`. Fiber
`00DAAD70` `ecx=[+56]` (quest) `call [+52]`.

```
vtbl+300("NOVI_Barrel") → 12-byte vector
N = (end-begin)/12
[esi+116] = 0
edi = 0
loop:
  if [esi+80]  → ret                 ; AttackOver
  if [esi+116] == 0 → yield, loop
  inc edi
  [esi+116] = 0
  if edi == 1   → 00DAEA70(0)
  if edi == N-1 → vtbl+288 / vtbl+2340("OBJECT_GOLD_1")
  if edi >  N-4 → beetle             ; leftover, FirstSeenWatchBarrelsSpawnsBeetle=false
```

Smash count is **local** `edi`. **DISPROVEN** as
physics / anim / `00DB7E10` inside this fn.

### 3b. `+116` writer — **PROVEN**

`NOVI_Barrel` `vtbl+20` `00DB7DB0`:

```
[quest+116] = 1
[quest+117] = 1
copy 12 bytes [thing-ref.vtbl+24] → quest+118
```

Start `00DB7E10` does **not** write the latch
(`BarrelStartWritesLatch=false`). Event-1
`00CDEE00` is dtor. **DISPROVEN** as smash.

### 3c. Live gone path — **PROVEN** pieces, **PARTIAL** ecx

Thing death `004C9B80`:

```
or [thing+146], 4
or [thing+145], 1          ; dead bit
vtbl+48; 004C8C00
005202B0; 0051E000         ; drop from world lists
```

`00F35A00` on a 00CB7950 arg:

```
ecx = [arg+44]             ; bound Thing (or null)
if ecx == 0: al = 1        ; no bind = not-gone
else jmp [thing.vtbl+0]    ; alive?
```

`00CB7950` when that returns 0:

```
[+5] = 1
00F35A30                   ; clear +44/+48
call [arg.vtbl+20]
```

Host names this `BarrelSmashCaller=00CB7950` /
`BarrelThingGoneFn=00F35A00` /
`BarrelKillFn=004C9B80`. **MATCH** VAs.

`watchbarrels-smash-vtbl20` still **DISPROVEN**
`00CB7950` as a call of **watcher** `0x012D7A3C+20`
(`00CDD410` `ret`). The gone-path `vtbl+20` that
must fire is the **script** object `0x012D94F0+20`
=`00DB7DB0`. Exact `ecx` at the `FF 52 14` for that
vtbl was UNREAD there; host const later names the
same dispatcher when the **bound Thing** is gone.
Do **not** fill the gap with a smash helper.

### 3d. What 2.0 **is** — **DISPROVEN** as smash

`00DB7E10` `00CBE2FF` `dist^2 < 2.0^2` then
`TEXT_QST_048_INSTRUCTION_BREAK_BARRELS[_PC]`.
Instruction wait. `WatchBarrels` never calls
`00CBE2FF`. `00DB7DB0` has no float.

### 3e. Do not invent

| Temptation | Why it is wrong | Class |
|---|---|---|
| `if dist < 2 smash` | 2.0 is start text | **DISPROVEN** |
| `if LMB near barrel smash` | smash is Thing death | **DISPROVEN** |
| action 26 persist = smash | 26 is widget accept post | **DISPROVEN** |
| action 27 release = smash | 27 is RMB hover-in (`action27-release`) | **DISPROVEN** |
| action 28 LMB-up = kick | 28 is unarm (`type6-action28`) | **DISPROVEN** |
| `CKickableDef` = barrel / chicken | class register only (`004F67BA`) | **DISPROVEN** |
| host `quest+116=1` | writer is `00DB7DB0` after kill | **DISPROVEN** |
| auto-complete `WatchBarrels` edi | count is live rising edges | **DISPROVEN** |
| first-seen beetle / gold | leftover thresholds | **LEFTOVER** |

---

## 4. Deeds vs gold vs trader vs Theresa

`+84` good count (`00DB0660` `inc [esi+84]`).
`+88` bad count (`00DAEA70` `inc [esi+88]`).
`vtbl+624` morality; bad path `fchs`.
`[+148]` BookTrader latch.

| Event | Fn | Side | Objective? |
|---|---|---|---|
| First barrel smash | `00DBE890` `edi==1` → `00DAEA70(0)` | bad | message `DID_FIRST_BAD_DEED`, not 01–05 |
| Teddy return | `00DB0600` `OBJECT_TEDDY_BEAR_UNGIVEABLE` → `00DB0660` | good | `02` if gold&lt;3 and `+148==0` |
| Gold count &gt; 2 | `00DBE2E0` `vtbl+508` | inventory | `03` |
| Sweets / chocolate box | `00DB4095` | trader | `04`, then `+148=1` |
| MEET_YES chocolate | `00DB97A0` | Theresa | `05` last |

`00DAEA70` also has wander callers (Guard / Affair /
Bully / …). Barrel smash is the **first-seen-on-fiber**
bad-deed site, not the only one. Do not auto-inc `+88`.

Last-barrel gold is `vtbl+2340("OBJECT_GOLD_1")` from
`WatchBarrels`, **not** `CGoldDef` (`004F67BA`).
Pickup until `vtbl+508 > 2` is **UNREAD** apply
(not `GiveGold` opcode). Do not `HeroGold += n`.

---

## 5. Gold / Kickable / action 27 / 28 — not childhood smash

`004F67BA-gold-kickable` remaining-pairs 69–74 are
Init Thing Components after Leave. **DISPROVEN** as
`00DBDE40` / `OBJECT_GOLD_1` / chicken kick.

`action27-release`: table `[1]=0055AE01` hover-in.
Not persist release. Not smash.

`type6-action28`: LMB **up** → action 28 unarm
`vtbl+588`. Not kick. Not `+116`.

Frontend actions are legal **later** as the player’s
attack/use that may **kill** a barrel Thing. They are
not a substitute for `004C9B80` + `00DB7DB0`.

---

## 6. What must be live (strict order)

Stop at the first missing step. Do **not** complete
a later objective to hide a gap.

1. **Proven activate** of `Q_NewOakValeIntro`.
   Still **UNREAD**. Host must **not** invent it.
2. **`00DABAC0`** name table (16 `NOVI_*`) **before**
   deeds. Factory ≠ start (`novi-factory-starts`).
3. **OBJECTIVE_01** from that run, then
   `StartBarrelTimer`, then **`00DBDE40`**.
4. **Map-ready** `StartOakVale` (`vtbl+48`). Do not
   invent `PlayerRegionName` /
   `StartOakValeSetupLoadsRegion=false`.
5. **Kid** `CREATURE_HERO_CHILD` and the three
   watchers. Fiber `00A446A0` → `00DAAD70` →
   `00DBE890` / `00DBE2E0` must **tick**.
6. **PreAttack TNG** holds living NPCs + named
   barrels (`PreAttackTngHoldsLivingNpcs=true`).
   West first-seen TNG has **no** `NOVI_*`.
7. **Father CS** then **player control**. First-seen
   is in `HerosOldHouse` with no control.
8. **Live Thing kill** `004C9B80` on a bound
   `NOVI_Barrel`. Then `00F35A00` gone and
   `00DB7DB0` `+116`.
9. **Live `WatchBarrels`** edges on that latch.
   First edge `00DAEA70`. Last-but-one gold spawn.
10. **Live gold pickup** until `vtbl+508 > 2` →
    OBJECTIVE_03.
11. **Teddy** `00DB0600` / **trader** `00DB4095` for
    02 / 04. Do not set `+84` / `+148` from Pump.
12. **Theresa MEET_YES** OBJECTIVE_05. Then the
    raid tail (`raid-avi-attackover-live`). Do **not**
    `ApplyPersist("AttackOver", true)`.

`PumpUntilSettled` may skip **opcode** PlayAVI
(DIK analog) on the **father dream** file. It must
**not** skip the later native raid AVI, and must
**not** complete flag waits (`ScriptRuntime`
comment). Same rule here: **do not auto-complete**.

---

## 7. Host map

| Piece | Original | Host | Class |
|---|---|---|---|
| QST FALSE names | `quests-qst.md` | `QuestFile` `AddQuest` only | **MATCH** catalog; `AddTestQuest` rows dropped (**PARTIAL**) |
| Bind `S_QNOVI` | `00CD6E27` | `QuestFactoryTable` | **MATCH** data |
| Activate | UNREAD | skip Notes | **MATCH** omit |
| `WatchBarrels*` consts | `00DBDE40` | `RegionTravel` | **MATCH** data |
| Fiber `00DBE890` | after activate | not run | **PROVEN** gap (`00DBDE40-host-gap`) |
| `00DB7DB0` / +116 / vtbl 20 | listing | `BarrelSmash*` | **MATCH** data |
| `00CB7950` gone → vtbl+20 | listing | `BarrelSmashCaller` | **MATCH** VA; live invoke **absent** |
| `004C9B80` | Thing death | none on barrels | **PROVEN** gap |
| `00DAEA70` / `00DB0660` | ±88 / +84 | **absent** | **PROVEN** gap |
| OBJECTIVE_01–05 | vtbl+2620 | **absent** | **PROVEN** gap |
| `CGoldDef` / `CKickableDef` | class register | Note-only | **LEFTOVER**; **DISPROVEN** childhood |
| action 27 / 28 | hover-in / unarm | `FrontendInputMap` | **MATCH** frontend; **DISPROVEN** smash |

---

## Classifications (short)

1. **Childhood objectives 01–05 are live HUD sets on
   `S_QNOVI` — PROVEN** sites. `06` is PostAttack
   (**DISPROVEN** as a childhood deed).
2. **Barrel smash is live Thing kill + `+116` poll —
   PROVEN** as `004C9B80` / `00DB7DB0` / `00DBE890`.
   Heuristic smash **DISPROVEN**.
3. **Gold / kickable class register is not Oakvale —
   DISPROVEN.** Last gold is WatchBarrels leftover
   spawn. Pickup **UNREAD**.
4. **Action 27/28 are not smash/kick — DISPROVEN.**
5. **Auto-complete / invented activate / `AttackOver=1`
   from deeds — DISPROVEN.**
6. **Host — MATCH constants, PROVEN gap** on activate,
   fiber, kill, latch, deeds, and all six texts.

---

## Next UNREAD

1. Who `00CB5AD0`s / `004B4A10`s `Q_NewOakValeIntro`
   after a region exists. Not this note. Not Pump.
2. Exact `ecx` at `00CB79B4`/`00CB79C6` when it is
   a `0x012D94F0` object (smash-vtbl20 leftover).
   Keep `BarrelSmashCaller=00CB7950` as the gone
   dispatcher. Do **not** add a heuristic.
3. `OBJECT_GOLD_1` `vtbl+2340` apply / pickup onto
   `vtbl+508`.
4. Do **not** close any of those by completing
   OBJECTIVE_01–05 from the host.
