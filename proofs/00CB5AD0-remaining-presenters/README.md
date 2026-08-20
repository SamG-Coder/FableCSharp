# Remaining `004B4260` / `00CB5AD0` grouping presenters

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** collapse Gameflow `00CDD440` `jmp [vtbl+8]`
onto `S_QNOVI` `00DABAC0`.
Do **not** treat `calls-by-dest.tsv` parents as the
int3-bounded functions.

Question: who on no-save New Game presents
`Q_NewOakValeIntro` to `00CB5AD0` so `00CDD440`
can `jmp [vtbl+8]` into `00DABAC0`?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **UNKNOWN**.

Authority: ExeIndex
`listing-00480000.txt` `004B2510` / `004B2795` /
`004B3E56` / `004B3F17` / `004B4260` / `004B42E8` /
`004B49E0` / `004B4A10` / `004B4AA0` / `004B5080` /
`004B5B84` / `0049EA40` / `0049EAC0` / `0049EF50` /
`0049F180` / `0049F24E`;
`listing-00880000.txt` `00892D80` / `00892EA0` /
`00892EE0` / `008969B1`;
`listing-00c80000.txt` `00CB5AD0`;
`listing-00cc0000.txt` `00CD6E27` / `00CDD440` /
`00CE7670` / `00CE7AE7` / `00CE7FEC`;
`listing-00d80000.txt` `00DAAC00` / `00DAAC16` /
`00DAACE0` / `00DAAD76` / `00DBEF70`;
`listing-00740000.txt` `0074C141`;
`listing-007c0000.txt` `007C9A1C`;
`calls-by-dest.tsv` dest `0x00CB5AD0` /
`0x004B4260` / `0x004B4A10` / `0x004B4AA0`;
`e8.tsv`; `ff.tsv` disp `1108` / `1116`;
`functions.tsv` `004B2510` / `0049EA40` /
`0049EF50` / `004B49E0` / `004B5080` /
`00892D80` / `00CE7650`;
`vtbl.tsv` `0x01260F0C` slots 276–279 /
`0x012D7A28`;
`xrefs-by-string.tsv` `Q_NewOakValeIntro`;
`assembly/compiled-defs/script` (0 intern);
siblings `proofs/q-novi-activator-callers`,
`proofs/004B2890-empty-first`,
`proofs/oakvale-later-activate`,
`proofs/00CDD440-vtbl8-slot`,
`proofs/cactivatequestdef-payloads`,
`proofs/gameflow-type33-give`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Who presents `Q_NewOakValeIntro` to `00CB5AD0` on no-save New Game? | **Nobody.** Unique `E8` is `004B42E8` inside `004B4260`. Every recovered list on this walk **excludes** the intern `0x012C5D14`. | **PROVEN** omit |
| `calls-by-dest` parent of that `E8`? | **`004B2510`.** Over-merge. Real fn is **`004B4260`** (int3 `004B425B`–`004B4260`). `004B2510` ends `004B2795`. | **PROVEN** grouping lie |
| Does any remaining `004B4260` grouping parent push the intern? | **No.** Un-grouped bodies below. None `push 0x012C5D14`. | **PROVEN** |
| Later `004B4260` after a region exists? | Gameflow `vtbl+1116` `00CE7AE7` / `00CE7FEC` **after** the Oakvale **wait**. Names are fresco / `Q_GuildTraining` / `V_*`, **not** Oakvale. Wait never returns on this walk. | **PROVEN** leftover; **DISPROVEN** as Oakvale presenter |
| Who first writes a `00DABAC0` vtbl onto a live quest? | **`00DAAC16` `mov [esi], 0x12D7A28`.** Only `E8` of `00DAAC00` is `00DBEF8B` in factory `00DBEF70`. That factory runs after a **hit** `00CB5AD0` of this name. | **PROVEN** ctor; **DISPROVEN** no-save |
| First no-save `00CDD440` dest? | Gameflow `vtbl+8` = **`00CE7670`**, not `00DABAC0`. | **PROVEN** |
| Invent `ActivateQuest("Q_NewOakValeIntro")` so Main can enter `00DABAC0`? | **No.** | **DISPROVEN** |

---

## Verdict

**No-save New Game never presents `Q_NewOakValeIntro`
to `00CB5AD0`. Main `00CDD440` therefore cannot
`jmp [vtbl+8]` into `00DABAC0` on this walk.**

