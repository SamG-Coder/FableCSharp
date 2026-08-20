# First DIVERGE after Leave: `004A67D0` / `004A6E30` after `"Load Particles"`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this walk is
`FinalAlbion.wld` (`0042F44D`), then `"Init Game"` `0042F491`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: is the first DIVERGE after Leave host
`004A67D0` / `004A6E30` **after** the `"Load Particles"`
string, instead of **inside** `"Init World"` `0041735A`
**before** `"Init Display Engine"` `00417418`?

Confirm listing order and host `EnterGame` leftover.

Authority: `Fable.exe` via ExeIndex
`listing-00400000.txt` (`0042F491` / `004184BD` /
`0041735A` / `00417418` / `004174F1` / `00416953`),
`listing-00480000.txt` (`004A67D0` / `004A6E30`),
`e8.tsv` dests `004A67D0` / `0041735A` / `004A6E30`,
`functions.tsv` callees of `004184BD` / `0041735A`;
`proofs/initgame-after-leave-order`;
host `src/Fable.Game/EngineLifecycle.cs` `EnterGame` /
`InitGameStages` / `InitWorldInitStages` (read only).

Siblings: `proofs/0042F491-init-game-callees`,
`proofs/init-world-004A6E30`.

---

## Verdict

**PROVEN.** First *top-level reorder* after Leave vs host
`EnterGame` is `004A67D0` / `004A6E30` **after**
`"Load Particles"` `004174F1`, as a sibling of
`LoadWorld()`. Native runs both **inside** `"Init World"`
`0041735A`, **before** `"Init Display Engine"` `00417418`.

Named `InitGameStages` **notes** match `004184BD` string
order (**MATCH**). Host leftover is **inline** `004184BD`
plus a **hoist** of the world ctor / vtbl+36 past Display /
Create Players / Sound / Particles. Not Oakvale.

| Claim | Class |
|---|---|
| Native listing: `"Init World"` `0041735A` → `004A67D0` → `[world].vtbl+36` `004A6E30` → `"Init Display Engine"` `00417418` → `"Create Players"` → `"Init Sound"` → `"Load Particles"` `004174F1` → `[game].vtbl+32` `00416953` | **PROVEN** |
| Only `.text` `E8` of `004A67D0` is `00417396` (inside `0041735A`) | **PROVEN** |
| Zero `.text` `E8` of `004A6E30`; only `00417410 call [eax+36]` | **PROVEN** |
| `004A67D0` / `004A6E30` are **not** `004184BD` children after particles | **PROVEN** native |
| Host `InitGameStages` named order matches listing strings | **PROVEN** **MATCH** (notes) |
| Host `EnterGame` constructs / notes `004A67D0` / `004A6E30` **after** the named loop (`Load Particles` last) | **PROVEN** leftover |
| That hoist is the first *top-level child-order* **DIVERGE** vs `EnterGame` | **PROVEN** |
| Native work is **inside** `0041735A`, **before** `00417418` | **PROVEN** |
| First *omitted* `004184BD` child `0044C6B6` is earlier | **PARTIAL** (hole, not this reorder) |
| Host `00419D90` note before `"Init World"` apply | **DIVERGE** note; child of `004A6E30`, not this first reorder |
| Oakvale / `00DBDE40` on this walk | **DISPROVEN** |

**Answer:** first *order* DIVERGE after Leave vs
`EnterGame` is **`004A67D0`/`004A6E30` late** (after
`004174F1`, before `00416953`) instead of **inside
`0041735A`** (before `00417418`). Listing names stay in
order. Host leftover is the inline + hoist.

---

## 1. Native listing order (`Fable.exe`)

`listing-00400000.txt` `004184BD` after Leave
`0042F48A` / `"Init Game"` `0042F491` / ctor
`00418DCA` / `[eax+4]`:

```
00418758  push "Init World"
00418784  call 0041735A
00418790  mov [eax+320], cl        ; eax = [esi+36] world
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
004188E9  call [eax+32]            ; 00416953
```

