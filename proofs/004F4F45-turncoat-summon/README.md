# `004EE23F` pairs 54–55: `CTurncoatDef` / `CSummonableCreatureDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI` /
`StartOakVale` / `HERO_ABILITY_TURNCOAT_*` /
`HERO_ABILITY_SUMMON_*`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent class names: only
`push "…"` listing strings in
`004EE932`…`004F9144`. CTC helper
`push "…"` bodies are out of range
(remaining-pairs counted those rows
unnamed).

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Question: recover remaining-pairs **54**
`CTurncoatDef` `004F4F1D` factory
`0x4E0F9C` sites `004F4F45` / `004F4F4C`
and **55** `CSummonableCreatureDef`
`004F4FD3` factory `0x4D885E` sites
`004F4FFB` / `004F5002`. Listing
factories: size, ctor, vtbl. Childhood
Oakvale use? Next is `CAIScratchpadDef`.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
(`004F4D91`…`004F55BC`; factories
`004E0F9C` / `004DEBA3` / `004D885E` /
`004D4C3E` / `004D4E07` — there is **no**
`listing-004e0000.txt`; `004E*` lives in
the `004c` map);
`listing-007c0000.txt` `007C77F0` /
`007C8000`;
`listing-00780000.txt` `0079C160` /
`0079C4B0`;
`e8.tsv` `004F4F45` / `004F4F4C` /
`004F4FFB` / `004F5002`;
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243EB4` / `0x01243E9C` /
`0x01243E88`;
`assembly/exe/00-index/vtbl.tsv`
`0x0124193C` / `0x0123C3A4`;
`rtti.txt` `0x0137AB58` / `0x01379444`;
`assembly/compiled-defs/game/entries.tsv`
index 53 / 54;
`proofs/004EE23F-remaining-pairs` rows
53–56;
`proofs/004F4A16-expr-will`;
`src/Fable.Game/EngineLifecycle.cs`
`AddFirstDefClass` (read only).

Siblings: `proofs/004EE23F-remaining-pairs`,
`proofs/004F4A16-expr-will`,
`proofs/004F4628-particle-animobj`,
`proofs/004EE23F-thing-components`.

Both pairs are shape-2 (`push` name +
factory + `0042DAE0` + `0044C6B0` +
`009B0AC0`). Listing strings are **not**
invented. `0042DAE0` is the name+factory
pack helper. Treating it as `009B0AC0`
is **DISPROVEN** (remaining-pairs §2).

---

## Verdict

| Field | Pair 54 | Pair 55 | Class |
| --- | --- | --- | --- |
| listing string | `CTurncoatDef` `004F4F1D` | `CSummonableCreatureDef` `004F4FD3` | **PROVEN** |
| `0044C6B0` | `004F4F45` | `004F4FFB` | **PROVEN** |
| `009B0AC0` | `004F4F4C` | `004F5002` | **PROVEN** |
| Factory | `004E0F9C` `00BFEA1A(84)` then `jmp 004DEBA3` | `004D885E` `00BFEA1A(48)` then `jmp 004D4C3E` | **PROVEN** |
| Ctor | `004DEBA3` `0044C0C0`; `[esi]=0124193C`; `[esi+64..72]=0` | `004D4C3E` `0044C0C0`; `[esi]=0123C3A4`; `or [esi+44], -1` | **PROVEN** |
| Size | **84** (`push 84`; vtbl[20] `004DEBC0`) | **48** (`push 48`; vtbl[20] `004D4C54`) | **PROVEN** |
| Vtbl | **`0124193C`** | **`0123C3A4`** | **PROVEN** |
| CTC between previous and this | **3** unnamed (`0x4D5723` … `0x4D56C3`) | **1** unnamed (`0x4D4C0E`) | **PROVEN** count; names **UNREAD** in-range |
| Shape | 2 (`push` + `0042DAE0`) | 2 | **PROVEN** |

| Question | Answer | Class |
| --- | --- | --- |
| Remaining-pairs row 54 / 55? | name / factory / sites / CTC counts | **MATCH** |
| Childhood Oakvale use? | **No.** Init Thing Components intern. Not `00DBDE40`. Not `StartOakVale`. Not `Q_NewOakValeIntro` / `S_QNOVI`. Later `CTCTurncoat` / `CTCSummonableCreature` are will-spell runtime (`HERO_ABILITY_TURNCOAT_*` / `HERO_ABILITY_SUMMON_*`), not childhood. | **DISPROVEN** |
| Host live objects? | **None.** `AddFirstDefClass` Notes through forty-seventh `CFireballSpellLevelDef` `004F3F02` then **returns**. Pairs 48…111 including these two are **LEFTOVER**. | **PROVEN** leftover |
| Next pair? | **`CAIScratchpadDef`** `004F558D` / `004F55B5` / `004F55BC` factory `0x4D4E07` `00BFEA1A(0x9C)` `jmp 007ABB30`. | **PROVEN** sites/factory imm/size; ctor body **UNREAD** |

**Answer:** pair 54 is `CTurncoatDef`
`004F4F45` / `004F4F4C` factory `004E0F9C`
size **84** vtbl `0124193C`. Pair 55 is
`CSummonableCreatureDef` `004F4FFB` /
`004F5002` factory `004D885E` size **48**
vtbl `0123C3A4`. Not Oakvale. Next is
`CAIScratchpadDef`.

---

## 1. Pair 54 — `CTurncoatDef`

`listing-004c0000.txt` after fifty-third
`CWillResponseDef` `004F4DC0`. Three
unnamed `004D2EF0` rows (`push 0x4D5723`
at `004F4DF3`, `0x4D5690` at `004F4E5E`,
`0x4D56C3` at `004F4EC9`). Helpers those
rows `call` (out of `004EE932`…`004F9144`;
do **not** promote as in-range names):
`004D5743` `"CTCFireballSpell"`,
`004D56B0` `"CTCTurncoatSpell"`,
`004D56E0` `"CTCTurncoat"`. Remaining-pairs
row 54 CTC between = **3**. **MATCH**
count. Then:

```
004F4F1D  push "CTurncoatDef"
004F4F22  lea ecx, [ebp-1656]
004F4F28  call 0099EBF0
004F4F2D  push 0x4E0F9C
004F4F32  lea eax, [ebp-1656]
004F4F38  push eax
004F4F39  lea ecx, [ebp-2516]
004F4F3F  call 0042DAE0
004F4F44  push eax
004F4F45  call 0044C6B0
004F4F4A  mov ecx, eax
004F4F4C  call 009B0AC0
```

`004F4F1D` `68 B4 3E 24 01` =
`push 0x01243EB4`. `strings.tsv`:

```
0x01243EB4	0xE43EB4	CTurncoatDef
```

Same listing annotates the immediate as
`"CTurncoatDef"`. Not invented.
`xrefs.tsv` `0x01243EB4`:

| Site | Fn | Role |
| --- | --- | --- |
| `004F4F1E` | `004EE137` (greedy parent of `004EE23F`) | this registrar |
| `007C77F4` | `007C77F0` | later type-name intern |
| `007C8008` | `007C8000` | later def lookup |

`e8.tsv`: `0x004F4F45` → `0x0044C6B0`,
`0x004F4F4C` → `0x009B0AC0`. Shape-2.
Matches remaining-pairs row 54.

`004E0F9C` (`listing-004c0000.txt`):

```
004E0F9C  push 84
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E0FAF
          mov ecx, eax
          jmp 004DEBA3
