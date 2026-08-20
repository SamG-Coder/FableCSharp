# `00CB5AD0` lookup vs bind `00CB5C90` `S_QNOVI`

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat bind `00CB5C90` as a presenter into
`00CB5AD0`. Do **not** collapse `00CD6E27` onto
construct `00DAAC16` / run `00DABAC0`.
Do **not** treat `calls-by-dest.tsv` parents as the
int3-bounded functions.

Question: unique `E8` of `00CB5AD0` is `004B42E8`.
Who calls `004B4260`? What recovered presenters can
reach `00CB5AD0` besides Gameflow `user.ini`? Does
`00CB5C90` bind `S_QNOVI` on no-save? When is
`00CD6E27` reached?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **UNKNOWN**.

Authority: ExeIndex
`listing-00480000.txt` `004A6550` / `004A6677` /
`004B2510` / `004B4260` / `004B42E8` / `004B4A10` /
`004B4A5A` / `004B5B84` / `0049EAC0` / `0049F180` /
`0049F24E`;
`listing-00880000.txt` `00892E80` / `00892EA0` /
`00892EC0` / `00892EE0`;
`listing-00c80000.txt` `00CB5AC0` / `00CB5AD0` /
`00CB5C70` / `00CB5C90` / `00CB5D80` / `00CB5E12` /
`00CB7210`;
`listing-00cc0000.txt` `00CD4FD0` / `00CD5170` /
`00CD52D0` / `00CD6E27` / `00CD6E6D` / `00CE7670`;
`e8.tsv` dest `00CB5AD0` (1) / `004B4260` (6) /
`00CB5C90` (161) / `00CB5D80` (1) / `00CD52D0` (1) /
`00CB5AC0` (156) / `004B4A10` (8);
`ff.tsv` dest `00CB5AD0` / `004B4260` / `00CB5C90`
(**0**); disp **1116** (2, both after Oakvale wait);
`calls-by-dest.tsv`; `functions.tsv` (no
`004B4260` / `00CD52D0` / `00CB5D80` rows);
`vtbl.tsv` `0x01260F0C` slots 276–279;
`xrefs-by-string.tsv` `Q_NewOakValeIntro` /
`S_QNOVI`;
siblings `proofs/00CB5AD0-remaining-presenters`,
`proofs/00DAAC00-sqnovi-no-save`,
`proofs/q-novi-activator-callers`,
`proofs/ini-activate-quest`,
`proofs/gameflow-oakvale-wait`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Unique `.text` `E8` of `00CB5AD0`? | **`004B42E8`** inside int3-bounded **`004B4260`**. `ff.tsv` dest **0**. | **PROVEN** |
| Who calls `004B4260`? | Six `E8`s: `0049EAD1` / `0049F24E` / `004B4A5A` / `004B5B84` / `00892EAF` / `00892EEF`. Zero `ff`. | **PROVEN** |
| Real fns (int3), not grouping parents? | **`0049EAC0`**, **`0049F180`**, **`004B4A10`**, save arm of **`004B5080`**, **`00892EA0`**, **`00892EE0`**. | **PROVEN** grouping lie |
| Besides Gameflow `user.ini`, who reaches `00CB5AD0` on no-save? | First-seen: **Init Quests `0049F24E`** (`world+172` TRUE names). `00416C11` empty skip does **not**. No recovered list includes intern `0x012C5D14`. | **PROVEN** |
| Later leftover presenters of **other** names? | Slot 279 `00CE7AE7` / `00CE7FEC` after the `0x33` wait; thing / action / `CTCExpression` / debug UI / save. | **PROVEN** leftover; **DISPROVEN** as Oakvale / no-save first |
| Does `00CB5C90` bind `S_QNOVI` on no-save? | **Yes.** `00CD6E6D` inside `00CD52D0`. Factory `00DBEF70`, persist `bl=0`, watcher `00CDBD20`. Not construct. | **PROVEN** bind |
| When is `00CD6E27` reached? | Same `00CD52D0` pass: `push 0x012C5D14` **before** the bind `E8`. Init World `"Init Scripts"` `004A6550` → unique `004A6677` `00CB5D80` → unique `00CB5E12` `00CD52D0`. **Before** Init Quests / `user.ini`. | **PROVEN** |
| Is `00CD6E27` / `00CB5C90` a presenter into `00CB5AD0`? | **No.** Bind never `E8`s lookup. Unique lookup `E8` stays `004B42E8`. | **DISPROVEN** |
| Invent `ActivateQuest("Q_NewOakValeIntro")`? | **No.** | **DISPROVEN** |

