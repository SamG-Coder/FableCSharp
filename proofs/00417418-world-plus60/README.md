# `00417418` `[game+36]` / `[world+60]` after world is inside Init World

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld`
→ Init Game `004184BD`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: after moving `004A67D0` / `004A6E30` inside
Init World, `00417418` Init Display reads `[game+36]` /
`[world+60]`. Confirm those are the world object and
mesh bank from `004A6E30`. Host `00434E10` leftover if
any remains?

Authority: Fable.exe dump
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041735A` / `00417418` / `00418757`–`004187E2` /
`00418DCA` / `00419270` / `004193E8` / `0041940C` /
`00434E10`);
`listing-00480000.txt` (`004A67D0` / `004A6E30` /
`0049E620` / `004A750B`);
`e8.tsv` dests `0041735A` / `00417418` / `00434E10` /
`004A67D0` / `0049E620`;
host notes only:
`src/Fable.Game/EngineLifecycle.cs` `EnterGame` /
`InitGameStages` / `InitWorldInitStages` /
`DisplayCtorFn` / `OpenMeshBank`;
siblings `proofs/00434E10-first-args`,
`proofs/init-world-004A6E30`,
`proofs/004A67D0-after-particles`,
`proofs/meshbank-after-leave`.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| `[game+36]` at `00417418`? | Live **world** from `"Init World"` `0041735A`: alloc `0x198` → `004A67D0` (vtbl `012390F0`) → `004193E8` store at `esi+36`. Blob `+12`. | **PROVEN** |
| Is that pointer **from** `004A6E30`? | **No.** `004A6E30` is `[world].vtbl+36` on the object already at `game+36`. It does not write `game+36`. | **DISPROVEN** as producer |
| `[world+60]` at `00417418`? | **Mesh bank** written inside `004A6E30` by only `E8` `004A750B` → `0049E620` (`"Init Mesh Bank"` / `"MBANK_ALLMESHES"` / `00A09F20`). Blob `+32`. Ctor zeros `+60` first. | **PROVEN** (object); heap VA **UNREAD** |
| Native no-world / null-`+60` `00417418`? | **No.** `[esi+36]` then `[eax+60]` has no test. | **DISPROVEN** |
| Host `00434E10` leftover after the move? | **Order leftover is gone.** Note still skips the `0x100` object and the six-dword blob (`display+248` / `+24` never stored). | **MATCH** order; **LEFTOVER** object |

---

## Verdict

**`[game+36]` is the `004A67D0` world. `[world+60]` is
the `004A6E30` mesh bank.** After the host moved those
VAs inside `"Init World"`, `EnterGame` notes them
**before** `00417418` / `00434E10` (**MATCH** native
order). Host `00434E10` leftover that **remains** is
the missing display object, not the old hoist past
particles.

| Claim | Status |
| --- | --- |
| `e8.tsv` dest `00417418` is only `004187E2` | **PROVEN** |
| `e8.tsv` dest `00434E10` is only `004174A6` | **PROVEN** |
| `e8.tsv` dest `004A67D0` is only `00417396` | **PROVEN** |
| `e8.tsv` dest `0049E620` is only `004A750B` | **PROVEN** |
| Native: `0041735A` → `004A67D0` → `004A6E30` → `00417418` | **PROVEN** |
| Blob `+12` = `[game+36]` world | **PROVEN** |
| Blob `+32` = `[world+60]` mesh bank from `0049E620` | **PROVEN** |
| `004A6E30` stores the world at `game+36` | **DISPROVEN** |
| First-seen `[world+60]` is still the ctor zero | **DISPROVEN** (overwritten before `00417418`) |
| Native first-seen world / mesh bank are null | **DISPROVEN** |
| Host notes `004A67D0` / `004A6E30` after particles | **DISPROVEN** (stale; now inside Init World) |
| Host `Note(00434E10)` after those notes | **MATCH** |
| Host `0x100` alloc / blob / `display+248` / `+24` | **LEFTOVER** skip |
| Host `DisplayPlus232=0x1E` | **MATCH** the write; object **PARTIAL** |
| Oakvale / `00DBDE40` on this site | **DISPROVEN** |

---

## 1. Native order (`004184BD`)

`listing-00400000.txt`:

```
00418757  push "Init World"
00418784  call 0041735A
00418789  mov eax, [esi+36]
00418790  mov [eax+320], cl
00418796  push "Init Display Engine"
004187E2  call 00417418
```

`0041735A` (`ecx` = game):

