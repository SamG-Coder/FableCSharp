# First animation event / notify (footstep, hit) after Leave Frontend

Investigation only. No production `src` edits.

Sibling of `proofs/xseq-first/README.md` (empty 3DAF/XSEQ helper)
and `proofs/xseq-walk-first/README.md` (no walk/idle *cycle*).
This note is the first **named animation event / notify**
(FOOTSTEP, HIT, START/STOP keys on a clip), not a pose sample.

Do **not** start at Oakvale / `CS_WAKING_UP_LOOP` / `3420` /
`WaitForAnimationEvent FOOTSTEP` / `PlayAnimation` `00CC15DA`.
That path is later `Q_NewOakValeIntro` / `CS_OAKVALE_INTRO_FATHER`,
not Leave / Init World / first no-save 3D Present.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

Sources: `proofs/xseq-first/README.md`;
`proofs/xseq-walk-first/README.md`;
`proofs/script-entity-cmds/README.md`;
`docs/runtime/FORWARD_TREE.md` §7;
`docs/runtime/COMMAND_MAP.generated.md` (`WaitForAnimationEvent`);
`XSeqFile.cs` (`FourCcFoot` / `FourCcTmev`);
`AnimationRuntime` in `ExecutionContext.cs`;
`ScriptCommandMap.cs` / `EntityDispatcher.cs`;
`EngineLifecycle.InitWorldInitStages`;
`ScriptRuntimeArchitectureTests`;
listings `004A6E30` / `006FAA90` / `006FABF0` / `006FA4E0` /
`006F9BE0` / `006F9F90` / `006F5C10` / `004AAF60` / `00CC41FC`;
RTTI `CManager@NAnimationEvents` / `CEventCutSceneAnimEvent` /
`CActionEventSound@NCreatureAction`;
strings `UseCompiledAnimationEvents` / `BEGIN_ANIMATION_EVENTS`.

---

## Verdict

**After Leave there is no footstep or hit *notify*.**

The first animation-event work on the no-save spine is Init
World **construct + optional list load**, not a fire:

1. `006FAA90` two 64-byte `NAnimationEvents::CManager`
   (`vtbl 01264974`).
2. `006FABF0` (`cl=1`) loads **Game** then **Sound** lists
   through `006FA4E0`.
3. `006F5C10` registers a type table at `0x13BA8C8` (first
   `0x14C` object `008C2530` vtbl `012808EC`).

Nothing on Leave / first Present **plays a clip**
(`FirstSeenPlaysAnim=false`), so the START/STOP keys never
compare against play time. `WaitForAnimationEvent` never
runs (`FirstSeenCallsPlayAnimationDispatcher=false`).

`FOOTSTEP` is **not** an exe string. Host tests use it as an
isolated `WaitForAnimationEvent` arg. Combat `HIT_*` /
`CEventHitThing` are a **different** family.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  PlayAVI / frontend 2D          // no NAnimationEvents managers
0042F2A2 Leave frontend
  009BE420 + 009BEEB0 Present
  no 006FAA90 / 006FABF0 / 00CC4252
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
    0049E620 Init Mesh Bank → world+60
    00AEAA90 / 00AEAA80 particle bank hooks
    004A757C "Init Animation Event Managers"
      ecx = [world+60] mesh bank
      006FAA90                     // FIRST managers
        alloc 64 vtbl 01264974  [+61]=1 → 006FD460 (game slot)
        alloc 64 vtbl 01264974  [+61]=0 → 006FD460 0x13BABBC (sound)
    004A75AE "Init Animation Events"
      cl=1  006FABF0
        "Loading Game Animation Events"  → 006FA4E0 (game manager)
        "Loading Sound Animation Events" → 006FA4E0 ([0x13BABBC])
      006F5C10                     // type table 0x13BA8C8
        first 0x14C 008C2530 vtbl 012808EC id 1
        then 00867950 / 00857E40 / … (NEntityEvents family)
  00416953 Load world FinalAlbion.wld
004189C2 first pumps
  FirstSeenPlaysAnim=false
  no 0070D580 inner play
  no 00CC41FC / 004AAF60
  no FOOTSTEP / HIT notify
