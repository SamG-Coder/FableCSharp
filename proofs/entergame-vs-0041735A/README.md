# Host `EnterGame` vs native `0041735A` / `00417418`

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `StartOakVale` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` (`0042F44D`), then
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **DIVERGE** / **MATCH**.

Question: host `EnterGame` notes Display / Create
Players / Sound / Particles then constructs world.
Native `004184BD` / `0041735A` Init World first,
then `00417418` Init Display. **First DIVERGE
site** and **what host must reorder**?

Authority: `Fable.exe` via ExeIndex
`listing-00400000.txt` (`004184BD` / `0041735A` /
`00417418` / `004166A8` / `00417A58` / `004174F1`)
and `listing-00480000.txt` (`004A67D0` / `004A6E30` /
`006B4900`); `functions.tsv` callees of `004184BD`;
`proofs/initgame-after-leave-order`;
`docs/runtime/FORWARD_TREE.md` §§6–7;
`src/Fable.Game/EngineLifecycle.cs` (`EnterGame` /
`InitGameStages` / `InitWorldInitStages` /
`InitWorldCameras` / `CreatePlayers`).

Sibling: `proofs/0042F491-init-game-callees`,
`proofs/init-world-004A6E30`.

---

## Verdict

**First order DIVERGE is the `"Init World"` site
`0041735A` (`00418784`).** Native constructs
`004A67D0` / `004A6E30` **inside** that call,
**then** `00417418`. Host notes the twelve
`InitGameStages` names in native string order
(**MATCH** notes), runs `00434E10` on
`"Init Display Engine"` with **no** `game+36`,
then after `"Load Particles"` hoists the world
ctor. That is the first swap of two `004184BD`
children the host also runs.

**Host must reorder:** move `Note(004A67D0)` /
`004A6E30` / `InitWorldInitStages` /
`OpenMeshBank` / `006B4900` from after
`004174F1` **into** the `"Init World"` arm,
**before** `"Init Display Engine"` `00434E10`.
Then run `CreatePlayers()` at the `"Create
Players"` note (`004166A8`), not after the
late world block. Leave `LoadWorld` `00416953`
last among the big children.

| Claim | Class |
|---|---|
| Native `004184BD` child order is World `0041735A` → Display `00417418` → Players `004166A8` → Sound `00417A58` → Particles `004174F1` → `vtbl+32` `00416953` | **PROVEN** |
| `0041735A` owns `004A67D0` + `[world].vtbl+36` `004A6E30` | **PROVEN** |
| `00417418` reads `[game+36]` and `[world+60]` | **PROVEN** |
| Host `InitGameStages` **names** match those strings | **PROVEN** **MATCH** |
| Host constructs world after Display / Players / Sound / Particles | **DIVERGE** — **first reorder** |
| Host `00434E10` before `004A67D0` | **DIVERGE** |
| Host `CreatePlayers()` after late `004A6E30` | **DIVERGE** (second, same hoist) |
| First *omitted* `004184BD` child is `0044C6B6` | **PARTIAL** (earlier hole, not this swap) |
| Host `00419D90` note before `0041735A` | **DIVERGE** (note hoist; work still inside late `004A6E30`) |
| Oakvale / `00DBDE40` on this walk | **DISPROVEN** |

**Answer:** first DIVERGE **site** =
`00418784` / host `"Init World"` arm.
**Reorder** = hoist world ctor + Init World
Init **above** `00417418` / `00434E10`.

---

## 1. Native `004184BD` (no-save)

`listing-00400000.txt`:

```
00418758  push "Init World"
00418784  call 0041735A
00418789  mov eax, [esi+36]
00418790  mov [eax+320], cl          ; [ebp-1] from 013B7C90
00418796  push "Init Display Engine"
004187E2  call 00417418
00418808  push "Create Players"
00418834  call 004166A8
0041885A  push "Init Sound"
00418886  call 00417A58
0041888B  cmp [0x13B8648], bl
00418891  jne 004188E5
00418894  push "Load Particles"
004188E0  call 004174F1
004188E9  call [eax+32]              ; 00416953
```

`functions.tsv` `004184BD` callees list
`0041735A` then `00417418` then `004166A8`
then `00417A58` then `004174F1`. Same order.
**PROVEN.**

`0041735A`:

```
00417379  call 0044C6B0
0041737E  push 0x198
00417386  call 00BFEA1A
00417396  call 004A67D0              ; world
0041739F  lea edi, [esi+36]          ; store game+36
004173E1  push "Init World Init"
0041740C  mov ecx, [edi]
00417410  call [eax+36]              ; 004A6E30
00417417  ret
```

