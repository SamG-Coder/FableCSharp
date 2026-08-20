# Host leftover at `"Init Display Engine"` `00434E10`

Investigation only. No production `src/` / `tests/`
edits. Do **not** start Oakvale / `00DBDE40` /
`StartOakVale`. No-save New Game is Leave
`0042F2A2` → `FinalAlbion.wld` → Init Game
`004184BD`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `proofs/00434E10-first-args` already
**PROVED** `00434E10` is `thiscall` on a new
`0x100` display + 36-byte blob (game,
player-manager, world at blob+12, mesh bank at
blob+32). Host `EnterGame` only sets
`DisplayPlus232=0x1E` and Notes `00434E10`.
What is the **smallest MATCH slice still
leftover** (`0x100` alloc / blob fields /
`display+248` world / `display+24` mesh bank)?
Confirm current `EngineLifecycle` still omits
the object.

Authority: existing proof
`proofs/00434E10-first-args` (blob / ctor
stores; **do not re-prove** unless host
changed); sibling
`proofs/00417418-world-plus60` (order already
moved inside Init World); host notes only:
`src/Fable.Game/EngineLifecycle.cs`
`DisplayCtorFn` / `DisplayPlus232` /
`EnterGame` Init Display Engine arm;
dump `listing-00400000.txt` `00417418` /
`00434E10` / `0041940C`. Test
`Init_World_004A67D0_runs_inside_0041735A_before_00417418`
asserts **order only**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Host still omits the `0x100` object? | **Yes.** `"Init Display Engine"` only `DisplayPlus232 = 0x1E` + `Note(00434E10)`. No `00BFEA1A(0x100)`, no blob, no `0041940C`, no `display+248` / `+24`. | **PROVEN** leftover omit |
| Order leftover vs Init World? | **Gone.** `InitGameStages` is World `0041735A` then Display `00417418`. The World arm notes `004A67D0` / `004A6E30` and `OpenMeshBank` **before** the next-iteration ctor note. | **MATCH** |
| `00434E10-first-args` §5 “world after particles”? | **Stale** on order (same as `00417418-world-plus60`). Object-omit claim there still holds. | **DISPROVEN** as current host order |
| Blob re-proved? | **No.** Host did not change blob fill or ctor stores. Authority remains `00434E10-first-args` §2–3. | **PROVEN** unchanged |
| Smallest leftover MATCH slice? | **`00BFEA1A(0x100)` + the six ctor-consumed blob dwords + `0041940C` `[game+40]`.** Named fields: `display+248` = world (`blob+12`), `display+24` = mesh bank (`blob+32`). | **PROVEN** leftover |
| Unused blob `+20/+24/+28`? | Filled by `00417418`, **never loaded** by `00434E10`. Host omit is **MATCH** skip. | **PROVEN** unused |
| `004350D0` / `00A0BF20` / other ctor zeros? | Later / same-ctor extras. Not the smallest slice. | **LEFTOVER** out of scope |

---

## Verdict

**Order is MATCH. Object is leftover.**

Native first-seen `00434E10` still needs a live
world at `[game+36]` and a live mesh bank at
`[world+60]`. Host now produces those notes
inside `"Init World"` **before** the ctor
note. That is the only host change since
`00434E10-first-args`. The ctor **this** is
still missing: no `0x100` display, no six
dword stores, no `[game+40]`.

Smallest MATCH work left on this site (after
Init World, on `"Init Display Engine"`):

1. `00BFEA1A(0x100)` — `this`
2. 36-byte blob pointer (`lea [ebp-44]`)
3. Six ctor copies (table below)
4. Keep existing `+232=0x1E` / vtbl
   `01231574` / `[+4]=1`
5. `0041940C` → `[game+40]`

Do **not** implement blob `+20/+24/+28`.
Do **not** grow this slice into `004350D0`
“Init Engine”, viewport `00A0BF20`, or
pump `00434F60` (already a `+232`
decrement).

---

## MATCH vs leftover

| Host | Native | Class |
| --- | --- | --- |
| `InitGameStages` name `"Init World"` then `"Init Display Engine"` | `00418784` then `004187E2` | **MATCH** |
| World arm: `Note(004A67D0)` / `Note(004A6E30)` / `OpenMeshBank` | `0041735A` → `004A67D0` → `004A6E30` `"Init Mesh Bank"` `0049E620` | **MATCH** order |
| `Note(0x00417418)` via stage apply | only `E8` `004187E2` | **MATCH** site |
| `Note(00434E10)` | only `E8` `004174A6` | **MATCH** site |
| `DisplayPlus232 = 0x1E` | ctor `00434F1F` | **MATCH** immediate; object **PARTIAL** |
| `DisplayVtbl = 01231574` in the note string | `mov [esi], 0x1231574` | **MATCH** constant; no store |
| `00BFEA1A(0x100)` | `0041748E` / `00417496` | **LEFTOVER** skip |
| 36-byte blob at `[ebp-44]` | `00419270` then six fills | **LEFTOVER** skip |
| `display+8/+16/+12/+248/+20/+24` | ctor copies | **LEFTOVER** omit |
| `0041940C` `[game+40]` | `004174AF` / `004174B5` | **LEFTOVER** skip |
| Blob `+20/+24/+28` (device `+44`) | filled, ctor unread | **MATCH** omit |
| `00419D90` note before `0041735A` | child of `004A6E30` | **LEFTOVER** hoist (other proof) |