---

## Verdict

**`00CB5AD0` is the name lookup. Unique `E8` is
`004B42E8` in `004B4260`. Bind is a different
function: `00CB5C90`.**

On no-save New Game, `00CB5C90` **does** insert
`Q_NewOakValeIntro` / `S_QNOVI` / factory
`00DBEF70` during Init World `"Init Scripts"`.
That is **`00CD6E6D`**, after the intern push
**`00CD6E27`**. It does **not** construct
`S_QNOVI` and does **not** enter `00CB5AD0`.

The only no-save first-seen presenter into
`00CB5AD0` besides later `user.ini`
`ActivateQuest("Gameflow")` is Init Quests
`0049F24E` walking `world+172`. That vector
**excludes** intern `0x012C5D14`. Every other
recovered `004B4260` site is a stub with 0
inbound, a save parser, a vtbl thunk with no
first-seen `ff`, or a leftover list **after**
Gameflow’s `0x33` wait.

Do not invent a seventh presenter.

---

## 1. Unique `00CB5AD0` `E8` is `004B42E8`

`e8.tsv` dest `0x00CB5AD0`: **1 row**.
`calls-by-dest.tsv`:

```
0x00CB5AD0  0x004B42E8  0x004B2510
```

`functions.tsv` has **no** `0x004B4260` row.
Indexer greatest-start `<=` site → `004B2510`
(2964-byte blob). Int3 bounds:

```
004B425B  int3
004B4260  sub esp, 44
          mov ebp, [esp+56]          ; CString* vector
          mov edi, ecx               ; QuestManager
          …
004B42D4  push esi
004B42D5  mov ecx, edi
004B42D7  call 004B00C0              ; already-active gate
004B42DC  test al / je 004B4363
004B42E4  mov ecx, [edi+120]
004B42E7  push esi                   ; CString* name
004B42E8  call 00CB5AD0              ; UNIQUE
          test edi, edi
          je miss-queue
          … store [esp+44]=edi … 004BB720
```

`00CB5AD0` (`listing-00c80000.txt`):

```
00CB5AD0  mov ebx, [esp+8]           ; CString* name
          lea esi, [ebp+4]
          call 00CB65D0
          cmp edi, esi → 00429950    ; intern compare
hit:      lea eax, [edi+4] / ret 4
miss:     xor eax, eax / ret 4
```

Lookup only. **DISPROVEN** as vtbl write and as
bind. `ff.tsv` dest `00CB5AD0`: **0**.

---

## 2. Who calls `004B4260`

`e8.tsv` dest `0x004B4260`: **6**. `ff.tsv`: **0**.

| `E8` | Grouping parent | Real fn (int3) | No-save `00CB5AD0`? |
|---|---|---|---|
| `0049EAD1` | `0049EA40` | **`0049EAC0`** `push 1` / `ecx+0xAC` / `jmp 004B2890` | **0 inbound `E8`**. Dead stub |
| `0049F24E` | `0049EF50` | **`0049F180`** `"Init Quests"` | **Yes.** `[world+172]` TRUE names. Oakvale intern **absent** |
| `004B4A5A` | `004B49E0` | **`004B4A10`** 1-name wrapper | Only if a recovered `004B4A10` `E8` runs |
| `004B5B84` | `004B5080` | save `"END_ACTIVE_QUESTS"` arm | **not** no-save |
| `00892EAF` | `00892D80` | **`00892EA0`** vtbl `01260F0C` slot **277** | **0 `E8`**, **0 `ff` disp 1108** |
| `00892EEF` | `00892D80` | **`00892EE0`** slot **279** | **2 `ff` disp 1116**, both in `00CE7670` **after** the Oakvale wait, **other** names |

`004B4A10` always `00433530`s a 12-byte list then
`004B4A5A call 004B4260`. Its eight `E8`s
(`q-novi-activator-callers`):

| `E8` | Real fn | Name | No-save first-seen? |
|---|---|---|---|
| `00416C11` | `00416953` suffix | `[game+90584]` vs empty intern | **skip** (`je 00416C16`) |
| `00892E8F` | **`00892E80`** slot **276** | `user.ini` `"Gameflow"` | **yes**, after Init Quests |
| `00892ECF` | **`00892EC0`** slot **278** | same thunk shape, flags `(1,0)` | **not** the ini `(1,1)` take |
| `004B4B5F` / `004B4D45` | `004B4AA0` / sibling | thing `0x6C` `+40` | needs live thing |
| `0061AC28` | `0061A6A0` | debug `world+196` card | leftover UI |
| `007EF3A1` | `007EF200` | nested `CTCExpression+120` | needs `0x8F` |
| `0084407E` | `00843FC0` | action `+168` | needs thing |

