# Gameflow card-bind: `00CE7957` `vtbl+1180` dest

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest`.
Do **not** collapse this call onto pair-42
`CQuestCardDef` vtbl `01241E44`.
Do **not** treat `00896A30` / `004B0C80`
as the dest of this site.

Question: what is the exact vtbl offset
and dest of the Gameflow card-bind call?
Is `EngineLifecycle.QuestCardBindVtbl=1180`
**MATCH** or **off-by-4** vs `1184`?

Status words: **PROVEN** / **INFERRED** /
**UNKNOWN** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Authority: `Fable.exe`
`assembly/exe/01-sections/text-map/listing-00cc0000.txt`
(`00CE7670` / `00CE77D7` / `00CE790F` /
`00CE7957` / `00CE7977` / `00CED998`);
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00cc0000.txt`
(same VAs; no raw bytes);
`assembly/exe/01-sections/text-map/listing-00880000.txt`
(`008968C0` / `00896A30`);
`assembly/exe/01-sections/text-map/ff.tsv`
`0x00CE7957`;
`assembly/exe/00-index/vtbl.tsv`
`0x01260F0C` slots 25 / 121 / 288 / 295 /
296;
`src/Fable.Game/EngineLifecycle.cs`
(`QuestCardBindVtbl`, `QuestCardBindFn`,
`ScriptManagerVtbl`) read only;
siblings `proofs/004F34C4-quest-card`
(pair-42 **DISPROVEN** as this vtbl),
`proofs/gameflow-state0-wait`,
`proofs/gameflow-type33-give`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Call site? | `00CE7957` in `00CE7670` state 0 (`00CE77D7`) | **PROVEN** |
| Bytes / offset? | `FF 92 9C 04 00 00` = `call [edx+1180]` | **PROVEN** |
| Receiver? | `[esi+64]` Init Scripts iface `01260F0C` | **PROVEN** |
| Dest of `+1180`? | slot 295 **`008968C0`** | **PROVEN** |
| Where is `00896A30`? | slot 296 **`+1184`** | **PROVEN** |
| `QuestCardBindVtbl=1180` vs this call? | **MATCH** | **MATCH** |
| `QuestCardBindFn=00896A30` as dest of this call? | **off-by-4** (that dest is `+1184`) | **DISPROVEN** pairing |
| Is this `CQuestCardDef` `01241E44`? | **No.** That table ends at slot 24 (`+96`). | **DISPROVEN** |
| Invent `ActivateQuest` here? | **No.** Card presenter + later wait. | **DISPROVEN** |

**Exact call: `[esi+64].vtbl+1180` → `008968C0`.
Host offset 1180 MATCHES. Host dest `00896A30`
is the `+1184` sibling — off-by-4.**

---

## Verdict

`00CE7670` state 0 interns
`"OBJECT_QUEST_CARD_OAKVALE_INTRO"` and
`"Q_NewOakValeIntro"`, then
`call [edx+1180]` on `[esi+64]`.
`ff.tsv` and the listing bytes are
**1180 decimal**. That iface is
`01260F0C` (same receiver as the
proven wait `vtbl+100` `00893570`
immediately after).

`vtbl.tsv` `0x01260F0C`:

| Slot | Off | Dest |
| ---: | ---: | --- |
| 25 | +100 | `00893570` wait (**MATCH**) |
| 121 | +484 | `008902E0` tattoo (**MATCH**) |
| 288 | +1152 | `00892F80` Give (**MATCH**) |
| **295** | **+1180** | **`008968C0`** |
| 296 | +1184 | `00896A30` |

Slot formula `off = slot×4` is locked by
the three **MATCH** rows. Dest of the
Gameflow call is therefore **`008968C0`**.

Host:

```
public const uint QuestCardBindFn = 0x00896A30;
public const int QuestCardBindVtbl = 1180;
```

`QuestCardBindVtbl=1180` **MATCH**es the
call offset. `QuestCardBindFn=00896A30`
is slot 296 (`+1184`). Pairing that dest
with this call is **off-by-4**. Arity
agrees: the site pushes **3** stack args
and `008968C0` is `ret 12`; `00896A30`
is `ret 16` (4 args + `004B0D30` /
`004AF610` / `004B0C80`). Calling
`00896A30` at this site would unbalance
the stack.

