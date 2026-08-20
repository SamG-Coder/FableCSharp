# Named stage `"Create Players"` `004166A8` first-seen vs host `CreatePlayers()`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`Q_NewOakValeIntro` / `S_QNOVI` / `hero_swap_*.tng`.
After Leave this walk is `FinalAlbion.wld` (`0042F44D`),
then `"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: named stage `"Create Players"` `004166A8`
after `"Init Display Engine"`: what does it call
**first-seen**? Host `CreatePlayers()` **MATCH** if
invoked at that named stage?

Authority: `Fable.exe` via ExeIndex
`listing-00400000.txt` (`004184BD` `00418808` /
`004166A8` / `0041732A` / `00417418`),
`listing-00440000.txt` (`0044C6B0` / `0044A530` /
`0044A1A0` / `0044BC10` / `0044A3B0`),
`listing-00480000.txt` (`004AE940`),
`listing-00980000.txt` (`0099A350`),
`e8.tsv` dests `004166A8` / `0044A530` / `004AE940` /
`0044A1A0` / `0044BC10` / `0044A3B0` / `00522A20` /
`00DBDE40`,
`functions.tsv` (`004184BD` callees),
`strings.tsv` `"Create Players"` `0x0122F084`;
host `src/Fable.Game/EngineLifecycle.cs`
`InitGameStages` / `EnterGame` / `CreatePlayers`
(read only).

Siblings: `proofs/initgame-after-leave-order`,
`proofs/entergame-vs-0041735A`,
`proofs/004A67D0-after-particles`.

---

## Verdict

**First-seen child of `004166A8` is `0044C6B0`
(`[0x13B879C]`).** Then `[game+28]` `0044A530`
(five `0x22C` slots via `0044A1A0` / `0044BC10`,
`[+24]=4`) then `004AE940` (`game+80568`,
`0099A350` → `[+9826]=1`). Not `0044A3B0` /
`hero_swap_*.tng`. Not Oakvale.

Host `CreatePlayers()` at the `"Create Players"`
arm of `InitGameStages` (after `"Init Display
Engine"`) is **MATCH**: same site as native
`00418834`, same first-seen `0044C6B0`, same
slot / `004AE940` work. Extra `00522A20`
`PlayerCreature` note is **LEFTOVER** (only
`.text` `E8` is `005236D4` in Thing Manager
ctor).

Older proofs that called late `CreatePlayers()`
after `LoadWorld` are stale vs current
`EnterGame` (`if (name == "Create Players")
CreatePlayers()`).

| Claim | Class |
|---|---|
| Native `004184BD`: Display `00417418` then `"Create Players"` `004166A8` | **PROVEN** |
| Only `.text` `E8` of `004166A8` is `00418834` | **PROVEN** |
| `004166A8` first `E8` is `0044C6B0` (`mov eax,[0x13B879C]; ret`) | **PROVEN** |
| Then `[esi+28]` `0044A530` (only `E8` of dest) | **PROVEN** |
| `0044A530`: slots `0..3` then push `4`; five `0044A1A0` / `0x22C` / `0044BC10`; `[+24]=4` | **PROVEN** |
| Then `lea [esi+80568]` `004AE940` (only `E8` of dest) → `0099A350` always `al=1` → `[+9826]=1` | **PROVEN** |
| `004166A8` is `hero_swap_*.tng` / `0044A3B0` | **DISPROVEN** (`0044A3B0` only `E8` is `00417345` in `"Init Player Manager"`) |
| `004166A8` / host `CreatePlayers()` is Oakvale / `00DBDE40` | **DISPROVEN** |
| Host named-stage invoke `CreatePlayers()` after Display | **MATCH** |
| Host first note `0044C6B0` then `0044A530` / slots / `004AE940` | **MATCH** |
| Host `Note(00522A20 PlayerCreature)` inside `CreatePlayers()` | **LEFTOVER** |
| `functions.tsv` folds `004166A8` into `004165E8` | **PARTIAL** index; listing `004166A8`…`004166E1` is the body |

---

## 1. Named site after Init Display

`listing-00400000.txt` `004184BD`:

```
00418796  push "Init Display Engine"
004187E2  call 00417418
00418808  push "Create Players"          ; 0x0122F084
00418832  mov ecx, esi                   ; game
00418834  call 004166A8
0041883A  push "Init Sound"
```

`e8.tsv` dest `004166A8`: **only** `00418834`.
`functions.tsv` `004184BD` callees:
`00417418` then `004166A8` then `00417A58`.

