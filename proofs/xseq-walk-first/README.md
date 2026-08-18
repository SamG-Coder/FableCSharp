# First XSEQ walk / idle *cycle* after Leave Frontend

Investigation only. No production `src` edits.

Sibling of `proofs/xseq-first/README.md`. That note is the first
**object** (`00AA4710` / `00A999B0` empty helper on Init Mesh Bank).
This note is the first **cyclic clip sample** that would move a
creature off bind pose (walk / idle / STAND / DEFAULT).

Do **not** start at Oakvale / `CS_WAKING_UP_LOOP` / `3420` /
`PlayAnimation` `00CC15DA`. That path is later
`Q_NewOakValeIntro` / `CS_OAKVALE_INTRO_FATHER`, not Leave /
Init World / first no-save 3D Present.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

Sources: `proofs/xseq-first/README.md`;
`docs/runtime/FORWARD_TREE.md` §§7, 14–15;
`docs/status/investigations/E-player-palskin.md`;
`docs/status/investigations/2026-08-18-palskin.md`;
`docs/status/investigations/2026-08-18-resource-manager.md`;
`XSeqFile.cs` / `MeshBank.cs` / `WorldShading.cs` /
`ExecutionContext.cs` (`AnimationRuntime`);
`XSeqFormatTests` / `EngineLifecycleTests` /
`ScriptRuntimeArchitectureTests`;
listings `00A26D40` / `00A2D200` / `00A4C5E0` / `00A4CDD0` /
`00A999B0` / `00AA4680` / `00AA4710` / `00662A00` / `004C7470` /
`0070C050` / `0070D580` / `00AA0090` / `00743E30`.

---

## Verdict

**There is no walk/idle XSEQ cycle on the Leave / Init Game /
first Present spine.** First Present dest is C3D bind locals
(`FirstSeenPlaysAnim=false`). A later named clip is **UNREAD**
as identity; do not invent `STAND` / `WALK` / `ST_IDLE` /
`DEFAULT` as that first cycle.

Native type-6 *reader* VAs are **PROVEN**. Host `XSeqFile.Parse`
is the stand-in. First-key sample into `PaletteForPose` is
**PROVEN** format (`XSeqFormatTests`). Time interpolation that
would *cycle* keys (`00AA0090`) is **UNREAD**. ANRT cyclic flag
is parsed and unused.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  PlayAVI / frontend 2D          // UI type 6 = glyphs 0054EF00, not 3DAF
0042F2A2 Leave frontend
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
    0049E620 Init Mesh Bank
      00A27030 0x460
        00AA4710 → 00A999B0      // empty 3DAF+XSEQ helper  (xseq-first)
        009D56C0 directory
        00A26660 buckets types 6–10 at bank+932
      006FAA90 Init Animation Event Managers   // not a clip
  00416953 Load world FinalAlbion.wld
004189C2 first pumps
  FirstSeenPlaysAnim=false
  no 00A26D40 type-6 payload
  no 00A4C5E0 / 00A4CDD0 unpack
  no 004C7470 / 0070D580 play
  no 00662A00 appearance table walk
  first PALSKIN dest = bind locals (00AA0090 on bank+960 mixer)
later 00501450 Lookout / 006AC910 hero 4299
  create has no PlayAnimation / STAND / CTCIdle / 005B37F7
  appearance+52 table body UNREAD
```

`3420` `CS_OAKVALE_DREAM_INTRO_YOUNG_HERO_WAKING_UP_LOOP` is
**not** on this list. **PROVEN** leftover fixture.

---

## 1. Native type-6 reader VAs (host `XSeqFile`)

`XSeqFile.cs` constants lock the persist / unpack pair. Tests
`Xseq_persist_addrs_match_00a999b0_and_00aa4680`.

| Const / slot | VA | Native | Class |
|---|---|---|---|
| `Ctor3Daf` | `00A999B0` | `push "3DAF"`; vtbl `0129E060`; alloc 52 `"ANRT"` `00AA8360` vtbl `0129DFF0`; children zeroed | **PROVEN** listing |
| `CtorXseq` | `00AA4680` | `push "XSEQ"`; vtbl `0129E194`; size 28; `+16/+20/+24=0` | **PROVEN**. **0 E8** sites; first live object is `00AA4710` which *calls* `00A999B0` then tags `"XSEQ"` vtbl `0129E1E4` |
| `XseqVtbl` | `0129E194` | `"Compressed Animation Sequence"` (`00AA49D0` is the RTTI string write, **not** an open) | **PROVEN** |
| Mesh vtbl+48 load | `00A26D40` | `cmp ebx, 6` / `je 00A26ED3` (also 7/8/9/10) | **PROVEN** branch |
| type 6–10 record | `00A2D200` then `00A25970` | `add [esi+4], 40` — 40-byte record at `bank+932` | **PROVEN** size. **DISPROVEN** as clip parse |
| `UnpackFn` | `00A4C5E0` | stream → buffer (`call [eax+24]`, `00A5C910` alloc) | **PROVEN** body. Callers inner persist, not Leave |
| `PersistLoadFn` | `00A4CDD0` | stream header then `call 00A4C5E0` | **PROVEN**. Loop `00A4DFF8` `add edx, 44` = `ClipRecordBytes` |
| `CompressFn` | `00A4EFC0` | write path | **PROVEN** addr. Unused on New Game |
| `LocalCopyFn` | `00AAF1E0` | C3D 48-byte locals (`BoneLocalBytes`) | **PROVEN** as mesh serialize. **DISPROVEN** as clip open |
| `HierarchyFn` | `00AA0090` | PALSKIN mixer on `bank+960` (`00AA0F60`); first-seen input is bind locals | **PROVEN** first call. Time interp leftover **UNREAD** |

Type-6 load arm (`00A26ED3`):

```
00A26EB6  cmp ebx, 6
00A26EB9  je  00A26ED3          ; also 7/8/9/10
00A26ED3  lea ecx, [esp+36]
          lea ecx, [esi+932]    ; type 6–10 bucket
          vtbl imm 0129CD64
          call 00A2D200         ; +40
          call 00A25970
          eax = [esi+936] - 40
