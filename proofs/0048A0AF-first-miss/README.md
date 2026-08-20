# First miss at `0048A0AF`, then first later take (4299)

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD` /
Graphic **4300**. That is later `Q_NewOakValeIntro`, not Leave /
Init Game / first no-save 3D Present.

Do **not** treat `0066FF20` / `00449B20` as the Hero **4299**
retry. That path is `CTCCoopSpirit` / `COOP_SPIRIT_PLAYER_*`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: `0048A070` at `0048A0AF` is the only `E8` of
`00489D40`. After `0049F180` `PLAYER_HERO` miss, no
`006AC910`. When is the first later take (`CREATURE_HERO`
**4299**)?

Authority: Fable.exe via ExeIndex `e8.tsv` /
`listing-00400000.txt` / `listing-00440000.txt` /
`listing-00480000.txt` / `listing-00500000.txt` /
`listing-00680000.txt`; sibling `proofs/hero-retry-caller`
(also `hero-retry-site`, `hero-00489D40-retry`,
`hero-4299-create`, `00501450-no-00449D90`,
`00416BCA-push0`, `creature-after-leave`).

---

## Verdict

**First `0048A0AF` after Leave is the Load World miss.
No `006AC910`. First `CREATURE_HERO` / mesh 4299 is a
later take of that same insn, after Lookout ContainsMap
has built `GuildArrivalHSP`. Outer feeder of that take
is not a second `E8`. WHEN of the outer is UNREAD.**

`0048A0AF` is the **only** `.text` `E8` of `00489D40`.
Parent is always `CPlayer::InitCharacterAs` `0048A070`.
4299 parent of `0048A070` is **`00449E2D`**
(`00449D90` `PLAYER_HERO` miss → `"CREATURE_HERO"`).
**PROVEN** listing.

First take: no-save `00416BCA` `0049F180(ecx=world, 0)`
→ `0049F1D7` → `00449E2D` → `0048A0AF`. `00488B20`
miss + `[0x13B8647]==0` → **`ret 4` / `al=0`**. Does
**not** reach `00489FC1`. **PROVEN.**

No-save never `E8`s `0049F180` again (`004A2C80` is
FableSav). `00501450` / `006C2170` / `0051FD80` have
**0** `E8` of this create chain. The later 4299 take
is therefore **not** on those stacks via `E8`.
**DISPROVEN** as those sites. Outer of `00449E2D`
after maps: **UNREAD** (vtbl / indirect; no extra
`.text` `E8`).

First success identity stays `CREATURE_HERO` / Graphic
**4299** / pose `GuildArrivalHSP`. **PROVEN**
(`hero-4299-create`). Host `SpawnHeroFromPlayerStart`
after `006C2170` **MATCH**es that order and is
**LEFTOVER** as a native `0049F180` site.

| Question | Answer | Class |
|---|---|---|
| Only `E8` of `00489D40`? | **`0048A070` @ `0048A0AF`** | **PROVEN** |
| First take after Leave? | `00416BCA` → `0049F1D7` → `00449E2D` → `0048A0AF` | **PROVEN** |
| That take `006AC910`? | **No.** holy miss + `[0x13B8647]==0` → `ret 0` | **PROVEN** |
| First later 4299 take insn? | **same `0048A0AF`** | **PROVEN** |
| 4299 name / mesh / pose? | `"CREATURE_HERO"` / **4299** / `GuildArrivalHSP` | **PROVEN** identity |
| When vs maps? | **After** Lookout `006C2170` ContainsMap (HSP exists). **Not** on `00501450` / `006C2170` `E8` | **PROVEN** order / **DISPROVEN** those `E8`s |
| Second `0049F180` `E8` after maps? | **No.** `00416BCA` once; `004A2C80` save | **DISPROVEN** |
| `0066FF89` / kid 4300? | coop / Oakvale leftover | **DISPROVEN** |
| Outer feeder of later `00449E2D`? | no extra `E8` | **UNREAD** |
| Exact WorldFrame of first `00489FC1`? | — | **UNREAD** |

**Overall: PARTIAL.** Identity and first-miss are
**PROVEN**. Clock of the later take is **after maps,
same insn**; the non-`E8` outer is **UNREAD**.

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
004184BD  Init Game
  004188E9  [game].vtbl+32 00416953          // 0 E8 of 00416953
00416953  Loading world FinalAlbion.wld
  [game+90588] empty → skip 004A3200         // no 004A21F0
  00416ABA  004A1840                         // ends 004A21DF; no E8 0049F180
  [0x13B8648]==0
  00416BC8  push 0
  00416BCA  call 0049F180                    // ONLY no-save E8
    0049F1D7  00449D90
      009AD410 "PLAYER_HERO" → 0044BA90 miss
      00449E0D  "CREATURE_HERO"
      00449E2D  0048A070
        0048A0AF  00489D40                   // FIRST take
          00488B20 miss + [0x13B8647]==0
          ret 0                             // no 00489FC1 / no 006AC910
    0048A0EA  00487CF0                       // +52 still empty
004189C2  dummy pumps                        // HeroSpawned=false
later 00501450                               // 0 E8 of this fn
  00449970 / 00487DC0 still 0
    je 00501495                             // no 00449D90
  005014EC  00500540(1,0,0) LookoutPoint
    006C2752  006C2170 Loading objects
      ContainsMap TNG: HOLY_SITE_PLAYER_START
        GuildArrivalHSP / LookoutPointHSP / MAIN_START_POSITION
      no PlayerCreature / no CREATURE_HERO
      no E8 00489D40 / 0048A070 / 00449D90 / 0049F180
  later take of 0048A0AF                     // FIRST 4299 take
    parent 00449E2D "CREATURE_HERO"          // PROVEN listing
    outer E8 UNREAD (not 00416BCA; not 004A2C80)
    00488B20 hit OR [0x13B8647]!=0
    00489FC1  006AC910                       // FIRST Hero Thing
      ecx = 009AD410("CREATURE_HERO")
      edx = GuildArrivalHSP pose / RHSet
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

No `jmp` of those create VAs. **PROVEN** as `.text` `E8`.

```
00489D40 CreateCharacter
  only E8: 0048A0AF          ← BOTH takes (miss and 4299)

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

