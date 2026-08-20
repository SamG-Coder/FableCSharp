# `004EE23F` pairs 42–43: `CQuestCardDef` / `CFlammableDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` → `"Init Game"`
`0042F491` → `00418DCA` → `[vtbl+4]`
`004184BD` → `00418585` `004EE23F`.
Do **not** invent `ActivateQuest`.
Do **not** invent CTC names from `004Dxxxx`
helpers as Add Def Class pairs.

Status words: **PROVEN** / **INFERRED** /
**UNKNOWN** / **DISPROVEN**. Extra:
**LEFTOVER** / **MATCH**.

Question: recover remaining-pairs **42**
`CQuestCardDef` `004F349C` factory `0x4E2333`
sites `004F34C4` / `004F34CB` and **43**
`CFlammableDef` `004F3552` factory `0x4E3DC3`
sites `004F357A` / `004F3581`. Confirm
sites, sizes, vtbls, persist ctors. Does
`CQuestCardDef` construct `Q_NewOakValeIntro`
on no-save? Is
`EngineLifecycle.QuestCardBindVtbl=1180`
this class?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
(`004F3310`…`004F3637`; factories
`004E2333` / `004E00BC` / `004E3DC3` /
`004E284B` — there is **no**
`listing-004e0000.txt`; `004E*` lives in
the `004c` map);
`listing-00440000.txt` `0044C0C0`;
`listing-00cc0000.txt` `00CE791D` /
`00CE7957`;
`listing-00880000.txt` `00896A30` /
`008968C0`;
`listing-00700000.txt` `007021C0` /
`00702930`;
`assembly/exe/01-sections/text-map/ff.tsv`
`0x00CE7957`;
`tools/Fable.ExeIndex/out/00-index/strings.tsv`
`0x01243FAC` / `0x01243F9C` / `0x01243F88`;
`assembly/exe/00-index/vtbl.tsv`
`0x01241E44` / `0x01242984` / `0x01260F0C`;
`assembly/exe/00-index/xrefs.tsv`;
`rtti.txt` `0x0137ACE4` / `0x0137B098`;
`assembly/compiled-defs/game/entries.tsv`
/ `INDEX.md` / `names.tsv`;
`proofs/004EE23F-remaining-pairs` rows
41–44;
`proofs/004F3338-hero-centre`;
`proofs/gameflow-type33-give`;
`src/Fable.Game/EngineLifecycle.cs`
(`FortySecondDefClass*`,
`FortyThirdDefClass*`,
`QuestCardBindVtbl`, `QuestCardBindFn`)
read only.

---

## Verdict

| Field | Pair 42 | Pair 43 | Class |
| --- | --- | --- | --- |
| listing string | `CQuestCardDef` `004F349C` | `CFlammableDef` `004F3552` | **PROVEN** |
| `0044C6B0` | `004F34C4` | `004F357A` | **PROVEN** |
| `009B0AC0` | `004F34CB` | `004F3581` | **PROVEN** |
| Factory | `004E2333` `00BFEA1A(116)` then `jmp 004E00BC` | `004E3DC3` `00BFEA1A(76)` then `jmp 004E284B` | **PROVEN** |
| Persist ctor | `004E00BC` `0044C0C0`; `[esi]=01241E44`; `[esi+56]=-1`; `[esi+60]=-1`; `[esi+76..84]=0`; `004DF7D3` at `+92` | `004E284B` `0044C0C0`; `[esi]=01242984`; `004E0A05` at `+56` | **PROVEN** |
| Size | **116** (`push 116`; vtbl[20] `004E00E9`) | **76** (`push 76`; vtbl[20] `004E2865`) | **PROVEN** |
| Vtbl | **`01241E44`** (25 slots, 0–24) | **`01242984`** (25 slots, 0–24) | **PROVEN** |
| CTC between previous and this | **3** unnamed (`0x4D4640` / `0x4D4670` / `0x4D2E44`) | **1** unnamed (`0x4D46A0`) | **PROVEN** count; in-range names **UNKNOWN** |
| Shape | 2 (`push` + `0042DAE0`) | 2 | **PROVEN** |

