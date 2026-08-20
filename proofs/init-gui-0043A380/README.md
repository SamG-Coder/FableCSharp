# `0043A380` Init GUI after `0049F180`

Investigation only. Production `src/` was not edited.

Do **not** treat `"Init GUI"` as frontend `PRESS_START` /
`frontend.bin` widgets (`0054E3D0` type 10 / `0041AFA0` dest).
Do **not** treat it as first `CTCInventory` (`00590D32`) or
Hero create (`006AC910`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: after `0049F180`, what does `0043A380`
(`ecx=[0x13B8790]`, name `PLAYER_GUI_PC`) construct
first-seen? HUD widgets? Relation to frontend leftover?

Authority: dump `listing-00400000.txt` `0043A380` /
`0043B570` / `0043B050` / `00438570` / `0043FF30` /
`0041BE70` / `0041BEB0` / `00417646`;
`listing-00480000.txt` `0049F180` / `00487FB0` /
`0048A210` / `00492BAB` / `0049166E`;
`listing-00440000.txt` `0044A530` / `0044A1A0` /
`0044C6B0` / `00449700`;
`listing-00640000.txt` `00647319`;
`listing-00680000.txt` `0069ECD0`;
`xrefs.tsv` `PLAYER_GUI_PC`;
`docs/runtime/FORWARD_TREE.md` §10;
`src/Fable.Game/EngineLifecycle.cs`
(`InitCharactersAndQuests`, `CreatePlayers`,
`TickPlayerGui`, `PlayerGuiReady`);
siblings `proofs/creature-after-leave`,
`hero-inventory-first`, `audit-frontend-leftover`,
`audit-playerinterface`, `audit-lifecycle-newgame`;
`EngineLifecycleTests.LoadWorld_00416953_no_save_is_004A1840_then_0049F180`.

---

## Verdict

**`0043A380` does not construct the GUI object, HUD
widgets, or frontend leftovers.**

It is a **re-init / reset** on an already-live
`[0x13B8790]` singleton. The only `E8` of `0043A380`
is `0049F214` (inside `0049F180`). First-seen New Game
already constructed that object during **Create Players**
(`0048A210` → `00487FB0` → alloc `0x338` → `0043B570`
→ `004195AF` store).

| Question | Answer | Class |
|---|---|---|
| `0049F180` site of `"Init GUI"`? | `0049F20E` `ecx=[0x13B8790]` `call 0043A380` | **PROVEN** |
| Other `E8` of `0043A380`? | **None** | **PROVEN** |
| `0043A380` allocs `PLAYER_GUI_PC` instance? | **No.** uses existing `ecx` | **DISPROVEN** |
| First store of `[0x13B8790]` (non-zero)? | Create Players `00487FFE` `004195AF` after `0043B570` | **PROVEN** |
| First `PLAYER_GUI_PC` **def** bind `[0x13B878C]`? | `0043B570` (same `0099EBF0` / `0044C6B0` / `0043FF30` / `009ADA40` sequence) | **PROVEN** first writer |
| `0043A380` first-seen takes that bind? | **No** if ctor already stored `[0x13B878C]` (`jne 0043A40F`) | **PROVEN** skip |
| HUD **meter / bar** objects (`0065431D` …)? | `0043B570` ctor, not `0043A380` | **PROVEN** |
| HUD **type-`0x22` widgets** (`0041BEB0`)? | later `0043B050` (`0069ECD0`), not `0043A380` | **PROVEN** |
| Frontend `PRESS_START` / type 10 leftover? | Different tree; Leave already tore it down | **DISPROVEN** as this fn |
| C# `PlayerGuiReady` is the ctor? | **Note-only** flag after `0043A380` | **LEFTOVER** |

**Answer:** first-seen work of `0043A380` is
**reset + recopy**, not construct. It walks
`this+24` (`00492BAB(0)`), clears `this+456`
(`00647319`), zeros `+8` on five existing meter
pointers (`+716`…`+748`), recopies def `+2044`
into GUI `+608` / `+620` (`00442770`), zeros
`[this+424]+48`, sets `[this+657]=1`.

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend                 // 2D PRESS_START tree gone
0042F491  Init Game
  Init Player Interface 004473A0
  Init World            004A6E30
  Create Players        004166A8
    0044C6B0 [0x13B879C]
    0044A530 slots 0–3 then 4
      0044A1A0 alloc 0x22C 0044BC10
      0048A210
        [+520]=1 for slots 0–3
        00487FB0
          00449700([player+16]) == [player+40]
          → push 0x338 00BFEA1A
            0043B570 vtbl 0123177C
              0049166E this+24
              PLAYER_GUI_PC → [0x13B878C]     // first def
              00442770 +608 / +620
              00BFEA1A meters 0065431D…       // first HUD objs
          004195AF [0x13B8790] = gui          // first instance
  00416953 Loading world FinalAlbion.wld
    004A1840
    [0x13B8648]==0
    00416BCA  0049F180(ecx=world, 0)
      "Init Characters" 00449970 / 00489D40 miss
      "Init GUI"        0043A380 [0x13B8790]  // reset, not ctor
      "Init Quests"     004B4260
later 0069ECD0  0043B050 0041BEB0 type 0x22   // HUD sprites
later 00435530  00435070 PlayerInterface HUD  // draw skip if no Thing
```

`004A2C80` inside `004A1840` when `[world+258]==0` is a
second `0049F180(1)` (same `0043A380`). First-seen take
**PARTIAL** (`creature-after-leave`).

---

## 1. `0049F180` only calls `0043A380`

`listing-00480000.txt` `0049F180`–`0049F25E`:

```
0049F180  sub esp, 48
0049F18D  push "Init Characters"
0049F1B3  mov ecx, [esi+12]
0049F1B6  call 00449970
0049F1BD  call 00487DC0
0049F1C4  je   0049F1CF
0049F1D7  call 00449D90
0049F1EA  push "Init GUI"
0049F20E  mov ecx, [0x13B8790]
0049F214  call 0043A380
0049F21B  push "Init Quests"
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260
```

No null test on `[0x13B8790]`. Native first-seen must
already have the singleton or this `call` is undefined.

`grep call 0043A380` across `listing-*.txt`: **one** site
(`0049F214`). **PROVEN.**

---

## 2. `0043A380` body — reset, not construct

`listing-00400000.txt` `0043A380`–`0043A4CB`:

```
0043A380  sub esp, 8
          esi = ecx                    // [0x13B8790]
0043A389  push 0
0043A38A  lea ecx, [esi+24]
0043A38D  call 00492BAB                // reset +24 (arg 0)
0043A392  mov ecx, [esi+456]
0043A398  call 00647319                // clear +456 list
          [esi+716]+8 = 0              // five meter flags
          [esi+724]+8 = 0
          [esi+732]+8 = 0
          [esi+740]+8 = 0
          [esi+748]+8 = 0
0043A3CA  mov eax, [0x13B878C]
0043A3D1  jne 0043A40F                 // already bound → skip
          0099EBF0 "PLAYER_GUI_PC"
          push 0x13B878C
          0044C6B0                     // eax=[0x13B879C] getter
          0043FF30 → 009ADA40          // store compiled def
0043A40F  count = (def+2048 − def+2044) via 0xB21642C9 / sar 6
0043A43C  00442770(esi+608, count, 1.0f)
          same count → 00442770(esi+620)
0043A4B6  mov ecx, [esi+424]
0043A4BC  mov [ecx+48], 0
0043A4BF  mov [esi+657], 1
0043A4CB  ret
```

Callee list: `00492BAB`, `00647319`, optional
`0099EBF0` / `0044C6B0` / `0043FF30` / `0099EAE0` ×2,
`00442770` ×2. **No** `00BFEA1A`. **No** `0041BEB0`.
**No** `0065431D`. **No** `0043B570`. **PROVEN.**

`0044C6B0` is `mov eax, [0x13B879C]; ret` (player-manager
getter). The two pushes stay for `0043FF30`, which is
refcount + `009ADA40` name lookup into `[0x13B878C]`.
Same sequence as `00438570` (getter) and `0043B570`
(ctor). **Not** a widget factory.

`00492BAB(0)` walks `+24` lists with `0041BE70`
(release, **not** `0041BEB0` construct), then resets
name slots to `0x122D70C` / `0x122D70E`. **Reset.**

`00647319` walks `[this+88]` and `vtbl+8` release, then
`00643F13(0)`. **Clear.**

---

## 3. Who constructs `[0x13B8790]` first?

`.text` writes of `[0x13B8790]`:

| Site | Op | Role |
|---|---|---|
| `00487FFE` `004195AF` | store after `0043B570` | **PROVEN** construct |
| `0041765D` `= ebx` (0) | game dtor after `0041915A` | teardown |
| `01228B5B` `= esi` (0) | atexit / BSS | teardown |

`00419697` is only `ecx=0x13B8790; jmp 004195AF`
(wrapper). Its `E8` is the Create Players path.

### 3.1 Create Players

`004166A8` → `0044A530`: slots `0..3` then `4`
(`0044A1A0` alloc `0x22C` `0044BC10` → `0048A210`).

`0044A1A0` for `slot != 4` sets descriptor `+20=1`
(`[player+520]=1`). Then:

```
0048A328  cmp [esi+520], 0
0048A34A  je  0048A39F          // skip GUI
0048A39A  call 00487FB0
```

`00487FB0`:

```
00487FB3  mov ecx, [esi+16]
00487FB7  mov edi, [esi+40]     // slot index
00487FBA  call 00449700         // [ecx+28]
00487FBF  cmp edi, eax
00487FC1  jne 00488010          // skip alloc
00487FC3  push 0x338
00487FC8  call 00BFEA1A
00487FEE  call 0043B570
00487FF9  mov ecx, 0x13B8790
00487FFE  call 004195AF
0048800B  jmp 004374B0
```

`00449700` is `mov eax, [ecx+28]; ret` — type of
`[player+16]` is **PARTIAL**. First-seen New Game
does not crash at `0049F214`, and this is the **only**
non-zero store, so slot 0 **does** take the alloc.
**PROVEN** as the writer; **PARTIAL** as the exact
`[player+16]+28` compare.

Host `CreatePlayers` Notes `0044A530` / `0044BC10` /
`004AE940` only. It does **not** Note `00487FB0` /
`0043B570` / `004195AF`. **LEFTOVER** vs the listing.

### 3.2 `0043B570` is the real ctor

```
0043B597  mov [esi], 0x123177C
0043B5A3  call 0049166E          // this+24, vtbl 012384BC
0043B5BA  call 009E2C80          // font/name at +312
          zero +320…+812
0043B7BA  [+656]=1  [+657]=1
0043B90C  if [0x13B878C]==0
            "PLAYER_GUI_PC" → 0043FF30 / 009ADA40
          004428F0 / 00442770 +608 / +620     // same vectors
          00BFEA1A 84  → 0066EAB7 → +812
          00BFEA1A 104 → 0065431D → +716
          00BFEA1A 104 → 0065431D → +724
          00BFEA1A 104 → 0065431D → +732
          00BFEA1A 104 → 0065431D → +740
          00BFEA1A 112 → 00654392 → +748
          00BFEA1A 0x84→ 006543AF → +756
          00BFEA1A 124 → 006543FF → +764
          then +320… more 0064E1E4 / 0064E24F / …
```

Those `+716`…`+748` pointers are exactly the five
`[+8]=0` stores in `0043A380`. **Meters exist before
Init GUI.** `0043A380` only clears their `+8` latch.

`0049166E` (`this+24`) is the HUD helper `0043A380`
later resets. First construct is the ctor, not Init GUI.

---

## 4. HUD widgets? Not here

| Object | First construct | `0043A380`? |
|---|---|---|
| GUI singleton `0x338` vtbl `0123177C` | `0043B570` | **No** |
| Compiled def `PLAYER_GUI_PC` `[0x13B878C]` | `0043B570` | fallback only |
| Float vectors `+608` / `+620` | `0043B570` `00442770` | **recopy** |
| Health / will / exp meters `0065431D`… | `0043B570` | flag `+8=0` only |
| Type-`0x22` sprites `0041BEB0` (`[obj]=0x22`) | `0043B050` from `0069ECD0` | **No** |
| MiniMap `00437CE0` / `0082BA00` | `SetRegionAsLoaded` after first region | **No** |
| Display name `00435070` | `00435530` Present; skip if `00487DC0` miss | **No** |

`0043B050` is a later HUD *build/draw* that calls
`0041BEB0` many times (type `0x22`). It is **not** on
`0043A380` and **not** on `0049F180`. **PROVEN.**

`00438570` is a **getter** for `[0x13B878C]` (same
fallback bind). Used by meter tick `004385F0` /
`00439F20`, not Init GUI.

---

## 5. Frontend leftover

Frontend (`0042EC7C`) and game HUD are different machines.

| | Frontend | `PLAYER_GUI_PC` |
|---|---|---|
| When | retail `0042EC7C` until Leave | Create Players, then `0043A380` reset |
| Source | `frontend.bin` + `names.bin` | compiled def name `"PLAYER_GUI_PC"` |
| First widget | type 10 `0054E3D0` `PRESS_START` | ctor meters; type `0x22` later |
| Dest | `0041AFA0` / `0054EF00` | not `0041AFA0` |
| Leave | `0042F2A2` tears the 2D tree down | singleton **kept** |

`proofs/audit-frontend-leftover` leftovers
(`CollectFrontendRecords` `+204`, AlignLeft, font 16
fallback, named-screen attach) are **frontend Present**
math. They do **not** become HUD widgets at `0043A380`.
**DISPROVEN** as this function.

Host leftover on the **game** side:

| Site | What | Class |
|---|---|---|
| `InitCharactersAndQuests` | `Note(0043A380)` + `PlayerGuiReady=true` | **LEFTOVER** — no reset / no vector copy / no `[0x13B878C]` |
| `CreatePlayers` | no `0043B570` / `004195AF` | **LEFTOVER** vs first construct |
| `TickPlayerGui` | `Note(0043A080 +164=0)` | **LEFTOVER** tick |
| `00435070` HUD body | still Note | sibling leftover |

Status row `#17` (`PlayerGuiReady` PROVEN site,
bind PARTIAL Note-only) matches this dump: the **call
site** is recovered; the **ctor and widgets** are not
what that flag implements.

---

## 6. What `0043A380` first-seen *does* construct

If `[0x13B878C]` is already set (Create Players ctor):
**nothing new.** Recopy two existing vectors; reset
flags.

If `[0x13B878C]==0` (ctor skipped — not first-seen
New Game): first-seen of this *function* would bind
the compiled `PLAYER_GUI_PC` def via `009ADA40`.
Still **not** widgets. Still **not** frontend.

`hero-inventory-first` already **DISPROVEN** this bind
as `CTCInventory`. Same listing: no `004C9D60`.

---

## Open

| Item | Class |
|---|---|
| Exact `[player+16]` type for `00487FB0` / `00449700` | **PARTIAL** |
| `004A2C80` first-seen take of a *second* `0043A380` | **PARTIAL** |
| Def `+2044` element stride (`0xB21642C9` / sar 6) | **UNREAD** |
| `0043B050` first live `0069ECD0` after Leave | **UNREAD** here (not this fn) |
| What `+657` / meter `+8` mean on first Present | **UNREAD** |
