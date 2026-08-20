# Who retries `00489D40` after the Load World holy-site miss

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD` /
Graphic **4300**. That is later `Q_NewOakValeIntro`, not Leave /
Init Game / first no-save 3D Present.

Do **not** treat `0066FF20` / `00449B20` as the Hero **4299**
retry. That path is `CTCCoopSpirit` and
`COOP_SPIRIT_PLAYER_*`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: first `00489D40` after Leave returns 0 (holy-site
miss, `[0x13B8647]==0`). Later `00489FC1` → `006AC910` creates
Hero **4299**. Who retries `00489D40`? Site, condition, first
success args. Not `CREATURE_HERO_CHILD`.

Authority: dump `00489D40` / `00489FC1` / `006AC910`
(`listing-00480000.txt`, `listing-00680000.txt`,
`listing-00440000.txt`, `listing-00640000.txt`, `e8.tsv`);
siblings `proofs/hero-4299-create`, `creature-after-leave`,
`hero-stats-first`, `script-setnewstart`, `tng-spawn`.

---

## Verdict

**There is no second `.text` `E8` of `00489D40`.**

The only call is `0048A0AF` inside `CPlayer::InitCharacterAs`
`0048A070`. First-seen after Leave is Load World
`0049F180` → `00449D90` (`PLAYER_HERO` miss →
`"CREATURE_HERO"`) → `0048A0AF`. `00488B20` misses
(`NOVStartHSP` / empty `[0x13B866C]` is not a live Thing)
and `[0x13B8647]==0` → **`ret 0`**. No `00489FC1`.

The **only later `E8` parent of `0048A070`** is
`0066FF89` (`CTCCoopSpirit` `0066FF20` → `00449B20`).
That retries `00489D40` with **`COOP_SPIRIT_PLAYER_*`**,
not `CREATURE_HERO`. **DISPROVEN** as Hero **4299**.

First **4299** is still `00489FC1` → `006AC910` on a
**later take of the same `00449D90` / `"CREATURE_HERO"`
string**. That take has **no extra `E8` of `0049F180` /
`00449D90` / `00489D40`**. Live site after Lookout
ContainsMap is **UNREAD**. Host folds create into
`SpawnHeroFromPlayerStart` after `006C2170`. **MATCH**
order. Noting `0049F180` at that host site is **LEFTOVER**.

| Question | Answer | Class |
|---|---|---|
| First `00489D40` after Leave? | Load World `0049F180` → `00449E2D` → `0048A0AF` | **PROVEN** |
| That call hits `00489FC1`? | **No.** holy miss + `[0x13B8647]==0` | **PROVEN** |
| Other `.text` `E8` of `00489D40`? | **none** — only `0048A0AF` | **PROVEN** |
| `004A2C80` `0049F180(1)` is the post-map retry? | **No.** Same `004A1840`, **before** `00416BCA` / maps | **PROVEN** insn; take **PARTIAL** (`[world+258]`) |
| `0066FF20` retries `00489D40`? | **Yes** as `E8`. **No** as Hero 4299 | **PROVEN** / **DISPROVEN** |
| First `00489FC1` / `006AC910` identity? | `CREATURE_HERO` / mesh **4299** / `GuildArrivalHSP` | **PROVEN** |
| That is `CREATURE_HERO_CHILD` / `0089F660`? | **No** | **DISPROVEN** |

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
00416953  Loading world FinalAlbion.wld
  00416ABA  004A1840
    00507C30  WLD / 004FDBC0 global .tng parse
    [world+258]==0 → 004A2C80 0049F180(1)   // PARTIAL take
      00449D90 PLAYER_HERO miss → CREATURE_HERO
      0048A0AF  00489D40
        00488B20  miss                    // no live HSP
        [0x13B8647]==0 → ret 0            // no 00489FC1
  [0x13B8648]==0
  00416BCA  0049F180(0)                   // PROVEN first-seen site
      same 00449D90 / 00489D40            // still miss
004189C2  dummy pumps
later 00501450  00500540(1,0,0) LookoutPoint
  006C2170 Loading objects
    ContainsMap TNG: HOLY_SITE_PLAYER_START
      GuildArrivalHSP / LookoutPointHSP / MAIN_START_POSITION
    no PlayerCreature / no CREATURE_HERO
  later take of 0048A0AF (site UNREAD; not 0066FF20)
    00488B20 hit OR [0x13B8647]!=0
    00489FC1  006AC910                    // FIRST Hero Thing
      ecx = 009AD410("CREATURE_HERO")
      edx = GuildArrivalHSP pose / RHSet
      size 0x208 → 0052AB20 → 006A9DD0
      Graphic 4299 MESH_HERO
```

