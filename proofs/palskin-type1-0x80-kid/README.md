# PALSKIN type1 `0x80` on kid 4300: first-seen submit vs skip

Investigation only. Production `src/` / `tests/` were not edited.

Question: what is PALSKIN **type1** layer **`0x80`** on Graphic
**4300** `MESH_YOUNGHERO_02`? First-seen **submit** of that layer
versus **skip**. Do **not** invent a layer. Do **not** invent
`Duration=1` as the type.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

Kid **4300** body **`0x100`** + Flag1 hair **`0x200` after sky** is
**MATCH**. Type1 **`0x80`** is a different slot.

Dump: PALSKIN type1 `0x80` **UNREAD** as a 4300 submit.
Host: `MeshBatches` Flag1 grouping, `ScenePass`, `FirstSceneWorld`.

Authority: listings `00BD77FE` / `00BD780D` / `00B33010`; ExeIndex
`palskin-queue-slots-00bd7838-00bd780d.md`,
`mainscene-layer-drain-00b33010-00b33010.md`;
`docs/status/investigations/2026-08-18-scene-layers.md`,
`2026-08-18-palskin.md`, `E-player-palskin.md`;
`docs/PARITY.md` §19; `Fable.Dump mesh 4300`;
`ScenePass.cs`, `MeshBatches.cs`, `InstanceDraw.cs`,
`WorldShading.cs`, `FirstSceneWorld.cs` (read only);
`WorldGeometryTests.Kid_4300_flag1_hair_drains_0x200_after_sky`,
`MeshFormatTests.Kid_c3d_stores_hair_flag1_and_bones`,
`WorldPipelineTests` FirstSceneWorld layer bits,
`ScenePassTests.Registration_is_34_layers_and_walks_landscape_before_sky`.

Siblings: `proofs/hero-palskin-first-submit` (Lookout **4299**, not
this Graphic), `palskin-open`, `audit-firstsceneworld`.

---

## Verdict

**First-seen kid 4300 does not submit PALSKIN type1 `0x80`.**
Skip. Not a missing hair/body layer. Not `Duration=1`.

Type1 is **`[inst+104]+8 == 1`** inside `00BD71B0` at `00BD780D`.
That dword queues helper `00BCE740` onto prim-queue slots **10**
then **14**. MainScene `00B33010` drains slot 10 on bit **`0x100`**
(before sky) and slot 14 on bit **`0x80`** (after sky). Flag1 extra
slot **9** / **`0x200`** is the **type0** tail only.

Kid **4300** first-seen is the type0 path:

| Piece | Native | Host | Class |
|---|---|---|---|
| face / torso / mouth `Flag1=0` | slot **8** → bit **`0x100`** | `DrawnPasses(Palskin, 0) = [0x100]` | **MATCH** |
| `Young Hero Hair` `Flag1=1` | slot 8 + extra **9** → **`0x100`** then **`0x200` after sky** | `DrawnPasses(Palskin, 1) = [0x100, 0x200]` | **MATCH** |
| type1 slot **14** / **`0x80`** | empty unless `[inst+104]+8==1` | never emitted | **skip** |

Live `[inst+104]+8` for 4300 is **UNREAD as 1** (no dword dump).
First-seen submit of `0x80` on this Graphic is still **skip**:
host `MeshBatches` / FirstSceneWorld draws lock **no** `PassBit==0x80`.
If 4300 were type1, Flag1 would **not** add slot 9, and hair would
land on `0x80` instead of `0x200`. That is **DISPROVEN** as the
first-seen MATCH.

Registration still **walks** bit `0x80` (index 25). Empty drain is
not a kid DIP. Do not invent 4300 triangles on that bit.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| What is PALSKIN type1 `0x80`? | Drain of queue **slot 14** after sky. Filled only when `[inst+104]+8==1`. | **PROVEN** |
| Is type1 `Duration=1` / XSEQ / `ApplyInner`? | **No.** Type is a dword at `[inst+104]+8` after `sub 0` / `dec`. | **DISPROVEN** |
| Is type1 helper type index 4 (hair MapFlags)? | **No.** Index 4 is `PalskinTypeIndex` → helper `+28`. First-seen bind does not read it. Queue type is a different field. | **DISPROVEN** |
| Is type1 C3D bank type 5 / family `0x9`/`0xB`/`0xD`? | **No.** 4300 is file type 5. Family keys register PALSKIN. Queue type is `0` or `1`. | **DISPROVEN** |
| First-seen 4300 submit on `0x80`? | **Skip.** Body `0x100`, Flag1 hair extra `0x200` after sky. | **MATCH** skip |
| Live `[inst+104]+8` on 4300? | Not dumped as `1`. | **UNREAD** as 1 |
| Invent a 4300 `0x80` geometry layer? | **No.** | **DISPROVEN** |

