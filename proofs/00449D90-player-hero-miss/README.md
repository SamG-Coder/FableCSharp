# `00449D90` `PLAYER_HERO` miss (first `0049F180` bind)

Investigation only. No production `src/` edits.

Question: first `0049F180` child `00449D90` is a
`PLAYER_HERO` miss, not `CREATURE_HERO_CHILD`. What
does the miss do? Host leftover Notes? Do **not**
create a Thing to fill a Note gap.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `Fable.exe` ExeIndex
`listing-00480000.txt` `0049F180`–`0049F1E5` /
`0048A070` / `00489D40`;
`listing-00440000.txt` `00449D90`–`00449E50` /
`0044BA90` / `004498C0`;
`e8.tsv` dests `00449D90` / `0048A070` / `00489D40`;
parent `proofs/0049F180-first-children`;
siblings `hero-4299-create`, `hero-00489D40-retry`,
`hero-retry-site`, `creature-after-leave`,
`hero-stats-first`;
`src/Fable.Game/EngineLifecycle.cs`
`InitCharactersAndQuests` / `SpawnHeroFromPlayerStart`
/ `ResolveHeroDefinition`;
`GameBinFormatTests` `FindMeshId("CREATURE_HERO")==4299`
/ `FindMeshId("CREATURE_HERO_CHILD")==4300`.

---

## Verdict table

| Claim | Answer | Class |
|---|---|---|
| First *work* child of no-save `0049F180` after the player-Thing miss? | **`0049F1D7` `00449D90`** (after `00449970` / `00487DC0` return 0) | **PROVEN** |
| That bind is `CREATURE_HERO_CHILD` / 4300 / `00DBDE40`? | **No.** Listing immediate is `"CREATURE_HERO"` | **DISPROVEN** |
| What the `PLAYER_HERO` miss **does**? | Intern name → `009AD410` → **`0044BA90` fail** (no Graphic) → `"CREATURE_HERO"` → `004498C0` slot → **`0048A070`** → empty `[CPlayer+52]` → **`00489D40`** → holy miss + `[0x13B8647]==0` → **`ret 0`** | **PROVEN** |
| Does first-seen `00449D90` create a Thing / `006AC910`? | **No.** | **PROVEN** |
| Host `InitCharactersAndQuests` Notes `00449D90`? | **No.** Notes `0049F180` + `00449970` / `00487DC0` only | **PROVEN** gap (**LEFTOVER**) |
| Host Notes `00449D90` later? | **Yes** — `SpawnHeroFromPlayerStart` / `ResolveHeroDefinition` under LevelLoader | **LEFTOVER** site. Identity **MATCH** |
| Fill that Note gap by constructing a Thing at Init Characters? | **No.** Native first take already `ret 0` | **DISPROVEN** as a create. **Do not** |

---

## Direct answers

**The miss is a name/factory fallback, not a spawn.**

`0049F180` `"Init Characters"` has no live player Thing
(`00487DC0` / `00A01B50` = 0), so it always
`push`es its stdcall arg and `call 00449D90`.
`ecx` is `[world+12]` (player manager).

`00449D90` looks up compiled `"PLAYER_HERO"`. TLC
`game.bin` has that def as type `PLAYER`, raw 21,
**0** subs, **no** Graphic. `0044BA90` (`arg<=0`
or empty `009AD9E0` appearance) returns `al=0` →
`je 00449E0B`.

Miss body:

1. Intern `"CREATURE_HERO"` (not `"CREATURE_HERO_CHILD"`).
2. `004498C0([esi+28])` — slot walk; miss falls back
   to `[manager+12][[manager+24]]`.
3. `0048A070` `CPlayer::InitCharacterAs` **on both**
   hit and miss.
4. Empty `[this+52]` → only `.text` `E8` of
   `00489D40` (`0048A0AF`).
5. First-seen: `00488B20` holy-site miss and
   `[0x13B8647]==0` → **`ret 0`**. No `00489FC1` /
   `006AC910`.

Then `0049F180` continues at `006B8410` (camera colour
bank reset). Init Characters is done with the Hero
**name**. The Hero **Thing** is a later take of the
same `"CREATURE_HERO"` string after Lookout maps
(`hero-4299-create`).