Inside `0041735A` (`listing-00400000.txt`):

```
0041737E  push 0x198
00417386  call 00BFEA1A
00417396  call 004A67D0            ; CWorld ctor, vtbl 012390F0
0041739F  lea edi, [esi+36]
004173A5  call 004193E8            ; store world at game+36
004173E1  push "Init World Init"
0041740C  mov ecx, [edi]
0041740E  mov eax, [ecx]
00417410  call [eax+36]            ; 004A6E30
00417417  ret
00417418  push ebp                 ; next fn: Init Display Engine
```

`listing-00480000.txt`: `004A67D0` writes
`[esi]=0x12390F0`. `004A6E30` first named push is
`"Init World Map"`.

`e8.tsv`: dest `004A67D0` **only** `00417396`. Dest
`0041735A` **only** `00418784`. Dest `004A6E30`
**empty**. `functions.tsv` `004184BD` callees include
`0041735A` then `00417418` then `004166A8` then
`00417A58` then `004174F1` — no `004A67D0` /
`004A6E30` at that level.

`00417418` reads `[esi+36]` (world) and
`[world+60]` (mesh bank from `004A6E30` `0049E620`)
before `00434E10`. Native **cannot** run Display
before the ctor.

Same spine as `proofs/initgame-after-leave-order` §§2, 5.

---

## 2. Host `EnterGame` leftover

`InitGameStages` twelve names are the `004184BD`
strings from Thing Components through Load Particles
(**MATCH** notes). `EnterGame` walks that list, then
does world work **after** the loop:

```
EnterGame
  notes: 0042F491 / 00418DCA / 004184BD
  foreach InitGameStages            ; last name "Load Particles"
    "Init World"           → note 0041735A only
    "Init Display Engine"  → 00434E10   ; no game+36 yet
    "Load Particles"       → note 013B8648
  Note 004A67D0                     ; LATE
  Note 004A6E30
  foreach InitWorldInitStages
  InitWorldCameras / CreatePlayers
  LoadWorld()                       ; 00416953
```

| Host vs native | Class |
|---|---|
| Named stage string order | **MATCH** |
| `004184BD` body inlined into `EnterGame` | **LEFTOVER** |
| `004A67D0` / `004A6E30` after `"Load Particles"` | **LEFTOVER** hoist (**DIVERGE**) |
| Native pair inside `0041735A`, before `00417418` | **PROVEN** |
| `0049F180` / `0043A380` / `004B4260` after `00416953` | **MATCH** later (`0042F491-init-game-callees`) |
| Jump to Oakvale from this site | **DISPROVEN** |

---

## 3. Why this is the first *reorder*, not the first *hole*

`initgame-after-leave-order` §4: earlier differences are
omissions or note hoists, not a swap of two `004184BD`
children the host also runs.

| Earlier item | Why not this DIVERGE |
|---|---|
| `0044C6B6` | first *omitted* child (**PARTIAL**). Host never runs it. |
| `[game+104]` from display | never here (**PARTIAL**) |
| `00419D90` note before `"Init World"` | host note of a **child of** `004A6E30`; not the ctor pair |

First *top-level child-order* DIVERGE is the late
`004A67D0` / `004A6E30`. `LoadWorld` is last on both
sides only because the ctor was pulled past Display /
Players / Sound / Particles.

---

## 4. What this does **not** say

- Named `InitGameStages` list is the wrong order.
  **DISPROVEN** — the *notes* match the strings.
- `004A6E30` is a sibling of `00416953` on native.
  **DISPROVEN** — it returns before Display Engine.
- New Game starts at `00DBDE40` / Oakvale.
  **DISPROVEN**.
- Live `[0x13B8648]` on a save walk. **UNREAD** here;
  first-seen no-save is 0, so `"Load Particles"` is
  taken.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\proofs\initgame-after-leave-order\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`EnterGame`, read only)
