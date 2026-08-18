# First persist after no-save Leave is Sunnyvale `00CDC070`

Investigation only. No production `src/` edits.

Do **not** start at `S_QNOVI` / `00DAADA0` / `AttackOver`.
That serializer is later `Q_NewOakValeIntro` fiber persist.
It is **not** on Leave / Init Game / first `004B4260`.

`PersistTable.cs` / `PersistTable.Sunnyvale` (38 slots) is
**not** authority. Authority is `listing-00cc0000.txt`
`00CDC070` / `00CDBA10` / `00CDBD20` and
`listing-00480000.txt` `004B4260` / `004B3CE0`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `listing-00cc0000.txt` (`00CDBA10`–`00CDBC48`,
`00CDBD20`–`00CDBD6E`, `00CDC070`–`00CDCA34`,
`00CD52D0` bind, `00CDD360` / `00CDD380` / `00CDD550`);
`listing-00480000.txt` (`004B4260`, `004B3CE0`, `004B3760`,
`004B27F0`, `004AFA10`);
`listing-00400000.txt` (`004045C0`, `00410BE0`, `0040E160`,
`0040E240`);
`listing-00c80000.txt` (`00CB5C90`, `00CB7900`);
`proofs/flag-persist-stores`, `proofs/fiber-first`,
`proofs/script-factory-tables`.

---

## Verdict

First persist *object* after no-save Leave is the
`Q_SunnyvaleMaster` `0x144` block (`vtbl 012C2748`).

| Thing | VA | On no-save first `004B4260`? | Class |
|---|---|---|---|
| Alloc | `00CDBD20` | yes — persist-flag 1, **before** zeros | **PROVEN** |
| First value write | `00CDBA10` (`[vtbl+8]`, `ret`) | yes — immediately after alloc | **PROVEN** |
| Named transfer | `00CDC070` (`ret 4`) | **not entered** (save-map miss) | **PROVEN** skip |
| `AttackOver` | `00DAADA0` | no | **DISPROVEN** as first persist |

`00CDC070` **binds** 81 named slots on that object (and two
globals-backed vectors). It does **not** bind `_LIKE` / `_HATE`.
Those strings are written by `00CDBA10` into
`0x143E938` / `0x143E93C`.

`00CDC070` is the serializer. First *write* is `00CDBA10`.
Host `Note(00CDC070)` on activate is a label, not an `E8`.

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
004184BD  Init Game → 00416953 FinalAlbion.wld
  [0x13B8648]==0
    0049F180  Init Characters          // CWorld HEADER 004045C0 — not this object
    0043A380  Init GUI
    004B4260([world+172])              // FIRST QUEST ACTIVATE
      loop names:
        "QuestManager: Activate Quest"
        00CB5AD0 lookup
        004BB720 enqueue
      004B4386  004B3CE0               // once, after the loop
        first queued = Q_SunnyvaleMaster
        [record+16]==1                 // only this row
          004B3F17  call [record+4]    // 00CDBD20
            alloc 0x144
            0099A2F0 / [this]=012C2748
            +84 CString ctor
            +308/+312/+316 = 0
          004B3F20  call [vtbl+8]      // 00CDBA10  FIRST SLOT WRITE
            +4/+12/+16 = 0
            0x143E934 ← empty 0x122D70E
            0x143E938 ← "_LIKE"
            0x143E93C ← "_HATE"
            0x143E928/92C/930 = 0
            slot zeros + four non-zeros (below)
          004B3F29  004B3760
            004B27F0(manager+12, queue name)
            miss → 004B3807            // skip [vtbl+4] 00CDC070
          004B3F47  [record+0]         // 00CDD550 factory 72, +68=persist
          004B3FEC  00CB7900
            vtbl+12 then vtbl+4 00CDD380 Main
            00CDD450 / 00CB7E50
        later names persist-flag 0
          004AFA10 reuse the 00CDBD20 object
    user.ini ActivateQuest("Gameflow")
      second 004B4260 → 004AFA10 again
