# `004EE23F` pairs 104–106: `CNymphDef` / `CSummonDef` / `CCameraCollisionDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI` /
`CREATURE_WOOD_NYMPH_01` / will-summon.
After Leave this walk is `FinalAlbion.wld`
(`0042F44D`) → `"Init Game"` `0042F491` →
`00418DCA` → `[vtbl+4]` `004184BD` →
`00418585` `004EE23F`.
Do **not** invent class names: only
`push "…"` listing strings in
`004EE932`…`004F9144`. CTC helper
`push "…"` bodies are out of range
(remaining-pairs counted those rows
unnamed).
Do **not** invent physics (rigidbodies,
capsules, sweeps, camera colliders).
`proofs/collision-first-seen`: first-seen
after Leave is pose persist + a Thing
bitset collect, not a collision solver.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Question: recover remaining-pairs **104**
`CNymphDef` `004F83F8` factory `0x4D93A0`
sites `004F8420` / `004F8427`, **105**
`CSummonDef` `004F84AE` factory `0x4D93E6`
sites `004F84D6` / `004F84DD`, **106**
`CCameraCollisionDef` `004F85CF` factory
`0x4D9465` sites `004F85F7` / `004F85FE`.
Listing factories: persist ctor, size,
vtbl. Does `CCameraCollisionDef` run on
first Present?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
(`004F81E2`…`004F863C`; factories
`004D93A0` / `004D93E6` / `004D9465`;
persist `004DF1E9` / `004DF264` /
`004DF31C`; size helpers `004D6177` /
`004D61A0` / `004D624B`; copy
`004E1769` / `004E17CD` / `004E187E`;
placement ctors `004D6165` / `004D618E` /
`004D6239`);
`listing-00440000.txt` `0044C0C0`;
`listing-00400000.txt` `00431061` /
`00431102`;
`listing-007c0000.txt` `007E2FD0` /
`007E3290` / `007E3020`;
`e8.tsv` `004F8420` / `004F8427` /
`004F84D6` / `004F84DD` / `004F85F7` /
`004F85FE`;
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243B20` / `0x01243B14` /
`0x01243B00`;
`assembly/exe/00-index/vtbl.tsv`
`0x0123DE3C` / `0x0123DEB4` /
`0x0123DFBC`;
`rtti.txt` `0x013799E8` / `0x01379A00` /
`0x01379A34`;
`assembly/compiled-defs/game/entries.tsv`
index 103 / 104 / 105;
`INDEX.md` counts 7 / 9 / 71;
`proofs/004EE23F-remaining-pairs` rows
103–107;
`proofs/collision-first-seen`;
`src/Fable.Game/EngineLifecycle.cs`
`AddFirstDefClass` (read only; through
`HundredThirdDefClass`
`CSoundAtmospheresDef`).

Siblings: `proofs/004EE23F-remaining-pairs`,
`proofs/004F8E89-hasname-tail`,
`proofs/004F4F45-turncoat-summon`,
`proofs/004F67BA-gold-kickable`.

All three pairs are shape-2 (`push` name +
factory + `0042DAE0` + `0044C6B0` +
`009B0AC0`). Listing strings are **not**
invented. `0042DAE0` is the name+factory
pack helper. Treating it as `009B0AC0`
is **DISPROVEN** (remaining-pairs §2).

---

## Verdict

| Field | Pair 104 | Pair 105 | Pair 106 | Class |
| --- | --- | --- | --- | --- |
| listing string | `CNymphDef` `004F83F8` | `CSummonDef` `004F84AE` | `CCameraCollisionDef` `004F85CF` | **PROVEN** |
| `0044C6B0` | `004F8420` | `004F84D6` | `004F85F7` | **PROVEN** |
| `009B0AC0` | `004F8427` | `004F84DD` | `004F85FE` | **PROVEN** |
| Factory | `004D93A0` `00BFEA1A(80)` then `0044C0C0` | `004D93E6` `00BFEA1A(76)` then `0044C0C0` | `004D9465` `00BFEA1A(44)` then `0044C0C0` | **PROVEN** |
| Persist ctor | `0044C0C0` in-line; placement `004D6165` same vtbl | `0044C0C0` in-line; placement `004D618E` | `0044C0C0` in-line; placement `004D6239` | **PROVEN** |
| Size | **80** (`push 80`; vtbl[20] `004D6177`) | **76** (`push 76`; vtbl[20] `004D61A0`) | **44** (`push 44`; vtbl[20] `004D624B`) | **PROVEN** |
| Vtbl | **`0123DE3C`** | **`0123DEB4`** | **`0123DFBC`** | **PROVEN** |
| CTC between previous and this | **3** unnamed | **1** unnamed | **2** unnamed | **PROVEN** count; names **UNREAD** in-range |
| Shape | 2 (`push` + `0042DAE0`) | 2 | 2 | **PROVEN** |

| Question | Answer | Class |
| --- | --- | --- |
| Remaining-pairs row 104 / 105 / 106? | name / factory / sites / CTC counts | **MATCH** |
| Childhood Oakvale / first Present run? | **No.** Init Thing Components intern. Not `00DBDE40`. Not Lookout Present. | **DISPROVEN** |
| `CCameraCollisionDef` on first Present? | **No.** Type-register on `"Init Thing Components"`. `collision-first-seen`: after Leave there is **no** recovered world collision tick. First Present is LookoutPoint pose persist + `006A80A0` bit `0x64`. Camera seed is `006B3FF0` pose, not this Def. Do **not** invent a camera collider from the name. | **DISPROVEN** |
| Host live objects? | **None.** `AddFirstDefClass` Notes through hundred-third `CSoundAtmospheresDef` `004F82A0` then **returns**. Pairs 104…111 including these three are **LEFTOVER**. | **PROVEN** leftover |
| Next pair? | **`CBettingDef`** `004F8972` / `004F899A` / `004F89A1` factory `0x4D96DD`. 8 unnamed CTC between. | **PROVEN** sites/factory imm; ctor **UNREAD** this pass |

**Answer:** pair 104 is `CNymphDef`
`004F8420` / `004F8427` factory `004D93A0`
size **80** vtbl `0123DE3C`. Pair 105 is
`CSummonDef` `004F84D6` / `004F84DD`
factory `004D93E6` size **76** vtbl
`0123DEB4`. Pair 106 is
`CCameraCollisionDef` `004F85F7` /
`004F85FE` factory `004D9465` size **44**
vtbl `0123DFBC`. Persist ctor is in-line
`0044C0C0`. Not first Present. Not a
solver.

---

## HundredFourth / Fifth / Sixth DefClass constants

Returned for host Note-only (not a live
object). Site is the `0044C6B0` call.
Ctor is the in-line persist `0044C0C0`
(factory does **not** `jmp` a dedicated
ctor; placement wrappers below write the
same vtbl with no extra stores).

```
HundredFourthDefClassSite    = 0x004F8420
HundredFourthDefClassFactory = 0x004D93A0
HundredFourthDefClassCtor    = 0x0044C0C0
HundredFourthDefClassVtbl    = 0x0123DE3C
HundredFourthDefClassSize    = 80
HundredFourthDefClassName    = "CNymphDef"

