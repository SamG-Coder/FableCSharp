# Host leftover at `0049E200` after Init Game

Investigation only. Production `src/` and `tests/` were
not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld` →
Init Game `004184BD` → vtbl+32 `00416953` → suffix
`0049BA70` / `00416392` / **`0049E200`** / `004AE9D0`.
First region is later `00501450`, not this site.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `0049E200` thing count after Init Game. Host
leftover vs MATCH? First leftover field?

Authority: sibling `proofs/0049E200-thing-count` (body,
`[world+80]`, empty ctor, dump-static insert). Dump
`listing-00400000.txt` (`00416392` / `0041890E` /
`004172F5`), `listing-00480000.txt` (`0049E200` /
`004AE9D0` / `0049D870` / `004A5E10`),
`listing-00500000.txt` (`0051E530` / `005223F0` /
`00523540`). Host notes only:
`EngineLifecycle.FinishInitGameAfterWorld` /
`AdvanceGameTicks` / `PlayerBindSlot1` /
`WorldThingCountApply` / `WorldThingCountWalk`.
Siblings: `00416392-after-initgame`,
`005223F0-plus128-gate`, `player-bind-world`.

---

## Verdict table

| Claim | Answer | Class |
|---|---|---|
| Native Init `0049E200` | `eax = 0051E530([world+80]) + [0x13B89BC]`. Site `0041890E` → store `004AE9D0` `+9840`. | **PROVEN** |
| Frame addend at Init | WorldFrame still BSS **0** (`004A5E10` has not run). | **MATCH** (host `WorldFrame` ctor 0) |
| Walk addend at Init | Dump-static **not** 0: leftover `[manager+128]==1` takes `00521AE0` and splices `+24`. Live occupancy **UNREAD**. | dump-static walk **DISPROVEN** empty; live **UNREAD** |
| Host vs native formula | Host never calls `0051E530`. `FinishInitGameAfterWorld` / `AdvanceGameTicks` write `PlayerBindSlot1 = WorldFrame`. | **LEFTOVER** store **PROVEN** |
| Host vs native Init **value** | Host `+9840=0`. Native `+9840=walk+0`. Equal **only if** walk 0. | Init **MATCH** **DISPROVEN** dump-static; host 0 is **LEFTOVER** |
| First leftover field | **`[player+9840]`** (`PlayerBindSlot1`). First leftover **operand** of `0049E200` is the `+24` walk, not WorldFrame. | **PROVEN** |

---

## Direct answers

### Host leftover vs MATCH?

**Leftover. Not MATCH.**

Native after Init Game (`0041890E` / `0041891D`):

```
0049E200  mov ecx, [ecx+80]          ; CThingManager
          call 0051E530              ; walk [manager+24]
          add eax, [0x13B89BC]       ; WorldFrame
004AE9D0  [player+9840] = that eax
```

Host `FinishInitGameAfterWorld`:

```
Note(WorldThingCountFn, … "00416392 … → 0049E200");
Note(WorldThingCountApply, … "0049E200 0051E530+[0x13B89BC]");
PlayerBindSlot0 = GamePlus72;     // +9836
PlayerBindSlot1 = WorldFrame;     // leftover: native is 0049E200 eax
PlayerBindSlot2 = 0;              // +9844
```

`WorldThingCountWalk` (`0051E530`) is a dead const. No
`CThingManager`. No `[world+80]`. No `+24` walk.

Sibling `00416392-after-initgame` treated Init **MATCH**
(both 0). That MATCH is **DISPROVEN** against the
`+128` writer (`0049E200-thing-count` /
`005223F0-plus128-gate`). Keep the leftover store; drop
the numeric MATCH.

| Piece | Native | Host | Class |
|---|---|---|---|
| Call order `0049BA70` → `00416392` → `004AE9D0` | yes | Note order yes | **MATCH** order |
| `+90394==0` → `0049E200` | yes | Note yes | **MATCH** gate |
| WorldFrame addend at Init | 0 | 0 | **MATCH** |
| `0051E530([world+80])` | dump-static ≠ 0 | never called; treat 0 | **LEFTOVER** |
| `+9840` at `0041891D` | walk + 0 | `WorldFrame` = 0 | **LEFTOVER** (Init MATCH **DISPROVEN** dump-static) |
| `+9836` / `+9844` | `[game+72]` / `[game+90428]` | `GamePlus72` / 0 | **MATCH** first-seen |

`LoadGlobalThingsFile` parse-without-`LoadSingleThing`
is the **same leftover** as a zero walk. Do not “fix”
`0049E200` by inserting 21k `GlobalThings` here, and
do not “fix” it by writing `PlayerBindSlot1 = WorldFrame`
as if that were `[world+80]`.

Live RAM occupancy of `+24` after `004FDBC0` is
**UNREAD**. Dump-static path fills it.

### First leftover field?

**`[player+9840]` / host `PlayerBindSlot1`.**

`004AE9D0` three-dword store (`listing-00480000`):

```
004AE9D0  if ![ecx+9826]: ret 12
          [ecx+9836] = arg1          ; [game+72]
          [ecx+9840] = arg2          ; 00416392 / 0049E200 eax
          [ecx+9844] = arg3          ; [game+90428]
