# `004EE23F` pairs 50–51: `CParticleAttacherDef` / `CAnimatingObjectDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` → `"Init Game"`
`0042F491` → `00418DCA` → `[vtbl+4]`
`004184BD` → `00418585` `004EE23F`.
Do **not** invent particle GPU / a `0x20`
soup / `PARTICLE_FRONTEND` / named
fire–insect–dust draw. Do **not** invent
CTC names from `004Dxxxx` helpers.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Question: recover remaining-pairs **50**
`CParticleAttacherDef` `004F4600` factory
`0x4E2AFA` sites `004F4628` / `004F462F`
and **51** `CAnimatingObjectDef` `004F46B6`
factory `0x4EBA6E` sites `004F46DE` /
`004F46E5`. `FirstSeenCanRenderParticles=false`
is already locked — does
`CParticleAttacherDef` still register on
Init Thing Components? Does first-seen
skip attach?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
(`004F439E`…`004F4A1D`; factories
`004E2AFA` / `004E0B9C` / `004EBA6E` /
`004EA1F0` — there is **no**
`listing-004e0000.txt`; `004E*` lives in
the `004c` map);
`listing-00400000.txt` `0041855B` /
`00418585` / `0041888B`;
`listing-007c0000.txt` `007F8540` /
`007F8590` / `007F87A0`;
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243F00` / `0x01243EEC`;
`assembly/exe/00-index/vtbl.tsv`
`0x01242364` / `0x0124376C`;
`proofs/004EE23F-remaining-pairs` rows
50–52;
`proofs/004EE23F-thing-components`;
`proofs/particles-first-seen`;
`src/Fable.Game/EngineLifecycle.cs`
(`FirstSeenCanRenderParticles`,
`FirstSeenRunningParticleListEmpty`,
`InitGameStages`, `AddFirstDefClass`)
read only;
`Dx9SubmitCapabilities.CanRenderParticles`.

Siblings: `proofs/004EE23F-remaining-pairs`,
`proofs/004EE23F-twentyfirst-class`,
`proofs/particles-first-seen`,
`proofs/particles-game`.

---

## Verdict

| Field | Pair 50 | Pair 51 | Class |
| --- | --- | --- | --- |
| listing string | `CParticleAttacherDef` `004F4600` | `CAnimatingObjectDef` `004F46B6` | **PROVEN** |
| `0044C6B0` | `004F4628` | `004F46DE` | **PROVEN** |
| `009B0AC0` | `004F462F` | `004F46E5` | **PROVEN** |
| Factory | `004E2AFA` `00BFEA1A(52)` then `jmp 004E0B9C` | `004EBA6E` `00BFEA1A(72)` then `jmp 004EA1F0` | **PROVEN** |
| Ctor | `004E0B9C` `0044C0C0`; `[esi]=01242364`; `[esi+40..48]=0` | `004EA1F0` `0044C0C0`; `[esi]=0124376C`; `005DD2E0` at `+40` | **PROVEN** |
| Size | **52** (`push 52`; vtbl[20] `004E0BB9`) | **72** (`push 72`; vtbl[20] `004EA20A`) | **PROVEN** |
| Vtbl | **`01242364`** | **`0124376C`** | **PROVEN** |
| CTC between previous and this | **5** unnamed (`0x4D4957` … `0x4E0BBD`) | **1** unnamed (`0x4E0C1E`) | **PROVEN** count; names **UNREAD** |
| Shape | 2 (`push` + `0042DAE0`) | 2 | **PROVEN** |

| Question | Answer | Class |
| --- | --- | --- |
| Still register on Init Thing Components while `FirstSeenCanRenderParticles=false`? | **Yes.** `004F4628` / `004F462F` is unconditional on `004EE23F`. That fn has **0** `cmp [0x13B8648]`. Parent `00418585` is the first named apply, **before** the skip-particles gate `0041888B`. | **PROVEN** |
| Does first-seen skip attach? | **Yes on this walk.** Init Thing Components intern + Add Def Class only. It does **not** call `007F8590` (attacher spawn). `FirstSeenCanRenderParticles=false` is GPU submit (`Dx9SubmitCapabilities.CanRenderParticles`), not a skip of `009B0AC0`. | **PROVEN** |
| Streetlamp `#11459` starts `NParticleEngine` on first Present? | **Not this site.** Lookout TNG authors the sub; running list on first Present is already empty. Draw remains **no GPU**. | **DISPROVEN** as this register; Thing-spawn `007F8590` **UNREAD** (no `.text` `E8`) |
| This walk is Oakvale VFX? | **No.** | **DISPROVEN** |