| Question | Answer | Class |
| --- | --- | --- |
| Sites / factory / size / vtbl / persist ctor? | As the table. Listing strings not invented. | **PROVEN** |
| Does `CQuestCardDef` construct `Q_NewOakValeIntro` on no-save? | **No.** Intern is Add Def Class only. Factory / persist ctor never push that name, never `00CB5AD0` / `004B3CE0` / `004B4A10` / `00892E80`. Game.bin rows are `NULLDEF_CQuestCardDef` / unnamed `CQuestCardDef`, not that quest. `OBJECT_QUEST_CARD_OAKVALE_INTRO` is type `OBJECT`. Host `Runtime.Quests` omits the name. Do **not** invent `ActivateQuest`. | **DISPROVEN** |
| Is `QuestCardBindVtbl=1180` this class? | **No.** `01241E44` ends at slot 24 (`+96`). `00CE7957` `FF 92 9C 04 00 00` is `[esi+64]` Init Scripts iface `01260F0C`. | **DISPROVEN** |
| Host live 116- / 76-byte objects? | **None.** `AddFirstDefClass` Notes the intern + flags. Not a live object. | **PROVEN** leftover intern; **MATCH** sites |

**Answer:** pair 42 is `CQuestCardDef`
`004F34C4` / `004F34CB` factory `004E2333`
size 116 vtbl `01241E44` persist ctor
`004E00BC` (`[+56]/[+60]=-1`). Pair 43 is
`CFlammableDef` `004F357A` / `004F3581`
factory `004E3DC3` size 76 vtbl
`01242984` persist ctor `004E284B`.
Registrar only. Not Oakvale construct.
`QuestCardBindVtbl=1180` is the Gameflow
script-iface slot, not this vtbl. Next
pair is `CBoastingPodiumDef` `004F3608` /
`004F3630` / `004F3637` factory
`0x4D8736`.

---

## 1. Pair 42 — `CQuestCardDef`

`listing-004c0000.txt` after forty-first
`CHeroCentreDef` `004F333F`. Three unnamed
`004D2EF0` rows (`push 0x4D4640` at
`004F3372`, `0x4D4670` at `004F33DD`,
`0x4D2E44` at `004F3448`). Then:

```
004F349C  push "CQuestCardDef"
004F34A1  lea ecx, [ebp-1616]
004F34A7  call 0099EBF0
004F34AC  push 0x4E2333
004F34B1  lea eax, [ebp-1616]
004F34B7  push eax
004F34B8  lea ecx, [ebp-2436]
004F34BE  call 0042DAE0
004F34C3  push eax
004F34C4  call 0044C6B0
004F34C9  mov ecx, eax
004F34CB  call 009B0AC0
```

`004F349C` `push 0x01243FAC`.
`strings.tsv`:

```
0x01243FAC	0xE43FAC	CQuestCardDef
```

Same listing annotates the immediate as
`"CQuestCardDef"`. Not invented.
`xrefs.tsv` `0x01243FAC` first hit
`0x004F349D`. Shape-2 (`push` +
`0042DAE0`). Matches remaining-pairs
row 42. `rtti.txt` `0x0137ACE4`
`CQuestCardDef`.

`004E2333` / persist ctor `004E00BC`
(same `004c` listing):

```
004E2333  push 116
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E2346
          mov ecx, eax
          jmp 004E00BC
004E2346  xor eax, eax
          ret

004E00BC  push esi
          mov esi, ecx
          call 0044C0C0
          mov [esi], 0x1241E44
          or [esi+56], -1
          or [esi+60], -1
          xor eax, eax
          mov [esi+76], eax
          mov [esi+80], eax
          lea ecx, [esi+92]
          mov [esi+84], eax
          call 004DF7D3
          mov eax, esi
          pop esi
          ret

004E00E9  push 116
          pop eax
          ret
```

Thunk shape: alloc **116**, null →
`xor eax, eax; ret`, else
`mov ecx, eax; jmp` persist ctor.
Base `0044C0C0` (`listing-00440000.txt`:
`009FBEC0`, `[esi+36]&=0xF8`,
`[esi+28]=0`, temp vtbl `01231D54`).
Then overwrite vtbl `01241E44`. Extra
stores: `+56` / `+60` = `-1`;
`+76` / `+80` / `+84` = `0`; nested
`004DF7D3` at `+92`. Object is 116
bytes (`00BFEA1A(116)` plus size
helper immediately after the ctor).

`004DF7D3` (`listing-004c0000.txt`)
pushes two stack bytes and calls
`004DD0EE`. Nested record starts at
`+92`. Copy `004E4294` (below) shows
it runs through `+104`.

`vtbl.tsv` `0x01241E44`:

| Slot | Dest | Role |
| ---: | --- | --- |
| 0 | `004E2349` | dtor (`004E2365` then `009FC550` / `00BFE9BC`) |
| 1–17 / 21–24 | `0042D930`…`0042DAA0` / `009ACE90` / `009FBEF0` / `009ACAB0` / `009ACB20` | shared family |
| 18 | `004E795A` | LoadDef persist (not intern) |
| 19 | `004E4294` | copy |
| 20 | `004E00E9` | size `push 116` |

**25 slots.** Last offset `+96`. No
slot at `+1180`.

Slot 18 `004E795A` is later LoadDef
serialize, **not** this intern:

| Off | Helper | Ctor seed |
| ---: | --- | --- |
| `+40` `+44` `+48` `+52` | `00431102` CString | **UNKNOWN** intern (ctor does not store) |
| `+56` `+60` | `0045228F` dword | **`-1`** |
| `+64` `+68` `+72` | `00431102` CString | **UNKNOWN** intern |
| `+76` | `00466A47` (copy uses `00454886` to `+88`) | **0** at `+76`/`+80`/`+84` |
| `+88`…`+91` | `0043314A` bytes | **UNKNOWN** intern |
| `+92` | `004E7A2D` nested (`004DF7D3`) | nested ctor |
| `+104` | `00431102` CString | **UNKNOWN** intern |
| `+108` | `0043314A` byte | **UNKNOWN** intern |
| `+112` | `00431020` | **UNKNOWN** intern |

Copy `004E4294` dword-copies
`+40`…`+72`, then `+76` block, four
bytes `+88`…`+91`, nested `+92`,
`+104`, `+108`, `+112`. Matches size
116. Field **names** on this Def are
**UNKNOWN** here. Do not steal
`00702720` Thing offsets
(`Finished` / `QuestName` / …) onto
this layout.

Later leftover (not this register):
`007021C0` type-name (`push -1` /
`"CQuestCardDef"` / `0099EBF0`);
`00702930` typed HANDLE get
(`[vtbl+56]` → `009ADA10`).
`xrefs.tsv` those two plus the intern.

`game.bin`: **101** `CQuestCardDef`
rows (`INDEX.md`). Id **41** =
`NULLDEF_CQuestCardDef` raw **132**.
Other rows are type-name
`CQuestCardDef` only (raw 132–160).
**0** named `Q_NewOakValeIntro`.

---

## 2. Pair 43 — `CFlammableDef`

One unnamed `004D2EF0` after pair 42
(`push 0x4D46A0` at `004F34FE`). Then:

```
004F3552  push "CFlammableDef"
004F3557  lea ecx, [ebp-1416]
004F355D  call 0099EBF0
004F3562  push 0x4E3DC3
004F3567  lea eax, [ebp-1416]
004F356D  push eax
004F356E  lea ecx, [ebp-2036]
004F3574  call 0042DAE0
004F3579  push eax
004F357A  call 0044C6B0
004F357F  mov ecx, eax
004F3581  call 009B0AC0
```

`004F3552` `push 0x01243F9C`.
`strings.tsv`:

```
0x01243F9C	0xE43F9C	CFlammableDef
```

Shape-2. Matches remaining-pairs row
43. `rtti.txt` `0x0137B098`
`CFlammableDef`. `xrefs.tsv` intern
`0x004F3553` plus later
`00780390` / `007815A0`.

```
004E3DC3  push 76
          call 00BFEA1A
          test eax, eax
          pop ecx
          je 004E3DD6
          mov ecx, eax
          jmp 004E284B
004E3DD6  xor eax, eax
          ret

004E284B  push esi
          mov esi, ecx
          call 0044C0C0
          lea ecx, [esi+56]
          mov [esi], 0x1242984
          call 004E0A05
          mov eax, esi
          pop esi
          ret

004E2865  push 76
          pop eax
          ret
```

Alloc **76**, jmp persist ctor.
Base `0044C0C0`. Vtbl `01242984`.
Nested `004E0A05` at `+56`
(`0099A310`, nested vtbl
`012421D4`, `004DF974` at nested
`+8`). Remainder 20 bytes
(76−56).

`vtbl.tsv` `0x01242984`:

| Slot | Dest | Role |
| ---: | --- | --- |
| 0 | `004E2869` | dtor |
| 1–17 / 21–24 | shared family | same as pair 42 |
| 18 | `004E599D` | LoadDef persist |
| 19 | `004E5834` | copy |
| 20 | `004E2865` | size `push 76` |

