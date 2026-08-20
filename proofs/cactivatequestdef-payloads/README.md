# `CActivateQuestDef` game.bin payloads (six 16-byte rows)

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat these compiled-def rows as New Game autostart.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: TLC install
`C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters`
(`data\CompiledDefs\game.bin` / `names.bin` present);
`src/Fable.Formats/Defs/GameBin.cs` (`Parse` / `ParseEntry` /
`InflateZlib`); `tools/Fable.Dump/Program.cs` `DumpGameBinFamily`
(`writeParts` false for `game.bin`);
`assembly/compiled-defs/game/entries.tsv` ids **61 / 9241 / 9248 /
12277 / 12857 / 12874**; `names.tsv` `CActivateQuestDef` /
`NULLDEF_CActivateQuestDef`; ExeIndex
`listing-00840000.txt` `00843F50` / `00843FC0`;
`listing-00600000.txt` `00629979` / `00629A09`;
`listing-00780000.txt` `007B5680` / `007B5740` / `007B5AA4`;
`listing-007c0000.txt` `007EF66C` / `007F0232` / `007F0410`;
`listing-004c0000.txt` `004D8A32` / `004D5056` / `004F5B7D`;
`vtbl.tsv` `0x0123C7F4` slot 18; `strings.tsv` /
`xrefs-by-string.tsv`; siblings `proofs/008421C0-activate`,
`proofs/q-novi-activator-callers`, `proofs/qst-autostart-list`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Are the six rows in `game.bin`? | **Yes.** Type `CActivateQuestDef`, **raw 16**, subdefs **0**, dump ASCII **empty**. | **PROVEN** |
| Instance / source names? | **61** = `NULLDEF_CActivateQuestDef`. Other five: type name only; `fileOff` empty (`GuessInstanceName` falls back to `CActivateQuestDef`). | **PROVEN** |
| Exact 16-byte payloads? | **UNREAD.** Install present. `game.bin` body is zlib-1 chunks (`GameBin.Parse`). `Fable.Dump` does not write per-entry hex for `game.bin`. This pass cannot inflate (binary `read_file` rejected). | **UNREAD** |
| Six quest names from payloads? | **UNREAD.** | **UNREAD** |
| Does any payload intern `Q_NewOakValeIntro` (`0x012C5D14`)? | **Not as ASCII / names.bin.** Intern dword inside the 16 bytes: **UNREAD**. `names.bin` has **no** `Q_NewOakValeIntro`. `data\` ASCII hit is only `FinalAlbion.qst`. | **DISPROVEN** ASCII; intern-in-payload **UNREAD** |
| `00843F50` `E8` callers (6) and CString? | Table below. **None** push `0x012C5D14`. | **PROVEN** |
| Runtime def layout? | Factory `004D8A32` alloc **48**, ctor `004D5056` vtbl `0x0123C7F4`, `[+40]\|=-1`. Persist slot 18 `007B5740`: **`+40` intern CString**, **`+44` bool**. Action ctor copies that intern to `this+168`. | **PROVEN** |

---

## Verdict

The six compiled-def rows exist and are **16-byte / 0-field**.
Their **quest intern CStrings are not in `entries.tsv`**.

Without inflated hex, the six names stay **UNREAD**.
`Q_NewOakValeIntro` is **not** stored as names.bin text or as
a 4+ ASCII run in those rows. Whether any row’s intern **dword**
equals `0x012C5D14` is **UNREAD** until the 16 bytes are dumped.

`00843F50` itself is **not** an Oakvale literal: four sites intern
expression names; two sites copy **`[CActivateQuestDef+40]`**.

Do **not** add `ActivateQuest("Q_NewOakValeIntro")` in `src/`.

---

## 1. The six `entries.tsv` rows

Dump columns: `index type instance source mesh raw subdefs strings`.
`raw` is **length**, not hex. `strings` is `ExtractAscii` runs of
length ≥ 4.

| Id | Type | Instance | Source | Raw | Subdefs | ASCII |
|---:|---|---|---|---:|---:|---|
| 61 | `CActivateQuestDef` | `NULLDEF_CActivateQuestDef` | `NULLDEF_CActivateQuestDef` | 16 | 0 | *(empty)* |
| 9241 | `CActivateQuestDef` | `CActivateQuestDef` | *(empty)* | 16 | 0 | *(empty)* |
| 9248 | `CActivateQuestDef` | `CActivateQuestDef` | *(empty)* | 16 | 0 | *(empty)* |
| 12277 | `CActivateQuestDef` | `CActivateQuestDef` | *(empty)* | 16 | 0 | *(empty)* |
| 12857 | `CActivateQuestDef` | `CActivateQuestDef` | *(empty)* | 16 | 0 | *(empty)* |
| 12874 | `CActivateQuestDef` | `CActivateQuestDef` | *(empty)* | 16 | 0 | *(empty)* |

`names.tsv`:

| Offset | CRC | Name |
|---|---|---|
| `0x00007118` | `0xD75076C4` | `NULLDEF_CActivateQuestDef` |
| `0x00007136` | `0xFA5557F6` | `CActivateQuestDef` |

No `Q_NewOakValeIntro` row in `names.tsv`.

Neighbours (not payload names; **PARTIAL** clustering only):

| Id | Previous / next typed rows |
|---:|---|
| 61 | NULLDEF table (class 62 in `004F5B7D`) |
| 9241 | `CChestDef` 9240, `CActionUseDef` 9242 |
| 9248 | `CChestDef` 9247, `CActionUseDef` 9249 |
| 12277 | `CChestDef` 12276, then `CCameraCollisionDef` / `CActionUseDef` / `CBuyHouseDef` |
| 12857 | `CPhysicsDef` 12856, `CCarryableDef` 12858, `CInventoryItemDef` 12859 mesh **5643** |
| 12874 | `CPhysicsDef` 12873, `CCarryableDef` 12875, `CInventoryItemDef` 12876 mesh **5662** |

Those live rows sit on **chest / carryable / inventory** clusters,
consumed later by `007B5680` / `CTCCarriedActionUseActivateQuest`
(`007B57C0`). **DISPROVEN** as no-save autostart (`qst-autostart-list`).

---

## 2. Why hex is UNREAD this pass

`GameBin.Parse`: 13-byte header, 14761×12 name-refs, then zlib-1
chunks (`MaxChunkInflate` 32 KiB). Payload bytes live in inflated
chunk bodies, not the name-ref table.

`DumpGameBinFamily(..., parseFrontend: false, parseScript: false)`
writes only `entries.tsv` + type counts for `game.bin` — **no**
`####-name.md` hex dump.

