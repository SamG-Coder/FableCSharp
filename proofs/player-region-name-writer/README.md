# Persist `PlayerRegionName` writer on no-save New Game

Investigation only. No production `src/` / `tests/` edits.

Question: who writes persist `PlayerRegionName` on no-save
New Game? Writer VA + stored string, or UNKNOWN.

Authority: `assembly/exe/00-index/strings.tsv`
`0x01231C98`; `xrefs-by-string.tsv` / `xrefs.tsv`;
`listing-00440000.txt` `00449E60` / `00449F90` /
`004109A0`; `listing-00480000.txt` `00487C20` /
`00487EF0` (`00487F10`) / `0049F4C0` / `004A05C0` /
`004A21F0`; `listing-00400000.txt` `00413840`;
`proofs/persist-plus189-first`;
`proofs/persist-plus189-newprofile`;
`proofs/persist-plus212`;
`proofs/persist-plus212-host`;
`proofs/004A1840-second-site`;
`proofs/first-region-after-leave`;
`proofs/issue-4-verify`;
`proofs/script-setnewstart`;
`proofs/userini-names`;
`EngineLifecycle.PlayerRegionName` /
`Persist_PlayerRegionName_is_00487C20_not_new_game`.

Do **not** invent `LookoutPoint` or `StartOakVale` as the
written name.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER**.

---

## Verdict

**UNKNOWN.** No-save New Game does not write persist
`PlayerRegionName`. The named field stays empty. There is
no recovered New Game writer VA and no recovered stored
string.

The only `.text` sites of the Lionhead string
`PlayerRegionName` (`0x01231C98`) are a **continue load**
and a **FableSav PLAYER save**. Neither runs on empty
`[game+90588]`.

| Claim | Class |
|---|---|
| String VA `0x01231C98` `PlayerRegionName` | **PROVEN** `strings.tsv` |
| `.text` xrefs | **PROVEN** two: `00449EDC` (`00449E60` load), `0044A04B` (`00449F90` save) |
| `00487C20` writes the persist key | **DISPROVEN** — region **load** from a nonempty name |
| `00487C20` `E8` of `00487C20` | **PROVEN** one site: `00487F10` inside `00487EF0` |
| Callers of `00487F10` as a function | **PROVEN** none — it is the call instruction, not an entry |
| No-save takes `00449E60` / `00487C20` | **DISPROVEN** (`004A1840-second-site`, `first-region-after-leave`) |
| Save writer of the key | **PROVEN** `00449F90` → `004109A0` from `0049F4C0` PLAYER |
| That save runs on no-save New Game | **DISPROVEN** |
| Stored save string is a `.text` immediate | **DISPROVEN** — `0099EFB0(*(004FC180()+24))` |
| That live `+24` CString on New Game | **UNREAD** as a write (the write does not run) |
| CWorld HEADER `CurrentRegionName` is this key | **DISPROVEN** — different string `0x01236AC0` |
| CUIDef persist `+189` / `+212` is this key | **DISPROVEN** |
| `userst.ini` `SetStartingHolySite("NOVStartHSP")` writes this key | **DISPROVEN** — `[0x13B866C]` holy-site ScriptName |
| Invent `LookoutPoint` / `StartOakVale` as the written value | **DISPROVEN** (method) |

**Answer:** **UNKNOWN** (empty on no-save). Do not invent a
name.

---

## 1. String xrefs (PROVEN complete)

`strings.tsv`:

```
0x01231C98    PlayerRegionName
```

`xrefs-by-string.tsv` / `xrefs.tsv` — **two** hits, both
PLAYER persist, not CWorld HEADER:

```
PlayerRegionName  0x01231C98  0x00449EDC  fn=0x00449E60
PlayerRegionName  0x01231C98  0x0044A04B  fn=0x00449F90
```

No other `.text` `push "PlayerRegionName"`. **PROVEN.**

Sibling CWorld HEADER name is **`CurrentRegionName`**
`0x01236AC0` (`0049F8E4` save, `004A2976` load,
`0047FDB4`, `00597A51`). Different key. **PROVEN.**

---

## 2. `00487C20` / `00487F10` — load, not New Game write

`listing-00480000.txt`:

```
00487C20  sub esp, 8
          mov edi, [esp+20]          ; persist blob*
          mov esi, ecx               ; CPlayer
          lea eax, [edi+8]           ; name CString on the blob
          call [world.vtbl+48]
          call 004FC210              ; FindRegionByName
          je  00487CD7               ; empty / miss → al=0
00487C55  call 00500540              ; 00500540(index,0,1)
          …
          lea ecx, [esi+52]
          call 00A01B90              ; player+52 = Hero Thing*, not a region name
          mov al, 1
          ret 4
```

One `E8` of `00487C20`:

```
00487EF0  … stash world vtbl+12 / vtbl+48 onto player+32/+36
00487F10  call 00487C20
00487F15  test al, al
00487F17  je  00487F9F               ; miss: no region load
00487F2C  call 00487CF0              ; copy Thing* onto +52 / +44
```

`00487F10` is the call site inside `00487EF0`. It is not an
entry with callers. **PROVEN.**

Parent of `00487EF0`: only `00449F25` inside `00449E60`
(`listing-00440000.txt`). That function **reads** the key:

```
00449EAD  push "PlayerCharacterUID"
          call 0044BB00
00449EDB  push "PlayerRegionName"
00449EE2  call 004109A0              ; persist CString transfer
          …
00449F25  call 00487EF0              ; then 00487C20
```

