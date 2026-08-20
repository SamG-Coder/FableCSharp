# Pairs 52–53: `CExpressionSubDef` / `CWillResponseDef` (not `+120` quest names)

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** wire `007EF200` as Oakvale.
Do **not** start at `00DBDE40` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` → `"Init Game"`
`0042F491` → `00418DCA` → `[vtbl+4]`
`004184BD` → `00418585` `004EE23F`.
Do **not** invent class names: listing
`push "…"` plus the intern helpers those
rows `call`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: recover remaining-pairs **52–53**
(`CExpressionSubDef` `004F49EE` factory
`0x4D8818` sites `004F4A16` / `004F4A1D`;
`CWillResponseDef` `004F4D91` factory
`0x4D9629` sites `004F4DB9` / `004F4DC0`).
Relation to `CTCExpression` `007EF200`
(vtbl `0x0124026C`) and `CActivateQuestDef`?
Does `CExpressionSubDef+120` hold quest
names?

Authority: `Fable.exe` ExeIndex
`listing-004c0000.txt` `004F46B6`…`004F4F4C`
/ `004D8818` / `004D9629` / `004DE881` /
`004DF562` / `004DC78F` / `004DC7E8` /
`004DB050` / `004DB085` / `004D8A32`;
`listing-007c0000.txt` `007EF200` /
`007EEFE0` / `007EF070` / `007E3AD0` /
`007E47D0`;
`listing-005c0000.txt` `005D8CF0` /
`005DA240` / `005F81BE`;
`listing-00440000.txt` `0044C0C0`;
`listing-00400000.txt` `00431102` /
`0040FE60`;
`listing-00680000.txt` `006869D0`;
`vtbl.tsv` `0x0123C2E4` / `0x0123E324` /
`0x012401F4` / `0x0124026C` / `0x0123C7F4`;
`rtti.txt` `CExpressionSubDef` `0x01379424`
/ `CWillResponseDef` `0x01379B20` /
`CExpressionDef` `0x01376DCC` /
`CTCExpression` `0x0137A424`;
`strings.tsv` / `xrefs-by-string.tsv`;
`compiled-defs/game/entries.tsv` /
`names.tsv` / `INDEX.md`;
`src/Fable.Game/EngineLifecycle.cs`
`AddFirstDefClass` through
`FortyFourthDefClassName`;
siblings `proofs/004EE23F-remaining-pairs`
rows 51–54, `proofs/ctcexpression-plus120-writers`,
`proofs/ctcexpression-quest-names`,
`proofs/007EF200-first-plus120`,
`proofs/00456A5A-expression-plus120`,
`proofs/cactivatequestdef-payloads`.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Pair 52? | **`CExpressionSubDef`**. Shape-2 `push` + `0042DAE0`. `0044C6B0` `004F4A16`, `009B0AC0` `004F4A1D`. Factory `004D8818` `00BFEA1A(44)` → `0044C0C0` vtbl **`0x0123C2E4`**. | **PROVEN** |
| Pair 53? | **`CWillResponseDef`**. Same shape. `0044C6B0` `004F4DB9`, `009B0AC0` `004F4DC0`. Factory `004D9629` `00BFEA1A(45)` → `0044C0C0` vtbl **`0x0123E324`**. | **PROVEN** |
| Does `CExpressionSubDef+120` hold quest names? | **No.** Object is **44** bytes (`push 44` factory and slot 20 `004D4B97`). Persist is one u32 at **`+40`**. Compiled raw **11**. No `+120`. | **DISPROVEN** |
| Same `+120` `007EF200` copies into `004B4A10`? | **No.** That slot is compiled **`EXPRESSION+120`** (size `0x90`), read as `esi = [[0x8F+12]]`. Not this CDef. | **DISPROVEN** |
| Same class as `CActivateQuestDef`? | **No.** Pair **62**, factory `004D8A32` `00BFEA1A(48)`, vtbl `0x0123C7F4`, persist intern at `+40` via `007B5740`. | **DISPROVEN** |
| Wire `ActivateQuest("Q_NewOakValeIntro")` from these VAs? | **No.** | **DISPROVEN** |
| Next pair? | **`CTurncoatDef`** `004F4F1D` / `004F4F45` / `004F4F4C` factory `0x4E0F9C` `00BFEA1A(84)`. | **PROVEN** sites; factory body **PARTIAL** (alloc only) |

**Answer:** 52–53 are leftover Add Def Class
pairs. `CExpressionSubDef` is a 44-byte CDef
whose only extra field is `+40` (raw u32 id
into `009AD9E0`). It is a **bridge** to
`[CExpressionDef+12]`, not the quest-name
slot `007EF200` ticks. `CWillResponseDef`
is eight bytes at `+37…+44`. Neither is
`CActivateQuestDef`. Neither is Oakvale.

---

## Direct answers

| Field | Pair 52 | Pair 53 |
| --- | --- | --- |
| Listing string | `004F49EE` `"CExpressionSubDef"` `0x01243ED8` | `004F4D91` `"CWillResponseDef"` `0x01243EC4` |
| Factory imm | `0x4D8818` | `0x4D9629` |
| `0044C6B0` | `004F4A16` | `004F4DB9` |
| `009B0AC0` | `004F4A1D` | `004F4DC0` |
| Alloc | **44** | **45** |
| Ctor | `0044C0C0` then `[esi]=0x0123C2E4` | `0044C0C0` then `[esi]=0x0123E324` |
| Size slot 20 | `004D4B97` `push 44` | `004D649F` `push 45` |
| Persist slot 18 | `004DE881` `+40` `00431102` u32 | `004DF562` `+37…+44` `0043314A` ×8 |
| Copy slot 19 | `004E0CA7` dword `+40` | `004E18C9` eight bytes |
| RTTI | `0x01379424` | `0x01379B20` |
| `game.bin` | **39** rows, raw **11**, ASCII empty | **34** rows, raw **43**, ASCII empty |
| CTC immediately before | `"CTCExpression"` + factory `0x4DC78F` | `"CTCWillResponse"` + factory `0x4D645D` |

---

## 1. Pair 52 sites (listing-004c0000)

After pair 51 `CAnimatingObjectDef`
`004F46E5`. Shape-2 (remaining-pairs §2):

```
004F49ED  push edi
004F49EE  push "CExpressionSubDef"
004F49F3  lea ecx, [ebp-1568]
004F49F9  call 0099EBF0
004F49FE  push 0x4D8818
004F4A0A  lea ecx, [ebp-2340]
004F4A10  call 0042DAE0
004F4A16  call 0044C6B0
004F4A1B  mov ecx, eax
004F4A1D  call 009B0AC0
```

`strings.tsv` `0x01243ED8` `CExpressionSubDef`.
`xrefs-by-string.tsv` first `.text` hit
`0x004F49EF` (this push). Other hits:
`005D8CF4` intern helper; `005DA248`
lookup. **No** `0x012C5D14`.

### Factory / vtbl / size

```
004D8818  push esi
          push 44
          call 00BFEA1A
          … je 004D8838
          mov ecx, esi
          call 0044C0C0
          mov [esi], 0x123C2E4
          mov eax, esi
          pop esi
          ret
