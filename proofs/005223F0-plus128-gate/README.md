# `005223F0` `[thing_manager+128]` first-seen writer

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld` →
Loading world `004A1840` → `00507C30`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

Question: `004FDBC0` constructs Things only if `005223F0`
`[thing_manager+128]==1`. What writes `+128` first-seen after
Leave / Loading world / `00507C30`? First-seen value after
Init Game? Host `LoadGlobalThingsFile` skip **MATCH** or
**leftover**?

Authority: Fable.exe dump (`listing-00480000.txt`
`0049EBF0` / `0049E1B0` / `0049D970` / `004A6E30` /
`004A67D0`; `listing-00500000.txt` `005223F0` / `00523540` /
`00521AE0` / `00507C30`; `listing-004c0000.txt` `004FBF60` /
`004FE030`; `listing-00400000.txt` `AllowDataGeneration`
`0x1375459`).
`proofs/host-tng-construct-early`, `proofs/004FDBC0-open`.

---

## Verdict

**Ctor writes `1`. Nothing on the first-seen New Game path
rewrites it before `004FDBC0`. Host skip is leftover.**

`CThingManager` `00523540` (`0049EBF0` Init Thing Manager,
inside Init World, after Leave, **before** Loading world)
does `mov [esi+128], 0x1`. The only other manager-`+128`
writer in this tree is `004FE030` (“Compiling global
things”), and that arm is **skipped** (`[0x1375459]==0`).
`00507C30` / `004FDBC0` therefore see **leftover `1`**.
`005223F0` takes `00521AE0`. Host
`LoadGlobalThingsFile` does not `LoadSingleThing` —
**LEFTOVER**, not MATCH.

Do not confuse **world** `+128` (`0049D970` loaded flag)
or **world-map** `+128` (list cursor) with this field.

| Claim | Class |
|---|---|
| `005223F0` runs `00521AE0` / `0051E2F0` iff `[manager+128]==1` | **PROVEN** |
| Object is `CThingManager` at `[world+80]` (map `+8` → world `vtbl+12` `0049E1B0`) | **PROVEN** |
| First-seen **writer** after Leave is ctor `00523540` `=1` | **PROVEN** |
| That write is during Init World / Init Game, **before** Loading world / `00507C30` | **PROVEN** |
| First-seen writer **inside** `00507C30` / `004FDBC0` | **none** (**PROVEN** skip of `004FE030`) |
| First-seen **value** at first `005223F0` | leftover **`1`** (**PROVEN** dump-static; live RAM **UNREAD**) |
| First-seen value after Init Thing Manager / after Init Game suffix | still **`1`** (**PROVEN** dump-static) |
| `004FE030` save / force `1` / restore | **PROVEN** dump; first-seen **SKIP** (`AllowDataGeneration`) |
| Host `LoadGlobalThingsFile` skip vs first-seen `+128==1` | **LEFTOVER** (**DISPROVEN** as MATCH) |
| Host skip vs `00416392==0` after LoadWorld | **PARTIAL** (`+24` count, not the gate) |
| Earlier “parse-only / MATCH skip” on this VA | **LEFTOVER** (gate was **UNREAD**; writer is now known) |

---

## Gate (`listing-00500000`)

```
005223F0  sub esp, 28
          esi = ecx                    // CThingManager
005223F7  mov eax, [esi+128]
005223FF  cmp eax, 1
00522407  jne 00522502                 // drop shared_ptr, ret 12
          …
0052249F  call 00521AE0                // Thing Manager: Load From File
005224AB  call 0051E2F0                // Activate Things
0052251F  ret 12
```

Caller is `004FBF60` (`listing-004c0000`) after the `.tng`
open:

```
004FC019  mov ecx, [edi+8]             // CWorldMap+8 = CWorld
004FC01C  mov edx, [ecx]
004FC01E  call [edx+12]                // 0049E1B0 → [world+80]
004FC021  mov ecx, eax
004FC023  call 005223F0
```

`005066E0` stores `map+8 = world` (`004A6EB4` `push esi`).
`0049E1B0` is `mov eax, [ecx+80]; ret`. Same pointer
`0049EBF0` just stored.

`00521AE0` remaps **mode 3 only** through the same dword
(`00521B06`: `1→mode 1`, `2→mode 0`). The `004FDBC0` /
`005223F0` call is **not** the `006C2170` `push 3` site.
The **enter/skip** test is still `cmp [esi+128], 1`.

---

## Who writes `CThingManager+128`

### 1. Ctor `00523540` — first-seen, value **1**

`004A6E30` child 12 (`proofs/init-world-004A6E30`):

```
004A71E2  "Init Thing Manager"
004A7230  call 0049EBF0                // ecx = world
```

```
0049EBF1  push 0xE8
0049EBF8  call 00BFEA1A
          …
0049EC2E  call 00523540                // ret 36
0049EC3D  mov [esi+80], eax            // world+80
```

Only `E8` to `00523540` in the text map.

```
0052355A  mov [esi], 0x1245C44         // CThingManager vtbl
          …