First miss leaves `[CPlayer+52]` empty, so a later
`0048A070` **will** take `0048A0AF` again. **PROVEN**
condition. That later `0048A070` for **4299** is still
**`00449E2D`**.

---

## 2. First take — `0048A0AF` misses (`listing-00480000.txt`)

`0048A070`:

```
0048A087  lea edi, [esi+52]
0048A08F  call 00A01B50
0048A094  test eax, eax
0048A096  je  0048A0A8          // empty +52 → create
0048A09F  test [eax+145], 1
0048A0A6  je  0048A0B4
0048A0A8  push [esp+12]         // CString*
0048A0AD  mov ecx, esi
0048A0AF  call 00489D40         // THE SITE
```

`00489D40`:

```
00489D65  call 00488B20
00489D6E  test al, al
00489D70  mov al, [0x13B8647]
00489D75  jne 00489D86
00489D77  cmp al, bl
00489D79  jne 00489D8E
00489D7B  xor al, al
00489D83  ret 4                 // FIRST-SEEN
…
00489FC1  call 006AC910         // not this take
```

Load World: holy list empty (region TNG not constructed),
flag 0 → early-out. **PROVEN.**

---

## 3. When the later take can succeed

`00489FC1` runs only if `00488B20` returns 1 **or**
`[0x13B8647]!=0`, and `[CPlayer+52]` is still empty,
and the `0048A070` arg is `"CREATURE_HERO"`.

Lookout ContainsMap (`006C2170` → `0051FD80`) constructs
`HOLY_SITE_PLAYER_START` **`GuildArrivalHSP`**. That is
**not** a `PlayerCreature` NewThing and **not**
`006AC910`. **PROVEN** (`first-0051FD80-file`,
`creature-after-leave`).

So the **earliest** the later take can hit create is
**after that Lookout object pass**. Dummy pumps before
`00501450` still have `HeroSpawned=false`. **PROVEN**
order.

It is **not** the `00501450` miss arm (`je 00501495`,
no `00449D90`; `00501450-no-00449D90`). **DISPROVEN.**

Native `00488B20` still matches `[0x13B866C]` /
`CPlayer+244`, not ScriptName `GuildArrivalHSP` by
default (`NOVStartHSP` leftover). Which of `+244` /
rewrite of `+866C` / `[0x13B8647]` first unblocks
no-save is **UNREAD**. Host prefers `GuildArrivalHSP`
by name: **MATCH** pose, **DIVERGE** selector unless
`+244` / rewrite.

---

## 4. Not these (later 4299 take)

| Candidate | Why not first 4299 `0048A0AF` |
|---|---|
| First `00416BCA` | this note’s miss; `ret 0` |
| `004A2C80` | save `004A21F0`; skipped on no-save |
| `00501450` / `006C2170` / `0051E2F0` / `0051FD80` | 0 `E8` of create chain |
| `00488AB0` / `004887C0` | live `+44` tick; empty after miss just returns |
| `00487F00` | binds an **existing** Thing; continue-save leftover |
| `0066FF89` | `COOP_SPIRIT_PLAYER_*` |
| `0089F660` | other `006AC910`; not `00489D40` |
| `00DBDE40` / kid 4300 | Oakvale leftover |

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
| `0048A0AF` is the only `E8` of `00489D40` | **PROVEN** |
| First take after Leave is `00416BCA` and returns 0 | **PROVEN** |
| First `006AC910` is not on that take | **PROVEN** |
| First later 4299 take is the same `0048A0AF` / `00449E2D` `"CREATURE_HERO"` | **PROVEN** listing |
| That take is after Lookout ContainsMap, not on `00501450` `E8` | **PROVEN** order |
| A second `.text` `E8` of `00489D40` / `0049F180` after maps | **DISPROVEN** |
| `004A2C80` / `0066FF89` / kid 4300 is that take | **DISPROVEN** |
| Outer non-`E8` feeder of later `00449E2D` | **UNREAD** |
| First-seen writer of `CPlayer+244` / `[0x13B8647]` / rewrite of `[0x13B866C]` | **UNREAD** |

---

## Do not

- Call `0066FF20` / `COOP_SPIRIT_PLAYER_*` from first Lookout spawn.
- Treat `004A2C80` as “after maps”.
- Treat a second `0049F180` as the 4299 caller.
- Note `0049F180` as a child of `006C2170` / `006AC910`.
- Treat `0089F660` as `00489FC1`.
- Spawn `CREATURE_HERO_CHILD` / 4300 here.
- Invent a second `.text` `E8` of `00489D40`.

Open: the non-`E8` feeder that first re-enters
**`00449E2D`** after ContainsMap. The **call site** of
that re-entry is still **`0048A0AF`**.
