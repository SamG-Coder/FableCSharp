# Audit: `XSeqFile.cs` vs dump — invented fields?

Investigation only. No production `src/` edits.

Authority: `src/Fable.Formats/Anims/XSeqFile.cs`;
`tests/Fable.Formats.Tests/XSeqFormatTests.cs`;
listings `00A4C5E0` / `00A4CDD0` / `00A4C1F0` / `00A4DEA0` /
`00A4DF10` / `00A4DFF8` / `00A4EFC0` / `00A99510` / `00A98AF0` /
`00A98A70` / `00A98E60` / `00A999B0` / `00AA4680` / `00AA4710` /
`00AA4500` / `00AA4570` / `00AB0020`;
fourcc getters in `listing-00a40000.txt` / `listing-00a80000.txt`;
`proofs/xseq-first/README.md`; `proofs/xseq-walk-first/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **INVENTED**.

---

## Verdict

**Several `XSeqTrack` / FourCC fields are invented.** Bank unwrap,
ANRT cyclic+duration, the 44-byte in-memory record, and the
compressed key arrays are dump-backed. `TryReadTrack` is **not**
`00A4C5E0`: it invents `BoneIndex` / `Parent`, names a flags byte
`PreFps`, and guesses a 2-byte rotation palette. `FOOT` / `PUPP`
are not persist tags.

Host `Parse` is a FourCC walk of the decompressed persist blob,
not the persist graph (`00AA7EA0` / `00AA7F40` / `00A4CDD0`).
First-key sample still works on fixture 3420 because names and
`f32[4]` / `i16[3]` arrays are in that blob. That does **not**
make the invented fields native.

---

## 1. What the dump actually is

Type-6 graphics.big payload (`00A99510`):

| First dword | Path | Class |
|---|---|---|
| `">>>>"` `0x3E3E3E3E` | uncompressed persist after the marker | **PROVEN** |
| else | `esi=[edi]`, `edi+=4`, `00C069D0` raw LZO into `esi` bytes | **PROVEN** |

C# `UncompressedMarker` / size-then-`Lzo.DecompressRaw` (framed
fallback) **MATCH**. Leading `3DAF`/`ANRT` as already-unpacked
input is a host convenience, not a bank dword.

After unwrap, persist objects (not a raw TLV schema the host
owns):

```
00A999B0  "3DAF"  vtbl 0129E060
  00A98E60  copyright "Copyright Big Blue Box Studios Ltd."
            [obj+12] = 0x64  (version 100)
  empty ANRT child 00AA8360 vtbl 0129DFF0 size 52
00AA4680  "XSEQ"  vtbl 0129E194 size 28  (+16/+20/+24=0)
00AA4710  calls 00A999B0 then tags "XSEQ" vtbl 0129E1E4
          also registers "XALO" vtbl 0129E144
00A4DF10  persist-load XSEQ list → 00A4CDD0 per track
00A4DFF8  add edx, 44     // ClipRecordBytes
```

---

## 2. FourCC constants

Dump getters / `push "….` (not host invention):

| C# | Tag | Dump | Class |
|---|---|---|---|
| `FourCc3Daf` | `3DAF` | `00A999B7` / `00A98A01` | **PROVEN** |
| `FourCcAnrt` | `ANRT` | `00A98820` | **PROVEN** |
| `FourCcAobj` | `AOBJ` | `00A98830` / `00AB0021` | **PROVEN** |
| `FourCcXseq` | `XSEQ` | `00A4BD00` / `00AA4681` | **PROVEN** |
| `FourCcXalo` | `XALO` | `00A4BD20` / `00AA44E1` | **PROVEN** tag. C# never parses the object |
| `FourCcHlpr` | `HLPR` | `00A98840` / `00AAE091` | **PROVEN** |
| `FourCcMvec` | `MVEC` | `00AAE060` | **PROVEN** tag. No C# body |
| `FourCcAmsk` | `AMSK` | `00AB0010` | **PROVEN** tag. No C# body |
| `FourCcTmev` | `TMEV` | `00AAE070` | **PROVEN** tag. No C# body |
| `FourCcFoot` | `FOOT` | **0** `push "FOOT"` / `mov eax, "FOOT"` in `listing-00a*.txt` | **INVENTED** |
| `FourCcPupp` | `PUPP` | **0** same | **INVENTED** |