```

`0044C0C0` is CDef base (`009FBEC0`,
`[esi+36] &= 0xF8`, `[esi+28]=0`,
vtbl `0x01231D54`). No store at `+40`
in this factory. No store at `+120`.

Neighbor ctor `004D4B85` writes the same
vtbl. Slot 20 sits immediately after it:

```
004D4B97  push 44
          pop eax
          ret
```

`vtbl.tsv` `0x0123C2E4`:

| Slot | Dest | Role |
| --: | --- | --- |
| 0 | `004D883C` | dtor (`00686830` / free) |
| 18 | `004DE881` | persist |
| 19 | `004E0CA7` | copy `[+40]` |
| 20 | `004D4B97` | size **44** |

Slots 1–17 / 21–24 are the shared
`0042D930`…`0042DAA0` / `009ACE90` /
`009ACAB0` family. **PROVEN.**

### Persist is `+40` u32, not a CString

```
004DE881  add ecx, 40
          push ecx
          mov ecx, [esp+8]
          call 00431102
          ret 4
```

`00431102` (`listing-00400000`):
`00404500` then load arm `0040FE60`
(4-byte stream copy into dest). **Not**
CString helper `00431143`. **Not**
`0045228F` (the `EXPRESSION+120` /
`CActivateQuestDef+40` intern helper).

Copy `004E0CA7`: `00431F10` then
`mov [esi+40], [edi+40]`. One dword.

Serialized raw **11** = 3-byte header +
CRC u32 + payload u32. **MATCH.**

---

## 2. Pair 53 sites

```
004F4D90  push edi
004F4D91  push "CWillResponseDef"
004F4D96  lea ecx, [ebp-1456]
004F4D9C  call 0099EBF0
004F4DA1  push 0x4D9629
004F4DAD  lea ecx, [ebp-2116]
004F4DB3  call 0042DAE0
004F4DB9  call 0044C6B0
004F4DBE  mov ecx, eax
004F4DC0  call 009B0AC0
```

`strings.tsv` `0x01243EC4`. Other xrefs:
`007E3AD4` intern helper; `007E47D8`
lookup (`CTCWillResponse` persist path).

```
004D9629  push esi
          push 45
          call 00BFEA1A
          … je 004D9649
          call 0044C0C0
          mov [esi], 0x123E324
          ret
