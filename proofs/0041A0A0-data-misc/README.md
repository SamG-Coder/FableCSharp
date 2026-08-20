# `0041A0A0` intern prefix `Data\Misc\` — first Init Game call

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD`. First pump is
later `004189C2` / type-1 `00CB8220`. Do **not**
treat boot `00402510` / Present `00494900` /
WLD `"Load region graph"` as that first Init
Game site.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `0041A080` intern prefix `0x122F3D0`
`Data\Defs\` is used by `004CDB10`. Sibling stub
`0041A0A0` pushes `0x122F3E8` `Data\Misc\`. Who
first-calls `0041A0A0` on the Init Game / first
pump walk? What leaf is concatenated? Host
leftover?

Authority: Fable.exe dump
`listing-00400000.txt` (`0041A060`–`0041A0D0`,
`00402510` `00402B5A`, `0042F3EE` twin
`0041A0C0`, `0042F491`);
`listing-00480000.txt` (`004A75A7`–`004A7601`,
`00494A25`, `0049A180`);
`listing-00500000.txt` (`00506D40` / `005099E9`);
`listing-006c0000.txt` (`006FABF0`–`006FACF2`,
`006F9EE0`);
`listing-00980000.txt` (`0099BE70` `ret 4`);
`e8.tsv` dest `0041A0A0` (17 sites), dest
`006FABF0` (only `004A75FC`);
`functions.tsv`;
`strings.tsv` (ASCII; wchar prefixes **absent**);
`docs/runtime/FORWARD_TREE.md` §7;
`src/Fable.Game/EngineLifecycle.cs`
(`InitWorldInitStages` / `LoadRegionGraphFile`);
`src/Fable.Core/GameInstall.cs` (`Misc\` slots);
siblings `proofs/004CDB10-subtitled-body`,
`proofs/anim-event-first`,
`proofs/init-world-004A6E30`,
`proofs/xseq-first`.

---

## Verdict

**First `E8` of `0041A0A0` on the Init Game walk
is `006FAC3F` inside `006FABF0`.** Parent is
`"Init Animation Events"` on `004A6E30`
(`004A75FA` `cl=1` then `004A75FC`). That is
still inside `004184BD` Init World, **before**
LoadWorld / first pumps.

Prefix is the stub immediate `0x122F3E8`
(`Data\Misc\`). Concatenated leaf is the wchar
at **`0x0126493C`**, interned with `0099B6B0`
then joined by `0099BE70` (not the
`0099BF30` wchar concat `004CDB10` uses).
Decoded filename is **UNREAD** (ASCII
`strings.tsv` skips wchar; listing does not
emit `.rdata` bytes). Do not invent a TLC
name.

Host leftover: `InitWorldInitStages` jumps
Mesh Bank → UI Manager and **never** calls
`006FABF0` / `0041A0A0`. `GameInstall` has
other `Misc\` files (stars, region graph, pc
banks). Those are **not** this first leaf.
Later WLD `00506D40` is a **different**
`0041A0A0` site.

| Claim | Class |
|---|---|
| Stub `0041A0A0` is `push 0x122F3E8` / `0099B6B0` / `ret` | **PROVEN** |
| `0x122F3E8` is UTF-16 `Data\Misc\` | **PROVEN** (authority decode; sibling of `0041A080` `0x122F3D0` `Data\Defs\`) |
| First Init Game / first-pump `E8` is `006FAC3F` | **PROVEN** |
| Caller is `006FABF0`; only `e8.tsv` dest is `004A75FC` | **PROVEN** |
| Leaf VA is `0x0126493C` (game list) | **PROVEN** |
| Decoded leaf filename | **UNREAD** |
| Same fn second site `006FACBA` leaf `0x01264904` (sound) | **PROVEN** order; text **UNREAD** |
| PE-first `E8` `00402B5A` (`00402510`) is this walk | **DISPROVEN** — boot GBANK/particle paths |
| `00494A25` (`00494900`) is this walk | **DISPROVEN** — wait loop **before** `00418DCA` |
| Twin `0041A0C0` at `0042F3EE` is this stub | **DISPROVEN** — same prefix, other VA |
| First pump `00CB8220` calls `0041A0A0` | **DISPROVEN** — dest absent |
| `006F9EE0` (same two leaves + `006F9BE0`) is first-seen | **DISPROVEN** — **zero** `E8` |
| Host runs `006FABF0` / `0041A0A0` | **DISPROVEN** — **LEFTOVER** omit |
| Host `LoadRegionGraphFile` is this first site | **DISPROVEN** — later `00506D40` |
| Oakvale on this site | **DISPROVEN** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Who first-calls `0041A0A0` on Init Game / first pump? | `006FABF0` at `006FAC3F`. Named parent `"Init Animation Events"` `004A75FC` on `004A6E30`. | **PROVEN** |
| What leaf is concatenated? | Push VA `0x0126493C` → `0099B6B0` → `0099BE70` onto prefix `Data\Misc\`. Text **UNREAD**. | **PROVEN** VA / **UNREAD** name |
| Host leftover? | Yes. No `0041A0A0`. `InitWorldInitStages` skips the whole animation-event pair. | **PROVEN** **LEFTOVER** |

**Answer:** first leftover **on this named site**
is the `Data\Misc\` + `0x0126493C` load
(`006FA4E0`). Do not start Oakvale.

---

## 1. Stub

`listing-00400000.txt`:

```
0041A060  push esi
0041A061  push 0x122F3B4          ; Data\Levels\
0041A068  call 0099B6B0
0041A070  ret

