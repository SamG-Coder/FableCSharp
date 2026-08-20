# Gameflow state 0 wait predicate — `vtbl+100` / `00893570`

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat wait-success as Guild / Lookout / `HeroGuildComplex`.
Do **not** set `SharedRun+4=1` or named index 1 as Guild.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: ExeIndex
`assembly/exe/01-sections/text-map/listing-00cc0000.txt`
(`00CE7670` / `00CE77D7` / `00CE7A02` / `00CE7FFB` /
`00CEF016` / `00CEF054` / `00CEF068`),
`listing-00880000.txt`
(`00893570` / `00893610` / `008ABED0` / `00892F40` /
`00892F60` / `00892EE0` / `0088E2A0` / `008A9E30`),
`listing-00480000.txt` (`004AF3C0` / `004AF610` / `004B0FC0` /
`004B4260`),
`listing-006c0000.txt` (`006E7510` / `006E7530` / `006E7410`),
`listing-00c80000.txt` (`00CB7940`),
`assembly/exe/00-index/vtbl.tsv` `0x01260F0C`,
`switch-ptrs.tsv` / `switch-index.tsv`;
siblings `proofs/gameflow-oakvale-wait`,
`proofs/script-gameflow`,
`proofs/quest-manager-plus44`.

---

## Verdict

**Wait succeeds when `[esi+64].vtbl+100` (`00893570`)
returns `al=1` for `"Q_NewOakValeIntro"`.** That is a
type-`0x33` script-object lookup plus **name compare**
against `QM+44[index]`. It is **not** `004AF610`
(`QM+56` already-constructed) and **not** `004B0FC0`
(thing-has).

Host / older notes call this `00893610`. Dump:
`call [edx+0x64]` is **slot 25 = `00893570`**.
`00893610` is slot 26 (`vtbl+104`), the copy-out
sibling. First-seen both miss at `008ABED0`. **PROVEN**
slot; host constant **DIVERGE**.

After a hit, the fiber is **still state 0**
(`[SharedRun+4]` stays `0` / `OV_INTRO`). First
actions are fresco `004B4260` + story-log 30 +
gossip bind. **Not** a region change. **Not** a
cutscene. Named Gameflow state 1 (`GUILD_TRAINING`)
is native **`SharedRun+4==0x64`**, not `==1`.
`==1` is jump-table default `00CEF016` **ret**.
**DISPROVEN** as skip-to-Guild.

Resume `00A44880` / `009D87F0` re-runs only the
wait loop. Quest still miss → yield. Does **not**
construct `Q_NewOakValeIntro`. **PROVEN**.

| Question | Answer | Class |
|---|---|---|
| Wait call? | `[esi+64].vtbl+100` `FF 52 64` | **PROVEN** |
| Native dest? | `0x01260F0C` slot 25 **`00893570`** | **PROVEN** |
| Host `QuestIsActiveFn=00893610`? | slot 26 GET sibling | **DIVERGE** |
| `004AF610` / `00892F40`? | `vtbl+1136` `QM+56` name | **DISPROVEN** as this wait |
| `004B0FC0` / `00892F60`? | `vtbl+1144` thing-has (Barrow) | **DISPROVEN** as this wait |
| Success predicate? | type `0x33` in `[iface+4]+96` **and** `004AF3C0` CString **equals** `Q_NewOakValeIntro` | **PROVEN** |
| First-seen / resume? | `008ABED0=0` → invert → `006E7410` yield | **PROVEN** |
| Construct Oakvale here? | no | **DISPROVEN** |
| After hit: state 1 Guild? | no; still `+4=0`; fresco then maybe fall to `0x64` | **DISPROVEN** skip |
| `SharedRun+4==1`? | `00CEF068[1]=4` → `00CEF016` ret | **DISPROVEN** as Guild |
| Guild arm? | `+4==0x64` `00CE7FFB` wait `Q_GuildTraining` then `HeroGuildComplex` | **PROVEN** later |

---

## Timeline (no-save New Game)

```
user.ini ActivateQuest("Gameflow")     // not Oakvale
first type-1 00CB8220
  Gameflow Main 00A44880 → 00CE7640 → 00CE7670
    attach CoreQuestReminder / CheckBarrowFieldsGuards
    [esi+68]+4 == 0 → 00CE77D7
      mov [ecx+4], 0                   // stay OV_INTRO
      tattoo / 00CBE87F(10) / 00896A30 card
      push "Q_NewOakValeIntro"
      [edx+100] 00893570 → al=0
      neg/sbb/inc bl=1
      je 00CE7A02 not taken
      [edx+28] 006E7410 → 00A44840 → 009D8650
later type-1
  00A44880 / 00A44660 / 009D87F0
  00893570 still 0 → yield
  no re-attach; no 00CB5AD0
```

`00DABAC0` / `00DBDE40` / `S_QNOVI` are **not** on
this list. **PROVEN**.

---

## 1. Wait site (`00CE7977` / `00CE79C9`)

`listing-00cc0000.txt` state 0 (`00CE77D7`):

