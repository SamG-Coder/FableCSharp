# First-seen `004B2890` after `004B4260`: `QM+112` vs `QM+56`

Investigation only. No production `src/` edits.

Question: after first `004B4260`, `004B2890` walks `QM+112`
(circular persist/boast list) then hero / `QM+56`. First-seen
no-save: is `+112` the empty sentinel? What does `+56` hold?
Any write before this call? Relation to the HeroBoasts factory?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Authority: ExeIndex `listing-00480000.txt` `004B2890` / `004B4590`
(`004B45CA` `+56`, `004B465D` `+112`) / `004B4260` / `004B3CE0` /
`004B05C0` (`004B07B7` load) / `0049F180`;
`listing-00cc0000.txt` `00CE6C40` / `00CE1A30`;
`proofs/quest-manager-plus44/README.md`;
`proofs/factory0-enqueue/README.md`;
`proofs/creature-after-leave/README.md`.

---

## Verdict

**Yes: first-seen `QM+112` is the ctor empty circular sentinel.
PROVEN.**

`004B2890` `cmp [head], head` → `je 004B2989`. No persist load
and no insert into `+112` runs before this call on no-save New
Game. `004B4260` / `004B3CE0` never touch `+112`.

**`QM+56` is the active-quest circular list (16-byte nodes).
PROVEN.** Ctor sentinel, then `004B3CE0` (inside the just-finished
`004B4260`) links one node per `world+172` name — including
HeroBoasts and the two factory-0 stubs.

**There is a write to `+56` before this call: `004B3CE0`.
There is no write to `+112` after ctor. PROVEN.**

**HeroBoasts factory `00CE6C40` does not fill `+112`.** It
allocates the 80-byte object that `004B3CE0` then hangs on
`+56`. First-seen `004B2890` still does **not** walk those
nodes: `00487DC0` has no player Thing yet, so the `+56` tail
`je 004B2AC1`. **PROVEN** skip; the list itself is not empty.

| Slot | Kind | First-seen at `004B2890` | Filled by | Class |
|---|---|---|---|---|
| `QM+112` | 40-byte circular (persist accepted boasts) | empty sentinel | ctor; load `004B07B7` | **PROVEN** empty |
| `QM+108` | 40-byte circular (working accepted boasts) | empty sentinel | ctor; `004B2890` `+112` loop | **PROVEN** empty (loop skipped) |
| `QM+104` | 40-byte circular (`004AF7A0` find) | empty sentinel | ctor | **PROVEN** empty |
| `QM+76/+80/+84` | CString vector (boast-name filter) | all 0 | ctor; later erase sites only in this listing | **PROVEN** empty |
| `QM+56` | 16-byte circular (active quest slots) | **9 nodes** after `004B3CE0` | ctor sentinel then `004B3CE0` | **PROVEN** filled; **PROVEN** not walked first-seen |

Do not collapse `+112` (persist boasts) with `+56` (quest
instances) or with `QM+44` / `world+172` (name catalogs).

---

## Timeline (no-save New Game)

```
004B4590  QuestManager ctor  [0x13B89FC]
  004B45CA  [QM+56] = 0 then 00BFEA0E(16)  [eax]=[eax+4]=eax
  004B45F5  [QM+76/+80/+84]=0
  004B4637  [QM+104] 00BFEA0E(40) sentinel
  004B4649  [QM+108] 00BFEA0E(40) sentinel
  004B465D  [QM+112] 00BFEA0E(40) sentinel   // EMPTY LIST

004A1840  Load Quests
  004A0D90  AddQuest → world+184 / +172 / QM+44
  // no 004B05C0  (NumAcceptedBoasts persist)

0049F180  Init Characters / GUI / Quests
  00449970 / 00487DC0  miss
  00449D90  PLAYER_HERO → CREATURE_HERO
    00489D40  miss  (no 006AC910)            // still no Thing
  0043A380  Init GUI
  0049F23D  ecx=[0x13B89FC]
  0049F247  lea edx, [esi+172]
  0049F24E  call 004B4260                    // WALK world+172
    004BB720 queue
    004B3CE0                                 // WRITE QM+56 (9 nodes)
      HeroBoasts: 00CE6C40 then 00CB7900 / 00CE1A30
  0049F253  ecx=[0x13B89FC]
  0049F259  call 004B2890
    [QM+112].next == head  → 004B2989        // skip boast restore
    00449970 / 00487DC0  still 0             // skip +56
    ret
```

---