```

Size getter `004D649F` `push 45`.
Neighbor ctor `004D648D` same vtbl.

`vtbl.tsv` `0x0123E324` slot 18
`004DF562`, slot 19 `004E18C9`,
slot 20 `004D649F`.

```
004DF562  lea eax, [esi+37]  call 0043314A
          … +38 +39 +40 +41 +42 +43 …
          add esi, 44
          call 0043314A          ; +44
          ret 4
```

Eight **bytes**. `0043314A` is the same
u8 helper `EXPRESSION` uses at `+124`.
Copy `004E18C9` copies those eight
bytes. Raw **43** = 3-byte header +
8×(CRC + u8). **MATCH.** No CString.
No `+120`. Size 45 cannot host it.

---

## 3. `CExpressionSubDef+120` is **DISPROVEN**

| Claim | Class | Evidence |
| --- | --- | --- |
| Field at `+120` on this object | **DISPROVEN** | alloc 44; persist ends at `+40`; copy one dword |
| Compiled payload is a quest CString | **DISPROVEN** | raw 11; ASCII column empty; `names.bin` has type / `NULLDEF` only |
| Instance name `Q_NewOakValeIntro` | **DISPROVEN** | 39 rows: `NULLDEF_CExpressionSubDef` + unnamed `CExpressionSubDef` |
| Same CRC / helper as `EXPRESSION+120` | **DISPROVEN** | `+120` is `0045228F` CRC `0x1FB35A1B`; this `+40` is `00431102` |
| `007EF200` `lea ebx,[esi+120]` is this type | **DISPROVEN** | `esi = [[0x8F+12]]` is the **nested** object (sibling: `EXPRESSION` `0x90`) |

Sibling `ctcexpression-plus120-writers`:
`CTCExpression` itself is **20** bytes.
Offset `+120` is not on it either.

---

## 4. Relation to `007EF200` / `CTCExpression`

`007EF200` is vtbl **`0x0124026C` slot 28**
(sibling `007EEF60-activate`). `this` is
the 20-byte component from factory
`004DC7E8` / ctor `004DB085`. It looks
up Thing component **`0x8F`**, then:

```
007EF303  mov esi, [ebp+12]
007EF30E  test [esi+116]
          jne camera 0041649C
