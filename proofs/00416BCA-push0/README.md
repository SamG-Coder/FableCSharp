# `00416BCA` is the only no-save `0049F180` (`push 0`)

Investigation only. No production `src/` edits.

Question: first no-save `0049F180` is only `00416BCA`
`push 0`. Confirm `004A2C80` `push 1` is save-only.
Host `InitCharactersAndQuests` site **MATCH**?

Do **not** start Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: Fable.exe via ExeIndex
`listing-00400000.txt` `00416953`–`00416C25`;
`listing-00480000.txt` `0049F180` / `004A1840` end /
`004A21F0`–`004A2D60` / `004A2D70` / `004A3200`;
`e8.tsv` dests of `0049F180` / `004A21F0` / `004A3200`;
sibling `proofs/0049F180-first-children`;
`proofs/004A1840-second-site`;
`src/Fable.Game/EngineLifecycle.cs`
`LoadWorld` / `InitCharactersAndQuests`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| First no-save `0049F180`? | **Only** `00416BCA` `push 0` `ecx=[esi+36]` world after `004A1840` when `[0x13B8648]==0` | **PROVEN** |
| Other `.text` `E8` of `0049F180`? | **One:** `004A2C80` | **PROVEN** |
| `004A2C80` `push 1` on no-save? | **No.** Inside save reader `004A21F0` (`ret 8`) | **DISPROVEN** as no-save |
| `004A2C80` save-only? | **Yes.** Only parents of `004A21F0` are `004A3200` / `004A2F10` / `004A2D70` | **PROVEN** |
| Host `InitCharactersAndQuests` site? | After `LoadWorldMap` (`004A1840`) when `SkipParticlesFirstSeen==0`; Note `"0049F180 push 0 ecx=world"`; no `004A2C80` | **MATCH** / **PROVEN** |

---

## 1. Exactly two `E8` of `0049F180`

`e8.tsv` dest `0049F180`:

```
0x00416BCA	0x0049F180
0x004A2C80	0x0049F180
```

Listings have those two `call 0049F180` only.
**PROVEN.**

---

## 2. No-save site is `00416BCA` `push 0`

`00416953` (`game` vtbl+32). Empty `[esi+90588]`
skips `"Loading save"` `004A3200`:

```
0041696B  lea edi, [esi+90588]
00416973  call 0099B220
00416978  test eax, eax
0041697C  jle 004169C8            ; no-save
0041697E  push "Loading save"
004169AF  call 004A3200
004169C8  push "Loading world"
00416AB3  mov ecx, [esi+36]       ; world
00416ABA  call 004A1840
00416ABF  cmp [0x13B8648], 0x00
00416AC6  mov ecx, [esi+36]       ; world again
00416AC9  je  00416BC8
… editor [0x13B8648]!=0 …
00416BC6  jmp 00416C31            ; skip 0049F180
00416BC8  push 0
00416BCA  call 0049F180
00416BCF  push "Activate Initial Quests"
```

`004A1840` ends `004A21DF` `ret 4` / `int3` pad.
It does **not** `E8` `0049F180`. **PROVEN.**

---

## 3. `004A2C80` `push 1` is save-only

```
004A21DF  ret 4
004A21E2  int3 … 004A21EF
004A21F0  sub esp, 0x170          ; FableSav reader
…
004A2BEE  mov al, [ebp+258]
004A2BF4  test al, al
004A2BFD  jne 004A2CC3            ; +258≠0 skip
004A2C7C  push 1
004A2C7E  mov ecx, ebp            ; world
004A2C80  call 0049F180
004A2D60  ret 8
```

`e8.tsv` callers of `004A21F0` (four, all save):

| Site | Parent |
|---|---|
| `004A32EA` / `004A340D` | `004A3200` `"Loading save"` |
| `004A3017` | `004A2F10` (`world+248` machine) |
| `004A2DC2` | `004A2D70` — `mov [esi+258], 1` **before** the call, so `jne 004A2CC3` **skips** `004A2C80` |

`004A3200` callers: `004169AF` (nonempty `+90588`) and
UI `0062CF30`. `004A2F10` sole `E8` is `004A5BD2` when
`[world+248]!=0`. No-save ctor leaves `+248` / `+258`
0 and never enters those parents.

No-save: empty `+90588` → no `004A3200` → no
`004A21F0` → no `004A2C80`. **PROVEN**
(`004A1840-second-site`).

`functions.tsv` size of `0x004A1840` (**2258**) is a
**bad merge** through `004A21F0`. Do not treat
`004A2C80` as inside `004A1840`.

Load-game body of `004A2C80` is **UNREAD** here
(not this no-save note).

---

## 4. Host site **MATCH**

`LoadWorld` is the only caller of
`InitCharactersAndQuests`:

```
LoadWorldMap();                    // 004A1840
if (SkipParticlesFirstSeen == 0)   // [0x13B8648]
    InitCharactersAndQuests();     // 00416BCA
```

The method Notes `"0049F180 push 0 ecx=world"`.
No host path Notes or calls `004A2C80`.
That is **MATCH** vs native no-save.

Leftovers **inside** the method (`004B4A10` sibling,
`00449D90` / `006B8410` / tail) are
`0049F180-first-children` §5, not a second site.
`SpawnHeroFromPlayerStart`’s later
`Note(InitCharactersFn)` is **LEFTOVER** (Lookout
`006AC910` is not `0049F180`). Not Oakvale.
