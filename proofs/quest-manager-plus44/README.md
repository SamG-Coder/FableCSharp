# QuestManager `+44` vs `world+172` / `world+184`

Investigation only. No production `src/` edits.

Question: `004B2850` is called from `AddQuest` (`004A0D90`) —
`push_back` of the name onto the quest-manager vector at
`[0x13B89FC]+44`. Same list as `world+172`, `world+184`, or a
third list? Does `004B4260` walk this or `world+172`? Relation
to `004B2890`?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `listing-00480000.txt` `004B2850` / `004B2890` /
`004B4260` / `004A10C4` (and `004A0D90` / `004A08D0` / `0049F180` /
`004B00C0` / `004B4590` / `004B4A10`);
`listing-00400000.txt` `00433530`;
`listing-00880000.txt` `00892E80`;
`proofs/qst-first-load/README.md`;
`proofs/wld-parse/README.md`.

---

## Verdict

**`[0x13B89FC]+44` is a third list. PROVEN.**

It is not `CWorld+172` and not `CWorld+184`. It lives on the
QuestManager singleton. Same `AddQuest` name is copied into it
after the world stores, via a different `00433530` grow.

**`004B4260` walks the *argument* vector, not `+44`.** The
no-save Init Quests site is `0049F247` `lea edx, [esi+172]` —
that is `world+172` (QST `AddQuest` TRUE). **PROVEN.**

`QM+44` is the membership table `004B00C0` searches before a
name from that walk is allowed to activate.

**`004B2890` is the sibling post-activate pass on the same
manager.** Init Quests always `004B4260` then `004B2890`. It
walks `QM+112` (circular node list), not `+44`, not
`world+172`. On no-save New Game that list is the empty ctor
sentinel. **PROVEN** as a different structure; the `+112` fill
path is persist / later, not `AddQuest`.

---

## Three name vectors (do not collapse)

| Slot | Object | Element | Writer | First no-save consumer |
|---|---|---|---|---|
| `world+184/+188/+192` | `CWorld` (`004A0D90` `ebp`) | every `AddQuest` name | `004A1080` `lea esi, [ebp+184]` | catalog; **not** `004B4260` |
| `world+172/+176/+180` | same `CWorld` | `AddQuest` **TRUE** only | `004A10C4` `lea esi, [ebp+172]` | `0049F24E` `004B4260` |
| `QM+44/+48/+52` | `[0x13B89FC]` | every `AddQuest` name | `004A1101` `call 004B2850` | `004B00C0` find (gate), **not** the walk |

`AddTestQuest` (`004A113B`) writes `world+196` only. No
`004B2850`. **PROVEN.**

`004A08D0` (FinalAlbion flag 1) clears `+184` / `+172` / `+196`.
It does **not** touch `QM+44`. Reloading QST with flag 1 can
make `+184` and `+44` diverge. **PROVEN** absence.

---

## Timeline (no-save New Game)

```
004A67D0  CWorld ctor
  004A68AE  [world+172/+176/+180]=0
  004A68C0  [world+184/+188/+192]=0

004B4590  QuestManager ctor
  004B45BF  [QM+44/+48/+52]=0          // empty vector
  004B465D  [QM+112] = circular dummy  // empty list

004A1840  Load Quests
  004A193A  ecx=world; call 004A0D90(FinalAlbion.qst, 1)
    004A0DA2  004A08D0 clear world vectors
    AddQuest:
      +184 always
      +172 if 00BFEBA8("TRUE")
      004A10F6  ecx=[0x13B89FC]; 004B2850(name)   // QM+44
    AddTestQuest → +196 only
  004A199A  ecx=world; call 004A0D90(GlobalQuests.qst, 0)

0049F180  Init Characters / GUI / Quests
  0049F23D  ecx=[0x13B89FC]
  0049F247  lea edx, [esi+172]
  0049F24E  call 004B4260              // WALK world+172
    each name:
      004B00C0  find in [QM+44, QM+48)  // FILTER third list
      00CB5AD0 / 004BB720
    004B3CE0  construct queued
  0049F253  ecx=[0x13B89FC]
  0049F259  call 004B2890              // WALK QM+112, then +56
```

---

## 1. `004B2850` is `push_back` onto `this+44`

`listing-00480000.txt` `004B2850`:

```
004B2850  mov eax, [ecx+52]        // cap
          lea esi, [ecx+44]        // vector begin
          mov ecx, [esi+4]         // end
          cmp ecx, eax
          je  004B2874             // grow
          test ecx, ecx
          je  004B286C
          push [esp+8]
          call 0099EC30            // CString copy into *end
004B286C  add [esi+4], 4           // end += sizeof(ptr)
          ret 4
004B2874  push 1 / push 1 / …
          mov ecx, esi
          call 00433530            // vector grow+insert
          ret 4
```

`00433530` (`listing-00400000.txt`) is the shared grow:
`(end-begin)>>2`, `00BFEA0E` of `count*4`. Same helper
`AddQuest` uses on `world+184` / `world+172`.

Only `E8` to `004B2850` in the listings: `004A1101`. **PROVEN.**

Ctor `004B4590` zeros the triple (`004B45BF`–`004B45C5`).
Dtor `004B4930` walks `[esi+44]` … `[esi+48]` with `0099EAE0`
then `00BFEA14`. Field identity is **PROVEN**.

---

## 2. `AddQuest` writes three stores, in that order

`004A0D90`: `ebp = ecx` = world (`004A193A` / `004A199A`
`mov ecx, esi` before each call).

