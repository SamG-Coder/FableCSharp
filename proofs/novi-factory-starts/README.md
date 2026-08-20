# `00DABAC0` factories: what each start actually runs

Investigation only. No production `src/` edits.

Do **not** treat factory VAs as cutscene / wander / watch
entry points. Each `00DABAC0` row writes a construct-only
factory at record `+16`. The factory allocates, plants a
vtbl, and hangs release `00CDEE00`. It does **not** call
`00CBFB7D`. Start is later object `vtbl+4`.

Do **not** call `WatchBarrels` (`00DBDE40` / `00DBE890`) a
factory start. That watcher is attached **after** all 16
name rows, after `StartBarrelTimer`, before deeds in
`00DBDE40`.

Host `ScriptFactoryTable.Recovered` has `Start` filled only
for `NOVI_LiveFather` (`00DB86B0`). The other 15 rows still
say `"start body unread"`. This note fills those bodies
from the listing.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER**.

Sources: `listing-00d80000.txt` `00DABAC0`–`00DAC2A1` and
factory/`sub esp` bodies; `novi-livefather-rdata-012d8370`
(16 u32 spill); `vtbl-novi-livefather-vtbl-012d8388`;
`00-index/xrefs.tsv` `fn=`; `text-map/functions.tsv`;
`src/Fable.Game/ScriptFactoryTable.cs`;
`RegionTravel.cs` (`WatchBarrels*`, `IntroCutsceneStart`);
`proofs/script-factory-tables/README.md`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Does a factory start a cutscene / wander / watch? | **no** — construct + `00CDEE00` release | **PROVEN** |
| Where is start? | object vtbl+4 (LiveFather = `00DB86B0`) | **PROVEN** |
| When are names registered vs deeds? | 16× `00CB8230` then objective / `StartBarrelTimer` then `00DBDE40` | **PROVEN** |
| Is `WatchBarrels` a `NOVI_*` factory start? | **no** — `00DBDE40` | **DISPROVEN** |
| Host `Start` for 15 names | still 0 / unread | **PROVEN** host gap |

---

## 1. Registrar `00DABAC0` (before deeds)

`listing-00d80000.txt` `00DABAC0`–`00DAC2A1`. Sixteen
identical 28-byte records, vtbl `0x012D8370`, factory at
`[edi+16]`, then `00CB8230`.

| # | Name at push | Factory `[+16]` | Site |
|--:|---|---|---|
| 1 | `NOVI_LiveFather` | `00DAC2C0` | `00DABB0C` |
| 2 | `NOVI_Theresa` | `00DAC420` | `00DABB6F` |
| 3 | `NOVI_Guard` | `00DAC580` | `00DABBD3` |
| 4 | `NOVI_Villager` | `00DADE50` | `00DABC37` |
| 5 | `NOVI_Bully` | `00DAEC60` | `00DABC9B` |
| 6 | `NOVI_Victim` | `00DAEDE0` | `00DABCFF` |
| 7 | `NOVI_TeddyGirl` | `00DAEF50` | `00DABD63` |
| 8 | `NOVI_AffairMan` | `00DB0880` | `00DABDCA` |
| 9 | `NOVI_AffairWoman` | `00DB1DB0` | `00DABE33` |
| 10 | `NOVI_AffairWife` | `00DB29A0` | `00DABE9D` |
| 11 | `NOVI_BookTrader` | `00DB3E30` | `00DABF07` |
| 12 | `NOVI_BarrelMan` | `00DB51B0` | `00DABF71` |
| 13 | `NOVI_BarrelThug` | `00DB6B40` | `00DABFDB` |
| 14 | `NOVI_Barrel` | `00DB7D00` | `00DAC045` |
| 15 | `NOVI_CreatedBeetle` | `00DB7FF0` | `00DAC0AF` |
| 16 | `OVI_DeadFather` | `00DB81B0` | `00DAC119` |

After row 16 (`00DAC12E` `00CB8230`):

```
00DAC146  call 00CB8930
00DAC158  test [esi+80]          ; AttackOver
          je skip
          Q__OakValeIntro_PostAttack
00DAC1BA  TEXT_QUEST_OAKVALE_INTRO_OBJECTIVE_01
00DAC22B  "StartBarrelTimer"
00DAC247  call 00CDD450          ; watcher ctor
00DAC24C  vtbl 012D7A3C
00DAC252  callback 00DB4F70
00DAC274  call 00CB7E50          ; attach
00DAC295  call 00DBDE40          ; WatchBarrels / deeds
```

