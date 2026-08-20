# First game Present HUD empty skip vs `009DD8F0` / `00435530`

Investigation only. No production `src/` or `tests/` edits.

Question: first game Present HUD empty skip
**MATCH**, or host hide of a missing HUD?
`PLAYER_GUI_PC` leftover?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: dump `fn --exact`
`009DD8F0` / `009DB700` / `00435530` /
`00435000` / `00435070` / `009DBFF0` /
`0049D9D0` / `004498C0` / `0048A210`;
`listing-00400000.txt` `00435530` HUD
gates `00435AA4` / `00435B57` /
`00435BDD` / `00435CC7`;
`listing-00480000.txt` `0048A29B` /
`00487570`;
`listing-009c0000.txt` `009DAA42`;
`.data` dword `0x1375720`;
`scan` / `datascan` of gate VAs;
`src/Fable.Game/EngineLifecycle.cs`
`ApplyDisplayCamera` /
`DisplayPlayerOverlayFn` /
`DisplayFlushShouldDip` /
`PlayerGuiReady`;
`src/Fable.Client/SilkEngineHost.cs`
`Draw`;
siblings `proofs/hud-after-leave`,
`proofs/dx9-3d-submit`,
`proofs/init-gui-0043A380`,
`proofs/issue-17-verify`;
`docs/status/investigations/A-dx9-submit.md`;
`EngineLifecycleTests.After_004AEA70_eq_1_00417001_is_00435F70_Present`.

Do not start Oakvale / `00DBDE40` /
`Q_NewOakValeIntro`. Do not invent a
`+16020` record. Do not treat frontend
`0042DF9E` as this Present.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| First game Present HUD dest empty? | Native `009DA9F0(1)` `[+16020]==[+16024]` → `009DB6E6` no DIP | **PROVEN** |
| Cause: host hide of missing HUD? | **No.** Overlay miss, interface miss, four `009DD8F0` gates closed, `00487570` skip. Native would not enqueue | **DISPROVEN** as hide |
| First Present empty skip vs host Notes? | Skip dest **MATCH**. Host never walks the closed `009DD8F0` sites | **MATCH** dest; notes **PARTIAL** |
| `009DD8F0` in `00435530` is player HUD? | Debug UTF-16 `"Recording!"` / `"Skipping unrecorded fram…"` / `"Frames behind "` | **DISPROVEN** as orbs |
| `PLAYER_GUI_PC` on first Present dest? | Singleton exists native; **not** a dest producer here | **LEFTOVER** host flag; dest **MATCH** empty |
| `DisplayFlushShouldDip(0, 0)` always false? | Host never stores `[this+16020]`. First-seen empty is still native | **MATCH** first-seen; later **LEFTOVER** stand-in |

---

## Verdict

**First game Present HUD empty skip is MATCH,
not a host hide of missing HUD.**

`00435F70` jmp `00435530` does call overlay
`00435000` and the four `009DD8F0` sites
exist, but first-seen no-save takes every
skip. Nothing writes a 60-byte `+16020`
record. `009DA9F0(1)` is the empty jump.
Host `ApplyDisplayCamera` Notes those
skips and `DisplayFlushShouldDip(0, 0)`
false. Client `Draw` is world/AVI 3D —
native also has no HUD dest to composite.

`PLAYER_GUI_PC` is leftover on the host
(`PlayerGuiReady=true` after a Note).
Native constructs it. It is **not** a
first-seen draw object. Missing host GUI
does not change first Present pixels.

| Claim | Status |
| --- | --- |
| `00435000` miss skip `00639E40` | **PROVEN** / **MATCH** |
| `00435070` miss skip `0057B43F` | **PROVEN** / **MATCH** |
| Four `00435530` `009DD8F0` gates closed first-seen | **PROVEN** |
| `00487570` (11× `009DD8F0`) skipped: `CPlayer+8=0` | **PROVEN** |
| `0x1375720=-1` skip `009DBFF0` debug overlay | **PROVEN** |
| `009DA9F0(1)` empty → `009DB6E6` | **PROVEN** / **MATCH** |
| Host empty skip hides native HUD dest | **DISPROVEN** |
| `PLAYER_GUI_PC` host object / meters | **LEFTOVER** |
| First later `009DD8F0` gate that opens | **UNREAD** |
| Host `DisplayFlushShouldDip(0, 0)` as a live queue read | **DISPROVEN**; first-seen empty **MATCH** |

---

## Evidence