`00CB5AD0` is a **name lookup** (`00CB65D0` then
`00429950`). Hit → `lea eax, [edi+4]` (bind-record
factory). Miss → `xor eax, eax`. It does **not**
write a vtbl. Unique `.text` `E8` is `004B42E8`.
`ff.tsv` dest `00CB5AD0`: **0**.

`004B4260` is the only presenter. `calls-by-dest.tsv`
six rows are **grouping artifacts** (same class as
`007EEF60` vs `007EF200`). Int3-bounded real callers:

| `E8` | Grouping parent | Real fn (int3) | No-save Oakvale intern? |
|---|---|---|---|
| `004B42E8` → `00CB5AD0` | `004B2510` (2964-byte blob) | **`004B4260`** | list walked, name **absent** |
| `0049EAD1` | `0049EA40` | **`0049EAC0`** | **0 inbound `E8`**. `ecx+0xAC` stub |
| `0049F24E` | `0049EF50` | **`0049F180`** Init Quests | `world+172` TRUE only |
| `004B4A5A` | `004B49E0` | **`004B4A10`** | eight `E8`, none `0x012C5D14` |
| `004B5B84` | `004B5080` | save `END_ACTIVE_QUESTS` arm | not no-save |
| `00892EAF` | `00892D80` (34160-byte thunk table) | **`00892EA0`** slot 277 | **0 `E8`**, **0 `ff` disp 1108** |
| `00892EEF` | `00892D80` | **`00892EE0`** slot 279 | **0 `E8`**. Two `ff` disp 1116, both after Oakvale wait, **other names** |

`00CE7670` state 0 waits `vtbl+100` for Give kind
**`0x33`**. Construct posts **`0x37`**. Bind
`00CD6E27` is `00CB5C90` only. `script.bin` has
**0** intern of `0x012C5D14`. `CActivateQuestDef`
16-byte rows never intern that pointer **as ASCII**.

First live `[quest]=0x012D7A28` (slot 2 = `00DABAC0`)
is leftover factory `00DBEF70` / `00DAAC00`, blocked
on the unread later presenter. Do not invent it.

Remaining **UNKNOWN**: a later live CString **equal**
to intern `0x012C5D14` after a region exists
(thing `0x8F` `+120`, action `+168`, component
`0x6C` `+40`, or a `CActivateQuestDef` intern
**dword**). That is off the no-save walk.

---

## 1. Grouping: `00CB5AD0` is not inside `004B2510`

`calls-by-dest.tsv`:

```
0x00CB5AD0  0x004B42E8  0x004B2510
```

`functions.tsv` has **no** `0x004B4260` row. Indexer
uses greatest start `<=` site → `004B2510`.

`listing-00480000.txt`:

```
004B2507  int3 …
004B2510  push ebp                  ; alloc 20-byte nodes
          …
004B2795  ret 4
004B2798  int3 …
          …
004B3E56  call 004B2510             ; from 004B3CE0 construct helper
          …
004B425B  int3 …
004B4260  sub esp, 44               ; REAL presenter
004B42E8  call 00CB5AD0             ; UNIQUE
```

`004B2510` is a **different** function. `004B3E56`
is the construct-path helper, **not** the lookup.
**PROVEN** grouping lie. Same artifact as
`007EEF60` / `008421C0` / `00892D80`.

`00CB5AD0` (`listing-00c80000.txt`):

```
00CB5AD0  push ebx
          mov ebx, [esp+8]          ; CString* name
          lea esi, [ebp+4]
          call 00CB65D0
          cmp edi, esi → 00429950   ; name compare
hit:      lea eax, [edi+4] / ret 4
miss:     xor eax, eax / ret 4
```

Lookup only. **DISPROVEN** as the vtbl write.

---

## 2. Six `004B4260` sites, un-grouped

`calls-by-dest.tsv` dest `0x004B4260` (6 rows).
`e8.tsv` same six. `ff.tsv` dest `004B4260`: **0**.

### `0049EAD1` — stub `0049EAC0`, not predicate `0049EA40`

```
0049EA3E  int3
0049EA40  push esi                  ; predicate; ret 4
          …
0049EABD  ret 4
0049EAC0  push 1
          add ecx, 0xAC             ; +172 on ecx, not world
          push 0
          push ecx
          mov ecx, [0x13B89FC]
0049EAD1  call 004B4260
0049EADC  jmp 004B2890
0049EAE1  int3
```

