# `004EE23F` remaining `0044C6B0` / `009B0AC0` pairs after `CCreatureNavigationDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent a listing parser. Read
`listing-004c0000.txt` after `004EE932`.
Do **not** invent class names: only
`push "…"` listing strings in
`004EE932`…`004F9144`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: After `CCreatureNavigationDef`
`004EE932` (fifth `009B0AC0` on `004EE23F`),
list **every** remaining `0044C6B0` /
`009B0AC0` pair until `ret`. For each: class
name push, factory imm, CTC rows between.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
from `004EE932` to the function `ret`
(`e8` dest `009B0AC0` / `0044C6B0` in
range); sibling
`proofs/004EE23F-fourth-class`.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Function `ret`? | **`004F9144`**. Next insn is `004F9145 jmp 004F914A` (next fn). **No** `int3` pad on the boundary. Next `int3` pad is `004FA538` after later fns. | **PROVEN** |
| Remaining pairs after fifth `009B0AC0`? | **106.** `e8.tsv` / listing dest `009B0AC0` in this fn: 111 total; first five end at `004EE932`. Same 106 dest `0044C6B0` (`call` then `mov ecx, eax` then `009B0AC0`). | **PROVEN** |
| First remaining pair? | **`CInventoryItemDef`** `004EF20A` / `004EF23D` / `004EF244` / factory `0x44F644`. | **PROVEN** |
| Last remaining pair? | **`CHasNameDef`** `004F8E61` / `004F8E89` / `004F8E90` / factory `0x4D98C8`. | **PROVEN** |
| Invent names from `004Dxxxx` helpers? | **No.** After `CTCActionUseSearch` most CTC rows have **no** in-range `push "…"`. Count those as unnamed `004D2EF0` rows. | **PROVEN** method |
| Host leftover after fifth? | **Yes.** `AddFirstDefClass` Notes four classes through `CTimeAppearanceFadeDef`. Fifth + these 106 are **LEFTOVER**. | **PROVEN** leftover |
| This walk is Oakvale? | **No.** | **DISPROVEN** |

**Answer:** 106 remaining Add Def Class pairs
from `CInventoryItemDef` `004EF244` through
`CHasNameDef` `004F8E90`. Ordered table
below. Factory LoadDef / vtbl walk
**UNREAD**.

---

## 1. Fifth pair (already locked; not remaining)

`listing-004c0000.txt` / sibling
`004EE23F-fourth-class`:

```
004EE8F8  push "CCreatureNavigationDef"
004EE921  mov [ebp-1696], 0x4DA871
004EE92B  call 0044C6B0
004EE930  mov ecx, eax
004EE932  call 009B0AC0
```

---

## 2. Bounds: `004EE932` → `004F9144` `ret`

No invented scan. Listing after
`004EE932` continues the same CTC /
Add Def Class walk. Epilogue
(`004EE23F-thing-components`):

```
004F8E61  push "CHasNameDef"
004F8E71  push 0x4D98C8
004F8E83  call 0042DAE0
004F8E89  call 0044C6B0
004F8E90  call 009B0AC0
…
004F9129  call 0073B130
004F9139  call 004EBACE          ; if flag
004F9144  ret
004F9145  jmp 004F914A           ; next function; no int3
```

`e8.tsv` dest `009B0AC0` after
`0x004EE932`: first remaining
`0x004EF244`, last `0x004F8E90`.
Dest `0044C6B0`: first remaining
`0x004EF23D`, last `0x004F8E89`.
No further pair before `ret`.

Two factory-store shapes (same
`0044C6B0` / `009B0AC0` consume):

| Shape | Sites | Listing |
| --- | --- | --- |
| `mov [ebp-N], imm` then `0099EC30` record | first **3** remaining | `CInventoryItemDef` / `CLookDef` / `CReadableDef` |
| `push imm` then `0042DAE0` then getter | remaining **103** | `CVillageDef` … `CHasNameDef` |

`0042DAE0` is the later name+factory
pack helper. It is **not** Add Def
Class. Treating it as `009B0AC0` is
**DISPROVEN**.

---

## 3. CTC rows are not Add Def Class

