# `008421C0` is not the `004B4A10` site — real activator is `00843FC0`

Investigation only. No production `src/` edits.

Candidate: function `008421C0` at site `0084407E` calls
`004B4A10`, maybe `Q_NewOakValeIntro` activate.

Do **not** start at `00DBDE40` / `S_QNOVI`. Do **not** invent
`ActivateQuest("Q_NewOakValeIntro")` on the no-save walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: Fable.exe dump
`assembly/exe/01-sections/text-map/listing-00840000.txt`
(`008421C0`–`00844100`, `00843F50` / `00843FC0` /
`0084407E` / `00844090` / `00860060`);
`listing-00480000.txt` (`004B4A10` / `004B4260`);
`listing-00400000.txt` (`00416C11`);
`listing-00880000.txt` (`00892E80` / `00892EC0`);
`listing-00600000.txt` (`00629979` / `0061AC28`);
`listing-00780000.txt` (`007B5590` / `007B5680` / `007B5AA4`);
`listing-007c0000.txt` (`007EF3A1` / `007EF66C`);
`listing-00680000.txt` (`00694D10` / `00687000`);
`calls-by-dest.tsv` / `e8.tsv` / `functions.tsv` /
`vtbl.tsv` / `rtti.txt` / `strings.tsv`;
`compiled-defs/game/entries.tsv` `CActivateQuestDef`;
siblings `proofs/addtestquest-token`,
`proofs/ini-activate-quest`,
`docs/PARITY.md` “Who activates `Q_NewOakValeIntro`”;
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Is `008421C0` the function at `0084407E`? | **No.** `008421C0` is 19 bytes and `jmp 00694430` at `008421D2`. `0084407E` is inside **`00843FC0`**. `functions.tsv` size **2879** is a greedy merge of many AI-action fns (`ST_CALL_OVER` … `HAMMER` … `00843FC0`). | **PROVEN** |
| What is `008421C0`? | vtbl **`0x01276B0C` slot 4**. `this+98==0` → `call [vtbl+12]` (slot 3 = `00848F10`), then tail **`00694430`**. | **PROVEN** |
| RTTI / class of `008421C0`? | vtbl `0x01276B0C` written by ctor **`00860060`**. No `.?AV…` hit next to this vtbl. **Not** `CActivateQuestDef`. | **PARTIAL** (vtbl **PROVEN**; type name **UNREAD**) |
| What is `00843FC0`? | vtbl **`0x012752C4` slot 12** (`call [reg+48]`). Ctor **`00843F50`** stores that vtbl. Runtime action for **`CActivateQuestDef`**. RTTI `0x01379518` `.?AVCActivateQuestDef@@`. | **PROVEN** |
| Args to `004B4A10` at `0084407E`? | `ecx=[0x13B89FC]` (QuestManager); stdcall **3** (`ret 12`): **arg1** `&this+168` (`CString` quest name), **arg2** `0`, **arg3** `movzx this+172`. | **PROVEN** |
| Does `004B4A10` use arg2/arg3? | **No load.** Body wraps **arg1** via `00433530` (`push 1; push 1` **inside** `004B4A10`) then `004B4260`. Call-site flags are stdcall padding. | **PROVEN** unread |
| `calls-by-dest.tsv` callers of `008421C0`? | **Zero `E8`.** No dest=`0x008421C0` row. No `call 008421C0` in listings. Only vtbl `0x01276B0C[4]`. Third-column hits (`004B4A10` `0084407E` `008421C0`) are the **mis-sized** parent. | **PROVEN** |
| Callers of `00843FC0`? | **Zero `E8`.** Only `0x012752C4[12]`. Indirect e.g. `00687000` `call [edx+48]`. Constructed by **`00843F50`** then `006644F0`. | **PROVEN** |
| Is this the `Q_NewOakValeIntro` activator? | **No** on no-save New Game. Name at `this+168` is the **def’s** quest string (e.g. `Expression_Follow` at `00629979`), not the Oakvale script bind. | **DISPROVEN** as Oakvale first activate |
| Debug no-save vs live New Game? | **Neither** path is `008421C0` / `00843FC0`. No-save skips `00416C11`; `user.ini` is `00892E80("Gameflow",1,1)`. Click New Game is `START_NEW_QUEST` / `004B5080`, not this action. | **PROVEN** |

---

## Verdict

