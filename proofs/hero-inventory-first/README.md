# First hero inventory / equipment bind after Leave

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `GiveHero OBJECT_TEDDY_BEAR_UNGIVEABLE`
/ `HeroWear` / `SetHeroWeapon` / clothing GUI `005B6881`. Those are
later leftover `Q_NewOakValeIntro` or menu screens, not Leave /
Init Game / first no-save Present.

Do **not** treat frontend `SetTexture` / type `0x22` as inventory.
That is `frontend.bin` sprites. This note is **not frontend**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `rtti.txt` / `strings.tsv` / `xrefs.tsv`;
listings `004184BD` / `004EE23F` / `004EECFE` / `004D2E74` /
`00590D32` / `005BFF07` / `0043A380` / `006A9DD0` / `00CC6392`;
`docs/runtime/FORWARD_TREE.md` §§6–10;
`proofs/script-global-cmds/README.md`, `proofs/bone-config-first/`,
`proofs/player-bind-world/`, `proofs/entity-task-queue/`;
`docs/status/investigations/E-player-palskin.md`,
`2026-08-18-palskin.md`;
`src/Fable.Game/Scripting/ExecutionContext.cs` (`HeroInventoryItem`);
`ScriptCommandMap` GiveHero / HeroWear / SetHeroWeapon.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Frontend binds `CTCInventory` / gives items? | **No.** 2D UI only. | **DISPROVEN** |
| First inventory *name* after Leave? | Init Thing Components `004EE23F` → `004EECFE` `"CTCInventory"` | **PROVEN** |
| First equipment *name* after Leave? | Same walk, later: `CWeaponDef` `004F1C92` then `CCarryingDef` `004F1D48` | **PROVEN** |
| First compiled item / weapon *data*? | `00416005(1)` `game.bin`. `CInventoryItemDef` intern `004EF20A` / `009B0AC0`. CREATURE_HERO `CWeaponDef` idx **10526**, `CCarryingDef` **10527**. | **PROVEN** types. Live bag **DISPROVEN** |
| First `CTCInventory` *instance*? | Factory `004D2E74` → ctor `00590D32` (size `0x228`, vtbl `01250DE4`). No `E8` except the factory. First no-save callee **UNREAD**. | **PARTIAL** |
| Init GUI `PLAYER_GUI_PC` constructs the bag? | `0043A380` → `0043FF30` / `009ADA40` stores the **def** at `[0x13B878C]`. No `004C9D60("CTCInventory")`. | **DISPROVEN** as instance |
| Hero create adds inventory? | `006A9DD0` only `004C9D60("CTCPhysicsControlled")` + `0042B0A2`. | **DISPROVEN** |
| First `GiveHero` / `HeroWear` / `SetHeroWeapon` after Leave? | **None.** Runner `00CBFB7D` is not on the tree. | **PROVEN** skip |
| First leftover *named* give? | Oakvale bully scripts `GiveHero OBJECT_TEDDY_BEAR_UNGIVEABLE`. Not Leave. | **LEFTOVER** |
| C# `World.Inventory` after Leave? | Unused on `EnterGame` / first pumps. | **LEFTOVER** |

**Answer:** first-seen after Leave is **register + persist**,
not a bag slot, not a worn mesh, not `GiveHero`. First *Thing*
inventory GUI (`00590D32`) and first equipment *apply*
(`vtbl+484` / `+488` / `+760`) are later / unread on this spine.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  0042DF9E  type 0x22 frontend.big     // not CTCInventory
0042F2A2 Leave frontend
  0042EBB6 / 009BE420                  // fade / black Present