`CREATURE_HERO_CHILD` / `00DBDE40` / `COOP_SPIRIT_PLAYER_*`
are **not** on this list. **PROVEN**.

---

## 1. Dump — `00489D40` / `00489FC1`

`listing-00480000.txt`. Only `E8` of this fn is `0048A0AF`
(`e8.tsv`).

```
00489D40  sub esp, 76
00489D45  mov ebp, ecx                    // CPlayer
00489D4B  call 00A01B50                   // [this+44]
00489D53  call 00A01B50                   // [this+52]
00489D5C  xor ebx, ebx
00489D65  call 00488B20                   // find holy site → [esp+12]
00489D6E  test al, al
00489D70  mov al, [0x13B8647]
00489D75  jne 00489D86                    // site hit → create body
00489D77  cmp al, bl
00489D79  jne 00489D8E                    // flag != 0 → 13B8650 pose
00489D7B  xor al, al
00489D83  ret 4                           // FIRST-SEEN
…
00489D86  cmp al, bl
00489D88  je  00489E21                    // hit && flag==0 → site pose
00489D8E  … [0x13B8650] via vtbl+64 …
00489E21  mov ecx, esi
00489E23  call 006A4D00                   // Thing XYZ
          [esi+96] vtbl+288               // RHSet
…
00489FA5  mov ecx, [ebp+24]
00489FAD  call 009AD410                   // def from arg CString
00489FB2  mov edx, [ebp+40]
00489FB5  lea ecx, [esp+56]
00489FB9  push ecx                       // arg2 params
00489FBA  push edx                       // arg1 [CPlayer+40]
00489FBB  lea edx, [esp+52]              // pose / RHSet
00489FBF  mov ecx, eax                   // def
00489FC1  call 006AC910                  // FIRST Hero Thing
…
0048A066  mov al, 0x01
0048A06C  ret 4
```

`[0x13B8647]` has **no** listing writer (sibling bytes
`13B8640` / `13B8641` / `13B8646` / `13B8648` are other
flags). First-seen stays **0**. **PROVEN** as early-out
on the Load World miss.

`00488B20` (`00489D65` only `E8`):

```
0048D5C0                 // collect candidates from [player+32]→[+4]+140
[esi+244]!=0 → 00488C53  // skip name; nearest vs [esi+232]
else 0099B2C0 [0x13B866C]
     0048CC60 + pred 0048BC70   // ScriptName == name (00BFEBA8)
hit  → store Thing, al=1
miss → "*** WARNING : failed to find a holy site with Sc"
       zero list → al=0
```

`0048BC70` compares Thing `+116` ScriptName to the search
string. Exact match. Lookout TNG after maps:

| ScriptName | No-save pose? |
|---|---|
| **`GuildArrivalHSP`** | **yes** (52.688, 69.597, 36.982) |
| `LookoutPointHSP` | no |
| `MAIN_START_POSITION` | no |

`NOVStartHSP` is **not** in that file (`script-setnewstart`).
`userst.ini` `SetStartingHolySite("NOVStartHSP")` writes
`[0x13B866C]` **before** frontend. Empty vs `"NOVStartHSP"`
both miss at Load World. **PROVEN**.

`[CPlayer+244]` skip-name is set by `00487470` (no `E8`;
copies XYZ to `+232` then `+244=1`). First-seen `+244` is
0. Success after maps with `NOVStartHSP` still in `+866C`
therefore needs **`+244!=0`** or a rewritten `+866C` or
`[0x13B8647]!=0`. Those writers on the no-save play path
are **UNREAD**.

---

## 2. Dump — `006AC910` at `00489FC1`

`listing-00680000.txt`. `E8` sites: **`00489FC1`** (this
create) and `0089F660` (leftover `al==2` convert inside
`0089F4A0`; goes through `0066EBE0`, not `00489D40`).

