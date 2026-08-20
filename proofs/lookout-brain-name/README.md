# Lookout first CAIBrain name (CREATURE+232)

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `Hero.WaitTask` /
`VILL1.WalkTo` / `CAIStateGroup_MinionWander` /
`BRAIN_GOOD_VILLAGER_BASE`. That path is later leftover
`Q_NewOakValeIntro`, not Leave / Init Game / first no-save
Present.

Sibling `proofs/creature-ai-first` already proved the **attach
site**. This note is the **brain identity** on that site:
`CCreatureDef+232` / `0079BD80`, first `CAIStateGroup_*`, and
whether `BRAIN_STAND_AROUND_LIKE_A_MORON` is live.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Question: first `CAIBrain` after Leave is Lookout
`CREATURE_BS_VILLAGER_MALE` `FH_Villager`. What name does
`CCreatureDef+232` resolve through `0079BD80`? What is the
first `CAIStateGroup_*`? Is `BRAIN_STAND_AROUND_*` gated on
`[0x13B86EA]`?

Authority: dump `0088C160` / `0079BD80` / `00833010` /
`008338D0`; `proofs/creature-ai-first`; listings
`listing-00800000.txt` / `listing-00880000.txt` /
`listing-00780000.txt` / `listing-00980000.txt` /
`listing-004c0000.txt` / `listing-00440000.txt`; RTTI
`CAIBrain` / `CBrainDef` / `CCreatureDef` / `CAIStateGroupBase`;
TLC `data\CompiledDefs\game.bin` (index path only; instance
string not printed here); `userst.ini`.

---

## Verdict

**Retail first brain is not `BRAIN_STAND_AROUND_LIKE_A_MORON`
and not `BRAIN_NULL`.** Those are a debug override and a
post-attach name compare. The live key is **CREATURE def
`+232`**: a **1-based definition-manager index**, looked up
by **`0079BD80` → `009AD9E0`**, yielding a **`CBrainDef*`**
(`BRAIN` type). `00833010` then allocs the `CAIBrain` fibre
`0088C160`.

The **instance string** of that `BRAIN` row for
`CREATURE_BS_VILLAGER_MALE` is **UNREAD** (game.bin sub-def
`FileName`; not an exe immediate). Do not invent
`BRAIN_GOOD_VILLAGER_BASE` / `BRAIN_NULL` /
`BRAIN_STAND_AROUND_*`.

First **`CAIStateGroup_*`** is the **first 12-byte slot** of
that `CBrainDef+60` vector, constructed in **`0088BF30`**
against table **`0x13BB008`**. Class name **UNREAD**.

| Claim | Status |
|---|---|
| First `CAIBrain` after Leave is Lookout `FH_Villager` `CREATURE_BS_VILLAGER_MALE` | **PROVEN** create (`npc-first-create`); attach site **PROVEN** code |
| Task object is `0088C160` fibre, vtbl `012780C4`, stack `0x7D00` | **PROVEN** (`00833010` / `0088C160`) |
| TNG `OverridingBrainName NULL` → Thing `+400` empty | **PROVEN** |
| `[0x13B86EA]` gates `BRAIN_STAND_AROUND_LIKE_A_MORON` onto `+400` | **PROVEN** gate |
| That byte is set on retail New Game | **DISPROVEN** (BSS; **no** `.text` writer; `userst.ini` has no such key) |
| First-seen brain name is `BRAIN_STAND_AROUND_LIKE_A_MORON` | **DISPROVEN** |
| First-seen brain name is `BRAIN_NULL` | **DISPROVEN** (compare only, `vtbl+40`) |
| `+232` is a CString name | **DISPROVEN** (`009AD9E0` indexes `table[id*4]+12`) |
| `+232` is a def-manager **index** → `CBrainDef*` | **PROVEN** type/path |
| Concrete `BRAIN_*` instance for this villager | **UNREAD** |
| First `CAIStateGroup_*` class on that brain | **UNREAD** |
| Oakvale `WaitTask` / `BRAIN_GOOD_VILLAGER_BASE` is this site | **DISPROVEN** / **LEFTOVER** |

