# Who calls `004B4A10` with `Q_NewOakValeIntro` (no-save New Game)

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
`00CD6E27` is bind-only. `00CE7670` only waits.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `listing-00400000.txt` `00416BCF` / `00416C11` /
`00419DAE`; `listing-00480000.txt` `0049D770` / `004A0D90` /
`004A0E7D` / `004A113B` / `004B2890` / `004B4A10` / `004B4B5F` /
`004B4D45` / `004B5080`; `listing-004c0000.txt` `004D4B72` /
`004D4B9B` / `004D57FD` / `004DB06C` / `004DB085` / `004F5B7D`;
`listing-00600000.txt` `0061AC28` / `00629979`;
`listing-00780000.txt` `007B5590` / `007B5680` / `007B5AA4`;
`listing-007c0000.txt` `007EEF60` / `007EF200` / `007EF3A1` /
`007EF600`; `listing-00840000.txt` `008421C0` / `00843F50` /
`00843FC0` / `0084407E`; `listing-00880000.txt` `00892D80` /
`00892E80` / `00892EC0`; `listing-00cc0000.txt` `00CD6E27` /
`00CE791D`; `00-index/strings.tsv` / `xrefs-by-string.tsv` /
`vtbl.tsv` / `rtti.txt`; `01-sections/text-map/calls-by-dest.tsv`
(`004B4A10` eight `E8`); `crc.tsv`; compiled-defs
`CActivateQuestDef` (6); `proofs/ini-activate-quest`,
`addtestquest-token`, `qst-first-load`, `004B2890-empty-first`;
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`.

---

## Verdict

**Nobody** on the recovered `004B4A10` `E8` set pushes the
intern `0x012C5D14` `"Q_NewOakValeIntro"`.

The two leftover grouping parents from `calls-by-dest.tsv`
(`007EEF60` / `008421C0`) are **not** the functions that
contain those calls. Real sites:

| `E8` | Real fn (int3-bounded) | Name arg to `004B4A10` |
|---|---|---|
| `007EF3A1` | **`007EF200`** `CTCExpression` vtbl `0x0124026C+28` | copy of nested `[esi+120]` via `00415DD0`; **not** a literal |
| `0084407E` | **`00843FC0`** `CCreatureAction_ActivateQuest` vtbl `0x012752C4+12` | `[this+168]` CString; **not** a literal |

No-save New Game still does **not** activate `Q_NewOakValeIntro`.
Host already Notes the skip / Gameflow wait and does **not**
call `ActivateQuest("Q_NewOakValeIntro")`. **No src DIVERGE.**

Remaining **UNKNOWN**: which later thing / `CActivateQuestDef`
instance first supplies that name to `004B4A10` **after** a
region exists. That is off the no-save walk.

---

## Status table

| Claim | Class | Evidence |
|---|---|---|
| Eight `E8` of `004B4A10` | **PROVEN** | `calls-by-dest.tsv`: `00416C11`, `004B4B5F`, `004B4D45`, `0061AC28`, `007EF3A1`, `0084407E`, `00892E8F`, `00892ECF` |
| `functions.tsv` parent `007EEF60` owns `007EF3A1` | **DISPROVEN** | `007EEF60` is a 27-byte dtor (`004DB004` / `00BFE9BC` / `ret 4`). Call is in `007EF200` (starts `sub esp, 0x100`) |
| `functions.tsv` parent `008421C0` owns `0084407E` | **DISPROVEN** | `008421C0` is 18 bytes then `jmp 00694430`. Call is in `00843FC0` |
| Same grouping artifact as `004B2510` vs `004B4260` | **PROVEN** | Unique `00CB5AD0` `E8` is `004B42E8` inside `004B4260` |
| `007EF3A1` / `0084407E` pass `"Q_NewOakValeIntro"` | **DISPROVEN** | No `push 0x012C5D14` in either fn. Names are runtime CStrings |
| `007EF200` class | **PROVEN** | ctor `004DB085` writes vtbl `0x0124026C`; type-id `004DB06C` returns `0x8F`; name intern `004D4B72` `"CTCExpression"` |
| `00843FC0` class | **PROVEN** | ctor `00843F50` writes vtbl `0x012752C4`; RTTI `0x01384F40` `CCreatureAction_ActivateQuest`; slot 12 = `00843FC0` |
| `004B4A10` signature | **PROVEN** | `thiscall` `[0x13B89FC]`, `ret 12`: `(CString* name, flag, flag)` → `00433530` 12-byte list → `004B4260` |
| `00416C11` name | **PROVEN** | `[game+90584]` vs empty intern `0x122D70E`. No-save: `0099E960` equal → `je 00416C16` **skip**. Would not be Oakvale even if taken |
| `004B4B5F` / `004B4D45` name | **PROVEN** | Thing component `0x6C` record `+40` CString. Debug / use-item path inside `004B49E0` |
| `0061AC28` name | **PROVEN** | Quest-selection UI `0061A6A0`: `esi` = `AddTestQuest` `world+196` record; empty `+24` card → `004B4A10(1,1, record)`. Leftover picker, not first no-save |
| `00892E8F` name | **PROVEN** | `00892E80` vtbl `0x01260F0C+276` (`+1104`). `user.ini` `ActivateQuest("Gameflow")` only after Leave |
| `00892ECF` name | **PROVEN** | Sibling `00892EC0` same name ptr, flags `(1,0)` |
| String xrefs of `Q_NewOakValeIntro` | **PROVEN** | `xrefs-by-string.tsv`: `00CD6E28` / `00CD6E87` bind `S_QNOVI`; `00CE791E` / `00CE7978` / `00CE79CA` Gameflow **wait**. No `ActivateQuest` push |
| `00CE7670` activates Oakvale | **DISPROVEN** | `vtbl+1180` card bind then `vtbl+100` (`00892F60` / `004AF610`) **IsActive**. Yields while 0 |
| `0049D770` parses QST / activates | **DISPROVEN** | Path join `0x01238C40` `Data\Levels\` + WLD stem + `.qst`. No `004B4A10` |
| `004A0D90` `AddTestQuest` activates | **DISPROVEN** | `004A113B` `push_back` `world+196` only. See `addtestquest-token` |
| `004B2890` Init Quests activates Oakvale | **DISPROVEN** | First no-save `0049F259`: empty `QM+112` → `je 004B2989`. No `004B4A10`. See `004B2890-empty-first` |
| `004B5080` `START_NEW_QUEST` on no-save | **DISPROVEN** | 0 external `E8`. Internal xrefs `004B50C1` / `004B50F6` / `004B527D` are save parse |
| `CActivateQuestDef` string xrefs | **PROVEN** | `004F5B7E` registrar; `007B5594` type-name setter; `007B5688` lookup (`007B5680`) |
| `CActivateQuestDef` game.bin | **PARTIAL** | 6 unnamed 16-byte defs (`entries.tsv` 61 / 9241 / 9248 / 12277 / 12857 / 12874). Field names **UNREAD** |
| `CActivateQuestDef` → `004B4A10` | **PROVEN** path, **DISPROVEN** Oakvale literal | `007B5680` lookup → `00843F50` (`[def]` intern + `[def+44]` bool) → queue `006644F0` → run `00843FC0` → `004B4A10([this+168])` |
| `007EF200` name source | **PROVEN** | Component `0x8F` (`CTCExpression`); `[ebp+12]` nested; non-empty `[esi+120]` vs `0x122D70E`; `[esi+124]` third arg. `[esi+116]` set → camera `0041649C` **instead** of activate |
| `00843F50` always a quest name | **DISPROVEN** | Same ctor also pushed `"Expression_Follow"` (`0062995D`) / `"Expression_Wait"` (`006299ED`) |
| Script bytecode opcode activates Oakvale | **DISPROVEN** as recovered opcode stream | Native vtbl `01260F0C+276..278`. Empty-name arm `00892D80` uses `vtbl+52`/`+48` (`004FC210`/`004FB490`) **index query**, not `004B4A10` |
| `FableCrc("Q_NewOakValeIntro")` immediate in `.text` | **UNREAD** | `crc.tsv` has no string; activation is CString intern `0x012C5D14`, not a name CRC. `names.tsv` CRC `0xFA5557F6` is **`CActivateQuestDef`**, not the quest |
| `strings.tsv` `ActivateQuest` | **PROVEN** | `0x0122F380` → `00419DAF` `fn=00419D90` registrar only |
| `strings.tsv` `AddTestQuest` | **PROVEN** | `0x01238E98` → `004A0E93` / `004A1128` in `004A0D90` |
| `strings.tsv` `START_NEW_QUEST` | **PROVEN** | `0x012394B0` → `004B50C1` / `004B50F6` / `004B527D` in `004B5080`; spaced `0x012393CE` has 0 xrefs |
| Host invents `ActivateQuest(Q_NewOakValeIntro)` | **DISPROVEN** | `EngineLifecycle.InitCharactersAndQuests` Notes skip; `ActivateNamedQuest` walks `world+172` only; test `No_save_does_not_activate_Q_NewOakValeIntro` |
| Host missing Note of a recovered no-save caller | **DISPROVEN** | New callers are thing / expression / action / debug UI. Not on no-save before first region |

---

## 1. `004B4A10` itself

```
004B4A10  sub esp, 12
          push 1; push 1
          mov ecx, [esp+36]          ; arg0 CString*
          lea eax, [esp+36]
          push eax; push ecx; push 0
          call 00433530              ; 12-byte name list
          call 004B4260              ; Init Quests walk
          ret 12