**None** `push 0x012C5D14`. Do not invent a
ninth `004B4A10` caller.

---

## 3. Presenters besides Gameflow `user.ini`

All lookup traffic is `004B4260` → unique
`004B42E8`. Besides slot-276 `00892E80`
`"Gameflow"`:

**On this no-save walk, one recovered presenter
runs first:** Init Quests `0049F24E`
`004B4260([world+172])`. Names are the WLD
`AddQuest` TRUE vector (`Q_SunnyvaleMaster` …).
`Q_NewOakValeIntro` is FALSE → catalog
`world+184` / `QM+44` only. **PROVEN** omit.

`00416C11` is recovered and **does not** reach
`004B4260` on no-save (empty intern skip).

Everything else is **not first-seen** or **not
this name**:

- `0049EAC0`: 0 inbound.
- `00892EA0`: 0 inbound `E8` / `ff`.
- `00892EE0`: leftover fresco / `Q_GuildTraining`
  / `V_*` after `00CE7670` yield.
- `004B5B84`: save parse.
- Thing / action / `CTCExpression` / debug UI:
  need a live object. Runtime CString **equal**
  to intern `0x012C5D14` stays **UNKNOWN**
  (`00CB5AD0-remaining-presenters`). That is
  not a recovered no-save presenter.

`xrefs-by-string.tsv` intern `0x012C5D14`:
`00CD6E28` / `00CD6E87` bind, `00CE791E` card,
`00CE7978` / `00CE79CA` wait. **Zero** into
`00CB5AD0` / `004B4260` / `004B4A10`. **PROVEN**.

---

## 4. `00CB5C90` binds `S_QNOVI` on no-save

`e8.tsv` dest `0x00CB5C90`: **161**, every site
inside `"Registering Scripts"` `00CD52D0`.
`ff.tsv` dest **0**. Grouping parent
`00CD4FD0` is a **14-byte dtor** (`0099EAE0` /
`jmp 0099EAE0`) then int3. `xrefs-by-string`
`fn=0x00CD5170` is a **different** walker
(`sub esp, 28` / `jmp 00CD52C8`). Real
registrar:

```
00CD52CF  int3 (end of previous)
00CD52D0  sub esp, 24
          push "Registering Master Script"
00CD52E9  xor ebx, ebx               ; persist 0 default
00CD5328  mov edi, 1
00CD532D  mov ebp, 0xCDBD20          ; watcher thunk
          … Sunnyvale persist 1, factory 00CDD550 …
00CD5358  call 00CB5C90
```

Oakvale row (`listing-00cc0000.txt`):

```
00CD6E0F  push ecx / mov ecx, esp
00CD6E12  push -1
00CD6E14  push "S_QNOVI"             ; intern 0x012F789C
00CD6E19  call 0099EBF0
00CD6E20  call 00CB5AC0              ; CString dtor thunk, NOT bind
00CD6E25  push -1
00CD6E27  push "Q_NewOakValeIntro"   ; intern 0x012C5D14
00CD6E30  call 0099EBF0
          …
00CD6E48  push "S_QNOVI"
00CD6E4D  mov [esp+44], edi          ; 1
00CD6E51  mov [esp+48], bl           ; persist 0
00CD6E55  mov [esp+32], 0xDBEF70     ; factory
00CD6E5D  mov [esp+36], ebp          ; 00CDBD20
00CD6E6D  call 00CB5C90              ; BIND
00CD6E86  push "Q_NewOakValeIntro"
00CD6E9A  call 00CBFAB8              ; catalog, edx=0; still not 00CB5AD0
```

`00CB5C90` copies the record through `00CB7210`
(`[this+4]` 24-byte slots: names, factory dword,
persist byte) and `ret 8`. Calls:
`0099EC30` / `00414CE0` / `009F05A0` /
`00433530` / `00CB7210` / `0099EAE0`. **No**
`00CB5AD0`. **DISPROVEN** as lookup / ctor /
`00DAAC16`.

`00CB5AC0` is `lea ecx, [esp+4]` / `0099EAE0` /
`ret 4`. Dest has **156** `E8`s, all the same
registrar pattern. `00CD6E20` discards a temp
`S_QNOVI` CString. **DISPROVEN** as bind.

Persist `bl` is still 0 from `00CD52E9`. Only
Sunnyvale wrote `[esp+48], 1` in this fn.