Host leftover is **Notes only**: the native VA ran
here and created nothing. `InitCharactersAndQuests`
does not spawn — that is **MATCH**.
`SpawnHeroFromPlayerStart` Notes this VA again when
it *does* create. Do **not** add a Thing at
`0049F180` to make the missing Note look filled.

---

## 1. Call site — only `E8` of `00449D90`

`e8.tsv` dest `00449D90`: **one** site `0049F1D7`.
**PROVEN.**

`listing-00480000.txt`:

```
0049F1B3  mov ecx, [esi+12]
0049F1B6  call 00449970
0049F1BD  call 00487DC0
0049F1C4  je  0049F1CF              // no Thing → bind
0049F1C6  test [eax+145], 1
0049F1CD  je  0049F1DC              // live + bit0 clear → skip
0049F1CF  mov eax, [esp+56]         // 0049F180 stdcall 0 or 1
0049F1D3  mov ecx, [esi+12]
0049F1D6  push eax
0049F1D7  call 00449D90             // ret 4
0049F1DC  mov ecx, [esi+24]
0049F1DF  mov ecx, [ecx+6500]
0049F1E5  call 006B8410
```

No-save first-seen is `00416BCA` `push 0`
(`0049F180-first-children`). `004A2C80` `push 1`
is save `004A21F0`, not this walk.

`00449D90` does **not** branch on that dword for
`PLAYER_HERO` vs `"CREATURE_HERO"`. It is leftover
on the stack for `0048A070` / `00489D40`. First-seen
create still `ret 0` either way.

No-save has no player Thing → **always** `0049F1D7`.
**PROVEN.**

---

## 2. Body — miss vs hit

`listing-00440000.txt` `00449D90`–`00449E50`
(`ret 4`):

```
00449D90  sub esp, 8
          esi = ecx                    // player manager
          0099EBF0 "PLAYER_HERO"
          009AD410([esi+8], &name)
          0044BA90([esi+8], def, &out)
          test al, al
          je  00449E0B                 // THIS TAKE
; hit (not TLC):
          [edi+60] → 009ACCC0 / 009D49B0
          push [esi+28]
          jmp 00449E24
; miss:
00449E0B  push -1
00449E0D  push "CREATURE_HERO"         // not CHILD
          0099EBF0
          push [esi+28]
00449E24  mov ecx, esi
          call 004498C0                // slot by [+40]
          mov ecx, eax
          call 0048A070                // both arms
          0099EAE0
          ; optional [edi+4] release
          ret 4
```

`0044BA90` (`ret 8`):

```
eax = arg
test eax, eax
jle 0044BAF7          // xor al,al
009AD9E0 appearance
je  0044BAF7          // empty → fail
… attach …
mov al, 1
```

TLC `PLAYER_HERO` has **no** Graphic → `al=0`.
Exact `009AD410` return dword (0 vs a PLAYER def
id with empty appearance) is **PARTIAL**. The
**`je 00449E0B` take** is **PROVEN**.

`004498C0` (`ret 4`): walk `[this+12]..+16]` for
`[slot+40] == arg`; miss →
`[this+12][[this+24]]`. Slot getter, not a
factory. **PROVEN.**

---

## 3. `0048A070` does not spawn first-seen

`e8.tsv` dest `0048A070`: `00449E2D` (this miss)
and `00449B31` (`00449B20` / later
`COOP_SPIRIT_PLAYER_*`). Second parent is
**DISPROVEN** as this Hero.

`0048A070`:

```
vtbl+12 / +48 on [this+28] → +32 / +36
00A01B50([this+52])
je  0048A0A8                    // empty → create
test [eax+145], 1
je  0048A0B4                    // live + bit0 clear → skip
0048A0A8  push [esp+12]         // 00449D90's unused arg
          call 00489D40         // ONLY E8 (e8.tsv 0048A0AF)
then log "CPlayer::InitCharacterAs"
```

First-seen `[this+52]` empty → `00489D40`.
`00489D40`: `00488B20` miss + `[0x13B8647]==0` →
`xor al,al` / `ret 4`. **No** `006AC910`.
**PROVEN** (`hero-00489D40-retry`).

So the miss **names** `CREATURE_HERO` and **attempts**
create. The attempt fails. That is the whole first
take.

---

## 4. Not `CREATURE_HERO_CHILD`