```
006AC910  sub esp, 64
006AC916  mov ebx, edx              // pose / RHSet
006AC918  mov edi, ecx              // def
006AC91C  mov ecx, 0x208
006AC923  call 004C7380
006AC933  call 0052AB20             // CThingPlayerCreature
006AC942  mov eax, [esp+88]        // stdcall arg2
006AC946  mov ecx, [esp+84]        // stdcall arg1 = [CPlayer+40]
006AC94A  push eax
006AC94B  push ecx
006AC94C  push ebx
006AC94D  push edi
006AC950  call 006A9DD0
006AC95F  push "CThingPlayerCreature::Create 1"
…
006AC9D4  call 004C9CA0             // activate
006ACA13  ret 8
```

Factory size `0x208` / `0052B880`. Graphic from
`CREATURE_HERO` is **4299** `MESH_HERO`. **PROVEN**
(`hero-4299-create`, `GameBinFormatTests`).

---

## 3. Who can call `00489D40` — complete `E8` graph

```
00489D40 CreateCharacter
  only E8: 0048A0AF

0048A070 InitCharacterAs
  if [this+52]==0 OR [Thing+145] bit0:
    push [esp+12]                  // incoming CString*
    call 00489D40
  E8 sites:
    00449E2D  00449D90             // CREATURE_HERO  ← 4299
    00449B31  00449B20             // coop

00449D90
  009AD410 "PLAYER_HERO" → 0044BA90 miss
  00449E0D push "CREATURE_HERO"
  004498C0 then 0048A070(CString CREATURE_HERO)
  only E8: 0049F1D7

0049F180 Init Characters
  00449970 / 00487DC0 miss → 00449D90([esp+56])
  E8 sites:
    004A2C80  push 1   inside 004A1840 if [world+258]==0
    00416BCA  push 0   after 004A1840 if [0x13B8648]==0

00449B20
  004498C0(arg0) then 0048A070(arg1)
  only E8: 0066FF89

0066FF20                       // CTCCoopSpirit
  00686D40 = [0x13B8A1C]+48    // player manager
  00449700 / 00449CB0 / 00449730
  00449B20(playerIndex, coop name)
  only E8: 0067078D
  00670710: cmp [this+16], 20 ; jl skip
  00670710 only E8: 00670A96 (00670A10 vtbl tick)
  00670B67 push "CTCCoopSpirit"
```

`00449730` jump table writes **`COOP_SPIRIT_PLAYER_ONE` /
`THREE` / `FOUR` / `TWO`**. **PROVEN**. That
`009AD410` is not `CREATURE_HERO` / 4299.

`0089F660` is **not** a `00489D40` retry.

---

## 4. First success args (4299 take)

When `00488B20` hits (or `[0x13B8647]!=0`) on the
`00449D90` string:

| Call | `ecx` | `edx` | stack |
|---|---|---|---|
| `00489D40` | `CPlayer` (slot from `004498C0`) | — | CString **`"CREATURE_HERO"`** (`ret 4`) |
| `00489FC1` `006AC910` | `009AD410` def of that name | pose / RHSet buffer | arg1=`[CPlayer+40]`, arg2=params |

Holy hit && `[0x13B8647]==0` takes `00489E21`:
`006A4D00(esi)` + `[esi+96]` vtbl+288. First Lookout
site the host (and first-scene dump) consume is
**`GuildArrivalHSP`** `(52.688, 69.597, 36.982)`, axes
+X / +Z. **PROVEN** identity / pose. Native
`00488B20` name vs that ScriptName is **PARTIAL**
(`NOVStartHSP` still in `[0x13B866C]` unless `+244`
or a rewrite).

`0049F180(0)` vs `(1)` is **not** the def name. `00449D90`
`ret 4` never reads that bool for the miss path; both
pushes `"CREATURE_HERO"` into `0048A070`. **PROVEN**.

Not these args:

| Arg | Why not first 4299 |
|---|---|
| `"CREATURE_HERO_CHILD"` | only `00DBDE40` / Oakvale leftover |
| `"COOP_SPIRIT_PLAYER_*"` | `0066FF20` leftover |
| `PLAYER_HERO` def | no Graphic; `0044BA90` miss |

