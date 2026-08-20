# Exact `00489D40` retry site before Hero 4299 `006AC910`

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD` /
Graphic **4300**. That is later `Q_NewOakValeIntro`, not Leave /
Init Game / first no-save 3D Present.

Do **not** treat `0066FF20` / `00449B20` as the Hero **4299**
retry. That path is `CTCCoopSpirit` and
`COOP_SPIRIT_PLAYER_*`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: first `00489D40` after Leave returns 0. Who retries
before `006AC910` Hero **4299**? Exact site. Not Oakvale kid.

Authority: siblings `proofs/hero-4299-create`,
`proofs/hero-00489D40-retry`; also `proofs/004A1840-second-site`,
`creature-after-leave`, `guild-arrival-hsp`, `thing-manager-activate`,
`load-job`; dump `e8.tsv` / `listing-00400000.txt` /
`listing-00440000.txt` / `listing-00480000.txt` /
`listing-00500000.txt` / `listing-00640000.txt` /
`listing-00680000.txt` / `listing-006c0000.txt` /
`listing-00880000.txt`.

---

## Verdict

**Exact `00489D40` site is `0048A0AF`.** There is no other
`.text` `E8`. The Hero **4299** retry is a **later take of
that same insn**, parent **`00449E2D`**
(`00449D90` miss → `"CREATURE_HERO"` → `0048A070` empty
`[CPlayer+52]` → `0048A0AF`).

First take after Leave is Load World **`00416BCA`**
`0049F180(ecx=world, 0)` → that chain → `00488B20` miss +
`[0x13B8647]==0` → **`ret 0`**. No `00489FC1`. **PROVEN.**

No-save has **no second `E8` of `0049F180` / `00449D90` /
`0048A070` / `00489D40`**. `004A2C80` `0049F180(1)` is
**inside `004A21F0` FableSav**, not `004A1840`. **DISPROVEN**
as a New Game / post-map retry (sibling `hero-00489D40-retry`
nested it under `004A1840` via the `functions.tsv` bad merge;
`004A1840-second-site` already split that frame).

The only *other* `E8` of `0048A070` is **`0066FF89`**. That
does retry `00489D40`, with **`COOP_SPIRIT_PLAYER_*`**.
**DISPROVEN** as **4299**.

First `006AC910` for adult **4299** remains `00489FC1` on the
**later `0048A0AF` take** of the `"CREATURE_HERO"` string, pose
`GuildArrivalHSP`. **PROVEN** identity (`hero-4299-create`).
Which non-`E8` feeder first re-enters `00449E2D` after
ContainsMap is **UNREAD**. Host `SpawnHeroFromPlayerStart`
after `006C2170` **MATCH**es order and is **LEFTOVER** as a
native `0049F180` site.

| Question | Answer | Class |
|---|---|---|
| Exact `00489D40` insn? | **`0048A0AF`** (only `E8`) | **PROVEN** |
| First take after Leave? | `00416BCA` → `0049F1D7` → `00449E2D` → `0048A0AF` | **PROVEN** |
| That take `006AC910`? | **No.** `ret 0` | **PROVEN** |
| 4299 retry insn? | **same `0048A0AF`** | **PROVEN** |
| 4299 retry parent? | **`00449E2D`** `"CREATURE_HERO"` | **PROVEN** listing |
| Other `.text` `E8` of `00489D40`? | **none** | **PROVEN** |
| `004A2C80` is the post-map retry? | **No.** `004A21F0` save; skipped on no-save | **DISPROVEN** |
| `0066FF89` is the 4299 retry? | **No.** coop name | **DISPROVEN** |
| `00501450` / `006C2170` / `0051E2F0` retry create? | **No** `E8` of this chain | **DISPROVEN** |
| `0089F660` is this `006AC910`? | leftover convert `al==2` | **DISPROVEN** |
| `CREATURE_HERO_CHILD` / 4300? | Oakvale leftover | **DISPROVEN** |
| Post-map outer feeder of `00449E2D`? | no extra `E8` | **UNREAD** (vtbl / indirect) |

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
      00449E2D  0048A070
        0048A0AF  00489D40                   // FIRST take
          00488B20 miss + [0x13B8647]==0
          ret 0                             // no 00489FC1
later 00501450                               // 0 E8 of this fn
  00449970 / 00487DC0  still 0
  005014EC  00500540(1,0,0) LookoutPoint
    006C2752  006C2170 Loading objects
      ContainsMap TNG: HOLY_SITE_PLAYER_START
        GuildArrivalHSP / LookoutPointHSP / MAIN_START_POSITION
      no PlayerCreature / no CREATURE_HERO
      no E8 00489D40 / 0048A070 / 00449D90 / 0049F180
  later take of 0048A0AF                     // same insn
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

## 1. Closed `.text` `E8` graph

`e8.tsv`:

| Dest | Sites |
|---|---|
| `00489D40` | **`0048A0AF`** only |
| `0048A070` | `00449E2D`, `00449B31` |
| `00449D90` | **`0049F1D7`** only |
| `0049F180` | `00416BCA`, `004A2C80` |
| `006AC910` | `00489FC1`, `0089F660` |

No `jmp` / `call [imm]` of those four create VAs in the
listings. **PROVEN** as `.text` `E8`.

```
00489D40 CreateCharacter
  only E8: 0048A0AF          ← EXACT SITE (both takes)

0048A070 InitCharacterAs
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

## 2. First take — `00416BCA` misses

`listing-00400000.txt`:

```
00416ABA  call 004A1840
00416ABF  cmp [0x13B8648], 0
00416AC6  mov ecx, [esi+36]          // world
00416AC9  je  00416BC8
…
00416BC8  push 0
00416BCA  call 0049F180
```