`004A6E30` sets `[world+60]` mesh bank
(`0049E620`) and `[world+24]` `006B4900`
**before** `"Init Navigation Manager"`
(`listing-00480000.txt` `004A6F50`).
WLD parse is **not** here. **PROVEN.**

`00417418` cannot precede that ctor:

```
0041742F  mov eax, [esi+36]          ; world
00417485  mov eax, [esi+36]
0041748B  mov eax, [eax+60]          ; mesh bank
004174A6  call 00434E10              ; game+40
```

**PROVEN.**

---

## 2. Host `EnterGame`

`InitGameStages` names **MATCH** the listing
strings (Thing Components … Load Particles).
The **work** those names own does not.

```
foreach InitGameStages
  Init World            → note 00419D90, note 0041735A,
                          note [world+320]     // no ctor
  Init Display Engine   → 00434E10             // no game+36
  Create Players        → note 004166A8 only
  Init Sound            → note 00417A58 only
  Load Particles        → note 013B8648 / 004174F1
note 004A67D0                                  // LATE
note 004A6E30
foreach InitWorldInitStages                    // OpenMeshBank
InitWorldCameras()                             // 006B4900 last
CreatePlayers()                                // LATE vs note
LoadWorld()                                    // 00416953
```

**DIVERGE** at the first stage whose native
body allocates the world.

---

## 3. Side-by-side (this split)

| Step | Native | Host | Class |
|---:|---|---|---|
| 1 | `"Init World"` `0041735A` → `004A67D0` / `004A6E30` | note `0041735A` only | **DIVERGE** — **first site** |
| 2 | `"Init Display Engine"` `00417418` / `00434E10` | `00434E10` now | **DIVERGE** |
| 3 | `"Create Players"` `004166A8` | note only | **PARTIAL** work |
| 4 | `"Init Sound"` / `"Load Particles"` | notes | **PARTIAL** work |
| 5 | `00416953` | `004A67D0` / `004A6E30` / cameras / `CreatePlayers()` / `LoadWorld()` | **DIVERGE** (same hoist) |

`0044C6B6` is an earlier **omission**, not a
reorder of two children host already runs.
`00419D90` is noted **before** `0041735A` on
host; native runs it **inside** `004A6E30`.
That is a note hoist, not the first
construct-order split.

`006B4900` after `InitWorldInitStages` is a
**second** DIVERGE inside the late
`004A6E30` stand-in (`init-world-004A6E30`).
Not the first.

---

## 4. What host must reorder

Pull this block from after `Load Particles`
into the `"Init World"` `if` (before the
Display arm):

1. `004A67D0` → `game+36`
2. `004A6E30` / `InitWorldInitStages` including
   `0049E620` `OpenMeshBank`
3. `006B4900` **inside** that list (after
   Environment, before Navigation) — not
   `InitWorldCameras()` after the list

Then:

4. `"Init Display Engine"` `00434E10` (now
   legal: world and `world+60` exist)
5. `"Create Players"` → call `CreatePlayers()`
   here, not after cameras
6. Sound / Particles notes (and work) stay
7. `LoadWorld()` / `00416953` stays last

Do **not** move `00416953` earlier. Do **not**
start Oakvale.

---

## 5. What this does **not** say

- Named `InitGameStages` list is the wrong
  order. **DISPROVEN** — notes match strings.
- `CreatePlayers` is Hero / Oakvale TNG.
  **DISPROVEN** — five `0x22C` slots
  (`initgame-after-leave-order`).
- `00416953` is a region load. **DISPROVEN**.
- First DIVERGE of the whole Leave tree is
  this hoist. **DISPROVEN** as a claim here —
  this file is only `EnterGame` vs
  `0041735A`/`00417418`.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004184BD` | `vtbl+4` parent | **PROVEN** |
| `0041735A` | Init World; owns ctor | **PROVEN**; host ctor **DIVERGE** |
| `004A67D0` | world ctor `game+36` | **PROVEN** native site; host **DIVERGE** late |
| `004A6E30` | Init World Init | **PROVEN** native site; host **DIVERGE** late |
| `00417418` | Init Display Engine | **PROVEN**; host early **DIVERGE** |
| `00434E10` | display ctor `game+40` | **PROVEN**; host before world **DIVERGE** |
| `004166A8` | Create Players | note **MATCH**; `CreatePlayers()` **DIVERGE** |
| `004174F1` | Load Particles | **PROVEN** first-seen taken |
| `00416953` | Loading world | **PROVEN** last big child |
| `00DBDE40` | later quest body | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\proofs\initgame-after-leave-order\README.md`
- `C:\FableCSharp\docs\runtime\FORWARD_TREE.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
