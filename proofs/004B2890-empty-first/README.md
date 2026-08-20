# First `004B2890` after `004B4260` on no-save — empty `QM+112`

Investigation only. No production `src/` edits.

Question: first `004B2890` after the no-save `004B4260`.
`QM+112` empty circular. Exact body: hero / `QM+56`? Any
write? Host leftover vs skip.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: ExeIndex `listing-00480000.txt` `004B2890` /
`004B4260` / `004B4590` / `004B3CE0` / `0049F24E` /
`0049F259` / `0049EAC0` / `004B05C0` / `004B07B7` /
`004B8C00` / `004B8E40` / `004B1960` / `004B4490`;
`listing-00440000.txt` `00449970` / `004498C0` /
`00449D90` / `0044A3B0`;
`listing-00a00000.txt` `00A01B50`;
`listing-00400000.txt` `0041732A`;
`proofs/quest-manager-plus44`;
also `qm44-gate-find`, `factory0-enqueue`,
`creature-after-leave`, `hero-4299-create`,
`qst-first-load`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First no-save site after `004B4260`? | **`0049F259` `call 004B2890`.** Same `ecx=[0x13B89FC]`. Next insn after `0049F24E`. | **PROVEN** |
| `QM+112` empty circular? | **Yes.** Ctor sentinel `[head]==head`. No no-save filler before this call. | **PROVEN** |
| Exact first-seen body? | `je 004B2989` skip the `+112` loop, then **hero** `00449970` / `00487DC0`, then **`je 004B2AC1`** — **no** `QM+56` walk. | **PROVEN** |
| Hero live here? | **No Thing.** Same miss class as Init Characters (`0049F1BD`) and first `004B4490`. `006AC910` is later Lookout. | **PROVEN** |
| Is `QM+56` empty? | **No.** `004B3CE0` (inside `004B4260`) already linked nine 16-byte nodes. The list is **filled** and **unread** on this call. | **PROVEN** filled; **PROVEN** not walked |
| Any write? | **No** heap / QM / event store. Stack frame only. No `004B73A0`, `004B1960`, `00BFEA0E`, `00687540`. | **PROVEN** |
| Host leftover vs skip? | Host `Note("004B2890")` then continue is a **skip of a no-op**. Implementing the empty `+112` / hero / `+56` arms here would be leftover theater. | **PROVEN** skip; **DISPROVEN** leftover work |

---

## Verdict

On no-save New Game the first `004B2890` is the sibling
immediately after Init Quests `004B4260([world+172])`.

`QM+112` is still the ctor dummy. The boast / persist
reader that fills it has not run. The function therefore
takes `je 004B2989` and **never** enters the node loop
(`004AF7A0` / `+108` insert / `00687540(41)`).

The tail **does** run: `[0x13B86A0]+28` → `00449970` →
`00487DC0`. Player slot `+44` is an empty `00A01B50`
handle. `ebp=0` → `je 004B2AC1`. The `QM+56` walk
(`004B8E40` / `004B1960` / `00687540(73)`) is not taken.

`004B4260` already constructed named slots onto `QM+56`.
That is **not** this function. First `004B2890` does not
mutate that list.

Parent `quest-manager-plus44` left “tail no-op without
instances” **UNREAD**. This note closes it for the
first-seen no-save call: **no-op, no write.**

---

## Timeline (no-save New Game)

```
004B4590  QuestManager ctor
  004B45CA  [QM+56]  = circular dummy   // 00BFEA0E(16), [eax]=[eax+4]=eax
  004B45F5  [QM+76]/[+80]=0             // CString find range
  004B465D  [QM+112] = circular dummy   // 00BFEA0E(40), [eax]=[eax+4]=eax

004A1840  Load Quests
  AddQuest → world+184 / TRUE +172 / 004B2850 QM+44
  // not QM+112, not QM+56

0049F180  Init Characters / GUI / Quests
  0049F1B3  [world+12] 00449970 / 00487DC0  miss
  0049F1D7  00449D90 PLAYER_HERO miss → CREATURE_HERO
            00489D40 holy-site miss          // no 006AC910
  0049F214  0043A380 Init GUI
  0049F24E  004B4260([world+172])
    004B00C0 / 00CB5AD0 / 004BB720
    004B3CE0  link nine slots onto QM+56     // +112 untouched
  0049F253  ecx=[0x13B89FC]
  0049F259  004B2890                         // THIS NOTE
    [QM+112] [head]==head → 004B2989
    [game+28] 00449970 / 00487DC0  miss → 004B2AC1
    ret                                      // no write
  00416BCF  Activate Initial Quests skip
later 004B4490 dummy pump   // same hero miss; not first 004B2890
later 00501450 Lookout      // first Hero Thing
```