```

`Hero.WaitForAnimationEvent FOOTSTEP` and
`Hero.PlayAnimation CS_WAKING_UP_LOOP` are **not** on this
list. **PROVEN**.

---

## 1. Frontend / Leave fire a notify?

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D UI | **PROVEN** | FORWARD_TREE §4; `xseq-first` |
| Leave teardown constructs `006FAA90` | **DISPROVEN** | `0042F2A2` is fade / `0042EBB6` / black Present. Mesh bank is Init World |
| Frontend / Leave runs `WaitForAnimationEvent` | **DISPROVEN** | `00CBFB7D` off tree; `FirstSeenCallsPlayAnimationDispatcher=false` |
| UI type 6 is an animation event | **DISPROVEN** | glyphs `0054EF00`, not `BEGIN_ANIMATION_EVENTS` |

**Answer:** no. Event managers do not exist until Init World.

---

## 2. What native loads (not a fire)

### Managers `006FAA90`

`ecx` is the mesh bank (`004A75A4` `[world+60]`, same slot
`00AEAA90` uses). Two 64-byte objects, same vtbl
`01264974` (`CManager@NAnimationEvents` `0x01380998`):

```
006FAA9B  push 64 → 00BFEA1A
  [esi]=01264974  [esi+4]=bank
  +8  009E5200 / 009E5C90 name
  +44/+48/+52 = 0     ; event vector empty
  +61 = 1             ; game
  006FD460            ; smart-ptr slot (listing "fsetRect")
006FAB49  push 64 → 00BFEA1A
  same vtbl; +61 = 0  ; sound
  006FD460  ecx=0x13BABBC
```

**PROVEN** construct. **DISPROVEN** as a clip open or a notify.

### Lists `006FABF0` → `006FA4E0`

`cl=1` prints the two loading strings (stage `009E9F40`).
Then `006FA4E0(filename)` on each manager.

`006FA4E0` head:

```
mov al, [0x13B860A]          ; UseCompiledAnimationEvents
test al, al
je  006FA5B1                 ; text BEGIN_ANIMATION_EVENTS
… 00997620 split + 006F9F90  ; compiled binary
```

`0x13B860A` is the dest of ini command `UseCompiledAnimationEvents`
(`0041412D` in `00413C50`). BSS default **0**. First-seen
`userst.ini` / `user.ini` do not set that name
(`ini-activate-quest`). **PROVEN** as the flag. First-seen
value **0** → text arm. **PROVEN** branch.

Text arm (`006FA5B1`):

```
00999230 exists(name)
  miss → 006FA9BB            ; no parse
  hit  → 0099AD80 open
         require "BEGIN_ANIMATION_EVENTS"
         loop "BEGIN_EVENTS:" … "END_EVENTS"
           name + float time + START|STOP
           pack: bit31 = STOP, low 30 = intern id
           006F9430 insert at manager+44
         "END_ANIMATION_EVENTS"
```

Write twin is `006F9BE0` (`EAnimType2` / `" START"` /
`" STOP"`). **PROVEN** format.

Filenames are UTF-16 (`0099B6B0` → `0099B3C0` `eax*2`
walk). Game list `0x0126493C` and sound list `0x01264904`
are prefixed by `0041A0A0` (`0x0122F3E8`). Compiled suffix
push is `0x012649D4`. **UNREAD** as decoded path strings
(ASCII `strings.tsv` skips wchar). Host has **no**
`GameInstall` slot for them.

If `00999230` misses, the vectors stay empty and Init
World still continues. That miss vs hit is **UNREAD**
against live TLC files.

### Type table `006F5C10`

Same stage string `"Init Animation Events"`, **after**
the two loads. Walks `0x13BA8C8` via `006F5400`. First
row: alloc `0x14C` → `008C2530` (base `00867950`, vtbl
`012808EC`, id 1). Later rows `00867950` / `00857E40` /
`008C91B0` / `008CF8F0` / `008BFB00` …. **PROVEN**
register. Individual RTTI bind **PARTIAL**.

Nearby RTTI (name only, **not** proven as those rows):

| RTTI | Role vs this note |
|---|---|
| `CEventCutSceneAnimEvent@NEntityEvents` | script/cutscene wait. **DISPROVEN** as Leave fire |
| `CActionEventSound@NCreatureAction` | later leftover sound notify (footstep-shaped) |
| `CActionEventShot` / `Explosion` / `TriggerParticle` | later leftover action keys |
| `CEventHitThing` / `CEventHitBy` | combat hit, **not** clip notify |
| `CActionEventAddObject` | **DISPROVEN** as a runner (`script-addobject`) |

---

## 3. FOOTSTEP / HIT are not first-seen names

| Token | Where | On Leave spine? |
|---|---|---|
| `FOOTSTEP` | **0** exe strings. Host `WaitForAnimationEvent FOOTSTEP` fixture; bank search falls back to that line | **DISPROVEN** as first-seen. Real first script arg **UNREAD** |
| `HIT` / `GET_HIT` / `PHYSICAL_HIT1` / `HIT_EFFECT` | combat / VFX strings | **DISPROVEN** as `BEGIN_EVENTS` keys |
| `START` / `STOP` | parser tokens `0x01264990` / `"STOP"` at `006FA7CD` | **PROVEN** as list syntax, not a fire |
| `FOOT` fourcc `0x544F4F46` | `XSeqFile` chunk; skipped when sampling tracks | **DISPROVEN** as notify table |
| `TMEV` fourcc `0x56454D54` | const only; `WalkChunks` never enters it | **UNREAD** leftover vs this spine |

Do not invent Lookout `FOOTSTEP` or first Present `HIT`.

---

## 4. Who would *fire* a notify (later leftover)

A fire needs a playing clip **and** a matching START/STOP
key **and** a listener:

```
PlayAnimation 00CC15DA → 004C7470 → 0070D580     ; time [clip+56]
  manager+44 walk (006F9430 insert / 8-byte slots)
    id = intern(name) & 0x3FFFFFFF
    bit31 START vs STOP
    time float vs play time                      ; UNREAD comparator