HundredFifthDefClassSite     = 0x004F84D6
HundredFifthDefClassFactory  = 0x004D93E6
HundredFifthDefClassCtor     = 0x0044C0C0
HundredFifthDefClassVtbl     = 0x0123DEB4
HundredFifthDefClassSize     = 76
HundredFifthDefClassName     = "CSummonDef"

HundredSixthDefClassSite     = 0x004F85F7
HundredSixthDefClassFactory  = 0x004D9465
HundredSixthDefClassCtor     = 0x0044C0C0
HundredSixthDefClassVtbl     = 0x0123DFBC
HundredSixthDefClassSize     = 44
HundredSixthDefClassName     = "CCameraCollisionDef"
```

---

## 1. Pair 104 — `CNymphDef`

`listing-004c0000.txt` after hundred-third
`CSoundAtmospheresDef` `004F82A7`. Three
unnamed `004D2EF0` rows. Helpers those
rows `call` (out of `004EE932`…`004F9144`;
do **not** promote as in-range names):

| `004D2EF0` | helper | factory `push` | helper `push "…"` |
| --- | --- | --- | --- |
| `004F82DF` | `004D6122` | `0x4DF1CC` | `"CTCSoundAtmosphereVillage"` |
| `004F8344` | `004D6152` | `0x4D6135` | `"CTCQuickAccessMenu"` |
| `004F83AF` | `004D617B` | `0x4E3FEF` | `"CTCNymph"` |

Remaining-pairs row 104 CTC between =
**3**. **MATCH** count. Then:

```
004F83F8  push "CNymphDef"
004F83FD  lea ecx, [ebp-1628]
004F8403  call 0099EBF0
004F8408  push 0x4D93A0
004F840D  lea eax, [ebp-1628]
004F8413  push eax
004F8414  lea ecx, [ebp-2460]
004F841A  call 0042DAE0
004F841F  push eax
004F8420  call 0044C6B0
004F8425  mov ecx, eax
004F8427  call 009B0AC0
```

`004F83F8` `68 20 3B 24 01` =
`push 0x01243B20`. `strings.tsv`:

```
0x01243B20	0xE43B20	CNymphDef
```

`xrefs.tsv` `0x01243B20` (xrefs greedy
fn `004F82D6`; `functions.tsv` walk
starts `004EE137` / `004EE23F`):

| Site | Fn | Role |
| --- | --- | --- |
| `004F83F9` | `004F82D6` | this registrar |
| `0080F0A4` | `0080F0A0` | later type-name intern |
| `0080FE68` | `0080FE50` | later def lookup |

`e8.tsv`: `0x004F8420` → `0x0044C6B0`,
`0x004F8427` → `0x009B0AC0`. Shape-2.
Matches remaining-pairs row 104.

`004D93A0` (`listing-004c0000.txt`):

```
004D93A0  push esi
          push 80
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D93C0
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123DE3C
          mov eax, esi
          pop esi
          ret