Parent shorthand **`CCreatureDef+232`** is **`[Thing+224]+232`**.
`Thing+224` is the **`CREATURE` object** (factory `0044C065`,
ctor `00670F70`, vtbl `0125B2DC`, size **`0x14C`**). Nested
`CCreatureDef` is factory `0044C08A`, ctor `006765A0`, vtbl
`0125B43C`, size **`0xE8`** — it has **no** `+232` dword.
Keep the parent name; the dword lives on **CREATURE**.

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game
  Init World 004A6E30
  00416953 Load world
later 00501450 Lookout 00500540(1,0,0)
  006C2170 pass 2 objects
    LookoutPoint / Q_FireHeart
      0051FD80 CREATURE_BS_VILLAGER_MALE FH_Villager "0"
        005272E0 / 00831F80          // FIRST CThingAICreature
        vtbl+16 008315C0
          OverridingBrainName "NULL" → +400 empty   // 0099EFE0(0x122D70E)
        vtbl+32 00833A70
          00666310
          008338D0
            copy CREATURE +144…+160 → Thing +440…
            00830DD0 STANDARD_FLY?
            mov al, [0x13B86EA]
            test al, al
            je skip                    // retail: ZF, skip
            // skipped: +400 = "BRAIN_STAND_AROUND_LIKE_A_MORON"
            +400 empty ([ptr]==0 or [ptr+4]==0)
            CREATURE+232 != 0
              004C7990                 // [0x13B8A1C]+32 manager
              0079BD80(id=+232)        // 009AD9E0 table[id*4]+12
              00833010(CBrainDef*)
                alloc 0xA8
                0088C160               // CAIBrain fibre
                  00A44740(0, 0x7D00, 0.1f)
                  vtbl 012780C4
                  +144 = CBrainDef*
                  0088BF30             // groups from CBrainDef+60
            then 00833A70
              0079BD80 again
              CBrainDef.vtbl+40("BRAIN_NULL") 005B3440 compare
```

`00DB5DB6` `BRAIN_GOOD_VILLAGER_BASE` is **not** on this list.

---

## 1. Dumps

### `008338D0` — pick name then `00833010`

`listing-00800000.txt`:

```
008338D0  Thing ecx
  [+224] = CREATURE*
  copy [def+144,148,152,160] → Thing +440…+452
  00830DD0
  mov al, [0x13B86EA]
  test al, al
  je 00833946
    0099EFE0("BRAIN_STAND_AROUND_LIKE_A_MORON") → +400
00833946
  if +400 has heap string ([ptr]!=0 && [ptr+4]!=0):
    004C7990 / 006D3E80(name) / 00833010    // 009ADA40 by string
  else if [def+232] != 0:
    004C7990 / 0079BD80(id) / 00833010      // 009AD9E0 by index
```

Retail New Game takes the **`else if [def+232]`** arm.
**PROVEN** control flow. Occupancy of `+232` **PARTIAL**
(needs the villager CREATURE row to be non-zero; ctor zeros
it, persist fills it).

### `0079BD80` — index → `CBrainDef*`

`listing-00780000.txt`:

```
0079BD80  arg0 = id (CREATURE+232)
  test eax, eax
  jle fail          // 0 / negative is not a brain
  push id
  lea dest
  call 009AD9E0     // ecx = 004C7990 manager
  steal into out-ptr, inc ref
  al=1
```

`009AD9E0` (`listing-00980000.txt`):

```
009AD9E0
  009AD6E0(id)
  ecx = [[esi+164] + id*4]
  ecx = [ecx+12]            // live def object
  *out = ecx