0042F491 Init Game → 00418DCA → 004184BD
  "Init Thing Components" 004EE23F     // FIRST inventory names
    004EECFE  CTCInventory      → 004D2EF0 / factory 004D2E74
    004EED6E  CTCInventoryClothing → factory 004E8967 / 004E7CC3
    004EEDEA  CTCInventoryWeapons
    004EEE5A  CTCInventoryAbilities
    004EEED6  CTCInventoryMagic
    004EEF46  CTCInventoryStats
    004EEFC2  CTCInventoryExperience
    004EF032  CTCInventoryTrade
    004EF0AE  CTCInventoryQuests
    004EF11E  CTCInventoryMap
    004EF19A  CTCInventoryItem  → factory 004DC5C7 / 004DAEA9 size 40
    004EF20A  CInventoryItemDef → 0044C6B0 + 009B0AC0
    … later same function …
    004F1C92  CWeaponDef        → 0042DAE0 / 009B0AC0
    004F1D48  CCarryingDef
  "Init Definition Manager" 00416005(1)
    game.bin persist                    // items + CREATURE_HERO slots
  … Init Graphics / Player / World …
  00416953 FinalAlbion.wld
    0049F180 Init Characters / Init GUI
      00449D90 PLAYER_HERO miss → CREATURE_HERO / 0048A070
      0043A380 PLAYER_GUI_PC def bind   // not 00590D32
      004B4260 START_INITIAL_QUESTS
    user.ini ActivateQuest("Gameflow")
      00CE7670 yield on Q_NewOakValeIntro miss
004189C2 first pumps
  no 00CBFB7D / no GiveHero / no HeroWear
later 00501450 Lookout
  006AC910 ConstructFromParams
    004C9D60("CTCPhysicsControlled")    // not CTCInventory
    CWeaponDef / CCarryingDef types on def  // apply UNREAD
```

---

## 1. Frontend / Leave — no bag

| Claim | Status |
|---|---|
| Frontend `0042EC7C` / type `0x22` binds inventory | **DISPROVEN** |
| Leave `0042F2A2` constructs `CTCInventory` | **DISPROVEN** (fade / clear / `FinalAlbion.wld` record) |
| `00594A73` is frontend menu inventory | **DISPROVEN** — in-game `CTCInventory` helpers; name vtbl `00594BCD` only pushes `"CTCInventory"` |
| `CInputProcessInventory*` exist | **PROVEN** RTTI. First no-save use **UNREAD**. Not frontend. |

---

## 2. First native bind after Leave — type register

`004184BD` → `004EE23F` (Init Thing Components). Pattern per CTC:

```
0099EBF0(name)
006869C0            // type id
004D2EF0(factory, name)
004D9D2F / 004E40C3 // table insert
```

| Name | Factory | Ctor | Size | Base |
|---|---|---|---|---|
| `CTCInventory` | `004D2E74` | `00590D32` | `0x228` | `005BFF07` `CTCInventoryBase@NInventory` vtbl `01253DBC` |
| `CTCInventoryClothing` | `004E8967` | `004E7CC3` | `0x170` | same `005BFF07`; vtbl `0124356C` |
| `CTCInventoryWeapons` | `004D2EBA` | `005C3947` | `0x1A0` | `005BFF07` |
| `CTCInventoryItem` | `004DC5C7` | `004DAEA9` | 40 | item component, not the bag |

`004D2EF0` is the CTC **type-name table** (`entity-task-queue`).
No I/O. **PROVEN** as first *name* use.

`CInventoryDef` / `CInventoryCategoryDef` / `CPlayerInventoryDef`
exist as RTTI only here. The `CPlayerInventoryDef` xref at
`004FF866` is **DISPROVEN** as a ctor (that site is
`call 00512FE0` inside villager populate `004FF750`). First
`CPlayerInventoryDef` persist **UNREAD**.

---

## 3. Compiled defs — Init Definition Manager

`00416005(1)` loads `game.bin` immediately after the type walk.

| Object | Role | Class |
|---|---|---|
| `CInventoryItemDef` | intern `004EF20A`; name setter `005B33A8` | **PROVEN** register. Per-item persist **UNREAD** |
| CREATURE_HERO `CWeaponDef` 10526 | strings `SWORD` / `weapon_pos_a` / `weapon_pos_b` | **PROVEN** type in palskin dump |
| CREATURE_HERO `CCarryingDef` 10527 | 7 u32 slots | **PARTIAL** |
| `NInventory::CItem` persist | `005BFD5E` keys `InventoryItems` / `SelectedInventoryItems` / `ConfiscatedItems2` | **PROVEN** names. First no-save call **UNREAD** (needs a live `CTCInventory`) |

This is not a hero bag. No `GiveHero`. No worn Graphic.

---

## 4. Init GUI is a def bind, not the bag

`0049F180` `"Init GUI"` → `[0x13B8790]` `0043A380`:

```
[0x13B878C]==0
  0099EBF0("PLAYER_GUI_PC")
  0044C6B0 / 0043FF30 → 009ADA40   // lookup compiled def
  store singleton [0x13B878C]