004E0FAF  xor eax, eax
          ret

004DEBA3  push esi
          mov esi, ecx
          call 0044C0C0
          xor eax, eax
          mov [esi], 0x124193C
          mov [esi+64], eax
          mov [esi+68], eax
          mov [esi+72], eax
          mov eax, esi
          pop esi
          ret

004DEBC0  push 84
          pop eax
          ret
```

Same thunk shape as nineteenth
`004E0B4B`: alloc immediate **84**,
null → `xor eax, eax; ret`, else
`mov ecx, eax; jmp` ctor. Base
`0044C0C0`. Three extra dwords at
`+64` `+68` `+72` from `xor eax, eax`.
Object is 84 bytes (`00BFEA1A(84)`
plus size helper immediately after
the ctor). Sibling `004F4A16-expr-will`
had alloc **PARTIAL**; ctor / vtbl
here **PROVEN**.

`vtbl.tsv` `0x0124193C`:

| slot | VA | note |
| --: | --- | --- |
| 0 | `004E0FB2` | dtor (`004E0FCE` then `[esi]=0x1230BA0` / `009FC550`) |
| 1–17 / 21–24 | shared `0042D930`…`0042DAA0` / `009ACE90` / `009FBEF0` / `009ACAB0` / `009ACB20` family | no invented names |
| 18 | `004E7F24` | persist (below) |
| 19 | `004E2CDF` | copy (`jmp 004E2CE4`) |
| **20** | **`004DEBC0`** | size `push 84; pop eax; ret` |

RTTI `0x0137AB58` `.?AVCTurncoatDef@@`.

Slot 18 persist (`004E7F24`) reads
`+37` (`0043314A` bool), `+40` / `+44` /
`+48` (`00431102`), `+52` / `+56` /
`+60` (`00431061`), `+64` (`00466A47`,
12-byte vector to `+76`), `+76` /
`+80` (`00431061`). Last field at
`+80` **MATCH**es size 84. Slot 19
copy writes the same span (`+37`,
`+40…+60`, `00454886` at `+64`,
`+76` / `+80`). Ctor only zeros the
vector at `+64…+72`; `+40…+60` stay
whatever `0044C0C0` left (base is
the first 40). Intern names of those
fields **UNREAD**. Do **not** invent
chant / absorb / release labels from
later `HERO_ABILITY_TURNCOAT_*`
strings.

Neighbor `004E1006` also `00BFEA1A(84)`
but `call 004DEBD4` and vtbl
`012419A4`. That is **not** this
factory.

`game.bin` `entries.tsv` index **53**
`NULLDEF_CTurncoatDef` raw **80**
(payload, not the 84-byte object).
`INDEX.md` **62** `CTurncoatDef` rows.
Compiled presence is **not** this
intern.

---

## 2. Pair 55 — `CSummonableCreatureDef`

One unnamed `004D2EF0` after pair 54
(`push 0x4D4C0E` at `004F4F7F`). Helper
`004D4C2B` (called `004F4F6D`) pushes
`"CTCSummonableCreature"`. Out of
range. Remaining-pairs CTC between =
**1**. **MATCH** count. Then:

```
004F4FD3  push "CSummonableCreatureDef"
004F4FD8  lea ecx, [ebp-1464]
004F4FDE  call 0099EBF0
004F4FE3  push 0x4D885E
004F4FE8  lea eax, [ebp-1464]
004F4FEE  push eax
004F4FEF  lea ecx, [ebp-2132]
004F4FF5  call 0042DAE0
004F4FFA  push eax
004F4FFB  call 0044C6B0
004F5000  mov ecx, eax
004F5002  call 009B0AC0
```

`004F4FD3` `68 9C 3E 24 01` =
`push 0x01243E9C`. `strings.tsv`:

```
0x01243E9C	0xE43E9C	CSummonableCreatureDef
```

`xrefs.tsv` `0x01243E9C`:

| Site | Fn | Role |
| --- | --- | --- |
| `004F4FD4` | `004EE137` | this registrar |
| `0079C164` | `0079C160` | later type-name intern |
| `0079C4B8` | `0079C4B0` | later def lookup |

`e8.tsv`: `0x004F4FFB` → `0x0044C6B0`,
`0x004F5002` → `0x009B0AC0`. Shape-2.
Matches remaining-pairs row 55.

`004D885E` (same `004c` listing):

```
004D885E  push 48
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004D8871
          mov ecx, eax
          jmp 004D4C3E
