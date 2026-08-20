# Who retries `00489D40` after the `0049F180` miss

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD` /
Graphic **4300**. That is later `Q_NewOakValeIntro`, not Leave /
Init Game / first no-save 3D Present.

Do **not** treat `0066FF20` / `00449B20` as the Hero **4299**
retry. That path is `CTCCoopSpirit` and
`COOP_SPIRIT_PLAYER_*`.

Do **not** treat a second `0049F180` as the post-miss caller.
No-save never re-enters Init Characters.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: who retries `00489D40` so `006AC910` creates Hero
**4299**? Site after the Load World `0049F180` miss. Not
Oakvale kid.

Authority: siblings `proofs/hero-4299-create`,
`proofs/hero-00489D40-retry`; also `proofs/hero-retry-site`,
`proofs/004A1840-second-site`, `creature-after-leave`,
`guild-arrival-hsp`, `tng-spawn`, `load-job`;
dump `e8.tsv` / `xrefs.tsv` / `listing-00400000.txt` /
`listing-00440000.txt` / `listing-00480000.txt` /
`listing-00500000.txt` / `listing-00640000.txt` /
`listing-00680000.txt` / `listing-00840000.txt`.

---

## Verdict

**Caller is `CPlayer::InitCharacterAs` `0048A070` at
`0048A0AF`.** That is the **only** `.text` `E8` of
`00489D40`. After the `0049F180` miss, no-save does
**not** call `0049F180` again. The **4299** create is a
**later take of the same `0048A0AF`**, parent
**`00449E2D`** (`00449D90` miss → `"CREATURE_HERO"`).

First take after Leave is Load World **`00416BCA`**
`0049F180(ecx=world, 0)` → `0049F1D7` → `00449E2D` →
`0048A0AF`. `00488B20` miss + `[0x13B8647]==0` →
**`ret 0`**. No `00489FC1`. **PROVEN.**

`00416953` is reached once (`004188E9` game `vtbl+32`).
**0** `E8` of `00416953`. `00416BCA` is therefore **not**
taken again after maps. `004A2C80` `0049F180(1)` is
**inside `004A21F0` FableSav**, not `004A1840`.
**DISPROVEN** as a New Game / post-map retry
(`004A1840-second-site`; sibling `hero-00489D40-retry`
nested it under `004A1840` via the `functions.tsv` bad
merge — that wording is **LEFTOVER**).

The only *other* `E8` of `0048A070` is **`0066FF89`**.
That does retry `00489D40`, with
**`COOP_SPIRIT_PLAYER_*`**. **DISPROVEN** as **4299**.

`"CREATURE_HERO"` xrefs besides `00449E0E`: `0085EE18`
(predicate `005FA740`) and `00D4721E` (`00D44CB0`
CutsceneMaze). **0** `E8` of `00489D40` / `0048A070`.
**DISPROVEN** as this spawn.

First `006AC910` for adult **4299** remains `00489FC1` on
the later `0048A0AF` take, pose `GuildArrivalHSP`.
**PROVEN** identity (`hero-4299-create`). Which non-`E8`
feeder first re-enters `00449E2D` after ContainsMap is
**UNREAD**. Host `SpawnHeroFromPlayerStart` after
`006C2170` **MATCH**es order and is **LEFTOVER** as a
native `0049F180` site.

| Question | Answer | Class |
|---|---|---|
| Who `E8`s `00489D40`? | **`0048A070` @ `0048A0AF`** only | **PROVEN** |
| Site after `0049F180` miss? | **same `0048A0AF`** (later take) | **PROVEN** |
| 4299 parent of that take? | **`00449E2D`** `"CREATURE_HERO"` | **PROVEN** listing |
| Second `0049F180` after maps? | **No.** `00416BCA` once; `004A2C80` save | **DISPROVEN** |
| `00501450` retries create? | **No.** empty `+44` skips `004C8CF0`; no `00449D90` | **DISPROVEN** |
| `0066FF89` is the 4299 retry? | **No.** coop name | **DISPROVEN** |
| First `00489FC1` / `006AC910`? | `CREATURE_HERO` / mesh **4299** / `GuildArrivalHSP` | **PROVEN** identity |
| `CREATURE_HERO_CHILD` / 4300? | Oakvale leftover | **DISPROVEN** |
| Post-map outer of `00449E2D`? | no extra `E8` | **UNREAD** (vtbl / indirect) |

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
004184BD  Init Game
  004188E9  [game].vtbl+32 00416953          // only this site; 0 E8
00416953  Loading world FinalAlbion.wld
  [game+90588] empty → skip 004A3200         // no 004A21F0
  00416ABA  004A1840                         // ends 004A21DF ret 4
    no E8 0049F180                           // PROVEN
  [0x13B8648]==0
  00416BC8  push 0
  00416BCA  call 0049F180                    // ONLY no-save E8
    0049F1D7  00449D90
      00449E0D  "CREATURE_HERO"
      00449E2D  0048A070                     // CALLER
        0048A0AF  00489D40                   // FIRST take
          00488B20 miss + [0x13B8647]==0
          ret 0                             // no 00489FC1
later 00501450
  00449970 / 00487DC0  still 0
    no 00449D90                             // miss does NOT retry
  005014EC  00500540(1,0,0) LookoutPoint
    006C2752  006C2170 Loading objects
      ContainsMap TNG: HOLY_SITE_PLAYER_START
        GuildArrivalHSP / LookoutPointHSP / MAIN_START_POSITION
      no PlayerCreature / no CREATURE_HERO
      no E8 00489D40 / 0048A070 / 00449D90 / 0049F180
  later take of 0048A0AF                     // same insn; same caller
    parent 00449E2D "CREATURE_HERO"          // PROVEN identity
    outer E8 UNREAD (not 00416BCA again; not 004A2C80)
    00489FC1  006AC910                       // FIRST Hero Thing 4299
      ecx = 009AD410("CREATURE_HERO")
      edx = GuildArrivalHSP pose / RHSet
      size 0x208 → 0052AB20 → 006A9DD0
      Graphic 4299 MESH_HERO
```