`e8.tsv` dest `0049EAC0`: **0**. `vtbl.tsv`: **0**.
`abs.tsv`: **0**.

Grouping parent `0049EA40` **is** called:

| Site | Parent | After the call |
|---|---|---|
| `0074C141` | `0074C130` | `test al` / `je 0074C5DD` |
| `007C9A1C` | `007C99F0` | `test al` / `je 007C9DF5` |

Both `00686A80` then **`0049EA40`**. Both consume
`al`. They do **not** fall into `0049EAC0`.
**DISPROVEN** as presenters of `00CB5AD0`.

### `0049F24E` — Init Quests `0049F180`, not `0049EF50`

```
0049EF4C  int3
0049EF50  push ebp / sub esp, 0x450
          push 0x1238E10            ; NOT Init Quests
          …
0049F17F  int3
0049F180  sub esp, 48
          push "Init Characters"
          …
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260
```

`world+172` is `AddQuest` **TRUE**.
`Q_NewOakValeIntro` is **FALSE** → `world+184` /
`QM+44` only. First `004B2890` at `0049F259` is
empty `+112` skip (`004B2890-empty-first`).
**DISPROVEN** as this name.

### `004B4A5A` — wrapper `004B4A10`, not `004B49E0`

```
004B49D2  int3
004B49E0  push esi                  ; 40-byte setter
          call 004B4450
          004B8C00 / 0049D870
          mov [esi+142], 1
004B4A08  ret
004B4A09  int3
004B4A10  sub esp, 12               ; 1-name list
004B4A5A  call 004B4260
004B4A96  ret 12
```

Eight `004B4A10` `E8` (`q-novi-activator-callers`):
**none** `push 0x012C5D14`. First no-save takes
are empty `+90584` skip and `user.ini` `"Gameflow"`.

`004B4AA0` (thing component `0x6C` `+40` →
`004B4B5F` `004B4A10`) grouping parent is still
`004B49E0`. Real callers:

| `E8` | Grouping | Real need |
|---|---|---|
| `006220C5` / `00622452` | `00621A20` (2317-byte blob) | live thing, `cmp [eax+20], -1` |
| `008969B1` | `00892D80` | live `0x6C`, copy `[edi+40]`, then HUD card |

**DISPROVEN** as no-save Init. Runtime `+40`
equal to intern `0x012C5D14`: **UNKNOWN**.

### `004B5B84` — save `START_ACTIVE_QUESTS`

Inside `004B5080` over-merge (size 1674 does not
cover `004B5B84`; greatest-start again).

```
004B50F5  push "START_NEW_QUEST"    ; parse arm
          …
004B5B54  push "END_ACTIVE_QUESTS"
004B5B84  call 004B4260             ; loaded name vector
```

Only external `E8` of `004B5080` is `004B58F3`
(self, save parser). **DISPROVEN** as no-save.

### `00892EAF` / `00892EEF` — thunks, not `00892D80`

`00892D80` is the empty-intern **index query**
(`004FC210` / `004FB490` / `[eax+85]`). Int3
before each sibling thunk.

```
00892EA0  mov eax, [esp+4]
          mov ecx, [0x13B89FC]
          push 1 / push 1 / push eax
00892EAF  call 004B4260             ; vtbl 01260F0C slot 277
          ret 4

00892EE0  mov eax, [esp+4]
          mov ecx, [0x13B89FC]
          push 0 / push 1 / push eax
00892EEF  call 004B4260             ; slot 279
          ret 4
```

`e8.tsv` dest `00892EA0` / `00892EE0`: **0**.
`ff.tsv` disp **1108** (slot 277): **0 rows**.
`ff.tsv` disp **1116** (slot 279): **2 rows**,
both in Gameflow `00CE7670` (grouping parent
`00CE7650`, 8914-byte blob):

```
00CE791D  push "Q_NewOakValeIntro"
          call [edx+1180]           ; card, not 004B4260
00CE7977  push "Q_NewOakValeIntro"
00CE7995  call [edx+100]            ; 00893570 kind 0x33
          miss → yield 006E7410     ; FOREVER on no-save
          …
00CE7A13  push "Hook_Fresco_07_OakValeRaid"
          0044BFF0 … Fresco_09 / _10 / "Q_GuildTraining"
00CE7AE7  call [edx+1116]           ; 00892EE0 AFTER wait
          …
          V_RockTrollFirstEncounter / V_StatueMaster /
          V_SwordInTheStone / V_TempleOfLight
00CE7FEC  call [edx+1116]           ; second leftover list
00CE8007  push "Q_GuildTraining"
          call [edx+100]            ; next wait
```