`0049EAC0` is the only other `.text` transfer (`jmp 004B2890`
after another `004B4260`). No `E8 0049EAC0` in the listings.
Not the first-seen no-save site. **PROVEN** as a later /
vtbl sibling; first-seen take **DISPROVEN**.

---

## 1. First site is `0049F259`, empty `+112`

`listing-00480000.txt` `0049F23D`–`0049F259`:

```
0049F23D  mov ecx, [0x13B89FC]
          push 1
          push 0
          lea edx, [esi+172]
          push edx
0049F24E  call 004B4260
0049F253  mov ecx, [0x13B89FC]
0049F259  call 004B2890
```

Only two transfers to `004B2890` in the listings:
`0049F259` `call` and `0049EADC` `jmp`. **PROVEN.**

`004B2890` head (`listing-00480000.txt`):

```
004B2890  sub esp, 28
          push ebx / ebp / esi
          mov esi, ecx
          mov eax, [esi+112]
          push edi
          mov edi, [eax]
          cmp edi, eax
          mov [esp+20], edi
          je  004B2989              // empty circular
```

Ctor `004B465D`–`004B466A`: `00BFEA0E(40)`, `[eax]=eax`,
`[eax+4]=eax`, `[QM+112]=eax`. Same dummy shape as `+56`
/ `+104` / `+108`. **PROVEN.**

`004A08D0` / `004B2850` / `004B4260` do **not** write
`+112`. `004B4260` walks the *arg* and queues on a local
12-byte vector then `+156` (`factory0-enqueue`). **PROVEN.**

The `+112` fill is persist / boast `004B05C0`:

```
004B05C0  persist reader
  version 1 or 3 → walk QM+108, "NumAcceptedBoasts"
  else 004B07B7
    004B8C00 clear [QM+112]
    "NumAcceptedBoasts" / "BoastScriptName" / …
    00BFEA0E(40) + 004B73A0 insert on +112
```

Sites: `004B64B4` / `004B64CF` / `004B655A`. Save load,
not no-save Init Quests. **PROVEN** as a later writer;
**DISPROVEN** as a writer before `0049F259`.

So first-seen `[ [QM+112] ] == [QM+112]`. The `+112` loop
(`[edi+8]`, name at `[edi+20]`, `004AF7A0` in `QM+104`,
flags `[edi+33]/[edi+34]`, insert on `QM+108`, optional
`00687540(41)`) is **not entered**. **PROVEN.**

---

## 2. Tail is hero, then `QM+56` — both skipped

After the empty compare:

```
004B2989  mov ecx, [0x13B86A0]
004B298F  mov ecx, [ecx+28]         // 0044A3B0 owner (Init Player Manager 0041732A)
004B2992  call 00449970             // slot by [this+28]
004B2997  mov ecx, eax
004B2999  call 00487DC0             // slot+44 → 00A01B50
004B299E  mov ebp, eax
004B29A0  test ebp, ebp
004B29A2  mov [esp+28], ebp
004B29A6  je  004B2AC1              // MISS → ret
004B29AC  test [ebp+145], 1
004B29B3  jne 004B2AC1              // bit0 set → ret
004B29B9  mov eax, [esi+56]
004B29BC  mov ebx, [eax]
004B29BE  cmp ebx, eax
004B29C0  je  004B2AC1              // empty +56
          ; else 004B8E40([QM+76, QM+80), quest+48)
          ; hit → 00BFEA0E(40) / 004B1960 / maybe 00687540(73)
004B2AC1  pop / add esp, 28 / ret
```

`00487DC0` is `add ecx, 44; jmp 00A01B50`. Empty handle
(`[ptr+4]==0`) is `xor eax, eax` with **no store**.
**PROVEN.**

Init Characters, same function, a few calls earlier:

```
0049F1B3  mov ecx, [esi+12]         // world player manager
0049F1B6  call 00449970
0049F1BD  call 00487DC0
0049F1C4  je  0049F1CF              // miss → 00449D90
```

`00449D90` binds `PLAYER_HERO` → `CREATURE_HERO` and
`00489D40` early-outs (`[0x13B8647]==0`). No
`006AC910`. Slot `+44` stays empty (`hero-4299-create` /
`creature-after-leave`). **PROVEN.**

Nothing between `0049F1D7` and `0049F259` creates a
player Thing (`0043A380` GUI, then `004B4260` quests).
First `004B4490` (later dummy pump) is the same
`[game+28]` / `00487DC0` miss (`EngineLifecycle.PumpQuests`).
First Hero Thing is later Lookout `006AC910`. **PROVEN**
miss at `004B2999`.

`+145` bit 0 is not consulted on `eax=0`. **PROVEN.**

---

## 3. `QM+56` is already full — unread

`004B3CE0` (only `E8` from `004B4260` at `004B4386`)
inserts a 16-byte node at `[QM+56]` for **every** queued
name, factory hit **and** factory 0 (`factory0-enqueue`
`004B3FAC` / `004B40E5`):