`CREATURE_HERO_CHILD` / `00DBDE40` / `COOP_SPIRIT_PLAYER_*`
are **not** on this list. **PROVEN.**

---

## 1. Closed `.text` `E8` graph — the caller

`e8.tsv`:

| Dest | Sites |
|---|---|
| `00489D40` | **`0048A0AF`** only |
| `0048A070` | `00449E2D`, `00449B31` |
| `00449D90` | **`0049F1D7`** only |
| `0049F180` | `00416BCA`, `004A2C80` |
| `006AC910` | `00489FC1`, `0089F660` |

No `jmp` of those four create VAs. **PROVEN** as `.text`
`E8`.

```
00489D40 CreateCharacter
  only E8: 0048A0AF          ← THE CALLER SITE (both takes)

0048A070 InitCharacterAs     ← WHO
  [this+52]==0 OR [Thing+145] bit0:
    0048A0AF 00489D40
  E8:
    00449E2D  00449D90       // CREATURE_HERO  ← 4299
    00449B31  00449B20       // coop

00449D90
  009AD410 "PLAYER_HERO" → 0044BA90 miss
  00449E0D "CREATURE_HERO"
  004498C0 then 0048A070
  only E8: 0049F1D7

0049F180 Init Characters
  00449970 / 00487DC0 miss → 00449D90
  E8:
    00416BCA  push 0   after 004A1840 if [0x13B8648]==0
    004A2C80  push 1   inside 004A21F0 FableSav
```

`0048A070` after the create call always `0048A0EA 00487CF0`
(bind `+52` → `+44`). Miss leaves both empty, so a later
`0048A070` **will** take `0048A0AF` again. **PROVEN**
condition. That later `0048A070` for **4299** is still
**`00449E2D`**.

---

## 2. After `0049F180` miss — who does *not* call

`00416953` is **not** re-entered. Game `vtbl+32` site is
**`004188E9`** only (`listing-00400000.txt`). The other
`call [eax+32]` in that file (`0042E0BB`) is UI. **0**
`E8` of `00416953`. **PROVEN.**

`004A2C80` sits after `004A2A01 call 004A1840` inside
`004A21F0` (FableSav `ret 8`). `004A1840` itself ends
`004A21DF ret 4`. No-save `[game+90588]` empty skips
`004A3200`. **DISPROVEN** as Leave / Init Game / first
Lookout.

`00501450` (`listing-00500000.txt`):

```
00501464  mov ecx, [eax+28]
0050146B  call 00449970
00501472  call 00487DC0
0050147B  cmp ebx, ebp
00501481  je  00501495          // empty → skip; NO 00449D90
00501483  test [ebx+145], 1
0050148A  jne 00501495
00501490  call 004C8CF0         // existing Thing only
005014EC  call 00500540         // LookoutPoint
```

Same `00449970` / `00487DC0` pair as Init Characters, but
the miss arm is **not** `00449D90`. **DISPROVEN** as the
retry. **0** `E8` of `00501450` itself.

`006C2710` `"Level loader update"` → `006C2752 006C2170`
→ pop `006C2BA0`. Objects path is `00522720` / `00521AE0`
/ `006C2470 0051E2F0`. **No** create-chain `E8`. **PROVEN**
(`thing-manager-activate`, `load-job`).

`004A5DA5 004498C0` then `004A5DB9 00488AB0` is WorldFrame
player tick. `00488AB0` → `004887C0`. Empty `+44` after
the miss just returns. **No** `E8 00489D40`. **DISPROVEN.**

