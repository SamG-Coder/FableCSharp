# Host leftover: `00419D90` hoist before `"Init World"`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`StartOakVale` / `Q_NewOakValeIntro` / `S_QNOVI`.
After Leave this walk is `FinalAlbion.wld` →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `00419D90` ActivateQuest hoist before
Init World. Host leftover vs MATCH? First leftover?

Authority: existing `proofs/00419D90-hoist`;
Fable.exe dump
`listing-00400000.txt` (`00419CE0` / `00419D90` /
`0041735A` / `004184BD` `00418757`–`004187E2`);
`listing-00480000.txt` (`004A6E30` / `004A7103` /
`004A712B`);
`e8.tsv` dests `0041735A` / `00419D90` / `00419CE0`;
`xrefs.tsv` `0x0122F380` `"ActivateQuest"`;
host notes only:
`EngineLifecycle.EnterGame` /
`IniActivateQuestRegister` /
`InitGameStages` / `InitWorldInitStages` /
`DispatchUserIniCommand`.
Siblings: `proofs/00419D90-hoist`,
`proofs/00417418-world-plus60`,
`proofs/0044C6B6-first-omit`,
`proofs/init-world-004A6E30`,
`proofs/ini-activate-quest`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Host notes `00419D90` before `"Init World"` apply? | **Yes.** `EnterGame` `if (name == "Init World")` `Note(IniActivateQuestRegister)` **then** `Note(0x0041735A)`. | **PROVEN** |
| Host leftover vs MATCH? | **LEFTOVER** hoist. Not MATCH. Wrong parent, wrong order. `Note` is `Trace.Add` only. | **PROVEN** leftover / **DISPROVEN** MATCH |
| Native order? | **`0041735A` first**, then later `00419D90` at unique `E8` `004A712B` inside `004A6E30` (`"Init Global Console"`). | **PROVEN** |
| First leftover on the `004184BD` walk? | **No.** First leftover is omit `0044C6B6` (`0041852D`) before `"Init Thing Components"`. | **DISPROVEN** as first leftover |
| First leftover **in the `"Init World"` arm**? | **Yes.** First host line on that arm is the hoist note. Native first child of `0041735A` is the world ctor, not `00419D90`. | **PROVEN** |
| First leftover **Note** of a later grandchild? | **Yes.** `0041863D` on `"Init Conversation Attitude"` MATCH. Next extra VA is `00419D90` before `0041735A`. | **PROVEN** |
| `00419D90` is the live `user.ini` activate? | **No.** Register only. Apply is `00419CE0` (zero `.text` `E8`). | **DISPROVEN** |
| Oakvale / `00DBDE40` on this site? | **No.** | **DISPROVEN** |

---

## Verdict

**LEFTOVER, not MATCH. Not the first leftover
on the walk.**

`IniActivateQuestRegister` (`0x00419D90`) is the
`"Init Global Console"` **command registrar**
(alloc 24, name `"ActivateQuest"`,
`[cmd+20]=00419CE0`, `009EC5E0` into
`[0x13CAA40]`). Native runs it only after
`"Init World"` `0041735A` has already built the
world and walked nav / flyer inside `004A6E30`.

Host `EnterGame` emits that VA as a pre-apply
Ini note on the `"Init World"` loop arm. That
is leftover label placement. World ctor notes
now sit **inside** the same arm **after**
`Note(0041735A)` and **MATCH** native parent
(`proofs/00417418-world-plus60`). The hoist
did **not** move with them.

Walk-first leftover stays `0044C6B6`. This
file’s leftover is the first wrong-parent
note **on the Init World arm**, not the first
hole after Leave.

---

## 1. Native: `0041735A` then `00419D90`

`e8.tsv`:

```
0x00418784  0x0041735A     // unique
0x004A712B  0x00419D90     // unique
```

Zero `.text` `E8` of dest `00419CE0`.

`listing-00400000.txt` — Init World is a
`004184BD` child **after** Player Interface:

```
00418757  push "Init World"
00418782  mov ecx, esi
00418784  call 0041735A
00418789  mov eax, [esi+36]
00418790  mov [eax+320], cl
00418796  push "Init Display Engine"
004187E2  call 00417418
```

`0041735A` owns the world, then vtbl+36:

```
00417396  call 004A67D0          ; world → game+36
004173E1  push "Init World Init"
00417410  call [eax+36]          ; 004A6E30
```

`listing-00480000.txt` — registrar is child 8
of `004A6E30`, **after** World Map /
Environment / nav / flyer:

```
004A7103  push "Init Global Console"
004A712B  call 00419D90
004A7132  push "Adding Console Commands"
004A715C  push "Init Combat Manager"
```

`00419D90` **cannot** precede `0041735A`.
**PROVEN** (`proofs/00419D90-hoist`).

---

## 2. What `00419D90` is

`listing-00400000.txt`:

```
00419D90  sub esp, 16
00419D97  push 24
00419D9D  call 00BFEA1A
00419DAE  push "ActivateQuest"        ; xrefs 0x0122F380
00419DF6  mov [esi+20], 0x419CE0      ; handler
00419E02  mov eax, [0x13CAA40]
00419E39  call 009EC5E0               ; insert command
00419E69  ret
```

Live apply is sibling `00419CE0` (game
`vtbl+36` gate `004197B0`, then `[world+56]`
vtbl+1104 `00892E80`). That path is after
`00416953` / `user.ini` (`00418969`), not at
register time. **PROVEN**
(`proofs/ini-activate-quest`).

---

## 3. Host leftover vs MATCH

`EnterGame` (`EngineLifecycle.cs`):

```
foreach InitGameStages:
  if name == "Init Conversation Attitude":
    Note(0041863D)                         // MATCH string
  if name == "Init World":
    Note(00419D90, "00419D90 ActivateQuest")  // HOIST
  Note(apply)                              // 0041735A
  if name == "Init World":
    Note(004A67D0) / Note(004A6E30)        // MATCH parent
    foreach InitWorldInitStages            // no 00419D90
    InitWorldCameras()
```

`Note` is `Trace.Add` only. No `00BFEA1A(24)`,
no `[cmd+20]`, no `009EC5E0`. Leftover **note**,
not leftover mutation.

`InitWorldInitStages` starts at World Map /
Environment / Nav. It never names
`"Init Global Console"`. The registrar is
therefore **not** noted at `004A712B`.

`DispatchUserIniCommand("ActivateQuest")`
notes `00419D90` **again** at `user.ini`
apply. That site is the **handler**
(`00419CE0`). Second leftover **label**.

| Host | Native | Class |
|---|---|---|
| `Note(00419D90)` before `Note(0041735A)` | `004A712B` after `0041735A` / `004A67D0` / nav | **LEFTOVER** hoist / **DIVERGE** |
| Named `"Init World"` → `"Init Display Engine"` | same strings | **MATCH** notes |
| `004A67D0` / `004A6E30` inside Init World arm | same parent | **MATCH** order (`00417418-world-plus60`) |
| `InitWorldInitStages` skips Global Console | child 8 of `004A6E30` | **LEFTOVER** omit |
| `user.ini` note `00419D90` | apply is `00419CE0` | **LEFTOVER** label |
| Hoist note **is** MATCH native register | only `E8` is `004A712B` | **DISPROVEN** |

---

## 4. First leftover?

| Scope | First leftover | This VA |
|---|---|---|
| Whole `004184BD` walk | omit `0044C6B6` at `0041852D` (work: `00BFEA1A(0xE0)` / `0044C6C2` / `0044C71F`) | **later** leftover note |
| Named `InitGameStages` strings | notes MATCH Thing Components … Load Particles | hoist is extra VA, not a name swap |
| `"Init World"` host arm | **this hoist** (first line, before apply) | **first on this arm** |
| If we **move** the note to `004A712B` | leftover becomes register **work** (`00BFEA1A(24)` / `009EC5E0`) | note site then **MATCH** |
| If we also **do** the register work | next omit on `004A6E30` is still Search Tools / Bullet Time / … (`init-world-004A6E30` subset) | this VA **MATCH** |
| `user.ini` apply | leftover label `00419D90` vs `00419CE0` | **second** leftover, later |

`proofs/004A67D0-after-particles` still calls
the late world ctor the first *top-level
reorder*. That host order is **stale**: ctor
notes are now inside Init World. The
`00419D90` hoist is **not** that reorder and
was never the first leftover.

Do **not** treat a moved `Note(00419D90)` as
constructing the command table. Do **not**
invent `ActivateQuest(Q_NewOakValeIntro)`
from this VA.

---

## 5. What this does **not** say

- Moving the host **note** is not the same as
  registering the command on time.
- First-seen TLC `user.ini` still activates
  **Gameflow** via `00419CE0` after Load World.
- New Game is Oakvale / `S_QNOVI`. **DISPROVEN**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `00419D90` | register `"ActivateQuest"` | **PROVEN**; host pre-`0041735A` note **LEFTOVER** |
| `004A712B` | only `.text` `E8` of `00419D90` | **PROVEN** |
| `0041735A` | Init World parent | **PROVEN**; named note **MATCH** |
| `004A6E30` | `"Init Global Console"` parent | **PROVEN**; host omit of that child **LEFTOVER** |
| `00419CE0` | live apply | **PROVEN**; host `user.ini` `00419D90` label **LEFTOVER** |
| `0044C6B6` | first `004184BD` leftover | **PROVEN** earlier omit |
| `00DBDE40` | later quest body | **DISPROVEN** here |

---

## Paths

- `proofs/00419D90-hoist/README.md`
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00480000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
- `tools/Fable.ExeIndex/out/00-index/xrefs.tsv`
- `src/Fable.Game/EngineLifecycle.cs` (`EnterGame` / `IniActivateQuestRegister`, notes only)