00DAADA0 / S_QNOVI                    // not on this list
```

`00CDD360` (Sunnyvale tick) is `vtbl+28` yield then
`00CB7940`. No persist write. **PROVEN**.

---

## Relative order

```
004B4260
  └─ 004B3CE0
       1. 00CDBD20     alloc 0x144, vtbl 012C2748
       2. 00CDBA10     [vtbl+8] zeros + _LIKE/_HATE     ← first write
       3. 004B3760     would call 00CDC070 on save hit
       4. 00CDD550     72-byte factory, stores persist at +68
       5. 00CB7900     Main fiber
```

`00CDC070` sits **after** `00CDBA10` in the same construct
when a save-restore map hits. No-save misses. It is **never**
before `00CDBD20` and **never** before `00CDBA10`.

`00CB5AD0` is lookup only. Fill `00CD52D0` is earlier
(Init World) and only stores `factory=00CDD550`,
`run=00CDBD20`, persist `1`. No object, no zeros, no bind walk.

---

## 1. `00CDBD20` — alloc, not the bind

`listing-00cc0000.txt`:

```
00CDBD20  push 0x144
          00BFEA1A
          0099A2F0
          [esi] = 012C2748
          0099E4B0([esi+84])
          [esi+308/312/316] = 0
          return esi
```

No `E8` to `00CDBA10` or `00CDC070`. No `_LIKE`.
`ecx` unused. Same `ebp` on every `00CD52D0` row
(`00CD532D mov ebp, 0xCDBD20`).

`00CDBDB0` is `mov eax, 0xCDBD20; ret` (getter).

Only Sunnyvale has persist-flag 1, so only Sunnyvale
**constructs** via `[record+4]`. Others `004AFA10` reuse
this object. **PROVEN**.

---

## 2. `00CDBA10` — first write (`[vtbl+8]`)

`004B3F20 call [edx+8]` has **no** stack arg.
`00CDBA10` is `ret`. `00CDC070` is `ret 4`.
So `[vtbl+8]` **cannot** be `00CDC070`. **PROVEN**.

`.rdata` `012C2748` is not in the `.text` listings
(`.rdata` starts `0x0122D000`). Slot `+8` is proven by
arity + the `004B3CE0` site, not by a dumped dword.

### `_LIKE` / `_HATE` (not `00CDC070` names)

```
00CDBA16  push 0x122D70E
          mov ecx, 0x143E934
          0099EFE0
00CDBA2E  push "_LIKE"
          mov ecx, 0x143E938
          0099EFE0
00CDBA3D  push "_HATE"
          mov ecx, 0x143E93C
          0099EFE0
00CDBA4C  [0x143E928] = 0
          [0x143E92C] = 0
          [0x143E930] = 0