Name bind is **before** deeds. **PROVEN.**

`00DBDE40` owns `WatchBarrels` (`xrefs` `00DBDF5E`) and
`WatchForGotGold`. `ManageQuestCoreMarkers` later names
`CREATURE_OAKVALE_STAG_BEETLE` (stag, not wasp). That is
**not** `NOVI_CreatedBeetle` vtbl+4.

---

## 2. Factory body (LiveFather + Bully)

Both listings are the same shape. LiveFather
`00DAC2C0`:

```
push 32 / 00BFEA1A
[esi] = 0x12C3224
004ABE90
[esi] = 0x12D8388          ; object vtbl
[esi+20] = ebx, [esi+24] = ecx
alloc 12
[eax] = 1
[eax+4] = 00CDEE00         ; release, not start
[eax+8] = object
ret 8
```

Bully `00DAEC60` is identical except `push 44` and vtbl
`0x12D879C`.

`00CDEE00` (`listing-00cc0000.txt`): `test ecx; push 1;
call [eax]` — dtor. **DISPROVEN** as start.

LiveFather vtbl `012D8388` (`vtbl` dump + rdata):

| slot | VA | role |
|---|---|---|
| +0 | `00DAC370` | dtor |
| +4 | `00DB86B0` | **start** |
| +8 | `00DAC390` | helper |
| +12 | `00DAC360` | getter `[ecx+20]` |

Fiber persist `00DB8630` / construct `00DB8520` calls
`[+52].vtbl+4`. **PROVEN** for LiveFather.

Other factories write the vtbls below. Slot +4 is the
first `sub esp, imm` after the factory’s dtor/helper,
matching LiveFather. Theresa slot +4 is also in the
16-u32 rdata spill (`012D83A8 = 00DB97A0`). **PROVEN**
Theresa. Others **PARTIAL** (same layout, no extra rdata
dump).

---

## 3. Per-name start (vtbl+4)

Kind is the **first real work** of that start, not a
later branch.

- **cutscene** — `00CBFB7D` / named `CS_*` as the script
  payload (DeadFather uses `007E73F0` + `CS_DEAD_DAD`).
- **wander** — NPC loop: talk / crime / give / WalkTo
  helpers `007E7490`/`007E7390`/`007E7450`. No
  `00CBFB7D` on the owning start.
- **watch** — wait-on-radius / instruction, or a
  `00CDD450` watcher. `NOVI_Barrel` is the only factory
  start in this bucket. `WatchBarrels` is **not** here.