```
node = 00BFEA0E(16)
[node+8] = 52-byte quest slot (name at +48)
link before head: [node]=head, [node+4]=head[-4], …
```

Nine `world+172` TRUE names are therefore on `+56` before
`0049F259`. `[ [QM+56] ] != [QM+56]`. **PROVEN.**

The first-seen `004B2890` still does **not** load
`[esi+56]`. Hero miss jumps over that arm. **PROVEN.**

Backup (not taken here): even a live hero would
`004B8E40` against `[QM+76, QM+80)`. Ctor zeros that
pair. `004B8E40` `sar ebx, 4` on `end-begin==0` goes
`jle` / miss → `je 004B2A21` next node, no alloc.
`004B3CE0` writes `+56` / `+156`, not `+76`. First-seen
`+76` empty is **PROVEN** from ctor + no writer on this
walk; a later filler is **UNREAD** and not this call.

Do **not** collapse “`+56` has slots” into “`004B2890`
walks them on New Game.”

---

## 4. Writes on this call

Taken path `004B2890` → `004B28A4 je` → `004B2989` →
`004B29A6 je` → `004B2AC1 ret`:

| Store | Happens? |
|---|---|
| `[esp+20]` = sentinel node | stack only |
| `[esp+28]` = 0 (hero) | stack only |
| `QM+112` / `+108` / `+104` / `+56` / `+76` | **no** |
| `00BFEA0E` / `00BFEA14` / `004B73A0` / `004B1960` | **no** |
| `0099EC30` name copy | **no** (inside skipped `+112` loop) |
| `00687540` (41 or 73) | **no** |
| `00A01B50` handle cleanup | **no** (`[+4]==0` → `xor eax,eax`) |
| `00449970` / `004498C0` | read slot vector |

`004B3CE0` `00687540(55,50)` already ran **inside**
`004B4260`, on `[world+96]`, not `QM+112`. That is a
different function and a different list. **PROVEN.**

---

## 5. Host leftover vs skip

`EngineLifecycle.InitCharactersAndQuests` after the
`world+172` activate loop:

```
Note(QuestManagerActivate, "Init Quests", "Quest", "004B2890");
```

No `+112` walk, no hero sync, no `+56` / `004B8E40`.
Native first-seen is the same: call, two empty/miss
jumps, return.

| Host action | Class |
|---|---|
| `Note` the VA then continue | **skip** of a **PROVEN** no-op |
| Walk `+112` / post event 41 here | **LEFTOVER** (list empty) |
| Walk `+56` / `004B1960` / event 73 here | **LEFTOVER** (hero miss) |
| Treat `004B2890` as a second activate | **DISPROVEN** (activate was `004B4260`) |
| Skip the `Note` entirely | **DIVERGE** vs the listing pair |

The Event Manager comment on `006874B0` already records
empty `[quest+112]` on first-seen `004B2890`. This note
adds: the hero tail is also a miss, so the whole body is
a skip, not a hidden `+56` pass.

`004B4490` (later pump) is a **different** function. It
does walk `+56` for `00CB8220`. Do not implement that
body at `0049F259`.

---

## What this is not

| Claim | Class |
|---|---|
| `004B2890` walks `QM+44` / `world+172` | **DISPROVEN** (`quest-manager-plus44`) |
| First `004B2890` walks `QM+112` nodes | **DISPROVEN** (ctor sentinel) |
| First `004B2890` walks `QM+56` | **DISPROVEN** (hero miss) |
| `QM+56` still empty after `004B4260` | **DISPROVEN** (`004B3CE0` linked nine) |
| `004B4260` fills `+112` | **DISPROVEN** (`+156` / local queue) |
| `AddQuest` / `004B2850` writes `+112` | **DISPROVEN** |
| Hero Thing exists at Init Quests | **DISPROVEN** |
| Host must implement the `+112`/`+56` arms to match first-seen | **DISPROVEN** |
| `0049EAC0` is the first no-save site | **DISPROVEN** |

---

## Classifications (short)

1. **First `004B2890` is `0049F259` after `004B4260`.
   PROVEN.** Same manager. Only `call` site on no-save
   Init Quests.

2. **`QM+112` empty circular. PROVEN.** Ctor dummy.
   Persist `004B05C0` / `004B07B7` is the filler and is
   not on this walk.

3. **Body is hero miss, then return. PROVEN.**
   `00449970` / `00487DC0` → `je 004B2AC1`. `QM+56` is
   populated and unread.

4. **No write. PROVEN.** Stack only. No event post, no
   node alloc, no QM field store.

5. **Host `Note` then skip is correct. PROVEN.**
   Implementing the skipped arms here would be
   leftover. The pair `004B4260` then `004B2890` stays
   in the trace; the empty body does not grow work.