A CTC row is a `004D2EF0` in this fn
(same block as siblings: `0099EBF0` /
helper, `006869C0`, `push factory`,
`004D2EF0`, `004D9D2F`, `004E40C3`).
No `0044C6B0`. No `009B0AC0`.

In-range `push "CTC…"` listing
strings exist from `004EE94E`
through `004EF7A8` (**30** strings).
After `CTCActionUseSearch` the listing
stops pushing CTC names; later rows
still `call 004D2EF0` (often after a
`004Dxxxx` helper). Those names are
**UNREAD** here. Do not copy helper
RTTI / other-fn `push` strings into
this table.

`CTCVolumeContainmentTrackerDef` is a
**Def** pair (`0044C6B0` /
`009B0AC0`), not a CTC row.

After `CHasNameDef` and before `ret`:
**6** more `004D2EF0` (`004F8ECE` …
`004F90E1`), no listing string, then
`0073B130`. Not a leftover pair.

Whole remaining range: **329**
`004D2EF0` after `004EE932` (340 in
this listing file minus 11 before the
fifth pair). Sum of “CTC between”
column + 6 tail = 329.

---

## 4. Named CTC before / between the first four remaining pairs

### 4.1 Between fifth pair and `CInventoryItemDef` — 19 CTC, all listing strings

| Push | Listing string | `004D2EF0` | factory `push` |
| --- | --- | --- | --- |
| `004EE94E` | `CTCPhysicsNavigator` | `004EE970` | `0x4D29E1` |
| `004EE9BE` | `CTCTargetingAI` | `004EE9E6` | `0x4E0047` |
| `004EEA3A` | `CTCTargetingPlayer` | `004EEA5C` | `0x4E3783` |
| `004EEAAA` | `CTCTargeted` | `004EEAD2` | `0x4DBF1D` |
| `004EEB26` | `CTCSpecialAbilities` | `004EEB48` | `0x4D2D98` |
| `004EEB96` | `CTCUndeadSoul` | `004EEBBE` | `0x4D7AE9` |
| `004EEC12` | `CTCSoundPlayer` | `004EEC34` | `0x4D30EE` |
| `004EEC82` | `CTCTalk` | `004EECAA` | `0x4D2E0A` |
| `004EECFE` | `CTCInventory` | `004EED20` | `0x4D2E74` |
| `004EED6E` | `CTCInventoryClothing` | `004EED96` | `0x4E8967` |
| `004EEDEA` | `CTCInventoryWeapons` | `004EEE0C` | `0x4D2EBA` |
| `004EEE5A` | `CTCInventoryAbilities` | `004EEE82` | `0x4D300F` |
| `004EEED6` | `CTCInventoryMagic` | `004EEEF8` | `0x4D2F46` |
| `004EEF46` | `CTCInventoryStats` | `004EEF6E` | `0x4D2FDC` |
| `004EEFC2` | `CTCInventoryExperience` | `004EEFE4` | `0x4D3042` |
| `004EF032` | `CTCInventoryTrade` | `004EF05A` | `0x4D3075` |
| `004EF0AE` | `CTCInventoryQuests` | `004EF0D0` | `0x4D30A8` |
| `004EF11E` | `CTCInventoryMap` | `004EF146` | `0x4D2F13` |
| `004EF19A` | `CTCInventoryItem` | `004EF1BC` | `0x4DC5C7` |

### 4.2 Between `CInventoryItemDef` and `CLookDef` — 2 CTC

| Push | Listing string | `004D2EF0` | factory |
| --- | --- | --- | --- |
| `004EF260` | `CTCCreatureExpression` | `004EF288` | `0x4D2AA4` |
| `004EF2DC` | `CTCLook` | `004EF2FE` | `0x4D38F3` |

### 4.3 Between `CLookDef` and `CReadableDef` — 4 CTC

| Push | Listing string | `004D2EF0` | factory |
| --- | --- | --- | --- |
| `004EF3A2` | `CTCActionUseTorch` | `004EF3CA` | `0x4DBF92` |
| `004EF41E` | `CTCActionUseScriptedHook` | `004EF440` | `0x4D2AD4` |
| `004EF48E` | `CTCActionUseSign` | `004EF4B6` | `0x4DBFCB` |
| `004EF50A` | `CTCActionUseReadable` | `004EF52C` | `0x4DC004` |

