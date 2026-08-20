# Init World `004A6E30` after Leave — child list, zeros, factory fill, navigator

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Question: after Leave, does `004A6E30` Init World Init zero
`world+172/+176/+180`, then fill factories at `00CD52D0`?
Complete child list **before** `00416953` `"Loading world"`.
Is `CNavigatorManager` constructed here?

Authority: `listing-00480000.txt` `004A6E30` / `004A68AE` /
`004A6550`; `listing-00400000.txt` `0041735A` / `00416953`;
`listing-00a00000.txt` `00A15670`; `listing-00c80000.txt`
`00CB5D80`; `listing-00cc0000.txt` `00CD52D0`;
`e8.tsv`; `rtti.txt` `CNavigatorManager`;
`proofs/navmesh-first/`; `docs/runtime/FORWARD_TREE.md` §§6–7, 10.

Do **not** nest `004A6550` / `00CD52D0` under `004A6E30`.
Do **not** start at `00DBDE40` / Oakvale.

---

## Verdict

**`CNavigatorManager` is constructed here. PROVEN.**

`004A6E30` (`[world].vtbl+36`) does **not** zero
`+172/+176/+180` and does **not** call `00CD52D0`.

| Claim | Where | Class |
|---|---|---|
| Zero `world+172/+176/+180` | `004A67D0` ctor at `004A68AE`, **before** `004A6E30` | **PROVEN** ctor; **DISPROVEN** as `004A6E30` |
| `00CD52D0` factory fill | `004A6550` (`[world].vtbl+28`) first insn of `00416953`, **after** `004A6E30` returns, **before** the `"Loading world"` log | **PROVEN** site; **DISPROVEN** as a child of `004A6E30` |
| `CNavigatorManager` | `004A6FFB` `00A15670` → `[world+72]` inside `004A6E30` | **PROVEN** |
| WLD / QST / `004B4260` | later in `00416953` after `"Loading world"` | **DISPROVEN** as Init World Init |

Same no-save spine, three functions:

```
0041735A  Init World
  004A67D0  CWorld ctor          // zeros +172/+176/+180
  [world].vtbl+36  004A6E30      // this file’s child list
… Create Players / Init Sound / Load Particles …
00416953
  [world].vtbl+28  004A6550      // 00CD52D0 fill
  then "Loading world"           // 004A1840 / QST fills +172
```

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game 004184BD
  "Init World" 0041735A
    alloc 0x198
    004A67D0  vtbl 012390F0
      [esi+20]=0 … [esi+124]=0
      004A68AE  [esi+172]=0
      004A68B4  [esi+176]=0
      004A68BA  [esi+180]=0
      004A68C0  [esi+184]=0  (and +188/+192/+196…)
    store world at game+36
    "Init World Init"
    [world].vtbl+36  004A6E30          // COMPLETE LIST BELOW
      00A15670  CNavigatorManager [world+72]
      // no 00CB5D80, no 00CD52D0, no +172 write
  "Init Display Engine" 00417418
  "Create Players" 004166A8
  "Init Sound" / "Load Particles"
  [game].vtbl+32  00416953
    00416968  [world].vtbl+28([game+40])
      004A6550
        "Init Atmos" / "Init Scripts" 006E7740 → world+56
        00CB5C70 → world+88
        004A6677  00CB5D80                 // only E8
          00CB5E12  00CD52D0               // factory FILL
          [esi+17]==0 skip 00CB7780
        "Engine Set World" / "Init Music Manager"
    [+90588] empty
    004169C8  "Loading world"
    004A1840  WLD / QST → +172 names
    0049F180 / 004B4260([world+172])
