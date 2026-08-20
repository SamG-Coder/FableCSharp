# `0042F491` / `004184BD` child order vs host `EnterGame` / `LoadWorld`

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `StartOakVale` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` (`0042F44D`), not a region.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `listing-00400000.txt` (`0042F491` /
`0042F2A2` / `004184BD` / `00418DCA` / `0041735A` /
`00417418` / `00416953` / `00416832` / `004168DC` /
`00416C8A` / `0041732A`);
`listing-00440000.txt` (`0044C6B6` / `0044C6B0` /
`0044C6C2` / `0044C71F`);
`listing-00480000.txt` (`004A6E30` / `004A67D0`);
`functions.tsv` callee list on `004184BD`;
`docs/runtime/FORWARD_TREE.md` §§4–7, 10;
`src/Fable.Game/EngineLifecycle.cs` (`EnterGame` /
`LoadWorld` / `InitGameStages` / `InitWorldInitStages` /
`InitWorldCameras` / `CreatePlayers` /
`FinishInitGameAfterWorld`);
`EngineLifecycleTests.New_game_is_leave_frontend_then_FinalAlbion_wld`,
`LoadWorld_00416953_no_save_is_004A1840_then_0049F180`,
`InitGame_004184BD_after_00416953_reserves_then_user_ini`.

Siblings: `proofs/audit-lifecycle-newgame`,
`proofs/init-world-004A6E30`, `proofs/0041E6D3-frontend-gate`.

---

## Verdict

**First DIVERGE: host runs `004A67D0` / `004A6E30`
after `"Load Particles"` `004174F1`, as a sibling of
`LoadWorld()`. Native runs them inside `"Init World"`
`0041735A`, before `"Init Display Engine"` `00417418`.**

Named `InitGameStages` **notes** match `004184BD`
string order (**MATCH**). The **work** those names
own does not. `00417418` reads `[game+36]` (world)
and `[world+60]` (mesh bank from `004A6E30`). Host
`00434E10` therefore runs with no world object.

| Claim | Class |
|---|---|
| After Leave, `0042F491` → `00418DCA` → `[vtbl+4]` `004184BD` | **PROVEN** / host **MATCH** |
| `004184BD` size is `378` (`functions.tsv`) | **DISPROVEN** — body is `004184BD`…`004189C1` |
| Twelve named stages in `InitGameStages` match listing strings | **PROVEN** **MATCH** (notes) |
| `004A67D0` / `004A6E30` are children of `0041735A`, not of `004184BD` after particles | **PROVEN** native |
| Host `EnterGame` constructs world after the named loop | **DIVERGE** — **first reorder** |
| `00417418` before `004A67D0` on host | **DIVERGE** |
| `CreatePlayers()` after `InitWorldCameras`, not at the `"Create Players"` note | **DIVERGE** |
| `LoadWorld()` after that late world ctor (native: after particles, world already live) | **DIVERGE** (same hoist) |
| `0044C6B6` → first-seen `0044C6C2`/`0044C71F` before Thing Components | **PARTIAL** (omitted; earlier than the reorder) |
| `004168DC` `"Init Fonts"` after `00416C8A` | **PARTIAL** |
| `00419D90` noted before `0041735A` | **DIVERGE** (hoist; native `004A712B`) |
| `00418EC6` `[+90593]=1` after `LoadWorld` | **DIVERGE** (native ctor, before `004184BD`) |
| Oakvale / `00DBDE40` on this walk | **DISPROVEN** |

**Answer:** first *order* DIVERGE vs `EnterGame` /
`LoadWorld` is **`004A67D0`/`004A6E30` late** (after
`004174F1`, before `00416953`) instead of **inside
`0041735A`** (before `00417418`). First *omitted*
`004184BD` child is earlier: **`0044C6B6`**.

---

## 1. Parent: Leave then `0042F491`

`listing-00400000.txt`:

```
0042F48A  call 0042EBB6          ; Leave teardown + Present
0042F48F  push -1
0042F491  push "Init Game"
0042F4B1  push 0x161E8
0042F4C7  call 00418DCA          ; vtbl 0122F180, [+90593]=1 at 00418EC6
0042F4D2  call [eax+4]           ; 004184BD
0042F4DD  mov [0x13B7D58], esi   ; old retail
```

Host `EnterGame` notes `0042F491` / `00418DCA` /
`004184BD` then the `004184BD` children. Stage gate
`LeaveFrontend` (or `Frontend` → `RequestNewGame`
first) **MATCH**. `00418EC6` is **not** a `004184BD`
child; host notes it after `FinishInitGameAfterWorld`
(**DIVERGE** vs ctor-before-`vtbl+4`).

---

## 2. Native `004184BD` children (no-save)

Log helpers `0099EBF0` / `009D8240` / `009E9F40` /
`0099EAE0` omitted.

```
004184D1  [0x13B86A0] = game
009E9EF0 / 009E9F90
00416832  "Init Text" / Opening Text Bank
009E9F90
00414C90 / 009ED190          ; BindKey / RunScript
fild [display+456] → [game+104]
0044C6B6                     ; [0x13B879C]==0 first-seen
  alloc 0xE0 → 0044C6C2 / 0044C71F
