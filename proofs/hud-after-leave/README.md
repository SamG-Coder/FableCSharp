# First-seen HUD after Leave / Init GUI (no-save, no region)

Investigation only. Production `src/` was not edited.

Do **not** start Oakvale / `00DBDE40` / `Q_NewOakValeIntro` /
`CREATURE_HERO_CHILD` / Graphic **4300**. Childhood HUD is
after that activate, not this walk.

Question: after Leave `0042F2A2` and Init GUI `0043A380`,
what HUD is **resident** on no-save **before any region**?
Does first Present take overlay `00639E40` or interface
`0057B43F`?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: `listing-00400000.txt` `00435000` / `00435070` /
`0043A380` / `0043B570` / `00435530`;
`listing-00480000.txt` `0049F180` / `00487FB0`;
`e8.tsv` `00435058` → `00639E40`, `004350C4` → `0057B43F`;
`src/Fable.Game/EngineLifecycle.cs` `ApplyDisplayCamera`;
siblings `proofs/init-gui-0043A380`,
`proofs/audit-playerinterface`,
`proofs/dx9-3d-submit`,
`proofs/0049F180-first-children`;
`docs/status/investigations/A-dx9-submit.md`;
`docs/runtime/FORWARD_TREE.md` §11;
`EngineLifecycleTests.After_004AEA70_eq_1_00417001_is_00435F70_Present`.

---

## Verdict

**First-seen HUD objects: empty.**

No-save after Leave, before any region, submits **no**
overlay string, **no** interface type-`0x22` quad, **no**
`009DD8F0` dest, **no** MiniMap, **no** `HUD_ORB_*`.
`00435000` `00487DD0` / `00A01B50` miss → skip `00639E40`.
`00435070` `00487DC0` miss → skip `0057B43F`.
`009DA9F0(1)` `[+16020]==[+16024]` empty → `009DB6E6` no DIP.

A `PLAYER_GUI_PC` singleton and its meter pointers **exist**
(Create Players `0043B570`, then `0043A380` reset). They
are **not** first-seen draw objects. Childhood HUD is
**DISPROVEN** here: Oakvale is not activated.

| Question | Answer | Class |
|---|---|---|
| Frontend 2D tree still live? | **No.** Leave `0042F2A2` / `0042EBB6` tore it down | **PROVEN** |
| Init GUI builds HUD widgets? | **No.** `0043A380` reset/recopy only | **DISPROVEN** |
| Overlay `00639E40` on first Present? | `00487DD0` miss → skip | **PROVEN** |
| Interface `0057B43F` on first Present? | `00487DC0` miss → skip | **PROVEN** |
| `009DA9F0` dest nonempty? | **No.** empty, no DIP | **PROVEN** |
| Type-`0x22` HUD sprites? | later `0043B050` / `0069ECD0` | **DISPROVEN** here |
| MiniMap `0082BA00`? | `SetRegionAsLoaded` after first region | **DISPROVEN** here |
| Childhood / `HUD_ORB_*` / kid 4300? | after Oakvale activate | **DISPROVEN** here |
| First-seen HUD objects (submit)? | **empty** | **PROVEN** |

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend            // PRESS_START / type 10 gone
0042F491  Init Game
  004473A0  Player Interface        // input, not HUD draw
  004166A8  Create Players
    0048A210 → 00487FB0
      0043B570 PLAYER_GUI_PC        // singleton + meters
      004195AF [0x13B8790] = gui
  00416953  FinalAlbion.wld
    00416BCA  0049F180(ecx=world, 0)
      00449970 / 00487DC0 miss      // no player Thing
      "Init GUI" 0043A380           // reset, not ctor
      "Init Quests" 004B4260        // no Oakvale activate
later WorldFrame>1  004AEA70=1
  00435F70 jmp 00435530
    00435000  00487DD0 miss skip 00639E40
    00435070  00487DC0 miss skip 0057B43F
    009D9C80 / 009DA9F0(1) empty dest
    no 00501450 / no region
```

`EngineLifecycle.ApplyDisplayCamera` Notes match those
skips (`00435000 skip 00639E40`, `00435070 skip 0057B43F`).
Pairing `00B25950` / ScenePass bits onto this body is
**DISPROVEN** (`dx9-3d-submit`, PARITY first Present dest).

---

## 1. Resident after Leave, before any region

Not frontend widgets. Leave already ended `0042DF9E`.

| Object | Site | First-seen draw? |
|---|---|---|
| `PLAYER_GUI_PC` `0x338` vtbl `0123177C` `[0x13B8790]` | Create Players `0043B570` / `004195AF` | **No** |
| Def `[0x13B878C]` | same ctor (`009ADA40`) | **No** |
| Meters `0065431D` at GUI `+716`…`+740`; `00654392` `+748` | same ctor | **No** (`+8=0` after reset) |
| Extra bars `006543AF` `+756` / `006543FF` `+764` | same ctor | **No** |
| Vectors `+608` / `+620` | ctor then `0043A380` `00442770` recopy | **No** |
| `PlayerInterface` `004473A0` `game+32` | Init Game named stage | **No** (input pump) |
| Type-`0x22` (`0041BEB0`) | later `0043B050` | **absent** |
| MiniMap `00437CE0` / `0082BA00` | after first region | **absent** |
| Overlay / interface dest | first Present skip | **absent** |

`0043A380` (`init-gui-0043A380`):

```
0043A38D  call 00492BAB          // reset this+24
0043A398  call 00647319          // clear +456
          [+716]+8 … [+748]+8 = 0