Dump tags C# omits: `ASEQ` (`00AAE080`), helper `HPNT` / `HDMY` /
`HCVL`. `WalkChunks` also treats `HLPR`/`MVEC` as nestable; those
are persist children, not extra track FourCCs.

`Ctor3Daf` / `CtorXseq` / `XseqVtbl` / `UnpackFn` / `PersistLoadFn`
/ `CompressFn` / `LocalCopyFn` / `HierarchyFn` are **PROVEN**
addresses (tests lock them). They are not file fields.

---

## 3. ANRT — not invented

`00A98A70` write / `00A98AF0` load / object `+44`/`+48`:

| Offset in ANRT persist payload | Native | C# |
|---|---|---|
| 0 | `u8` → `[obj+44]`; `setne` | `Cyclic = payload[0] != 0` |
| 1 | `u32`/`f32` → `[obj+48]` | `Duration = BitConverter.ToSingle(..., 1)` |
| 5+ | `00AA8830` children (`AOBJ` / …) | `WalkChunks` on payload |

**PROVEN** pair. C# `Duration = duration > 0f ? duration : 1f`
is an **INVENTED** fallback (native keeps 0). Cyclic is unused
at sample time (`TrySample` drops `time`).

---

## 4. 44-byte XSEQ track (native) vs `XSeqTrack`

In-memory record filled by `00A4C5E0` (this = `ebp`), sampled by
`00A4C1F0` / `00A4DEA0`, built by compress `00A4EFC0`. Size
**PROVEN** (`00A4DFF8` `add edx, 44`).