```

These are **globals**, not `this+N` persist slots.
`00CDC070` later copies **only** `0x143E928`…`0x143E930`
as `NPCAttitudesTransferVector` (`005815D6`). Loop ends at
`cmp ebp, 0x143E934`. `_LIKE` / `_HATE` are **outside**
that vector. **PROVEN**.

### Object writes (first persist-slot bytes)

Before the globals: `[esi+4]=0`, `[esi+12]=0`, `[esi+16]=0`.
`+4` is `PostSavePosition`.

Then zeros of the named slots (`+17` `HauntedBarrowFieldsCompleted`
first among the `00CDC070` bools), `0099EFE0(0x122DCE0)` onto
`+84` (`TeddySolution`), then the rest.

Four **non-zero** defaults (so “all zeros” is **DISPROVEN**):

| off | `00CDBA10` | `00CDC070` name | serializer default if walked |
|--:|---|---|---|
| +60 | `2` | `ArcheryStateCurrent` | `00CDCA50` → 0 |
| +64 | `2` | `ArcheryStateRequired` | `00CDCA50` → 0 |
| +168 | `10` (`0xA`) | `HighestSkillScore` | `ebx` → 0 |
| +195 | `1` | `SkillRepeating` | `bl` → 0 |

If no-save activate walked `00CDC070` in copy-default mode
(`004045C0` / `00410BE0` case `004045DB` / `00410BFB`), those
four would become 0. They are written **after** alloc and
**are** the live New Game values. That is extra proof the
serializer is not the first writer.

---

## 3. `00CDC070` — what it binds

Prologue: `sub esp, 28`; `esi=ecx` (the `0x144` object);
`edi=[esp+48]` persist context; `ret 4`.

`0 E8` callers. Reached only as a vtbl method (host labels
it `vtbl+4`; rdata dword **UNREAD**). First helper is
`00CDCA40` (`xor eax,eax; ret`).

### Transfer helpers

| Helper | Kind | Default stub on this walk |
|---|---|---|
| `004045C0` | bool (`[ctx+24]` jmp) | `0040E240` `xor al,al` |
| `00410BE0` | int | `0040E160` `xor eax,eax` |
| `00CDCA70` | dword (same mode jmp) | `00CDCA40` → 0 |
| `00CDCC20` | enum/int | `00CDCA50` → 0 |
| `004109A0` | `CString` | `0040E1E0` empty |
| `00CDCDD0` | dword-like | `00CDCA60` → 0 |
| `004106F0` | dword-like | `0040E260` `xor eax,eax` |
| `00410620` | float | `0040E250` `fld [0x122DEDC]` |
| `005815D6` | attitude vector | stack copy of `0x143E928`…`930` |
| `00CDCF80` | byte vector | `esi+239`…`+241` (3) |

`004045C0` / `00410BE0` / `00CDCA70` all `00404500` the
name then `jmp [table+ [ctx+24]*4]`. Mode 0 copies
`*default → *field`. Save/load cases write/read the stream.
No-save construct does not supply this context.

### Named fields in listing order (81)

Authority = `push "…"` in `00CDC070`. Offsets = `lea …, [esi+N]`.

| # | Name | off | helper |
|--:|---|--:|---|
| 1 | `PostSavePosition` | +4 | `00CDCA70` |
| 2 | `HauntedBarrowFieldsCompleted` | +17 | `004045C0` |
| 3 | `GrannyMemoryReturned` | +74 | `004045C0` |
| 4 | `IsLunaHuman` | +75 | `004045C0` |
| 5 | `FriendOfForeman` | +72 | `004045C0` |
| 6 | `BridgeOpened` | +73 | `004045C0` |
| 7 | `ArcheryHighScore` | +68 | `00410BE0` |
| 8 | `ArcheryStateCurrent` | +60 | `00CDCC20` |
| 9 | `ArcheryStateRequired` | +64 | `00CDCC20` |
| 10 | `CondemnedManDead` | +76 | `004045C0` |
| 11 | `CondemnedManForgiven` | +77 | `004045C0` |
| 12 | `CondemnedManMeetsBodyGuard` | +78 | `004045C0` |
| 13 | `CondemnedManMeetsBodyGuardCutSceneStart` | +79 | `004045C0` |
| 14 | `CondemnedManMeetsBodyGuardCutSceneFinished` | +80 | `004045C0` |
| 15 | `TeddySolution` | +84 | `004109A0` |
| 16 | `OrchardFarmRaidLastCompleted` | +88 | `00410BE0` |
| 17 | `OrchardFarmTraderEscortCounter` | +92 | `00410BE0` |
| 18 | `SeenAbbeyMotherAtGuild` | +96 | `004045C0` |
| 19 | `DefeatedThunder` | +97 | `004045C0` |
| 20 | `LostToThunder` | +98 | `004045C0` |
| 21 | `KilledThunder` | +99 | `004045C0` |
| 22 | `CollectedSoulFromArena` | +100 | `004045C0` |
| 23 | `KilledBriar` | +101 | `004045C0` |
| 24 | `CollectedSoulFromMother` | +102 | `004045C0` |
| 25 | `KilledGM` | +103 | `004045C0` |
| 26 | `CollectedSoulFromNostro` | +104 | `004045C0` |
| 27 | `DeliveredSoul` | +108 | `00410BE0` |
| 28 | `CurrentHeroSoulsPosition` | +112 | `00CDCDD0` |
| 29 | `WhisperKilledByHero` | +116 | `004045C0` |
| 30 | `ArenaFinished` | +117 | `004045C0` |
| 31 | `HangingTreeBanditKilled` | +292 | `004045C0` |
| 32 | `HangingTreeGuardKilled` | +293 | `004045C0` |
| 33 | `GatesRequireClosing` | +118 | `004045C0` |
| 34 | `GatesRequireOpening` | +119 | `004045C0` |
| 35 | `AchievementsWorthyOfSong` | +220 | `004106F0` |
| 36 | `StoryTellerSpecialStories` | +252 | `004106F0` |
| 37 | `StoryTellerToldSpecialStories` | +256 | `004106F0` |
| 38 | `HeroDrunkness` | +120 | `00410BE0` |
| 39 | `VillagerAngryRating` | +128 | `00410620` |
| 40 | `TrophyDealerHeroSpokenToDemonDoors` | +124 | `004045C0` |
| 41 | `StruckDealWithLadyGrey` | +213 | `004045C0` |
| 42 | `HeroExposedLadyGrey` | +214 | `004045C0` |
| 43 | `HeroMarriedLadyGrey` | +215 | `004045C0` |
| 44 | `TimeAdvancePointTriggered` | +264 | `004045C0` |
| 45 | `ScorpionsDestroyed` | +156 | `004045C0` |
| 46 | `GuildWarningOccuring` | +192 | `004045C0` |
| 47 | `SkillTestOccuring` | +193 | `004045C0` |
| 48 | `WillTestOccuring` | +194 | `004045C0` |
| 49 | `ScorpionsDestroyedCutscenePlayed` | +157 | `004045C0` |
| 50 | `SkillTrainingStarted` | +158 | `004045C0` |
| 51 | `WillTrainingStarted` | +159 | `004045C0` |
| 52 | `MovingDummiesNeeded` | +160 | `004045C0` |
| 53 | `MeleeApprenticeNeededForCutscene` | +176 | `004045C0` |
| 54 | `GlobalMeleeGrade` | +180 | `00410BE0` |
| 55 | `GlobalSkillGrade` | +184 | `00410BE0` |
| 56 | `GlobalWillGrade` | +188 | `00410BE0` |
| 57 | `SkillRepeating` | +195 | `004045C0` |
| 58 | `SkillRepeatKnown` | +196 | `004045C0` |
| 59 | `SkillDummyReset` | +197 | `004045C0` |
| 60 | `HighestSkillScore` | +168 | `00410BE0` |
| 61 | `SingingStonesInSync` | +320 | `004045C0` |
| 62 | `SwordInTheStoneComplete` | +199 | `004045C0` |
| 63 | `AmbushTradersAllGuardsDead` | +200 | `004045C0` |
| 64 | `AmbushTradersAllTradersDead` | +201 | `004045C0` |
| 65 | `AmbushTradersKillCount` | +204 | `00410BE0` |
| 66 | `AmbushTradersBanditHireCount` | +208 | `00410BE0` |
| 67 | `AmbushTradersSpyDead` | +212 | `004045C0` |
| 68 | `BountyHuntWithinTimeLimit` | +216 | `004045C0` |
| 69 | `BountyHuntTimeLimitExceeded` | +218 | `004045C0` |
| 70 | `BountyHuntDecapitation` | +217 | `004045C0` |
| 71 | `NPCAttitudesTransferVector` | globals `0x143E928`…`930` | `005815D6` |
| 72 | `BreakSiegeFinished` | +224 | `004045C0` |
| 73 | `WhiteBalverineFinished` | +225 | `004045C0` |
| 74 | `MadBomberFinished` | +226 | `004045C0` |
| 75 | `BanditCampTwinbladeKilled` | +58 | `004045C0` |
| 76 | `PrisonRaceNumber` | +232 | `00410BE0` |
| 77 | `PrisonKeyStolenByHero` | +238 | `004045C0` |
| 78 | `BooksPreviouslyOpened_Vector` | +239…+241 | `00CDCF80` |
| 79 | `MaxChickenKickingScore` | +248 | `00410BE0` |
| 80 | `JackBossBattleHeroGoodAtEnd` | +140 | `004045C0` |
| 81 | `JackBossBattleResult` | +136 (`add esi, 0x88`) | `00410BE0` |

No `_LIKE`. No `_HATE`. No `AttackOver`.

---

## 4. Why `00CDC070` is not the no-save first write

`004B3760` (after `00CDBA10`):

```
arg2 = queue name (not the persist object)
004B27F0(manager+12, name)     // map at manager+28
eax==0 → 004B3807 ret          // skip
eax!=0 → 00404720 / 009BADD0
         call [ecx.vtbl+4]     // persist walk (00CDC070 if ecx is the 0x144)