Slot 18 `004E599D`: `+40`
`00431102` CString; `+44` `+48`
`+52` `00431061`; then `add esi, 56`
`004E59E1` nested persist. Payload
at those extras is **UNKNOWN** on
this intern (ctor only runs
`004E0A05`).

`game.bin`: **4** `CFlammableDef`
rows. Id **42** =
`NULLDEF_CFlammableDef` raw **43**.
Not Oakvale.

---

## 3. `Q_NewOakValeIntro` is not this intern

`004E2333` / `004E00BC` / the
`004F349C` block: **0** pushes of
`Q_NewOakValeIntro`
(`0x012C5D14`) or
`OBJECT_QUEST_CARD_OAKVALE_INTRO`.
**0** `E8` to `00CB5AD0` /
`004B3CE0` / `004B4A10` /
`00892E80`. Factory constructs a
116-byte Def record (or null), not
a quest.

`names.tsv` / `entries.tsv`:
`OBJECT_QUEST_CARD_OAKVALE_INTRO`
is type **`OBJECT`** (id 3710),
not `CQuestCardDef`. Compiled
`CQuestCardDef` rows do not carry
that object name or the quest
intern.

No-save Gameflow wait
(`proofs/gameflow-type33-give`):
`00CE7670` state 0 binds the
Oakvale **card** then polls
Give-kind `0x33`. Nobody Gives
that name on this walk. Construct
posts `0x37`. Host test
`Init_Thing_Components_004F34CB_adds_CQuestCardDef`
asserts `Runtime.Quests` does
**not** contain
`Q_NewOakValeIntro`.

Inventing
`ActivateQuest("Q_NewOakValeIntro")`
from this pair is **DISPROVEN**.

---

## 4. `QuestCardBindVtbl=1180` is not `CQuestCardDef`

Host:

```
public const uint QuestCardBindFn = 0x00896A30;
public const int QuestCardBindVtbl = 1180;
```

`listing-00cc0000.txt` /
`ff.tsv` `0x00CE7957`:

```
00CE790F  mov ecx, [esi+64]
…
00CE791D  push "Q_NewOakValeIntro"
00CE7930  push "OBJECT_QUEST_CARD_OAKVALE_INTRO"
00CE7941  mov ecx, [esi+64]
00CE7944  mov edx, [ecx]
00CE7957  FF 92 9C 04 00 00    call [edx+1180]
```

Bytes `9C 04 00 00` = **1180
decimal**. Receiver is
`[esi+64]` (Init Scripts iface),
**not** a `CQuestCardDef` instance.

`vtbl.tsv` `0x01241E44` has slots
0–24 only (`+0`…`+96`). Treating
`+1180` as this class is
**DISPROVEN**.

Iface `0x01260F0C`
(`proofs/gameflow-state0-wait`):

| Slot | Off | Dest |
| ---: | ---: | --- |
| 25 | +100 | `00893570` wait |
| 295 | +1180 | `008968C0` |
| 296 | +1184 | `00896A30` |

Sibling Gameflow proofs lock dest
`00896A30` at `+1180`. `vtbl.tsv`
places `00896A30` at slot 296
(`+1184`) and `008968C0` at
`+1180`. Dest identity is a
**sibling** lock, **UNKNOWN** as
re-opened here. **Neither** dest
is on `01241E44`.

`00896A30` (`listing-00880000.txt`)
calls `004B0D30` then `004AF610`
(already active) then `004B0C80`
card find. Needs the quest
**already** constructed. Card
bind is **DISPROVEN** as this
Add Def Class pair.

---

## 5. CTC rows are not Add Def Class

Remaining-pairs method: only
in-range `push "…"` listing
strings are pair names. After
`CTCActionUseSearch` the walk
stops pushing CTC names;
`004D2EF0` rows stay unnamed.

Counts **PROVEN** from
`listing-004c0000.txt`:

| After | `004D2EF0` factory `push` | Count |
| --- | --- | ---: |
| 41 `CHeroCentreDef` | `0x4D4640` `004F3372`; `0x4D4670` `004F33DD`; `0x4D2E44` `004F3448` | 3 |
| 42 `CQuestCardDef` | `0x4D46A0` `004F34FE` | 1 |
| 43 `CFlammableDef` | `0x4E7E19` `004F35B4` | 1 |