004D8871  xor eax, eax
          ret

004D4C3E  push esi
          mov esi, ecx
          call 0044C0C0
          mov [esi], 0x123C3A4
          or [esi+44], -1
          mov eax, esi
          pop esi
          ret

004D4C54  push 48
          pop eax
          ret
```

Alloc **48**, ctor `004D4C3E`. Base
`0044C0C0`. One extra dword at `+44`
set to `-1`. Object is 48 bytes.

`vtbl.tsv` `0x0123C3A4`:

| slot | VA | note |
| --: | --- | --- |
| 0 | `004D8874` | dtor (`[esi]=0x1230BA0` then `009FC550`) |
| 1–17 / 21–24 | shared family | no invented names |
| 18 | `004DE891` | persist: `+40` `00431061`, `+44` `0045228F` |
| 19 | `004E0CC0` | copy `[+40]` / `[+44]` after `00431F10` |
| **20** | **`004D4C54`** | size `push 48; pop eax; ret` |

RTTI `0x01379444`
`.?AVCSummonableCreatureDef@@`.

`game.bin` index **54**
`NULLDEF_CSummonableCreatureDef` raw
**19**. `INDEX.md` **45**
`CSummonableCreatureDef` rows. Same
class of leftover as pair 54: name +
factory into the Def map. Not a
summon. Not a first-seen Thing.

The CTC row factory `004D4C0E` is
`00BFEA1A(28)` then `0079C240`. That
is **not** Add Def Class (no
`0044C6B0` / `009B0AC0`).

---

## 3. Childhood Oakvale — DISPROVEN

This walk is Init Thing Components
class register. `004EE23F` at these
pairs:

1. `0099EBF0` name.
2. `0042DAE0` packs factory.
3. `0044C6B0` / `009B0AC0` Add Def
   Class.

No region. No TNG. No hero create.
`listing-004c0000.txt` in this span
has **zero** `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI` /
`StartOakVale`. Parent is `004EE23F`
(`004EE23F-thing-components`).

Later `.text` uses of the **same
strings** are not this intern:

| VA | Role | On `004EE23F`? |
| --- | --- | --- |
| `007C77F0` | intern `"CTurncoatDef"` | **no** |
| `007C8000` | LoadDef lookup `"CTurncoatDef"` then `[vtbl+56]` | **no** (`e8.tsv` dest only `007C7929`) |
| `007C77D0` | `functions.tsv` `CTurncoatDef\|HERO_ABILITY_TURNCOAT_CHANT` | **no** |
| `0079C160` | intern `"CSummonableCreatureDef"` | **no** |
| `0079C4B0` | LoadDef lookup `"CSummonableCreatureDef"` then `[vtbl+56]` | **no** (`e8.tsv` dest only `0079C279`) |
| `0079C450` | `functions.tsv` `CTCSummonableCreature\|CSummonableCreatureDef` | **no** |

Those are Thing-runtime GetName /
LoadDef, same shape as pair 50's
`007F87A0` (not attach). `HERO_ABILITY_TURNCOAT_SPELL`
`0x0122F2F4` xrefs `00419880` (ability
name table). `HERO_ABILITY_TURNCOAT_CHANT`
/ `_ABSORB` / `_RELEASE` /
`_CONTINUOUS` live under `007C6AA0`…
`007C7C40`. `HERO_ABILITY_SUMMON_SPELL`
xrefs the same `00419880` table;
`HERO_ABILITY_SUMMON_CREATURE_APPEAR`
/ `_DISAPPEAR` / `_UPGRADE` live under
`0079ACC0`…`0079B6F0`. Childhood
Oakvale has **no** will. Treating
this intern as a childhood Turncoat
or Summon cast is **DISPROVEN**.

`CREATURE_OAKVALE_STAG_BEETLE` exists
in `game.bin` (`entries.tsv` 1296,
19 subdefs). That is compiled creature
data, **not** this `009B0AC0`. This
site does not spawn the beetle, does
not attach `CTCTurncoat`, and does
not open Oakvale TNG.

---

## 4. Host leftover

`AddFirstDefClass` currently Notes
through forty-seventh
`CFireballSpellLevelDef` `004F3F02` /
`004D8D10` and **returns**.

| After 47th | Native | Host after 47th |
| --- | --- | --- |
| 48 `CSkeletalMorphDef` … 53 `CWillResponseDef` | remaining-pairs / `004F4A16-expr-will` | **LEFTOVER** |
| 3 unnamed `004D2EF0` | listing `004F4DDB`…`004F4F17` | **LEFTOVER** |
| 54 `CTurncoatDef` `004F4F45` / `004E0F9C` size 84 vtbl `0124193C` | **PROVEN** (this file) | **LEFTOVER** |
| 1 unnamed `004D2EF0` (`0x4D4C0E`) | listing `004F4F67`…`004F4FCD` | **LEFTOVER** |
| 55 `CSummonableCreatureDef` `004F4FFB` / `004D885E` size 48 vtbl `0123C3A4` | **PROVEN** (this file) | **LEFTOVER** |
| 13 unnamed `004D2EF0` then 56 `CAIScratchpadDef` | below | **LEFTOVER** |

Note-only would **MATCH** the listing
sites. Live 84- / 48-byte objects are
still leftover. `009AD6E0` /
`009FC4F0` on each object are **not**
MATCH. `+64…+72` / `[+44]\|=-1`
writes are **UNREAD** in the host
object (there is none).

Not Oakvale. Not a Thing instance.
Not a file I/O site.

---

## 5. Next — `CAIScratchpadDef`

Thirteen unnamed `004D2EF0` after
pair 55 (`push 0x4D4BAE` `004F5035`,
`0x4D4BDE` `004F50A0`, `0x4D4C58`
`004F510B`, `0x4D4C88` `004F5176`,
`0x4D4CB8` `004F51E1`, `0x4D4CEB`
`004F524C`, `0x4D4D1B` `004F52B7`,
`0x4E2B8E` `004F5322`, `0x4E75A0`
`004F538D`, `0x4D4DA1` `004F53F8`,
`0x4D4DD4` `004F5463`, `0x4E762D`
`004F54CE`, `0x4D4E20` `004F5539`).
Last helper `004D4E40` pushes
`"CTCAIScratchpad"` (out of range).
Remaining-pairs row 56 CTC between =
**13**. **MATCH** count. Then:

```
004F558D  push "CAIScratchpadDef"
004F559D  push 0x4D4E07
004F55AF  call 0042DAE0
004F55B5  call 0044C6B0
004F55BA  mov ecx, eax
004F55BC  call 009B0AC0
```

`004F558D` `push 0x01243E88`.
`strings.tsv`:

```
0x01243E88	0xE43E88	CAIScratchpadDef
```

**PROVEN** name / sites / factory imm.
Shape-2. Matches remaining-pairs row
56.

`004D4E07`:

```
004D4E07  push 0x9C
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004D4E1D
          mov ecx, eax
          jmp 007ABB30