```

No-save `[0x13B8648]==0` does not load a save into that map.
First `Q_SunnyvaleMaster` name is new. `004B27F0` returns 0.
**PROVEN** skip of the `[vtbl+4]` walk.

Even on a hit, `00CDBA10` already ran.

`e8.tsv` has **0** sites `→ 00CDC070` / `→ 00CDBA10`.
Both are vtbl-only.

---

## 5. `004B4260` vs this object

`004B4260` does **not** call persist itself. It logs, predicates,
`00CB5AD0`s, `004BB720`s, then **one** `004B3CE0`.

WLD `START_INITIAL_QUESTS` first name is `Q_SunnyvaleMaster`
(same string as first `00CD52D0` row). That is why the first
persist object is Sunnyvale, not Gameflow and not Oakvale.

Gameflow is a **second** `004B4260` from `user.ini` and
`004AFA10`-reuses this same `00CDBD20` object. It does not
re-run `00CDBA10` or `00CDC070`. **PROVEN**.

CWorld `0049F180` `004045C0` (`TeleportingEnabled` …) is
**earlier** in the same Init Game suffix. Different object.
**DISPROVEN** as this persist table.

---

## 6. Host vs listing (not authority)

| Host | Listing | Class |
|---|---|---|
| `PersistTable.Sunnyvale` length 38 | `00CDC070` 81 names | **PARTIAL** table. **DISPROVEN** as complete bind |
| “defaults are `00CDBA10` zeros” | +60/+64/+168/+195 non-zero | **DISPROVEN** as all-zero |
| `Note(00CDC070)` on activate | 0 `E8`; `004B3760` miss | **DIVERGE** as a live call |
| `Install(Sunnyvale)` all 0 / false | native +168=10, +195=1, +60/+64=2 | **DIVERGE** |
| `AttackOver` / `00DAADA0` first persist | Sunnyvale object | **DISPROVEN** pairing |
| `SunnyvaleBind` VA `00CDC070` | that function | **PROVEN** VA only |

---

## Classifications (short)

1. **First persist after no-save Leave — Sunnyvale `0x144` /
   `00CDBD20` + `00CDBA10`. PROVEN.** Not `00DAADA0`.

2. **`00CDC070` binds 81 named slots on that object. PROVEN**
   from `listing-00cc0000.txt`. First name
   `PostSavePosition` (`this+4`). Bool `004045C0`, int
   `00410BE0`, plus string / enum / float / two vectors.

3. **`_LIKE` / `_HATE` are `00CDBA10` globals
   (`0x143E938` / `0x143E93C`), not `00CDC070` fields.
   PROVEN.** Persist vector is `NPCAttitudesTransferVector`
   over `0x143E928`…`930` only.

4. **First write is `00CDBA10` (`004B3CE0` `[vtbl+8]`)
   inside first `004B4260`. PROVEN.** `00CDC070` is later
   and skipped on no-save (`004B27F0` miss).

5. **Order: `004B4260` → `004B3CE0` → `00CDBD20` →
   `00CDBA10` → (`00CDC070` only on save hit) →
   `00CDD550` → `00CB7900`. PROVEN.**