---

## Evidence → Original → Host → Gap

### Evidence (dump)

`00BD71B0` (`edi` = instance from `[esp+680]`):

```
00BD77FE  mov eax, [edi+104]          ; inst+104
00BD7801  test eax, eax
          je  00BD7958                ; null → no queue
00BD780D  mov eax, [eax+8]            ; [inst+104]+8
00BD7810  sub eax, 0
00BD7813  je  00BD789C                ; type 0
00BD7819  dec eax
00BD781A  jne 00BD7958                ; not 0/1 → skip
; type 1:
          00BCE740 helper
          00B84720(slot 10)           ; 6A 0A
          00BCE740 helper
          00B84720(slot 14)           ; 6A 0E
```

Type 0 (`00BD789C`):

```
          00BCE740 helper
          00B84720(slot 8)            ; 6A 08
00BD78D5  mov eax, [esp+84]
00BD78D9  mov cl, [eax+41]            ; C3DMeshMaterial+41 Flag1
          test cl, cl
          je  00BD7919
          00BCE740 helper
          00B84720(slot 9)            ; 6A 09  Flag1 extra
```

Type1 never reads `+41`. Flag1 extra is type0-only.

`00B33010` (`mainscene-layer-drain`):

```
00B33083  cmp eax, 0x80
          je  00B3311A                ; slot 14 (then 13 if [0x1436E3C+8])
00B33183  cmp eax, 0x100
          je  00B331BD                ; slots 8 then 10
00B3318A  cmp eax, 0x200
          … 00B331AA push 9           ; Flag1 extra
```

Registration order (`ScenePasses.Registration`, 34 layers):

```
0x20 static  → 0x100 PALSKIN 8+10 → 0x2000 sky → … → 0x80 slot 14 → 0x200 slot 9
```

`Fable.Dump mesh 4300` (graphics.big type **5**):

```
#4300 type=5 MESH_YOUNGHERO_02  bones=76  prims=4  stride 28 flags 0x14
  mat 'face'             diffuse=792
  mat 'torso'            diffuse=794
  mat 'mouth'            diffuse=1253
  mat 'Young Hero Hair'  diffuse=793   Flag1=1 MapFlags=1
```

Hair is the only Flag1=1 material
(`Kid_c3d_stores_hair_flag1_and_bones`).

### Original (first-seen 4300)

`FirstSeenPlaysAnim=false`. Opacity ctor `00B991F5` `[inst+39]=0xFF`.
Kid create / `00DBDE40` does not PlayAnimation. Dest palettes are
identity. Queue type is **not** flipped by a clip.

Type0 + Flag1 hair:

```
00B33010
  0x100  00B849F0(8) then (10)     ; slot 8 has body+hair; slot 10 empty
  0x2000 sky else-path
  0x80   00B849F0(14)              ; empty — type1 skip
  0x200  00B849F0(9)               ; Flag1 hair
```

Slot 10 is drained on `0x100` for type1-A. Empty when type is 0.
Slot 14 is drained on `0x80` for type1-B. Empty on first-seen 4300.

### Host

`MeshFile` stamps `SceneLayer.Palskin` and copies material Flag1
onto each triangle. `MeshBatches.Build` / `BuildMeshes` group by
`(Layer, tex, blend, Flag1)` then `ScenePasses.DrawnPasses`:

```
Palskin, flag1==0 → [0x100]
Palskin, flag1!=0 → [0x100, 0x200]
never 0x80 unless [inst+104]+8==1   // not wired
```

`InstanceDraw.Palskin` hard-codes `PassBit=0x100` and comments
type1 `0x80` as **not first-seen 4300** (`[inst+104]+8` unread as
1). `FirstSceneWorld` soup is Oakvale SHOT2 (kid 4300 is in
`WorldGeometry.PlayerMeshId`); `MeshBatches.Build` of that
geometry has `0x100` and `0x200`, **not** `0x80`
(`WorldPipelineTests`). Trace D is **father** on `0x100` only —
not this layer.

### Gap