007EF36B  lea ebx, [esi+120]     ; CString vs 0x122D70E
007EF3A1  call 004B4A10
```

Type id `0x8F` is `CExpressionDef` slot 21
`004DB06C`. That wrapper is **16** bytes
(factory `004DC78F` / ctor `004DB050`
vtbl `0x012401F4`). It also cannot own
`+120`. The nested `+12` object does.

### 4.1 Register order on `004EE23F`

Seven `004D2EF0` between pair 51 and
pair 52. Remaining-pairs counted them
unnamed (no in-range `push "…"`). Intern
helpers in this listing recover the
names (not invented):

| n | Helper | Listing string | factory `push` |
| --: | --- | --- | --- |
| 1 | `004D4A86` | `CTCBerserk` | `0x4D4A69` |
| 2 | `004D4AB9` | `CTCDivineWrath` | `0x4D4A99` |
| 3 | `004D4AEC` | `CTCUnholyPower` | `0x4D4ACC` |
| 4 | `004D4AFF` | `CTCHaste` | `0x4E2B4B` |
| 5 | `004D4B2F` | `CTCForcePushed` | `0x4D4B12` |
| 6 | `004D4B5F` | `CTCRegionDisplay` | `0x4D4B42` |
| 7 | `004D4B72` | **`CTCExpression`** | **`0x4DC78F`** |

Then pair 52. Then eight CTC before
pair 53:

| n | Helper | Listing string | factory |
| --: | --- | --- | --- |
| 1 | `004D4B9B` | **`CTCCarriedActionUseExpression`** | **`0x4DC7E8`** |
| 2 | `004D58B3` | `CTCMultiStrike` | `0x4D5896` |
| 3 | `004D5883` | `CTCMultiArrow` | `0x4D5866` |
| 4 | `004D561D` | `CTCDrainLife` | `0x4D5600` |
| 5 | `004D564D` | `CTCHealLife` | `0x4D5630` |
| 6 | `004D567D` | `CTCGhostSword` | `0x4D5660` |
| 7 | `004D5D70` | `CTCEffectOnDie` | `0x4D5D53` |
| 8 | `004D647A` | **`CTCWillResponse`** | **`0x4D645D`** |

**PROVEN** sandwich:

```
CTCExpression name + CExpressionDef factory 004DC78F   // 16-byte 0x8F
CExpressionSubDef                         pair 52
CTCCarriedActionUseExpression + 004DC7E8             // 20-byte, 007EF200
… six will-spell CTC …
CTCWillResponse
CWillResponseDef                          pair 53
```

Do **not** collapse the two factories.

| Factory | Size | Ctor / vtbl | Slot 22 intern | RTTI |
| --- | --: | --- | --- | --- |
| `004DC78F` | 16 | `004DB050` `0x012401F4` | `004DB072` → `004D4B72` **`CTCExpression`** | `CExpressionDef` `0x01376DCC` |
| `004DC7E8` | 20 | `004DB085` `0x0124026C` | `004DB09D` → `004D4B9B` **`CTCCarriedActionUseExpression`** | siblings / `rtti.txt` `CTCExpression` `0x0137A424` |

Slot 22 of the tick object is
**`CTCCarriedActionUseExpression`**, not
the `CTCExpression` string. Sibling
`007EF200-first-plus120` attributed
`004F4A50` `004D2EF0(0x4DC7E8)` to
`"CTCExpression"`. The in-range helper
at that site is `004D4B9B`. The
`"CTCExpression"` helper is the **previous**
CTC row (`004F4988` / `0x4DC78F`).
Name pairing **PROVEN** from this listing;
RTTI-vs-register-name **PARTIAL**.

### 4.2 `007EEFE0` uses `CExpressionSubDef+40` as an id

`CExpressionDef` vtbl `0x012401F4` slot 4
`007EEFE0` (`e8.tsv` dest `005DA240` at
`007EF00A`):

```
007EEFEB  cmp [ebp+12], 0
          jne already-set
          mov esi, [[ebp+4]+112]     ; owner+112
007EF00A  call 005DA240              ; lookup "CExpressionSubDef"
          cmp [edi+40], 0
          jle skip                   ; signed <= 0
          push &[ebp+12]
          push [edi+40]
          call 006869D0              ; ecx=[this+4] → 004C7990 getter
          call 005F81BE              ; 009AD9E0(id) → dest +12