```

Create Players already set `+9826=1`. **PROVEN.**

| Field | Native first-seen | Host | Class |
|---|---|---|---|
| `+9836` | `[game+72]` = 0 | `PlayerBindSlot0 = GamePlus72` | **MATCH** |
| `+9840` | `0049E200` eax | `PlayerBindSlot1 = WorldFrame` | **LEFTOVER** (first leftover field) |
| `+9844` | `[game+90428]` = 0 | `PlayerBindSlot2 = 0` | **MATCH** |

First leftover **operand** of `0049E200` itself (not a
player field): `0051E530` walk of **`[manager+24]`**.
WorldFrame `[0x13B89BC]` is **not** leftover at this
site (addend 0 on both sides). The construct-gate field
that makes the walk nonzero dump-static is leftover
**`[manager+128]==1`** (`005223F0-plus128-gate`) — that
is the cause, not the stored count.

`AdvanceGameTicks` after `0041726D` repeats the same
leftover: native `004172F5` pushes a **fresh**
`00416392` (walk + frame); host writes `WorldFrame`
again.

---

## Native site (no `00DBDE40`)

`listing-00400000` Init Game suffix, `[0x13B8648]==0`:

```
00418901  call 0049BA70
00418906  push [esi+90428]
0041890C  mov ecx, esi
0041890E  call 00416392               ; jmp 0049E200
00418913  push eax                    ; arg2 → +9840
00418914  push [esi+72]               ; arg1 → +9836
00418917  lea ecx, [esi+80568]
0041891D  call 004AE9D0
```

`listing-00480000`:

```
0049E200  mov ecx, [ecx+80]
0049E203  call 0051E530               ; unique E8 of 0051E530
0049E208  mov ecx, [0x13B89BC]
0049E20E  add eax, ecx
0049E210  ret
```

`listing-00500000` `0051E530`: `[edi+24]` circular; empty
sentinel → 0; else add `vtbl+92()` unless `[thing+145]&1`.

Ctor `00A373B0` empty **PROVEN**. At `0041890E` empty
**DISPROVEN** dump-static (`005223F0` leftover `+128==1`
→ `00521AE0` / `0051FD80` / `00A371F0` splice). Exact
sum **UNREAD**.

---

## Host tests do not prove the walk

`InitGame_004184BD_after_00416953_reserves_then_user_ini`
proves Note order `00416392` before `004AE9D0`.
`First_pump_0041674A_is_0_so_00418289_skips_00416E78`
asserts host `PlayerBindSlot1==0`. Neither executes
`0051E530`. Host slot 0 is **not** proof of native
`0049E200==0`.

---

## Not these

| Claim | Class |
|---|---|
| Host `PlayerBindSlot1=WorldFrame` **is** `0049E200` | **DISPROVEN** |
| `[world+80]` / `CThingManager+24` is WorldFrame | **DISPROVEN** |
| Init `+9840` MATCH because both 0 | **DISPROVEN** dump-static; live **UNREAD** |
| First leftover field is `+9836` or `+9844` | **DISPROVEN** |
| First leftover operand is WorldFrame | **DISPROVEN** (frame MATCH 0; leftover is the walk) |
| `0049E200` constructs / opens TNG | **DISPROVEN** |
| StartOakVale / `00DBDE40` on this walk | **DISPROVEN** |
| Implement `LoadSingleThing` here to “match” leftover `+128` | leftover theater **DISPROVEN** as this VA |

---

## Do not

- Start Oakvale / `00DBDE40` as New Game.
- Keep Init MATCH (both 0) against leftover `+128==1`.
- Store host `PlayerBindSlot1` as proof of `0051E530`.
- Call `[world+80]` WorldFrame.
- Treat `0049E200` as Thing *create*.