Host `InitGameStages` tenth name is
`("Create Players", 0x004166A8)` after
`"Init Display Engine"` `00417418`.
`EnterGame` runs `CreatePlayers()` on that
name. **MATCH** site.

---

## 2. `004166A8` body (first-seen)

`listing-00400000.txt`:

```
004166A8  push esi
004166A9  push edi
004166AA  mov esi, ecx                   ; game
004166AC  call 0044C6B0                  ; FIRST E8
004166B1  mov ecx, [esi+28]
004166B4  call 0044A530
004166B9  lea edi, [esi+80568]
004166BF  push esi
004166C0  mov ecx, edi
004166C2  call 004AE940
004166C7  test al, al
004166C9  jne 004166DF                   ; taken first-seen
004166CD  call 0099A330                  ; dead (predicate 1)
004166DA  jmp 004AE990                   ; dead
004166E1  ret
```

`0044C6B0` (`listing-00440000.txt`):

```
0044C6B0  mov eax, [0x13B879C]
0044C6B5  ret
```

`eax` is unused. First-seen is still the
singleton getter. `[game+28]` is the owner
from `"Init Player Manager"` `0041732A`
(`00BFEA1A(44)` → `0044A3B0` → `004193A0`
store). That owner already exists before
Display. **PROVEN.**

---

## 3. Slots, not hero_swap

`0044A530` (only caller `004166B4`):

```
0044A536  xor esi, esi
0044A540  push esi
0044A543  call 0044A1A0                  ; i = 0..3
0044A548  inc esi
0044A549  cmp esi, 4
0044A54C  jl 0044A540
0044A54E  push 4
0044A552  mov [edi+24], 0x4
0044A559  call 0044A1A0                  ; fifth
```

`0044A1A0`: `push 0x22C` / `00BFEA1A` /
`0044BC10` (only `E8` of ctor). `0044BC10`
writes vtbl `01231CC4` and zeros; first
`E8` `0099A310`. No TNG. No `00DBDE40`.

`0044A3B0` first-seen `"hero_swap_1.tng"`
is **Init Player Manager**, not this stage.
`e8.tsv` dest `0044A3B0` = only `00417345`.
**DISPROVEN** as Create Players work.

`004AE940` first `E8` `0099A350`:
`mov al,1; mov [ecx+4],al; ret`. Always
writes `[player+9826]=1` / `[+9824]=1`.
Fail path `0099A330` **UNREAD** on first-seen.

---

## 4. Host `CreatePlayers()` at the named stage

Current `EnterGame` (read only):

```
foreach InitGameStages
  "Init Display Engine" → Note 00434E10
  "Create Players"      → CreatePlayers()
CreatePlayers
  Note 0044C6B0 [0x13B879C]          ; MATCH first-seen
  Note 0044A530 slots 0-4            ; MATCH
  Note 0044A1A0 / 0044BC10 × 5       ; MATCH
  Note 004AE940 / 0099A350 +9826=1   ; MATCH
  Note 00522A20 PlayerCreature       ; LEFTOVER
  Note 004166A8 slots=5 active=4     ; MATCH counts
```

Invoking the method here does **not** need
world or Display objects: native `004166A8`
only touches `game+28` and `game+80568`.
World ctor order is a different leftover
(`004A67D0-after-particles`).

`00522A20` dest in `e8.tsv` is only
`005236D4` (`00523540` Thing Manager).
Not a child of `004166A8`. Keep the note
out of this stage.

---

## 5. What this does **not** say

- Create Players spawns Hero / Lookout TNG.
  **DISPROVEN** — slots + `game+80568` flag.
- `0043B570` `PLAYER_GUI_PC` is first-seen
  here. **DISPROVEN** — only `.text` `E8`
  in dump walk is `00487FEE`, later GUI.
- `functions.tsv` `004165E8` callee list
  (`0044C6B0,0044A530,004AE940`) replaces
  the listing. **PARTIAL** — prologue merge;
  work function is `004166A8`.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `00418834` | only caller of `004166A8` | **PROVEN** |
| `004166A8` | Create Players | **PROVEN**; host invoke **MATCH** |
| `0044C6B0` | first-seen | **PROVEN** **MATCH** |
| `0044A530` | five slots, `[+24]=4` | **PROVEN** **MATCH** |
| `0044A1A0` / `0044BC10` | `0x22C` ctor | **PROVEN** **MATCH** |
| `004AE940` / `0099A350` | `game+80568` `+9826` | **PROVEN** **MATCH** |
| `0044A3B0` / `hero_swap_*.tng` | Player Manager owner | **DISPROVEN** here |
| `00522A20` | host extra note | **LEFTOVER** |
| `00DBDE40` | Oakvale quest body | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