```
00CE7977  push "Q_NewOakValeIntro"
00CE7995  call [edx+100]            ; vtbl+0x64
00CE7998  mov bl, al
00CE799A  neg bl
00CE799C  sbb bl, bl
00CE79A5  inc bl                    ; hit→0; miss→1
00CE79AE  je 00CE7A02               ; skip wait if already 1
00CE79B0  call [edx+28]             ; 006E7410 yield
00CE79BA  call 00CB7940
00CE79C1  jne 00CEF016
00CE79C9  push "Q_NewOakValeIntro"
00CE79E7  call [eax+100]            ; same predicate
00CE7A00  jne 00CE79B0              ; still miss → yield
00CE7A02  … post-wait (still +4==0) …
```

Invert: `al=0` → `bl=1` → enter / stay in wait.
`al=1` → `bl=0` → `00CE7A02`. **PROVEN**.

`vtbl.tsv` `0x01260F0C` (Init Scripts iface):

| Slot | Off | Dest |
|---:|---:|---|
| 7 | +28 | `006E7410` yield |
| **25** | **+100** | **`00893570`** |
| 26 | +104 | `00893610` |
| 279 | +1116 | `00892EE0` → `004B4260(list,1,0)` |
| 284 | +1136 | `00892F40` → **`004AF610`** |
| 286 | +1144 | `00892F60` → **`004B0FC0`** |
| 11 | +44 | `0088E2A0` region (`004FB880`) |
| 721 | +2884 | `008A9E30` → `008AE810` |

Barrow uses `00892F60` / `00892F40`. Gameflow Oakvale
wait does **not**. **PROVEN**.

---

## 2. Native condition (`00893570`)

`listing-00880000.txt`:

```
00893570  sub esp, 20
          call 006E7510             ; QM+136 context +24
          dec edi
          call 006E7530             ; same chain +28
          mov ecx, [esi+4]
          mov ecx, [ecx+96]         ; object list
          mov [esp+12], 0x33        ; type 51
          call 008ABED0
          je  00893600              ; al=0
          mov eax, [eax+60]         ; catalog index
          mov ecx, [0x13B89FC]
          call 004AF3C0             ; QM+44[index]
          ; compare [slot] CString to arg
          call 00411570             ; memcmp if not intern-eq
          mov al, 1                 ; names equal
00893600  xor al, al                ; miss or name mismatch
```

`004AF3C0` is **index** into `QM+44/+48` (every
`AddQuest` name). It is **not** a walk of `QM+56`.

`004AF610` (`listing-00480000`): walk `[this+56]`
comparing `[node+8]+48`. That is “already constructed
on the factory list”. **Different predicate.**

`004B0FC0`: walk things, bit `+145` / `ch&0x10`,
name at a `0x6C` node. Barrow / trader. **Different.**

`00893610`: same `008ABED0` type `0x33`, then
`0099EFB0` **copy** into the arg; `al=1` on any hit
(no name compare). Wait does **not** call it.
**PROVEN**.

Success therefore needs **all** of:

1. `[iface+4]+96` list has a type-`0x33` object in the
   `006E7510`/`006E7530` range (`008ABED0` ≠ 0).
2. `[found+60]` is a valid `QM+44` index (`004AF3C0`
   not the empty sentinel `0x13BD804`).
3. That CString **equals** `"Q_NewOakValeIntro"`.

First-seen / parked resume: step 1 fails
(`008ABED0=0`). `Q_NewOakValeIntro` is already in
`QM+44` from QST `AddQuest(..., FALSE)` — membership
is **not** enough. `004AF610` becoming true after a
later `004B4260` of that name is **UNREAD** as a
guarantor of the type-`0x33` object. Do not assume
`ActivateQuest` unblocks this wait.

Who first writes a type-`0x33` whose index names
Oakvale is **UNREAD**. Not this tick.

---

## 3. Jump table vs named “state 1”

`[esi+68]+4` (`SharedRun+4`), `cmp eax, 0xC8` then
`movzx` `00CEF068` / `jmp [00CEF054+edx*4]`.

| `+4` | Index | Dest | Role |
|---:|---:|---|---|
| 0 | 0 | `00CE77D7` | OV_INTRO wait Oakvale |
| **1** | **4** | **`00CEF016`** | **ret** (default) |
| 2…99 | 4 | `00CEF016` | ret |
| **100 (`0x64`)** | **1** | **`00CE7FFB`** | **GUILD_TRAINING** |
| 150 | 2 | `00CE88BA` | later |
| 200 | 3 | `00CE8A9A` | later |

`QuestFactoryTable.GameflowStateNames[1]` =
`"GUILD_TRAINING"` is the **name** for the `0x64`
arm, **not** for `+4==1`. Host `GameflowState=1`
as that arm is **DIVERGE**.

`00CE77D7` writes `[ecx+4], ebp` (`0`) every entry.
Wait-success does **not** bump `+4`. **PROVEN**.

