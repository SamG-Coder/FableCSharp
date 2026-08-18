# Native animation blend / mixer first use after Leave

Investigation only. No production `src` edits.

Do **not** start at Oakvale / `PlayAnimation` `00CC15DA` /
`0070D580` / `CS_WAKING_UP_LOOP` / `3420`. That path is later
`Q_NewOakValeIntro`, not Leave / Init World / first no-save
Present.

Do **not** confuse this mixer with GPU `D3DRS` blend, TOD blend
`00B46C80`, camera blend `006B42F0`, or vertex UBYTE4 PALSKIN
weights.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE** / **INVENTED**.

Sources:

- listings `00A27030` / `00AA0F60` / `00A29800` / `00A9D730` /
  `00AA0090` / `00AA09F0` / `00A9F2F0` / `00A52650` / `00A4C1F0` /
  `00A242C0` / `00BD2D90` / `00BD6810` / `00B3CB30`
- `e8.tsv` callers of `00AA0F60` / `00AA0090` / `00AA09F0`
- `docs/runtime/FORWARD_TREE.md` §§7, 14
- `docs/status/investigations/2026-08-18-resource-manager.md`
- `docs/status/investigations/2026-08-18-palskin.md`
- `proofs/xseq-first/README.md`
- `MeshBank.cs` / `XSeqFile.cs` / `WorldShading.cs` /
  `AnimationRuntime` in `ExecutionContext.cs`
- RTTI `C3DAnimationBlendState` / `C3DAnimationInterpolator`
  (name only; vtbl bind **UNREAD**)

---

## Verdict

**Native mixer is the 76-byte `00AA0F60` object** (vtbl
`0129E134`) stored at `MBANK_ALLMESHES+960` and copied to
`world+68`. It is **not** `CTCAnimationComplex` and **not**
`AnimationRuntime`.

**First object after Leave is construct-only.** Init Mesh Bank
`00A27030` allocates it empty: two `00A5C570` scratch
allocators, `mixer+4 = bank`, **zero channels**.

**First *evaluate* is later `00AA0090` from PALSKIN dest pack
`00BD2E35`.** That is after region load / first PALSKIN drain
(`00BD549D` → `00BD2D90`), not Leave and not frontend.

**First-seen channel count is 0** (`FirstSeenPlaysAnim=false`).
The 20-byte channel walk, time lerp at `+12`, and weight lerp at
`+16` do **not** run on that first call. Tail still runs
hierarchy `00A9E1E0` on C3D bind locals. Dest ≈ identity.

C# has **no mixer object**. `PaletteForPose` drops `time`.
`AnimationRuntime.ChannelArmed` is a host bool, not a native
channel.

---

## Timeline (no-save New Game)

```
0042EC7C retail / frontend 2D
  no 00AA0F60, no 00AA0090
0042F2A2 Leave frontend
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
    Init Mesh Bank 0049E620
      00A09F20 MBANK_ALLMESHES miss
        00A27030 size 0x460
          alloc 76 → 00AA0F60          // FIRST mixer
            vtbl 0129E134
            +4/+8/+12/+16 = 0
            +20  00A5C570(0, 0x1000)   // locals scratch
            +48  00A5C570(0, 0x1000)   // dest scratch
          lea bank+960
          00A29800 wrap mixer
          00A9D730 mixer+4 = bank
        world+68 = [bank+960]
00416953 Load FinalAlbion.wld
004189C2 first pumps
  FirstSeenPlaysAnim=false
  no 00CBFB7D / 0070D580
  (after region + first PALSKIN drain)
    00BD549D → 00BD2D90  [helper+288]==0
      ecx = [mesh+80]+4 → [that+960]   // the 00AA0F60
      00BD2E35  00AA0090               // FIRST evaluate
        channel count 0 → 00AA097D
        skip 00A9DFA0
        00A9E1E0 hierarchy bind locals
        dest = S × IBM ≈ identity
```

`PlayAnimation` / `0070C050` / `0070D580` / `3420` are **not**
on this list. **PROVEN.**

---

## 1. Mixer object

### 1a. Ctor `00AA0F60` — **PROVEN**

```
00AA0F60  push esi / edi
          mov esi, ecx
          0099A2F0
          [esi]    = 0x0129E134
          [esi+4]  = 0
          [esi+8]  = 0
          [esi+12] = 0
          [esi+16] = 0
          lea ecx, [esi+20]; 00A5C570(0, 0x1000)
          lea ecx, [esi+48]; 00A5C570(0, 0x1000)
          ret
```

`00A5C570` is a scratch allocator (vtbl `0129A064`, default
block `0x400`, here cap `0x1000` at `+12`). Not a prefilled
clip buffer.

| Offset | After ctor | After `00A9D730` | Role |
|---|---|---|---|
| `+0` | vtbl `0129E134` | same | mixer |
| `+4` | 0 | bank `00A27030` | `00A242C0` table owner |
| `+8` / `+12` / `+16` | 0 | 0 first-seen | **UNREAD** later |
| `+20` | allocator | used by `00AA0090` via `00A5C910` | `n*48` locals |
| `+48` | allocator | used by `00AA09F0` via `00A5C910` | `n*64` dest |