### 4.4 Between `CReadableDef` and `CVillageDef` — 28 CTC

First five have listing strings; next
23 have **no** in-range `push "…"`.

| Push | Listing string | `004D2EF0` | factory |
| --- | --- | --- | --- |
| `004EF5D0` | `CTCActionUsePickUp` | `004EF5F8` | `0x4DC065` |
| `004EF64C` | `CTCActionUsePickUpGenericBox` | `004EF66E` | `0x4DC09E` |
| `004EF6BC` | `CTCActionUsePutInInventory` | `004EF6E4` | `0x4DC0D7` |
| `004EF738` | `CTCActionUseMapTable` | `004EF75A` | `0x4DC110` |
| `004EF7A8` | `CTCActionUseSearch` | `004EF7D0` | `0x4DC149` |
| — | *(no listing string)* × 23 | `004EF840` … `004F0100` | **UNREAD** names |

---

## 5. Ordered remaining Add Def Class pairs

`n` is 6…111 on `004EE23F` (1…5 already
locked). CTC column = `004D2EF0` count
**between the previous pair and this
one**. After row 6 the later CTC names
are **UNREAD** except the strings in
§4.

Factory column is the listing immediate
(`mov [ebp-N]` or `push` before
`0042DAE0`). LoadDef body / vtbl
**UNREAD**.