Pair-42 `CQuestCardDef` `01241E44` is
**DISPROVEN** as this slot (25 slots,
`+0`…`+96`). Do **not** invent
`ActivateQuest`.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `listing-00cc0000` `00CE7957` `FF 92 9C 04 00 00` | `call [edx+1180]` | `QuestCardBindVtbl=1180` | **MATCH** offset |
| `ff.tsv` `0x00CE7957` disp `1180` parent `00CE7650` | same | same | **MATCH** |
| `00CE7941` `mov ecx, [esi+64]` / `mov edx, [ecx]` | Init Scripts iface | `ScriptManagerVtbl=01260F0C` | **MATCH** receiver |
| Same `[esi+64]` at `00CE7988` `call [edx+100]` | slot 25 `00893570` | `QuestIsActiveVtbl=100` | **MATCH**; locks the table |
| `vtbl.tsv` `01260F0C` slot 295 | `008968C0` | `QuestCardBindFn=00896A30` | **off-by-4** dest |
| `vtbl.tsv` slot 296 | `00896A30` at **+1184** | Note `"00896A30 vtbl+1180 … 004B0C80 miss"` | **DISPROVEN** dest on this call |
| `008968C0` `ret 12`; 3 pushes (`card`, `quest`, `ebp=0`) | arity **MATCH** | dest assumed `00896A30` | **DISPROVEN** arity |
| `00896A30` `ret 16`; first `E8` `004B0D30` | sibling slot | host dest identity | **PROVEN** as `+1184` only |
| `01241E44` slots 0–24 | pair-42 registrar | not this call | **DISPROVEN** |

---

## 1. Call site (`00CE7957`)

`00CE7670` (`listing-00cc0000.txt`):
`xor ebp, ebp` then `mov esi, ecx`.
State 0 is `00CE77D7`. After tattoos
and `00CBE87F(10)`:

```
00CE790F  8B 4E 40                  mov ecx, [esi+64]
00CE7912  8B 01                     mov eax, [ecx]
00CE7914  55                        push ebp
00CE7915  FF 90 F4 05 00 00         call [eax+1524]
00CE791B  6A FF                     push -1
00CE791D  68 14 5D 2C 01            push "Q_NewOakValeIntro"
00CE7922  8D 8C 24 2C 06 00 00      lea ecx, [esp+1580]
00CE7929  E8 C2 72 CB FF            call 0099EBF0
00CE792E  6A FF                     push -1
00CE7930  68 F4 5C 2C 01            push "OBJECT_QUEST_CARD_OAKVALE_INTRO"
00CE7935  8D 8C 24 DC 00 00 00      lea ecx, [esp+220]
00CE793C  E8 AF 72 CB FF            call 0099EBF0
00CE7941  8B 4E 40                  mov ecx, [esi+64]
00CE7944  8B 11                     mov edx, [ecx]
00CE7946  55                        push ebp
00CE7947  8D 84 24 28 06 00 00      lea eax, [esp+1576]
00CE794E  50                        push eax
00CE794F  8D 84 24 DC 00 00 00      lea eax, [esp+220]
00CE7956  50                        push eax
00CE7957  FF 92 9C 04 00 00         call [edx+1180]
```

`9C 04 00 00` = **1180**.
`ff.tsv`: `0x00CE7957  call  [edx+1180]  1180`.

Three stack args, stdcall:

1. card CString `OBJECT_QUEST_CARD_OAKVALE_INTRO`
2. quest CString `Q_NewOakValeIntro`
3. `ebp` (**0**)

`ecx` = `[esi+64]`. Next is the wait
`00CE7977` `push "Q_NewOakValeIntro"`
`call [edx+100]`. Same iface.
**PROVEN**.

Later Gameflow `00CED998` is the same
shape (`Q_FireHeart` /
`OBJECT_QUEST_CARD_FIRE_HEART`,
`push ebp`, `call [eax+1180]`). Same
slot, same dest. Sibling notes that
write `00896A30` there inherit the
same **off-by-4**.

---

## 2. Iface is `01260F0C`, not `01241E44`

`[esi+64]` is the Init Scripts /
script-manager iface. Host
`ScriptManagerVtbl=0x01260F0C`
(`006E7740`). The very next call on
this pointer is `vtbl+100` slot 25
`00893570`, already **PROVEN** in
`proofs/gameflow-state0-wait`.

`vtbl.tsv` `0x01241E44` (pair 42
`CQuestCardDef`) has slots **0–24**
only (`+0`…`+96`). There is no
`+1180` on that table. Pair-42 as
this bind is **DISPROVEN**
(`proofs/004F34C4-quest-card`).

---

## 3. `vtbl.tsv` dest: `008968C0` at +1180

`assembly/exe/00-index/vtbl.tsv`
`0x01260F0C`:

```
0x01260F0C	25	0x00893570
0x01260F0C	121	0x008902E0
0x01260F0C	288	0x00892F80
0x01260F0C	295	0x008968C0
0x01260F0C	296	0x00896A30
```