| Name | Factory | vtbl | Start | Kind | First evidence |
|---|---|---|---|---|---|
| `NOVI_LiveFather` | `00DAC2C0` | `012D8388` | `00DB86B0` | **cutscene** | `00DB88DD` `CS_OAKVALE_INTRO_FATHER` → `00CBFB7D` |
| `NOVI_Theresa` | `00DAC420` | `012D83A4` | `00DB97A0` | **cutscene** | `CS_OAKVALE_INTRO_THERESA_MEET*` then `CS_OAKVALE_INTRO_THERESA` + raid AVI (`fn=00DB97A0`) |
| `NOVI_Guard` | `00DAC580` | `012D83C0` | `00DAC760` | **wander** | `sub esp, 0x148`; deed-count `[+88]/[+104]`; `TEXT_QST_048_GUARD_*`; **no** `00CBFB7D` |
| `NOVI_Villager` | `00DADE50` | `012D8678` | `00DADF80` | **wander** | `sub esp, 108`; `SCRIPT_NAME_HERO`; `TEXT_QST_048_VILLAGER_*`; **no** `00CBFB7D` |
| `NOVI_Bully` | `00DAEC60` | `012D879C` | `00DBCD60` | **wander** | `sub esp, 0x114`; hero tests; later `CS_OAKVALEINTRO_BRATHIT` is a **branch**, not first line |
| `NOVI_Victim` | `00DAEDE0` | `012D87B8` | `00DBB310` | **wander** | `sub esp, 0x13C`; wait `vtbl+32`; later `CS_OAKVALEINTRO_BULLYRUN1/2/DUMMY` |
| `NOVI_TeddyGirl` | `00DAEF50` | `012D87D4` | `00DAF080` | **wander** | `sub esp, 0x158`; lookup `NOVI_Bully` + teddy give; **no** `00CBFB7D` |
| `NOVI_AffairMan` | `00DB0880` | `012D89EC` | `00DB09E0` | **wander** | `sub esp, 0x140`; `TEXT_QST_048_AFFAIRMAN_*`; **no** `00CBFB7D` |
| `NOVI_AffairWoman` | `00DB1DB0` | `012D8C08` | `00DB1F00` | **wander** | `sub esp, 0xAC`; `TEXT_QST_048_AFFAIRWOMAN_*`; **no** `00CBFB7D` |
| `NOVI_AffairWife` | `00DB29A0` | `012D8C98` | `00DB2B10` | **wander** | `sub esp, 0xA4`; `TEXT_QST_048_AFFAIR_WIFE_*`; **no** `00CBFB7D` |
| `NOVI_BookTrader` | `00DB3E30` | `012D8E80` | `00DB3FA0` | **wander** | `sub esp, 0x128`; `TEXT_QST_048_TRADER_*`; **no** `00CBFB7D` |
| `NOVI_BarrelMan` | `00DB51B0` | `012D9014` | `00DB5330` | **wander** | `sub esp, 0x194`; `M_BarrelManWalkOff*`; **no** `00CBFB7D` |
| `NOVI_BarrelThug` | `00DB6B40` | `012D9234` | `00DB6C60` | **wander** | `sub esp, 0xE8`; `TEXT_QST_048_BARRELTHUG_*`; **no** `00CBFB7D` |
| `NOVI_Barrel` | `00DB7D00` | `012D94F0` | `00DB7E10` | **watch** | `00CBE2FF` (`WaitForUnderRadius`, r=2.0) then `TEXT_QST_048_INSTRUCTION_BREAK_BARRELS`; **not** `WatchBarrels` |
| `NOVI_CreatedBeetle` | `00DB7FF0` | `012D9560` | `00DB80C0` | **wander** | wait `[0x143E8F8]` vtbl+348/+356/+360 then `[edx+432](1,1)`; no `CS_*`; stag spawn string is in `00DBDE40`, not here |
| `OVI_DeadFather` | `00DB81B0` | `012D957C` | `00DB8300` | **cutscene** | `MK_OVID_DAD` then `CS_DEAD_DAD` via `007E73F0` (not `00CBFB7D`) |

`xrefs.tsv` owning `fn=` can sit on the preceding helper
(`00DAC650` Guard, `00DB0950` AffairMan, `00DB2A70` Wife,
`00DB3EE0` Trader, `00DB5310` BarrelMan, `00DB6C40` Thug).
The start column above is the `sub esp, imm` body, same
rule as proven `00DB86B0`.

Bully-run / brat-hit `CS_*` live **inside** Victim/Bully
starts as later events. First opcode of those starts is
not `00CBFB7D`. Class = **wander**, with a later cutscene
branch. **PARTIAL** if someone wants “ever plays a CS”.

---

## 4. Host `ScriptFactoryTable` vs listing

| Host | Listing | Class |
|---|---|---|
| 16 `Recovered` names + factory VAs | 16 `00DABAC0` rows | **PROVEN** |
| `LiveFatherStart = 00DB86B0` | vtbl+4 | **PROVEN** |
| `Bind(..., Start=0, "start body unread")` ×15 | starts in §3 | **PROVEN** unread in host |
| comment “factory starts the cutscene” | only Father / Theresa / DeadFather | **DISPROVEN** as generic |
| `WatchBarrelsThing = NOVI_Barrel` | barrel **start** is radius+text; WatchBarrels is `00DBDE40` | **DISPROVEN** as same start |

---

## Classifications (short)

1. **Factory ≠ start — PROVEN.** `00DAC2C0` and `00DAEC60`
   only construct. Release `00CDEE00`.
2. **Start = vtbl+4 — PROVEN** LiveFather / Theresa.
   **PARTIAL** same layout for the other 14.
3. **Cutscene starts —** `NOVI_LiveFather` `00DB86B0`,
   `NOVI_Theresa` `00DB97A0`, `OVI_DeadFather` `00DB8300`.
4. **Watch start —** only `NOVI_Barrel` `00DB7E10`
   (`00CBE2FF`). `WatchBarrels` is **not** a factory.
5. **Wander starts —** Guard, Villager, Bully, Victim,
   TeddyGirl, Affair*, BookTrader, BarrelMan, BarrelThug,
   CreatedBeetle. Beetle is stag-path leftover in
   `00DBDE40`, not a wasp.
6. **Host Start column — UNREAD** for 15 names until
   `ScriptFactoryTable` is updated. This proof does not
   edit `src/`.