005235CD  mov [esi+128], 0x1
00523695  mov [esi+132], ebx           // 0
0052369B  mov [esi+136], ebx           // 0
005236C7  mov [esi+124], 0x3C
```

RTTI `CThingManager` `0x0137B970`. Nearby `+124/+132/+136`
are other manager fields; thing-object `+128` list links
(`0051DF80` / `004C90DE`) are **not** this dword.

### 2. `004FE030` — not first-seen

After the WLD token loop (`0050938F` back-edge ends):

```
005093A7  mov al, [0x1375459]          // AllowDataGeneration
005093AC  test al, al
005093AE  je 005094C9                  // first-seen TAKEN
00509497  call 004FE030                // skipped
```

`0x1375459` is the `AllowDataGeneration` console byte
(`0041440D` / `0041443D`). No first-seen store; BSS **0**.
`00403376` skips `00418C3B` on the same byte.

If taken, `004FE030`:

```
004FE048  save  [manager+128] → [esp+36]
004FE05F  mov [eax+128], 1
          "Compiling global things"
          per prox map: 004FBF60 → 005223F0   // gate forced on
          00522600 / 0051FBA0
004FE241  restore [eax+128] from [esp+36]
```

That would be a writer **inside** `00507C30`, still
**before** `"Load global things"`. First-seen New Game
does **not** enter it.

### 3. No other manager-`+128` store

In `listing-00500000` the only `mov [esi+128], imm` on this
object is `005235CD`. `0051DFBE` / `0051E060` / `0051EB47`
are **thing** `+128` next-links (`esi` from `[manager+132]`).
`0051F070` flush walks `+72/+76`, not `+128`.

`00521AE0` at `00509810` (GTNG hit only) is **not** a
writer. TLC `.gtng` miss `0050963B je 00509857` skips it
(`proofs/thing-manager-activate`).

---

## Timeline (no-save)

```
0042F2A2  Leave
0042F491  Init Game  004184BD
  "Init World" 0041735A
    004A67D0  CWorld ctor
      [world+80]=0
      [world+128]=0                  // BYTE loaded-flag; NOT the gate
    004A6E30
      "Init Thing Manager" 0049EBF0
        00523540  [manager+128]=1    ← FIRST WRITE
  "Init Display Engine" … "Load Particles"
  [game].vtbl+32  00416953  Loading world
    004A1840 → world vtbl+8 0049E220 → 00507C30
      token loop
      [0x1375459]==0 → skip 004FE030
      "Init thing maps"
      "Load GTNG" miss → 00509857
      "Load global things"
        [0x13B8609]==0 → 004FDBC0
          004FBF60(1) LookoutPoint.tng
            005223F0  reads leftover 1 → 00521AE0
    0049D970  [world+128]=1          // loaded-flag; NOT the gate
0041890E  00416392 → 0049E200        // [manager+24] count
```

Init Game’s named stages include Init World (the write)
**and** LoadWorld (the first `005223F0` read). After
`004184BD` returns the dword is still **1**.

---

## Not this field

| Site | Object | What |
|---|---|---|
| `004A685B` / `0049D970` / `004A5D0A` | **CWorld** `+128` | loaded byte/dword (`WorldLoadedFlagOffset`) |
| `005067C0` / `005067CE` | **CWorldMap** `+128` | list cursor |
| `004C90DE` / `0051DF80` | **CThing** `+128` | intrusive next |
| `EngineLifecycle.GamePlus128` | game clock | unrelated |

---

## Host `LoadGlobalThingsFile`

Host is the `004FDBC0` arm (`[0x13B8609]=0`). It
`ThingFile.Parse`s proximity maps into `GlobalThings` and
does **not** `LoadSingleThing` / `InsertThing`.

| Sense | Native first-seen | Host | Class |
|---|---|---|---|
| File I/O first `LookoutPoint.tng` | `004FBF60(1)` | `TryLoadThings("LookoutPoint")` | **MATCH** |
| `005223F0` construct | leftover `+128==1` → **taken** | no `LoadSingleThing` | **LEFTOVER** |
| `00416392` after LoadWorld | `0051E530` on `[manager+24]` | notes `0` | **PARTIAL** |

`00416392` is **not** the gate. `0049E200` → `0051E530`
sums `vtbl+92` on `[manager+24]` nodes with
`!([thing+145] & 1)`. Thing ctor `004C90FD` sets
`+145=0x04` (bit 0 clear). Empty `+24` after a taken
`00521AE0` would mean those CThings are **not** on that
list (or are torn down). That does **not** put the gate
back to 0.

Do **not** keep “parse-only MATCH” as the working model
now that the writer is known. Live RAM at the first
`005223F0` is still **UNREAD**. Dump-static first-seen is
**1**.

---

## UNREAD / PARTIAL

- Live `[manager+128]` bytes at `005223F7` (no debugger).
- Whether a taken `00521AE0` here inserts onto `+24`
  (would move `00416392`) or only a scratch / `+132` list.
- `005223F0` mode dword into `00521AE0` (`ret 36`; not
  required to classify the `==1` enter test).

---

## Do not

- Treat world `+128` / `0049D970` as the `005223F0` gate.
- Call `004FE030` first-seen (AllowDataGeneration off).
- Keep host skip as MATCH vs native first-seen `1`.
- “Fix” host by inventing a `+128=0` store that the dump
  does not take on this path.
- Start this walk at Oakvale / `00DBDE40`.