Size **76**. **PROVEN.**

RTTI `C3DAnimationBlendState` (`01378BFC`) and
`C3DAnimationInterpolator` (`01378C24`) sit next to each
other. **UNREAD** as this vtbl. Do not pin the English name.

### 1b. Callers of the ctor

| Site | When | Class |
|---|---|---|
| `00A272F6` ← `00A27030` | Init Mesh Bank after Leave | **PROVEN** first |
| `00B3CBAC` ← `00B3CB30` | later object that embeds two `00A27030` at `+32` / `+1152` then an in-place mixer at `+2312` | **DISPROVEN** as first after Leave |

`e8.tsv`: those two `E8` only.

### 1c. Bind into the bank

```
00A272D6  push 76
00A272F6  call 00AA0F60
00A272FF  lea edi, [esi+960]
00A27308  call 00A29800        // [bank+960] = mixer, 12-byte ctrl
00A27310  call 00A9D730        // [mixer+4] = bank
```

`00A9D730` is `mov [ecx+4], arg; ret 4`. `00A9D740` clears
`+4` (dtor path `00A27360`).

`world+68 = [bank+960]` **PROVEN** (`FORWARD_TREE` §14;
resource-manager). That slot is the smart pointer, not a second
mixer.

---

## 2. Channel + weight (the blend walk)

### 2a. Evaluate `00AA0090` — **PROVEN** control flow

`this = mixer`. `ret 32` (8 dwords). PALSKIN packer:

```
00BD2E21  [ebx+80]
00BD2E28  ecx = [that+4]
00BD2E2B  ecx = [ecx+960]      // mixer
          push &[helper+116]   // blend source A
          push &[helper+124]   // blend source B
          … mesh, dest, bone count, 1
00BD2E35  call 00AA0090
```

`00B83750` may supply an extra pose cache (`[helper+120]==0`
→ `eax=0` first-seen).

Inside `00AA0090`:

1. `ebx = [mesh+152]` bone count.
2. `00A5C910` at `mixer+20` for `n*48` locals
   (`lea ecx,[ebx+ebx*4]; * 0x30`).
3. `00A9F2F0` on the two source headers (time lerp of
   `[clip+12]`, optional group recurse when `vtbl+8==2`).
4. Channel count = `([list+16]-[list+12]) / 20`
   (`imul 0x66666667; sar 3`). **20-byte records.**
5. `jbe 00AA097D` when count is 0.
6. Else per channel: index `+8`, time `+12`, weight `+16`.
7. Tail: optional `00A9DFA0`, then **always** `00A9E1E0`,
   optional `00A9D750`, `00A5C720` free `mixer+20`.

### 2b. 20-byte channel record — **PROVEN** `+8/+12/+16`

| Off | Use | Class |
|---|---|---|
| `+0` / `+4` | not read in the first-seen walk body | **UNREAD** |
| `+8` | `u32` index → `00A242C0` = `[bank+896][i]` | **PROVEN** |
| `+12` | `float` time. Lerp A→B by packer `t`, then `00A52650` | **PROVEN** |
| `+16` | `float` weight. Lerp A→B; `0x3F800000` / `0xBF800000` sign; accumulate 48-byte locals | **PROVEN** |

`00A242C0` is four insns: `eax=[ecx+896]; eax=[eax+arg*4]`.
`ecx` is `mixer+4` (the bank). Index is **not** an XSEQ
pointer.

`00A26C60` binds that clip object to the mesh (`[mesh+232]`
stamp) and returns a key-block with flags at `+8`.
`test cl,1` after `shr +8, 2` picks the exact-key path vs
slerp path.

`00A52650` (`ecx` = clip): `time * [clip+80]` → integer key
and frac; `key %= [clip+84]`. **PROVEN** as time→key.
`[clip+80]` / `[+84]` units **PARTIAL**.

`00A4C1F0` samples two 16-byte keys (`shl 4`) and lerps
(quat path when `[ecx+11] & 2`). **PROVEN** as key sample.
Not first-seen.

`00A88C10` copies 16 bytes into the 48-byte local; following
`fadd` stores add translation/scale extras. **PROVEN**
accumulate.

`00A9F2F0` header blend: leaf (`vtbl+8==0`) uses
`00A4D770` / `00A4CEC0`; `vtbl+8==2` recurses (depth &lt; 5)
and lerps `[+16]` weights. **PROVEN** as nested blender.
**DISPROVEN** as first-seen (needs a live clip header).

### 2c. Sibling `00AA09F0`

Only `E8` is `00BD68B7` ← `00BD6810`. Allocates `n*64` at
`mixer+48`, calls `00AA0090`, then copies dest (4-wide
unroll). **Not** the first-seen packer (`00BD2D90` uses
`00AA0090` directly and writes `[helper+288]`).