0041A080  push esi
0041A081  push 0x122F3D0          ; Data\Defs\
0041A088  call 0099B6B0
0041A090  ret

0041A0A0  push esi
0041A0A1  push 0x122F3E8          ; Data\Misc\
0041A0A6  mov esi, ecx
0041A0A8  call 0099B6B0
0041A0AD  mov eax, esi
0041A0AF  pop esi
0041A0B0  ret

0041A0C0  push esi
0041A0C1  push 0x122F3E8          ; same prefix, other stub
0041A0C8  call 0099B6B0
0041A0D0  ret
```

`ecx` is the dest CString. `eax` returns that
dest. `0041A0C0` is a **twin**, not an `E8` of
`0041A0A0`. New Game uses the twin at
`0042F3EE` (`push 0x122DA38`) **before**
`"Init Game"` `0042F491`. **DISPROVEN** as this
question’s dest.

---

## 2. Every `E8` of `0041A0A0`

`e8.tsv` dest `0041A0A0` (17):

| Site | Containing | Walk vs Init Game | Leaf |
|---|---|---|---|
| `00402B5A` | `00402510` | **before** — GBANK/PARTICLE token paths into `0x13CA79C` | `push 0x122DA10` |
| `00494A25` | `00494900` ← `00496D27` | **before** — `[esi+8]==0` wait, then `00418DCA` | `push 0x1238740` |
| `0049A180` … `0049A913` (11) | `00498490` ← `004999D4` | **after** Init World — Present/texture (`009BEF20` / `009BEEB0`) | `0x1238740` / `0x1238B10` / … |
| `00506D4F` | `00506D40` ← `005099E9` | **later** Init Game — WLD `"Load region graph"` | arg CString (`PLAYER_GUI_PC+0xA94`), **no** imm VA |
| `006F9EFD` / `006F9F49` | `006F9EE0` | **not first-seen** — zero `E8` of `006F9EE0` | `0x126493C` / `0x1264904` |
| `006FAC3F` / `006FACBA` | `006FABF0` | **this walk** | `0x126493C` then `0x1264904` |

PE-lowest site is `00402B5A`. That is **not**
`004184BD`. `00402510` has one `e8.tsv` caller:
`004034F1`.

---

## 3. Init Game first-seen

`004184BD` → `"Init World"` `0041735A` →
`004A6E30` vtbl+36 (FORWARD_TREE §7):

```
004A75A7  call 006FAA90          ; managers (0041A080 Defs names)
004A75AE  push "Init Animation Events"
…
004A75FA  mov cl, 0x01
004A75FC  call 006FABF0          ; FIRST 0041A0A0 on this walk
004A7601  call 006F5C10
004A7608  push "Init UI Manager"
```

`e8.tsv` dest `006FABF0`: **only** `004A75FC`.

`006FABF0` (`listing-006c0000.txt`):

```
006FABF0  sub esp, 12
          test cl, cl            ; 1 → log "Loading Game Animation Events"
