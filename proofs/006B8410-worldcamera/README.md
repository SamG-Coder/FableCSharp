# `006B8410` WorldCamera+6500 → `00881210`

Investigation only. No production `src/` edits.

Question: `006B8410` is unique first `0049F180` child
via `WorldCamera+6500` → `00881210`. What does it
construct? Host leftover (no Note / no call)?

Do **not** start at Oakvale / `00DBDE40` /
`CAM_OVIF_SHOT2`. That is later `Q_NewOakValeIntro`,
not Leave / Load World / first no-save `0049F180`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: ExeIndex `listing-00480000.txt` `0049F180`–
`0049F1EA`; `listing-00680000.txt` `006B4900` /
`006B4B02`–`006B4B4A` / `006B8410` / `006B84B0`;
`listing-00880000.txt` `00880A40` / `00881210` /
`00881370`; `e8.tsv` dest `006B8410`;
`src/Fable.Game/EngineLifecycle.cs`
`InitCharactersAndQuests`;
`src/Fable.Game/WorldCamera.cs` `Construct`;
siblings `proofs/0049F180-first-children`,
`audit-worldcamera`, `init-gui-0043A380`.

---

## Verdict

| Claim | Answer | Class |
|---|---|---|
| Unique `.text` `E8` of `006B8410`? | **Only** `0049F1E5` | **PROVEN** |
| First child of `0049F180`? | **No.** After `"Init Characters"` bind (`00449970` / `00487DC0` / miss `00449D90`) | **DISPROVEN** as first child |
| Path is `[world+24]+6500` → thunk → `00881210`? | `mov ecx,[esi+24]` / `[ecx+6500]` / `call 006B8410` / `add ecx,0x90` / `jmp 00881210` | **PROVEN** |
| What does `006B8410` **construct**? | **Nothing.** Two-insn reset thunk, not a ctor | **DISPROVEN** as construct |
| Who constructed `[+6500]`? | Init World `006B4900` `push 0x160` / `006B84B0` → `00881370` at `+144` | **PROVEN** |
| First-seen `00881210` work? | Counts `+20/+124/+164==0` → skip list resets; dummy heads self-point; tail `00880A40` re-zeros scalars | **PROVEN** empty reset |
| Host `InitCharactersAndQuests` Note / call? | **Neither.** Notes `0049F180` / bind / GUI / quests only | **PROVEN leftover** |
| Host `WorldCamera` models `+6500`? | **No.** `Construct` skips the colour-filter bank | **PROVEN leftover** |
| Filling that gap first-seen? | Empty reset theater | **LEFTOVER** (do not implement) |
| Oakvale / `006B8640` seed here? | Seed is later `004A5DF3` `006B3FF0`, after this Note | **DISPROVEN** |

**Answer:** `006B8410` constructs **no** object. It
offsets the already-live colour-filter bank at
`WorldCamera+6500` by `+144` and **resets** that
list block. Host has **no Note and no call**.

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
0042F491  Init Game
  004A6E30  Init World
    006B4900  WorldCamera [world+24]
      +6496  008852E0
      +6500  alloc 0x160 → 006B84B0     // CONSTRUCT
        +144 00881370  counts 0, dummy heads
  00416953  Loading world
    004A1840
    [0x13B8648]==0
    00416BCA  0049F180(ecx=world)
      "Init Characters" 00449970 / 00487DC0 / 00449D90
      0049F1E5  006B8410                  // THIS — reset, not ctor
        ecx = [WorldCamera+6500]
        add ecx, 0x90
        jmp 00881210                      // empty first-seen
      "Init GUI"    0043A380
      "Init Quests" 004B4260 / 004B2890
004189C2  dummy pumps
  WorldFrame 0→1: 004A5DF3 006B3FF0
    006B8640([this+6500])                 // later copy, not here
