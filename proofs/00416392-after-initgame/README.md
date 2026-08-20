# `00416392` after Init Game / before first region

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld` →
Init Game `004184BD` → vtbl+32 `00416953` → suffix
`0049BA70` / **`00416392`** / `004AE9D0`. First region is
later `00501450`, not this site.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: what is `00416392`? First-seen value after Init
Game / before first region? Host leftover if any? Related
to `004FDBC0` construct gate?

Authority: Fable.exe dump `listing-00400000.txt`
(`00416392` / `0041890E` / `00416F67` / `004172F5` /
`00416602` / `00418418`), `listing-00480000.txt`
(`0049E200` / `004AE9D0` / `0049D870`),
`listing-00500000.txt` (`0051E530` / `005223F0`).
Siblings: `tng-spawn` §4–6, `player-bind-world`,
`004FDBC0-open`, `004FDBC0-vs-host`,
`host-tng-construct-early`, `initgame-after-leave-order`,
`dummy-pumps-before-region`, `first-region-after-leave`.
Host notes only: `EngineLifecycle.WorldThingCountFn` /
`FinishInitGameAfterWorld` / `PlayerBindSlot1`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| What is `00416392`? | Game thiscall **count getter**. `+90394==0` → `[game+36]` world → `0049E200` → `0051E530([world+80]) + [0x13B89BC]` (WorldFrame). Alt arm returns **1** (input `+56/+57` both 0). **Not** a TNG open, construct, region load, or Hero bind. | **PROVEN** |
| First-seen after Init Game / before first region? | Site **`0041890E`**. Gate `+90394==0`. WorldFrame addend **0**. Walk addend **0** on the working model. Stored `player+9840` **0**. | **PROVEN** site / gate / frame; walk **PARTIAL** live; sum **0** working model **PROVEN** as comments + empty-list dump |
| Is it a loader / first region / `00DBDE40`? | **No.** After `00416953` / `004BBC00`, before dummy `004189C2` and before `00501450`. | **DISPROVEN** |
| Host leftover? | `PlayerBindSlot1 = WorldFrame` instead of `0051E530 + WorldFrame`. Init **MATCH** (both 0). Later **DIVERGE** if the walk ≠ 0. Host does not call `0051E530`. | **PROVEN** leftover store; Init **MATCH** |
| Related to `004FDBC0` construct gate? | **Census after** that open, **not** the gate. Gate is `005223F0` `[manager+128]==1`. First-seen count **0** is **PARTIAL** evidence the gate did not fill countable Things. | **PROVEN** timing; live `+128` **UNREAD**; count→gate **PARTIAL** |

---

## Verdict

**`00416392` is WorldThingCount, not construct.**

No-save Init Game suffix (`[0x13B8648]==0`) calls it once
at `0041890E` with `ecx=game`. First-seen `game+90394` is
**0** (dump has **cmp** only, no `mov […+90394]`), so the
body is the jump into `0049E200`. That returns the
countable-Thing walk plus WorldFrame.

WorldFrame `[0x13B89BC]` is still BSS **0**: unique `inc`
is `004A5E10` at the end of world tick `004A5A40`, which
has not run. Dummy pumps and first region have not run.

`eax` stored at `004AE9D0` `+9840` is therefore **0** iff
`0051E530([world+80])` is 0. The only earlier TNG construct
candidate is `004FDBC0` inside `00507C30`, gated by
`[manager+128]`. That live byte is **UNREAD**. Empty
countable list + frame 0 is the working New Game model.

Host leftover is the **slot write**, not a missing
construct: `FinishInitGameAfterWorld` /
`AdvanceGameTicks` set `PlayerBindSlot1 = WorldFrame`.
Do **not** “fix” this site by calling `LoadSingleThing`.

---

## Dump — `00416392`

`listing-00400000.txt`:

```
00416392  xor dl, dl
00416394  cmp [ecx+90394], dl
0041639A  je 004163AF
0041639C  mov eax, [0x13B8388]
004163A1  cmp [eax+56], dl
004163A4  jne 004163AF
004163A6  cmp [eax+57], dl
004163A9  jne 004163AF
004163AB  xor eax, eax
004163AD  inc eax
004163AE  ret
004163AF  mov ecx, [ecx+36]     ; world = [game+36]
004163B2  jmp 0049E200
```

`.text` `+90394` sites: **two cmps** (`00416394`,
`00418418`). No write with that displacement. Game ctor
size `0x161E8` (`0042F4B1`) covers the byte; first-seen
**0**. **PROVEN**.

`00418418` is a later pump helper (`+90394` then
`[game+72]>1` then input `+56/+57`), **not** Init Game.

`0049E200` (`listing-00480000.txt`):

```
0049E200  mov ecx, [ecx+80]     ; thing manager
0049E203  call 0051E530
0049E208  mov ecx, [0x13B89BC]  ; WorldFrame
0049E20E  add eax, ecx
0049E210  ret
```

`0049D870` is `mov eax, [0x13B89BC]` (`ret`). Same dword.

`0051E530` (`listing-00500000.txt`):

```
0051E530  edi = ecx
          eax = [edi+24]        ; list sentinel
          esi = [eax]
          ebp = 0
          if esi == eax: return 0