```

Frontend never reaches `004A67D0` / `004A6E30` / `00A15670`.
**PROVEN** (`navmesh-first`; one `E8` of `00A15670`: `004A6FFB`).

---

## 1. Zeros are the ctor, not `004A6E30`

`listing-00480000.txt` `004A67D0` (`ecx` = new `0x198` world):

```
004A67FF  xor ebx, ebx
…
004A68AE  mov [esi+172], ebx
004A68B4  mov [esi+176], ebx
004A68BA  mov [esi+180], ebx
004A68C0  mov [esi+184], ebx
```

`+172/+176/+180` is the MSVC CString vector
(begin / end / cap) later used as `AddQuest` TRUE
(`proofs/qst-clear-004A08D0/`, `world-plus184-first-use`).
Ctor zeros empty pointers. It does not allocate a buffer.

`004A6E30` starts `xor ebx, ebx` and never stores
`[esi+172]` / `+176` / `+180`. Closest later zero in that
fn is `004A7165 mov [esi+136], ebx` (unrelated).

Caller of the ctor is only `00417396` inside `0041735A`,
then `00417410 call [eax+36]` → `004A6E30`. **PROVEN** order:
zeros **then** Init World Init.

QST `004A08D0` later *clears* the same triples (flag-1
`FinalAlbion.qst`). That is inside `004A1840`, after
`"Loading world"`. **DISPROVEN** as Init World Init.

---

## 2. `00CD52D0` is not a child of `004A6E30`

`e8.tsv`:

| Dest | Only site |
|---|---|
| `00CD52D0` | `00CB5E12` in `00CB5D80` |
| `00CB5D80` | `004A6677` in `004A6550` |
| `00A15670` | `004A6FFB` in `004A6E30` |

`004A6E30` has **0** `E8` to `004A6550` / `00CB5D80` /
`00CD52D0`. **PROVEN** absence.

`004A6550` is world vtbl+28 (`EngineLifecycle.WorldPrepareVtbl`).
`00416953` first insn:

```
0041695F  mov ecx, [esi+36]     ; world
00416962  mov eax, [ecx]
00416965  push [esi+40]
00416968  call [eax+28]         ; 004A6550
…
004169C8  push "Loading world"
```

So factory fill is **inside** `00416953`, **before** the
Loading-world string, **after** `004A6E30` has returned.

`00CD52D0` (`listing-00cc0000.txt`): `"Registering Master Script"`
then `00CD5307 push "Q_SunnyvaleMaster"` / factory `00CDD550` /
run `00CDBD20`. Bind only (`00CB5C90` ×161). No fiber.
`[esi+17]==0` skips `00CB7780`. **PROVEN** (`script-bank-open`,
`script-factory-tables`).

Proofs that nest `004A6550` under `004A6E30` (`fiber-first`,
`script-factory-tables` timeline diagrams) are **wrong on
parent**. The VAs and “bind only” claims stay valid.

---

## 3. Complete `004A6E30` child list (before `00416953`)

`004A6E30`–`004A7702` (`ret`). TLC wrappers
(`0099EBF0` / `009D8240` / `0099EAE0` / `009E9F40`) omitted.
`esi` = world. Alloc fail → `xor eax,eax` then still store.

| Order | TLC / note | Construct | Store |
|---:|---|---|---|
| 1 | `"Init World Map"` | alloc `0xD8` `005066E0` (shift 5, bound `0x2000`) | `[world+20]` |
| 2 | `"Init Environment"` | alloc 60 `006BBC30` then `004ADD30` | `[world+28]` |
| 3 | *(no string)* WorldCamera | alloc `0x1970` `006B4900` | `[world+24]` |
| 4 | `"Init Navigation Manager"` | alloc 48 **`00A15670`** | **`[world+72]`** |
| 5 | `"Navigator A Star"` | `[nav].vtbl+4` flag `[0x129CBA4]` | — |
| 6 | `"Navigator flyer"` | `[nav].vtbl+4` flag `[0x129CB44]` | — |
| 7 | *(no string)* flyer object | alloc 16 `006B97E0` | `[world+84]` |
| 8 | `"Init Global Console"` | `00419D90` | — |
| 9 | `"Adding Console Commands"` | log only | — |
| 10 | | `mov [esi+136], ebx` | `[world+136]=0` |
| 11 | `"Init Combat Manager"` | alloc 92 `006ED3F0` then `006E8300` | `[world+76]` |
| 12 | `"Init Thing Manager"` | `0049EBF0(ecx=world)` | (inside callee) |
| 13 | `"Init Event Manager"` | alloc 8 `00687510` `004ADF80` | `[world+96]` |
| 14 | `"Init Search Tools"` | alloc 24 `0049C7A0` (map + `[world+80]`) | `[world+32]` |
| 15 | `"Init Game Camera Manager"` | alloc `0x160` `0069AE80` | `[world+48]` |
| 16 | `"Init Bullet Time Manager"` | alloc 28 `004C60F0` `004AE080` | `[world+104]` |
| 17 | `"Init Opinion Reaction Manager"` | alloc `0x728` `007004B0` `004ADAE0` | `[world+116]` |
| 18 | `"Init Script Conversation Manager"` | alloc 20 `006E6150` `004AE810` | `[world+124]` |
| 19 | `"Init Game Camera"` | alloc `0xC8` `006FD8C0` | `[world+44]` |
| 20 | | `[world+52] = [world+48]` | copy |
| 21 | `"Init Mesh Bank Manager"` | log only | — |
| 22 | `"Init Mesh Bank"` | `0049E620` | `[world+60]/+64` (inside) |
| 23 | `"Setting Particle Engine Mesh Bank"` | `00AEAA90([world+60])` | — |
| 24 | `"Setting Particle Engine Graphic Bank"` | `00AEAA80([game+8]+90568)` | — |
| 25 | `"Init Animation Event Managers"` | `006FAA90([world+60])` | — |
| 26 | `"Init Animation Events"` | `006FABF0(1)` / `006F5C10` | — |
| 27 | `"Init UI Manager"` | `0041E5F2` → `0041D198`; `0041DF10` | — |
| 28 | `"Init Speech Gain Manager"` | `006E3EC0` | — |
| 29 | `DefWindowProcW` singleton miss | alloc 16 `004AE2A0` `004A9A80` | then always |
| 30 | tail | **`006C37D0`** Bones enum (`bone-config-first`) | — |

Not in this function (later `004A6550` / `00416953`):

- Init Atmos / Init Scripts / `00CB5D80` / `00CD52D0`
- Engine Set World / Init Music Manager
- `00507C30` WLD parse (map vtbl+12, not `005066E0`)
- QST `004A0D90` / `004B4260`

FORWARD_TREE §7 already has this order through Speech Gain.
Search Tools (`004A7284`) and the bone tail are extra vs that
tree’s named block. Host `InitWorldInitStages` is a **subset**
(name-only notes; no 48-byte navigator). **DIVERGE** payload /
**PARTIAL** names.

---

## 4. `CNavigatorManager` construct — **here**

`004A6F82` `"Init Navigation Manager"`:

```
004A6FCE  push 48
004A6FD0  00BFEA1A
004A6FE4  [esi+8]+0x1613C
004A6FEB  push 4
004A6FFB  00A15670
004A7009  [world+72] = eax
```

`00A15670` (`listing-00a00000.txt`):

| Field | Value |
|---|---|
| vtbl | `0x129CA84` |
| RTTI | `CNavigatorManager` `0x013970D4` |
| size | 48 |
| `00A373B0` | ctor arg **5** |
| `+32` | `0x320` |
| `+36` / `+44` | circular sentinels |
| `+40` | `[arg].vtbl+8` |

Then A\* / flyer register on that same object (`navmesh-first`).
No mesh, no `CNavQuadTree`, no A\* query. First Lookout
quadtree commit is skipped (`job+12=0`). **PROVEN** construct;
**DISPROVEN** as first pathing.

Only `E8` of `00A15670` is this site. Frontend construct is
**DISPROVEN**.

---

## Host vs native

| Host | Native | Class |
|---|---|---|
| `InitWorldInitFn = 0x004A6E30` | world vtbl+36 | **MATCH** |
| `InitWorldInitStages` names a subset | full list above | **PARTIAL** |
| `"Init Navigation Manager"` `0x00A15670` | 48-byte `CNavigatorManager` | **PARTIAL** (name; no alloc) |
| `WorldPrepareVtbl = 28` → `004A6550` | first insn of `00416953` | **MATCH** parent |
| `00CD52D0` at Init World Init | at vtbl+28 inside Loading world | **DISPROVEN** if claimed as `004A6E30` child |
| `+172` empty until QST | ctor zero then `004A0D90` fill | **MATCH** emptiness at `004A6E30` exit |

---

## Classification table

| Claim | Status |
|---|---|
| `004A6E30` runs after Leave, before `"Loading world"` | **PROVEN** |
| `004A6E30` zeros `world+172/+176/+180` | **DISPROVEN** (`004A67D0` `004A68AE`) |
| `004A6E30` fills `00CD52D0` | **DISPROVEN** (`004A6550` @ `00416968`) |
| Factory fill still happens **before** the `"Loading world"` string | **PROVEN** |
| Complete child list is §3 | **PROVEN** listing |
| `CNavigatorManager` constructed in `004A6E30` | **PROVEN** |
| Navigator during frontend | **DISPROVEN** |
| WLD parse inside `004A6E30` | **DISPROVEN** |
| Host stage array is the complete native list | **DISPROVEN** (subset) |

Dumps: `listing-00480000.txt` `004A67D0` / `004A6550` /
`004A6E30`; `listing-00400000.txt` `0041735A` / `00416953`;
`listing-00a00000.txt` `00A15670`; `listing-00c80000.txt`
`00CB5D80`; `listing-00cc0000.txt` `00CD52D0`; `e8.tsv`
(`004A6FFB` / `004A6677` / `00CB5E12`); `rtti.txt`;
`proofs/navmesh-first/`.