```

That is a **table index**, not a hash and not a CString.
`006D3E80` is the **name** twin (`009ADA40`). **PROVEN.**

`id==0` fails (`jle`). Native IDs are **≥ 1**. Mapping onto
host `GameBinEntry.Index` (0-based) vs `NameRef.Counter` is
**PARTIAL**.

### `00833010` / `0088C160` — the fibre

`listing-00800000.txt` / `listing-00880000.txt`:

```
00833010  Thing ecx, CBrainDef* arg
  if [Thing+56] & 0x80000:  component 0xD3 → 008018B0
  alloc 0xA8
  0088C160(thing, …, CBrainDef*)
    00A44740(0, 0x7D00, 0.3DCCCCCD)
    vtbl 012780C4
    +144 = CBrainDef* (addref)
    0088BF30(CBrainDef*)          // fill +40/+44 group list
  00834E40 store at Thing +424
```

**PROVEN** as the first AI task object on this creature.

`BRAIN` factory is `00462E12` (`push 96` / `jmp 00459A6F`),
vtbl `0123520C`, size **96**. `CBrainDef+60/+64/+68` is the
group vector (12-byte records; `0x2AAAAAAB` = 1/12).

---

## 2. `[0x13B86EA]` / `BRAIN_STAND_AROUND_*`

Only `.text` site in the dump:

```
0083392D  mov al, [0x13B86EA]
00833932  test al, al
00833934  je 00833946
00833936  push "BRAIN_STAND_AROUND_LIKE_A_MORON"
```

No `mov [0x13B86EA]`, `or`, or `inc` in any listing.
Neighbour bytes `0x13B86E5`…`0x13B86EF` are other debug
gates; only `0x13B86E9` has a writer (`0065C7B9` **clears**
it).

VA `0x13B86EA` sits past file-backed `.data`
(`0x01374000 + 0x44000 = 0x013B8000`) → **BSS**, PE-zero.

`userst.ini` has `AllowDebugProfile FALSE;` and no
stand-around / moron key.

Retail New Game: byte **0** → skip the push → `+400` stays
empty (TNG `NULL`). **DISPROVEN** as the first-seen brain.

If a later cheat/console sets the byte, `006D3E80` would
look up the debug name **instead of** `+232`. That is not
Leave / first Present.

---

## 3. What `+232` is (and is not)

| Candidate | Why |
|---|---|
| TNG `OverridingBrainName` | **DISPROVEN** this file (`NULL` → empty `+400`) |
| `BRAIN_STAND_AROUND_LIKE_A_MORON` | **DISPROVEN** (gate off) |
| `BRAIN_NULL` | **DISPROVEN** as stored name; `00833AED` compare after fibre exists |
| CString at CREATURE+232 | **DISPROVEN** (`009AD9E0` `id*4` index) |
| Nested `CCreatureDef` field | **DISPROVEN** (size `0xE8`; no `+232`) |
| CREATURE `+232` def index | **PROVEN** as the lookup key |
| `BRAIN_GOOD_VILLAGER_BASE` | **LEFTOVER** (`00DB5DB6` Oakvale script) |
| `BRAIN_WIFE` / `_CHICKEN` / `_MELEE_GUARD` / `_ARENA_CELLS` / `_PLAYER_UNDEAD_SERVANT` | **DISPROVEN** as this villager (other sites) |

Exe immediates named `BRAIN_*` are **not** the villager
default. The default lives in **game.bin** as the `BRAIN`
sub-def of `CREATURE_BS_VILLAGER_MALE` (`HasSubDefTable`
includes `"CREATURE"`; type registrar `0044CF44` `"BRAIN"`).

`Fable.Dump bin CREATURE_BS_VILLAGER_MALE` prints that child
(`type=BRAIN inst=BRAIN_…`). This note did not run that dump.
Instance string **UNREAD**.

`00833A70` after attach:

```
0079BD80(def+232)
[CBrainDef].vtbl+40("BRAIN_NULL")
005B3440
```

If the resolved name **is** `BRAIN_NULL`, `+460` gets a
side table. That is a **null-brain** branch, not the assign.
**DISPROVEN** as the name stored into `+400` / `+232`.

---

## 4. First `CAIStateGroup_*`

`0088BF30` (`listing-00880000.txt`):

```
ebp = CBrainDef*
n = ([+64] - [+60]) / 12
for each 12-byte slot:
  name = intern 009D49B0(slot[0])
  skip if already on CBrainDef+72 list
  match 0x13BB008[i*8]  (CString + ctor)
  call ctor
  [group].vtbl+4(brain, slot+4, slot+8)
  push into CAIBrain +40/+44
