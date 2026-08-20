# Intro leftover after `SneakTo` stub `004C72B0`

Investigation only. No production `src/` edits.

Parent: `proofs/sneakto-intro`. That note closed apply as
thing `vtbl+20` `004C72B0` (`mov al,1; ret 4`). This note
is **what remains after that stub** on intro
`Hero.SneakTo`. Do **not** lerp. Do **not** invent `0.3`
gait. Host `TickMove` Sneak is **Complete**, no XYZ write.

Do **not** start this at Leave / Init Game / first no-save
Present. Runner `00CBFB7D` is later leftover
`Q_NewOakValeIntro` (`00DABAC0` → TNG `NOVI_LiveFather` →
`00DB86B0` → `00CBFB7D`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Sources:

- listing `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00cc0000.txt`
  (`00CC0CB5`–`00CC0F2A`)
- listing `listing-004c0000.txt` `004C72B0` / `004C72C0`
- listing `listing-00680000.txt` `006A9960` / `006A9550`
- listing `listing-00640000.txt` `00661A40`
- dumps `script-runtime/sneakto-token-00cc0cb5-00cc0cb5.md`,
  `sneakto-apply-00cc0e5a-00cc0e5a.md`,
  `sneakto-yield-once-00cc0e96-00cc0e96.md`,
  `sneakto-wait-poll-00cc0f1a-00cc0f1a.md`,
  `sneakto-wait-yield-00cc0ecd-00cc0ecd.md`,
  `sneakto-thing-vtbl20-stub-004c72b0-004c72b0.md`,
  `vtbl-player-thing-vtbl-012457fc-012457fc.md`,
  `vtbl-cthingaicreature-vtbl-0127293c-0127293c.md`,
  `waittask-poll-stub-00661a40-00661a40.md`
- def `assembly/compiled-defs/script/0481-CS_OAKVALE_INTRO_FATHER.md`
- host `EntityTaskQueue.TickMove`, `MovementRuntime.Sneak`,
  `EntityDispatcher` SneakTo
- siblings `proofs/sneakto-intro/`, `entity-task-queue/`,
  `creature-move-first/`, `cs-oakvale-intro-father-lines/`

---

## Verdict

**After the stub, intro `SneakTo` leftover is interpreter
yield plus a discarded dest name. It is not a mesh step,
not a sneak clip, and not a later `vtbl+20` override.**

| Question | Answer | Class |
|---|---|---|
| Apply still `004C72B0`? | `mov al,1; ret 4`. Same slot on player / AI / building. | **PROVEN** stub |
| Mesh leftover of this verb? | No XYZ, no SetMesh, no `006A9960`. Later snap is `.Teleport`. | **DISPROVEN** as SneakTo |
| Anim leftover of this verb? | Mode `2` is pushed onto the stub and discarded. Clips are other verbs (`vtbl+72` / `+76`). | **DISPROVEN** |
| Later override of thing `vtbl+20`? | rdata stays `004C72B0`. No listing store at `012457FC+20`. Intro Hero stays `CThingPlayerCreature`. `+24` FollowNavRoute is the same stub pattern `004C72C0`. | **DISPROVEN** |
| What leftover remains? | dest lookup discarded; FALSE one `vtbl+28`; TRUE leftover `+104` then one `vtbl+28`; idle `00CC7081`. Placement is later Teleport. | **LEFTOVER** |
| Host `TickMove` Sneak? | `Complete = true; return;` no `World.Positions` write. | **MATCH** |
| Invent `0.3` speed? | Intro stores **`0.0`**. Default `0x3E99999A` only when arg1 empty. | **DISPROVEN** as intro gait |

Keep `FirstSeenSneakToAppliesMove=false`.
Keep `IntroSneakSpeed=0`.

---

## 1. Evidence (native after `call [edx+20]`)

### 1a. Stub body (`listing-004c0000.txt`)

```
004C72B0  mov al, 0x01
004C72B2  ret 4
004C72B5  int3
004C72C0  mov al, 0x01     ; FollowNavRoute vtbl+24, same shape
004C72C2  ret 4
```

Dump `sneakto-thing-vtbl20-stub-004c72b0-004c72b0.md`: 2 insns.
Pops one stdcall dword (the dest arg). Returns success.
Does not store XYZ, gait `+176`, `or [this+146],2`, XSEQ, or
`CTCCreatureNavigation`. **PROVEN.**

### 1b. Intro opcode (`listing-00cc0000.txt`)

Token `00CC0CB5` `.SneakTo`. Apply:

```
00CC0DB3  push 2                 ; SneakToMode (WalkTo 0, RunTo 1)
00CC0DBE  call [eax+48]          ; 00664370 wrap, type 0x31
00CC0DC8  call [edi+2048]
… dest: vtbl+280 if marker already matches, else or [ebp+92],16 / vtbl+288
00CC0E35  push [ebp-1640]        ; flags (discarded)
00CC0E3B  fld  [ebp-1664]        ; speed (intro 0.0)
00CC0E51  push 2                 ; gait
00CC0E54  fstp [esp]
00CC0E57  push eax               ; dest
00CC0E5A  call [edx+20]          ; 004C72B0
00CC0E5D  test [ebp+92], 0x10    ; leftover dest-obj cleanup
00CC0E6D  call 004AA840
00CC0E78  call 0099EAE0
00CC0E80  call 00CBEDBA          ; wait?
00CC0E87  jne 00CC0F1A
00CC0E96  cmp [ebp+103], 0       ; FALSE: one vtbl+28 then idle
00CC0F1A  call [eax+104]         ; TRUE leftover poll
00CC0F23  jne 00CC0ECD
00CC0F25  jmp 00CC7081
```

**No** `call [edx+16]`. Sibling dest+gait `006A9960` is not
this handler. **PROVEN** absence.

### 1c. Creature `vtbl+20` dumps (no later override)

| Vtbl | Class | `+16` | `+20` | `+24` |
|---|---|---|---|---|
| `012457FC` | `CThingPlayerCreature` (intro Hero) | `006A9960` | **`004C72B0`** | `004C72C0` |
| `0127293C` | `CThingAICreature` (Father / VILL1) | `008315C0` | **`004C72B0`** | `004C72C0` |
| `0124509C` | `CThingBuilding` | `00838930` | **`004C72B0`** | `004C72C0` |

`.rdata` slot is the stub on every dumped CThing. Text-map
has **no** store to `012457FC+20`. Intro does not swap Hero
`[this]` to another creature vtbl. Other dumps with a
different `+20` (`CTCAnimationComplex` `00686860`, quest /
PlayAVI / landscape) are **other objects**, not this apply.
**DISPROVEN** as a later `vtbl+20` patch.

`006A9960` (player `+16`, **not** SneakTo):

```
006A9960  call 00662930
006A9977  fld [ecx+80]
006A997A  fst [esi+176]
006A999F  or [esi+146], 0x02
```

That gait/moving flag is a **sibling slot**. Pairing it as
SneakTo apply (`COMMAND_MAP.generated.md`) is leftover
commentary, not `00CC0E5A`.

### 1d. TRUE leftover poll (still leftover after stub)

Hero `+104` is `006A9550` `jmp 00661A40`. `00661A40` is
`ret 4` (garbage `al`). Nonzero → `00CC0ECD` skip-key
`00CBEB7E` else one `[0x143E8F8] vtbl+28` then idle.
`FirstSeenSneakToTruePollsArrival=true` /
`FirstSeenSneakToTrueYieldsOnce=true` mean **one leftover
poll**, not “block until mesh arrives.” Arrival is
**DISPROVEN** because apply never started a move.

---

## 2. Original (intro lines)

`CS_OAKVALE_INTRO_FATHER` vector 0
(`0481-CS_OAKVALE_INTRO_FATHER.md`):

| After | Raw | Wait | This verb moves? |
|---|---|---|---|
| `Hero.WaitTask FOO` | `Hero.SneakTo MK_OVIF_HERO4,0.0,FALSE,FALSE,FALSE` | no | **no** |
| (later) | `Hero.Teleport MK_OVIF_HERO4` | n/a | **yes** (`0089B780`) |
| `NoLoadUseCamera CAM_OVIF_SHOT6` | `Hero.SneakTo MK_OVIF_HERO5,0.0,FALSE,FALSE,FALSE` | no | **no** |
| last of vector 0 | `Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE` | leftover poll | **no** |

Vector 1 skip-list (does **not** auto-run):
`Hero.Teleport MK_OVIF_HERO5`. First-seen skip false.

Neighbours that are **not** SneakTo leftover of the stub:

| Raw | Slot |
|---|---|
| `Hero.PlayAnimation CS_LOOK_LEFT` / `ST_IDLE_SUBTLE` / `CS_TIRED` | thing `vtbl+72` `004C7470` |
| `Father.PlayCombatAnimation TURNING_AC90` | `vtbl+76` |
| `Create … VILL1` | `vtbl+364` `008A9100` |
| `VILL1.WalkTo MK_OVI_ID_VW1` | same `+20` stub, mode 0 |

Intro speed is explicit **`0.0`** (`IntroSneakSpeed=0`).
Default dword `0x3E99999A` at `00CC0D27` is only the empty
arg1 fill. Do not treat `0.3` as the intro sneak gait.

---

## 3. Host (`EntityTaskQueue` / dispatcher)

`MovementRuntime.Sneak` keeps the script speed and does
**not** `ResolveSpeed(0→0.3)`:

```
// 004C72B0 stub: keep script 0.0. Do not
// invent ResolveSpeed(0→0.3) locomotion.
return Queue(actor, EntityTaskKind.Sneak, marker, dest, speed);
```

`EntityTask.TickMove`:

```
if (Kind == EntityTaskKind.Sneak)
{
    Complete = true;
    return;
}
```

No `World.Positions` write on Sneak. **MATCH** vs
`al=1; ret 4`. Walk / Run / Follow / NavRoute still lerp
in the same function; that is **not** intro SneakTo.

Dispatcher FALSE wait: `YieldOnce` `"SneakTo vtbl+20 stub"`.
TRUE wait: `YieldOnce` `"SneakTo TRUE leftover once"`.
Parse of intro `0.0,TRUE` speed is `0f`. **MATCH.**

---

## 4. Gap (leftover after stub)

Format: **Evidence → Original → Host → Gap.**

### 4a. Mesh

| | |
|---|---|
| Evidence | stub writes nothing; no `call [edx+16]`; no SetMesh |
| Original | three intro Sneaks; snap is later `.Teleport` `0089B780` |
| Host | Sneak `TickMove` Completes without XYZ |
| Gap | **none as locomotion.** Host `SeedStart` / `Destinations[actor]` may still record a dest name if Positions is empty — leftover **record**, not a step. **DISPROVEN** mesh leftover of this verb. |

### 4b. Animation

| | |
|---|---|
| Evidence | no `vtbl+72` / `+76` / `+80` on `00CC0CB5`; mode 2 discarded |
| Original | sneak-looking clips are separate `.PlayAnimation` / `.PlayCombatAnimation` lines |
| Host | `EntityTaskKind.Sneak` is movement slot, not `TickAnim` |
| Gap | **none as sneak clip.** Do not pair this opcode to an XSEQ or crouch mesh. **DISPROVEN.** |

### 4c. Later `vtbl+20` override

| | |
|---|---|
| Evidence | player / AI / building `+20` all `004C72B0`; `+24` also stub; no rdata store |
| Original | intro Hero remains `CThingPlayerCreature` `012457FC` |
| Host | no host vtbl swap |
| Gap | **none.** A later adult / Lookout morph is a different leftover, and AI `+20` is the same stub anyway. **DISPROVEN.** |

### 4d. What *does* remain

| Leftover | Native | Host | Class |
|---|---|---|---|
| Dest name lookup `vtbl+280/288` | pushed, stub pops it | `Destinations` / `ScriptSneakTo` log | **LEFTOVER** dest record |
| Dest-obj cleanup `004AA840` | after stub if bit 16 | n/a | **LEFTOVER** stack |
| FALSE yield `00CC0E96` | one `vtbl+28` then `00CC7081` | `YieldOnce` | **MATCH** leftover |
| TRUE poll `00CC0F1A` | `+104` `00661A40` `ret 4` once | `YieldOnce` leftover once | **MATCH** leftover; **DISPROVEN** arrival |
| Later Teleport `MK_OVIF_HERO4` / skip `HERO5` | `0089B780` writes pose | `World.Teleport` | **LEFTOVER** placement, **other opcode** |
| `COMMAND_MAP` “dest+gait `006A9960`; TickMove” | sibling `+16`, not `00CC0E5A` | comment still names TickMove | **LEFTOVER** pairing |
| Empty-arg default `0.3` | `00CC0D27` if arg1 empty | `SneakToDefaultSpeed` then parse overwrite | **PROVEN** default; **DISPROVEN** as intro (`0.0`) |

`Tick` still *calls* `TickMove` for `EntityTaskKind.Sneak`.
The body now returns Complete. Calling the method is a
host extra; the XYZ lerp is gone. **MATCH** effect.

---

## Classifications (short)

1. **Stub — `004C72B0` `al=1; ret 4`. PROVEN.** Same
   dword on player, AI, building. No later `vtbl+20`
   override on intro Hero. **DISPROVEN.**
2. **Mesh leftover of intro SneakTo — DISPROVEN.**
   Placement is later `.Teleport`. Do not lerp.
3. **Anim leftover of intro SneakTo — DISPROVEN.**
   Mode 2 discarded. Clips are other verbs.
4. **Remaining leftover — dest lookup discarded +
   one leftover yield (TRUE also leftover `+104`).
   LEFTOVER.** Host Sneak `TickMove` Complete **MATCH**.
5. **Do not invent `0.3`.** Intro stores `0.0`.

Do not start New Game locomotion at `Hero.SneakTo`.
Do not pair this opcode to `006A9960`, an XSEQ, or a
crouch mesh. Keep `FirstSeenSneakToAppliesMove=false`.
