# First AI task on a creature after Leave

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `Hero.WaitTask` /
`VILL1.WalkTo` / `CAIStateGroup_MinionWander` as the first-seen
task. That path is later leftover `Q_NewOakValeIntro`, not Leave /
Init Game / first no-save Present.

Do **not** collapse **script `CAction*`** (`proofs/entity-task-queue`)
with **CAIBrain / `CAIStateGroup*`**. Same English “task,” different
objects.

Do **not** treat **spawn placement** (`CTCPhysicsNavigator` XYZ,
`GuildArrivalHSP`) as locomotion. Nav *manager* vs creature *task*
are also different (`proofs/navmesh-first`,
`proofs/creature-move-first`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: after Leave Frontend, what is the first **AI task** that
native actually attaches to a **creature**? Is that a brain /
state-group, a village task, a script `CAction`, or a navigator
query?

Sources: Anniversary loose `LookoutPoint.tng` (UID/def/script MATCH
TLC dump); listings `0051FD80` / `005272E0` / `00831F80` /
`008315C0` / `00833A70` / `008338D0` / `00833010` / `0088C160` /
`00714370` / `00666310`; vtbl `0127293C`; RTTI `CAIBrain` /
`CBrainDef` / `CAIStateGroupBase` / `CTCPhysicsNavigator` /
`CTCCreatureNavigation`; `e8.tsv` `00A44740`;
`proofs/npc-first-create`, `entity-task-queue`, `navmesh-first`,
`creature-move-first`, `fiber-first`, `tng-first-def`,
`thing-manager-activate`.

---

## Verdict

**First AI task after Leave is a `CAIBrain` fibre on the first
TNG `AICreature`, attached at Load Single Thing Initial Activate.**

Creature (already `npc-first-create`):

| Field | Value |
|---|---|
| Def | `CREATURE_BS_VILLAGER_MALE` |
| Script | `FH_Villager` / `ScriptData="0"` |
| UID | `18446741874686300555` |
| Section | `Q_FireHeart` (Lookout, after Bridge + Gameflow + NULL) |
| Factory | `005272E0` size `0x1D8` → ctor `00831F80` vtbl `0127293C` |

On that Thing, `0051FD80` (non-player `[thing+24]` not 2/3):

1. **`vtbl+16` `008315C0`** — persist TNG AI keys + dest/gait
   seed (`00662930`, def `+80`, `or [+146],2`).
2. **`vtbl+32` `00833A70`** — `"Load Single Thing: Initial
   Activate"` → `00666310` → **`008338D0`** → **`00833010`** →
   **`0088C160`**.

`0088C160` is the **task object**: alloc `0xA8`, vtbl
`012780C4`, **`00A44740`(flag 0, stack `0x7D00`, 0.1f)**. That
is a **CAIBrain fibre**, not `CActionMoveTo` and not
`CVillageTask*`.

TNG `OverridingBrainName NULL` writes the empty CString
(`0x122D70E`) at Thing `+400`. Brain identity is therefore
**`CCreatureDef+232`**, looked up with `0079BD80`. The concrete
`CBrainDef` / first `CAIStateGroup_*` name on this villager is
**UNREAD**.

| Claim | Status |
|---|---|
| Frontend / Leave itself starts an AI task | **DISPROVEN** |
| First creature to receive one is `FH_Villager` `CREATURE_BS_VILLAGER_MALE` | **PROVEN** create; attach **PARTIAL** (needs `+232` and `00666310`/`006A4D60` success) |
| That attach is `00833A70` / `0088C160` `CAIBrain` fibre | **PROVEN** code; first-seen occupancy **PARTIAL** |
| First task is script `WalkTo` / `CAction*` / `EntityTaskQueue` | **DISPROVEN** |
| First task is `CVillageTask*` / `CAIStateGroup_VillageTask*` | **DISPROVEN** as this site (different RTTI; later village) |
| First task is `BRAIN_STAND_AROUND_LIKE_A_MORON` | **PARTIAL** / likely debug (`[0x13B86EA]`) |
| `BRAIN_NULL` is the assigned brain | **DISPROVEN** (name compare on def `vtbl+40`) |
| First *nav on this creature* is `CTCPhysicsNavigator` pose | **PROVEN** TNG block; tick **UNREAD** |
| First *nav* is `CTCCreatureNavigation` / A\* query / `WalkTo` mesh | **DISPROVEN** attach / **UNREAD** query / leftover script |
| Hero `006AC910` is this first AI task | **DISPROVEN** (later; PlayerCreature; no `0088C160` on that create) |

---

## Timeline (no-save New Game)

```
0042EC7C retail
  0042DF9E 2D UI                         // no Thing, no CAIBrain
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  Init World 004A6E30
    00A15670  CNavigatorManager [world+72]   // world nav, not a creature task
    vtbl+4 "Navigator A Star" / "Navigator flyer"
    00CB5D80 bind Q_FireHeart / S_QFHT       // BIND only; no 00D35090
  00416953 Load world
    004FDBC0 proximity TNG parse             // no 0051FD80
    004B4260 START_INITIAL_QUESTS            // not Q_FireHeart
004189C2 dummy pumps  index 0
later 00501450 Lookout 00500540(1,0,0)
  006C2170
    pass 1 topology 004FF080 / 008224E0
      [rec+12]==0 skip CNavQuadTree
    pass 2 objects
      BowerstoneBridge  88  0 AICreature
      LookoutPoint
        Gameflow / NULL                     // no AICreature
        Q_FireHeart
          0051FD80 CREATURE_BS_VILLAGER_MALE FH_Villager "0"
            005272E0 / 00831F80             // FIRST CThingAICreature
            StartCTCPhysicsNavigator        // pose persist
            vtbl+16  008315C0               // persist AI keys + dest seed
              OverridingBrainName "NULL" → +400 empty
            vtbl+32  00833A70               // FIRST AI TASK SITE
              00666310
              008338D0
                copy def +144… into Thing +440…
                00830DD0 STANDARD_FLY?
                if [0x13B86EA]  +400 = "BRAIN_STAND_AROUND_LIKE_A_MORON"
                else if +400 nonempty  006D3E80 + 00833010
                else if def+232        0079BD80 + 00833010
                  00833010 alloc 0xA8  0088C160
                    00A44740 stack 0x7D00   // CAIBrain fibre
                    0088BF30 state-group table
              def vtbl+40("BRAIN_NULL") compare
        later AICreatures (female FH, Wasp, beggar, traders)
      GuildExterior 6 AICreature            // later
    006AC910 CREATURE_HERO                  // after maps; not this brain
```

`00CBFB7D` / `.WalkTo` / `CActionMoveTo` / Oakvale father are
**not** on this list. **PROVEN.**

---

## 1. What “AI task” is (native)

Three families. Do not merge.

| Family | Native | First-seen after Leave |
|---|---|---|
| **CAIBrain + `CAIStateGroup_*`** | fibre `0088C160` / `00A44740`; groups from brain def table `0x13BB008` | **this proof** — attach site on first `AICreature` |
| Scripted `CAction*` / Thing `vtbl+16/+20/+72/+104` | `CTCScriptedControl` | **none** (`entity-task-queue`) |
| Village work | `CVillageTask*` / `CAIStateGroup_VillageTask*` | **not** this TNG block (no village member work token; `VillageUID 0`) |

`CAIBrain` RTTI `0x013861F4`. `CBrainDef` `0x013775D0`.
`CAIStateGroupBase` `0x013893E4` (xrefs `008FAA80` family).
`CVillageTaskBase` `0x0137E5E4` is a **different** tree.

`fiber-first` already listed `0088C173` as a `00A44740` site
with order **PARTIAL** vs `00507C30`. Global TNG is parse-only
(`004FDBC0`), so that fibre is **not** during WLD load. First
legal `0088C160` is this Lookout `0051FD80` activate.
**PROVEN** vs `00507C30`; occupancy still **PARTIAL**.

---

## 2. First creature (not a task yet)

`npc-first-create` / Anniversary `LookoutPoint.tng` line 7172:

```
XXXSectionStart Q_FireHeart;
NewThing AICreature;
DefinitionType "CREATURE_BS_VILLAGER_MALE";
ScriptName FH_Villager;
ScriptData "0";
StartCTCPhysicsNavigator;
  PositionX 124.357422; PositionY 81.08252; PositionZ 30.099813;
EndCTCPhysicsNavigator;
OverridingBrainName NULL;
WanderWithInformation FALSE;
…
```

Bridge has **0** `AICreature`. Gameflow / NULL have **0**.
Host dump `pos=-` is `CTCPhysicsStandard`-only; native still
constructs. Mesh **5149**.

`00522A20` maps kind `"AICreature"` → family `"CREATURE"` →
factory **`005272E0`**. **PROVEN.**

---

## 3. Load Single Thing order on that NPC

`0051FD80` after construct (`listing-00500000.txt` `00520182`):

```
[thing+24] == 2 or 3  → vtbl+64          // PlayerCreature path
else                  → vtbl+16          // AI: 008315C0
"Load Single Thing: Initial Activate"
[esp+19]              → vtbl+36 then +40
else                  → vtbl+32          // AI: 00833A70
  al==0               → 004C9B80 fail
```

TNG `Player 0` is not 2/3. First NPC therefore **does**
`008315C0` then `00833A70`. **PROVEN** control flow.

`0051E2F0` Activate Things later walks the same vector
(pass 1 `vtbl+32` if `+148&7==0`). Whether pass 1 is a no-op
after `0051FD80` already called `+32` is **PARTIAL** (same
leftover as `thing-manager-activate`).

---

## 4. `vtbl+16` `008315C0` — persist + dest seed, not WalkTo

Starts like player dest (`creature-move-first`):

```
00662930
fld [def+80] → Thing +180/+176
or [+146], 2
alloc 40 → 00834D30 at +352
```

Then walks TNG keys (`HomeBuildingUID`, `WorkBuildingUID`,
`OverridingBrainName`, `WanderWithInformation`, …).

`OverridingBrainName` (`00831A2F`):

| TNG value | `+400` |
|---|---|
| missing / empty / `"NULL"` | `0099EFE0(0x122D70E)` empty |
| other | copy into `+400` |

This file writes `NULL`. **PROVEN** empty override.

This is **not** interpreter WalkTo (`vtbl+20` `004C72B0` stub).
**DISPROVEN** as a script task.

---

## 5. `vtbl+32` `00833A70` — the task attach

```
00666310                         // creature post-create (DEFAULT / door flags)
  fail → al=0, skip brain
008338D0                         // brain seed
and [+350], ~1
006A4D60                         // appearance/ready
  fail → skip
if def+232 > 0:
  004C7990 / 0079BD80
  def.vtbl+40("BRAIN_NULL")
  005B3440 compare
  00835560 on +460
else:
  00835560 on +460
al=1
```

`00666310` returns `al=1` at `0066667C` after `006A4D60`.
Whether first villager takes that path (not the `00666693`
fail) is **PARTIAL**. `00833A70` then calls `006A4D60` again.

### `008338D0`

```
copy [def+144,148,152,160] → Thing +440…+452
00830DD0  "STANDARD_FLY" optional
if [0x13B86EA] != 0:
  0099EFE0("BRAIN_STAND_AROUND_LIKE_A_MORON") → +400
if +400 has a heap string (+4 != 0):
  006D3E80 lookup + 00833010
else if def+232:
  0079BD80 lookup + 00833010
```

`[0x13B86EA]` occupancy **UNREAD**. Treating
`BRAIN_STAND_AROUND_LIKE_A_MORON` as the retail New Game brain
is **PARTIAL** (gated; name is a debug string). Empty `+400`
plus `def+232` is the retail shape.

`BRAIN_NULL` is **not** stored into `+400`. It is pushed for
`CBrainDef vtbl+40` after the fibre exists. **DISPROVEN** as
the assigned name.

### `00833010` / `0088C160`

```
if [Thing+56] & 0x80000:  component 0xD3 walk 008018B0
alloc 0xA8
0088C160(thing, …, CBrainDef*)
  00A44740(0, 0x7D00, 0.1f)
  vtbl 012780C4
  +144 = CBrainDef*
  0088BF30  fill +40/+44 group list from 0x13BB008 name table
store brain at Thing +424 via 00834E40
```

**PROVEN** as the AI task object. First `CAIStateGroup_*`
constructed from that table: **UNREAD** (needs the villager
`CBrainDef` list). Do not invent `CAIStateGroup_Wander` /
`StandStill` / `MinionWander`.

`0088BF30` then `0088B870` picks a live group (`008FCF10`) and
`[group].vtbl+8` (`008FCF50`) to run it. First tick of that
vtbl after Leave is **UNREAD** (would be `00A44880` / brain
`vtbl+4`, after pumps).

---

## 6. Navigation on that same creature (not the task)

World:

| Event | Class |
|---|---|
| `00A15670` Init Navigation Manager | **PROVEN** after Leave, **before** any creature |
| Lookout topology `004FF080` / `008224E0` | **PROVEN**; not `CNavQuadTree` |
| First-seen quadtree insert | **DISPROVEN** (`+12=0`) |
| Live A\* query | **UNREAD** |

Creature:

| Object | First-seen | Class |
|---|---|---|
| `StartCTCPhysicsNavigator` on `FH_Villager` | TNG persist; factory string at `00714370` | **PROVEN** name + block; body **PARTIAL** |
| Pose `(124.357, 81.083, 30.100)` | TNG | **PROVEN** file; world graphic **UNREAD** |
| `CTCCreatureNavigation` / `006CD540` | not on this create (`creature-move-first`) | **DISPROVEN** as first attach |
| `vtbl+20` WalkTo mesh | `004C72B0` stub; no `00CBFB7D` | **DISPROVEN** |
| `NAVIGATION_SEED` on Bridge | other Thing kind | **DISPROVEN** as this creature |

`00714370` looks up `"CTCPhysicsNavigator"` via parent
`vtbl+88`, stores `[esi+52]`, copies `edi+12/16/20`. That is
**navigator component on the Thing**, not `CAIBrain` and not
A\*.

Host `EntityTaskQueue` / `TickMove` interpolating this NPC is
**LEFTOVER** / **DIVERGE**. Native dest seed is `008315C0` /
`00662930`; mesh step is still the WalkTo stub.

---

## 7. Host vs native

| Host | Native after msg 15 | Class |
|---|---|---|
| `LoadSingleThing` every Lookout `AICreature` | `0051FD80` `005272E0` | **MATCH** order |
| Note `"Initial Activate vtbl+32"` | `00833A70` | **MATCH** site; host does **not** `0088C160` |
| `EntityTaskQueue` empty | no `CAction*` | **EQUIVALENT** empty script slot |
| No `CAIBrain` type | fibre `012780C4` | **DIVERGE** / missing |
| Dump `pos=-` (`CTCPhysicsStandard`) | `CTCPhysicsNavigator` XYZ present | **DIVERGE** field pick |
| `StartNewGame` father WalkTo | not Leave | **DIVERGE** |
| `Init Navigation Manager` note | `00A15670` | **PARTIAL** (name; no 48-byte object) |

---

## Not these

| Candidate | Why not first AI task |
|---|---|
| Frontend | no creatures |
| `TRACK_NODE_BASIC` `GuardTrack` | first `0051FD80`, not `CThingAICreature` |
| Hero `006AC910` | later; PlayerCreature; physics-controlled |
| `LookoutPointBeggar` | 7th Lookout `AICreature` (`V_BeggarAndChild`) |
| `00D35090` `00CB8230("FH_Villager")` | needs constructed `Q_FireHeart` / `S_QFHT` |
| `Q_SunnyvaleMaster` fibre | quest watcher, not a creature brain |
| World-map fibre `006C26B0` | first *any* fibre; not AI |
| `CActionPlayAnimation` / `WaitTask` | leftover Oakvale |
| `CVillageTaskPatrol` / Torch / Horn | different RTTI; not this TNG |
| `BRAIN_CHICKEN` / `BRAIN_WIFE` / `BRAIN_GOOD_VILLAGER_BASE` | other defs / later scripts |
| `FollowNavRoute` / `SneakTo` | leftover opcodes |

---

## UNREAD / PARTIAL

- Live `00666310` / `006A4D60` success on first villager
  (`004C9B80` miss would skip the brain).
- `[0x13B86EA]` (moron-brain gate).
- `CCreatureDef+232` / `CBrainDef` instance name for
  `CREATURE_BS_VILLAGER_MALE` (game.bin sub-def not parsed here).
- First `CAIStateGroup_*` class constructed in `0088BF30`.
- First brain `vtbl+4` / `00A44880` tick that actually runs a
  group.
- Whether `0051E2F0` pass 1 re-enters `00833A70`.
- `CTCPhysicsNavigator` tick vs world XYZ after persist.
- TLC WAD vs Anniversary TNG *bytes* (UID/def/script already
  MATCH).

---

## Classifications (short)

1. **Native AI task = `CAIBrain` fibre `0088C160` +
   `CAIStateGroup_*`, stored at Thing `+424`.** Not
   `System.Threading.Tasks`, not `EntityTaskQueue`. **PROVEN**
   types/sites; first group name **UNREAD**.
2. **First creature after Leave that can take that slot:
   Lookout `Q_FireHeart` `FH_Villager` `CREATURE_BS_VILLAGER_MALE`.**
   **PROVEN** create.
3. **Attach site: `0051FD80` Initial Activate `vtbl+32`
   `00833A70` → `008338D0` → `00833010`.** TNG override is
   empty; def `+232` is the name source. **PROVEN** flow;
   occupancy **PARTIAL**.
4. **Navigation on that creature is `CTCPhysicsNavigator` pose
   + `vtbl+16` dest seed. Not `CTCCreatureNavigation`, not A\*,
   not WalkTo mesh.** **PROVEN** / **DISPROVEN** as labelled.
5. **Script / village / Oakvale brains are leftover vs Leave.**

Do not start New Game AI at `Hero.WaitTask FOO`. Do not call
`Init Navigation Manager` the first creature task.
)