```

`005DA240` intern `"CExpressionSubDef"`,
`[this.vtbl+56]`, then `009ADA10`.
**PROVEN** type-name lookup.

`005F81BE`: `arg1 <= 0` fail; else
`009AD9E0(id)` (generic id→object,
sibling `appearance-0042B0A2-first`)
and refcount-store into `*[arg2]`.

So `CExpressionSubDef+40` is a **positive
integer key**, not a quest intern, not
offset `+120`. One writer of the nested
pointer `007EF200` later reads.

The other writer is persist `007EF070`
(`"ExpressionDef"` `004109A0` →
`006869D0` / `00593666` into `+12`).
That path does **not** touch
`CExpressionSubDef`.

Whether every live `009AD9E0([+40])`
returns an `EXPRESSION*` (`0x90`) is
**PARTIAL**: `007EF200` treats
`[0x8F+12]` as that layout; this path
is one filler of that slot.

`game.bin` counts **MATCH** as parallel
banks, not nested subdefs: **39**
`CExpressionSubDef` and **39**
`EXPRESSION`; `EXPRESSION` `subdefs`
column is **0**.

### 4.3 Pair 53 is the will-response CDef

`007E3AD0` intern `"CWillResponseDef"`.
`007E47D0` is the same lookup shape as
`005DA240` (type name → `[eax+56]` →
`009ADA10`). `functions.tsv` tags
`CWillResponseDef|CTCWillResponse`.
Neighbor CTC is `"CTCWillResponse"`.
Eight bools, not a quest intern.
**PROVEN** pairing; live tick **UNREAD**.

---

## 5. Not `CActivateQuestDef`

| | `CExpressionSubDef` | `CActivateQuestDef` |
| --- | --- | --- |
| Remaining-pairs n | **52** | **62** `004F5B7D` |
| Factory | `004D8818` size **44** | `004D8A32` size **48** |
| Vtbl | `0x0123C2E4` | `0x0123C7F4` |
| Persist | `004DE881` `+40` `00431102` u32 | `007B5740` `+40` intern + `+44` bool |
| `game.bin` | 39 × 11 | 6 × 16 |
| Action | `007EEFE0` id lookup | `00843FC0` → `004B4A10([this+168])` |

Same `0044C0C0` base only. `CActivateQuestDef`
ctor `004D5056` `or [esi+40], -1` then
intern persist — the helper family of
`EXPRESSION+120`, **not** pair 52.

Sibling `cactivatequestdef-payloads`:
payload names **UNREAD** as hex; ASCII /
`names.bin` **not** `Q_NewOakValeIntro`.
Do not invent that activate from pair 52
or pair 62.

---

## 6. `game.bin` / names.bin

`names.tsv`:

| Offset | Name |
| --- | --- |
| `0x00006F36` | `NULLDEF_CExpressionSubDef` |
| `0x00006F54` | `CExpressionSubDef` |
| `0x00006F6A` | `NULLDEF_CWillResponseDef` |
| `0x00006F87` | `CWillResponseDef` |
| `0x00006F9C` | `NULLDEF_CTurncoatDef` |

**No** `Q_NewOakValeIntro`. `INDEX.md`
counts 39 / 34 / (next) 62 `CTurncoatDef`.

Inflated u32 at `CExpressionSubDef+40`
inside the 11-byte bodies: **UNREAD**
(dump length only). Whatever those ids
are, they are not a `+120` CString and
cannot be the Oakvale intern
`0x012C5D14` as a persist-name offset
(`names.bin` has no that string).

---

## 7. Host leftover

`AddFirstDefClass` Notes through
`FortyFourthDefClassName`
`CBoastingPodiumDef` `004F3630`
(remaining-pairs n=44).

No `CExpressionSubDef`. No
`CWillResponseDef`. No factory
`0x4D8818` / `0x4D9629`. Pairs 45–111
including 52–53 stay **LEFTOVER**.

Note-only even for 52–53 would still
omit live `009AD6E0` / `009FC4F0` on
each object (**not** MATCH).

This walk is type register, not a Thing,
not first Present, not `004B4A10`.
**DISPROVEN** as Oakvale.

---

## Original (no-save)

```
004EE23F  Init Thing Components
  004F46E5  pair 51 CAnimatingObjectDef
  7 CTC … CTCExpression + 004DC78F
  004F4A16 / 004F4A1D  pair 52 CExpressionSubDef   // leftover
  CTCCarriedActionUseExpression + 004DC7E8         // 007EF200 vtbl
  … CTCWillResponse
  004F4DB9 / 004F4DC0  pair 53 CWillResponseDef    // leftover
  CTCFireballSpell / CTCTurncoatSpell / CTCTurncoat
  004F4F45 / 004F4F4C  pair 54 CTurncoatDef        // next