```
0041737E  push 0x198
00417386  call 00BFEA1A
00417396  call 004A67D0
0041739F  lea edi, [esi+36]
004173A5  call 004193E8          ; [game+36] = world
004173E1  push "Init World Init"
0041740C  mov ecx, [edi]         ; world
00417410  call [eax+36]          ; 004A6E30
00417417  ret
```

`004A67D0` writes vtbl `012390F0` and
`mov [esi+60], ebx` / `mov [esi+64], ebx`
(`004A6822`). `+60` is empty until mesh-bank
open.

---

## 2. `004A6E30` writes `[world+60]`

`esi` = world. After `"Init Mesh Bank Manager"`
(log only):

```
004A74E1  push "Init Mesh Bank"
004A7509  mov ecx, esi
004A750B  call 0049E620
004A753A  mov ecx, [esi+60]
004A753D  call 00AEAA90
```

`0049E620` (`ecx` = world):

```
0049E62A  push "Opening Mesh Bank"
0049E655  push "MBANK_ALLMESHES"
0049E679  call 00A09F20
0049E67E  mov edi, [eax+4]
0049E681  mov ebx, [eax]
0049E6AC  mov [esi+60], ebx
0049E6AF  mov [esi+64], edi
0049E6F5  mov edx, [esi+60]
0049E6FE  mov [esi+68], [edx+960]
0049E72C  mov ecx, [esi+60]
0049E72F  call 004BBFD0
```

That is the only `.text` `E8` of `0049E620`.
First-seen pointer **value** is heap — **UNREAD**
without a live trace. The **object** is that
open, not 0, when `004A6E30` returns.

---

## 3. `00417418` reads both, then `00434E10`

`ecx` = game. `00419270` zeros 36 bytes at
`[ebp-44]` (`+28=0xFFFFFFFF`).

```
0041742F  mov eax, [esi+36]     ; world
00417435  mov [ebp-32], eax     ; blob+12
…
00417485  mov eax, [esi+36]
00417488  mov [ebp-44], esi     ; blob+0 = game
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

`00434E10` `ret 4`. Consumes:

| Blob | Source (first-seen) | Display |
| ---: | --- | ---: |
| `+0` | game | `+8` |
| `+4` | `0044C6B0` | `+16` |
| `+8` | `[game+28]` | `+12` |
| `+12` | `[game+36]` `004A67D0` world | `+248` |
| `+16` | `[game+90568]` `GBANK_MAIN` | `+20` |
| `+32` | `[world+60]` `0049E620` mesh bank | `+24` |

Also: vtbl `01231574`, `[+4]=1`, `[+232]=0x1E`.
Blob `+20/+24/+28` are filled in `00417418` and
**never loaded** by the ctor.

---

## 4. Host leftover that remains

`EnterGame` now nests world under `"Init World"`:

```
foreach InitGameStages:
  Note(apply)                         // 0041735A then 00417418
  if "Init Display Engine":
    DisplayPlus232 = 0x1E
    Note(00434E10)
  if "Init World":
    Note(004A67D0)
    Note(004A6E30)
    InitWorldInitStages → OpenMeshBank
```

| Host | Native | Class |
| --- | --- | --- |
| `004A67D0` / `004A6E30` / mesh bank before `00417418` | same | **MATCH** |
| `Note(00434E10)` on `"Init Display Engine"` | only `E8` `004174A6` | **MATCH** site |
| `DisplayPlus232=0x1E` | ctor `00434F1F` | **MATCH** immediate |
| `00BFEA1A(0x100)` + blob + `0041940C` | `00417496` / `004174A6` / `004174B5` | **LEFTOVER** skip |
| `display+248` / `display+24` | world / mesh bank | **LEFTOVER** omit |
| `00419D90` note before `0041735A` | child of `004A6E30` | **LEFTOVER** hoist (other proof) |

`proofs/00434E10-first-args` §5 (“world ctor after
particles”) is **stale** on order. The missing
`0x100` object claim there still holds.

`OpenMeshBank` on `PresentWorld` /
`ExpandPresentedWorld` is a later second open
guarded by `Meshes.Opened` — not a second native
`0049E620` site.

---

## 5. What this does **not** say

- `004A6E30` allocates the world. **DISPROVEN**
  (`00417386` / `004A67D0`).
- `[world+60]` is WorldCamera / navigator.
  **DISPROVEN** (`+24` / `+72`).
- Host holds a display whose `+248` / `+24` match
  native. **DISPROVEN** (notes only).
- Exact heap VA of first-seen mesh bank.
  **UNREAD**.
- Oakvale / `00DBDE40` from this read.
  **DISPROVEN**.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Game\MeshBank.cs`
- `C:\FableCSharp\proofs\00434E10-first-args\README.md`
- `C:\FableCSharp\proofs\init-world-004A6E30\README.md`