0043A3D1  jne 0043A40F           // def already bound
          00442770 +608 / +620
          [this+424]+48 = 0
          [this+657] = 1
```

No `00BFEA1A`. No `0041BEB0`. No `0065431D`. **PROVEN.**

Host `PlayerGuiReady=true` after a `Note(0043A380)` is
**LEFTOVER** (`issue-17-verify`). It is not a dest.

---

## 2. Display overlay miss (`00435000`)

```
00435000  mov ecx, [ecx+12]
00435004  call 00449960
0043500B  call 00487DD0          // +44 jmp 00A01B50
00435010  test eax, eax
00435012  je  0043505E           // miss → ret
          [eax+145] bit0 / [eax+48] 0x4000
00435058  call 00639E40          // only if Thing + gates
```

No-save first Present: no player Thing (`00A01B50` 0).
Same miss as Init Characters. **PROVEN skip.**

`00639E40` is text (`005BCAFE`). Not a HUD quad.

Host:

```
Note(00449960, "00435000 00449960");
Note(00487DD0, "00487DD0 +44 jmp 00A01B50 miss");
Note(00435000, "00435000 skip 00639E40");
```

**MATCH** the listing skip. Inventing an always-`00639E40`
call is **DISPROVEN**.

---

## 3. Display interface skip (`00435070` / `0057B43F`)

```
00435070  mov eax, [0x13B86A0]
00435079  mov ecx, [eax+28]
0043507C  call 00449970
00435083  call 00487DC0
00435088  test eax, eax
0043508A  je  004350C9           // miss → ret
          test [eax+32], 0x10
          slot 4 on +68 (0040F020)
004350C4  call 0057B43F          // only if Thing + slot
```

`e8.tsv`: only site of `0057B43F` from this fn is
`004350C4`. First-seen `00487DC0` is 0. **PROVEN skip.**

`0057B43F` would pack type `0x22` via `0041BEB0` then
vtbl+92 (`00B23BC0`). That rec is **not** built here.

Host:

```
Note(00487DC0, "00435070 00487DC0 miss");
Note(00435070, "00435070 skip 0057B43F");
```

**MATCH.** `DisplaySubmitStages` lists the **call sites**
(`PlayerOverlay` / `PlayerInterface`), not a taken apply.

`009DD8F0` gates on `00435530` stay closed (`dx9-3d-submit`).
First nonempty `+16020` is **UNREAD** later; first-seen
empty is **PROVEN**.

---

## 4. Childhood HUD after Oakvale activate

**Not this walk.**

| Claim | Status |
|---|---|
| No-save `0049F180` activates `Q_NewOakValeIntro` | **DISPROVEN** (`No_save_does_not_activate_Q_NewOakValeIntro`) |
| First hero name is `CREATURE_HERO_CHILD` / 4300 | **DISPROVEN** (`00449D90` immediate `"CREATURE_HERO"`; first create still miss) |
| `HUD_ORB_QUEST_CORE` / guild TEXT_QST on first Present | **DISPROVEN** (Gameflow `+72=0` yield; `dialogue-first`) |
| MiniMap / region HUD | **DISPROVEN** before `00501450` / `SetRegionAsLoaded` |

Childhood HUD / kid meters / Oakvale intro chrome belong
to a **later** `00DBDE40` / `S_QNOVI` take. Do not bind
them at `0043A380` or first `00435530`.

---

## 5. Host leftover

| Site | What | Class |
|---|---|---|
| `ApplyDisplayCamera` skip Notes | match listing | **MATCH** |
| `ApplyDisplayCamera` + `009DA9F0` as ScenePass bits | 2D dest is empty; 3D is `00B27D90` | **DISPROVEN** pairing |
| `00B25950` noted on `00435530` | no `E8` | **DISPROVEN** |
| `PlayerGuiReady` without `[0x13B8790]` / meters | flag only | **LEFTOVER** |
| `CreatePlayers` no `0043B570` | native ctor missing | **LEFTOVER** |
| Client Present world/AVI only | no HUD dest | **MATCH** first-seen empty |

---

## Open

| Item | Class |
|---|---|
| First later take of `00639E40` / `0057B43F` after a live Thing | **UNREAD** here |
| First live `0069ECD0` → `0043B050` type `0x22` | **UNREAD** |
| Which `009DD8F0` gate opens first | **UNREAD** (`A-dx9-submit`) |
| Childhood HUD object list on Oakvale activate | **UNREAD** (not this walk) |

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `0042F2A2` | Leave; frontend 2D gone | **PROVEN** |
| `0043B570` | first `PLAYER_GUI_PC` + meters | **PROVEN** construct; **not** first-seen draw |
| `0043A380` | Init GUI reset | **PROVEN** no widgets |
| `00435000` | overlay lookup | **PROVEN** miss / skip `00639E40` |
| `00435070` | interface lookup | **PROVEN** miss / skip `0057B43F` |
| `0057B43F` | type `0x22` apply | **DISPROVEN** first-seen |
| `00639E40` | overlay text | **DISPROVEN** first-seen |
| `009DA9F0` | 2D dest | **PROVEN** empty |
| `00DBDE40` | Oakvale / childhood HUD feeder | **DISPROVEN** here |

**Answer: empty.**
