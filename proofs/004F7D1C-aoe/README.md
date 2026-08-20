# `004EE23F` pair 98 `009B0AC0` / `0044C6B0` is `CAreaOfEffectAttackDef`

Investigation. Host may ship **Note-only**
`NinetyEighthDefClass*` after pair 97.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent CTC names from `004Dxxxx`
helpers. Remaining-pairs: after
`CTCActionUseSearch` later `004D2EF0` rows
are unnamed.
Do **not** invent `ActivateQuest` /
`WASD` / `MUSIC_SET`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover remaining-pairs row 98
`CAreaOfEffectAttackDef` `004F7CF4` factory
`0x4E6CF3` sites `004F7D1C` / `004F7D23`.
One unnamed CTC between 97 and 98.
Factory: persist ctor, size (`push N` /
vtbl[20]), vtbl. First-seen childhood?
Does this registrar `ActivateQuest`?

| Field | Value | Class |
| --- | --- | --- |
| Listing string | `004F7CF4` `"CAreaOfEffectAttackDef"` | **PROVEN** |
| `0044C6B0` | `004F7D1C` | **PROVEN** |
| `009B0AC0` | `004F7D23` | **PROVEN** |
| Factory | `004E6CF3` `00BFEA1A(76)` then `00430370`; vtbl **`0124318C`** | **PROVEN** |
| Persist ctor | `004E3F65` same `00430370` then `[esi]=0124318C`. Factory does **not** `jmp` here. Host `Ctor` is **`00430370`** (factory call; same as pair 97). | **PROVEN** |
| Size | **76** (`push 76` at factory `004E6CF4`; vtbl[20] `004E3F77` `push 76; pop eax; ret`) | **PROVEN** |
| Vtbl | **`0124318C`** | **PROVEN** |
| CTC between 97 and 98 | **1** unnamed `004D2EF0` at `004F7CB1` factory `0x4D5F35` | **PROVEN** count; in-range name **UNREAD** |
| Remaining-pairs row 98 | name / factory / sites / 1 CTC | **MATCH** |
| First-seen childhood? | **No.** Registrar only. Not `00DBDE40`. | **DISPROVEN** |
| `ActivateQuest`? | **No.** Add Def Class is not `ActivateQuest`. | **DISPROVEN** |

Authority: `Fable.exe`
`listing-004c0000.txt` `004F7C79` /
`004F7CF4` / `004E6CF3` / `004E3F65` /
`004E3F77` / `004D5F35` / `004D5F52`;
`listing-00400000.txt` `00430370`;
`listing-008c0000.txt` `008C5F50` /
`008C79B0`;
`fn 004F7D1C`;
`proofs/004EE23F-remaining-pairs` row 98;
`proofs/004EE23F-thing-components`;
`src/Fable.Game/EngineLifecycle.cs`
`NinetySeventhDefClass*`.
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243B7C` **`CAreaOfEffectAttackDef`**.
`assembly/exe/00-index/vtbl.tsv` `0x0124318C`.
`rtti.txt` `0x0137B2F4`
`.?AVCAreaOfEffectAttackDef@@`.

Listing string at `004F7CF4` is
**`CAreaOfEffectAttackDef`** (not invented).
Shape-2 (`push` + `0042DAE0`).

```
004F7CF4  push "CAreaOfEffectAttackDef"
004F7CF9  lea ecx, [ebp-1580]
004F7CFF  call 0099EBF0
004F7D04  push 0x4E6CF3
004F7D09  lea eax, [ebp-1580]
004F7D0F  push eax
004F7D10  lea ecx, [ebp-2364]
004F7D16  call 0042DAE0
004F7D1B  push eax
004F7D1C  call 0044C6B0
004F7D21  mov ecx, eax
004F7D23  call 009B0AC0
```

```
004E6CF3  push esi
004E6CF4  push 76
004E6CF6  call 00BFEA1A
004E6CFB  mov esi, eax
004E6CFD  test esi, esi
004E6CFF  pop ecx
004E6D00  je 004E6D13
004E6D02  mov ecx, esi
004E6D04  call 00430370
004E6D09  mov [esi], 0x124318C
004E6D0F  mov eax, esi
004E6D11  pop esi
004E6D12  ret
004E6D13  xor eax, eax
004E6D15  pop esi
004E6D16  ret

004E3F65  push esi
          mov esi, ecx
          call 00430370
          mov [esi], 0x124318C
          mov eax, esi
          pop esi
          ret

004E3F77  push 76
          pop eax
          ret