Dumps this pass (`tools/Fable.ExeIndex` `fn --exact`,
`scan` / `datascan`, PE `.data` at `0x1375720`).
Listings under `assembly/exe/01-sections/text-map/`.

`009DD8F0` is 34 insns, `ret 20`. It builds a
wide string via `0099B720`, then:

```
009DD931  push 0x3F800000          ; scale 1.0
009DD93D  call 009DB700            ; only enqueue E8
```

`009DB700` `ret 24`: skip if
`[[this+14908]+472]`; else 60-byte local,
`lea esi, [edi+16020]`, `add [esi+4], 60`
or grow `009E1750`. Only other `E8 009DB700`
is wrap `009DBFF0` (`009DC00E`).

`00435530` after Clear always:

```
004357D0  call 00435000            ; overlay
004357D8  call 00435070            ; interface
… gated 009DD8F0 …
00435D40  call 009D9C80
00435D4B  push 1
00435D4D  call 009DA9F0
```

No `E8 00B25950`. Host pairing ScenePass
bits onto this body is **DISPROVEN**
(`dx9-3d-submit`).

---

## Original

### Overlay / interface (player HUD apply)

`00435000` (`DisplayPlayerOverlayFn`):

```
00435001  mov ecx, [ecx+12]
00435004  call 00449960
0043500B  call 00487DD0            ; +44 jmp 00A01B50
00435010  test eax, eax
00435012  je  0043505E             ; miss → ret
          [eax+145] bit0 / [eax+48] 0x4000
00435058  call 00639E40            ; text, only if Thing
```

No-save first Present: no player Thing
(`00A01B50` 0). **PROVEN skip.**

`00435070`:

```
0043507C  call 00449970
00435083  call 00487DC0
00435088  test eax, eax
0043508A  je  004350C9             ; miss → ret
004350C4  call 0057B43F            ; type 0x22, only if Thing
```

Same miss. **PROVEN skip.**

### `00487570` camera-debug strings

Before overlay, `00435530` may call
`00487570` (eleven `E8 009DD8F0`) when
`0049D9D0` → `00449970` → `004498C0`
returns a `CPlayer` with `[+8]!=0` and
`00633BE0` (`[+528]→[+4]→[+8]`) is 19.

Create Players `0048A210` (`xor ebx, ebx`):

```
0048A29B  mov [esi+8], bl          ; CPlayer+8 = 0
```

First Present: `[ebx+8]==0` →
`je 004357BA`, no `00487570`.
**PROVEN skip.**

### Four `009DD8F0` gates in `00435530`

| Site | Gate | First-seen | Take? |
| --- | --- | --- | --- |
| `00435AA4` | `[0x13B8629]` then retail+56==0 | BSS **0**, no PE imm writer | **skip** `je 00435AA9` |
| `00435B57` | `[0x13B86EB]` and `[game+90596]!=0` | BSS **0**, `datascan` only the read | **skip** `je 00435B65` |
| `00435BDD` | `[0x13B860C]` and cutscene/retail | BSS **0** (`PlayerCatchupMenuFirstSeen`) | **skip** `je 00435BE2` |
| `00435CC7` | `[0x13B86E7]` | BSS **0**, only the read | **skip** `je 00435CCC` |

`scan` / `datascan` immediates:

- `0x13B86EB` / `0x13B86E7`: **one** hit each, the read.
- `0x13B860C` / `0x13B8629`: reads only (`0041674A`, `00435530`, `0069E790`).
- All four sit past `.data` file size (`rva 0xF74000` file `0x44000` ends `0x013B8000`). BSS first-seen **0**.

Strings if a gate opened (UTF-16 `.rdata`):

| Imm | Text |
| --- | --- |
| `0x12316D8` | `Recording!` |
| `0x122DE44` | `%d` (with `0x12316B8` `Frames behind `) |
| `0x1231680` | `Skipping unrecorded fram…` |

These are debug / capture overlays, **not**
`HUD_ORB_*` / `PLAYER_GUI_PC` meters.

### `009DBFF0` debug overlay

```
00435873  mov eax, [0x1375720]
0043587C  jl  00435A36
… 009DBFF0 at 00435A0F …
```

`.data` init at `0x1375720`: `FF FF FF FF`
= **−1**. `jl` skips. No PE writer of that
VA. **PROVEN skip.** `009DBFF0` is only a
push-shuffle into `009DB700`.

### Drain

```
009DA9F7  mov edx, [ebp+16020]
009DA9FD  mov ecx, [ebp+16024]
009DAA03  sub ecx, edx
… count * 0x88888889 / 60 …
009DAA42  je  009DB6E6             ; empty: no DIP
```

