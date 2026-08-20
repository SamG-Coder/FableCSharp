# `CPlayerGuiDef+824` persist CRC `0x2D7A6960`; name UNREAD

Investigation only. No production `src/` / `tests/` edits.

Do **not** invent `HUD_ORB_*` as this
dword's Lionhead name. Do **not** treat
instance size `0x338` as this field.
Do **not** treat first Present as drawing
it (`PlayerGuiDefPlus338IsHud=false`).
Do **not** start Oakvale / `00DBDE40`.

Question: sibling `player-gui-pc-338`
left the file CRC / Lionhead name of
persist `+824` (`0x338`) **UNREAD**
because a nested `00466A47` vector at
`+336` sits earlier. Recover CRC/name
if listing + `game.bin` tags allow.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Authority: dump `listing-00440000.txt`
`004736C4` / `0047398D` / `00473E87` /
`00466A47` / `00466AA7` / `00431102` /
`004568BC` / `00995030`;
`listing-00980000.txt` `00995030`;
`game.bin` `#7075` `PLAYER_GUI_PC`
`#7074` `PLAYER_GUI_DEFAULT` `#141`
`NULLDEF_PLAYER_GUI`;
`Fable.Dump tex` bank ids 5900–5906;
`names.bin`; `strings.tsv`;
`FableCrc` (`0xEDB88320`, init 0);
host `EngineLifecycle.PlayerGuiDefPlus338`
/ `PlayerGuiDefPlus338IsHud` /
`PlayerGuiObjectSize`;
sibling `proofs/player-gui-pc-338`.

Listing displacements are **decimal**
(`[esi+824]` = `0x338`).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| File CRC at def `+824`? | **`0x2D7A6960`** little-endian `60 69 7A 2D` at `PLAYER_GUI_PC` file `0x0589` | **PROVEN** |
| Lionhead field name? | no `FableCrc` hit in `names.bin`, PE strings, or guessed stems | **UNREAD** |
| Helper / type? | `004736C4` `lea eax,[esi+824]` `call 00431102` i32 | **PROVEN** |
| First-seen payload (PC / DEFAULT)? | i32 **5902** (`0x170E`) | **PROVEN** |
| `NULLDEF_PLAYER_GUI` payload? | **0** | **PROVEN** |
| Bank id 5902 name? | `HUD_MODES_CREEP_ON` (`Fable.Dump tex 5902`) | **PROVEN** as **payload**, not field name |
| Field name `HUD_ORB_*`? | `FableCrc("HUD_ORB_QUEST_CORE")=0x368DE1FB` ≠ `0x2D7A6960` | **DISPROVEN** |
| Field CRC `GraphicIndex`? | `FableCrc("GraphicIndex")=0x38E36902` | **DISPROVEN** as this CRC |
| Nested `00466A47` at `+336` walkable? | yes: CRC skip + `i32` count + `count*4`; PC `n=10` | **PROVEN** |
| Instance `CPlayerGui` size `0x338`? | different object (`00487FC3 push 0x338`) | **PROVEN** |
| First Present HUD dest? | still empty skip; host `PlayerGuiDefPlus338IsHud=false` | **MATCH** |

**Answer:** CRC is **`0x2D7A6960`**. Lionhead
name stays **UNREAD**. Payload is graphic
**5902** `HUD_MODES_CREEP_ON`, not
`HUD_ORB_*`.

---

## Verdict

Walking `004736C4` in persist order,
including the `+336` dword vector,
lands on `+824` at packed file
`0x0589`:

```
60 69 7A 2D  0E 17 00 00
CRC 0x2D7A6960   i32 5902
```

Same CRC and 5902 on
`PLAYER_GUI_DEFAULT`. `NULLDEF` keeps
the CRC and stores **0**.

`names.bin` has **no** stored hash
`0x2D7A6960`. `FableCrc` of PE
`strings.tsv` identifiers and of
`HUD_ORB_*` / `GraphicIndex` /
Health/Will/Orb stems does **not**
hit. Do not label the field
`HUD_ORB_*`.

The i32 is a textures.big **id**. The
seven-dword cluster `+808`…`+832` is
the `HUD_MODES_*` pair set (border,
target off/on, creep off/on, safe
off/on). That is payload meaning
**PARTIAL** (id lookup **PROVEN**;
which HUD-mode apply reads `+824`
is **UNREAD** here). First Present
after Leave still does not DIP this
graphic (`hud-after-leave`).

Host:

| Constant | Value |
|---|---|
| `PlayerGuiDefPlus338` | **824** |
| `PlayerGuiDefPlus338IsHud` | **false** |
| `PlayerGuiObjectSize` | **`0x338`** (instance) |

---

## 1. Walk through `00466A47` `+336`

`004736C4` field 53:

```
0047398D  lea  eax, [esi+336]
          push eax
          mov  ecx, edi
          call 00466A47
```