```

Host already ships through pair 97
`CBalverineBattleDef` (`NinetySeventhDefClass*`
`004F7C72` / `004E4883` / `00430370` /
`01242F74` / 72). Next leftover pair is
this row.

---

## Verdict

Pair 98 on `004EE23F` **MATCH**es
`proofs/004EE23F-remaining-pairs` row 98.

Init Thing Components **registers** the
def class. It does **not** construct a
Thing instance, does **not** start
first-seen childhood, and does **not**
call `ActivateQuest`.

Do **not** add `ActivateQuest("Q_NewOakValeIntro")`
in `src/`. Note-only + flag. Live 76-byte
object is **LEFTOVER**.

---

## 1. Registrar pair MATCH remaining-pairs row 98

`proofs/004EE23F-remaining-pairs` §5:

| n | `push` | listing string | factory imm | `0044C6B0` | `009B0AC0` | CTC between |
| --: | --- | --- | --- | --- | --- | --: |
| 97 | `004F7C4A` | `CBalverineBattleDef` | `0x4E4883` | `004F7C72` | `004F7C79` | 0 |
| 98 | `004F7CF4` | `CAreaOfEffectAttackDef` | `0x4E6CF3` | `004F7D1C` | `004F7D23` | 1 |

`listing-004c0000.txt` after pair 97
`CBalverineBattleDef` `004F7C79`:

| Listing | Remaining-pairs | Class |
| --- | --- | --- |
| `004F7CF4` `push "CAreaOfEffectAttackDef"` | `004F7CF4` | **MATCH** |
| `004F7D04` `push 0x4E6CF3` | factory `0x4E6CF3` | **MATCH** |
| `004F7D16` `call 0042DAE0` | shape-2 pack | **MATCH** |
| `004F7D1C` `call 0044C6B0` | `004F7D1C` | **MATCH** |
| `004F7D23` `call 009B0AC0` | `004F7D23` | **MATCH** |
| one `004D2EF0` at `004F7CB1` between 97 and 98 | CTC between = 1 | **MATCH** count |

`strings.tsv`:

```
0x01243B7C	0xE43B7C	CAreaOfEffectAttackDef
```

Same listing annotates the immediate as
`"CAreaOfEffectAttackDef"`. `xrefs.tsv`
`0x01243B7C`:

| Site | Fn | Role |
| --- | --- | --- |
| `004F7CF5` | `004F76EB` (greedy parent of `004EE23F`) | this registrar |
| `008C5F54` | `008C5F50` | later type-name intern |
| `008C79B8` | `008C79B0` | later def lookup |

`0042DAE0` is the name+factory pack helper.
Treating it as `009B0AC0` is **DISPROVEN**
(remaining-pairs §2).

Zero-CTC battle cluster
(`CScorpionKingBattleDef` …
`CBalverineBattleDef`) **ends** at pair 97.
Pair 98 is **after** one CTC row. Not
part of that cluster.

---

## 2. CTC between 97 and 98

`listing-004c0000.txt` between
`004F7C79` and `004F7CF4`:

```
004F7C94  lea ecx, [ebp-32]
004F7C97  call 004D5F52
004F7C9C  lea eax, [ebp-32]
004F7C9F  push eax
004F7CA0  call 006869C0
004F7CA5  push eax
004F7CA6  push 0x4D5F35
004F7CAB  lea ecx, [ebp-5364]
004F7CB1  call 004D2EF0
004F7CB6  push eax
004F7CB7  lea eax, [ebp-32]
004F7CBA  push eax
004F7CBB  lea ecx, [ebp-10812]
004F7CC1  call 004D9D2F
004F7CC6  push eax
004F7CC7  mov ecx, esi
004F7CC9  call 004E40C3
```

In-range `004EE23F` has **no** `push "…"`
on this CTC row. Remaining-pairs counted it
unnamed. Helper `004D5F52` (called
`004F7C97`) pushes `"CTCTextureDecal"` then
`0099EBF0`. That string is **out of**
`004EE932`…`004F9144`. Do not promote it
as an in-range registrar name. Factory
imm on that row is `0x4D5F35`
(`00BFEA1A(36)` then `0070A520`). The CTC
row is **not** Add Def Class (no
`0044C6B0` / `009B0AC0`).

---

## 3. Factory size / ctor / vtbl

`listing-004c0000.txt` `004E6CF3`:
`00BFEA1A` with immediate **76**, null →
`xor eax, eax; ret`, else
`mov ecx, eax; call 00430370` then
`[esi]=0x0124318C`.

`00430370` (`listing-00400000.txt`):
`009FBEC0` then base vtbl `01230CBC` and
dword/byte stores through `+54`. Same
base persist ctor as pair 90–97.

In-place persist ctor `004E3F65` is the
same two stores as the factory tail
(`00430370` then class vtbl). Factory
does **not** `jmp 004E3F65`. Host
`NinetyEighthDefClassCtor` is **`00430370`**
to **MATCH** `NinetySeventhDefClassCtor`.

`vtbl.tsv` `0x0124318C`:

| slot | VA | note |
| --: | --- | --- |
| 0 | `004E3FB4` | dtor (`00430300` then optional `00BFE9BC`) |
| 1–17 / 21–24 | shared `0042D930`…`004303E0` family | no invented names |
| **18** | **`004E3F7B`** | persist: `+60` / `+64` via `00431102`; `+68` / `+72` via `00431061` |
| 19 | `004E6CC1` | copy: copies `+60`…`+72` after `00431ED0` |
| **20** | **`004E3F77`** | size `push 76; pop eax; ret` |

Persist intern names at `+60`…`+72` are
**UNREAD**. Object is 76 bytes. Copy ctor
`004E6CC1` is **not** the factory.

RTTI `0x0137B2F4` `.?AVCAreaOfEffectAttackDef@@`
is this **def** vtbl.

---

## 4. First-seen childhood — DISPROVEN

`004EE23F` at this pair:

1. `0099EBF0` name `"CAreaOfEffectAttackDef"`.
2. `0042DAE0` packs factory `004E6CF3`.
3. `0044C6B0` `004F7D1C`.
4. `009B0AC0` `004F7D23` Add Def Class.

No `00DBDE40`. No `StartOakVale`. No
`HerosOldHouse`. No `Q_NewOakValeIntro` /
`S_QNOVI`. Type-register on `"Init Thing
Components"`, not first-seen childhood
spawn. **DISPROVEN.**