`calls-by-dest.tsv` / `functions.tsv` **lie about the parent**.
Site `0084407E` is **`00843FC0`**, slot 12 of the
**`CActivateQuestDef` action** (`vtbl 0x012752C4`).
It is a **thing-action** start: copy `this+173` onto the
QuestManager, then

`004B4A10([0x13B89FC], &this+168, 0, this+172)`.

`008421C0` is a **different** 19-byte slot-4 thunk on
`vtbl 0x01276B0C`. **0** direct callers.

This is **not** how no-save New Game (debug host walk)
or live click New Game starts `Q_NewOakValeIntro`.
**DISPROVEN** as that activator.

---

## 1. `008421C0` body (listing-00840000.txt)

```
008421C0  push esi
008421C1  mov esi, ecx
008421C3  mov al, [esi+98]
008421C6  test al, al
008421C8  jne 008421CF
008421CA  mov eax, [esi]
008421CC  call [eax+12]          ; slot 3 = 00848F10 for 0x01276B0C
008421CF  mov ecx, esi
008421D1  pop esi
008421D2  jmp 00694430           ; generic action tick (also 0x012752C4[4])
008421D7  int3
```

Next real fn is `008421E0` (`push "ST_CALL_OVER"`).
`functions.tsv` `0x008421C0  2879` swallows that plus
`00843FC0` (end `0084408E`). **LEFTOVER** map, not a
single procedure.

### vtbl `0x01276B0C`

| slot | VA | note |
|---:|---|---|
| 0 | `00860100` | |
| 3 | `00848F10` | start-ish; sets `this+98=1` |
| **4** | **`008421C0`** | |
| 10 | `008600D0` | `mov eax, 8; ret` |
| 12 | `00694D10` | generic start (`[this+99]=1`) |

Ctor **`00860060`** (`ret 12`) writes `[esi]=0x01276B0C`
and `this+168` thing ptr (`00A01B90`). Copy ctor
`00860240`. `E8` of ctor: `0085F757` / `0085F795`
(mode 13) and `0092DFDB` / `0092E0A3`. **No**
`CActivateQuestDef` string on this type.

---

## 2. Real `004B4A10` site: `00843FC0`

```
00843FC0  sub esp, 12
          push ebx, ebp, esi, edi
          mov edi, ecx                        ; this
          lea esi, [edi+173]
          call 00A01B50                       ; thing at +173
          … copy via 00A01B10 / 00A01B90 onto [0x13B89FC]+168 …
          test 00A01B50(+173)
          je 00844066
            004C7CC0 → 009D49B0(0x13CA828) → 0099EFB0(QM+0xB0)
00844066  mov ecx, [0x13B89FC]
          xor edx, edx
          mov dl, [edi+172]
          lea eax, [edi+168]
          push edx                            ; arg3
          push 0                              ; arg2
          push eax                            ; arg1 = CString*
0084407E  call 004B4A10
          mov [edi+99], 1
          ret
```

Same object’s slot 56 is **`00844090`**:
`004AF610(&this+168)` (already-active?); miss →
`jmp [vtbl+16]` (slot 4 = `00694430`).

### vtbl `0x012752C4` (`CActivateQuestDef` action)

| slot | off | VA |
|---:|---:|---|
| 0 | 0 | `0084B500` |
| 4 | 16 | `00694430` |
| 11 | 44 | `0084C8D0` |
| **12** | **48** | **`00843FC0`** |
| 56 | 224 | `00844090` |

RTTI: `rtti.txt` `0x01379518  CActivateQuestDef`;
`strings.tsv` `.?AVCActivateQuestDef@@`. Def name xrefs
`0x01243E40` at `004F5B7E` / `007B5593` / `007B5687`.

Ctor **`00843F50`** (`ret 16`, 4 stdcall args) stores
`[esi]=0x012752C4`, copies arg3 into **`this+168`**
(`0099EC30`), arg4 byte into **`this+172`**, thing into
**`this+173`**.

---

## 3. Concrete `004B4A10` args

`004B4A10` is `ret 12`. `ecx` is always QuestManager
`[0x13B89FC]` at this site (loaded at `00844066`).

| | site `0084407E` | `user.ini` `00892E80` | no-save skip `00416C11` |
|---|---|---|---|
| ecx | `[0x13B89FC]` | `[0x13B89FC]` | `[0x13B89FC]` |
| arg1 | `&this+168` (def quest `CString`) | ini name (`"Gameflow"`) | `world+90584` |
| arg2 | **`0`** | **`1`** | **`0`** |
| arg3 | **`this+172`** (byte) | **`1`** | **`1`** |

