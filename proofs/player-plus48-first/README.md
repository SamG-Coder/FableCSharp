# First non-zero `player+48` after Leave

Investigation only. No production `src/` edits.

Do **not** treat `006AC910` as the store of `player+48`.
Do **not** invent a taken `004AFCA0` on first GamePump.
Do **not** collapse `player+44` (handle) with `player+52`
(create dest) or with QuestManager `+44` (`proofs/quest-manager-plus44`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: first GamePump `00487DC0` `+44 jmp 00A01B50`
`+48=0` miss. When does `+48` become non-zero? After
`006AC910` Hero create? Blocks `004AFCA0`?

Authority: dump `00A01B50` / `00487DC0` / `004AFCA0`
(`listing-00a00000.txt`, `listing-00480000.txt`,
`listing-00440000.txt`, `listing-004c0000.txt`,
`listing-00680000.txt`, `e8.tsv`);
sibling `proofs/quest-player-sync-skip`;
also `hero-4299-create`, `hero-stats-first`,
`creature-after-leave`;
`docs/PARITY.md` (`004B4490` after `00CB8220` skip);
`docs/runtime/FORWARD_TREE.md` §11;
`EngineLifecycle.PumpQuests` /
`EngineLifecycleTests.Pump_004166E2_is_009E1BC0_minus_game_plus96`.

---

## Verdict

**First GamePump after Leave: `player+48=0`. `00A01B50`
returns 0. That miss is what skips `004AFCA0`.** **PROVEN.**

**`+48` becomes non-zero when `00A01B90` first assigns a
live Thing into `player+44`.** That assign is **`00487CF0`
at `00487D56`**, on the same `0048A070` stack as a
**successful** `00489D40` / `006AC910`. **PROVEN** store.
**DISPROVEN** as a write *inside* `006AC910`.

`006AC910` only returns the Hero pointer. `00489D40` stores
it at **`player+52`** (`0048A027`). `0048A0EA 00487CF0`
then copies that pointer into **`+52` and `+44`**. The
`+44` assign allocates the control block at **`+48`**.

Init Characters’ first `00489D40` **misses** (no holy site,
`[0x13B8647]==0`). First type-1 therefore still sees
`+48=0` and **blocks** `004AFCA0`. Later Lookout
`GuildArrivalHSP` create is the first non-zero. **PROVEN**
order; exact WorldFrame of the later pump **UNREAD**.

| Claim | Class |
|---|---|
| First GamePump `00487DC0` → `00A01B50` `+48=0` miss | **PROVEN** |
| That miss skips `004AFCA0` (`004B455B je`) | **PROVEN** |
| `+145` / `+142` cause the first skip | **DISPROVEN** (no Thing) |
| `006AC910` writes `player+48` | **DISPROVEN** (0 `00A01B90` in that fn) |
| After successful create, `+52` first, then `+44`/`+48` | **PROVEN** same stack |
| First non-zero `+48` is that `00487CF0` | **PROVEN** writer. Reach: after Lookout create, **not** Leave / first pump |
| Next type-1 takes `004AFCA0` | **PROVEN** gates. Frame index **UNREAD** |

Host `PumpQuests` always notes `"00A01B50 +48=0 miss"` /
`"004AFCA0 skip"`. Correct for first-seen. After bind it is
**LEFTOVER** vs native (`quest-player-sync-skip`).

---

## Layout (`00A01B50`)

Dump `listing-00a00000.txt`:

```
00A01B10  [this]=0x129C95C; [this+4]=0     ; ctor
00A01B50  ecx=[this+4]
          if 0 → eax=0                    ; MISS
          else eax=[ecx]                  ; Thing*
00A01B90  assign Thing* into [this+4]
          Thing==0 → leave [this+4]=0
          else alloc/share 8-byte control
            [block]=Thing; [this+4]=block
00487DC0  add ecx, 44
00487DC3  jmp 00A01B50                    ; alias 00487DD0
```

Player slot from `0044BC10`:

```
lea edi, [esi+44]
call 00A01B10
mov [edi], 0x1231C4C                      ; overwrite vtbl
```

| Offset | Object | Meaning |
|---|---|---|
| `player+44` | 8-byte handle | `00A01B50` `this` |
| `player+48` | `[handle+4]` | control block **or 0** |
| `[+48]+0` | control | Thing* |
| `player+52` | sibling handle | CreateCharacter dest |

**`+48==0` ⇔ `00A01B50(player+44)` miss.** Not a Thing field.
Not QuestManager `+48`. Not `world+48`.

---

## First GamePump (`00487DC0` / `004AFCA0`)

`004A5A40` type-1, `[world+248]=0` / `[world+260]=0` →
only `E8` of `004B4490` at `004A5D88`. Tail:

```
004B4550  mov ecx, ebp            ; slot from 00449970
004B4552  call 00487DC0           ; +44 → 00A01B50
004B4557  mov edi, eax
004B4559  cmp edi, ebx            ; ebx=0
004B455B  je 004B4589             ; (1) FIRST-SEEN
004B455D  test [edi+145], 0x01
004B4564  jne 004B4589             ; (2) dead
004B4566  movsx edx, [edi+142]
004B456D  cmp edx, [esi+144]
004B4573  je 004B4589             ; (3) already synced
004B4575  mov ecx, esi
004B4577  call 004AFCA0
```

`e8.tsv`: `004B4577` and `00524375` are the only `E8` of
`004AFCA0`. First-seen never reaches either: `(1)` fires.

`004AFCA0` itself re-does `00449970` / `00487DC0` and would
also `je` on 0. Irrelevant: the outer `E8` is not taken.

Ctor `004B46BF` `[QM+144]=0xFFFFFFFF`. A live Thing with
`+142=0` would **not** take (3). First-seen never gets there.

---

## Who zeros `+48` before first pump

| Site | Store | Result |
|---|---|---|
| `0044BC10` slot ctor | `00A01B10` at `+44` | `+48=0` **PROVEN** |
| `0048A210` Create Players | temp `00A01B90(0)` then `lea ecx,[esi+44]; 00A01B90` | still 0 **PROVEN** |
| `0049F180` → `0048A070` → `00489D40` | holy-site miss, `[0x13B8647]==0` → `ret 0`; no `006AC910` | `+44` still 0 **PROVEN** |
| same `0048A070` after miss | `00A01B50(+52)=0` then `00487CF0(0)` | still 0 **PROVEN** |

PlayAVI `006286F0` `00487DC0` is a lookup, not a bind.
**DISPROVEN** as fill.

---

## When `+48` becomes non-zero

### `006AC910` does not write it

Dump `006AC910`–`006ACA13`: `004C7380` size `0x208`,
`0052AB20`, `006A9DD0`, `004C9CA0`. **No** `00A01B90`.
**No** `lea …+44`. Returns Thing* in `eax` / `ebp`.

Only `E8` of `006AC910`: `00489FC1` (`CPlayer::CreateCharacter`)
and leftover `0089F660`. **PROVEN.**

### Create writes `+52`, not `+44`

`00489D40` success tail (`listing-00480000.txt`):

```
00489FC1  call 006AC910
          mov esi, eax                  ; Hero
0048A00B  push esi
0048A018  call 00A01B90                 ; temp
0048A027  lea ecx, [ebp+52]
0048A02A  call 00A01B90                 ; player+52 = Hero
```

No `lea ecx,[ebp+44]` assign on this path. The `+44`
reads at `00489D48` / `00489E9F` are lookups.

### `00487CF0` is the `+48` writer

Only `E8` of `00489D40` is `0048A0AF` inside `0048A070`.
After create (hit or miss):

```
0048A0E0  mov ecx, edi                  ; +52
0048A0E2  call 00A01B50
0048A0E7  push eax
0048A0EA  call 00487CF0
```

`00487CF0`:

```
00487D20  lea ecx, [esi+52]
00487D23  call 00A01B90                 ; +52 = arg
00487D56  lea ecx, [esi+44]
00487D59  call 00A01B90                 ; +44 = arg  → +48 control
```

On success `arg` is the `006AC910` Thing. `00A01B90` with
non-null `edi` does `00BFEA1A(8)` and `mov [esi+4], eax`
(`00A01BEE`). That `[esi+4]` **is** `player+48`.

On the Init-Characters miss, `arg` is 0 → `00A01BCD`
`test edi,edi; je` leaves `+48=0`.

Other `E8` of `00487CF0`: `00487F2C` (persist
`00487C20` hit), `0089F882`. **LEFTOVER** vs first fill.

---

## Timeline (no-save New Game)

```
0044BC10 / 0048A210          player+48 = 0
00416953 LoadWorld
  0049F180 Init Characters
    00487DC0 miss
    0048A070 → 00489D40 miss   // still +48=0
004189C2 first pumps
  type-1 004A5A40
    004B4490
      00487DC0 +44 jmp 00A01B50
      +48=0 → eax=0
      004AFCA0 SKIP            // THIS MISS
… later region / GuildArrivalHSP …
  0048A070
    00489FC1 006AC910          // Hero Thing; does not store +48
    0048A02A player+52 = Hero
    0048A0EA 00487CF0
      00487D59 player+44 = Hero
      00A01B90 → player+48 ≠ 0 // FIRST NON-ZERO
next type-1 004B4490
  00A01B50 hit
  ctor 004C90F0 +142=0, 004C90FD +145=0x04 (bit0 clear)
  QM+144 still -1 → 004B4577 004AFCA0 RUNS
```

Thing ctor `004C8FB0` (`004C90F0` / `004C90FD`) **PROVEN**.
First taken `004AFCA0` still walks empty `QM+92` / `QM+96`
sentinels (`quest-player-sync-skip`). List fill **UNREAD**.

`0051FD80` PlayerCreature bind also `00487DC0`. Lookout TNG
has no `PlayerCreature`. **DISPROVEN** as this fill.

---

## Host

`PlayerSlotPlus44Offset=44`. `PumpQuests` notes
`00487DC0 +44 jmp 00A01B50`, `00A01B50 +48=0 miss`,
`004AFCA0 skip`. Test asserts those events. **PROVEN**
first-seen.

Do not store Hero into `+44` inside `006AC910`. Do not
call `004AFCA0` on the first type-1. Do not treat
`player+52` as the GamePump handle.
)