## 1. `004B2890` body (`listing-00480000.txt`)

Two independent walks. Same `this` (`[0x13B89FC]`).

```
004B2890  sub esp, 28
          mov esi, ecx
          mov eax, [esi+112]
          mov edi, [eax]
          cmp edi, eax
          je  004B2989              // empty +112
004B28B0: ebx = [edi+8]             // BoastId
          copy CString [edi+20]     // BoastScriptName
          004AF7A0(this, id, name)  // find in QM+104
          if hit:
            [hit+25] = [edi+33]     // BoastFailed
            [hit+26] = [edi+34]     // BoastCompleted
            alloc 40, 004B73A0 payload, insert QM+108
            if !failed && !completed:
              00687540(event 41) via [QM+124]+96
          edi = [edi]; cmp edi, [esi+112]
          jne 004B28AC

004B2989  ecx = [[0x13B86A0]+28]
          00449970 / 00487DC0       // player Thing
          je  004B2AC1              // no Thing → skip +56
          test [Thing+145], 1
          jne 004B2AC1
          eax = [esi+56]; ebx = [eax]
          cmp ebx, eax
          je  004B2AC1              // empty +56
004B29C6: name = [node+8]+48        // quest instance CString
          004B8E40([QM+76], [QM+80], name)
          miss → next node
          hit  → temp 40-list, 004B1960 copy matching QM+108
                  maybe 00687540(event 73) if [Thing+60] bit 28
          ebx = [ebx]; cmp ebx, [esi+56]
```

**`004B2890` does not walk `QM+44` or `world+172`. PROVEN**
(sibling `quest-manager-plus44`).

Only two sites: `0049F259` `call` (Init Quests) and `0049EADC`
`jmp` after `0049EAC0` (`004B4260` on `world+172` again).
`004B4A10` (ini `ActivateQuest`) does **not** call `004B2890`.

---

## 2. `+112` is empty sentinel first-seen

Ctor `004B4590`:

```
004B465D  mov [esi+112], ebx        // 0
          push 40
          call 00BFEA0E
          mov [eax], eax
          mov [eax+4], eax
          mov [esi+112], eax        // dummy: next=prev=self
```

Same 40-byte dummy shape as `+104` / `+108`. `004B8C00` (dtor
`004B4813`, persist load wipe) unlinks real nodes then restores
`[head]=[head+4]=head`.

**Writers of `QM+112` in this listing**

| Site | What | Before first `004B2890`? |
|---|---|---|
| `004B465D` ctor | sentinel | yes — empty |
| `004B07B7` persist **load** (`004B05C0` mode not 1/3) | `004B8C00` wipe, then `NumAcceptedBoasts` loop inserts 40-byte nodes | **no** |
| `004B4813` dtor | free | no |
| `004B2890` itself | read only | — |

`004B05C0` (`[ctx+24]`):

- mode **1 or 3** → **save**: walk `QM+108`, write
  `NumAcceptedBoasts` / `BoastScriptName` / `BoastId` /
  `BoastFailed` / `BoastCompleted`.
- else → **load** `004B07B7`: wipe `+112`, read those keys,
  `00BFEA0E(40)` + `004B73A0` insert on `+112`.

Callers: `004B64B4` / `004B64CF` / `004B655A` (quest-manager
persist). **Not** on Leave / `004A1840` / `0049F180`. **PROVEN**
absence.

`004B4260` ends at `004B3CE0` + queue teardown. No `[this+112]`.
**DISPROVEN** that activate fills `+112`.

Node payload (`004B73A0` at `node+8`; `004B2890` readers):

| Off | Field |
|---:|---|
| `+0/+4` | circular next / prev |
| `+8` | BoastId |
| `+20` | BoastScriptName `CString` |
| `+33` | BoastFailed |
| `+34` | BoastCompleted |

---

## 3. `+56` holds active quest slots (write is `004B3CE0`)

Ctor:

```
004B45CA  mov [esi+56], ebx
          push 16
          call 00BFEA0E
          mov [eax], eax / [eax+4], eax
          mov [esi+56], eax
```

16-byte nodes, not 40. `004B3CE0` (called from `004B4260`
`004B4386` after the name loop) inserts on **both** factory
hit and factory 0:

```
esi = [QM+56]                 // sentinel
00BFEA0E(16)
[node+8]  = quest object      // 52-byte slot (+48 = name)
[node+12] = ref wrapper
link before sentinel
```

Factory hit also `00CB7900` (Main / fiber). Factory 0 is a
named stub, still on the same list (`factory0-enqueue`).