**Answer:** pair 50 is `CParticleAttacherDef`
`004F4628` / `004F462F` factory `004E2AFA`
size 52 vtbl `01242364`. Pair 51 is
`CAnimatingObjectDef` `004F46DE` /
`004F46E5` factory `004EBA6E` size 72
vtbl `0124376C`. `FirstSeenCanRenderParticles=false`
does **not** skip intern. First-seen
**skips attach** on this site. Next pair
is `CExpressionSubDef` `004F4A16` /
`004F4A1D` factory `0x4D8818`.

---

## 1. Pair 50 — `CParticleAttacherDef`

`listing-004c0000.txt` after forty-ninth
`CTrapDef` `004F43CD`. Five unnamed
`004D2EF0` rows (`push 0x4D4957` at
`004F4400`, `0x4E7E46` at `004F446B`,
`0x4D87DF` at `004F44D6`, `0x4D4A13` at
`004F4541`, `0x4E0BBD` at `004F45AC`).
Then:

```
004F4600  push "CParticleAttacherDef"
004F4605  lea ecx, [ebp-1624]
004F460B  call 0099EBF0
004F4610  push 0x4E2AFA
004F4615  lea eax, [ebp-1624]
004F461B  push eax
004F461C  lea ecx, [ebp-2452]
004F4622  call 0042DAE0
004F4627  push eax
004F4628  call 0044C6B0
004F462D  mov ecx, eax
004F462F  call 009B0AC0
```

`004F4600` `68 00 3F 24 01` =
`push 0x01243F00`. `strings.tsv`:

```
0x01243F00	0xE43F00	CParticleAttacherDef
```

Same listing annotates the immediate as
`"CParticleAttacherDef"`. Not invented.
`xrefs.tsv` `0x01243F00` first hit
`0x004F4601`. Shape-2 (`push` +
`0042DAE0`). Matches remaining-pairs
row 50.

`004E2AFA` (`listing-004c0000.txt`; no
`listing-004e0000.txt`):

```
004E2AFA  push 52
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E2B0D
          mov ecx, eax
          jmp 004E0B9C
004E2B0D  xor eax, eax
          ret

004E0B9C  push esi
          mov esi, ecx
          call 0044C0C0
          xor eax, eax
          mov [esi], 0x1242364
          mov [esi+40], eax
          mov [esi+44], eax
          mov [esi+48], eax
          mov eax, esi
          pop esi
          ret

004E0BB9  push 52
          pop eax
          ret
```

Same thunk shape as nineteenth
`004E0B4B`: alloc immediate **52**,
null → `xor eax, eax; ret`, else
`mov ecx, eax; jmp` ctor. Base
`0044C0C0`. Three extra dwords at
`+40` `+44` `+48` from `xor eax, eax`.
Object is 52 bytes (`00BFEA1A(52)`
plus size helper immediately after
the ctor).

`vtbl.tsv` `0x01242364` slot 20 is
`004E0BB9`. Slot 0 is `004E2B10`
(`mov [esi], 0x1230BA0` then
`009FC550`). Slots 1–17 / 21–24 are
the shared `0042D930`…`0042DAA0` /
`009ACE90` / `009FBEF0` / `009ACAB0` /
`009ACB20` family. Slot 18 `004E9CAA`
is `add ecx, 40` then a `00404500`
pack of `0x122D70E` — **not** attach.
Slot 19 `004E299B` copies `+40`. No
invented names.

---

## 2. Pair 51 — `CAnimatingObjectDef`

One unnamed `004D2EF0` after pair 50
(`push 0x4E0C1E` at `004F4662`). Then:

```
004F46B6  push "CAnimatingObjectDef"
004F46BB  lea ecx, [ebp-1448]
004F46C1  call 0099EBF0
004F46C6  push 0x4EBA6E
004F46CB  lea eax, [ebp-1448]
004F46D1  push eax
004F46D2  lea ecx, [ebp-2100]
004F46D8  call 0042DAE0
004F46DD  push eax
004F46DE  call 0044C6B0
004F46E3  mov ecx, eax
004F46E5  call 009B0AC0
```

`004F46B6` `68 EC 3E 24 01` =
`push 0x01243EEC`. `strings.tsv`:

```
0x01243EEC	0xE43EEC	CAnimatingObjectDef
```