```

**PROVEN.** Every caller is a name + two flags into the same
bulk activator. Unique `00CB5AD0` remains `004B42E8` inside
`004B4260`.

---

## 2. `007EF3A1` — `007EF200` `CTCExpression`, not `007EEF60`

`007EEF60`:

```
007EEF60  push esi
          mov esi, ecx
          call 004DB004              ; CTCExpression dtor body
          test [esp+8], 1
          je 007EEF78
          push esi
          call 00BFE9BC
          ...
          ret 4
```

`007EF200` (thing `edi`, `this` expression component):

```
007EF2C7  mov ebx, [this+4]
          ... component 0x8F on ebx ...
007EF303  mov esi, [ebp+12]
007EF36B  lea ebx, [esi+120]
007EF36E  push 0x122D70E
          call 005FA740              ; non-empty?
007EF382  lea edx, [esp+16]
          call 00415DD0              ; copy [esi+120] → stack CString
007EF390  mov al, [esi+124]
          lea ecx, [esp+16]
          push eax
          push 0
          push ecx
          mov ecx, [0x13B89FC]
007EF3A1  call 004B4A10
```

Name is **whatever** the nested expression object stored at
`+120`. **PROVEN** not the Oakvale intern.

---

## 3. `0084407E` — `00843FC0` action run, not `008421C0`

`008421C0`:

```
008421C0  mov al, [esi+98]
          test al, al
          jne 008421CF
          call [vtbl+12]