`00466A47` `00404500` skips 4-byte CRC,
mode 2 (`[esi+24]==2`) `00466AA7`:

```
read i32 count          ; 4 bytes
004428F0 resize dest
loop count:
  0040FE60 → dest[i]    ; 4 bytes each
```

File after CRC: `count` + `count*4`.
PC/DEFAULT `n=10` → payload **44**
(CRC `0x24699BBE` at file `0x01ED`).
`NULLDEF` `n=0` → payload **4**.

Other helpers before `+824` (field
index 144) are fixed-size once that
vector is consumed:

| Helper | File after CRC |
|---|---|
| `00431102` / `00431061` / `004595A3` | 4 |
| `004568BC` (`00995030` two dwords) | 8 |
| `00456903` (three floats) | 12 |
| `00464CBF` float vector | `4+n*4` |
| `00475D99` 8-byte records | `4+n*8` |

Packed start: 3-byte hdr `01 00 01`
then `u16 0` (same skip as
`FrontendUiDef.TryParse`), then CRC
tags. Walk of all three `PLAYER_GUI`
rows reaches `+832` without truncating.

---

## 2. `+824` tag

`00473E87`:

```
00473E87  lea  eax, [esi+824]         ; +0x338
          push eax
          mov  ecx, edi
          call 00431102
```

`PLAYER_GUI_PC` file `0x0589`:

| Bytes | Meaning |
|---|---|
| `60 69 7A 2D` | CRC **`0x2D7A6960`** |
| `0E 17 00 00` | i32 **5902** |

Cluster (PC / DEFAULT; NULLDEF all 0):

| Off | CRC | i32 | `tex` name |
|---|---|---|---|
| `+808` | `0x17D044F0` | 5900 | `HUD_MODES_BORDER` |
| `+812` | `0x765949FC` | 5905 | `HUD_MODES_TARGET_OFF` |
| `+816` | `0x4F8E249F` | 5906 | `HUD_MODES_TARGET_ON` |
| `+820` | `0x5B39523C` | 5901 | `HUD_MODES_CREEP_OFF` |
| **`+824`** | **`0x2D7A6960`** | **5902** | **`HUD_MODES_CREEP_ON`** |
| `+828` | `0x6701F9D8` | 5903 | `HUD_MODES_SAFE_OFF` |
| `+832` | `0x16355C44` | 5904 | `HUD_MODES_SAFE_ON` |

Those names are **bank entry** names
for the payload ids, not persist field
CRCs. `HUD_ORB_*` CStrings still sit
later in the 18148-byte blob
(`player-gui-pc-338`).

---

## 3. Name search (negative)

| Probe | Result |
|---|---|
| `names.bin` stored hash `0x2D7A6960` | none |
| `FableCrc` of every `names.bin` string | miss |
| `FableCrc` of `strings.tsv` text | miss |
| `FableCrc("GraphicIndex")` | `0x38E36902` |
| `FableCrc("HUD_ORB_QUEST_CORE")` | `0x368DE1FB` |
| Health / Will / Orb / Graphic / Sprite stems | miss |
| PE `68 60 69 7A 2D` / `0x2D7A6960` | none (CRC lives in the file only) |

Lionhead spelling of CRC `0x2D7A6960`
is **UNREAD**. Inventing `HUD_ORB_*`
or `GraphicIndex` as the **field**
name is **DISPROVEN**.

---

## 4. Host leftover

`PlayerGuiDefPlus338=824` and
`PlayerGuiObjectSize=0x338` **MATCH**
the two different `0x338`s.
`PlayerGuiDefPlus338IsHud=false`
**MATCH**es first Present empty skip.
Host still does not store or bind
`0x2D7A6960` / 5902. Live def field
**LEFTOVER**.

---

## Classification

| Item | Class |
|---|---|
| CRC `0x2D7A6960` at def `+824` | **PROVEN** |
| Payload 5902 on PC/DEFAULT | **PROVEN** |
| `tex 5902` = `HUD_MODES_CREEP_ON` | **PROVEN** id→name |
| Lionhead field name | **UNREAD** |
| `HUD_ORB_*` as this CRC | **DISPROVEN** |
| `00466A47` `+336` blocks the walk | **DISPROVEN** (walked) |
| First Present draws this graphic | **DISPROVEN** |

---

## Open

| Item | Class |
|---|---|
| English name whose `FableCrc` is `0x2D7A6960` | **UNREAD** |
| First `0043xxxx` reader of `[def+824]` | **UNREAD** |
| Which HUD-mode apply samples 5902 | **UNREAD** here |

---

## Sources

- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\assembly\compiled-defs\names.tsv`
- `C:\FableCSharp\src\Fable.Formats\Defs\FableCrc.cs`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\player-gui-pc-338\README.md`
- `C:\FableCSharp\proofs\hud-after-leave\README.md`
