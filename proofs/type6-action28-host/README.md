# Type 6 LMB up / action 28 vs host `FrontendInputMap`

Investigation only. No production `src/` or `tests/` edits.

Question: type 6 LMB up → action 28 vs host
`FrontendInputMap`. **MATCH** or leftover? First leftover?

Authority: existing proofs
`proofs/type6-action28/README.md`,
`proofs/action28-after-26/README.md`,
`proofs/type6-same-poll-as-4/README.md`,
`proofs/action28-type11-38/README.md`,
`proofs/action28-plus228/README.md`,
`proofs/0055ACF0-first-caller/README.md`,
`proofs/host-input-type4/README.md`,
`proofs/009F4F10-second-record/README.md`,
`proofs/00A66B20-mouse-array/README.md`;
dump `Fable.exe` `00A03D60` / `0042E3EE` `0042E498` /
`0055CB10` / `0054E280` `0054E319` / `0055AD60` `0055ADDE` /
`0055ACF0` / `0055AF60` (listings already recovered in
those notes);
`src/Fable.Game/FrontendInputMap.cs` (read only),
`src/Fable.Game/EngineInput.cs` (read only);
live queue is `src/Fable.Client/Program.cs` (read only).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **MATCH** / **LEFTOVER** / **STALE**.

Do not re-prove type 6 = LMB up, type 4 = LMB down, or
action 26 `vtbl+584` / `0055AF60` posts empty `+372`.
Host Return (DIK 28) as Press Start / accept is
**DISPROVEN**.

---

## Verdict

**Classify is MATCH. Armed `+228` hop is MATCH shape.
First leftover is the first-visible scan.**

`FrontendInputMap.ActionFromEvent(6)=28` is
`0042E49D` `push 28`. `MessageFromAction` stays null on
28 (and on type-1 Return). `MessageFromWidgets(28)` posts
persist `+228` only when a type 11/38 is `Armed`. That is
the recovered `0055ADDE` → `vtbl+588` → `0055ACF0`
`push [this+380]` shape. Unarmed lists stay silent.

The first leftover **inside** `FrontendInputMap` is
`MessageFromPlus228List`: first `Visible && !Clip &&
Armed` type 11/38 `MessageId`. Native apply is
`0055CB10` on the local-map set, then **that** widget’s
`+380`. Host Return as `0xE5` / `0x126` / 15 is
**DISPROVEN**, not this leftover.

| Claim | Status |
| --- | --- |
| Native type 6 ctor `00A03D60` (`[+40]=6`, device 3) | **PROVEN** (`type6-action28`) |
| Raw 4 is LMB **up** → type 6 | **PROVEN** |
| `0042E3EE` type 6 → `push 28` at `0042E49D` | **PROVEN** |
| Apply is `0055CB10` | **PROVEN** |
| Type-10 action 28 posts `+352` / `0xE5` | **DISPROVEN** (`0054E319`) |
| Type 11/38 action 28 unarmed posts `+380` | **DISPROVEN** (`[+364]=0` skip) |
| After selected 26, action 28 calls `vtbl+588` | **PROVEN** |
| Type 11/38 `+588` dword is `0055ACF0` | **PARTIAL** (ABI; no rdata) |
| `0055ACF0` posts `[+380]` / `[def+228]` | **PROVEN** body |
| Type 6 is a DIK / Return | **DISPROVEN** |
| Host Return (type 1 / DIK 28) is Press Start | **DISPROVEN** |
| `ActionFromEvent(6)=28` | **MATCH** |
| `MessageFromAction(28,*)` is null | **MATCH** |
| `MessageFromWidgets(28)` → `+228` if `Armed` | **MATCH** shape |
| Unarmed `MessageFromPlus228List` is null | **MATCH** |
| Type 10 not posted on 28 | **MATCH** |
| First-visible `Visible && !Clip` 11/38 scan | **LEFTOVER** (first) |
| Local-map insert/erase of 28 | **LEFTOVER** (absent) |
| `Type6RecordCtor` / device / origin pair | **LEFTOVER** (absent) |
| `Program.cs` queues LMB-up as type 6 | **MATCH** producer (not the map) |
| Same-`Update` type 4 **and** type 6 | **LEFTOVER** (`009F4F10-second-record`) |