---

## 5. When `00CD6E27` is reached

`00CD6E27` is **`push 0x012C5D14`**, not a
`call`. It runs only if `00CD52D0` runs.

Unique `E8` dest `00CD52D0`: **`00CB5E12`**.
That site is inside `00CB5D80`:

```
00CB5D80  push 0xD0 / 00BFEA1A / 00CD3F00 / 00CD3F40
          push "Registering Script Defs" …
          call 00F2A0F0
00CB5DE7  push "Registering Scripts"
00CB5E12  call 00CD52D0              ; UNIQUE
```

Unique `E8` dest `00CB5D80`: **`004A6677`**.
Grouping parent `004A3740` is another blob.
Int3 `004A654A` then:

```
004A6550  push "Init Atmos"
          …
          push "Init Scripts"
004A6661  call 00CB5C70              ; 32-byte table ctor
004A666A  lea edi, [esi+88]
004A6670  call 004AB370              ; store at world+88
004A6675  mov ecx, [edi]
004A6677  call 00CB5D80              ; UNIQUE
          … 004B4590 / 004A9A10 QM wire …
          push "Engine Set World"
```

No-save Init Game already entered `"Init World"`
before Init Display / Init Quests / `user.ini`
(`0042F491-init-game-callees`,
`004A67D0-after-particles`). This bind therefore
runs on **no-save and save**. It is **not** gated
on `world+172`.

Order on this walk:

```
004A6550  Init Atmos / Init Scripts
  004A6677  00CB5D80
    00CB5E12  00CD52D0
      00CD6E27  push intern              // HERE
      00CD6E6D  00CB5C90 bind S_QNOVI
004A0D90  FinalAlbion.qst AddQuest FALSE
0049F24E  004B4260([world+172])          // first 00CB5AD0
          names ≠ Q_NewOakValeIntro
user.ini  00892E80 004B4A10("Gameflow")  // second 00CB5AD0
first type-1 00CE7670 wait 0x33          // no 00CB5AD0
```

`00CD6E27` is therefore **reached on no-save**,
**before** any `00CB5AD0`, and **does not**
present the name to lookup.

---

## What this is not

| Claim | Class |
|---|---|
| `004B2510` owns `00CB5AD0` | **DISPROVEN** (int3 `004B4260`) |
| `00CD4FD0` / `00CD5170` own the Oakvale bind | **DISPROVEN** (dtor / other walker; real `00CD52D0`) |
| `004A3740` owns `004A6677` | **DISPROVEN** (int3 `004A6550`) |
| `00CD6E20` `00CB5AC0` binds `S_QNOVI` | **DISPROVEN** (dtor thunk) |
| `00CD6E27` is the bind `E8` | **DISPROVEN** (`push`; bind is `00CD6E6D`) |
| Bind constructs `S_QNOVI` / writes `00DABAC0` | **DISPROVEN** (`00DBEF70` / `00DAAC16` later) |
| Bind is a `00CB5AD0` presenter | **DISPROVEN** (0 `E8` between them) |
| Gameflow `user.ini` is the first `00CB5AD0` | **DISPROVEN** (Init Quests first) |
| Init Quests presents Oakvale | **DISPROVEN** (`world+172` omit) |
| `00CE7670` presents Oakvale to `00CB5AD0` | **DISPROVEN** (wait / card only) |
| Host `ActivateQuest("Q_NewOakValeIntro")` | **DISPROVEN** |

---

## Remaining UNKNOWN

1. First live thing / `CActivateQuestDef` intern
   **dword** equal to `0x012C5D14` after a region
   exists — leftover presenter, off this walk.
2. Whether `[QM+120]` is the same object as
   `world+88` (`00CB5C70` table). Lookup uses
   `[QM+120]`; bind uses the `00CB5D80` `this`.
   Pointer identity is **not** required to keep
   bind off the presenter list.

Until (1) shows a recovered CString, the no-save
presenter of `Q_NewOakValeIntro` to `00CB5AD0`
stays **nobody**. Bind already ran.

---

## Sources (absolute)

- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\calls-by-dest.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\ff.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00880000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00c80000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00cc0000.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\xrefs-by-string.tsv`
- `C:\FableCSharp\proofs\00CB5AD0-remaining-presenters\README.md`
- `C:\FableCSharp\proofs\00DAAC00-sqnovi-no-save\README.md`
- `C:\FableCSharp\proofs\q-novi-activator-callers\README.md`
- `C:\FableCSharp\proofs\ini-activate-quest\README.md`