copy vectors +2044 into GUI +608 / +620
```

`0043FF30` is refcount + pointer store. **DISPROVEN** as
`00590D32`. Host `PlayerGuiReady` matches the *site*, not a
`CTCInventory` instance.

---

## 5. Hero Thing — equipment *types*, no inventory add

`006AC910` → `006A9DD0`:

```
00662880 parent
0042B0A2 appearance [thing+112]
004C9D60("CTCPhysicsControlled")
004C9CA0 activate
```

No `push "CTCInventory*"`. **PROVEN.**

CREATURE_HERO still carries `CWeaponDef` / `CCarryingDef` /
`CAppearanceDef` on the compiled def (`004CA010` bind). Socket /
mesh apply **UNREAD**. First Lookout frame is bind pose
(`FirstSeenPlaysAnim=false`). Clothing GUI `005B6881`
(`TEXT_GUI_MENU_CLOTHING_TOTAL_ARMOUR`) and `PC_UI_FRAME`
`005B8743` are the only `E8` callers of DEFAULT play `005B37F7`.
**DISPROVEN** as first after Leave.

`00489D40` CreateCharacter during Init Characters may run when
no player creature exists. It does **not** add inventory by name.
Whether it is the same `006AC910` as GuildArrivalHSP is already
covered by `player-bind-world` / palskin; either way the
ConstructFromParams inventory add is **DISPROVEN**.

---

## 6. Script give / wear — leftover vs Leave

| Verb | Token / apply | Native | First after Leave? |
|---|---|---|---|
| `GiveHero` | `00CC6392` / `00CC63E5` | `vtbl+484` × (count−have) | **DISPROVEN** |
| `SetHeroWeapon` | `00CCFD57` / `00CCFDA9` | `vtbl+488` | **DISPROVEN** |
| `HeroWear` | `00CC9222` / `00CC9274` | `vtbl+760` | **DISPROVEN** |
| `HeroHair` | `00CC9130` / `00CC9182` | `vtbl+764` | **DISPROVEN** |
| `PutInHeroHands` | `00CCFBCA` / `00CCFC20` | `vtbl+572` / `+568` | **DISPROVEN** |
| `TakeFromHero` | `00CCFB51` / `00CCFBA3` | `vtbl+556` | **DISPROVEN** |

`script-global-cmds`: Leave does not enter `00CBFB7D`.
First leftover named give in the bank dump is Oakvale
`GiveHero OBJECT_TEDDY_BEAR_UNGIVEABLE`. `HeroWear OBJECT_HERO_NO_HAT`
is `CS_DRAGON_OUTRO_EVIL`. **LEFTOVER.**

Worn-mesh trap (palskin): `OBJECT_HERO_*` Graphic **4126** is
`MESH_HERO_FOLDED_HAT_BANDITCAMP`, not hair. Real attach is
`CAppearanceModifierDef`. Do not bind 4126 as first equipment.

---

## 7. C# after Leave

| Site | What | Class |
|---|---|---|
| `ExecutionContext.Inventory` / `GiveHero` | name list + count; `vtbl+484` analog | **LEFTOVER** vs Leave (no runner) |
| `HeroClothes` / `HeroWeapon` / `HeroHairs` | name store only; mesh **not bound** | **LEFTOVER** vs native apply |
| `EngineLifecycle.EnterGame` / `SpawnHero` | no `CTCInventory`, no `CInventoryItemDef` | **PROVEN** absence |
| `HeroInventoryItem` | host DTO | **DIVERGE** vs `NInventory::CItem` persist |

Host must not invent a starting katana / teddy / young-hero outfit
on New Game.

---

## Classifications (short)

1. **Frontend / Leave inventory I/O — DISPROVEN.**
2. **First inventory name after Leave — `004EECFE` `CTCInventory` in `004EE23F`. PROVEN.**
3. **First equipment name after Leave — `004F1C92` `CWeaponDef` then `004F1D48` `CCarryingDef`. PROVEN.**
4. **First `game.bin` item/weapon persist — `00416005(1)`. PROVEN types. Bag contents UNREAD.**
5. **First `CTCInventory` instance / `GiveHero` / worn mesh — not on Leave / first pump. DISPROVEN as first-seen.**
6. **C# inventory after Leave — LEFTOVER / unused.**