```

First **constructed** group is **vector index 0** of
`CBrainDef+60`. First **run** group is later `0088B870` /
`008FCF10` (picks first slot with `[group+17] > arg` and
`008FCF10` true), then `[group].vtbl+8` (`008FCF50`).
First tick after Leave **UNREAD**.

Do **not** invent `CAIStateGroup_Wander` /
`StandStill` / `MinionWander` / `LookAtInterestingThings`.
Those RTTI names exist; this villager’s list is in game.bin.

Village `CVillageTask*` / `CAIStateGroup_VillageTask*` stay
**DISPROVEN** as this TNG (`VillageUID 0`; different RTTI).

---

## 5. Host vs native

| Host | Native after msg 15 | Class |
|---|---|---|
| Note `"Initial Activate vtbl+32"` | `00833A70` → `008338D0` | **MATCH** site |
| No `CAIBrain` | fibre `012780C4` at Thing `+424` | **DIVERGE** / missing |
| No CREATURE `+232` | index → `0079BD80` | **DIVERGE** / missing |
| `EntityTaskQueue` empty | no `CAction*` | **EQUIVALENT** empty script |
| Oakvale `WaitTask` in later leftover | not Leave | **DIVERGE** |

---

## Not these

| Candidate | Why not first Lookout brain |
|---|---|
| Frontend | no Things |
| Hero `006AC910` | later; PlayerCreature; no `0088C160` on that create |
| `LookoutPointBeggar` | later `V_BeggarAndChild` |
| Script `Create` / `00CBFB7D` | none after Leave |
| `Q_SunnyvaleMaster` fibre | quest watcher, not a creature brain |
| World-map fibre `006C26B0` | first *any* fibre; not AI |
| `CActionPlayAnimation` / `WaitTask` | leftover Oakvale |

---

## UNREAD / PARTIAL

- Live `00666310` / `006A4D60` success on first villager
  (`004C9B80` miss would skip the brain).
- Numeric CREATURE `+232` for `CREATURE_BS_VILLAGER_MALE`
  (persist fill vs ctor zero).
- `BRAIN` instance **string** (game.bin sub-def).
- First `CAIStateGroup_*` class in `0088BF30`.
- First `0088B870` / `vtbl+8` tick.
- Whether `0051E2F0` pass 1 re-enters `00833A70`.

---

## Classifications (short)

1. **First AI task after Leave is still `0088C160`
   `CAIBrain` on Lookout `FH_Villager`.** Not Oakvale
   `WaitTask`. **PROVEN** site.
2. **`BRAIN_STAND_AROUND_LIKE_A_MORON` is gated on
   `[0x13B86EA]` and that byte is BSS-zero with no writer.
   DISPROVEN as retail first-seen.**
3. **Retail key is CREATURE `+232` (parent name
   `CCreatureDef+232`): a def-manager index through
   `0079BD80` / `009AD9E0` → `CBrainDef*`.** **PROVEN**
   path; **UNREAD** instance name.
4. **First `CAIStateGroup_*` is slot 0 of that brain’s
   `+60` vector via `0088BF30` / `0x13BB008`.** Class
   **UNREAD**. Do not invent Wander / StandStill /
   MinionWander.
5. **`BRAIN_NULL` is a compare, not the assigned name.**

Do not start New Game AI at `Hero.WaitTask FOO`.
)