004D93C0  xor eax, eax
          pop esi
          ret
```

No extra dword stores after the vtbl
write. Object is 80 bytes.

Placement ctor `004D6165` is the same
`0044C0C0` then `[esi]=0123DE3C`. Factory
in-lines it (no `jmp`). Same shape as
`CGoldDef` `004D8EC5` / `CHasNameDef`
`004D98C8`.

`vtbl.tsv` `0x0123DE3C`:

| slot | VA | note |
| --: | --- | --- |
| 0 | `004D93C4` | dtor (`[esi]=0x1230BA0` / `009FC550`) |
| 1–17 / 21–24 | shared `0042D930`…`0042DAA0` / `009ACE90` / `009FBEF0` / `009ACAB0` / `009ACB20` family | no invented names |
| 18 | `004DF1E9` | persist (below) |
| 19 | `004E1769` | copy (`jmp 004E176E`) |
| **20** | **`004D6177`** | size `push 80; pop eax; ret` |

```
004D6177  push 80
          pop eax
          ret
```

RTTI `0x013799E8` `.?AVCNymphDef@@`.

Slot 18 persist (`004DF1E9`) reads
`+40` / `+48` (`00431061` f32), `+44`
(`00431061`; out of store order), `+52`
(`00431061`), `+56` / `+60` / `+64`
(`00431102` u32), `+68` / `+72` / `+76`
(`00431061`). Last field at `+76`
**MATCH**es size 80. Slot 19 copy writes
the same span `+40…+76`. Intern names of
those fields **UNREAD**. Do **not** invent
nymph AI / succubus labels from later
`CREATURE_*_NYMPH_*` strings.

`game.bin` `entries.tsv` index **103**
`NULLDEF_CNymphDef` raw **83**
(payload, not the 80-byte object).
`INDEX.md` **7** `CNymphDef` rows.
Live creature rows (`CREATURE_WOOD_NYMPH_01`
…) are type `CREATURE`, not this intern.
Compiled presence is **not** this
register.

---

## 2. Pair 105 — `CSummonDef`

One unnamed `004D2EF0` after pair 104
(`push 0x4E181D` at `004F845A`). Helper
`004D61A4` (called `004F8448`) pushes
`"CTCAISummon"`. Out of range.
Remaining-pairs CTC between = **1**.
**MATCH** count. Then:

```
004F84AE  push "CSummonDef"
004F84B3  lea ecx, [ebp-1636]
004F84B9  call 0099EBF0
004F84BE  push 0x4D93E6
004F84C3  lea eax, [ebp-1636]
004F84C9  push eax
004F84CA  lea ecx, [ebp-2476]
004F84D0  call 0042DAE0
004F84D5  push eax
004F84D6  call 0044C6B0
004F84DB  mov ecx, eax
004F84DD  call 009B0AC0
```

`004F84AE` `68 14 3B 24 01` =
`push 0x01243B14`. `strings.tsv`:

```
0x01243B14	0xE43B14	CSummonDef
```

`xrefs.tsv` `0x01243B14`:

| Site | Fn | Role |
| --- | --- | --- |
| `004F84AF` | `004F82D6` | this registrar |
| `007FDDC4` | `007FDAD0` | later type-name intern |
| `007FE5B8` | `007FE5B0` | later def lookup |

`e8.tsv`: `0x004F84D6` → `0x0044C6B0`,
`0x004F84DD` → `0x009B0AC0`. Shape-2.
Matches remaining-pairs row 105.

`004D93E6`:

```
004D93E6  push esi
          push 76
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D9406
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123DEB4
          mov eax, esi
          pop esi
          ret
