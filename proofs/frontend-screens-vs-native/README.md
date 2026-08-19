# Frontend screens vs native dest — not a Vulkan defect

CPU blit of the same dest/UV records the client
submits as `FrontendBatch` (Vulkan Present
`009BEEB0`). If this blit is wrong, dest /
visibility is wrong; Vulkan is not the cause.

| Screen | After AVI skip | Dest vs `0041AFA0` | CPU blit |
| --- | --- | --- | --- |
| Press Start | Type-6 point `512,384`; title `112,48`–`522,253`; forest 410 lattice | **MATCH** | Correct TLC title |
| New Profile | Coastal tiles 410 lattice; type-6 leftover204=0 | dest math **MATCH** first-style leftover | Coastal + sunbeam_1 overlay **MATCH** two type-18 child-0 groups. `WASD`/`INVERTED`/`SUNBEAM_2` hidden. `ARROWS`+`NORMAL` still share dest `544,245` because both type-16 parents land at `448,245` (table-row dest leftover, not type-6 width). Type 38 ON+OFF same dest. |
| Main Menu | New Game msg 15; title dest same as Press Start | leftover **MATCH** GraphicIndex | CPU blit shows New Profile on top: `ResidentSlotTrees` order is slot `0` then `0x14` then `0x17`. Native also walks every `[ui+84]`. `SelectState(6)` on the old current is **not** `Visible=false` (**DISPROVEN**). |

Client LMB: down `Type4` / up `Type6`. Return
is PlayAVI skip only, not Press Start accept
(**DISPROVEN** as accept). Sequence Type4+Type6
is native `0xE5` → `0x126` → 15.

Clip persist CRC for CUIDef `+392` is **UNREAD**;
do not invent `FableCrc("Clip")`.