"Init Thing Components"      004EE23F
"Init Definition Manager"    00416005(1)
"Init Graphics"              00416C8A
004168DC                     "Init Fonts" ENG_ARIAL_18
"Init Subtitled Message"     004CDB10
"Adding Console Variables"   log only (0041863D)
"Init Conversation Attitude" 004CD670
"Init Player Manager"        0041732A  ; alloc 44, 0044A3B0 → game+28
"Init Player Interface"      004473A0 → game+32
[0x13B7C90]==0 skip 0049E740 ; [ebp-1]=1
"Init World"                 0041735A
  0044C6B0
  alloc 0x198 → 004A67D0     ; world, game+36
  "Init World Init"
  [world].vtbl+36 004A6E30   ; map / env / 006B4900 / nav / …
  [world+320] = [ebp-1]
"Init Display Engine"        00417418
  [esi+36] world, [world+60] mesh bank
  alloc 0x100 → 00434E10 → game+40
  004350D0
"Create Players"             004166A8
"Init Sound"                 00417A58
[0x13B8648]==0
  "Load Particles"           004174F1
[game].vtbl+32               00416953   ; LoadWorld
[0x13B8648]==0
  0049BA70 / 00416392 / 004AE9D0
  default_user.ini / user.ini
009A4EC0  [engine+240]=004167DA [+244]=game
[+90544]=0  009E1BC0 → [+90548]  [+90592]=1
```

`004A6E30` is **not** a sibling of `00416953`.
`FORWARD_TREE` §6 / §7 / `init-world-004A6E30`.

---

## 3. Host `EnterGame` / `LoadWorld`

```
EnterGame
  RequestNewGame?                  ; if still Frontend
  notes: 0042F491, 00418DCA, 004184BD, 013B86A0
  note  009E9EF0 / 009E9F90 / 00416832
  note  009ED190
  foreach InitGameStages:          ; notes, 12 names MATCH
    Init Conversation Attitude → note 0041863D
    Init World                 → note 00419D90   ; HOIST
    note stage apply
    Init Graphics              → OpenTextureBank
    Init Display Engine        → 00434E10        ; no world yet
    Init Player Interface      → Player.Construct + 0044A3B0
    Init World                 → note [world+320]
    Load Particles             → note 013B8648
  note 004A67D0                    ; LATE
  note 004A6E30
  foreach InitWorldInitStages
  InitWorldCameras()               ; 006B4900 after the list
  CreatePlayers()                  ; LATE vs the named note
  LoadWorld()                      ; 00416953
  GameRenderEnabled = true
  FinishInitGameAfterWorld()       ; 0049BA70 … user.ini … 009A4EC0
  note [+90592]=1
  note 00418EC6 [+90593]=1         ; LATE vs ctor