Game `vtbl+32` `00416953` is reached from Init Game
**`004188E9 call [eax+32]`**. **0** `E8` of `00416953`.
No-save `[game+90588]` empty skips `"Loading save"`
`004A3200`. **PROVEN.**

`00489D40` (`listing-00480000.txt`):

```
00489D65  call 00488B20
00489D6E  test al, al
00489D70  mov al, [0x13B8647]
00489D75  jne 00489D86
00489D77  cmp al, bl
00489D79  jne 00489D8E
00489D7B  xor al, al
00489D83  ret 4                       // FIRST-SEEN
…
00489FC1  call 006AC910               // not this take
```

---

## 3. `004A2C80` is not a New Game retry

`004A1840` ends `004A21DF ret 4` / `int3` pad.
`004A21F0` is the next function (FableSav, `ret 8`).
`004A2C80` sits **after** `004A2A01 call 004A1840` in that
save reader:

```
004A2BEE  mov al, [ebp+258]
004A2BF4  test al, al
004A2BFD  jne 004A2CC3                // +258 ≠ 0 → skip
…
004A2C7C  push 1
004A2C7E  mov ecx, ebp                // CWorld
004A2C80  call 0049F180
```

`e8.tsv` dest `004A21F0`: `004A2DC2` / `004A3017` /
`004A32EA` / `004A340D` — save family, not no-save
`00416ABA`. **DISPROVEN** as Leave / Init Game / first
Lookout. Sibling `hero-00489D40-retry` row “inside
`004A1840` if `[world+258]==0`” is **LEFTOVER** wording.

---

## 4. After maps — who does *not* retry

`00501450` (`listing-00500000.txt`): `00449970` /
`00487DC0` (still 0) then `005014EC 00500540`. **No**
`E8` of `00489D40` / `0048A070` / `00449D90` / `0049F180`.
**0** `E8` of `00501450` itself.

`006C2710` `"Level loader update"` → `006C2752 006C2170`
→ pop `006C2BA0`. `006C2170` objects path is `00522720` /
`00521AE0` / `006C2470 0051E2F0`. **No** create-chain
`E8`. **PROVEN** (`thing-manager-activate`, `load-job`).

`004A5DA5 004498C0` then `004A5DB9 00488AB0` is WorldFrame
player tick. `00488AB0` uses live `+44`; **no** `E8
00489D40`. Empty `+44` after the miss just returns.
**DISPROVEN** as the retry.

`00487F00` (0 `E8`; CPlayer vtbl) → `00487C20` →
`00500540` / `0051ED80` **binds an existing Thing** into
`+52`. Continue-save leftover (`load-job`). **DISPROVEN**
as `006AC910`.

`00487470` (0 `E8`; vtbl) writes XYZ to `+232` and
`+244=1` (nearest-site walk). Does **not** call create.
Writer on no-save **UNREAD**.

`0066FF89` → `00449B20` → `00449B31 0048A070`:
`00449730` names `COOP_SPIRIT_PLAYER_*`. **DISPROVEN**
as 4299.

`0089F660` is the other `006AC910` (`0089F4A0` `al==2` via
`0066EBE0`). **Not** `00489D40`. **DISPROVEN.**

`CREATURE_HERO` string xrefs besides `00449E0E`:
`0085EE18` (predicate `005FA740`) and `00D4721E`
(`00D44CB0` CutsceneMaze). **DISPROVEN** as this spawn.

---

## 5. Success args (later `0048A0AF` take)

When `00488B20` hits **or** `[0x13B8647]!=0` on the
`00449D90` string:

| Call | Site | `ecx` | stack |
|---|---|---|---|
| `00489D40` | **`0048A0AF`** | `CPlayer` from `004498C0` | CString **`"CREATURE_HERO"`** |
| `006AC910` | **`00489FC1`** | `009AD410` of that name | arg1=`[CPlayer+40]`, arg2=params; `edx`=holy pose |

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
| Exact `00489D40` site (miss and 4299 retry) is `0048A0AF` | **PROVEN** |
| 4299 parent is `00449E2D` `"CREATURE_HERO"` | **PROVEN** |
| First take after Leave is `00416BCA` and returns 0 | **PROVEN** |
| A second `.text` `E8` of `00489D40` exists | **DISPROVEN** |
| `004A2C80` is the no-save / post-map retry | **DISPROVEN** (FableSav `004A21F0`) |
| `0066FF89` is the first 4299 retry | **DISPROVEN** |
| `00501450` / `006C2170` / `00488AB0` / `00487F00` retry `00489D40` | **DISPROVEN** |
| First `00489FC1` / `006AC910` is `CREATURE_HERO` / **4299** at `GuildArrivalHSP` | **PROVEN** identity |
| `CREATURE_HERO_CHILD` is that create | **DISPROVEN** |
| Post-map outer feeder of `00449E2D` | **UNREAD** (no extra `E8`) |

---

## Do not

- Call `0066FF20` / `COOP_SPIRIT_PLAYER_*` from first Lookout spawn.
- Treat `004A2C80` as “after maps” or as inside `004A1840`.
- Treat `0089F660` as `00489FC1`.
- Spawn `CREATURE_HERO_CHILD` / 4300 here.
- Note `0049F180` as a child of `006C2170` / `006AC910`.
- Invent a second `.text` `E8` of `00489D40`.

Open: first-seen writer of `CPlayer+244` / `[0x13B8647]` /
rewrite of `[0x13B866C]` after Leave; the non-`E8` feeder
that first re-enters **`00449E2D`** after ContainsMap.
The **call site** of that re-entry is still **`0048A0AF`**.
