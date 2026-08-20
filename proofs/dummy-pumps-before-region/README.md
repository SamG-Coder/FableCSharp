# Dummy / fade / inner pumps after `004BBC00` and before first `00501450`

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`,
`00DBDE40`, or StartOakVale. The Oakvale *string* on
type-1 is a later `00CE7670` wait, not this enqueue.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: dump `listing-00400000.txt` (`004166E2` /
`0041674A` / `004162B5` / `00416953` tail /
`00416E78` / `00417001` / `00417747` / `004189C2`);
`listing-00480000.txt` (`004BBC00` / `004B4490`);
`listing-00500000.txt` (`00501450`);
`listing-00980000.txt` (`009A57B0`);
`listing-00b00000.txt` (`00B239A0`);
`listing-00c80000.txt` (`00CB8220`);
`e8.tsv` (`call 00501450` = 0);
`docs/runtime/FORWARD_TREE.md` §§8–11;
`docs/PARITY.md` Loading-world / first-pump rows;
`EngineLifecycle.Pump` / `PumpGame` / `PumpGameUpdate`
/ `EnqueueAfterDummy` after `EnterGame`;
`EngineLifecycleTests`
(`First_pump_004189C2_is_0040D2A0_then_00B239A0_not_a_region`,
`Second_pump_004189C2_loops_inner_not_00501450`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`,
`Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0`).

---

## Verdict

**After Loading world `004BBC00 ret 4` the no-save path
does one dummy prefix, one fade *install*, then an
unbounded inner-frame loop. None of those sites `E8`
`00501450`. First inner does not `00CB8220`. Later
dummy inners do, still on index 0.**

`00501450` E8 / E9 / imm / vtbl count is **0**. Body
is **PROVEN**; who first calls it is **UNREAD**. Host
`EnqueueAfterDummy` on the second `Pump` is
**DISPROVEN** leftover. Host `Pump` after `EnterGame`
is one inner per call; native `004189C2` is one outer
with a tight `00418AB1` loop.

| Kind | Count before first `00501450` | Site | `00CB8220`? | Class |
|---|---|---|---|---|
| Loading-world after `004B4A10` | 1 `ret 4` | `00416C2C` → `004BBC00` | no | **PROVEN** |
| Init Game suffix | not a pump | `0049BA70` / `004AE9D0` / `user.ini` | no | **PROVEN** |
| Dummy prefix | **1** / `004189C2` entry | `00418A3C` `004FB150` / `004FC180` index 0 | no | **PROVEN** |
| Fade install | **1** | `00418A90` vtbl+220 `00B239A0(12, 20.0)` | no | **PROVEN** |
| Fade tick `0041649C` | **0** first-seen | `00418289` frontend+GUI; later `00416FD7` | no | **PROVEN** skip |
| Inner `004162B5` | **unbounded** until `WM_DESTROY` | `00418B14` / loop `00418AB1` | first **no**; later type-1 **yes** | **PROVEN** loop; **UNREAD** as a finite N-to-`00501450` |
| `009A57B0` first inner | **2** | `004162C4`, `00417028` | no | **PROVEN** |
| `009A57B0` catchup inner | **3** | + `00416F9D` | yes via `004B453E` | **PROVEN** |
| `00CB8220` | 0 then 1/type-1 | only `E8` `004B453E` | — | **PROVEN** |
| `00501450` from this tree | **0** | no `call 00501450` | n/a | **UNREAD** caller |

---

## After `004BBC00` — not a pump

Dump `004BBC00` is one insn:

```
004BBC00  ret 4
```

Only no-save site is Loading world `00416953`:

```
00416C25  push 0x13B8674
00416C2A  mov ecx, esi
00416C2C  call 004BBC00
00416C31  pop edi
00416C32  pop esi
00416C33  leave
00416C34  ret
```

That is the end of vtbl+32. Then Init Game suffix
(`FORWARD_TREE` §10 / `FinishInitGameAfterWorld`):
`0049BA70` / `00416392` / `004AE9D0` / `user.ini` /
`ActivateQuest("Gameflow")` / store `004167DA`.
**PROVEN** not a region and **0** `00501450`.

Host `EnterGame` is that suffix. First host `Pump`
after `RequestNewGame` is `LeaveFrontend` →
`EnterGame` only (`GamePumpFirstDone` still false).
**PROVEN** (`First_pump_004189C2_*`).

---

## Dummy prefix — count 1

First `004189C2` (game vtbl+8), dump:

```
00418A3C  mov edi, [esi+36]          ; world
          vtbl+52 004AE8C0
00418A48  call 004FB150              ; [WorldMap+156]
00418A57  call 004FC180              ; [map+44]+index*88
00418A5C  mov ecx, [eax+36]
00418A5F  cmp ecx, ebx               ; ctor 0 = dummy
00418A61  je  00418A70               ; first-seen take
```

`[game+8]` is zeroed at `004189E7`. Named-start
flags `0x13B85F6` / `0x13B85F5` / `0x13B8628` are
BSS 0. **PROVEN** one dummy record probe per
`004189C2` entry. Not `SetRegionAsLoaded`. Not
`00501450`.