`00CEF016` is the epilogue: `pop`s + `add esp, 0x824`
+ `ret`. No region. No cutscene. **PROVEN**.

---

## 4. After wait succeeds — still state 0

`00CB7940` (`listing-00c80000`): `[this+44]` then
`[eax+5]`. `00CB7950` stores the **current watcher**
at factory `+44`. `00CE7640` writes watcher `+5=1`
**after** `00CE7670` returns. During the wait-success
fallthrough `00CB7940` is **0**, so `jne 00CEF016`
is not taken. **PROVEN** as abort-if-already-done,
not “hero exists” for this site.

First actions at `00CE7A02` (still `+4==0`):

```
00CE7A11  Hook_Fresco_07_OakValeRaid     0044BFF0
00CE7A4F  Hook_Fresco_09_TimePassing     0044BFF0
00CE7A7F  Hook_Fresco_10_UneasyAlliance  0044BFF0
00CE7AAF  Q_GuildTraining                0044BFF0
00CE7AE7  [edx+1116] 00892EE0            004B4260(list,1,0)
00CE7AFB  00CBE87F(0x1E)                 TEXT_QST_LOG_STORY_30
          TEXT_AI_GOSSIP_GUILD_TRAINING_GUILD / GUILD_TRAINING
00CE7BC8  [edx+2884] 008A9E30            VILLAGE_GUILD_COMPLEX_INSIDE
                                         + GUILD_TRAINING  (map bind)
          … more 0044BFF0 names …
00CE7FEC  [edx+1116] 00892EE0            004B4260 again
00CE7FFB  ← fall into jump-slot 1 body
```

`00892EE0` is `push 0,1` then `004B4260`. That can
construct `Q_GuildTraining` **as a name on a list**.
It is **not** `004FB880` / `0088E2A0` region current.
It is **not** `00CBFB7D` / `CS_*` cutscene.
`VILLAGE_GUILD_COMPLEX_INSIDE` is `008A9E30` →
`008AE810` (script-state map), same family as
`00CE6CF0` `OV_INTRO` inserts. **DISPROVEN** as
region change.

No `PlayAVI` here (that was state-0 **entry**
`vtbl+2664`). No `00DBDE40`. **PROVEN** omit.

Then the Guild **arm** (`00CE7FFB`) finally writes
the numeric state:

```
00CE7FFB  mov ecx, [esi+68]
00CE8000  mov [ecx+4], 0x64
00CE8007  push "Q_GuildTraining"
00CE8025  call [edx+100]            ; 00893570 again
          miss → yield
00CE80A3  Hook_Fresco_10_UneasyAlliance  vtbl+1120
00CE80D4  00CBE87F(0x32)            TEXT_QST_LOG_STORY_50
00CE80E0  push "HeroGuildComplex"
00CE80FE  call [eax+44]             ; 0088E2A0 region wait
```

**First Guild-arm action is write `0x64` then wait
`Q_GuildTraining`.** Region wait on
`HeroGuildComplex` is **after** that wait hits.
Must **not** skip there from Oakvale wait-success
or from named index 1. **PROVEN**.

---

## 5. Host vs native

| Host | Native | Class |
|---|---|---|
| `QuestIsActiveFn=00893610` | wait is `00893570` `vtbl+100` | **DIVERGE** |
| `00893610 name 0` note | first-seen `008ABED0` miss (both) | **PROVEN** miss |
| `GameflowState=0` | `SharedRun+4=0` `00CE77D7` | **MATCH** |
| `GameflowStateSlots[1]=GUILD_TRAINING` | name string **PROVEN**; numeric `+4=1` **DISPROVEN** | **PARTIAL** |
| Invent `ActivateQuest(Q_NewOakValeIntro)` to leave yield | `00893570` needs type-`0x33`+name; `004AF610` is other slot | **DISPROVEN** |
| Jump to Guild / Lookout / `HeroGuildComplex` on activate | still `+4=0`; fresco / log 30 first; region wait is later `0x64` | **DISPROVEN** skip |

---

## Classifications (short)

1. **Wait predicate — PROVEN `00893570` (`vtbl+100`):
   type-`0x33` `008ABED0` hit **and** `004AF3C0`
   name equals `Q_NewOakValeIntro`.** Miss →
   `006E7410` / `009D8650`. Resume same miss.
2. **`00893610` / `004AF610` / `004B0FC0` as this
   wait — DISPROVEN.** GET sibling / `QM+56` /
   thing-has. Host `00893610` label **DIVERGE**.
3. **Construct / activate Oakvale at this site —
   DISPROVEN.** Wait only.
4. **After hit / “state 1” — DISPROVEN as Guild
   skip.** Still `SharedRun+4=0`. First work is
   fresco `00892EE0`/`004B4260` + `TEXT_QST_LOG_STORY_30`
   + gossip bind. `+4==1` is `00CEF016` ret.
   Guild is `+4==0x64` `00CE7FFB`: wait
   `Q_GuildTraining`, **then** `HeroGuildComplex`.
   No cutscene. No region change at wait-success.