`004B4A10` itself:

```
004B4A10  sub esp, 12
          push ebp, esi, edi
          push 1
          push 1
          mov esi, ecx
          mov ecx, [esp+36]          ; arg1 only
          lea eax, [esp+36]
          push eax / push ecx / push 0
          call 00433530              ; wrap name → temp vector
          call 004B4260              ; ret 12; walks vector → 00CB5AD0
          … destroy vector …
          ret 12
```

No `[esp+40]` / `[esp+44]` (arg2/arg3) read. Sibling
`00892EC0` is `push 0; push 1; name` — still unused
padding.

---

## 4. Callers

### `008421C0`

| source | result |
|---|---|
| `e8.tsv` dest `0x008421C0` | **empty** |
| listing `call 008421C0` | **none** |
| `calls-by-dest.tsv` dest column | **none** |
| `vtbl.tsv` | **`0x01276B0C` slot 4 only** |

`calls-by-dest.tsv` rows with **fn=`0x008421C0`**
(including `0x004B4A10  0x0084407E  0x008421C0`) are
the 2879-byte merge. They are **not** callers of
`008421C0`.

### `00843FC0`

| source | result |
|---|---|
| `e8.tsv` dest `0x00843FC0` | **empty** |
| listing `call 00843FC0` | **none** |
| `vtbl.tsv` | **`0x012752C4` slot 12** |
| indirect | `call [reg+48]` e.g. `00687000` |

### `00843F50` (`E8`, then `006644F0` attach)

| site | name / note |
|---|---|
| `00629979` | `"Expression_Follow"`; arg4=`0` |
| `00629A09` | `"Expression_Wait"`; arg4=`0` |
| `007B5AA4` | `007B5680` lookup `"CActivateQuestDef"`; arg4=`[def+44]`; name via `009D49B0` |
| `007EF66C` | `007EF600` same def lookup (editor/debug neighbour of `007EF3A1`) |
| `007F0232` | same pattern |
| `007F0410` | same pattern |

`game.bin` has **6** `CActivateQuestDef` entries
(1 `NULLDEF` + 5 unnamed). Quest strings inside those
defs: **UNREAD** here. None is required to be
`Q_NewOakValeIntro`.

---

## 5. Debug no-save vs live New Game

```
no-save (debug host / automatic after Leave)
  004A1840  Load Quests
  0049F24E  004B4260([world+172])     ; Q_SunnyvaleMaster first
  00416BCF  "Activate Initial Quests"
  00416C02  +90584 empty vs 0x122D70E
            je 00416C16                 ; SKIP 004B4A10
  user.ini  00892E80("Gameflow", 1, 1)
  00CE7670  00893610 Q_NewOakValeIntro = 0  ; wait only

00843FC0 / 008421C0                    ; not on this walk
CActivateQuestDef action               ; needs a spawned thing

live click UI_TEXT_NEW_GAME
  START_NEW_QUEST / 004B5080           ; 0 external E8; not 00843FC0
```

| Path | `008421C0` | `00843FC0` `004B4A10` | `Q_NewOakValeIntro` |
|---|---|---|---|
| No-save first-seen | no | no | **not activated** (`PARITY` **PROVEN**) |
| `user.ini` | no | no (uses `00892E80`) | no (`Gameflow`) |
| `CActivateQuestDef` later | no | **yes**, def name | only if a def stores that string (**UNREAD**) |
| Click New Game | no | no | `START_NEW_QUEST` / script bind, **not** this vtbl |

Host must not treat `008421C0` or `00843FC0` as the
first Oakvale activate.

---

## Nearby strings (false `008421C0` blob)

`functions.tsv` last field:
`ST_CALL_OVER|STANDARD_SCARED|WHATS_THAT|ALARM|…|HAMMER`.
Those are **sibling action ctors** after `008421D7`
(`int3`), not operands of `008421C0` or `00843FC0`.
`Q_NewOakValeIntro` xrefs stay at `00CD6E27` (bind) and
`00CE791D` (Gameflow wait) — **zero** in `00840000`.

---

## Host

Read-only. `EngineLifecycle` must not map this site to
`ActivateQuest("Q_NewOakValeIntro")`. Existing test
`No_save_does_not_activate_Q_NewOakValeIntro` **MATCH**.
)