---

## Fade — count 1 install, 0 ticks

Same first-pump tail, **once**, then `jmp 00418B48`
into the inner compare (so the first inner still
runs on this entry):

```
00418A70  call 0040D2A0              ; PlayAVI singleton
00418A75  call 0040BC80
00418A7C  fld  [0x122F160]           ; 20.0f
00418A8E  push 12
00418A90  call [eax+220]             ; 00B239A0
00418A96  call 009F2660
00418AA7  call 009F26B0
00418AAC  jmp  00418B48
```

`00B239A0` dump:

```
00B239A0  mov eax, [esp+4]
00B239A4  mov edx, [esp+8]
00B239A8  mov [ecx+24], 1
00B239AC  mov [ecx+28], eax          ; 12
00B239AF  mov [ecx+32], edx          ; 20.0
00B239B2  ret 8
```

That is a **store**, not a per-inner fade pump.

`0041649C` first-seen does **not** run:
`00418289` only calls it when `00416296` and
`00490A22` both block (game is not frontend).
After `WorldFrame>1`, `00416E78` can call
`0041649C` at `00416FD7` but first-seen
`00446A30` returns `al=0`. **PROVEN** skip.

---

## Inner-frame pumps — unbounded; not `00501450`

```
00418AB1  call 0098E1B0              ; ret
00418AB8  call 00416231
00418AC7  call 009A6460              ; first-seen 1
00418B07  call 009F8BA0              ; [game+52]==0
00418B14  call 004162B5              ; inner frame
00418B30  call 00416202
00418B35  call 00415E85              ; skip
00418B3C  call 0044C6B0
00418B43  call 009AC9E0              ; ret 4
00418B48  cmp  [esi+8], bl
00418B4B  je   00418AB1              ; loop
```

Leave is only `009A6460==2` → `[game+8]=1` →
`004175E5`. That 2 is WndProc `009A5BEA`
`WM_DESTROY`. First-seen New Game does not
destroy the window. **PROVEN**.

So native count of inners *before* first
`00501450` is **not a closed integer** on this
tree: `00501450` is not a `004189C2` callee
(`0` `call 00501450` in listings). Host second
`Pump` is the **next inner**, not enqueue.
**DISPROVEN** as the `00501450` site.

### `004162B5` / `009A57B0` / `004166E2`

```
004162B5  call 009A4EC0
004162C4  call 009A57B0
004162C9  test al, al
004162CB  je   00416329              ; skip vtbl+20/28
004162CD  and  [0x13B89A8], 0
004162E0  call [eax+20]              ; 00418289
004162F9  call 009E9FB0              ; first-seen 0
00416318  call [eax+28]              ; 00417001
```

`009A57B0` dump:

```
009A57B0  mov edi, [esi+148]         ; HWND
009A57BA  call [0x1440378]           ; GetForegroundWindow
009A57C0  cmp edi, eax
009A57C2  sete eax
009A57E1  mov [esi+9], al
009A57EB  ret
```

IAT `0x1440378` is **not** GetTickCount. Host
`GraphicsCreated` / tick gate is **DISPROVEN**.
First-seen focused window → 1.

Same inner, render also gates:

```
00417028  call 009A57B0
00417042  call 0049D870
00417047  cmp eax, 1
0041704A  jle 0041725F               ; first-seen skip camera
```

First inner: **two** `009A57B0` (`004162C4` +
`00417028`). No `00416F9D` (vtbl+24 not called).

`004166E2` dump (clock):

```
004166E2  call 009F7050              ; slot
0041671D  call 009E1BC0              ; QPC IAT 0x143FE00
00416731  cmp  [0x13B86A4], 0        ; no writer
00416744  fsub [esi+96]              ; 004189DC snapshot
00416749  ret
```

`.text` callers:

| Site | Parent | First dummy inner? |
|---|---|---|
| `00416774` | `0041674A` / `004AEAA0` | **yes** → 0 |
| `004170C2` | `00417001` interp | no (`WorldFrame<=1`) |
| `00417196` | `00417001` interp | no |

`0041674A`: `[game+9]=1`, `0x13B8688` no writer,
`004166E2*15 − +9836` `fcomp 1.0`. First inner
`004166E2=0` → al=0 → `004AEB8A`. **PROVEN**.
`00418289` skips vtbl+24 `00416E78` and
`0041726D`. **No `00CB8220`.**

Later inners: `004166E2` grows as
`009E1BC0-[game+96]`. When `*15 − slot > 1`,
`004AEAA0` hits, type-1 `004A5A40` runs.
Host needs about `Pump(0.25f)` after the dummy
inner (`Type1_00CB8220_*`, `After_WorldFrame_gt_1_*`).
Native is many tight inners for the same QPC
delta. Host sticky `DisplayTime=0` is
**DISPROVEN**.

`00417747` / `0041777B 009A57B0` is the
`[game+52]!=0` branch. First-seen `[game+52]=0`
takes `004162B5`. Not on this walk.