| n | `push` | listing string | factory imm | `0044C6B0` | `009B0AC0` | CTC between | CTC names (listing only) |
| --: | --- | --- | --- | --- | --- | --: | --- |
| 6 | `004EF20A` | `CInventoryItemDef` | `0x44F644` | `004EF23D` | `004EF244` | 19 | §4.1 (19 strings) |
| 7 | `004EF34C` | `CLookDef` | `0x4D80E4` | `004EF37F` | `004EF386` | 2 | §4.2 |
| 8 | `004EF57A` | `CReadableDef` | `0x4DAA0E` | `004EF5AD` | `004EF5B4` | 4 | §4.3 |
| 9 | `004F0149` | `CVillageDef` | `0x4E213B` | `004F0171` | `004F0178` | 28 | §4.4 (5 strings + 23 unnamed) |
| 10 | `004F01FF` | `CVillageMemberDef` | `0x4DA7AD` | `004F0227` | `004F022E` | 1 | unnamed |
| 11 | `004F02B5` | `CBuyableHouseDef` | `0x4E0148` | `004F02DD` | `004F02E4` | 1 | unnamed |
| 12 | `004F036B` | `CBuyHouseDef` | `0x4D7B5B` | `004F0393` | `004F039A` | 1 | unnamed |
| 13 | `004F048C` | `CWifeDef` | `0x4D7BA1` | `004F04B4` | `004F04BB` | 2 | unnamed |
| 14 | `004F0618` | `CDoorDef` | `0x4D7BE7` | `004F0640` | `004F0647` | 3 | unnamed |
| 15 | `004F06CE` | `CLightDef` | `0x4D7C73` | `004F06F6` | `004F06FD` | 1 | unnamed |
| 16 | `004F0784` | `CSpotLightDef` | `0x4D7CB9` | `004F07AC` | `004F07B3` | 1 | unnamed |
| 17 | `004F083A` | `CClockDef` | `0x4E4477` | `004F0862` | `004F0869` | 1 | unnamed |
| 18 | `004F08F0` | `CHeroDef` | `0x4D7CFF` | `004F0918` | `004F091F` | 1 | unnamed |
| 19 | `004F0CFE` | `CCreatureModeDef` | `0x4E0B4B` | `004F0D26` | `004F0D2D` | 9 | unnamed |
| 20 | `004F0DB4` | `CPerceivedThingDef` | `0x4D7EB6` | `004F0DDC` | `004F0DE3` | 1 | unnamed |
| 21 | `004F0E6A` | `CBedDef` | `0x4DA7F3` | `004F0E92` | `004F0E99` | 1 | unnamed |
| 22 | `004F0F20` | `CStealthDef` | `0x4D7EFC` | `004F0F48` | `004F0F4F` | 1 | unnamed |
| 23 | `004F10AC` | `CTrophyDef` | `0x4D7F7B` | `004F10D4` | `004F10DB` | 3 | unnamed |
| 24 | `004F11CD` | `CCreatureGeneratorDef` | `0x4E0513` | `004F11F5` | `004F11FC` | 2 | unnamed |
| 25 | `004F1283` | `CChestDef` | `0x4D805C` | `004F12AB` | `004F12B2` | 1 | unnamed |
| 26 | `004F147A` | `CExplodingObjectDef` | `0x4D809E` | `004F14A2` | `004F14A9` | 4 | unnamed |
| 27 | `004F1530` | `CContainerRewardHeroDef` | `0x4E3C81` | `004F1558` | `004F155F` | 1 | unnamed |
| 28 | `004F1C92` | `CWeaponDef` | `0x4E3D15` | `004F1CBA` | `004F1CC1` | 17 | unnamed |
| 29 | `004F1D48` | `CCarryingDef` | `0x4DFE62` | `004F1D70` | `004F1D77` | 1 | unnamed |
| 30 | `004F1DFE` | `CCarryableDef` | `0x4DA767` | `004F1E26` | `004F1E2D` | 1 | unnamed |
| 31 | `004F1EB4` | `CEnemyDef` | `0x4D835A` | `004F1EDC` | `004F1EE3` | 1 | unnamed |
| 32 | `004F22BE` | `COpinionOfHeroDef` | `0x4D83D9` | `004F22E6` | `004F22ED` | 9 | unnamed |
| 33 | `004F244A` | `CShopDef` | `0x4E26BC` | `004F2472` | `004F2479` | 3 | unnamed |
| 34 | `004F256B` | `CStockItemDef` | `0x4D8482` | `004F2593` | `004F259A` | 2 | unnamed |
| 35 | `004F2621` | `CGiftDef` | `0x4D8547` | `004F2649` | `004F2650` | 1 | unnamed |
| 36 | `004F27A5` | `CHeroSuitDef` | `0x4E2809` | `004F27CD` | `004F27D4` | 3 | unnamed |
| 37 | `004F2C0E` | `CHeroExperienceDef` | `0x4EBAE7` | `004F2C36` | `004F2C3D` | 10 | unnamed |
| 38 | `004F2CC0` | `CExperienceDef` | `0x4E27AE` | `004F2CE8` | `004F2CEF` | 1 | unnamed |
| 39 | `004F2F8D` | `CReplaceableMeshDef` | `0x4E60D8` | `004F2FB5` | `004F2FBC` | 6 | unnamed |
| 40 | `004F3043` | `CMultiStaticMeshDef` | `0x4E31FA` | `004F306B` | `004F3072` | 1 | unnamed |
| 41 | `004F3310` | `CHeroCentreDef` | `0x4D86F0` | `004F3338` | `004F333F` | 6 | unnamed |
| 42 | `004F349C` | `CQuestCardDef` | `0x4E2333` | `004F34C4` | `004F34CB` | 3 | unnamed |
| 43 | `004F3552` | `CFlammableDef` | `0x4E3DC3` | `004F357A` | `004F3581` | 1 | unnamed |
| 44 | `004F3608` | `CBoastingPodiumDef` | `0x4D8736` | `004F3630` | `004F3637` | 1 | unnamed |
| 45 | `004F386A` | `CTCVolumeContainmentTrackerDef` | `0x4D94C8` | `004F3892` | `004F3899` | 5 | unnamed |
| 46 | `004F3E24` | `CThingDrainLifeShotDef` | `0x4D8D56` | `004F3E4C` | `004F3E53` | 13 | unnamed |
| 47 | `004F3EDA` | `CFireballSpellLevelDef` | `0x4D8D10` | `004F3F02` | `004F3F09` | 1 | unnamed |
| 48 | `004F40D1` | `CSkeletalMorphDef` | `0x4E3DD9` | `004F40F9` | `004F4100` | 4 | unnamed |
| 49 | `004F439E` | `CTrapDef` | `0x4E5CF2` | `004F43C6` | `004F43CD` | 6 | unnamed |
| 50 | `004F4600` | `CParticleAttacherDef` | `0x4E2AFA` | `004F4628` | `004F462F` | 5 | unnamed |
| 51 | `004F46B6` | `CAnimatingObjectDef` | `0x4EBA6E` | `004F46DE` | `004F46E5` | 1 | unnamed |
| 52 | `004F49EE` | `CExpressionSubDef` | `0x4D8818` | `004F4A16` | `004F4A1D` | 7 | unnamed |
| 53 | `004F4D91` | `CWillResponseDef` | `0x4D9629` | `004F4DB9` | `004F4DC0` | 8 | unnamed |
| 54 | `004F4F1D` | `CTurncoatDef` | `0x4E0F9C` | `004F4F45` | `004F4F4C` | 3 | unnamed |
| 55 | `004F4FD3` | `CSummonableCreatureDef` | `0x4D885E` | `004F4FFB` | `004F5002` | 1 | unnamed |
| 56 | `004F558D` | `CAIScratchpadDef` | `0x4D4E07` | `004F55B5` | `004F55BC` | 13 | unnamed |
| 57 | `004F5643` | `COccupiableDef` | `0x4D88FC` | `004F566B` | `004F5672` | 1 | unnamed |
| 58 | `004F56F9` | `CBossDef` | `0x4E0D4C` | `004F5721` | `004F5728` | 1 | unnamed |
| 59 | `004F58F0` | `CFishingDef` | `0x4E0DB9` | `004F5918` | `004F591F` | 4 | unnamed |
| 60 | `004F5A11` | `CGuardDef` | `0x4D89EC` | `004F5A39` | `004F5A40` | 2 | unnamed |
| 61 | `004F5AC7` | `CInterestingToVillagersDef` | `0x4D89B4` | `004F5AEF` | `004F5AF6` | 1 | unnamed |
| 62 | `004F5B7D` | `CActivateQuestDef` | `0x4D8A32` | `004F5BA5` | `004F5BAC` | 1 | unnamed |
| 63 | `004F5C9E` | `CCrateStackDef` | `0x4D8A6A` | `004F5CC6` | `004F5CCD` | 2 | unnamed |
| 64 | `004F5D54` | `COverheadDisplayDef` | `0x4D8AB0` | `004F5D7C` | `004F5D83` | 1 | unnamed |
| 65 | `004F5E0A` | `CTavernTableDef` | `0x4D8AF6` | `004F5E32` | `004F5E39` | 1 | unnamed |
| 66 | `004F6001` | `CTavernDef` | `0x4D8BE1` | `004F6029` | `004F6030` | 4 | unnamed |
| 67 | `004F60B7` | `CObjectAugmentationsDef` | `0x4EC526` | `004F60DF` | `004F60E6` | 1 | unnamed |
| 68 | `004F6384` | `CDrunkennessDef` | `0x4D8C91` | `004F63AC` | `004F63B3` | 6 | unnamed |
| 69 | `004F6792` | `CGoldDef` | `0x4D8EC5` | `004F67BA` | `004F67C1` | 9 | unnamed |
| 70 | `004F691E` | `CAICreatureWillPowerIndicatorDef` | `0x4D926A` | `004F6946` | `004F694D` | 3 | unnamed |
| 71 | `004F6969` | `CKickableDef` | `0x4D7C2D` | `004F6991` | `004F6998` | 0 | — |
| 72 | `004F69B4` | `CTavernGameDef` | `0x4E2D3B` | `004F69DC` | `004F69E3` | 0 | — |
| 73 | `004F69FF` | `CTavernGameCardBaseDef` | `0x4E2DB2` | `004F6A27` | `004F6A2E` | 0 | — |
| 74 | `004F6A4A` | `CTavernGameCoinBaseDef` | `0x4D8F51` | `004F6A72` | `004F6A79` | 0 | — |
| 75 | `004F6B00` | `CTavernGameShoveHaPennyDef` | `0x4E2D70` | `004F6B28` | `004F6B2F` | 1 | unnamed |
| 76 | `004F6BB6` | `CTavernGameCoinGolfDef` | `0x4D8F97` | `004F6BDE` | `004F6BE5` | 1 | unnamed |
| 77 | `004F6D42` | `CTavernGameSpotTheAdditionDef` | `0x4E11C3` | `004F6D6A` | `004F6D71` | 3 | unnamed |
| 78 | `004F6ECE` | `CDecapitationDef` | `0x4D9047` | `004F6EF6` | `004F6EFD` | 3 | unnamed |
| 79 | `004F6F84` | `CCoinGameObstacleDef` | `0x4D8F0B` | `004F6FAC` | `004F6FB3` | 1 | unnamed |
| 80 | `004F71C2` | `CWallMountEffectsDef` | `0x4D90C6` | `004F71EA` | `004F71F1` | 5 | unnamed |
| 81 | `004F726C` | `CFishDef` | `0x4D910C` | `004F7294` | `004F729B` | 1 | unnamed |
| 82 | `004F7316` | `CTeleporterDef` | `0x4D9152` | `004F733E` | `004F7345` | 1 | unnamed |
| 83 | `004F741B` | `CExplosionDef` | `0x4E3096` | `004F7443` | `004F744A` | 2 | unnamed |
| 84 | `004F75E2` | `CResurrectionItemDef` | `0x4D91DE` | `004F760A` | `004F7611` | 4 | unnamed |
| 85 | `004F768C` | `CKrakenDef` | `0x4E13AD` | `004F76B4` | `004F76BB` | 1 | unnamed |
| 86 | `004F7736` | `CKrakenTentacleDef` | `0x4D9224` | `004F775E` | `004F7765` | 1 | unnamed |
| 87 | `004F77E0` | `CHeroSpecialMovementDef` | `0x4D9198` | `004F7808` | `004F780F` | 1 | unnamed |
| 88 | `004F78E9` | `CIdleSchedulerDef` | `0x4E6232` | `004F7911` | `004F7918` | 2 | unnamed |
| 89 | `004F7993` | `CCarriedReadableDef` | `0x4D92B0` | `004F79BB` | `004F79C2` | 1 | unnamed |
| 90 | `004F7A3D` | `CJackOfBladesBattleDef` | `0x4E4748` | `004F7A65` | `004F7A6C` | 1 | unnamed |
| 91 | `004F7A88` | `CScorpionKingBattleDef` | `0x4E47BF` | `004F7AB0` | `004F7AB7` | 0 | — |
| 92 | `004F7AD3` | `CThunderBattleDef` | `0x4E4624` | `004F7AFB` | `004F7B02` | 0 | — |
| 93 | `004F7B1E` | `CWhisperBattleDef` | `0x4E4667` | `004F7B46` | `004F7B4D` | 0 | — |
| 94 | `004F7B69` | `CWaspQueenBattleDef` | `0x4E46A4` | `004F7B91` | `004F7B98` | 0 | — |
| 95 | `004F7BB4` | `CMazeBattleDef` | `0x4E45CE` | `004F7BDC` | `004F7BE3` | 0 | — |
| 96 | `004F7BFF` | `CTrollBattleDef` | `0x4E4833` | `004F7C27` | `004F7C2E` | 0 | — |
| 97 | `004F7C4A` | `CBalverineBattleDef` | `0x4E4883` | `004F7C72` | `004F7C79` | 0 | — |
| 98 | `004F7CF4` | `CAreaOfEffectAttackDef` | `0x4E6CF3` | `004F7D1C` | `004F7D23` | 1 | unnamed |
| 99 | `004F7D9E` | `CFishingRodDef` | `0x4D9321` | `004F7DC6` | `004F7DCD` | 1 | unnamed |
| 100 | `004F7F02` | `CRumbleDef` | `0x4E3290` | `004F7F2A` | `004F7F31` | 3 | unnamed |
| 101 | `004F81E2` | `CShipDef` | `0x4D8799` | `004F820A` | `004F8211` | 7 | unnamed |
| 102 | `004F822D` | `CShopItemDef` | `0x4D8411` | `004F8255` | `004F825C` | 0 | — |
| 103 | `004F8278` | `CSoundAtmospheresDef` | `0x4E32E3` | `004F82A0` | `004F82A7` | 0 | — |
| 104 | `004F83F8` | `CNymphDef` | `0x4D93A0` | `004F8420` | `004F8427` | 3 | unnamed |
| 105 | `004F84AE` | `CSummonDef` | `0x4D93E6` | `004F84D6` | `004F84DD` | 1 | unnamed |
| 106 | `004F85CF` | `CCameraCollisionDef` | `0x4D9465` | `004F85F7` | `004F85FE` | 2 | unnamed |
| 107 | `004F8972` | `CBettingDef` | `0x4D96DD` | `004F899A` | `004F89A1` | 8 | unnamed |
| 108 | `004F8B69` | `COracleMinigameDef` | `0x4D97E9` | `004F8B91` | `004F8B98` | 4 | unnamed |
| 109 | `004F8C1F` | `CFireheartMinigameDef` | `0x4D982F` | `004F8C47` | `004F8C4E` | 1 | unnamed |
| 110 | `004F8D40` | `CLightningOrbDef` | `0x4D9882` | `004F8D68` | `004F8D6F` | 2 | unnamed |
| 111 | `004F8E61` | `CHasNameDef` | `0x4D98C8` | `004F8E89` | `004F8E90` | 2 | unnamed |