Shape-2. Matches remaining-pairs row 51.

`004EBA6E` (same `004c` listing):

```
004EBA6E  push 72
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004EBA81
          mov ecx, eax
          jmp 004EA1F0
004EBA81  xor eax, eax
          ret

004EA1F0  push esi
          mov esi, ecx
          call 0044C0C0
          lea ecx, [esi+40]
          mov [esi], 0x124376C
          call 005DD2E0
          mov eax, esi
          pop esi
          ret

004EA20A  push 72
          pop eax
          ret
```

`005DD2E0` (`listing-005c0000.txt`)
zeros a 32-byte record at `ecx`
(`[0..24]=0`, `[+13]=0`, `[+28]=-1`).
Ctor writes vtbl then inits `+40` as
that record. Object is 72 bytes
(40-byte `0044C0C0` base + 32).

`vtbl.tsv` `0x0124376C` slot 20 is
`004EA20A`. Slot 0 is `004EBA84`.
Slots 1–17 / 21–24 shared family.
Slot 18 `004ED459` / slot 19
`004EBBF2` are later copy helpers,
**not** this intern.

---

## 3. `FirstSeenCanRenderParticles=false` does not skip intern

Host lock (`EngineLifecycle.cs`):

```
public const bool FirstSeenCanRenderParticles = false;
public const bool FirstSeenRunningParticleListEmpty = true;
```

Tests assert both. `CanRenderParticles`
is a `Dx9SubmitCapabilities` Present
flag (`Dx9SubmitOwnership.cs`). Default
`false`. No setter. That is **GPU
submit**, not Add Def Class.

Init Game parent (`listing-00400000.txt`):

```
0041855B  push "Init Thing Components"
00418585  call 004EE23F          ; no ecx=esi; no 13B8648
…
0041888B  cmp [0x13B8648], bl    ; skip-particles / skip-frontend
00418891  jne 004188E5
00418894  push "Load Particles"
004188E0  call 004174F1
```

`InitGameStages[0]` is `"Init Thing
Components"` `004EE23F`. `"Load
Particles"` `004174F1` is last, and
only if `[0x13B8648]==0`. First-seen
no-save **does** open `PARTICLE_MAIN`
(`particles-first-seen`). That bank
open is **not** this pair.

`listing-004c0000.txt` has **0**
`13B8648` hits. `004EE23F` never
reads `FirstSeenCanRenderParticles`.
Pair 50 still `call 0044C6B0` /
`009B0AC0`. Register on Init Thing
Components is **PROVEN**. Treating
`FirstSeenCanRenderParticles=false`
as a skip of `CParticleAttacherDef`
intern is **DISPROVEN**.

game.bin NULLDEF order matches the
walk (`entries.tsv` 49 / 50 /
`NULLDEF_CParticleAttacherDef` /
`NULLDEF_CAnimatingObjectDef`). That
is compiled-def presence, not a
running emitter.

---

## 4. First-seen skip attach — this walk does not attach

Attach is **not** `009B0AC0`. Later
Thing-runtime (`listing-007c0000.txt`):

| VA | Role | On `004EE23F`? |
| --- | --- | --- |
| `007F8690` | `GetName` `"CParticleAttacherDef"` | **no** |
| `007F87A0` | LoadDef lookup `"CParticleAttacherDef"` then `[vtbl+56]` | **no** |
| `007F8540` | list trim / `004C9B80` | **no** |
| `007F8590` | position + `00703210` spawn (generic def-by-name, `morph-first`) | **no** |

`e8.tsv` dest `007F8590`: **none**.
No `.text` `E8`. Callers are vtbl,
not this intern. `004EE23F` first
`E8` dests are map seed / CTC pack /
`0044C6B0` / `009B0AC0`
(`004EE23F-thing-components`). First
work is type-register, not spawn.

So first-seen **skips attach on this
site**. `FirstSeenCanRenderParticles=false`
is the Present GPU skip, stacked on
top. `FirstSeenRunningParticleListEmpty`
already locks the running-system list
on Lookout Present as **∅**.

Lookout streetlamp
`OBJECT_STREETLAMP_LIT_SINGLE_01` sub
`CParticleAttacherDef` `#11459` is
authored (`particles-first-seen` /
environment investigation). Whether
later Thing construct hits `007F8590`
is **UNREAD** here (no `E8`). Do
**not** emit invented fire/glow
billboards. First Present still
submits **no** particle GPU.