### Other `009A57B0` `E8` (not dummy/fade)

| Site | Parent | First-seen dummy? |
|---|---|---|
| `004162C4` | `004162B5` | yes |
| `00416F9D` | `00416E78` | only after catchup |
| `00417028` | `00417001` | yes (then `WorldFrame<=1`) |
| `0041777B` | `00417747` | no |
| `009A5DD4` | WndProc | not this frame |
| `00AB48xx`… | other | not this tree |

---

## Do they `00CB8220`?

Only `.text` `E8`:

```
004B453E  call 00CB8220              ; 004B4490, [esi+56] walk
```

`00CB8220` dump:

```
00CB8220  push esi
00CB8223  call 00CB7C40
00CB822A  pop esi
00CB822B  jmp 00CB8170
```

Reached only from type-1 `004A5A40` → `004B4490`
after `004AEBA0` returns 1.

| Pump | `00CB8220` | Notes |
|---|---|---|
| Dummy prefix / fade install | **no** | before inner |
| First inner (`004166E2=0`) | **no** | no `004A5A40` |
| Later dummy type-1 inners | **yes** | Sunnyvale first, Gameflow last; state 0 **yields** on inactive `Q_NewOakValeIntro`; no activate |
| Resume type-1 | **yes** | `00A44880` / `00893610` still 0 → yield |
| `00501450` | n/a | still 0 `E8` |

Do **not** treat that yield as Oakvale start.
**DISPROVEN** activate.

---

## `00501450` — body only; caller UNREAD

Dump head:

```
00501450  push ebp
00501464  call 00449970
00501472  call 00487DC0
005014A3  call 004FEEC0              ; +156=0
          count = (+48-+44)/88
005014CA  cmp ecx, 1
005014D5  jbe 005018F8
005014EC  call 00500540              ; (i,0,0) i=1..count-1
```

No `call 00501450` in `listing-*.txt`. Not
`004162B5` / `00418289` / `004189C2`.
`FORWARD_TREE` §9: reached *after* dummy pump
in the host notes; E8 caller **UNREAD**.

---

## Host `Pump` leftover vs native

Native after `EnterGame`:

```
004189C2 once
  dummy 004FC180 index 0
  fade  00B239A0 once
  loop 00418AB1
    004162B5 / 009A57B0 / 004166E2
    009AC9E0
    [game+8]==0 → again
```

Host `EngineLifecycle.Pump` after `EnterGame`:

```
Pump #0  LeaveFrontend → EnterGame     // includes 004BBC00
Pump #1  PumpGame first                // dummy + fade + ONE inner
Pump #2+ PumpGame GamePumpFirstDone    // ONE inner each
```

`Pump` / `PumpGame` never call
`EnqueueAfterDummy` / `LoadFromFirstRealRegion`.
Tests that need `00501450` call it **explicitly**
after dummy pumps
(`Second_pump_00501450_*`).

| Host leftover | Native | Class |
|---|---|---|
| `EnqueueAfterDummy` on second `Pump` | next `00418AB1` inner | **DISPROVEN** |
| One host `Pump` = one `004189C2` | one `004189C2` = many inners | **DIVERGE** |
| Always-run vtbl+24 | only `004AEBA0==1` | **DISPROVEN** |
| Sticky `DisplayTime=0` | `004166E2=009E1BC0-[game+96]` | **DISPROVEN** |
| `009A57B0` = GetTickCount / `GraphicsCreated` | `GetForegroundWindow==[+148]` | **DISPROVEN** |
| Seed / Present only after `00501450` | type-1 + `00435F70` on dummy | **LEFTOVER** |
| `GetTickCountIat = 0x1440378` alias | same IAT is GetForegroundWindow | **LEFTOVER** |
| `00CB8220` body UNREAD (old comment) | `00CB7C40`+`00CB8170` **PROVEN** | **LEFTOVER** |

---

## Host catchup counts (tests, not native N-to-region)

After `RequestNewGame`:

1. `Pump()` → `EnterGame` / `004BBC00`. No `004189C2`.
2. `Pump()` → dummy + fade + first inner. `QuestPumpRan=false`. `FirstRealRegionLoadDone=false`.
3. `Pump(0.1f)` or `Pump(0.25f)` → first type-1 / `00CB8220`. Still dummy. Still no `00501450`.
4. Further `Pump(0.25f)` → `WorldFrame` 2, 3… `00416F9D 009A57B0` then `004457F0`. Still no `00501450`.

Native inners between (2) and (3) are however many
`00418AB1` iterations it takes for
`004166E2*15-slot>1`. Host folds that into one
`dt`. **DIVERGE** grain, same gate.

---

## What not to implement

- Do not call `00501450` from `Pump` / `PumpGame`.
- Do not pair `EnqueueAfterDummy` to “second frame”.
- Do not skip `00CB8220` on dummy type-1 inners.
- Do not activate Oakvale from those `00CB8220` ticks.
- Do not treat `004BBC00` or `00B239A0` as region load.