First `004B4260` `world+172` TRUE names (nine):

| # | Name | `00CB5AD0` | `+56` node |
|--:|---|---|---|
| 1 | `Q_SunnyvaleMaster` | `00CDD550` | live + `00CB7900` |
| 2 | `ChapterAndSceneManager` | 0 | stub, no fiber |
| 3 | `PersonalScriptMain` | `00CDE2F0` | live |
| 4 | `PersonalScript_GlobalThings` | `00CE19A0` | live |
| 5 | `NPCDeath` | 0 | stub, no fiber |
| 6 | **`HeroBoasts`** | **`00CE6C40`** | **live + `00CE1A30`** |
| 7 | `V_HeroDolls` | `00E98640` | live |
| 8 | `CS_PlayCutscene` | `00F01760` | live |
| 9 | `Global_WatchForHeroDeath` | `00EE90A0` | live |

So when `004B2890` runs, `[QM+56].next != head`. **PROVEN.**

The `+56` **consumer** in `004B2890` still no-ops first-seen:

1. `00487DC0` is 0. Init Characters already missed; `00489D40`
   does not `006AC910` (`creature-after-leave`). Init GUI does
   not create a player Thing. **PROVEN** skip `004B2AC1`.
2. Even with a Thing, inner `004B8E40` searches `QM+76..+80`.
   Ctor zeros that triple; this listing’s `lea …+76` sites are
   **erase** (`004B0160` / `004B3A20` `004B8770`), not a New
   Game fill. Empty find returns `end` (`004B8FD5`) →
   `je 004B2A21` next node. No `004B1960`, no event 73.

`+56` walk purpose: for each **active** quest whose name is in
`QM+76`, copy matching `QM+108` boast nodes and maybe post
event 73. That is boast-UI / persist replay against live
quests — not activate.

---

## 4. Relation to HeroBoasts factory `00CE6C40`

`listing-00cc0000.txt`:

```
00CE6C40  push 80 / 00BFEA1A
          00CB8110                  // script-object base
          [esi+64]=edi; [esi+68]=ebx
          vtbl 012C3688
          0099E4B0 [esi+72]         // empty CString
          ret esi
```

Main `00CE1A30` (reached from `004B3CE0` `00CB7900`):

```
00BFEA1A(60) + 00CDD450("Main")
00CB7E50 attach watcher
```

**No `QM+112`. No `QM+56` insert inside the factory.** The
list insert is generic `004B3CE0` for every constructed slot.
HeroBoasts is row 6 of nine, not a special case of `004B2890`.

`S_HB` `HasStarted==false` on first pumps (`fiber-yield-first`).
That is the fiber / opcode path, not this persist walk.

`+112` *would* matter on **save load**: `004B05C0` load fills
`+112` from `NumAcceptedBoasts`, then some later `004B2890`
copies into `+108` and matches against `+56` (which would
include the HeroBoasts slot if that quest is active). First
no-save never takes that path. **PROVEN** as structure;
load-game first `004B2890` is **UNREAD** here.

---

## 5. Host

`EngineLifecycle.InitCharactersAndQuests` Notes `004B4260`
then `004B2890` and does not model `+112` / `+56` walks.
Comment at `EventManagerPumpFn` already says first-seen
`004B2890` walks empty `[quest+112]`. That is **MATCH**.
It does not say `+56` was just filled by `004B3CE0` and then
skipped for no Thing — that gap is **LEFTOVER** vs this
listing, not a first-seen behavior miss (native also skips).

---

## Classifications (short)

1. **First-seen `QM+112` empty ctor sentinel — PROVEN.**
   `cmp [eax],[eax]` → `004B2989`. Persist load `004B07B7` is
   not on no-save Init Quests.
2. **`QM+56` = active quest circular list — PROVEN.** 16-byte
   nodes, instance at `+8`, name at instance `+48`.
3. **Write before this call — `+56` yes (`004B3CE0` inside
   `004B4260`); `+112` no. PROVEN.**
4. **HeroBoasts factory is a `+56` payload, not a `+112`
   writer. PROVEN.** `00CE6C40` / `00CE1A30` never `lea …+112`.
5. **First-seen `+56` walk skipped — PROVEN** (`00487DC0` 0).
   List is not empty. Inner boast match would also miss empty
   `QM+76`.
6. **`+112` vs `+44` / `world+172` — different structures.
   PROVEN** (`quest-manager-plus44`).