008421D2  jmp 00694430
```

`00843F50` ctor writes vtbl `0x012752C4` and copies arg CString
to `+168`, bool to `+172`.

`00843FC0` run:

```
00844066  mov ecx, [0x13B89FC]
          xor edx, edx
          mov dl, [edi+172]
          lea eax, [edi+168]
          push edx
          push 0
          push eax
0084407E  call 004B4A10
```

`00843F50` `E8` sites include `007EF66C` / `007B5AA4`
(`CActivateQuestDef` lookup) **and** `00629979`
(`"Expression_Follow"`). The `+168` slot is a generic name
string. **DISPROVEN** as a hardcoded Oakvale activate.

---

## 4. Other `004B4A10` names (complete `E8` set)

```
00416C11  [game+90584]     // empty skip on no-save
004B4B5F  [comp 0x6C + 40]
004B4D45  copy of same +40, flags (1,1)
0061AC28  picker record esi from world+196
00892E8F  script/ini CString, (1,1)   // user.ini "Gameflow"
00892ECF  same CString, (1,0)
```

**PROVEN** none of these push `0x012C5D14`.

---

## 5. QST / Init Quests / `CActivateQuestDef` / script

`0049D770` — WLD stem → `Data\Levels\FinalAlbion.qst`. **PROVEN**
path only (`qst-first-load`).

`004A0D90` — token walk. `AddQuest` → `+184` / TRUE `+172` /
`004B2850`. `AddTestQuest` → `004A113B` `+196`. Oakvale is
`AddQuest(..., FALSE)` plus one test card. **PROVEN.**

`004B2890` — first no-save is empty `QM+112` skip. **PROVEN.**

`CActivateQuestDef` — class register `004F5B7D`; lookup
`007B5680` same pattern as `CSmashableDef` `007EEEA0`. Six
unnamed game.bin rows. Runtime activate is
`007B5680` → `00843F50` → `00843FC0` → `004B4A10`. Def
payload that would equal `Q_NewOakValeIntro` is **UNREAD**.

Script manager vtbl `0x01260F0C`:

| Slot | Off | VA | Role |
|---:|---:|---|---|
| 276 | 1104 | `00892E80` | `004B4A10(name,1,1)` |
| 277 | 1108 | `00892EA0` | `004B4260(name,1,1)` |
| 278 | 1112 | `00892EC0` | `004B4A10(name,1,0)` |
| 284 | 1136 | `00892F40` | name-is-active (`004AF610`) |
| 286 | 1144 | `00892F60` | thing-has (`004B0FC0`) |

`00892D80` empty-intern arm: `vtbl+52` (`004FC210`) then
`vtbl+48` (`004FB490`) then `[eax+85]`. **Index query**, not
activate-by-CRC. Gameflow uses slot 286 / 25 (`vtbl+100`)
**wait**. **PROVEN.**

---

## 6. `strings.tsv` hits

| String | VA | `.text` xrefs |
|---|---|---|
| `ActivateQuest` | `0x0122F380` | `00419DAF` (`00419D90` bind → handler `00419CE0`) |
| `AddTestQuest` | `0x01238E98` | `004A0E93`, `004A1128` |
| `START_NEW_QUEST ` (trailing space) | `0x012393CE` | **none** |
| `START_NEW_QUEST` | `0x012394B0` | `004B50C1`, `004B50F6`, `004B527D` |
| `CActivateQuestDef` | `0x01243E40` | `004F5B7E`, `007B5594`, `007B5688` |
| `CTCActionUseActivateQuest` | `0x0123D2C4` | `004D57FE`, `006ACAE8`, `006AE6BC` (type-name / predicate, not `004B4A10`) |
| `Q_NewOakValeIntro` | `0x012C5D14` | bind + Gameflow wait only |

---

## Timeline (no-save) — still no Oakvale `004B4A10`

```
004A0D90  FinalAlbion.qst
  AddQuest FALSE Q_NewOakValeIntro → +184, QM+44
  AddTestQuest → +196 only
