# `00501450` uses `00449970`/`00487DC0` and does not `E8` `00449D90` on miss

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD`.
Do **not** treat `00501450` as Init Characters.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `00501450` uses `00449970`/`00487DC0` but does not
call `00449D90` on miss. First-seen after Leave? Host leftover?

Authority: Fable.exe dump
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`00501450`…`00501985`), `listing-00480000.txt` (`0049F180` /
`0049F1D7`), `listing-00440000.txt` (`00449D90` / `00449970`),
`listing-00400000.txt` (`00416BCA`), `e8.tsv`;
siblings `proofs/first-region-after-leave`,
`proofs/dummy-pumps-before-region`,
`proofs/host-00501450-timing`,
`proofs/hero-00489D40-retry`,
`proofs/hero-4299-create`;
`EngineLifecycle.LoadFromFirstRealRegion` /
`SpawnHeroFromPlayerStart`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| `00501450` uses `00449970` / `00487DC0`? | **Yes.** `0050146B` / `00501472`. `ecx=[0x13B86A0]+28` | **PROVEN** |
| `00501450` `E8` `00449D90` on miss? | **No.** `ebx==0` → `je 00501495` → `004FEEC0`. Hit path is `004C8CF0(1)` only | **PROVEN** |
| Any `.text` `E8` of `00449D90` from `00501450`? | **0.** Sole dest is `0049F1D7` inside `0049F180` | **PROVEN** |
| `00501450` first-seen after Leave? | **No.** After Init Game + dummy `004189C2`. **0** `E8`/`E9`/imm/vtbl | **PROVEN** skip; **UNREAD** caller |
| First `00449970`/`00487DC0` after Leave? | Loading world `00416BCA` `0049F180(0)` “Init Characters” | **PROVEN** |
| First `00449D90` after Leave? | Same `0049F180` miss (`0049F1C2 je 0049F1CF`) | **PROVEN** |
| Host leftover? | Yes: `SpawnHeroFromPlayerStart` after `006C2170` notes `0049F180` / `00449D90` as if `00501450` miss created Hero | **LEFTOVER** |
| Host `LoadFromFirstRealRegion` pair note? | Notes `00449970`/`00487DC0`, **no** `00449D90` | **MATCH** body |

---

## Contrast (dump)

Init Characters **does** call `00449D90` on miss.
`00501450` does **not**. Same two getters; different miss.

```
0049F180  Init Characters                    // first-seen after Leave
  0049F1B6  call 00449970
  0049F1BD  call 00487DC0
  test eax, eax
  je 0049F1CF
  test [eax+145], 1
  je 0049F1DC
  0049F1D7  call 00449D90                    // PLAYER_HERO → CREATURE_HERO
                                             // only e8.tsv dest of 00449D90

00501450  region enqueue                     // later; caller UNREAD
  00501459  mov eax, [0x13B86A0]
  00501464  mov ecx, [eax+28]
  0050146B  call 00449970
  00501472  call 00487DC0
  mov ebx, eax
  xor ebp, ebp
  cmp ebx, ebp
  je 00501495                                // miss: no 00449D90
  test [ebx+145], 1
  jne 00501495
  call 004C8CF0(1)                           // live Thing only
  00501495  call 004FEEC0(current, 0)
  … 00500540(i,0,0) / RegionGraph / 00500540(saved,0,1)
  00501985  ret
```

`00449970` is `mov eax,[ecx+28]; jmp 004498C0`.
`00487DC0` is `add ecx,44; jmp 00A01B50`.
Neither `E8`s `00449D90`.

`00501450` ends `00501985 ret` / `int3` / `00501990` UpdateNavMaps.
`functions.tsv` size 2248 swallows that next fn. Callee list
`00449970,00487DC0,…` has **no** `00449D90`.

---

## First-seen after Leave (no-save)

```
0042F2A2  Leave frontend
0042F491  Init Game → 004184BD
  00416953  Loading world
    00416ABA  004A1840                       // WLD parse; not 00501450
    [0x13B8648]==0
    00416BC8  push 0
    00416BCA  call 0049F180                  // FIRST pair + FIRST 00449D90
      00449970 / 00487DC0 miss → 00449D90
      00489D40 holy miss → ret 0             // no 006AC910
  user.ini Gameflow                          // 0 E8 00501450
004189C2  dummy pumps                        // 0 E8 00501450
later     00501450                           // UNREAD E8; pair again, no 00449D90
```

`e8.tsv`:

| Dest | Sites |
|---|---|
| `0x00501450` | **none** |
| `0x00449D90` | **only** `0x0049F1D7` |
| `0x0049F180` | `0x00416BCA` (no-save), `0x004A2C80` (save/`[world+258]` arm; not this first-seen) |
| `0x00449970` from `00501450` | `0x0050146B` |
| `0x00487DC0` from `00501450` | `0x00501472` |

Sibling `005025B0` is the same pair then `004FEEC0` /
`00500540` — also **no** `00449D90`. First-seen type-1
skips that parent (`[world+260]=0`).

---

## Host leftover

`LoadFromFirstRealRegion` (`00501450`) notes the pair and
does **not** note `00449D90`. That matches the dump miss.

Leftover is folding Init Characters onto the later region
apply:

- `ApplyLoadJob` → `SpawnHeroFromPlayerStart` notes
  `0049F180` / `00449D90` after `006C2170`.
- Native `00501450` miss never enters that stack.
- First `00449D90` already ran at `00416BCA` and missed
  create. Later `006AC910` is a later take of
  `0048A0AF` (`proofs/hero-00489D40-retry`), **not** a
  `00501450` `E8`.
- Pairing `EnqueueAfterDummy` to the second `Pump` is
  already **DISPROVEN** (`proofs/host-00501450-timing`).

Do **not** invent `00449D90` under `00501450`.
Do **not** treat `00501450` as first-seen after Leave.