After n=111: **6** unnamed `004D2EF0`
(`004F8ECE`, `004F8F39`, `004F8FA0`,
`004F900B`, `004F9076`, `004F90E1`).
No further `0044C6B0` / `009B0AC0`.

Zero-CTC clusters (adjacent Def
pairs, listing has no `004D2EF0`
between): `CKickableDef` …
`CTavernGameCoinBaseDef` (4);
`CScorpionKingBattleDef` …
`CBalverineBattleDef` (7);
`CShopItemDef` / `CSoundAtmospheresDef`
(2).

---

## 6. Host leftover

`EngineLifecycle.AddFirstDefClass`
Notes `CHeroMorphDef` /
`CHighlightItemDef` /
`CSmokeGeneratorDef` /
`CTimeAppearanceFadeDef` only.

No `CCreatureNavigationDef`. No
`CInventoryItemDef` … `CHasNameDef`.
No `0x44F644` / later factory imms.
Whole remaining `004EE23F` walk is
still leftover (`004EE23F-thing-components`).

| If host adds… | Leftover is… |
| --- | --- |
| first four Note-only (current) | fifth `CCreatureNavigationDef` `004EE932`, then these 106 |
| Note-only all 111 names | still live `009AD6E0` / `009FC4F0` on each object (**not** MATCH) |
| live Add Def Class for all 111 | next omit is `0073B130` / `004EBACE` tail |

