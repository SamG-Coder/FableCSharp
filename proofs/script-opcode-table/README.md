# First 20 native script tokens (`exe-commands`) vs `ScriptCommandMap`

Investigation only. No production `src` edits.

Do **not** treat table order as first-seen after Leave.
`0042F2A2` Leave / Init Game runs **zero** `00CBFB7D` verbs.
Do **not** start at `CS_OAKVALE_INTRO_FATHER` / `00DB86B0`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **INVENTED**.

Sources:

- dump `tools/Fable.ExeIndex/out/01-sections/script-bank/exe-commands.md`
  (ASCII `0x012C1500`–`0x012C2C00`; family is **script-bank**, not
  `script-runtime/`)
- dump `tools/Fable.ExeIndex/out/01-sections/script-runtime/`
  (token / apply sites for recovered verbs)
- `src/Fable.Game/ScriptCommandMap.cs` (`NativeTokens`, `All`)
- `docs/runtime/COMMAND_COVERAGE.md`
- `proofs/script-command-map/README.md`
- `proofs/script-global-cmds/README.md`

---

## Verdict

The first 20 **native verbs** in `exe-commands.md` are the first 20
`NativeTokens` rows (Global). They are **not** opcodes after Leave.

| Question | Answer | Class |
|---|---|---|
| First ASCII in the slice is a verb? | **no** — starts at `Registering Scripts` | **PROVEN** |
| First verb string? | `CameraFOVLookBetween` `0x012C1870` | **PROVEN** |
| First 20 verbs == `NativeTokens[0..19]`? | **yes** (VA + name + Global) | **PROVEN** |
| Any of those 20 run after Leave? | **none** | **PROVEN** / **DISPROVEN** as first-seen |
| `GameInfo` / `Fullscreen` in `All`? | **no** — coverage **UNREAD** | **PROVEN** |
| `StopMusic` / `StayFadedOut` TokenSite | `0` (name proven, handler unread) | **PROVEN** |

---

## 1. Slice before the first verb

`exe-commands.md` is every C string in `0x012C1500`–`0x012C2C00`.
`NativeTokens` is the verb filter of that slice.

Skipped **before** `CameraFOVLookBetween`:

`Registering Scripts` `Registering Script Defs`
`#<END_NEW_ENTITY_SCRIPT` `START_NEW_ENTITY_SCRIPT` /
`END_*` / `START_*` bank markers `DESC` `NAME`
`TEXT_QST_LOG_STORY_` title suffixes (`_DESC` … `_NONE`)
`true` `false`

Skipped **inside** the first-20 window (not verbs):

| After | Dropped strings |
|---|---|
| `DoCameraPreloading` | `SCRIPT_DEF` `TEXT_GUI_SKIP` |
| `TintScreenOut` | `ALLDEF:` `ALL:` |
| `TintScreenTo` | `fire` `white` `black` `override` |

Those drops match `NativeTokens`. **PROVEN.**

`script-runtime` has no `exe-commands.md`. Token files there are
handler sites (`00CC…` / `00CD…`), not the string table.

---

## 2. First 20 native verbs

Dump order. Family is Global for all 20.