`00449E60` itself is only reached from FableSav apply
`004A2B05` after `push "PLAYER"` (`004A21F0`). No-save
`[game+90588]` empty skips `004A3200` → no `004A21F0` →
no PLAYER load. **PROVEN** (`004A1840-second-site`).

So `00487C20` is continue/named persist **load**. Empty
name `je`s. Not a New Game writer. **PROVEN.**

---

## 3. Persist field writer (save, not New Game)

`00449F90` is the only function that **pushes** the string
on a write-shaped path:

```
00449F90  call 004498C0              ; player*
          lea ecx, [eax+52]
          call 00A01B50              ; Hero Thing*
          …
          call 004C73D0              ; Thing UID
          call [vtbl+52] / [vtbl+64]
          call 004FC190
          call 004FC180              ; current WorldMap record*
          add eax, 24                ; record+24 CString
          call 0099EFB0              ; copy that CString to stack
          push "PlayerCharacterUID"
          call 0044BB00
          push "PlayerRegionName"
0044A051  call 004109A0              ; persist CString transfer
```

`004109A0` is generic named CString persist
(`00404500` CRC then `[ctx+24]` jmp `0x410AA4`:
`00410A60` / `00410A32` / `00410A01` / `004109E3` /
`004109C4`). Mode is the persist context, not a hardcoded
region name. **PROVEN** as transfer; **DISPROVEN** as a
`.text` immediate store of `LookoutPoint` or `StartOakVale`.

Sole `E8` of `00449F90`:

```
0049FB2A  push "PLAYER"
0049FB5C  call 00449F90              ; inside 0049F4C0
```

`0049F4C0` is FableSav **writer** (`push "HEADER"` at
`0049F5A2`, then `WorldName` / `CurrentRegionName` /
`ENTITIES` / `PLAYER` / `QUESTS` / `REGIONS` /
`FACTIONS`). Gate:

```
0049FB1A  mov al, [esi+258]
0049FB22  jne 0049FCEF               ; +258 ≠ 0 → skip PLAYER
```

`E8` of `0049F4C0`:

| Site | Parent | Writes PLAYER / `PlayerRegionName`? |
|---|---|---|
| `004A0134` | `004A00E0` sets `[world+258]=1` first | **no** (`jne 0049FCEF`) |
| `004A06D7` | `004A05C0` (does not set `+258` here) | **yes** if `+258==0` |

`004A05C0` sites: `004A0827`, `004A08AB` (`004A0850`,
also gated on `[world+256]`). Those are save helpers, not
Leave / `00416953` / msg 15. **PROVEN** as save;
**DISPROVEN** as no-save New Game.

No-save New Game: empty `+90588` → `00416ABA 004A1840`
then `0049F180(0)` Init Characters. `0049F180` transfers
CWorld HEADER flags via `004045C0` (`TeleportingEnabled`,
…). It does **not** push `PlayerRegionName`. **PROVEN.**

---

## 4. HEADER vs PLAYER vs CUIDef vs holy site

FableSav layout recovered from `0049F4C0` / `004A21F0`:

```
HEADER     WorldName / CurrentRegionName / flags / …
ENTITIES
PLAYER     PlayerCharacterUID / PlayerRegionName   ← this key
QUESTS
REGIONS
FACTIONS
```

`CurrentRegionName` is CWorld HEADER. `PlayerRegionName`
is PLAYER. Do not collapse them. **PROVEN.**

CUIDef file persist (`persist-plus189*` / `persist-plus212*`)
is `00631C60` dest `CUIDef+189` / `+190` / `+212`
(`0xBDACBABA` / `0xAC637D43` / `0xCB9ADD65`) on
`frontend.bin` widgets. Writer is not `004109A0`. NEW_GAME
button `+189`/`+212` payloads stay **UNREAD** there and
are **not** this string. **DISPROVEN** as
`PlayerRegionName`.

`userst.ini` `SetStartingHolySite("NOVStartHSP")`:

```
00413840  copy arg CString → [0x13B866C]
```

Applied at `00414C66` **before** frontend. First
`0049F180` holy-site lookup **misses**. Not a region-name
persist write. **PROVEN** store; **DISPROVEN** as this key
(`script-setnewstart`, `userini-names`).

---

## 5. What no-save actually does with the field

Host / native no-save:

- `PlayerRegionName` empty → skip `00487C20`
- first real open is `00501450` `00500540(1,0,0)` (Lookout
  as **index 1**, not as this persist string)
- continue with a nonempty HEADER PLAYER blob takes
  `00449E60` → `00487C20` instead

Tests that **set** `PlayerRegionName = "StartOakVale"`
(`Persist_PlayerRegionName_is_00487C20_not_new_game`) prove
the **load** arm, not a New Game write. **PROVEN** as
continue; **LEFTOVER** if treated as New Game.

The save-time payload, **if** `00449F90` ran, would be
whatever CString lives at `004FC180()+24` (WorldMap record
stride 88). Dummy first pump is index 0 (empty slot). After
a later load it would be the live region record. That is
**not** a New Game store, and the bytes are **not** a
`.text` name. Do **not** fill that slot with
`LookoutPoint` or `StartOakVale`.

---

## Do not invent

- `PlayerRegionName=StartOakVale` on no-save.
- `PlayerRegionName=LookoutPoint` on no-save.
- `00487C20` as a persist **writer**.
- CWorld `CurrentRegionName` as this key.
- CUIDef `+189` / `+212` as this key.
- `NOVStartHSP` / `SetStartingHolySite` as this key.
- A New Game HEADER PLAYER blob.

Return: **UNKNOWN**.