004D9406  xor eax, eax
          pop esi
          ret
```

Placement ctor `004D618E` writes the
same vtbl. No extra stores. Object is
76 bytes.

`vtbl.tsv` `0x0123DEB4`:

| slot | VA | note |
| --: | --- | --- |
| 0 | `004D940A` | dtor (`01230BA0` / `009FC550`) |
| 1–17 / 21–24 | shared family | no invented names |
| 18 | `004DF264` | persist |
| 19 | `004E17CD` | copy (`jmp 004E17D2`) |
| **20** | **`004D61A0`** | size `push 76; pop eax; ret` |

RTTI `0x01379A00` `.?AVCSummonDef@@`.

Slot 18 persist (`004DF264`) reads
`+40` / `+44` / `+48` (`00431102` u32),
`+52` (`00431061` f32), `+56` / `+60`
(`00431102`), `+64` (`00431061`), `+68` /
`+72` (`00431102`). Last field at `+72`
**MATCH**es size 76. Slot 19 copy writes
`+40…+72`. Field names **UNREAD**. Do
**not** steal `HERO_ABILITY_SUMMON_*`
from pair 55 `CSummonableCreatureDef`
(`proofs/004F4F45-turncoat-summon`:
will-spell runtime, not childhood).

`game.bin` index **104**
`NULLDEF_CSummonDef` raw **75**.
`INDEX.md` **9** `CSummonDef` rows.
Compiled presence is **not** this intern.

---

## 3. Pair 106 — `CCameraCollisionDef`

Two unnamed `004D2EF0` after pair 105.
Helpers (out of range):

| `004D2EF0` | helper | factory `push` | helper `push "…"` |
| --- | --- | --- | --- |
| `004F851B` | `004D61B7` | `0x4D942C` | `"CTCWobble"` |
| `004F8586` | `004D6226` | `0x4D6209` | `"CTCCameraCollision"` |

`0x4D942C` alloc **48** then `004D61CA`
(vtbl `0123DF34`, `fld1` at `+40`,
`fldz` at `+44`). `0x4D6209` alloc
**16** then `007E3020`. Those are CTC
rows, **not** this Add Def Class pair.
Remaining-pairs CTC between = **2**.
**MATCH** count. Then:

```
004F85CF  push "CCameraCollisionDef"
004F85D4  lea ecx, [ebp-1644]
004F85DA  call 0099EBF0
004F85DF  push 0x4D9465
004F85E4  lea eax, [ebp-1644]
004F85EA  push eax
004F85EB  lea ecx, [ebp-2492]
004F85F1  call 0042DAE0
004F85F6  push eax
004F85F7  call 0044C6B0
004F85FC  mov ecx, eax
004F85FE  call 009B0AC0
```

`004F85CF` `68 00 3B 24 01` =
`push 0x01243B00`. `strings.tsv`:

```
0x01243B00	0xE43B00	CCameraCollisionDef
```

`xrefs.tsv` `0x01243B00`:

| Site | Fn | Role |
| --- | --- | --- |
| `004F85D0` | `004F82D6` | this registrar |
| `007E2FD4` | `007E2FD0` | later type-name intern (`push -1` / `"CCameraCollisionDef"` / `0099EBF0`) |
| `007E3298` | `007E3290` | later typed get (`[vtbl+56]`) |

`e8.tsv`: `0x004F85F7` → `0x0044C6B0`,
`0x004F85FE` → `0x009B0AC0`. Shape-2.
Matches remaining-pairs row 106.

`004D9465`:

```
004D9465  push esi
          push 44
          call 00BFEA1A
          mov esi, eax
          test esi, esi
          pop ecx
          je 004D9485
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123DFBC
          mov eax, esi
          pop esi
          ret