| Off | Native use | Persist `00A4C5E0` | C# `TryReadTrack` |
|---|---|---|---|
| +0 | name `char*` | vtbl+24 CString, alloc, copy | **INVENTED** first two `i32` `BoneIndex`/`Parent`, then CString (or rewind) |
| +4 | `f32` time scale; `fmul [ecx+4]` in `00A4DEA0` | `u32` store | `SamplesPerSecond` **PARTIAL** (right type if cursor is aligned; layout before it is not) |
| +8 | frame/period; `movzx` / `idiv` | `u32` store | `FrameCount` **PARTIAL** same |
| +10 | packed flags, low 5 bits then bit 6 | `u8` `and 0x1F`; later `u8` → bit 6 | **`PreFps` INVENTED name** for a raw byte |
| +11 | 2-bit channel modes (rot / pos / …) | three `u8` packed `0x03` / `0x0C` / `0x30` | **UNREAD** (absorbed into skip/`posFactor`) |
| +12 | `f32` position factor; `fmul [ecx+12]` then `fild` i16 | `u32` store | `PositionFactor` **PROVEN** meaning **if** the two `f32`s sit here |
| +16 | `f32` scale factor (compress max-abs) | `u32` store | `ScalingFactor` **PROVEN** as stored dword; C# never applies it |
| +20 | `f32[4]*` quats | `u16` count @+24, copy `count*16` | first quat **PROVEN** as bytes; count gate `<4096` host-only |
| +24 | `u16` rot count | that `u16` | `RotationCount` **PROVEN** slot |
| +26 | `u16` pos count | later `u16` | `PositionCount` **PROVEN** slot |
| +28 | `u16` rot-palette count | `u16` then **`count` bytes** @+36 | C# `palRot * (rotCount>255 ? 2 : 1)` **INVENTED** width |
| +30 | `u16` pos-palette count | `u16` then `count` bytes @+40 | **UNREAD** (C# stops after first i16 triple) |
| +32 | `i16[3]*` positions | `count*6` copy | first triple `* PositionFactor` **PROVEN** decode (`00A4C4BA`) |
| +36 | `u8*` rot palette | 1 byte / index; compress `cmp ebx, 0xFF` **refuses** palette if index > 255 (`00A4F57F`) | 2-byte-when-`rotCount>255` **DISPROVEN** |
| +40 | `u8*` pos palette | 1 byte / index | **UNREAD** |

No bone index. No parent. Track identity is the **name string**.
Hierarchy parent lives on the C3D 60-byte bone (`MeshFile`), not
in XSEQ.

`00A4CDD0` (wrapper, `ret 16`) skips **4** stream bytes, reads
another 4 into `[edi+4]`, then `00A4C5E0` **overwrites** `+0`/`+4`.
Those eight bytes are persist framing, not `BoneIndex`/`Parent`.

---

## 5. Invented / wrong host fields (short)

| C# field / behavior | Class | Why |
|---|---|---|
| `XSeqTrack.BoneIndex` | **INVENTED** | no such dword on the 44-byte record |
| `XSeqTrack.Parent` | **INVENTED** | same |
| `XSeqTrack.PreFps` | **INVENTED** name | persist `u8` is `+10` flags (`0x1F` / bit 6) |
| palette `width = rotCount > 255 ? 2 : 1` | **DISPROVEN** | always 1-byte; `>0xFF` disables palette |
| `FourCcFoot` / `FourCcPupp` | **INVENTED** | not in exe FourCC getters |
| leftover `TryReadTrack` on non-`XSEQ` chunks | **INVENTED** recover | native only `00A4CDD0` on XSEQ children |
| `Duration` fallback `1f` | **INVENTED** default | ANRT `+48` may be 0 |
| `LooksLikeFourCc` A–Z scan | **PARTIAL** recover | persist tags are real; byte-scan is not `00AA7F40` |
| skip `u32` version if `0 < v < 1000` | **PARTIAL** | ctor writes `0x64` at 3DAF `+12`; not a general version field reader |
| `WakeLoopId` / `WakeLoopName` | **LEFTOVER** fixture | not a format field (`xseq-first`) |

Dump-backed and kept:

| C# | Class |
|---|---|
| `>>>>` / size + raw LZO | **PROVEN** `00A99510` |
| ANRT `u8` + `f32` | **PROVEN** `00A98AF0` |
| name CString on the track | **PROVEN** `00A4C5E0` first payload |
| `f32` @+4, count @+8, `f32` @+12/@+16 | **PROVEN** once cursor is the 44-byte persist body |
| `u16` + `f32[4]*` quats | **PROVEN** |
| `u16` + `i16[3]*` × factor | **PROVEN** `00A4C4BA` / `00A4C577` |
| `ClipRecordBytes = 44` | **PROVEN** in-memory stride, not a file header |
| `BoneLocalBytes = 48` | **PROVEN** uncompressed local (`00A4F0D8`) |

---

## 6. Synthetic test encodes the invented layout

`XSeqFormatTests.BuildSynthetic` writes:

```
i32 boneIndex=2
i32 parent=-1
cstring "Pelvis"
u8 1          // PreFps
f32 15        // fps
u32 2         // frames
4 zero bytes
f32 1 / f32 1
u16 1 + quat
u16 0
u16 1 + i16[3]
```

That is the **host** `TryReadTrack` sketch, not `00A4C5E0`
(name, flags byte, `f32`, `u32`, four packed `u8`s, two `f32`s,
then counts). Wake `3420` only asserts `Tracks.Count > 0` and
bone **names**. Names surviving a scan does not prove
`BoneIndex` / `PreFps` / 2-byte palettes.

---

## 7. C# vs native (runtime leftover)

Unchanged from `xseq-walk-first`: first-key into
`PaletteForPose` is a **PROVEN** format experiment;
`TrySample` ignores `time`; `00AA0090` interp **UNREAD**.
`GetAnim` is the later `00A26D40` type-6 slot, not Leave /
first Present.

Do not treat `BoneIndex`, `Parent`, `PreFps`, `FOOT`, or
`PUPP` as persist fields.