```

Types 1/2/4/5 take the 96-byte C3D record (`00A26DE6`). Type 3
is 16 bytes at `bank+920`. Host `MeshBank.Open` drops type 3
(**PARTIAL** vs native bucket). Type 6 is indexed, not parsed,
at directory time (`ParsedCount=0`). **PROVEN.**

`00A4C5E0` / `00A4CDD0` E8 sites in `.text`: `00A4CE8C`,
`00A4DFF8`, `00A503AE`. **DISPROVEN** as callees of `004A6E30` /
`0049E620` / Leave.

Host `XSeqFile.Parse`: first dword uncompressed size then raw
LZO (not framed C3D LZO) unless `>>>>` / leading `3DAF`/`ANRT`.
Walks `ANRT`/`AOBJ`/`XSEQ`. `ANRT[0]!=0` → `Cyclic`;
`ANRT+1` float → `Duration`. `TrySample` / `ApplyFirstLocals`
ignore `time` and take the first stored quat / i16×factor pos.

That is **EQUIVALENT** to `00A26D40` type 6 + `00A4C5E0` as a
later on-demand slot (`MeshBank.GetAnim`). **Not** first-seen
New Game.

---

## 2. Walk / idle *names* — do not invent the first cycle

Exe strings exist. They are **not** Leave / create / first
Present callers.

| Token | VA / site | On Leave spine? |
|---|---|---|
| `STAND` | `0x012674DC` (near `STAND_FRONT/LEFT/RIGHT/HAPPY/BORED`) | **DISPROVEN**. Zero code xrefs (`WorldShading` comment). fnmap “STAND” hits `004EE137` / `006A9A00` are substring false-positives (`CBuyableHouse` / other) |
| `ST_WALK_` | `00743E30` `push "ST_WALK_"` then `0099EBF0` copy | **PROVEN** as a prefix setter. **DISPROVEN** as Leave / `006AC910` / first Present |
| `ST_IDLE` | many later AI/combat sites (`007ECFD0` … `00E9E420`) | **DISPROVEN** as this spine |
| `DEFAULT` | `00662AAB` `push "DEFAULT"` miss path of `00662A00` | **PROVEN** fallback *once lookup runs*. Lookup does **not** run at create |
| `CTCIdle` | fnmap **0 fns** | **DISPROVEN** as a play site |
| `CTCIdleScheduler` | RTTI `0137AFD0`; type-table push `004D5EA8` / `004D2EF0` | **PROVEN** name register. Ctor / first tick **UNREAD**. Not an XSEQ open |
| `PlayLoopingAnim WALK` | host test only (`ScriptRuntimeArchitectureTests`) | **LEFTOVER** vs native first-seen |

Appearance table:

```
00662A00  ecx = appearance
  [ebx+308] → 0073A6E0 / vtbl+16
  0042B0A2([ebx+112])           ; attach
  lea edi, [esi+52]             ; 20-byte name table
  005DC2E0 contains(name)
    hit  → 005DC340 walk
    miss → push "DEFAULT" → 005DC340
```

`CAppearanceDef` idx 10533 on `CREATURE_HERO` is type **PROVEN**;
raw body **UNREAD**. Combat names visible at +3697 are not the
20-byte runtime table. First clip id/name after Leave stays
**UNREAD**. Do not pair Lookout to `STAND` / `WALK` / `IDLE` /
`DEFAULT`.

---

## 3. Who would *start* a cycle (later leftover)

If a clip ever played, native is:

```
script 00CC14B8 / apply 00CC15DA
  actor.vtbl+72(name, flags)          ; 004C7470
    walk [this+68..+72) 8-byte slots
    skip [comp+8]!=0
    else [comp.vtbl+68](name)
      type 90 CTCAnimationComplex +68 = 00686920  (al=1; ret 4)
  00662A00 appearance+52
  0070C050 request (ret 28 pack)
  0070B460 [comp+12]
  0070D580 inner play                 ; PlayTime=0, duration [clip+44]/max(mode,1)