```

---

## 1. Unique site, not first child

`listing-00480000.txt`:

```
0049F1DC  mov ecx, [esi+24]         // WorldCamera
0049F1DF  mov ecx, [ecx+6500]
0049F1E5  call 006B8410
0049F1EA  push "Init GUI"
```

`e8.tsv` dest `006B8410`: **one** row, `0049F1E5`.
**PROVEN** unique.

`0049F180` children before that call: string
`"Init Characters"`, stub `009D8240`, CString dtor,
`00449970`, `00487DC0`, then miss-gated `00449D90`.
`006B8410` is the **first WorldCamera** child and
the first **unconditional** child after the bind.
It is **not** the first child of `0049F180`.

---

## 2. Thunk is reset, ctor is earlier

`listing-00680000.txt`:

```
006B8410  add ecx, 0x90             // +144
006B8416  jmp 00881210
```

No alloc. No `00BFEA1A`. No vtbl write.

Ctor of the `ecx` object is Init World
`006B4900` (`listing-00680000.txt`):

```
006B4B02  push 0x160
006B4B07  call 00BFEA1A
006B4B13  mov ecx, [esi+6496]
006B4B1A  push ecx
006B4B1B  mov ecx, eax
006B4B1D  call 006B84B0
006B4B26  mov edi, [esi+6500]
006B4B2C  mov [esi+6500], eax
```

`006B84B0`:

```
006B84B0  mov eax, [esp+4]
006B84B7  lea ecx, [esi+144]
006B84BD  mov [esi+4], eax
006B84C0  call 00881370
006B84C9  xor al, al
006B84CB  mov [esi+120], al
006B84CE  mov [esi+121], al
006B84D1  mov [esi], ecx
006B84D6  ret 8
```

`00881370` (`this` = bank `+144`) plants vtbl
`01278058`, three dummy circular heads, and
**zeros** counts at `+20` / `+124` / `+164`.
Ends with `00880A40` (scalar zero / `1.0f`
defaults). **PROVEN** construct site.

`00881210` is the matching **clear**:

```
00881210  edi = ecx                    // bank+144
          [edi+20]==0  → skip +16
          [edi+124]==0 → skip +120
          [edi+164]==0 → skip +160
          3× walk [edi+4] / [edi+108] / [edi+148]
            head==head → no 00BFEA14
          jmp 00880A40                 // re-zero scalars
```

Ctor left those counts **0** and heads self-linked.
Insert helpers (`006B84E0` / `006B8550`) are **not**
on the `0049F180` walk. First `006B3FF0` /
`006B8640` is later (`004A5DF3`). First-seen
`00881210` is an **empty reset**. **PROVEN.**

A later live fill of those lists is **UNREAD**
here and is not this Note.

---

## 3. Host leftover (no Note / no call)

`InitCharactersAndQuests` (`EngineLifecycle.cs`):

```
Note(InitCharactersFn, … "0049F180 push 0 ecx=world");
Note(PlayerCreatureBindFn, … "00449970 / 00487DC0");
Note(InitGuiFn, … "0043A380 …");
Note(InitQuestsFn, … "004B4260 …");
Note(QuestManagerActivate, … "004B2890");
```

No `006B8410`. No `00881210`. No `WorldCamera+6500`.
**PROVEN** no Note / no call.

`WorldCamera.Construct` matches `006B4900` axes /
weights / `+68` and does **not** allocate `+6496` /
`+6500`. `audit-worldcamera` already classed that
bank leftover.

Implementing a first-seen empty list reset would be
leftover theater. Do **not** grow a host call here.

---

## 4. Not these

| Candidate | Why not |
|---|---|
| `006B84B0` / `00881370` construct | Init World, before `0049F180` |
| `006B8640` copy on `+6500` | first seed `006B3FF0`, after dummy pumps |
| `006B42F0` / `00B23EC0` colour-filter apply | first `0049E080`, WorldFrame>1 |
| `008852E0` at `+6496` | sibling bank; not this `ecx` |
| Oakvale `CAM_OVIF_SHOT2` | later intro; not this child |

---

## Open

- Live (non-first-seen) contents of the `+144`
  lists after script / `006B8550` inserts:
  **UNREAD** as a later note, not this walk.
- `00880A40` fields as named colour-filter
  packets: **PARTIAL** (zeros + `1.0f` **PROVEN**;
  semantic names **UNREAD**).