WaitForAnimationEvent 00CC41FC / apply 00CC4252
  arg0 required; empty → 00CC7081
  00CBEB7E skip-true → 00CC7081
  actor vtbl+48 → 004AB130
  leftover poll 004AAF60:
    mov ecx, [ecx+4]
    jmp [eax+236]                                ; body UNREAD
  jmp 00CC707C
```

`004AAF60` is a 7-insn thunk (same family as `+224`…
`+240`). **PROVEN** slot. `vtbl+236` body **UNREAD**.

| Claim | Class |
|---|---|
| `0070D580` E8 from Leave / `006AC910` / first Present | **DISPROVEN** (`xseq-walk-first`) |
| `00CC41FC` on Leave / first pumps | **DISPROVEN** |
| First Lookout frame emits FOOTSTEP | **DISPROVEN** (bind pose, no inner play) |
| Oakvale `WaitForAnimationEvent` after `PlayAnimation` | **LEFTOVER**. First event *name* on that def **UNREAD** |

`AnimationRuntime.Tick` advances `PlayTime` and completes
the clip; it **never** signals `EventWaits`. Host wait is
arm-only. **PROVEN** C# gap.

---

## 5. C# vs native

| Site | What | Class |
|---|---|---|
| `InitWorldInitStages` Mesh Bank → UI Manager | skips `006FAA90` / `006FABF0` / `006F5C10` | **LEFTOVER** (Note table only; `xseq-first`) |
| `AnimationRuntime.WaitEvent` | records `EventWaits` + vtbl 236 | **EQUIVALENT** apply gate. **DISPROVEN** as Leave |
| `AnimationRuntime.Tick` fires named events | no | **MATCH** skip vs first-seen; **PARTIAL** vs leftover fire |
| `XSeqFile.FourCcFoot` / `FourCcTmev` | format consts | **LEFTOVER** vs `BEGIN_ANIMATION_EVENTS` |
| `FOOTSTEP` in architecture tests | isolated / bank-or-fallback | **LEFTOVER** fixture |
| Event list I/O | none | **LEFTOVER**. Native `006FA4E0` has no host parser |

---

## Classifications (short)

1. **Frontend / Leave notify — DISPROVEN.** No managers.
2. **First after Leave — two empty `CManager` then Game/Sound
   `006FA4E0` then `006F5C10` type table. PROVEN construct.**
   Decoded filenames **UNREAD**. Text vs compiled follows
   `[0x13B860A]` default 0.
3. **First FOOTSTEP / HIT *fire* — not on this spine.
   DISPROVEN as first-seen.** Needs `0070D580` + table hit.
4. **`WaitForAnimationEvent` after Leave — DISPROVEN.**
   Leftover poll `004AAF60` → `vtbl+236` (**UNREAD** body).
5. **C# event tables / fire during New Game — LEFTOVER if
   called; MATCH if left idle.** Native first Present stays
   bind-pose, silent.

Do not treat `HERO.WaitForAnimationEvent FOOTSTEP` or combat
`HIT_*` strings as the first animation notify after Leave.