After the name CString and `00BFEBA8("TRUE")` → `bl`:

```
004A1080  lea esi, [ebp+184]       // always
          … 0099EC30 / 00433530 …

004A10B2  test bl, bl
          je  004A10F6             // FALSE skips +172
004A10C4  lea esi, [ebp+172]       // TRUE only
          … same push_back …

004A10F6  mov ecx, [0x13B89FC]
          lea eax, [esp+20]        // same name CString
          push eax
          call 004B2850            // always, TRUE or FALSE
```

So:

- `+184` and `QM+44` see **every** `AddQuest` (TRUE and FALSE).
- `+172` sees only TRUE.
- They are still three buffers. **DISPROVEN** that `+44` *is*
  `+184` (different object; `004A08D0` clears only the world
  side).

`Gameflow` is `AddQuest(..., FALSE)` → in `+184` and `QM+44`,
not in `+172`. Later `user.ini` `004B4A10` can still activate
it because `004B00C0` finds it in `+44`. **PROVEN** membership;
activate site is not `004A0D90`.

---

## 3. `004B4260` walks the arg, filters with `+44`

```
004B4260  sub esp, 44
          mov ebp, [esp+56]        // after ebx/ebp: arg0
          mov eax, [ebp+4]         // end
          mov edi, ecx             // this = QuestManager
          mov ecx, [ebp+0]         // begin
          sub eax, ecx
          sar eax, 2               // count
          jbe 004B437F
          …
          lea esi, [ecx+index*4]
          mov ecx, edi
          call 004B00C0            // gate
          je  skip
          mov ecx, [edi+120]
          call 00CB5AD0
          call 004BB720
          …
          call 004B3CE0
          ret 12
```

The function never loads `[edi+44]` as the walk. It only
reaches `+44` inside `004B00C0`.

`004B00C0`:

```
004B0110  mov esi, [ebx+48]        // end
          mov ecx, [ebx+44]        // begin
          call 004B8FF0            // CString find (unrolled, stride 4)
          cmp eax, esi
          setne bl                 // found → allow
```

Empty / `"NULL"` short-circuits to allow (`004B014A`). Miss →
skip activate. **PROVEN.**

Init Quests site (`0049F180`):

```
0049F23D  mov ecx, [0x13B89FC]
          push 1
          push 0
          lea edx, [esi+172]       // esi = world
          push edx
          call 004B4260
```

**`004B4260` walks `world+172`. DISPROVEN that it walks `QM+44`.**

Other `004B4260` sites (none pass `QM+44`):

| Site | Arg0 | Then `004B2890`? |
|---|---|---|
| `0049F24E` | `world+172` | yes, next insn |
| `0049EAC0` | `this+0xAC` (`+172`) | `jmp 004B2890` |
| `004B4A5A` | stack temp from one name (`00433530`) | no |
| `004B5B84` | `END_ACTIVE_QUESTS` temp | no |
| `00892EAF` / `00892EEF` | caller vector | no |

`00892E80` `ActivateQuest` goes through `004B4A10` (temp
one-name vector) then `004B4260`. Still not `+44`.

---

## 4. Relation to `004B2890`

Same `this` (`[0x13B89FC]`). Always paired after the first
`004B4260` on Init Quests / `0049EAC0`.

```
004B2890  mov esi, ecx
          mov eax, [esi+112]
          mov edi, [eax]
          cmp edi, eax
          je  004B2989             // empty circular list
          …
          ebx = [edi+8]
          copy name [edi+20]
          call 004AF7A0            // find in QM+104
          flags [edi+33]/[edi+34] → result
          insert 40-byte node on QM+108
          maybe 00687540
          edi = [edi]; cmp [esi+112]
004B2989  hero via [0x13B86A0]+28
          walk QM+56 / 004B8E40 / 004B1960
          ret
```

`+112` is a **node list**, not a CString vector:

- Ctor `004B465D`–`004B466A`: `00BFEA0E(40)`, `[eax]=eax`
  (sentinel).
- Dtor `004B4813` `004B8C00` then free.
- `004B07B7` `lea ebx, [esi+112]` + `004B8C00` sits in the
  persist/boast reader (`NumAcceptedBoasts` / `BoastFailed`).
  That is **not** `AddQuest`.

On no-save New Game the `+112` walk is empty (`je 004B2989`).
The tail (hero / `QM+56`) still runs. Whether that tail is a
no-op without instances is **UNREAD** here.

**`004B2890` does not walk `QM+44` or `world+172`. PROVEN.**

`004AF7A0` (used by the `+112` loop) searches `QM+104`, another
list. Do not rename `+104` / `+108` / `+112` without a later
proof.

---

## Classifications (short)

1. **`QM+44` vs `world+172` / `world+184` — third list. PROVEN.**
   Different object (`[0x13B89FC]` vs `CWorld`). Own begin/end/cap.
   Own `00433530`. Ctor/dtor on the manager.
2. **Same *payload* as `world+184` after first QST parse — PROVEN
   as copies, DISPROVEN as alias.** Every `AddQuest` name, TRUE
   and FALSE. `004A08D0` can desync them.
3. **`004B4260` walk — `world+172` (or other *arg*). PROVEN.**
   `QM+44` is only the `004B00C0` membership filter.
4. **`004B2890` — sibling after activate, `QM+112` then `+56`.
   PROVEN.** Not the `+44` catalog. Empty `+112` on no-save.
5. **`004B2850` is not activate. PROVEN.** Activate is later
   `004B4260`.