| Slot | ×4 | Listing call | Host constant | Class |
| ---: | ---: | --- | --- | --- |
| 25 | 100 | `00CE7995` `[edx+100]` | `QuestIsActiveVtbl=100` `00893570` | **MATCH** |
| 121 | 484 | `00CE7887` `[eax+484]` | `GiveNamedObjectFn=008902E0` | **MATCH** |
| 288 | 1152 | Give presenters | `QuestGiveVtbl=1152` `00892F80` | **MATCH** |
| 295 | **1180** | `00CE7957` `[edx+1180]` | dest claimed `00896A30` | dest **off-by-4** |
| 296 | **1184** | not this Gameflow site | `QuestCardBindFn=00896A30` | **PROVEN** slot of that VA |

`VtblDest(0x01260F0C, 295) = 0x008968C0`.
`VtblDest(0x01260F0C, 296) = 0x00896A30`.
**PROVEN**.

---

## 4. Arity splits the two dests

`listing-00880000.txt` `008968C0`
(assembly bytes):

```
008968C0  51                        push ecx
008968C1  53                        push ebx
008968C2  8B D9                     mov ebx, ecx
008968C4  8B 4B 14                  mov ecx, [ebx+20]
…
008968F7  E8 64 8C C1 FF            call 004AF560
…
008969D4  … "HUD_ORB_QUEST_CORE"
008969DF  … "TEXT_QST_078_GM_MSG_NEW_QUEST_CARD"
008969FF  call [edx+1096]
…
00896A1B  C2 0C 00                  ret 12
```

`ret 12` = 3 dwords. First `E8` after
the hero lookup is `004AF560` (`QM+92`
walk), **not** `004AF610`. HUD strings
are on this dest. **PROVEN**.

`00896A30`:

```
00896A30  8B 44 24 10               mov eax, [esp+16]
00896A34  8B 54 24 08               mov edx, [esp+8]
…
00896A4E  E8 DD A2 C1 FF            call 004B0D30
00896A62  E8 A9 8B C1 FF            call 004AF610
00896A76  call 004B0C80
…
00896B26  push "LOG_ENTRY"
…
00896B53  C2 10 00                  ret 16
```

`ret 16` = 4 dwords. Needs
`004AF610` already active, then
`004B0C80` card find. Host Note
`"00896A30 vtbl+1180 … 004B0C80 miss"`
describes **this** sibling, not
`00CE7957`. **DISPROVEN** as dest of
the Gameflow call.

A 3-push `call [edx+1180]` cannot
target a `ret 16` thunk. **PROVEN**.

---

## 5. Host constants

`EngineLifecycle.cs`:

```
/// Gameflow state 0 binds the Oakvale
/// card via vtbl+1180 then waits.
public const uint QuestCardBindFn = 0x00896A30;
public const int QuestCardBindVtbl = 1180;
```

`TickGameflowMain` Note:

```
"00896A30 vtbl+1180 OBJECT_QUEST_CARD_OAKVALE_INTRO 004B0C80 miss"
```

| Constant | Native | Class |
|---|---|---|
| `QuestCardBindVtbl=1180` | `00CE7957` disp | **MATCH** |
| `QuestCardBindFn=00896A30` | dest of **+1184** | **off-by-4** vs this call |
| Note `004B0C80` | body of `00896A30` | **LEFTOVER** on this site |

Offset is correct. Dest identity is
the next slot. No `src/` change in
this proof.

---

## Classifications

| Claim | Class |
|---|---|
| `00CE7957` is `call [edx+1180]` | **PROVEN** |
| Receiver `[esi+64]` = `01260F0C` | **PROVEN** |
| Dest = slot 295 `008968C0` | **PROVEN** |
| `QuestCardBindVtbl=1180` | **MATCH** |
| `00896A30` at `+1184` | **PROVEN** |
| Host dest `00896A30` on this call | **off-by-4** **DISPROVEN** |
| Pair-42 `01241E44` is this slot | **DISPROVEN** |
| This call is `ActivateQuest` | **DISPROVEN** |
| `00CE7670` constructs Oakvale | **DISPROVEN** |

---

## INDEX

| VA / name | Role |
|---|---|
| `00CE7670` / `00CE77D7` | Gameflow Main / state 0 |
| `00CE7957` | `FF 92 9C 04 00 00` card call |
| `01260F0C` | `[esi+64]` iface |
| `008968C0` | dest of `+1180` (slot 295) |
| `00896A30` | dest of `+1184` (slot 296) |
| `004AF560` | first QM helper in `008968C0` |
| `004B0D30` / `004AF610` / `004B0C80` | `00896A30` chain; **not** this call |
| `01241E44` | `CQuestCardDef`; **not** this iface |
| `QuestCardBindVtbl=1180` | host offset; **MATCH** |
| `QuestCardBindFn=00896A30` | host dest; **off-by-4** |