Helpers (`004D465D` /
`004D468D` / `004D2E61` /
`004D46BD` / `004D46D0`) do
push CTC strings. Those are
**not** in-range pair names.
Do not promote them to rows
42–43. Names stay **UNKNOWN**
in the remaining-pairs table.

---

## 6. Host leftover

`AddFirstDefClass` Notes
forty-second / forty-third and
sets flags. No 116- / 76-byte
object. Intern sites **MATCH**.
Live ctor stores **LEFTOVER**.

| After 41st | Native | Host |
| --- | --- | --- |
| 3 unnamed `004D2EF0` | listing `004F335A`…`004F3496` | **LEFTOVER** |
| 42 `CQuestCardDef` `004F34C4` / `004E2333` size 116 vtbl `01241E44` | **PROVEN** | Note-only + flag |
| 1 unnamed `004D2EF0` (`0x4D46A0`) | listing `004F34E6`…`004F354C` | **LEFTOVER** |
| 43 `CFlammableDef` `004F357A` / `004E3DC3` size 76 vtbl `01242984` | **PROVEN** | Note-only + flag |

Not Oakvale. Not a Thing
instance. Not a file I/O site.
Not `ActivateQuest`.

---

## 7. Next — `CBoastingPodiumDef`

One unnamed `004D2EF0` after pair
43 (`push 0x4E7E19` at
`004F35B4`). Then remaining-pairs
row 44:

```
004F3608  push "CBoastingPodiumDef"
004F360D  lea ecx, [ebp-1552]
004F3613  call 0099EBF0
004F3618  push 0x4D8736
004F361D  lea eax, [ebp-1552]
004F3623  push eax
004F3624  lea ecx, [ebp-2308]
004F362A  call 0042DAE0
004F362F  push eax
004F3630  call 0044C6B0
004F3635  mov ecx, eax
004F3637  call 009B0AC0
```

`strings.tsv` `0x01243F88`
**`CBoastingPodiumDef`**. Shape-2.
**PROVEN** name / sites / factory
imm. Factory body out of scope
here.

---

## Original

Forty-second Add Def Class on
`004EE23F`:

1. `0099EBF0` name `"CQuestCardDef"`.
2. `0042DAE0` packs factory `004E2333`.
3. `0044C6B0` `004F34C4`.
4. `009B0AC0` `004F34CB`.

Factory alloc 116, persist ctor
`004E00BC`. Base `0044C0C0`. Vtbl
`01241E44`. `[+56]/[+60]=-1`.
`004DF7D3` at `+92`.

Forty-third:

1. `0099EBF0` name `"CFlammableDef"`.
2. `0042DAE0` packs factory `004E3DC3`.
3. `0044C6B0` `004F357A`.
4. `009B0AC0` `004F3581`.

Factory alloc 76, persist ctor
`004E284B`. Base `0044C0C0`. Vtbl
`01242984`. `004E0A05` at `+56`.

Does **not** construct
`Q_NewOakValeIntro`.
`QuestCardBindVtbl=1180` is
`[esi+64]` `01260F0C`, not this
class.

---

## INDEX

| VA / name | Role |
| --- | --- |
| `004EE23F` | Init Thing Components |
| `004F349C` / `004F34C4` / `004F34CB` | pair 42 intern / `0044C6B0` / `009B0AC0` |
| `004E2333` / `004E00BC` | pair 42 factory / persist ctor |
| `01241E44` | `CQuestCardDef` vtbl (25 slots) |
| `004E00E9` | size helper 116 |
| `004E795A` / `004E4294` | later persist / copy — **not** intern |
| `004F3552` / `004F357A` / `004F3581` | pair 43 intern / `0044C6B0` / `009B0AC0` |
| `004E3DC3` / `004E284B` | pair 43 factory / persist ctor |
| `01242984` | `CFlammableDef` vtbl (25 slots) |
| `004E2865` | size helper 76 |
| `004E0A05` | pair 43 nested at `+56` |
| `004F3608` / `004F3630` / `004F3637` | next pair `CBoastingPodiumDef` |
| `00CE7957` `[edx+1180]` | Gameflow card bind on `01260F0C` — **not** `01241E44` |
| `QuestCardBindVtbl=1180` | host constant for that iface slot |
| `00896A30` | sibling card-find dest; needs `004AF610` already active |
| `Q_NewOakValeIntro` | Gameflow wait name; **not** constructed here |