`00487F00` (0 `E8`; CPlayer vtbl `01231CC4`) → `00487C20`
binds an existing Thing into `+52`. Continue-save leftover
(`00449E60` @ `004A2B05`). **DISPROVEN** as `006AC910`.

`00487470` (0 `E8`; vtbl) writes XYZ to `+232` and
`+244=1` (nearest-site walk). Does **not** call create.
Writer on no-save **UNREAD**.

`0066FF89` → `00449B20` → `00449B31 0048A070`:
`00449730` names `COOP_SPIRIT_PLAYER_*`. **DISPROVEN**
as 4299.

`0089F660` is the other `006AC910` (`0089F4A0` `al==2` via
`0066EBE0`). **Not** `00489D40`. **DISPROVEN.**

`0085EE10` / `00D44CB0` push `"CREATURE_HERO"` and do
**not** `E8` this chain. **DISPROVEN.**

---

## 3. Success args (later `0048A0AF` take)

When `00488B20` hits **or** `[0x13B8647]!=0` on the
`00449D90` string:

| Call | Site | Who | `ecx` | stack |
|---|---|---|---|---|
| `00489D40` | **`0048A0AF`** | **`0048A070`** | `CPlayer` from `004498C0` | CString **`"CREATURE_HERO"`** |
| `006AC910` | **`00489FC1`** | **`00489D40`** | `009AD410` of that name | arg1=`[CPlayer+40]`, arg2=params; `edx`=holy pose |

Holy hit && flag 0 takes `00489E21`: `006A4D00` +
`[esi+96]` vtbl+288. First Lookout pose consumed is
**`GuildArrivalHSP`** `(52.688, 69.597, 36.982)`.
**PROVEN** identity (`hero-4299-create`,
`guild-arrival-hsp`). Native name vs that ScriptName
(`NOVStartHSP` still in `[0x13B866C]` unless `+244` /
rewrite) **PARTIAL**.

Not `"CREATURE_HERO_CHILD"`, not `"COOP_SPIRIT_PLAYER_*"`,
not a `PLAYER_HERO` Graphic (none).

---

## Host leftovers

| Host | Native | Class |
|---|---|---|
| `SpawnHeroFromPlayerStart` Notes `0049F180` / `00489D40` after ContainsMap | those VAs already ran (and missed) at `00416BCA` | **LEFTOVER** site. **MATCH** order / def / HSP |
| `ResolveHeroDefinition` Notes `00449D90` as LevelLoader | bind `E8` is `0049F1D7` only | **LEFTOVER** |
| Prefer `GuildArrivalHSP` by ScriptName | native `00488B20` uses `[0x13B866C]` / `+244` | **MATCH** pose. **DIVERGE** selector unless `+244` / rewrite |

---

## Classification table

| Claim | Status |
|---|---|
| Who retries `00489D40` is `0048A070` at `0048A0AF` | **PROVEN** |
| Site after `0049F180` miss is that same `0048A0AF` | **PROVEN** |
| 4299 parent is `00449E2D` `"CREATURE_HERO"` | **PROVEN** |
| First take after Leave is `00416BCA` and returns 0 | **PROVEN** |
| A second `.text` `E8` of `00489D40` exists | **DISPROVEN** |
| `0049F180` is the post-map retry caller | **DISPROVEN** (not re-entered) |
| `004A2C80` is the no-save / post-map retry | **DISPROVEN** (FableSav `004A21F0`) |
| `00501450` / `006C2170` / `00488AB0` / `00487F00` retry `00489D40` | **DISPROVEN** |
| `0066FF89` is the first 4299 retry | **DISPROVEN** |
| First `00489FC1` / `006AC910` is `CREATURE_HERO` / **4299** at `GuildArrivalHSP` | **PROVEN** identity |
| `CREATURE_HERO_CHILD` is that create | **DISPROVEN** |
| Post-map outer feeder of `00449E2D` | **UNREAD** (no extra `E8`) |

---

## Do not

- Call `0066FF20` / `COOP_SPIRIT_PLAYER_*` from first Lookout spawn.
- Treat `004A2C80` as “after maps” or as inside `004A1840`.
- Treat a second `0049F180` as the 4299 caller.
- Note `0049F180` as a child of `006C2170` / `006AC910`.
- Treat `0089F660` as `00489FC1`.
- Spawn `CREATURE_HERO_CHILD` / 4300 here.
- Invent a second `.text` `E8` of `00489D40`.

Open: first-seen writer of `CPlayer+244` / `[0x13B8647]` /
rewrite of `[0x13B866C]` after Leave; the non-`E8` feeder
that first re-enters **`00449E2D`** after ContainsMap.
The **caller** of that re-entry is still **`0048A070` @
`0048A0AF`**.