```

`LoadWorld` first note is `004B4590` then `00416953`.
Native `00416953` first child is `[world].vtbl+28`
(`004A6550`); `004B4590` is inside that (`004A6697`).
Hoist **DIVERGE**, after the Init World one.

---

## 4. Side-by-side (first split)

| Step | Native `004184BD` | Host `EnterGame` | Class |
|---:|---|---|---|
| 1 | `[0x13B86A0]=game` | same note | **MATCH** |
| 2 | `009E9EF0` / `009E9F90` / `00416832` | one bundled note | **MATCH** note; `00416832` open **PARTIAL** |
| 3 | `00414C90` / `009ED190` | `009ED190` only | **MATCH** apply |
| 4 | `[game+104]` from display | never here | **PARTIAL** |
| 5 | `0044C6B6` → `0044C6C2` | *none* | **PARTIAL** — first omitted child |
| 6 | Thing Components … Player Interface | same notes | **MATCH** names |
| 7 | `0044A3B0` in `0041732A` | noted under Player Interface | **DIVERGE** (one stage late) |
| 8 | `00419D90` in `004A6E30` | noted **before** `0041735A` | **DIVERGE** hoist |
| 9 | `0041735A` → `004A67D0` → `004A6E30` | note `0041735A` only | **DIVERGE** — **first top-level reorder** |
| 10 | then `00417418` / `00434E10` | `00434E10` **now** (no `game+36`) | **DIVERGE** |
| 11 | `004166A8` / `00417A58` / `004174F1` | notes only | **PARTIAL** work |
| 12 | then `00416953` | `004A67D0` / `004A6E30` / cameras / `CreatePlayers()` / `LoadWorld()` | **DIVERGE** |

First *top-level child-order* DIVERGE is row **9**.
Rows 5 and 4 are earlier holes, not a swap of two
`004184BD` children host also runs.

---

## 5. Why `00417418` cannot precede `0041735A`

```
00417429  mov ecx, [0x13B8390]
0041742F  mov eax, [esi+36]      ; world — ctor 004A67D0
0041746E  call 0044C6B0
00417485  mov eax, [esi+36]
0041748B  mov eax, [eax+60]      ; mesh bank — 004A6E30 0049E620
004174A6  call 00434E10          ; game+40
```

`004A6E30` at `004A6F50` allocs `0x1970` `006B4900`
into `world+24` **before** `"Init Navigation Manager"`.
Host `InitWorldCameras` runs **after**
`InitWorldInitStages` (which already notes
`0069AE80` / `006FD8C0`) and **after** particles.
That is a **second** DIVERGE inside the late
`004A6E30` stand-in, not the first.

Native `004A6E30` also has `"Init Global Console"`
`00419D90`, Bullet Time, Opinion, Script Conversation,
particle-bank hooks, Animation Events, Speech Gain.
Host `InitWorldInitStages` is a subset (**PARTIAL**).

---

## 6. `LoadWorld` is not the first split

Native `004188E9` `call [eax+32]` is the **last**
big child of `004184BD`, after particles.
Host `LoadWorld()` is also last among the big
blocks — but only because world ctor was pulled
**past** Display / Create Players / Sound /
Particles to sit just above it.

Inside `00416953`, no-save order
`vtbl+28` → skip `004A3200` → `+90576
FinalAlbion.wld` → `004A1840` → `0049F180` →
`004BBC00` still **MATCH** the host notes
(`LoadWorld_00416953_*`). That is **not** this
file’s first DIVERGE.

`00415E17` copies the Leave path into `game+90576`
in `00418DCA`, **before** `004184BD`. Host sets
`WorldFileName` in `RequestNewGame` (`0042F44D`).
**MATCH** for the string; not a `004184BD` child.

---

## 7. What this does **not** say

- New Game is `00DBDE40` / Oakvale. **DISPROVEN**.
- Named `InitGameStages` list is the wrong order.
  **DISPROVEN** — the *notes* match the strings.
- `0044C6B6` is the first *reorder*. It is the
  first *omission* (**PARTIAL**). First-seen
  `[0x13B879C]` is 0, so native **does** take
  `0044C6C2` / `0044C71F`.
- `CreatePlayers` `004166A8` is Hero / Oakvale TNG.
  **DISPROVEN** — five `0x22C` slots.
- `00416953` is a region load. **DISPROVEN**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `0042F2A2` | Leave, before this walk | **PROVEN** |
| `0042F491` | `"Init Game"` parent | **PROVEN** **MATCH** |
| `00418DCA` | ctor `0x161E8` / `00418EC6` | **PROVEN**; host `00418EC6` note **DIVERGE** |
| `004184BD` | `vtbl+4` start | **PROVEN** |
| `0044C6B6` | player-manager present? | **PARTIAL** host |
| `004168DC` | Init Fonts | **PARTIAL** host |
| `0041735A` | Init World (owns `004A67D0`/`004A6E30`) | **PROVEN**; host ctor **DIVERGE** |
| `00417418` | Init Display Engine | **PROVEN**; host early **DIVERGE** |
| `004166A8` | Create Players | note **MATCH**; `CreatePlayers()` **DIVERGE** |
| `004174F1` | Load Particles | **PROVEN** first-seen taken |
| `00416953` | Loading world | **PROVEN** last big child |
| `00419D90` | Init Global Console | host before `0041735A` **DIVERGE** |
| `00DBDE40` | later quest body | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\docs\runtime\FORWARD_TREE.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\init-world-004A6E30\README.md`
- `C:\FableCSharp\proofs\audit-lifecycle-newgame\README.md`