004D9485  xor eax, eax
          pop esi
          ret
```

Placement ctor `004D6239` writes the
same vtbl. No extra stores. Object is
44 bytes.

`vtbl.tsv` `0x0123DFBC`:

| slot | VA | note |
| --: | --- | --- |
| 0 | `004D9489` | dtor (`01230BA0` / `009FC550`) |
| 1–17 / 21–24 | shared family | no invented names |
| 18 | `004DF31C` | persist |
| 19 | `004E187E` | copy |
| **20** | **`004D624B`** | size `push 44; pop eax; ret` |

```
004DF31C  add ecx, 40
          push ecx
          mov ecx, [esp+8]
          call 00431102
          ret 4
```

One u32 at `+40`. Last field **MATCH**es
size 44. Copy `004E187E`: `00431F10`
then `mov [esi+40], [edi+40]`. Lionhead
name of that dword **UNREAD**. Do **not**
name it radius / AABB / collider.

RTTI `0x01379A34` `.?AVCCameraCollisionDef@@`.

`game.bin` index **105**
`NULLDEF_CCameraCollisionDef` raw **11**.
Serialized raw **11** = 3-byte header +
CRC u32 + payload u32 (**MATCH** one
`00431102`, same as
`proofs/004F4A16-expr-will` pair 52).
`INDEX.md` **71** `CCameraCollisionDef`
rows. Live rows sit on later chest /
object clusters (`entries.tsv` 9244
neighbours `CChestDef` / `CActionUseDef`;
`cactivatequestdef-payloads` 12277).
Compiled presence is **not** this intern
and **not** first Present.

After this pair: unnamed CTC
`004F863C` factory `0x4D624F` helper
`004D626C` `"CTCFadeOutAndIn"`, then
more unnamed CTC, then pair 107
`CBettingDef`.

---

## 4. First Present — `CCameraCollisionDef` does not run

`proofs/collision-first-seen` (do not
re-open; do not invent physics):

| Claim | Class |
| --- | --- |
| Frontend collision tick? | **DISPROVEN** (2D UI, no Things) |
| First physics *object* after Leave? | Type row `CTCPhysicsStandard` `004EE790` / factory `004D297B`. **Not** `CCameraCollisionDef`. |
| First pose used as world XYZ? | TNG `CTCPhysicsStandard.Position*` + RH axes. Lookout props **PROVEN**. |
| First `006A80A0` after Leave? | `0048D400` bit **`0x64`**. Collect filter, not a hit. |
| First locomotion / mesh step? | **None** (`creature-move-first`). |
| Unity-style collider? | **Not recovered.** |

No-save timeline (collision-first-seen):

```
0042F2A2 Leave
  FinalAlbion.wld