`CAnimatingObjectDef` intern is the
same class of leftover: name +
factory into the Def map. Not a
mesh. Not a first-seen C3D.

---

## 5. Host leftover

`AddFirstDefClass` currently Notes
through forty-third `CFlammableDef`
`004F357A` / `004E3DC3` and
**returns**. Pairs 44…111 including
these two are **LEFTOVER**.

| After 43rd | Native | Host after 43rd |
| --- | --- | --- |
| 44 `CBoastingPodiumDef` … 49 `CTrapDef` | remaining-pairs | **LEFTOVER** |
| 5 unnamed `004D2EF0` | listing `004F43E8`…`004F45FA` | **LEFTOVER** |
| 50 `CParticleAttacherDef` `004F4628` / `004E2AFA` size 52 vtbl `01242364` | **PROVEN** (this file) | **LEFTOVER** |
| 1 unnamed `004D2EF0` (`0x4E0C1E`) | listing `004F464A`…`004F46B0` | **LEFTOVER** |
| 51 `CAnimatingObjectDef` `004F46DE` / `004EBA6E` size 72 vtbl `0124376C` | **PROVEN** (this file) | **LEFTOVER** |
| 7 unnamed `004D2EF0` then 52 `CExpressionSubDef` | below | **LEFTOVER** |

Note-only would **MATCH** the listing
sites. Live 52- / 72-byte objects are
still leftover. `+40…+48` / `005DD2E0`
writes are **UNREAD** in the host
object (there is none).

Not Oakvale. Not a Thing instance.
Not a file I/O site.

---

## 6. Next — `CExpressionSubDef`

Seven unnamed `004D2EF0` after pair 51
(`push 0x4D4A69` `004F4718`,
`0x4D4A99` `004F4783`, `0x4D4ACC`
`004F47EE`, `0x4E2B4B` `004F4859`,
`0x4D4B12` `004F48C4`, `0x4D4B42`
`004F492F`, `0x4DC78F` `004F499A`).
Then remaining-pairs row 52:

```
004F49EE  push "CExpressionSubDef"
004F49FE  push 0x4D8818
004F4A10  call 0042DAE0
004F4A16  call 0044C6B0
004F4A1B  mov ecx, eax
004F4A1D  call 009B0AC0
```

**PROVEN** name / sites / factory imm.
Factory body **UNREAD** here.

---

## Original

Fiftieth Add Def Class on `004EE23F`:

1. `0099EBF0` name `"CParticleAttacherDef"`.
2. `0042DAE0` packs factory `004E2AFA`.
3. `0044C6B0` `004F4628`.
4. `009B0AC0` `004F462F`.

Factory alloc 52, ctor `004E0B9C`.
Base `0044C0C0`. Vtbl `01242364`.
Three extra dwords `+40…+48` = `0`.

Fifty-first:

1. `0099EBF0` name `"CAnimatingObjectDef"`.
2. `0042DAE0` packs factory `004EBA6E`.
3. `0044C6B0` `004F46DE`.
4. `009B0AC0` `004F46E5`.

Factory alloc 72, ctor `004EA1F0`.
Base `0044C0C0`. Vtbl `0124376C`.
`005DD2E0` at `+40`.

`FirstSeenCanRenderParticles=false`
does not skip either pair. First-seen
does not attach from this walk.

---

## INDEX

| VA / name | Role |
| --- | --- |
| `004EE23F` | Init Thing Components |
| `004F4600` / `004F4628` / `004F462F` | pair 50 intern / `0044C6B0` / `009B0AC0` |
| `004E2AFA` / `004E0B9C` | pair 50 factory / ctor |
| `01242364` | `CParticleAttacherDef` vtbl |
| `004F46B6` / `004F46DE` / `004F46E5` | pair 51 intern / `0044C6B0` / `009B0AC0` |
| `004EBA6E` / `004EA1F0` | pair 51 factory / ctor |
| `0124376C` | `CAnimatingObjectDef` vtbl |
| `005DD2E0` | pair 51 `+40` zero record |
| `004F4A16` / `004F4A1D` | next pair `CExpressionSubDef` |
| `0x013B8648` | skip-frontend / skip Load Particles — **not** this fn |
| `FirstSeenCanRenderParticles` | GPU submit; default **false** |
| `007F8590` | later attacher spawn — **not** `004EE23F` |
| `#11459` | Lookout streetlamp sub; first Present system **not** this site |