**Answer:** **MATCH** on type 6 → 28 and on armed `+228`.
**First leftover:** first-visible armed type 11/38
`MessageId` vs `0055CB10` + local map + `this+380`.

---

## 1. Native type 6 → action 28 (**PROVEN**)

Already locked in `type6-action28` /
`type6-same-poll-as-4`. Short restatement only.

```
00A03D60  [ecx+32]=3; [ecx+40]=6; origin; two doubles; ret 20
00AB5420  raw 4 → 00A03D60
0042E483  je 0042E498          ; type 6 after type-4 sub 3
0042E49D  push 28
0042E49F  jmp 0042E5AB         ; [input.vtbl+0] = 0055CB10
```

Same poll as type 4: one `0042E3EE` harvests, then
`009F4ED0` / `009F4F10` walks every mux slot. Type 4 then
type 6 is **26 then 28**. There is no “already did 26,
skip 28” gate.

Return is **not** this event. Type 1 / DIK 28 is
`0042E4B0` `push 33`.

---

## 2. Native action 28 post (**PROVEN** call; slot **PARTIAL**)

`type6-action28` “28 posts nothing” is **STALE** if
`+588` is `0055ACF0` (`action28-type11-38`).

Type 10 `0054E280` index `03` → `0054E319`: debounce
only. No `+352`. No `0xE5`.

Type 11/38 `0055AD60` `0x55AE88[2]=0055ADDE`:

```
0055ADDE  if [inner+364]==0: stamp only
          call [outer.vtbl+588]     ; 0-arg
          [inner+364]=0
```

`0055ACF0` (candidate `+588`):

```
SelectState([this+364])
inner.vtbl+16(28)               ; erase local 28
push [this+380]
call [vtbl+524]                 ; [def+228] list
```

Ctor `[+364]=0`. First-seen type 6 skips `+588`. Type 38
enable `0055AEB0` omits 28 until a **selected** action 26
runs `0055AF60` `vtbl+12(28)`. Type 11 activate
`0054DC30` already maps 28 if parent `+545`.

After that arm: type 6 → 28 → `+380` (`0x126` / 15 /
INVISIBLE `0xE5`). Action 26 still posts empty `+372`.

`01249554+588` / `0124B04C+588` dwords stay **PARTIAL**.

---

## 3. Host `FrontendInputMap` (**MATCH** then leftover)

```
Type6        = 6
ActionType6  = 28
ActionFromEvent(Type6, _) = 28          // key unused
MessageFromAction(any, screen) = null   // 28 included
MessageFromWidgets(26) = type-10 +352
MessageFromWidgets(28) = MessageFromPlus228List
MessageFromPlus228List:
  first Visible && !Clip && Armed
  && MessageId != 0
  && (Type==11 || Type==38)
  → MessageId
Plus228PostFn      = 0055ACF0
Plus228ListOffset  = 380
PersistMessageDefOffset = 228
```

`EngineInput.ApplyEvent` type 6 is the same classify
(`Dispatch(28)`). That is the `0042E3EE` analog, not a
second mapper.

| Site | Native | Host map | Class |
| --- | --- | --- | --- |
| type 6 → 28 | `0042E49D` | `ActionType6` | **MATCH** |
| key / DIK | unused | ignored | **MATCH** |
| Return → 28 / `0xE5` | type 1 → 33 | `ActionFromEvent(1,28)=33`; `MessageFrom*` null | **MATCH** / accept **DISPROVEN** |
| 28 → screen-name id | none | `MessageFromAction` null | **MATCH** |
| type-10 28 | `0054E319` | not in `MessageFromType10Attach` | **MATCH** |
| 11/38 28 unarmed | skip `+588` | `!Armed` → null | **MATCH** |
| 11/38 28 armed | `+588` / `0055ACF0` `[+380]` | `MessageId` (`+228`) | **MATCH** shape |
| `+588` dword | **PARTIAL** | `Plus228PostFn=0055ACF0` | **MATCH** ABI / rdata **PARTIAL** |
| which widget | `0055CB10` + local 28 | first visible armed 11/38 | **LEFTOVER** |
| local 28 insert | `0055AF60` `vtbl+12` | none | **LEFTOVER** |
| local 28 erase | `0055ACF0` `vtbl+16` | none | **LEFTOVER** |
| `00A03D60` constant | ctor VA | type 4 has `Type4RecordCtor`; type 6 does not | **LEFTOVER** |