006FAC2C  push 0x126493C         ; wchar leaf
006FAC31  lea ecx, [esp+20]
006FAC35  call 0099B6B0          ; intern leaf
006FAC3A  push eax
006FAC3B  lea ecx, [esp+16]
006FAC3F  call 0041A0A0          ; prefix Data\Misc\
006FAC44  mov edx, eax
006FAC46  lea ecx, [esp+12]
006FAC4A  call 0099BE70          ; concat (ret 4)
006FAC4F  push eax
006FAC50  mov ecx, esi           ; game manager
006FAC52  call 006FA4E0          ; load
          …
006FACA7  push 0x1264904         ; sound leaf
006FACBA  call 0041A0A0
006FACCD  call 006FA4E0          ; [0x13BABBC]
006FACF2  ret
```

`0099BE70` takes `edx` = prefix CString, a
pushed leaf CString, dest in `ecx`. It is
**not** `0099BF30` (wchar `0099B940`
`[edx+eax*2]`), which `004CDB10` uses with a
raw `push 0x1239E74`.

`006FAA90` (immediately before) uses
**`0041A080`** (`Data\Defs\`) for manager
names `0x1264A14` / `0x12649E0`. **DISPROVEN**
as a `0041A0A0` call.

`006FA4E0` then branches on
`[0x13B860A]` `UseCompiledAnimationEvents`
(`anim-event-first`). BSS default 0 → text
arm `00999230` exists(path). Hit vs miss
**UNREAD**. Empty vectors still let Init
World continue.

WLD `00506D40` is **after** `004A6E30`
(FORWARD_TREE §7 vs §9). First pumps
`004189C2` / `00CB8220` have **no** dest
`0041A0A0`.

---

## 4. Leaf `0x0126493C`

Caller **does** push a VA. Recovered:

| VA | Role | Neighbour (ASCII `strings.tsv`) |
|---|---|---|
| `0x01264904` | sound list (second site) | — |
| `0x0126493C` | **game list (first site)** | — |
| `0x01264974` | vtbl `CManager@NAnimationEvents` | — |
| `0x01264978` | — | `END_ANIMATION_EVENTS` |
| `0x012649D4` | compiled suffix (`006F9F90`, not this concat) | after `BEGIN_ANIMATION_EVENTS` |

Wide text is **UNREAD**. Do not invent
`GameAnimationEvents.txt` / region-graph /
`stars.dat`. Slot `0x0126493C`…`0x01264974`
is 56 bytes (28 wchar including NUL). That
bounds length only.

---

## 5. Host leftover

`InitWorldInitStages` after `"Init Mesh Bank"`
is `"Init UI Manager"` `0041D198`. Native
inserts particle-bank hooks, `006FAA90`,
`006FABF0` / `006F5C10` between those notes
(`xseq-first` / `anim-event-first`).
**LEFTOVER** omit.

`GameInstall` `Misc\` slots that **exist**:

- `stars.dat`
- `FinalAlbion_StartingRegionGraph.txt`
- `Misc\pc` banks

None of those is the first-site leaf VA.
`LoadRegionGraphFile` notes `00506D40` /
`PLAYER_GUI_PC+0xA94` — a **later**
`0041A0A0` with a **runtime** CString, not
`0x0126493C`. Mixing that MATCH into this
site is **DISPROVEN**.

No C# `0041A0A0`. No `006FA4E0` parser
(`anim-event-first` “Event list I/O none”).

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00480000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-006c0000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00980000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
- `tools/Fable.ExeIndex/out/00-index/strings.tsv`
- `docs/runtime/FORWARD_TREE.md`
- `src/Fable.Game/EngineLifecycle.cs`
- `src/Fable.Core/GameInstall.cs`
- `proofs/004CDB10-subtitled-body/README.md`
- `proofs/anim-event-first/README.md`
- `proofs/init-world-004A6E30/README.md`