**DISPROVEN** as Oakvale intern. **PROVEN** as
later `004B4260` of **other** names, **blocked**
on the `0x33` wait. Do not collapse “Gameflow
eventually `004B4260`s” into “it `00CB5AD0`s
`Q_NewOakValeIntro`”.

---

## 3. No-save timeline — still no Oakvale `00CB5AD0`

```
00CD6E27  00CB5C90 bind
          Q_NewOakValeIntro / S_QNOVI / factory 00DBEF70
          // NOT 00CB5AD0, NOT 00DAAC16
004A0D90  AddQuest FALSE → world+184 / QM+44
          TRUE only → world+172                    // skip
0049F24E  004B4260([world+172])                    // grouping 0049EF50
004B42E8  00CB5AD0                                 // grouping 004B2510
          names: Q_SunnyvaleMaster … not Oakvale
0049F259  004B2890 empty +112 skip
00416C11  +90584 empty skip 004B4A10
user.ini  00892E80 004B4A10("Gameflow")
          same unique 00CB5AD0
first type-1 00CB8220
  Sunnyvale 00CDD440 → 00CDD360
  Gameflow  00CDD440 → 00CE7670                    // NOT 00DABAC0
    vtbl+1180 card
    vtbl+100 0x33 miss → yield                     // FOREVER
    00CE7AE7 / 00CE7FEC not reached
```

`xrefs-by-string.tsv` `0x012C5D14`: **five** sites
(`00CD6E28` / `00CD6E87` bind, `00CE791E` card,
`00CE7978` / `00CE79CA` wait). **Zero** into
`00CB5AD0` / `004B4A10` / `004B4260`. **PROVEN**.

`assembly/compiled-defs/script`: **0** intern /
ASCII `Q_NewOakValeIntro`. **PROVEN**.

---

## 4. Who writes `00DABAC0` onto a yielded quest

rdata `012D7A28` slot 2 is the PE constant
`00DABAC0`. A live object indexes it only after:

```
00DBEF70  push 0x10C / 00BFEA1A
00DBEF8B  call 00DAAC00             ; only E8
00DAAC00  call 00CB8110             ; base vtbl 012C1648
                                    ; slot 2 = 00CBD4C0 ret
00DAAC16  mov [esi], 0x12D7A28      ; LIVE slot 2 = 00DABAC0
```

`e8.tsv` dest `00DBEF70`: **0** (vtbl factory).
`e8.tsv` dest `00DABAC0`: **0**.
`e8.tsv` dest `00DAAC00`: **1** (`00DBEF8B`).

Main watcher (`00DAACE0`):

```
00DAAD21  mov [esi], 0x12D7A3C
00DAAD27  mov [esi+52], 0xCDD440
00DAAD2E  mov [esi+56], edi         ; S_QNOVI
00DAAD49  call 00CB7E50             ; attach; no 00CDD440 yet
```

Clone `00DAAD70` (`+16`):

```
00DAAD73  mov ecx, [esi+56]
00DAAD76  call [esi+52]             ; 00CDD440
00CDD440  mov eax, [ecx]
          jmp [eax+8]               ; 012D7A28+8 = 00DABAC0
```

That chain needs a **prior** `00CB5AD0` **hit**
so `004B3CE0` `call [eax+4]` can run `00DBEF70`.
On no-save the name never enters the 12-byte
queue. **DISPROVEN** first-seen.

No-save first `00CDD440` is Gameflow:

```
00CE75F7  mov [esi+52], 0xCDD440
00CE75FE  mov [esi+56], edi         ; Gameflow
          00CDD440 jmp [012C3FA4+8]
                   00CE7670
```

**PROVEN**. **DISPROVEN** as `00DABAC0`.

`004B3CE0` `004B3F17 call [eax+4]` /
`004B3F20 call [edx+8]` is the construct arm
**after** a hit. It is **not** a second
presenter into `00CB5AD0`. Whether that
`[edx+8]` is first `00DABAC0` vs later
`00DAAD76` is leftover vs no-save and is
**not** reopened here.

---

## 5. Later thing after a region exists