---

## 7. Not Oakvale

No `00DBDE40` / region / TNG / hero
create on these pairs. Parent is
`004EE23F`. **DISPROVEN.**

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004EE932` / `004EE92B` | fifth Add Def Class `CCreatureNavigationDef` | **PROVEN** earlier; not remaining |
| `004EF244` / `004EF23D` | first remaining pair `CInventoryItemDef` | **PROVEN** leftover |
| `004F8E90` / `004F8E89` | last pair `CHasNameDef` | **PROVEN** leftover |
| 106 pair sites + factory imms + name pushes | table §5 | **PROVEN** |
| 30 CTC listing strings `004EE94E`…`004EF7A8` | §4 | **PROVEN** |
| later CTC `004D2EF0` counts | table CTC column | **PROVEN** count; names **UNREAD** |
| factory LoadDef / vtbl | each `0x4…` dest | **UNREAD** |
| `004F9144` `ret` | fn end | **PROVEN** |
| `004F9145` | next fn; no `int3` | **PROVEN** |
| `004FA538` | next `int3` pad (later fns) | **PROVEN** pad site; not this epilogue |
| `AddFirstDefClass` | four Notes only | **MATCH** first four; remaining **LEFTOVER** |
| `00DBDE40` | Oakvale | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`AddFirstDefClass`)
- `C:\FableCSharp\proofs\004EE23F-fourth-class\README.md`
- `C:\FableCSharp\proofs\004EE23F-thing-components\README.md`