0042F491 Init Game
  004EE23F Init Thing Components
    004EE790  CTCPhysicsStandard     // pose type row
    …
    004F85FE  CCameraCollisionDef    // THIS — name intern only
  00416953 Load world
    004FDBC0 LookoutPoint.tng parse
004189C2 first pumps
  dummy WorldMap+156=0
  no 00501450, no 006A80A0
later
  00501450 i=1 LookoutPoint
    0051FD80 Things (pose persist)
    006AC910 CREATURE_HERO @ GuildArrivalHSP
    0048D400  006A80A0 bit 0x64
WorldFrame 0→1: 004A5DF3 006B3FF0     // camera pose, not this Def
```

`CCameraCollisionDef` `009B0AC0` is a
**type-table intern** on Init Thing
Components. It does **not** tick on
first Present. `007E2FD0` /
`007E3290` are later leftover intern /
`[vtbl+56]` lookup (same pattern as
`CGoldDef` `006A93C0` / `006AE320`).
Unnamed CTC `CTCCameraCollision`
factory `004D6209` `00BFEA1A(16)` →
`007E3020` is **not** pair 106 and is
**not** on the first-seen spine.
`e8.tsv` has **no** `.text` `E8` of
`007E2DF0` (vtbl-only; first call
**UNREAD**; **DISPROVEN** as Leave /
first Present).

Host first-seen camera is `006B3FF0`
pose (`WorldCamera.PoseFn`), not a
collision object. Do **not** invent a
solver those deeds can bounce the
camera off.

---

## 5. Host leftover

`EngineLifecycle.AddFirstDefClass`
currently Notes through hundred-third
`CSoundAtmospheresDef` `004F82A0`.
Pairs 104 / 105 / 106 are **not**
Noted. **PROVEN** leftover intern;
**MATCH** sites if later Note-only +
`*DefClassRegistered` flag. MATCH is
Notes+flag, **not** a live 80 / 76 /
44-byte object.

This walk is **not** Oakvale. **Not**
Lookout Thing construct. Parent is
`004EE23F`.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004F8420` / `004F8427` | pair 104 `CNymphDef` | **PROVEN** leftover |
| `004D93A0` | factory `00BFEA1A(80)` `0044C0C0` vtbl `0123DE3C` | **PROVEN** |
| `004F84D6` / `004F84DD` | pair 105 `CSummonDef` | **PROVEN** leftover |
| `004D93E6` | factory `00BFEA1A(76)` `0044C0C0` vtbl `0123DEB4` | **PROVEN** |
| `004F85F7` / `004F85FE` | pair 106 `CCameraCollisionDef` | **PROVEN** leftover |
| `004D9465` | factory `00BFEA1A(44)` `0044C0C0` vtbl `0123DFBC` | **PROVEN** |
| `004DF1E9` / `004DF264` / `004DF31C` | persist slot 18 | **PROVEN** offsets; field names **UNREAD** |
| `007E2FD0` / `007E3290` | later intern / typed get | **PROVEN** leftover vs first Present |
| `007E3020` / `004D6209` | unnamed `CTCCameraCollision` CTC | **PROVEN** CTC row; **DISPROVEN** as pair 106 / first Present |
| first Present collision solver | — | **DISPROVEN** (`collision-first-seen`) |