0051E540  ecx = [esi+8]         ; CThing
          test [ecx+145], 1
          jne skip
          call [vtbl+92]
          add ebp, eax
          esi = [esi]
          cmp esi, [edi+24]
          jne 0051E540
          return ebp
```

Empty circular list → **0**. Flagged Things
(`[thing+145] & 1`) do not add. `vtbl+92` identity
(1 vs subtree) **UNREAD**; unused if the list is empty.

---

## Site after Init Game / before first region

`004184BD` after vtbl+32 `00416953` / `004BBC00 ret 4`.
No-save `[0x13B8648]==0` only (`004188EC`).

```
004188F8  push ebx                    ; 0
004188F9  push 60
004188FB  lea ecx, [esi+90488]
00418901  call 0049BA70
00418906  push [esi+90428]            ; arg3 of 004AE9D0
0041890C  mov ecx, esi                ; game
0041890E  call 00416392               ; ← THIS
00418913  push eax                    ; arg2 = count
00418914  push [esi+72]               ; arg1
00418917  lea ecx, [esi+80568]
0041891D  call 004AE9D0
          default_user.ini / user.ini
```

`004AE9D0`:

```
if ![this+9826]: ret 12
[+9836] = arg1     ; [game+72]     first-seen 0
[+9840] = arg2     ; 00416392 eax  first-seen 0
[+9844] = arg3     ; [game+90428]  first-seen 0
ret 12
```

Create Players already set `+9826=1`, so the three writes
run. **PROVEN**.

Four `E8 00416392` in the PE (`listing-00400000`):

| Site | When vs first region |
|---|---|
| **`0041890E`** | Init Game suffix — **this question** |
| `00416F67` | `00416E78` prefix; only after catchup `004AEAA0` |
| `004172F5` | `0041726D` after a consumed tick; second bind |
| `00416602` | helper `004165E8`; not the suffix |

`00416E78` is **not** first-seen. First `0041674A` is 0 so
`00418289` skips vtbl+24 (`player-bind-world`).

Timeline (no `00DBDE40`):

```
00416953  Loading world
  00507C30
    004FDBC0  prox .tng open          ← construct gated
    00509982  region graph
  Set Static Map
  0049F180  Init Characters           ← no 006AC910
004BBC00  ret 4
0049BA70 / 00416392 / 004AE9D0        ← HERE; CurrentRegion still dummy
004189C2  dummy pumps  index 0        ← still not a region
later 00501450 → 00500540(1,0,0)      ← first real region
  006C2170  ContainsMap construct
