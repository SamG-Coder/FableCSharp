# After Leave: Init Game `0042F491` first-seen callees

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave the next
string is `"Init Game"` then `FinalAlbion.wld` is already
on the path record (`0042F44D`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: ExeIndex `listing-00400000.txt` (`0042F297`–
`0042F508`, `004184BD`–`004189C1`, `0041735A`–`00417417`,
`00416953`–`00416C34`);
`listing-00480000.txt` (`004A6E30`, `0049F180`–`0049F2CB`);
`e8.tsv` dests `00418DCA` / `0041735A` / `0049F180` /
`0043A380` / `004B4260`;
`docs/runtime/FORWARD_TREE.md` §§4–7, 10;
`src/Fable.Game/EngineLifecycle.cs` (`EnterGame` /
`LoadWorld` / `InitCharactersAndQuests`);
siblings `proofs/leave-0042F2A2-host`,
`proofs/initgame-after-leave-order`,
`proofs/init-world-004A6E30`,
`proofs/init-gui-0043A380`,
`proofs/0049F180-first-children`.

`0042F491` is **not** a function. It is the
`push "Init Game"` site on the same `[esi+41]!=0`
arm of `0042EC7C` that just ran Leave `0042F2A2`.

---

## Verdict

**Of the five named VAs, only `004184BD` happens on
this site.** It is `[ebx].vtbl+4` after ctor
`00418DCA`. The other four are nested **later**
inside that vtbl+4:

```
0042F491  "Init Game"
  00418DCA
  [eax+4]  004184BD          // ON SITE
    "Init World" 0041735A
      [world].vtbl+36  004A6E30          // LATER
    [game].vtbl+32  00416953
      004A1840
      [0x13B8648]==0 → 0049F180(0)       // LATER
        "Init GUI"    0043A380           // LATER
        "Init Quests" 004B4260           // LATER first-seen
```

Host leftover is **inline**, not a reorder of those
five among themselves. `EnterGame` notes `004184BD`
then walks named `InitGameStages`, then constructs
world / `004A6E30` **after particles**, then
`LoadWorld` → `InitCharactersAndQuests`. Native
`004A6E30` is **inside** `0041735A`, **before**
`"Init Display Engine"`. That hoist is the order
leftover. `0049F180` / `0043A380` / `004B4260` stay
after `004A6E30` on both sides.

| VA | Role | This site vs later | Class |
|---|---|---|---|
| `004184BD` | game `vtbl+4` | **on site** `0042F4D2` | **PROVEN** |
| `004A6E30` | Init World Init | **later** `0041735A` `call [eax+36]` | **PROVEN** |
| `0049F180` | Init Characters | **later** no-save `00416BCA` | **PROVEN** |
| `0043A380` | Init GUI | **later** only `E8` `0049F214` | **PROVEN** |
| `004B4260` | Init Quests | **later** first-seen `0049F24E` | **PROVEN** |
| Oakvale / `00DBDE40` here | — | **not this walk** | **DISPROVEN** |
| Host `EnterGame` first-child list | inlines `004184BD` | **LEFTOVER** | **PROVEN** |
| Host `004A6E30` after `"Load Particles"` | vs native inside `0041735A` | **LEFTOVER** / **DIVERGE** | **PROVEN** |
| Host relative order of the five | `004184BD` → `004A6E30` → `0049F180` → `0043A380` → `004B4260` | **MATCH** | **PROVEN** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First-seen callees of `0042F491` (listing order)? | §2 | **PROVEN** |
| Which of the five on this site? | **only** `004184BD` | **PROVEN** |
| `004A6E30` / `0043A380` / `0049F180` / `004B4260` on this site? | **No.** nested under `004184BD` | **DISPROVEN** as first children |
| Host order leftover? | Yes: `EnterGame` **inlines** vtbl+4 and **hoists** `004A6E30` past Display / Create Players / Sound / Particles. Not Oakvale. | **PROVEN** leftover |

---

## 1. Parent: Leave then this site

Same `0042EC7C` New Game arm (`listing-00400000.txt`):

```
0042F297  cmp [esi+41], bl
0042F29A  je  0042F4EC
0042F2A2  push "Leave frontend"
…
0042F44D  push "FinalAlbion.wld"
0042F48A  call 0042EBB6
0042F48F  push -1
0042F491  push "Init Game"          // THIS SITE
…
0042F4C7  call 00418DCA
0042F4D2  call [eax+4]             // 004184BD
0042F4E3  call 004131D0
0042F4E8  mov al, 1
0042F4EA  jmp 0042F501
```

Leave and Init Game are **siblings** on one `ret 4`.
Host `Pump` `RequestNewGame` then `EnterGame` matches
that pair. **PROVEN.**

`e8.tsv` `00418DCA` sites: `00413003` (LOAD arm),
`0042F4C7` (this), `00496E2C`. First-seen New Game
take is `0042F4C7`. **PROVEN.**

---

## 2. First-seen callees of `0042F491` (listing order)

Taken calls from the `"Init Game"` push through the
success `jmp 0042F501`. Log / CString helpers listed
because they are real `E8`s; they are not the five
named VAs.

| # | Site | Target | Role |
|---:|---|---|---|
| 1 | `0042F499` | `0099EBF0` | CString `"Init Game"` |
| 2 | `0042F4A4` | `009D8240` | log (`ret`) |
| 3 | `0042F4AC` | `0099EAE0` | CString dtor |
| 4 | `0042F4B6` | `00BFEA1A` | alloc `0x161E8` |
| 5 | `0042F4C7` | `00418DCA` | game ctor, vtbl `0122F180`, `[+90593]=1` |
| 6 | `0042F4D2` | `[eax+4]` = **`004184BD`** | start |
| 7 | `0042F4E3` | `004131D0` | path-record dtor |

No `E8` of `004A6E30` / `0043A380` / `0049F180` /
`004B4260` on this arm. `004184BD` is not an `E8`
(`e8.tsv` dest empty); it is the vtbl slot. **PROVEN.**

Stores on the same span: `[[ebp+124]] = game`,
`[0x13B7D58] = esi` (old retail). Not callees.

---

## 3. Later: the other four, still under `004184BD`

### `004A6E30` — Init World Init

`004184BD` at `00418758` `"Init World"` then
`00418784 call 0041735A` (`e8.tsv` only that dest
from this walk).

`0041735A`: alloc `0x198` → `004A67D0` → `game+36`,
`"Init World Init"`, then

```
0041740C  mov ecx, [edi]     ; world
0041740E  mov eax, [ecx]
00417410  call [eax+36]      ; 004A6E30
```

Zero `.text` `E8` of `004A6E30`. Next sibling in
`004184BD` is `"Init Display Engine"` `00417418`.
**PROVEN later. DISPROVEN as a `0042F491` child.**

### `0049F180` — Init Characters

Last big child of `004184BD` is `004188E9 call [eax+32]`
=`00416953`. No-save `[game+90588]` empty →
`"Loading world"` → `00416ABA call 004A1840`. Then

```
00416ABF  cmp [0x13B8648], 0
00416AC9  je  00416BC8
00416BC8  push 0
00416BCA  call 0049F180      ; ecx = world
```

`e8.tsv` dests of `0049F180`: `00416BCA` (this) and
`004A2C80` (save reader `004A21F0`, **not** this walk).
**PROVEN later. DISPROVEN as a `0042F491` child.**

### `0043A380` — Init GUI

Inside `0049F180`:

```
0049F1EC  push "Init GUI"
0049F20E  mov ecx, [0x13B8790]
0049F214  call 0043A380
```

`e8.tsv` dest `0043A380`: **only** `0049F214`. Reset,
not ctor (`proofs/init-gui-0043A380`). **PROVEN later.**

### `004B4260` — Init Quests

Same function, next named string:

```
0049F21B  push "Init Quests"
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260
```

First-seen on no-save New Game is this `E8`. Other
`e8.tsv` dests (`0049EAD1`, `004B4A5A`, `004B5B84`,
`00892EAF` / `00892EEF`) are save / `004B4A10` /
script `ActivateQuest`. The sibling after
`0049F180` returns is `"Activate Initial Quests"`
`00416C11 call 004B4A10` (empty `+90584` vs
`0x122D70E` first-seen skip). **PROVEN later.
DISPROVEN as a `0042F491` child.**

---

## 4. Host leftover

`EnterGame` notes this site, then **does the body of
`004184BD` in the same method**:

```
EnterGame
  Note 0042F491 / 00418DCA / 004184BD     // MATCH first children 5–6
  foreach InitGameStages                  // LEFTOVER inline
    "Init World" → note 0041735A only
    "Init Display Engine" → 00434E10      // native needs world+60
    "Load Particles"
  Note 004A67D0 / 004A6E30                // LATE vs 0041735A
  InitWorldInitStages / cameras / CreatePlayers
  LoadWorld → InitCharactersAndQuests
    0049F180 / 0043A380 / 004B4260        // later, same relative order
```

| Host vs native first children of `0042F491` | Class |
|---|---|
| Notes `0042F491` / `00418DCA` / `004184BD` | **MATCH** |
| Skip `0099EBF0` / `0099EAE0` / `00BFEA1A` / `004131D0` | **LEFTOVER** skip |
| Skip `009D8240` (`ret`) | **MATCH** skip |
| Named stages + `004A6E30` as `EnterGame` siblings | **LEFTOVER** inline |
| `004A6E30` after particles, before `00416953` | **LEFTOVER** hoist (**DIVERGE** vs `0041735A`) |
| `0049F180` → `0043A380` → `004B4260` after world load | **MATCH** later |
| Extra `0049F180` Note in `SpawnHeroFromPlayerStart` | **LEFTOVER** duplicate |
| Jump to Oakvale from this site | **DISPROVEN** |

Implement no new `0042F491` body. Do not promote
Init World / Init GUI / Init Characters / `004B4260`
to first children of this site.

---

## 5. What this does **not** say

- `"Init Game"` is the function at `004184BD`.
  **DISPROVEN** — `004184BD` is vtbl+4; the string
  lives at `0042F491`.
- `004A6E30` is a sibling of `00416953` on native.
  **DISPROVEN** — it returns before Display Engine.
- `0043A380` constructs `PLAYER_GUI_PC`.
  **DISPROVEN** — ctor is Create Players `0043B570`.
- First `004B4260` is `user.ini` `ActivateQuest`.
  **DISPROVEN** — that `00892EAF` is after
  `00416953` returns.
- `004A2C80` `0049F180(1)` on no-save.
  **DISPROVEN** (`proofs/0049F180-first-children`).

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\docs\runtime\FORWARD_TREE.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\leave-0042F2A2-host\README.md`
- `C:\FableCSharp\proofs\initgame-after-leave-order\README.md`