PlayLoopingAnim 00CC186C is vtbl+80, not +72
005B37F7 DEFAULT → 0070C050 mode 6 → 0070D580
  E8 callers: clothing GUI 005B6881, PC_UI_FRAME 005B8743 only
```

| Claim | Class |
|---|---|
| `004C7470` walks components | **PROVEN** (31 insns, `ret 4`) |
| `00686920` accepts the name | **PROVEN** stub. Does **not** sample XSEQ |
| `0070D580` starts playback | **PROVEN** body. `jle 0070D71D` skips time walk when mode≤0 |
| Create `006AC910` / `006A9DD0` / `004C9CA0` call any of the above | **DISPROVEN** |
| `005B37F7` on create | **DISPROVEN** (`FirstSeenAppearancePlaysDefault=false`) |
| `00CBFB7D` / `.PlayAnimation` / `.PlayLoopingAnim` on Leave / first pumps | **DISPROVEN** (`FirstSeenCallsPlayAnimationDispatcher=false`) |
| First Lookout frame is bind pose | **PROVEN** (`FirstSeenPlayAnimationAppliesPose=false`) |

A walk/idle *cycle* would need: type-6 payload (`00A26D40` +
`00A4C5E0`) **and** inner play (`0070D580`) **and** time interp
(`00AA0090`). None of those three run on this spine.

---

## 4. Cyclic flag vs first-key (format, not runtime)

`XSeqFile.Parse` ANRT payload:

| Offset | Field | Host |
|---|---|---|
| 0 | `u8` cyclic | `Cyclic = payload[0] != 0` |
| 1 | `f32` duration | `Duration` (fallback 1) |

`XSeqFormatTests` synthetic ANRT writes cyclic=1, duration=2.
Wake `3420` is LZO `3DAF` with `ANRT`/`AOBJ`/`XSEQ` tracks;
first keys move kid **4300** palettes/triangles off bind.
**PROVEN** format. **DISPROVEN** as Leave clip.

`TrySample` discards `time`. `PaletteForPose` discards `clip`
and `time`, then `FirstSeenPalettes(ApplyFirstLocals(bones))`.
So even a cyclic walk/idle file, if forced through host submit,
poses **frame 0 only**. Native leftover that would step keys
is `00AA0090` (**UNREAD**). `0070D580` `fild`/`fdivr [ecx+44]`
into `[ecx+56]` is a channel duration, not the key lerp.

---

## 5. C# vs native

| Site | What | Class |
|---|---|---|
| `MeshBank.Open` directory, type 6 bucket, `ParsedCount=0` | MESH open | **MATCH** |
| `00A27030` empty `00AA4710` helper | not constructed in C# | **LEFTOVER** (`xseq-first`) |
| `MeshBank.GetAnim` / `XSeqFile.Parse` | on-demand type 6 | **EQUIVALENT** to `00A26D40`+`00A4C5E0`. Lifecycle never calls it (**MATCH** skip) |
| `WakeLoopId=3420` | Oakvale dream | **LEFTOVER** vs Leave. Keep as format fixture |
| `PaletteForPose(..., sequence)` first-key | 48-byte locals | **PROVEN** format. Unused on first Present |
| `AnimationRuntime.LookupClip` miss → `DEFAULT` | `00662A00` | **EQUIVALENT** later. Empty `Clips` on engine path |
| `PlayLoopingAnim` host `ClipKey=WALK` | vtbl+80 | **PARTIAL** vs native; **DISPROVEN** as first-seen |
| `InitWorldInitStages` omits `006FAA90` | event managers | **LEFTOVER** (not XSEQ) |

---

## Classifications (short)

1. **Frontend walk/idle XSEQ — DISPROVEN.** No mesh bank. UI type
   6 is glyphs (`0054EF00`).
2. **First XSEQ *object* after Leave — empty helper. PROVEN**
   in `xseq-first`. Not a cycle.
3. **First type-6 *payload* / unpack — not on this spine.
   DISPROVEN as first-seen.** Reader VAs **PROVEN**.
4. **First walk/idle *name* after Leave — UNREAD.** `STAND`
   zero xrefs. `ST_WALK_` / `ST_IDLE` are later leftover
   setters. `DEFAULT` is the miss path of `00662A00`, which
   create does not call. `3420` wake **DISPROVEN**.
5. **First PALSKIN dest after Leave — bind locals. PROVEN.**
   A cycle cannot appear until `0070D580` + `00AA0090`.
6. **Host `GetAnim` / `PlayLoopingAnim WALK` during New Game —
   LEFTOVER if called; MATCH if left on-demand.**

Do not treat `XSeqFile.Parse(3420)`, `STAND`, `WALK`, or
`PlayAnimation` apply as the first animation cycle after Leave.
