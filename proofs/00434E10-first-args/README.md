# First-seen `00434E10` args from `"Init Display Engine"` `00417418`

Investigation only. No production `src/` edits.

Question: `00417418` Init Display Engine reads
`[game+36]` / `[world+60]` then `00434E10`. What
are those args first-seen? Host runs `00434E10`
with no world — leftover?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`00417418` / `004184BD` `004187E2` / `00416C8A` /
`0041732A` / `0041735A` / `00419270` / `0041940C` /
`00415BF0` / `00434E10` / `004350D0`);
`listing-00440000.txt` (`0044C6B0`);
`listing-00480000.txt` (`0049E620` / `004A750B`);
`listing-009c0000.txt` (`009F2F90` / `009F2F60`);
`e8.tsv`; `functions.tsv`;
`src/Fable.Game/EngineLifecycle.cs`
(`EnterGame` / `InitGameStages` / `DisplayCtorFn`);
`src/Fable.Game/MeshBank.cs`;
`proofs/initgame-after-leave-order`;
`proofs/init-world-004A6E30`;
`proofs/0042F491-init-game-callees`.

Do not re-prove `004A67D0` child order except as
the producer of `game+36`. Do not invent dest pixels.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| First-seen `00434E10` site? | Only `.text` `E8`: `004174A6` inside `00417418`. Parent of `00417418` is only `004187E2` in `004184BD`. | **PROVEN** |
| Calling convention / stack args? | `thiscall` + **one** stdcall pointer. `ret 4`. `this` = `00BFEA1A(0x100)`. Arg = `lea [ebp-44]` blob. | **PROVEN** |
| First-seen `this` (`ecx`)? | New `0x100` display. Ctor writes vtbl `01231574`, `[+4]=1`, `[+232]=0x1E`. Then `0041940C` → `game+40`. | **PROVEN** |
| First-seen `[game+36]` in the blob? | Live **world** from `"Init World"` `0041735A` / `004A67D0` (`0x198`, vtbl `012390F0`). Blob `+12`. Ctor copies to `display+248`. | **PROVEN** |
| First-seen `[world+60]` in the blob? | **Mesh bank** from `004A6E30` `"Init Mesh Bank"` `0049E620` (`MBANK_ALLMESHES` via `00A09F20`). Blob `+32`. Ctor copies to `display+24`. | **PROVEN** (object); heap VA **UNREAD** |
| Other blob fields the ctor copies? | `+0` game → `display+8`; `+8` `[game+28]` (`0044A3B0`) → `+12`; `+4` `0044C6B0` (`[0x13B879C]`) → `+16`; `+16` `[game+90568]` (`GBANK_MAIN` from `00416C8A`) → `+20`. | **PROVEN** |
| Blob `+20/+24/+28` (device `+44` `009F2F90` / `00415BF0` / `009F2F60`)? | Filled first-seen. **`00434E10` never reads them.** | **PROVEN** unused by ctor; COM method **PARTIAL** |
| Native no-world `00434E10`? | **No.** `[esi+36]` then `[eax+60]` has no null test. World-less call is not a path. | **DISPROVEN** |
| Host `EnterGame` notes `00434E10` before `004A67D0`? | **Yes.** Named stage loop only sets `DisplayPlus232=0x1E`. No `0x100` alloc, no blob. World ctor is after particles. | **LEFTOVER** note / **DIVERGE** order |

---

## Verdict

**First-seen `00434E10` is `thiscall` on a new
`0x100` display plus one pointer to a 36-byte
init blob.** `[game+36]` is the world already
stored by `0041735A`. `[world+60]` is the mesh
bank already stored by `0049E620`. Host’s
world-less `00434E10` note is leftover.

| Claim | Status |
| --- | --- |
| `e8.tsv` dest `00434E10` is only `004174A6` | **PROVEN** |
| `e8.tsv` dest `00417418` is only `004187E2` | **PROVEN** |
| First-seen after Leave, after `"Init World"` | **PROVEN** |
| Blob `+12` = `[game+36]` world | **PROVEN** |
| Blob `+32` = `[world+60]` mesh bank | **PROVEN** |
| Ctor stores world at `display+248`, mesh bank at `display+24` | **PROVEN** |
| Native first-seen world / mesh bank are null | **DISPROVEN** |
| Host `00434E10` with no world is a native path | **DISPROVEN** |
| Host early `Note(DisplayCtorFn)` | **LEFTOVER** |
| Host world ctor after `"Load Particles"` | **DIVERGE** (`initgame-after-leave-order`) |
| Host `+232=0x1E` constant | **MATCH** the write; object **PARTIAL** |

---

## 1. Only site

`e8.tsv`:

```
0x004187E2  0x00417418
0x004174A6  0x00434E10
```

`004184BD` at `004187E0`:

```
00418782  call 0041735A          ; Init World — game+36
00418789  mov  eax, [esi+36]
00418790  mov  [eax+320], cl
00418796  push "Init Display Engine"
…
004187E0  mov  ecx, esi          ; game
004187E2  call 00417418
```

No frontend / no-save / save fork. First-seen is
this no-save New Game walk after Leave.

---

## 2. `00417418` fills one blob, then constructs

`ecx` = game. `00419270` zeros a 36-byte record
at `[ebp-44]` (`+28=0xFFFFFFFF`, rest 0).