`EngineLifecycle` has `DisplayPlus232` /
`DisplayPlus104` (pump). It has **no**
`DisplayObjectSize=0x100`, no
`display+248`, no `display+24`, no
`game+40` display pointer.

---

## 1. Current host (read only)

`EnterGame` `InitGameStages`:

```
("Init World",           0x0041735A),
("Init Display Engine",  0x00417418),
```

Loop body (trimmed):

```
Note(apply)                         // 0041735A then 00417418
if "Init Display Engine":
    DisplayPlus232 = 0x1E
    Note(00434E10)                  // vtbl 01231574 +232=0x1E
if "Init World":
    Note(004A67D0)
    Note(004A6E30)
    InitWorldInitStages → OpenMeshBank
```

Proposed MATCH order vs Init World — **keep
this sequence**. Native cannot call
`00434E10` without `[esi+36]` / `[eax+60]`.
Host already notes those producers first.
Next edit belongs **inside** the Display
arm, not a second world hoist.

```
"Init World"
    0041735A / 004A67D0 / 004A6E30
    OpenMeshBank                     // world+60
"Init Display Engine"
    00417418
    00BFEA1A(0x100)
    blob +12=world +32=mesh bank     // +0/+4/+8/+16 too
    00434E10                         // +248 / +24 / +232=0x1E
    0041940C [game+40]
```

`00419D90` stays a separate leftover hoist
(`proofs/00419D90-hoist`). Not this object.

---

## 2. Native site (not re-proved)

`listing-00400000.txt` `00417418`:

```
0041742F  mov eax, [esi+36]     ; world
00417435  mov [ebp-32], eax     ; blob+12
…
0041748B  mov eax, [eax+60]     ; mesh bank — no test
0041748E  push 0x100
00417493  mov [ebp-12], eax     ; blob+32
00417496  call 00BFEA1A
004174A0  lea ecx, [ebp-44]
004174A3  push ecx
004174A4  mov ecx, eax
004174A6  call 00434E10
004174AF  add esi, 40
004174B5  call 0041940C          ; [game+40] = display
```

Ctor stores that are still leftover on host
(`00434E10-first-args` §3; dump `00434E2E`–
`00434F36`):

| Blob | First-seen source | Display | Host |
| ---: | --- | ---: | --- |
| `+0` | game | `+8` | **LEFTOVER** |
| `+4` | `0044C6B0` `[0x13B879C]` | `+16` | **LEFTOVER** |
| `+8` | `[game+28]` `0044A3B0` | `+12` | **LEFTOVER** |
| `+12` | `[game+36]` world | `+248` | **LEFTOVER** (named) |
| `+16` | `[game+90568]` `GBANK_MAIN` | `+20` | **LEFTOVER** |
| `+32` | `[world+60]` mesh bank | `+24` | **LEFTOVER** (named) |
| `+20/+24/+28` | device `+44` | unread | **MATCH** skip |

Also MATCH-as-constant (already on host):
vtbl `01231574`, `[+4]=1`, `[+232]=0x1E`.

`0041940C` is `mov [ecx], edx` with
`ecx = game+40`, `edx = display`. First-seen
slot is ctor-zero → no release.

---

## 3. What this does **not** say

- Host still notes `00434E10` before
  `004A67D0`. **DISPROVEN** (stale
  `00434E10-first-args` §5).
- Native no-world / null-`+60` ctor.
  **DISPROVEN** (`00434E10-first-args`).
- Blob `+20/+24/+28` belong in the smallest
  MATCH slice. **DISPROVEN**.
- `004350D0` / fade dest / layer bits are
  this leftover. **DISPROVEN** (later).
- Exact heap VA of first-seen mesh bank.
  **UNREAD**.
- Oakvale / `00DBDE40` on this site.
  **DISPROVEN**.

---

## Sources

- `C:\FableCSharp\proofs\00434E10-first-args\README.md`
- `C:\FableCSharp\proofs\00417418-world-plus60\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs`
  (`Init_World_004A67D0_runs_inside_0041735A_before_00417418`)
