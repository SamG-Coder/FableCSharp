# Host hoists `00419D90` before `0041735A`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
After Leave this walk is Init Game `004184BD` →
`"Init World"` `0041735A`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: Fable.exe dump
`listing-00400000.txt` (`00419CE0` / `00419D90` /
`0041735A` / `00418757`–`004187E2`);
`listing-00480000.txt` (`004A6E30` / `004A712B`);
`e8.tsv` dests `0041735A` / `00419D90`;
`xrefs.tsv` `0x0122F380` `"ActivateQuest"`;
host notes only:
`EngineLifecycle.EnterGame` / `InitGameStages` /
`InitWorldInitStages` / `DispatchUserIniCommand`.
Siblings: `proofs/initgame-after-leave-order`,
`proofs/init-world-004A6E30`,
`proofs/ini-activate-quest`,
`proofs/userini-activatequest`,
`proofs/0042F491-init-game-callees`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Host hoists `00419D90` before `0041735A`? | **Yes.** `EnterGame` notes `00419D90` on the `"Init World"` loop arm **before** `Note(0x0041735A)`. | **PROVEN** |
| Native order? | **`0041735A` first**, then later `00419D90`. Only `E8` to `0041735A` is `00418784`. Only `E8` to `00419D90` is `004A712B` inside `004A6E30` (`[world].vtbl+36` from `00417410`). | **PROVEN** |
| What is `00419D90`? | `"Init Global Console"` **register**: alloc 24, name `"ActivateQuest"`, `[cmd+20]=00419CE0`, `009EC5E0` into `[0x13CAA40]`. **Not** the live activate. | **PROVEN** |
| Leftover? | **Yes.** Host note is a **hoist** (wrong parent, wrong order). `InitWorldInitStages` **omits** `"Init Global Console"`. `DispatchUserIniCommand` re-notes `00419D90` as if it were the apply. | **PROVEN** leftover / **DIVERGE** |
| `00419D90` is a `004184BD` sibling before Init World? | **No.** It is a grandchild of `0041735A`. | **DISPROVEN** |
| `00419D90` is the `user.ini` activate call? | **No.** Handler is `00419CE0` (zero `.text` `E8`). | **DISPROVEN** |
| Oakvale / `00DBDE40` on this site? | **No.** | **DISPROVEN** |

---

## Verdict

**Native: `0041735A` then `00419D90`. Host: the reverse note.**

`00419D90` is the ActivateQuest **command registrar**
under `"Init Global Console"`. It runs only after
Init World has already constructed the world
(`004A67D0` → `game+36`) and walked nav / flyer
inside `004A6E30`. Host `EnterGame` emits that VA
as a pre-`0041735A` Ini note. That is leftover
label placement, not a native sibling.

---

## 1. Native order (dump)

`e8.tsv`:

```
0x00418784  0x0041735A     // unique
0x004A712B  0x00419D90     // unique
```

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

`0041735A` owns the world object, then vtbl+36:

```
0041737E  push 0x198
00417396  call 004A67D0          ; world → game+36
004173E1  push "Init World Init"
0041740C  mov ecx, [edi]         ; world
00417410  call [eax+36]          ; 004A6E30
```

`listing-00480000.txt` — `00419D90` is **after**
`"Init World Map"` / Environment / nav / flyer:

```
004A6E30  … "Init World Map" …
004A7103  push "Init Global Console"
004A712B  call 00419D90
004A7132  push "Adding Console Commands"
004A715C  push "Init Combat Manager"
```

So native first-seen New Game:

```
004184BD
  "Init World" 0041735A
    004A67D0
    [world].vtbl+36 004A6E30
      … nav / flyer …
      "Init Global Console" 00419D90   // HERE
      "Adding Console Commands"
      "Init Combat Manager" …
  "Init Display Engine" 00417418
```

`00419D90` **cannot** precede `0041735A`.

---

## 2. What `00419D90` is

`listing-00400000.txt`:

```
00419D90  sub esp, 16
00419D97  push 24
00419D9D  call 00BFEA1A
00419DAE  push "ActivateQuest"        ; xrefs 0x0122F380
00419DD6  mov [esi], 0x122E5B0
00419DE5  mov [esi], 0x122E638
00419DF0  mov [esi], 0x122E65C
00419DF6  mov [esi+20], 0x419CE0      ; handler
00419E02  mov eax, [0x13CAA40]
00419E39  call 009EC5E0               ; insert command
00419E69  ret
```

Live apply is the sibling `00419CE0` (game
`vtbl+36` gate `004197B0`, then `[world+56]`
vtbl+1104 `00892E80`). That path is after
`00416953` / `user.ini` (`00418969`), not at
register time. **PROVEN** in
`proofs/ini-activate-quest`.

---

## 3. Host leftover

`EnterGame` (`EngineLifecycle.cs`):

```
foreach InitGameStages:
  if name == "Init World":
    Note(00419D90, "00419D90 ActivateQuest")   // HOIST
  Note(apply)                                  // 0041735A
…
Note(0041735A, "004A67D0")                     // LATE
Note(004A6E30)
foreach InitWorldInitStages                    // no 00419D90
```

`InitWorldInitStages` starts at World Map /
Environment / Nav. It never names
`"Init Global Console"`. The registrar is
therefore **not** noted at `004A712B`.

`DispatchUserIniCommand("ActivateQuest")`
notes `00419D90` again at `user.ini` apply.
That is the **handler** site (`00419CE0`).
Register has already (natively) run inside
`004A6E30`.

| Host | Native | Class |
|---|---|---|
| Note `00419D90` before `0041735A` | `004A712B` after `0041735A` / `004A67D0` / nav | **DIVERGE** hoist |
| `InitWorldInitStages` skips Global Console | `004A712B` child 8 of `004A6E30` | **LEFTOVER** omit |
| `user.ini` note `00419D90` | apply is `00419CE0` | **LEFTOVER** label |
| Named `InitGameStages` `"Init World"` → `"Init Display Engine"` | same strings | **MATCH** notes |

---

## 4. What this does **not** say

- Moving the host **note** is not the same as
  constructing the world on time. World ctor
  late vs `00417418` is a **separate** leftover
  (`proofs/initgame-after-leave-order`).
- First-seen TLC `user.ini` still activates
  **Gameflow** via `00419CE0` after Load World.
- Do not invent `ActivateQuest(Q_NewOakValeIntro)`
  from this VA.

---

## Paths

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00480000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
- `tools/Fable.ExeIndex/out/00-index/xrefs.tsv`
- `src/Fable.Game/EngineLifecycle.cs` (notes only)