0049F24E  004B4260([world+172])     // Q_SunnyvaleMaster …
0049F259  004B2890                  // empty +112 skip
00416BCF  +90584 empty skip 004B4A10
user.ini  00892E80 004B4A10 "Gameflow"
00CE7670  00892F60 Q_NewOakValeIntro = 0 → yield
007EF200 / 00843FC0                 // need a live thing; not here
```

---

## Host

`EngineLifecycle.cs` Notes `00416BCF` skip and
`"004B4A10 not Q_NewOakValeIntro"`. `ActivateNamedQuest` only
walks `world+172`. No `ActivateQuest("Q_NewOakValeIntro")`.
**MATCH.** Do not add a recovered-caller Note that pretends
`007EF200` / `00843FC0` ran on no-save.

---

## Next dump sites (UNKNOWN remains)

1. **game.bin `CActivateQuestDef` payloads** — six 16-byte
   unnamed rows. Dump interned CString / `+44` bool. Check
   whether any name is `Q_NewOakValeIntro`.
2. **First thing with `CActivateQuestDef` or `CTCExpression+120`
   after a region load** — `007B5680` / `007EF200` need
   `[thing+145]` live. Not `00501450`. Likely Oakvale West
   after `006B3FF0`, not Init Game.
3. **`FableCrc("Q_NewOakValeIntro")` value** then `crc.tsv` /
   `.text` imm hunt. Activation recovered here is CString, so
   a CRC hit would be a **different** table, not `004B4A10`.
4. **`00843F50` remaining `E8`** — `007F0232` / `007F0410` /
   `007B5AA4` name setup (listing-007c0000 / 00780000). Confirm
   none intern `0x012C5D14`.
5. **Save `004B5080` `START_NEW_QUEST` operand** — 0 no-save
   `E8`; dump only if a save game is in scope.

Until (1)–(2) show a live name equal to `Q_NewOakValeIntro`,
the no-save activator stays **UNKNOWN** and must not be
invented.