`00AA0FA0` / `00AA0C50` are a no-lerp sibling (single
`[esi+12]` time). First-seen `E8` from PALSKIN drain
**DISPROVEN**.

---

## 3. Frontend / Leave / first Present

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present calls `00AA0F60` / `00AA0090` | **DISPROVEN** | 0 `E8` on `0042DF9E`; mesh bank is after Leave |
| Leave itself evaluates channels | **DISPROVEN** | Leave is `0042F2A2` audio/UI teardown |
| First mixer *object* is Init Mesh Bank | **PROVEN** | `00A272F6` |
| First `00AA0090` is Init World | **DISPROVEN** | only later `00BD2E35` / wrapper `00AA0A4A` |
| First `00AA0090` is first PALSKIN dest pack | **PROVEN** call site. First live drain is `00BD549D` after region load | palskin §6–8 |
| First-seen channel/weight lerp runs | **DISPROVEN** | count 0 → `00AA097D`; `FirstSeenPlaysAnim=false` |
| First-seen pose is bind locals via `00A9E1E0` | **PROVEN** | dest ≈ identity |
| `005B37F7` DEFAULT / `0070D580` arms channels on create | **DISPROVEN** | create `006AC910` does not play; `FirstSeenAppearancePlaysDefault=false` |
| `00B3CBAC` in-place mixer is first | **DISPROVEN** | second ctor; not Init World |
| Appearance+52 20-byte *name* table is this channel | **DISPROVEN** | that table is `00662A00` clip names |

**Answer:** after Leave the mixer **exists** (empty). First
**use** of blender/channel/weight *fields* is **none** on the
first PALSKIN pack. First **use** of the mixer *function* is
that empty `00AA0090` + `00A9E1E0`.

---

## 4. C# vs native

| Site | Native | Host | Class |
|---|---|---|---|
| Mixer object | `00AA0F60` at `bank+960` / `world+68` | `MeshBank` has no field | **LEFTOVER** |
| Scratch `+20` / `+48` | `00A5C570` `0x1000` | none | **LEFTOVER** |
| `InitWorldInitStages` | Mesh Bank then particles + `006FAA90` then UI | Mesh Bank then UI | **LEFTOVER** (events, not mixer). Mixer ctor is *inside* `00A27030`, which host also skips |
| `00AA0090` | time lerp + 20-byte channels + `00A9E1E0` | `WorldShading.BoneHierarchyBuild` constant only | **DIVERGE** |
| Time | `00A52650` / channel `+12` | `PaletteForPose(..., time)` discards `time` | **DIVERGE** |
| Channel `+8` index | `[bank+896][i]` | none | **LEFTOVER** |
| Channel `+16` weight | lerp + accumulate | none | **LEFTOVER** |
| `AnimationRuntime.ChannelArmed` | no such flag | set true in `ApplyInner` | **INVENTED** |
| `AnimationRuntime.ApplyInner` | `0070D580` request | `ClipKey` / `PlayTime=0` / `Duration` | **PARTIAL** apply. **DISPROVEN** as mixer arm |
| `Animation.Clips` 20-byte | appearance+52 names | empty on engine path | **MATCH** skip first-seen; **not** mixer channels |
| `XSeqFile` / `GetAnim` | later type-6 payload | on-demand parse | **EQUIVALENT** slot. **DISPROVEN** as first-seen |
| `PaletteForPose` first key | `00A4C1F0` leftover | `ApplyFirstLocals` | **EQUIVALENT** later. Unused first Present |
| `FirstSeenPalettes` | `00A9E1E0` × IBM | parent walk × `bone.Matrix` | **EQUIVALENT** pose when 0 channels |
| Vertex UBYTE4 weights | PALSKIN VS `a0` | `PalskinBlendWeightOffset` / `SkinPosition` | **MATCH** file. **Not** mixer `+16` |
| CPU `TrianglesForPose` | dest upload `c38` | flatten bind verts | **DIVERGE** site; pose **MATCH** first-seen |

---

## Classifications (short)

1. **Mixer = 76-byte `00AA0F60` at `bank+960`. PROVEN.**
   First after Leave is that ctor. Empty.
2. **Blender walk = `00AA0090` / `00A9F2F0` on two PALSKIN
   sources `+116` / `+124`. PROVEN** as the evaluate. First
   call is dest pack `00BD2E35`, not Leave.
3. **Channel = 20 bytes: `+8` bank index, `+12` time, `+16`
   weight. PROVEN** layout. First-seen **count 0**.
4. **Weight lerp / key sample `00A4C1F0` — leftover until a
   clip plays. DISPROVEN as first-seen.**
5. **C# mixer / `ChannelArmed` / `time` sample — LEFTOVER /
   INVENTED / DIVERGE.** Keep `FirstSeenPalettes` as the
   first-Present pose. Do not invent channels on New Game.

Do not treat `PlayAnimation` apply or `PaletteForPose(clip)`
as the first mixer use after Leave.