Lookout `006B3FF0` is first region. Native
Give of this **name** is `00DBE295` **after**
`StartOakVale` map-ready **and** `AttackOver`,
inside already-ticking `S_QNOVI`. Circular:
Give unblocks Gameflow wait only if childhood
already ran. **PROVEN** site; **blocked**
(`gameflow-type33-give`).

Thing / action paths that can `004B4A10` a
**runtime** CString after a live `+145`:

| Path | Name source | No-save? | Equal intern? |
|---|---|---|---|
| `007EF200` `CTCExpression` | nested `+120` | needs `0x8F` | **UNKNOWN** |
| `00843FC0` action | `this+168` from def `+40` | needs thing | **UNKNOWN** as intern dword |
| `004B4AA0` | component `0x6C` `+40` | needs thing | **UNKNOWN** |
| `CActivateQuestDef` 16-byte | persist intern u32 | not Leave | ASCII **DISPROVEN**; dword **UNKNOWN** |
| debug `0061AB30` | `world+196` card | `[+343]` UI | **DISPROVEN** New Game |

Until a recovered live CString **equals**
`0x012C5D14`, the later presenter stays
**UNKNOWN**. Do not fill it with
`ActivateQuest("Q_NewOakValeIntro")`.

---

## What this is not

| Claim | Class |
|---|---|
| `004B2510` owns `00CB5AD0` | **DISPROVEN** (int3 bounds) |
| `0049EA40` `E8`s `004B4260` | **DISPROVEN** (predicate; real is `0049EAC0`) |
| `0074C141` / `007C9A1C` present Oakvale | **DISPROVEN** (`test al` of `0049EA40`) |
| `0049EF50` is Init Quests | **DISPROVEN** (`0049F180`) |
| `004B49E0` is the 1-name wrapper | **DISPROVEN** (`004B4A10`) |
| `00892D80` `E8`s `004B4260` | **DISPROVEN** (`00892EA0` / `00892EE0`) |
| `00CE7670` constructs Oakvale | **DISPROVEN** (wait `0x33`) |
| `00CE7AE7` / `00CE7FEC` lists include Oakvale intern | **DISPROVEN** (fresco / GuildTraining / `V_*`) |
| Bind `00CD6E27` writes `00DABAC0` | **DISPROVEN** (`00DBEF70` factory dword) |
| `00CB6EA0` writes slot 2 | **DISPROVEN** (`00CDD440-vtbl8-slot`) |
| `script.bin` intern `0x012C5D14` | **DISPROVEN** (0 hits) |
| `CActivateQuestDef` ASCII intern | **DISPROVEN** |
| Host `ActivateQuest("Q_NewOakValeIntro")` | **DISPROVEN** |

---

## Host

`EngineLifecycle` Notes `00416BCF` skip and
`"004B4A10 not Q_NewOakValeIntro"`.
`ActivateNamedQuest` walks `world+172` only.
`No_save_does_not_activate_Q_NewOakValeIntro`
omits the name. Pump traces must not contain
`Va==00DABAC0` or `Va==00DAAC00`. **MATCH**.

Do **not** add a recovered-caller Note that
pretends `00892EA0` / `00892EE0` / `0049EAC0` /
`004B4AA0` ran on no-save.

---

## Remaining UNKNOWN

1. Inflated `CActivateQuestDef` intern **dword**
   equal to `0x012C5D14` (ASCII already empty).
2. First live thing after a region whose
   `CTCExpression+120` / action `+168` /
   `0x6C+40` CString intern equals that pointer.
3. Exact `call [vtbl+8]` that **resumes** a
   yielded `00DABAC0` (PARITY 0b) — not this
   presenter question.

Until (1)–(2) show a live name, the no-save
presenter of `Q_NewOakValeIntro` to `00CB5AD0`
stays **nobody**, and the later presenter stays
**UNKNOWN**.

---

## Sources (absolute)

- `C:\FableCSharp\assembly\exe\01-sections\text-map\calls-by-dest.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\ff.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00880000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00c80000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00cc0000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00d80000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00740000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-007c0000.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\xrefs-by-string.tsv`
- `C:\FableCSharp\assembly\compiled-defs\script`
- `C:\FableCSharp\proofs\q-novi-activator-callers\README.md`
- `C:\FableCSharp\proofs\004B2890-empty-first\README.md`
- `C:\FableCSharp\proofs\oakvale-later-activate\README.md`
- `C:\FableCSharp\proofs\00CDD440-vtbl8-slot\README.md`