0044C72B  Compile EXPRESSION+120                   // sibling; not these pairs
004A0D90  AddQuest FALSE Q_NewOakValeIntro
007EF200  needs live 0x8F + nested +120            // not these CDefs
```

`xrefs.tsv` `0x012C5D14`: bind + Gameflow
wait only. **0** hits in `004D8818` /
`004DE881` / `004F49EE` / `007EEFE0`.

---

## Host

No `src/` change. No
`ActivateQuest("Q_NewOakValeIntro")`.
No `007EF200` Oakvale Note.
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`
stays **MATCH** skip.

Do **not**:

- treat `CExpressionSubDef+40` as
  `EXPRESSION+120` / `QuestName`
- collapse pair 52 with pair 62
- collapse `004DC78F` with `004DC7E8`
- invent a first `+120` quest intern
  from these register sites

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004F49EE` / `004F4A16` / `004F4A1D` | pair 52 Add Def Class | **PROVEN** leftover |
| `004D8818` / `0x0123C2E4` / size 44 | factory / vtbl / size | **PROVEN** |
| `004DE881` `+40` `00431102` | persist u32 | **PROVEN** |
| `CExpressionSubDef+120` quest name | — | **DISPROVEN** |
| `007EEFE0` / `005DA240` / `005F81BE` | `+40` id → `[CExpressionDef+12]` | **PROVEN** path |
| `007EF200` `0x0124026C` slot 28 | tick nested `+120` | **PROVEN** (sibling); **DISPROVEN** as this CDef |
| `004F4988` `004D4B72` + `0x4DC78F` | CTC row `"CTCExpression"` | **PROVEN** |
| `004F4A3E` `004D4B9B` + `0x4DC7E8` | CTC row `"CTCCarriedActionUseExpression"` | **PROVEN** |
| `004F4D91` / `004F4DB9` / `004F4DC0` | pair 53 Add Def Class | **PROVEN** leftover |
| `004D9629` / `0x0123E324` / size 45 | factory / vtbl / size | **PROVEN** |
| `004DF562` `+37…+44` | eight u8 | **PROVEN** |
| `004F4CBA` `004D647A` + `0x4D645D` | CTC row `"CTCWillResponse"` | **PROVEN** |
| `004D8A32` / pair 62 | `CActivateQuestDef` | **DISPROVEN** as 52–53 |
| `004F4F1D` / `004F4F45` / `004F4F4C` | pair 54 `CTurncoatDef` | **PROVEN** sites |
| `00DBDE40` | Oakvale | **DISPROVEN** here |

---

## Gap / next

| Item | Class |
| --- | --- |
| Inflated `CExpressionSubDef+40` u32 ×39 | **UNREAD** hex |
| Lionhead name of that `00431102` field | **UNREAD** |
| `009AD9E0([+40])` always `EXPRESSION*` | **PARTIAL** |
| First live `007EF200` with non-empty nested `+120` | **UNREAD**; **DISPROVEN** as no-save Lookout / these pairs |
| `CWillResponseDef` eight bools meaning | **UNREAD** |
| `CTurncoatDef` ctor / vtbl / persist | **UNREAD** |

**Next unread site:** pair 54
`CTurncoatDef` `004F4F45` / `004F4F4C`
factory `004E0F9C` `00BFEA1A(84)` then
`jmp 004DEBA3`. Three CTC before it:
`CTCFireballSpell` / `CTCTurncoatSpell`
/ `CTCTurncoat`. Still leftover. Still
not Oakvale. Still must not invent
`ActivateQuest`.