```

---

## First-seen value

| Addend | At `0041890E` | Class |
|---|---|---|
| Gate `+90394` | 0 → take `0049E200`, **not** return 1 | **PROVEN** |
| `[0x13B89BC]` WorldFrame | 0 (no `004A5E10` yet) | **PROVEN** |
| `0051E530` walk | 0 if `[manager+24]` empty / all flagged | **PARTIAL** live; empty dump path **PROVEN** |
| `eax` / `+9840` | **0** on that sum | working model **PROVEN**; live walk **PARTIAL** |

`0049F180` does **not** construct Hero (`00489D40` ret 0).
Create Players is five `0x22C` slots, not `[world+80]`.
Dummy `WorldMap+156=0` has **no** `006C2170` objects.
So the only way `eax≠0` here is a taken `004FDBC0`
`+128==1` fill of unflagged Things **or** some other
unread insert into `[manager+24]`.

---

## Related to `004FDBC0` construct gate?

**Timing: yes. Identity: no.**

`004FDBC0` is earlier, inside `00507C30` / `"Load global
things"` (`[0x13B8609]==0`). It **always** opens
`LookoutPoint.tng` first (`ebx=1`). Construct is
`005223F0`:

```
005223F7  eax = [esi+128]
005223FF  cmp eax, 1
00522407  jne 00522502          ; open + drop
0052249F  call 00521AE0         ; NewThing walk
```

Live `[manager+128]` on that first call: **UNREAD**.

| If `+128` | Then at `0041890E` |
|---|---|
| ≠1 (working model) | open/drop; walk 0; `00416392==0` |
| ==1 and Things countable | walk >0 unless `[+145]&1`; `eax≠0` |

First-seen `00416392==0` is therefore **PARTIAL** evidence
the gate did **not** fill the countable list. It is **not**
a dump of `+128`. Flagged Things or a different list would
also yield 0.

Do **not** treat `00416392` as the construct gate. Do
**not** treat `004FDBC0` as this count.

Host `LoadGlobalThingsFile` parses 151 / ~21746 into
`GlobalThings` and does **not** `LoadSingleThing`. That
**MATCH**es the skip model. `EnsureLevels` WAD+`_RT.stb`
during the same host call is a **different** leftover
(`004FDBC0-vs-host`).

---

## Host leftover

`EngineLifecycle.FinishInitGameAfterWorld`:

```
PlayerBindSlot0 = GamePlus72;     // MATCH +9836
PlayerBindSlot1 = WorldFrame;     // leftover: native is 00416392 eax
PlayerBindSlot2 = 0;              // MATCH first-seen +9844
```

`AdvanceGameTicks` repeats `PlayerBindSlot1 = WorldFrame`
after `0041726D`. Native `004172F5` pushes a **fresh**
`00416392` (walk **+** frame).

| When | Native `+9840` | Host `PlayerBindSlot1` | Class |
|---|---|---|---|
| Init Game `0041891D` | 0 + 0 | 0 | **MATCH** |
| After ticks, walk still 0 | WorldFrame | WorldFrame | **MATCH** |
| After ticks, walk ≠ 0 | walk + frame | frame only | **DIVERGE** |

Tests (`InitGame_004184BD_after_00416953_reserves_then_user_ini`,
`First_pump_0041674A_is_0_so_00418289_skips_00416E78`) prove
the **host** slot is 0 after `RequestNewGame` `Pump()`, and
that `LoadRegionFn` / `00DBDE40` are absent. They do **not**
execute `0051E530`.

Leftover that is **not** this VA: implementing construct on
`LoadGlobalThingsFile` to “make the count nonzero.” That
would be the unread `+128==1` arm. Do not apply.

---

## Not these

| Claim | Class |
|---|---|
| `00416392` opens / constructs TNG | **DISPROVEN** |
| `00416392` is `004AE9D0` / Hero / `006AC910` | **DISPROVEN** |
| `00416392` is first pump / first region / `00501450` | **DISPROVEN** |
| `00416392` is `004FDBC0` / `005223F0` / `0051FD80` | **DISPROVEN** |
| First-seen return 1 (`+90394` arm) | **DISPROVEN** (gate 0) |
| StartOakVale / `00DBDE40` on this walk | **DISPROVEN** |

---

## Do not

- Start Oakvale / `00DBDE40` as New Game.
- Call `00416392` WorldThing *create*.
- Treat first-seen 0 as a live read of `[manager+128]`.
- Store host `PlayerBindSlot1` as proof of the walk (it is
  WorldFrame).
- Insert 21k `GlobalThings` here to “match” a taken gate.