---

## 5. Condition for the first `00489FC1`

```
00488B20 al==1   OR   [0x13B8647]!=0
AND  [CPlayer+52] empty (or Thing+145 bit0)
AND  009AD410(arg) is CREATURE_HERO
```

Load World first-seen: list empty (dummy map / TNG not
applied), flag 0 → **`ret 0`**. **PROVEN**.

After `006C2170` ContainsMap, Lookout holy Things exist.
Name `NOVStartHSP` still does not match
`GuildArrivalHSP`. A later `00449D90` take therefore
still early-outs unless one of:

1. `[CPlayer+244]!=0` — nearest-site walk (`00487470`).
2. `[0x13B866C]` rewritten to a loaded Lookout ScriptName
   (writers: `00413840` before frontend, `00416A55`
   `game+90580` if non-empty).
3. `[0x13B8647]!=0` — `13B8650` pose path.

Which of those first becomes true on no-save is **UNREAD**.
Host skips the name walk and picks `GuildArrivalHSP`.
**DIVERGE** vs a raw `00488B20(NOVStartHSP)` if `+244`
stays 0.

`00501450` / `006C2170` / `0051E2F0` have **no** `E8`
of `00489D40` / `0048A070` / `0049F180`. **PROVEN**.

---

## 6. Not these

| Candidate | Why not first 4299 `00489FC1` |
|---|---|
| `004A2C80` | same Load World as `00416BCA`, **before** maps; would miss or would be the *first* call, not a later success |
| `0066FF20` / `00449B20` | `CTCCoopSpirit`; `COOP_SPIRIT_PLAYER_*` |
| `0089F660` | leftover `006AC910`; not `00489D40` |
| `0051FD80` Lookout | no `PlayerCreature` NewThing |
| `00487470` | sets `+244`; does not call create |
| `00488AB0` / `004887C0` | world-frame player tick; no `E8 00489D40` |
| `00DBDE40` / kid 4300 | leftover intro |

---

## Host leftovers

| Host | Native | Class |
|---|---|---|
| `SpawnHeroFromPlayerStart` Notes `0049F180` / `00489D40` after ContainsMap | those VAs already ran (and missed) in Load World | **LEFTOVER** site |
| `ResolveHeroDefinition` Notes `00449D90` as LevelLoader | same: bind already ran | **LEFTOVER** |
| Prefer `GuildArrivalHSP` by ScriptName | native `00488B20` uses `[0x13B866C]` / `+244` | **MATCH** pose. **DIVERGE** selector unless `+244` / rewrite |

---

## Classification table

| Claim | Status |
|---|---|
| First `00489D40` after Leave is Load World `0048A0AF` and returns 0 | **PROVEN** |
| A second `.text` `E8` of `00489D40` exists | **DISPROVEN** |
| `0066FF20` is the first 4299 retry | **DISPROVEN** (`CTCCoopSpirit`) |
| `004A2C80` is the post-map 4299 retry | **DISPROVEN** (order) |
| First `00489FC1` / `006AC910` is `CREATURE_HERO` / **4299** at `GuildArrivalHSP` | **PROVEN** identity |
| First success args are `ecx=CPlayer`, name `"CREATURE_HERO"`, pose from the holy Thing | **PROVEN** listing. Live `+244` / `+866C` **PARTIAL** |
| `CREATURE_HERO_CHILD` is that create | **DISPROVEN** |
| Later `E8 0049F180` after `006C2170` | **DISPROVEN** (none). Live take **UNREAD** |

---

## Do not

- Call `0066FF20` / `COOP_SPIRIT_PLAYER_*` from first Lookout spawn.
- Treat `004A2C80` as “after maps”.
- Treat `0089F660` as `00489FC1`.
- Spawn `CREATURE_HERO_CHILD` / 4300 here.
- Note `0049F180` as a child of `006AC910`.

Open: first-seen writer of `CPlayer+244` / `[0x13B8647]` /
rewrite of `[0x13B866C]` after Leave; first-seen take of
`004A2C80` (`[world+258]`); the non-`E8` site that first
re-enters `0048A0AF` with `"CREATURE_HERO"` after
ContainsMap.
