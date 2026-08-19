# Frontend screens vs native dest — not a Vulkan defect

CPU blit of the same dest/UV records the client
submits as `FrontendBatch` (Vulkan Present
`009BEEB0`). If this blit is wrong, dest /
visibility is wrong; Vulkan is not the cause.

| Screen | After AVI skip | Dest vs `0041AFA0` | CPU blit |
| --- | --- | --- | --- |
| Press Start | Type-6 point `512,384`; title `112,48`–`522,253`; forest 410 lattice | **MATCH** | Correct TLC title |
| New Profile | Coastal tiles 410 lattice; type-6 leftover204=0 | dest math **MATCH** first-style leftover | Stacked type-18 swap layers + overlapping text |
| Main Menu | New Game msg 15; title dest same as Press Start | leftover **MATCH** GraphicIndex | Same stacked-layer leftover |

Client LMB: down `Type4` / up `Type6`. Return
is PlayAVI skip only, not Press Start accept
(**DISPROVEN** as accept). Sequence Type4+Type6
is native `0xE5` → `0x126` → 15.

Clip persist CRC for CUIDef `+392` is **UNREAD**;
do not invent `FableCrc("Clip")`.