```
0041742F  mov  eax, [esi+36]     ; world
00417435  mov  [ebp-32], eax     ; blob+12
…
0041746E  call 0044C6B0          ; [0x13B879C]
00417473  mov  [ebp-40], eax     ; blob+4
00417476  mov  eax, [esi+28]     ; 0044A3B0
00417479  mov  [ebp-36], eax     ; blob+8
0041747C  mov  eax, [esi+90568]  ; GBANK_MAIN
00417482  mov  [ebp-28], eax     ; blob+16
00417485  mov  eax, [esi+36]
00417488  mov  [ebp-44], esi     ; blob+0 = game
0041748B  mov  eax, [eax+60]     ; mesh bank — no test
0041748E  push 0x100
00417493  mov  [ebp-12], eax     ; blob+32
00417496  call 00BFEA1A
004174A0  lea  ecx, [ebp-44]
004174A3  push ecx
004174A4  mov  ecx, eax
004174A6  call 00434E10
004174AF  add  esi, 40
004174B5  call 0041940C          ; [game+40] = display
004174E7  mov  ecx, [esi]
004174E9  call 004350D0          ; "Init Engine"
```

`[esi+36]` then `[eax+60]` is unguarded. A null
world is a crash, not a ctor variant.

Device `[0x13B8390]+44` also fills blob `+20`
(`009F2F90` first dword), `+24` (`00415BF0` =
`009F2F90` second dword), `+28` (`009F2F60` /
`009E3830` string first dword). Scratch at
`ebp-8` / `ebp-4` is not part of the 36-byte
record. **`00434E10` does not load `+20/+24/+28`.**

---

## 3. `00434E10` consumes six dwords

`ret 4`. `edi` = stack arg. After `sub esp,24`
and three pushes, `[esp+40]` is that pointer.

| Blob | Source (first-seen) | Display |
| ---: | --- | ---: |
| `+0` | game (`00418DCA` / `[0x13B86A0]`) | `+8` |
| `+4` | `0044C6B0` → `[0x13B879C]` | `+16` |
| `+8` | `[game+28]` `0041732A` / `0044A3B0` | `+12` |
| `+12` | `[game+36]` `004A67D0` world | `+248` |
| `+16` | `[game+90568]` `00416C8A` `GBANK_MAIN` | `+20` |
| `+32` | `[world+60]` `0049E620` `MBANK_ALLMESHES` | `+24` |

Also: vtbl `01231574`, `[+4]=1`, `[+232]=0x1E`,
`00A0BF20` at `display+48` (viewport-ish;
`proofs/dx9-3d-submit`). Those are not the
`[game+36]` / `[world+60]` args.

---

## 4. Producers run before this `E8`

| Field | Writer | When vs `004174A6` |
| --- | --- | --- |
| `game+36` | `0041735A` `004A67D0` + `004193E8` | same `004184BD`, previous named stage |
| `world+60` | `004A6E30` `004A750B` `0049E620` | inside that stage, `"Init Mesh Bank"` |
| `game+28` | `0041732A` | earlier named stage |
| `[0x13B879C]` | `0044C6C2` if `0044C6B6` saw 0 | earlier in `004184BD` |
| `game+90568` | `00416C8A` `add esi, 0x161C8` + `004194BA` | `"Init Graphics"` / `"Opening Main Graphic Bank"` |

`00418DCA` zeros `game+90568` (`00418EB6`).
Init Graphics overwrites it **before** Display.

`0049E620` (`MeshBank.OpenFn`): `"Opening Mesh
Bank"` / `"MBANK_ALLMESHES"` / `00A09F20`, then
`[world+60]=ebx` `[world+64]=edi`. First-seen
mesh-bank **pointer value** is heap — **UNREAD**
without a live trace. The **object** is that
open, not 0, on the success path the host also
notes (`OpenMeshBank`).

---

## 5. Host leftover

`EnterGame` `InitGameStages` hits `"Init Display
Engine"` **inside** the name loop:

```
DisplayPlus232 = 0x1E;
Note(00434E10, "00434E10 vtbl 01231574 +232=0x1E");
```

`Note(004A67D0)` / `Note(004A6E30)` / mesh-bank
open run **after** the loop (after particles).
No `0x100` alloc. No blob. No `display+248` /
`+24` stores.

That is not a second native site. It is the
same leftover hoist as
`proofs/initgame-after-leave-order`: native
world exists **before** `00434E10`; host notes
the ctor **without** it.

`+232=0x1E` **MATCH**es the immediate in the
ctor. The host does not hold the display
object those six pointers land on (**PARTIAL**).

---

## 6. What this does **not** say

- `00434E10` is also a frontend ctor.
  **DISPROVEN** — one `E8`.
- First-seen `[world+60]` is still the
  `00419270` zero. **DISPROVEN** — overwritten
  after `004A6E30` returns.
- Blob `+20/+24/+28` are ctor inputs.
  **DISPROVEN**.
- `0044C6B0` and `[game+28]` are the same
  pointer. **DISPROVEN** — singleton vs
  `0044A3B0` size 44.
- Exact COM name of `[device+44].vtbl+48`.
  **UNREAD** / **PARTIAL**.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-009c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Game\MeshBank.cs`
- `C:\FableCSharp\proofs\initgame-after-leave-order\README.md`
- `C:\FableCSharp\proofs\init-world-004A6E30\README.md`