`action28-type11-38` §5 already called the armed hop
**MATCH** shape. That still holds. The leftover is not
the integer 28 or the persist field.

---

## 4. First leftover

**`MessageFromPlus228List` first-visible scan.**

Native:

1. `0055CB10` focused `[+8]` else broadcast `+12` else
   `+4`.
2. Inner must already map 28 (type 11 activate or type
   38 after selected 26).
3. `[inner+364]` must be 1.
4. `vtbl+588` posts **this** `+380`.

Host map:

1. Walk `_frontendWidgets` in factory order.
2. Skip `!Visible` / `Clip` / `!Armed` / `MessageId==0`.
3. First type 11 or 38 wins.

`Visible` / `Clip` are not the `0055ACF0` gates. Native
`+545` / local-map / `+352` select are not in the map.
`Armed` is a single bool for `[inner+364]`; it is not
the selected u8 at `widget+352`.

If two type 11/38 widgets are armed with nonzero
`MessageId`, host posts the earlier visible one. Native
posts every applying inner’s own list (or none, if that
inner is unmapped / unselected / unarmed).

That is the first leftover **in** `FrontendInputMap`.
Later leftovers (no `vtbl+12`/`+16`, no
`Type6RecordCtor`) are the same gap, not a second
classify bug.

---

## 5. Related leftovers (not the map’s first)

These sit **next to** the map. They are not
`ActionFromEvent`.

| Site | Native | Host | Class |
| --- | --- | --- | --- |
| Live LMB up | `00A03D60` | `Program.cs` `QueueInput(Type6, 0)` | **MATCH** |
| Same poll 4 then 6 | mux FIFO | one `Update` is only an edge | **LEFTOVER** (`009F4F10-second-record`) |
| Record shape | 52 bytes, device 3, origin | `(type, key)` | **LEFTOVER** (`00A66B20-mouse-array`) |
| Arm all 11/38 on 26 | `+352` then `[+364]=1` on **that** inner | `ArmType34Widgets` every 11/38 | **LEFTOVER** (`EngineLifecycle`) |
| One `Dispatch` then `return` | both 26 and 28 apply this `0042E3EE` | first mapped message wins | **LEFTOVER** (`MaybeActivateNewGameFromInput`) |
| Current `_frontendWidgets` | resident slots | switched screen list | **LEFTOVER** (input leftover `84a8350`) |

`host-input-type4` “never queues type 4 / 6” is
**STALE**. `type6-same-poll-as-4` §5 “host still none”
is **STALE**. `type6-action28` §8 “28 → message null
**MATCH** as no-op” is **STALE** vs present
`MessageFromWidgets(28)`. `newgame-plus380-first` §6
“`MessageFromPlus228List` on action **26**” is
**STALE**: the map now posts that list on **28** only.

---

## 6. Do not invent

- LMB up → action 26.
- Type 6 DIK / Return as `0xE5` / `0x126` / 15.
- Action 28 as type-10 `+352`.
- Action 26 as `[def+228]` / `0055ACF0`.
- Pixel click dest as the type-6 producer.
- Printed `01249554+588` / `0124B04C+588` without rdata.

---

## Sources

- `proofs/type6-action28/README.md`
- `proofs/action28-after-26/README.md`
- `proofs/type6-same-poll-as-4/README.md`
- `proofs/action28-type11-38/README.md`
- `proofs/action28-plus228/README.md`
- `proofs/0055ACF0-first-caller/README.md`
- `proofs/host-input-type4/README.md`
- `proofs/009F4F10-second-record/README.md`
- `proofs/00A66B20-mouse-array/README.md`
- `src/Fable.Game/FrontendInputMap.cs`
- `src/Fable.Game/EngineInput.cs`
- `src/Fable.Client/Program.cs` (LMB-up queue only)
- `src/Fable.Game/EngineLifecycle.cs` (`ArmType34Widgets` /
  `MaybeActivateNewGameFromInput`; not the map)
