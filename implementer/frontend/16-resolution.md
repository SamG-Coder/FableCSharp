# 16 — 640×480 persist → 1024×768 dest (`0052E580`)

Authority: `Fable.exe` persist `00631C60` / `005331A0` / `0052E580` +
`frontend.bin` PRESS_START walk. No visual tile-size guesses.

## Verdict

Per-widget remap bits are **not** all 1. Dest staying at persist
`70,30` on a 1024 buffer was wrong because the **type-10 root**
sets def `+520=1`. Children inherit dest scale `+272 = parent+264`.

| Widget | Type | def+520 remap size | def+521 remap origin | Remap applies? |
|---|---|---|---|---|
| `UI_FRONTEND_PRESS_START_MENU` | 10 | **1** | 0 | size only (`0052E580` on persist scale 1 → dest scale 1.6) |
| `UI_TITLE` | 5 | 0 | 0 | inherit root 1.6 → origin `70*1.6=112`, `30*1.6=48` |
| `UI_TITLE_01` | 0 | 0 | 0 | inherit TITLE 1.6 |
| `UI_TITLE_02` | 0 | 0 | 0 | inherit TITLE 1.6; persist X=256 → `256*1.6+112` |
| `UI_BLENDING_BACKGROUNDS_FORREST` | 5 | 0 | 0 | inherit root 1.6 |
| `UI_FRONTEND_BG_FORREST_1_1` | 0 | 0 | 0 | inherit forest 1.6 |
| `UI_PRESS_START_TEXT` | 6 | 0 | 0 | inherit root 1.6 → `320*1.6=512`, `240*1.6=384` |
| `UI_MOUSE_POINTER` | 32 | 0 | 0 | inherit root 1.6; `+191=1` absolute |
| `UI_PRESS_START_SWAP` | 18 | 0 | 0 | inherit root 1.6 |
| `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 | 0 | 0 | inherit root 1.6 |
| `UI_LEGAL_TEXT` | 6 | 0 | 0 | inherit root 1.6 |

CRC (name UNREAD): size `0xC50CA371`, origin `0xB466D948`.
`0043314A` / `00403EB0` `setne`: nonzero file byte → 1.

Type-10 root persist `Width=0` still submits dest `0,0,0,0`. Parent
dest **origin** is 0, so children combine at persist pos × inherited
dest scale.

## Native path (PROVEN)

```
0041E3F6 / 004299A8
  [0x13B8768]=1
  [0x13B876C/70]=1024/768

005331A0
  +302 bit 6 ← def+520   vtbl+464 0052F3A0
  +302 bit 7 ← def+521   vtbl+468 0052F3B0

0052E580 when [0x13B8768]:
  x' = x / [0x1375CD4=640] * [0x13B876C]
  y' = y / [0x1375CD8=480] * [0x13B8770]

0052F5C0 dest scale +264:
  +264 = (remapSize ? 0052E580(+92) : +92) * +272

0052FFD0 dest origin +248:
  +248 = remapOrigin ? 0052E580(+52) : +52
  +248 = +248 * +272 + +256     // unless absolute

00531EC0 child inherit:
  child+272 = parent+264
  child+256 = parent dest origin
```

PRESS_START first-seen: persist scale `+92=1`, root
`remapSize=1` → `+264 = (1/640*1024, 1/480*768) = (1.6, 1.6)`.
Child `remapOrigin=0` → origin = persistPos × 1.6 + 0.

## Persist walk that copied the bytes

CUIDef persist `00631C60`. Style persist `00625630` (`vtbl+72`
`0x0125871C`). After first style prefix, `0x56A59976` is style
`+120` **u8**, then `+64` i32 (default 7), then `+108` i32 vec
(count 0). CUIDef tail then includes:

| Off | CRC | Type | PRESS_START value |
|---|---|---|---|
| +188 | `0x64D3430E` | u8 | 0 (centre) |
| +191 | `0x38BBD87F` | u8 | 0 (mouse=1 absolute) |
| +504 | `0x2CB06C8E` | u8 | 0 |
| +512 | `0x7084E2DD` | u8 | 0 |
| +520 | `0xC50CA371` | u8 | **1** root, **0** children |
| +521 | `0xB466D948` | u8 | **0** all |

Ctor `006300BC` / `00630145` zeros `+188/+504`; does not write
`+520/+521` (calloc 0). File persist overwrites.

## What C# must do

Generic: parse def+520/+521 into `FrontendUiDef` and pass
`ScaleSizeToViewport` / `ScaleOriginToViewport` into
`FrontendLayout.Compute`. Not a Press Start hack.

Root `ScaleSize=1` is enough for first-seen 1024×768. Applying
`pos*1024/640` on every widget would be invented; the inherit
path is the native one.

## TITLE dest after remap

```
70 / 640 * 1024 = 112
30 / 480 * 768  = 48
```