Install **is** present (`compiled-defs/INDEX.md` same root).
`read_file` on `game.bin` returns “Cannot read binary file.”
No inflated hex of ids 61 / 9241 / 9248 / 12277 / 12857 / 12874
exists under `assembly/compiled-defs/game/`.

**Exact 16 bytes: UNREAD.** Do not invent them.

Likely on-disk layout (**PARTIAL**, not confirmed against bytes):
3-byte `GameBin` header + field CRC + intern u32 + field CRC +
bool u8 = **16**. Persist `007B5740` type-2 arm `0044FC00` reads
a **4-byte intern pointer** into `[def+40]`. If that model holds,
`Q_NewOakValeIntro` would appear as LE `14 5D 2C 01`, not ASCII.
Until hex exists, that test is **UNREAD**.

Field CRCs: `004F5B7D` registers the class with factory
`0x4D8A32` and **no** in-range `004D2EF0` named members.
Lionhead names of `+40` / `+44` stay **UNREAD**.

---

## 3. Runtime object (how ctor “copies interned name from def”)

```
004D8A32  push 48
          call 00BFEA1A
          jmp 004D5056

004D5056  call 0044C0C0              ; CDef base
          mov [esi], 0x0123C7F4
          or [esi+40], -1
          ret
```

vtbl `0x0123C7F4` slot 18 = persist `007B5740`:

```
007B5740  lea ebx, [edi+40]          ; intern CString
          call 00404500              ; type [stream+24]
            2 → 0044FC00             ; 4-byte intern → [+40]
            3 → 00993EB0
          add edi, 44                ; bool
          call 00404500
            2 → 00403EB0
            3 → 00993E30             ; byte [+44]
```

Lookup `007B5680` (`push "CActivateQuestDef"`) returns that 48-byte
def. Use-item `007B57C0` / `007EF600` then:

```
009D49B0(0x13CA828, [def+40])     ; intern copy
00843F50(..., CString*, [def+44])
006644F0                          ; queue action
```

`00843FC0` (vtbl `0x012752C4` slot 12) calls
`004B4A10([this+168], 0, this+172)`.

`007B5820` compares `[def+40]` to `"Global_TeleportToHeroGuild"`
on the **carried-use** path. That is a runtime compare, **not**
a recovered payload string. Do not list it as a dumped name.

---

## 4. `00843F50` `E8` callers (listing `call 00843F50` = **6**)

Ctor `ret 16`: arg1 thing, arg2 unused-here, arg3 `CString*`,
arg4 bool → `this+168` / `this+172`.

| Site | Parent | CString intern | Arg4 |
|---|---|---|---|
| `00629979` | `00629930` | `"Expression_Follow"` **`0x01259170`** | `0` |
| `00629A09` | `006299C0` | `"Expression_Wait"` **`0x01259184`** | `0` |
| `007B5AA4` | `007B57C0` `CTCCarriedActionUseActivateQuest` | **`009D49B0([def+40])`** — not a literal | `[def+44]` |
| `007EF66C` | `007EF600` | **`009D49B0([def+40])`** — not a literal | `[def+44]` |
| `007F0232` | `007F01F0` | `"Expression_Fish"` **`0x012718F8`** | `0` |
| `007F0410` | `007F03D0` | `"Expression_Dig"` **`0x01271908`** | `0` |

`strings.tsv` stores `0x0125916F` as `AExpression_Follow`
(previous byte); listing / push immediate is **`0x01259170`**
`"Expression_Follow"`.

**DISPROVEN:** none of the six `E8` sites push `0x012C5D14`.
`xrefs-by-string.tsv` `Q_NewOakValeIntro` remains
`00CD6E28` / `00CD6E87` (bind) and `00CE791E` / `00CE7978` /
`00CE79CA` (Gameflow wait).

---

## 5. `Q_NewOakValeIntro` vs these rows

| Store | Has the name? |
|---|---|
| `names.bin` | **no** |
| `entries.tsv` ASCII | **no** (empty `strings`) |
| `data\` grep ASCII | `FinalAlbion.qst` only (`AddQuest` FALSE / `AddTestQuest`) |
| `00843F50` immediates | **no** |
| Inflated intern u32 in the six 16-byte bodies | **UNREAD** |
| No-save `004B4A10` | **not** this class (`q-novi-activator-callers`) |

---

## Host

No `src/` change. Do not add `ActivateQuest("Q_NewOakValeIntro")`.

Next dump (when a runner can `GameBin.Load`): print
`Convert.ToHexString(Entries[id].Raw)` for the six ids and
resolve any intern u32 via `strings.tsv`. That is the only
missing step for the six names.