---

## 5. Registration is not `ActivateQuest` — DISPROVEN

Pair 62 is `CActivateQuestDef`. This pair
is `CAreaOfEffectAttackDef`. No
`00CB5AD0`. No `004B4A10`. No
`CCreatureAction_ActivateQuest`. Later
xrefs `008C5F50` / `008C79B0` intern /
lookup the type name. They are **not**
`ActivateQuest("Q_NewOakValeIntro")`.

Do **not** invent `ActivateQuest` /
`WASD` / `MUSIC_SET` here.

---

## 6. Next pair / host leftover

Next remaining-pairs row 99:
`CFishingRodDef` `004F7D9E` factory
`0x4D9321` sites `004F7DC6` / `004F7DCD`,
1 unnamed CTC (`004F7D5B` factory
`0x4D5F65`). Factory body / vtbl
**UNREAD** here.

| If host adds… | Leftover is… |
| --- | --- |
| Note-only through 97 (previous) | this pair 98, then 99…111 |
| Note-only `NinetyEighthDefClass*` | still live `009AD6E0` / `009FC4F0` on the 76-byte object (**not** MATCH) |

---

## Host constants (Note-only)

Integrator ships flags only. No live
76-byte object.

| Constant | Value |
| --- | --- |
| `NinetyEighthDefClassSite` | `0x004F7D1C` |
| `NinetyEighthDefClassFactory` | `0x004E6CF3` |
| `NinetyEighthDefClassCtor` | `0x00430370` |
| `NinetyEighthDefClassVtbl` | `0x0124318C` |
| `NinetyEighthDefClassSize` | `76` |
| `NinetyEighthDefClassName` | `"CAreaOfEffectAttackDef"` |

`009B0AC0` site `004F7D23` is consumed by
`AddDefClassFn` Note, not a separate
constant (same as pair 97).

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004F7CF4` | `push "CAreaOfEffectAttackDef"` | **PROVEN** |
| `004F7D1C` / `004F7D23` | pair 98 `0044C6B0` / `009B0AC0` | **PROVEN** leftover |
| `004E6CF3` | factory `00BFEA1A(76)` `00430370` vtbl `0124318C` | **PROVEN** |
| `004E3F65` | in-place persist ctor | **PROVEN** |
| `004E3F77` | vtbl[20] size 76 | **PROVEN** |
| `00430370` | base persist ctor (factory call) | **PROVEN** |
| `0124318C` | class vtbl | **PROVEN** |
| `004F7CB1` | 1 CTC `004D2EF0` factory `0x4D5F35` | **PROVEN** count; name **UNREAD** in-range |
| `004D5F52` | helper `push "CTCTextureDecal"` | **PROVEN** helper string; **not** in-range |
| `004E3F7B` `+60`…`+72` intern names | persist payload | **UNREAD** |
| `00DBDE40` | Oakvale / first-seen childhood | **DISPROVEN** here |
| `ActivateQuest` / `00CB5AD0` | quest activate | **DISPROVEN** here |
| `AddFirstDefClass` Notes 98 | Note-only + flag | **MATCH** registrar; live object **LEFTOVER** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-008c0000.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\strings.tsv`
- `C:\FableCSharp\assembly\exe\00-index\rtti.txt`
- `C:\FableCSharp\proofs\004EE23F-remaining-pairs\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`NinetySeventhDefClass*`)