First-seen begin==end. No `009DB700`.
**PROVEN empty dest.**

### `PLAYER_GUI_PC`

Create Players `0043B570` then Init GUI
`0043A380` reset (`hud-after-leave`,
`init-gui-0043A380`). Meters exist with
`+8=0`. `0043A080` tick `[world+164]=0`.
None of that is `009DD8F0` or `00639E40`
on this Present.

---

## Host

`EngineLifecycle.ApplyDisplayCamera`
(`00435F70` jmp `00435530`):

```
Note(DisplayPlayerOverlayFn, … "00435000 skip 00639E40");
Note(DisplayPlayerInterfaceFn, … "00435070 skip 0057B43F");
var shouldDip = DisplayFlushShouldDip(0, 0);   // always false
Note(DisplayFlushLayersFn, …
    "009DA9F0(1) [+16020] empty dest");
Note(… "009DA9F0 skip DIP 009DB6E6");
```

`DisplaySubmitStages` is Begin / Clear /
overlay / interface / Flush2D /
FlushLayers / End / Present. **No**
`009DD8F0` / `009DB700` row.

`DisplayFlushShouldDip(0, 0)` is
`DisplayQueueCount` on caller-supplied
begin/end. Host never stores
`[this+16020]`. First Present passes
`0, 0` on purpose.

`PlayerGuiReady = true` after
`Note(InitGuiFn, … "0043A380 reset PLAYER_GUI_PC")`.
No `0x338` alloc, no `[0x13B8790]`, no
meters (`issue-17-verify`).

Client `SilkEngineHost.Draw`: camera
world / AVI only. No HUD dest path.
`Program` `window.Render` does not
composite overlay strings.

Test
`After_004AEA70_eq_1_00417001_is_00435F70_Present`
locks overlay skip, interface skip,
`empty dest`, `SubmittedLayerBits` empty,
`LayerFlushCount==0`.

---

## Gap

| Item | Native first Present | Host | Class |
| --- | --- | --- | --- |
| Overlay `00639E40` | miss skip | Note skip | **MATCH** |
| Interface `0057B43F` | miss skip | Note skip | **MATCH** |
| Walk four `009DD8F0` sites | yes, all `je` skip | omitted | dest **MATCH**; notes **PARTIAL** |
| Enqueue `009DB700` | not reached | not called | **MATCH** |
| `009DA9F0(1)` empty | `009DB6E6` | `DisplayFlushShouldDip(0,0)` | **MATCH** first-seen |
| Live `[+16020]` vector | empty | never stored | first-seen **MATCH**; later **LEFTOVER** |
| `PLAYER_GUI_PC` object | ctor + reset | flag only | **LEFTOVER** |
| HUD dest pixels | none | none | **MATCH** |
| Hide of a native HUD dest | n/a | n/a | **DISPROVEN** |

Host always-empty flush is a stand-in
*implementation*. On **this** no-save
first Present the native producers are
also idle, so the skip is not covering
up missing HUD work.

---

## Open

| Item | Class |
| --- | --- |
| First later writer of `0x13B860C` / `0x13B8629` / `0x13B86EB` / `0x13B86E7` | **UNREAD** (no imm store in the PE) |
| First take of `00487570` after `CPlayer+8!=0` and mode 19 | **UNREAD** |
| First `00639E40` / `0057B43F` after a live Thing | **UNREAD** (`hud-after-leave`) |
| Host `PlayerGuiReady` without `[0x13B8790]` | **LEFTOVER** (`issue-17-verify`) |

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `00435530` | game Present body | **PROVEN** order; HUD dest empty **MATCH** |
| `00435000` | overlay lookup | **PROVEN** miss |
| `00435070` | interface lookup | **PROVEN** miss |
| `009DD8F0` | HUD/debug string → `009DB700` | **PROVEN** closed first-seen |
| `009DB700` | 60-byte `+16020` enqueue | **DISPROVEN** first-seen |
| `009DBFF0` | wrap → `009DB700` | **DISPROVEN** first-seen (`0x1375720=-1`) |
| `009DA9F0` | drain; empty `009DB6E6` | **PROVEN** / **MATCH** |
| `00487570` | camera debug strings | **DISPROVEN** first-seen |
| `0043B570` / `0043A380` | `PLAYER_GUI_PC` ctor / reset | **PROVEN** native; host **LEFTOVER** |

**Answer: MATCH empty skip, not host hide.
`PLAYER_GUI_PC` leftover is the object/flag,
not first Present dest.**