| String / id | This `00449D90`? |
|---|---|
| `"PLAYER_HERO"` | **PROVEN** first intern |
| `"CREATURE_HERO"` | **PROVEN** miss immediate `00449E0D` |
| `"CREATURE_HERO_CHILD"` | **DISPROVEN** — zero hits in `listing-00440000.txt` near this fn |
| Graphic **4299** `MESH_HERO` | later successful `006AC910`, not this take |
| Graphic **4300** | Oakvale leftover `00DBDE40` |

`GameBinFormatTests`: `CREATURE_HERO` / `_TRAINING`
→ **4299**; `CREATURE_HERO_CHILD` / `YOUNG_HERO`
→ **4300**. Different defs.

Other `"PLAYER_HERO"` intern `004497FA` is
`004497E0` (slot-name helper: 4 →
`PLAYER_NEUTRAL`, `[esi+28]` → `PLAYER_HERO`,
else `PLAYER_SPIRIT`). Not a child of
`0049F180`.

---

## 5. Host leftover Notes

`InitCharactersAndQuests` (called from Load World
when `[0x13B8648]==0` — **MATCH** site):

```
Note(InitCharactersFn, … "0049F180 push 0 ecx=world");
Note(PlayerCreatureBindFn, … "00449970 / 00487DC0");
Note(InitGuiFn, … "0043A380 …");
… quests …
```

No `Note(InitHeroDefFn)`. No `009AD410 PLAYER_HERO`.
No `00449E0D`. No `0048A070`. No `00489D40`.

Later, after Lookout `006C2170`,
`SpawnHeroFromPlayerStart`:

```
Note(InitCharactersFn, … "0049F180 Init Characters");
Note(InitHeroDefFn, … "00449D90 PLAYER_HERO then CREATURE_HERO");
Note(CreateCharacterFn, … "00489D40 " + HSP);
SpawnHero → ResolveHeroDefinition → InsertThing
```

`ResolveHeroDefinition`:

```
Note(DefLookupFn, … "009AD410 PLAYER_HERO");
if PLAYER_HERO has mesh > 0 → that name
else
  Note(InitHeroDefFn, … "00449E0D CREATURE_HERO fallback");
  Note(InitCharacterAsFn, … "0048A070 CREATURE_HERO");
```

TLC always takes the else. **MATCH** id.

| Host | Native first-seen | Class |
|---|---|---|
| `InitCharactersAndQuests` omits `00449D90` | `0049F1D7` always on no-save | **LEFTOVER** gap |
| Same method does **not** `new ThingInstance` | `00489D40` `ret 0` | **MATCH** — keep |
| `SpawnHeroFromPlayerStart` Notes `0049F180` / `00449D90` | those insns already ran at `00416BCA` | **LEFTOVER** site |
| `ResolveHeroDefinition` Notes during `SpawnHero` | same identity, different time | **LEFTOVER** site. **MATCH** fallback |
| Recover text “Hero via `0049F180` → `006AC910`” | two stacks | **LEFTOVER** wording (`hero-4299-create`) |

Filling the Init Characters gap with a create would
**DIVERGE**: native first take is a failed
`00489D40`. First Hero Thing stays later
`006AC910` at `GuildArrivalHSP`.

---

## 6. Not these

| Candidate | Why not this miss |
|---|---|
| `CREATURE_HERO_CHILD` / `00DBDE40` | Oakvale intro leftover |
| `006AC910` / mesh 4299 | later Lookout; not a child of this take |
| `0051FD80` | region TNG after dummy pumps |
| `004497E0` `"PLAYER_HERO"` | slot-name helper, other fn |
| `00449B20` / `0066FF20` | other `0048A070`; coop spirit |
| Frontend `009AD410` | UI Type=10 |
| `004EE23F` `CHeroDef` | Init Game type register |
| `004166A8` Create Players | slots + GUI ctor, before this fn |
| `004AE9D0` | tick slots, not Hero |

---

## Open

- `009AD410("PLAYER_HERO")` exact return vs
  `0044BA90` `arg<=0` vs empty `009AD9E0`:
  **PARTIAL** (fail take **PROVEN**).
- Which non-`E8` feeder later re-enters
  `00449E2D` so `00489D40` hits `00489FC1`:
  **UNREAD** (`hero-retry-site`).
- Load-game `004A2C80` body of this same fn:
  **UNREAD** (not no-save).