004D4E1D  xor eax, eax
          ret
```

Alloc **156** (`0x9C`). Ctor
`007ABB30` (not this listing file)
**UNREAD** here. `game.bin` index **55**
`NULLDEF_CAIScratchpadDef`.

---

## Original

Fifty-fourth Add Def Class on `004EE23F`:

1. `0099EBF0` name `"CTurncoatDef"`.
2. `0042DAE0` packs factory `004E0F9C`.
3. `0044C6B0` `004F4F45`.
4. `009B0AC0` `004F4F4C`.

Factory alloc 84, ctor `004DEBA3`.
Base `0044C0C0`. Vtbl `0124193C`.
Three extra dwords `+64…+72` = `0`.

Fifty-fifth:

1. `0099EBF0` name `"CSummonableCreatureDef"`.
2. `0042DAE0` packs factory `004D885E`.
3. `0044C6B0` `004F4FFB`.
4. `009B0AC0` `004F5002`.

Factory alloc 48, ctor `004D4C3E`.
Base `0044C0C0`. Vtbl `0123C3A4`.
`or [esi+44], -1`.

Neither is childhood Oakvale.

---

## INDEX

| VA / name | Role |
| --- | --- |
| `004EE23F` | Init Thing Components |
| `004F4F1D` / `004F4F45` / `004F4F4C` | pair 54 intern / `0044C6B0` / `009B0AC0` |
| `004E0F9C` / `004DEBA3` | pair 54 factory / ctor |
| `0124193C` | `CTurncoatDef` vtbl |
| `004E7F24` / `004E2CDF` | pair 54 persist / copy |
| `004F4FD3` / `004F4FFB` / `004F5002` | pair 55 intern / `0044C6B0` / `009B0AC0` |
| `004D885E` / `004D4C3E` | pair 55 factory / ctor |
| `0123C3A4` | `CSummonableCreatureDef` vtbl |
| `004DE891` / `004E0CC0` | pair 55 persist / copy |
| `004F55B5` / `004F55BC` | next pair `CAIScratchpadDef` |
| `007C77F0` / `007C8000` | later Turncoat GetName / LoadDef — **not** `004EE23F` |
| `0079C160` / `0079C4B0` | later Summonable GetName / LoadDef — **not** `004EE23F` |
| `00DBDE40` | Oakvale — **DISPROVEN** here |