| Item | Class |
|---|---|
| Queue formula `00BD780D` 0→8(+Flag1 9); 1→10+14 | **PROVEN** |
| Drain `0x100`=8+10, `0x80`=14, `0x200`=9 after sky | **PROVEN** |
| Kid 4300 hair Flag1=1 / body Flag1=0 | **PROVEN** |
| Host Flag1 grouping body `0x100` + hair `0x200` after sky | **MATCH** |
| Host no `0x80` draws on 4300 / FirstSceneWorld | **MATCH** skip |
| Live `[inst+104]+8` dword on 4300 | **UNREAD** as 1 |
| Who writes `[inst+104]` | **UNREAD** |
| Type1 used later (fade / second instance / not first-seen) | **UNREAD** |
| `Duration=1` as that type | **DISPROVEN** |
| Invent 4300 tris on `0x80` | **DISPROVEN** |
| Lookout 4299 as this Graphic | **DISPROVEN** (`hero-palskin-first-submit`) |
| status.md “type1/Flag1 routing UNREAD; geometry still on `0x100`” | Flag1 **MATCH** on 4300; type1 `0x80` **skip** / leftover as a live dword |

---

## 1. Type1 is `[inst+104]+8`, not Duration

`00BD780D`: `mov eax,[eax+8]` / `sub eax,0` / `je type0` /
`dec eax` / `jne skip`. Integer **0** or **1**.

Not:

| Tempting alias | Why not |
|---|---|
| `XSeqFile.Duration = duration>0 ? duration : 1f` | Invented clip default. Not a queue dword. |
| `Animation.States[].Duration` / `ApplyInner` | Play path. `FirstSeenPlaysAnim=false` on first-seen 4300. |
| Helper `+28` type index **4** | Hair MapFlags=1 + Flag1 mask 5. Bind pass 4 does not read it (`FirstSeenPalskinBindUsesHelperTypeIndex=false`). |
| Family types `0x9`/`0xB`/`0xD` | `00BD27F0` factory keys. |
| C3D entry type **5** | Bank payload class. 4300 is type 5 whether queued as 0 or 1. |
| Material Flag1 | Type0 extra slot **9** / `0x200`, not slot 14. |

Do not invent `Duration=1` to force type1.

Instance ctor `00B991F5` writes **`[this+39]=0xFF`** (opacity).
It does not write `[+104]+8`. Writer of that dword stays
**UNREAD**.

---

## 2. Kid 4300 file is type0-shaped, not type1-shaped

Four PALSKIN prims, all stride **28** / flags **`0x14`**, 76
bones, bind-pose identity palettes. No extra CMultiStatic.

Flag1 extra exists **only** on the type0 tail after slot 8. Host
MATCH emits hair on **`0x200` after sky**. Type1 would instead
duplicate the instance onto slots 10+14 (`0x100` then `0x80`) and
**drop** slot 9. First-seen MATCH cannot be both.

So first-seen 4300 is **not** type1, even while `[inst+104]+8` is
**UNREAD as 1**.

---

## 3. First-seen submit vs skip

Drawn bits for this Graphic (host `MeshBatches` / tests):

```
0x100  body + hair (type0 slot 8; slot 10 empty)
0x2000 sky
0x80   skip          // type1 slot 14 empty
0x200  hair Flag1    // type0 slot 9
```

`Kid_4300_flag1_hair_drains_0x200_after_sky`:

- contains `PassBit==0x100` and `0x200`
- does **not** contain `0x80`
- every draw is `0x100` or `0x200`
- `Rank(0x100) < Rank(0x2000) < Rank(0x200)`

`ScenePassTests`: `DrawnPasses(Palskin, 1)` is `{0x100,0x200}`,
does not contain `0x80`.

`WorldPipelineTests` FirstSceneWorld: scene draws include `0x100`
and `0x200`, **`DoesNotContain 0x80`**.

Native still **visits** `0x80` (`Native_draw_order_is_begin_layers_end_present`
includes it in the drawn-submit rank list). Visit ≠ kid DIP.
`00B33175` `00B847B0(14)` / `00B849F0(14)` on an empty slot is
skip.

---

## 4. Do not

- Invent a 4300 layer on `0x80` (hair, mouth, second body pass,
  clothing 4126, Duration=1).
- Treat Flag1 `0x200` as type1 `0x80`.
- Fold type1 into `DrawnPasses` without `[inst+104]+8==1`.
- Call empty MainScene drain of slot 14 a first-seen kid submit.
- Pair this with Lookout adult **4299** (`hero-palskin-first-submit`).
- Wire `FirstSceneWorld` onto no-save Present and call that this
  dump (`audit-firstsceneworld`).

Next leftover is the **writer** of `[inst+104]+8`, not another
4300 C3D and not a Duration default.