| # | String VA | Verb | `NativeTokens` | `All` | TokenSite | ApplySite | Overall | After Leave |
|---|---|---|---|---|---|---|---|---|
| 1 | `0x012C1870` | `CameraFOVLookBetween` | yes | yes | `00CCB479` | `00CCB728` | Partial | **no** |
| 2 | `0x012C1888` | `CameraLookBetween` | yes | yes | `00CCAA6C` | `00CCADB9` | Partial | **no** |
| 3 | `0x012C189C` | `CameraLookAt` | yes | yes | `00CCA73F` | `00CCA953` | Partial | **no** |
| 4 | `0x012C18AC` | `UseCamera` | yes | yes | `00CC9F3A` | `00B23B50` | Partial | **no** (leftover #17) |
| 5 | `0x012C18B8` | `StartTimeCode` | yes | yes | `00CD1373` | `0` | Partial | **no** (leftover #11) |
| 6 | `0x012C18C8` | `DoCameraPreloading` | yes | yes | `00CC86D0` | `00CBF29F` | Partial | **no** (leftover #7) |
| 7 | `0x012C18F8` | `StopMusic` | yes | yes | `0` | `0` | Partial | **no** |
| 8 | `0x012C1904` | `PlayMusic` | yes | yes | `00CC8EAC` | `00CBF7FE` | Partial | **no** (leftover #1) |
| 9 | `0x012C1910` | `Play2DSound` | yes | yes | `00CBF89E` | `00CBF8DA` | Partial | **no** |
| 10 | `0x012C191C` | `SetLightScene` | yes | yes | `00CD1425` | `00CD172A` | Partial | **no** |
| 11 | `0x012C192C` | `CameraShake` | yes | yes | `00CD131F` | `00CD1366` | Partial | **no** |
| 12 | `0x012C1938` | `CameraEffect` | yes | yes | `00CD1258` | `00CD12C2` | Partial | **no** |
| 13 | `0x012C1948` | `TintScreenOut` | yes | yes | `00CD11D0` | `00CD11F7` | Partial | **no** |
| 14 | `0x012C1968` | `TintScreenTo` | yes | yes | `00CD0CE4` | `00CD115A` | Partial | **no** |
| 15 | `0x012C19A0` | `FadeOut` | yes | yes | `00CD0987` | `008907E0` | **Proven** | **no** (leftover #2) |
| 16 | `0x012C19A8` | `FadeIn` | yes | yes | `00CC4B22` | `0088E4C0` | **Proven** | **no** (leftover #13) |
| 17 | `0x012C19B0` | `StayFadedOut` | yes | yes | `0` | `0` | Partial | **no** |
| 18 | `0x012C19C0` | `SetTime` | yes | yes | `00CD07D6` | `00CD082A` | **Proven** | **no** (`COMMAND_MAP` extra) |
| 19 | `0x012C19C8` | `GameInfo` | yes | **no** | — | — | **Unread** | **no** |
| 20 | `0x012C19D4` | `Fullscreen` | yes | **no** | — | — | **Unread** | **no** |

String VAs come from `exe-commands.md`. TokenSite / ApplySite /
overall come from `ScriptCommandMap.All` / `COMMAND_COVERAGE.md`.
Unknown native verbs stay **UNREAD** (block), not no-op.

---

## 3. Map mismatches in this window

| Name | Dump | Map | Class |
|---|---|---|---|
| `GameInfo` `Fullscreen` | real tokens | no `Find` row | **UNREAD** native (not invented) |
| `StopMusic` `StayFadedOut` | real tokens | `All` row, TokenSite=`0` | name **PROVEN**; `00CBFB7D` site **UNREAD** |
| `Get` | **not** in this slice | `All` TokenSite=`0` | **INVENTED** as exe token (outside this 20) |
| `PlayCombatAnimation` `RemoveThing` | **not** in this slice | leftover aliases | not in first 20 |

No invented names among the first 20 dump verbs.

---

## 4. Table order is not leftover order

If `Q_NewOakValeIntro` later enters `00CBFB7D`, the first leftover
lines are **not** this table prefix. From
`proofs/script-command-map` §4:

`PlayMusic` → `FadeOut` → `CameraPause` → `.Teleport` →
`.LookToThing` → `DoScriptFrame` → `DoCameraPreloading` → `PlayAVI`
…

Overlap with this 20: `UseCamera` `StartTimeCode`
`DoCameraPreloading` `PlayMusic` `FadeOut` `FadeIn` `SetTime`.
`CameraPause` / `PlayAVI` / entity verbs are **later** in
`exe-commands` (`0x012C2058` / `0x012C1DE8` / `0x012C22xx`).

Calling the first 20 table rows “first after Leave” is **INVENTED**.

---

## Classifications (short)

1. First 20 native verbs = `exe-commands` after dropping non-verbs
   = `NativeTokens[0..19]`. **PROVEN.**
2. After Leave: **no** `00CBFB7D` opcode from this table. **PROVEN.**
3. 18/20 have `All` rows. `GameInfo` / `Fullscreen` stay **UNREAD**.
4. Three overall **PROVEN** in this window: `FadeOut` `FadeIn`
   `SetTime`. The rest recovered are **PARTIAL**.
5. Do not fill first-seen from table order or from `COMMAND_MAP.md`.
