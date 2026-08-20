# `[quest+80]` after `vtbl+2584(12)` / `HerosOldHouse`

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** wire `00DBDE40` into `EngineLifecycle.Pump`.
Do **not** write `[quest+80]=1` from `StartNewGame` /
`ApplyPersist(true)` / the 12 s return / first Present.
Do **not** collapse leftover **#4** (Lookout first Present
vs Oakvale intro view).

Question (PARITY 0b / `docs/status/README.md`): who writes
`[quest+80]=1` after the blocking `vtbl+2584(12)` and
`HerosOldHouse` lookup? The leftover called that store
**UNREAD** (not a `mov` in `00DBDE00–00DBF000`; later-quest
`+80` stores are not this gate).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00d80000.txt` `00DAADA0` / `00DAADD0` /
`00DABAC0` `00DAC158` / `00DAC295` / `00DAC420` /
`00DB97A0` / `00DBB218`–`00DBB2A7` / `00DBDE40`–
`00DBE22F` (`00DBE0C6` PreAttack / `00DBE13E` 12 s /
`00DBE15E` house / `00DBE1F3` spin);
siblings `proofs/00DBDE40-after-activate`,
`proofs/00DBDE40-host-gap`,
`proofs/00DBB2A7-attackover-store`,
`proofs/intro-fiber-attackover`,
`proofs/raid-avi-attackover-live`;
`RegionTravel.PreAttackGateOffset` / `AttackOverStore` /
`FirstSeenPlus80WrittenInStartOakVale` /
`FirstSeenAttackOverStoreRuns` /
`FirstSeenStartsIntroCutscene`;
`PersistTable.AttackOverWrite` / `AttackOverStore`;
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`;
`WorldSceneTests` `FirstSeenStartsIntroCutscene`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| What is `[quest+80]`? | Persist byte **`AttackOver`** on `S_QNOVI` / `Q_NewOakValeIntro`. Bind `00DAADA0` `004045C0("AttackOver", this+80)` seeds **0**. | **PROVEN** |
| Who writes `1` **in** `00DBDE40` after `vtbl+2584(12)` + `HerosOldHouse`? | **Nobody.** After those calls the fn only **reads** `[esi+80]` and yields. | **DISPROVEN** as a `mov` |
| Is there a `mov [reg+80], 1` in `00DBDE00–00DBF000`? | **No.** `FirstSeenPlus80WrittenInStartOakVale=false`. | **PROVEN** absence |
| Who writes `1` on **this** quest object? | `00DBB2A7` `mov [ecx+80], 1` after `CS_OAKVALE_INTRO_THERESA` + PlayAVI `1_raid_on_oak_vale_comp.xmv`. `ecx = [ebp+20]` = parent `S_QNOVI`. | **PROVEN** later store |
| Are later-quest `+80=1` stores this gate? | **No.** `00D939D3` is `TEXT_QST_036` granny. Not `S_QNOVI`. | **DISPROVEN** as this wait |
| Is `00DAADA0` the `+80=1` write? | **No.** Bind, stack default 0. | **DISPROVEN** |
| Does no-save first Present reach the 12 s wait / house / spin / store? | **No.** Quest never constructed. First Present is Lookout. | **DISPROVEN** |
| Does `FirstSeenStartsIntroCutscene=true` mean first Present writes `+80`? | **No.** That flag is Father CS from `NOVI_LiveFather` **if** Oakvale is constructed. Leftover vs Leave. | **LEFTOVER** pairing |
| Who activates `Q_NewOakValeIntro` on no-save? | Not Leave / `004B4260` / `00CE7670` / `user.ini`. | **UNREAD**. Do **not** invent |

PARITY leftover “UNREAD” is **MATCH** as “no `mov` in the
wait window.” It is **stale** as “identity of the field
writer unknown”: that VA is `00DBB2A7`. The store is
**later** (Theresa + raid), not first-seen, and is **not**
a store after `00DBE13E` / `00DBE15E` in the same function.

---

## Verdict

**`[quest+80]` is `AttackOver`. The 12 s / house site
does not write it. The later writer is `00DBB2A7`.
No-save first Present never reaches either.**

```
00DAADA0  bind AttackOver @ this+80 = 0     // not store 1
00DAADD0  mov [esi+80], bl                  // clear
00DABAC0  test [esi+80]                     // READ; 1 → skip setup
          E8 00DBDE40                       // only caller 00DAC295
00DBDE40  map-wait StartOakVale
          READ [esi+80]                     // 00DBDED9; 1 → PostAttack
          kid + three watchers
00DBE0C6  Q_NewOakValeIntro_PreAttack
00DBE13E  vtbl+2584(12.0)                   // blocking wait
00DBE15E  lookup HerosOldHouse
00DBE1F3  READ [esi+80]
00DBE200  yield vtbl+28 until [esi+80]!=0   // NO mov here
          // 00DBDE00–00DBF000: no mov [reg+80], 1
later     00DB97A0 Theresa start
00DBB238  CS_OAKVALE_INTRO_THERESA
00DBB260  vtbl+1476 raid AVI
00DBB2A7  mov [ecx+80], 1                   // the store
00DBDE40  spin exits → 00DBE3C0 PostAttack  // not first-seen
```

Host must not invent the activate to “reach” this.
`EngineLifecycle.Pump` must not `E8 00DBDE40` and must
not poke persist true.

---

## 1. What `[quest+80]` is

`00DAADA0` (`listing-00d80000.txt`):

```
00DAADA0  push ecx
00DAADA1  lea  eax, [esp+3]
00DAADA6  add  ecx, 80
00DAADA9  push ecx
00DAADAE  push "AttackOver"
00DAADB3  mov  [esp+15], 0              // seed 0
00DAADB8  call 004045C0
00DAADBE  ret  4
```

S_QNOVI vtbl `012D7A28` slot 4 (`+16`) is this persist
bind. Slot 3 `00DAADD0` does `xor ebx, ebx` then
`mov [esi+80], bl` (clear, not store 1).

`this` is the `00DAAC00` quest object (size `0x10C`).
`esi` in `00DBDE40` is that same `ecx`. Host names:

| Host | Native |
|---|---|
| `PersistTable.AttackOverWrite = 00DAADA0` | bind |
| `PersistTable.AttackOverWriteIsBind = true` | seed 0 |
| `PersistTable.AttackOverOffset = 80` | `PreAttackGateOffset` |
| `NewGameScript.PersistAttackOverName` | `"AttackOver"` |

**PROVEN** name + offset + default 0.

---

## 2. `00DBDE40` after PreAttack / 12 s / `HerosOldHouse`

Only `E8` of `00DBDE40` is `00DAC295` inside slot-2
`00DABAC0`. After map-wait + AttackOver early-out
(`00DBDED9` **read**; true jumps to `00DBE21E` PostAttack):

```
00DBE0C6  push "Q_NewOakValeIntro_PreAttack"
00DBE0E0  call [eax+1104]
00DBE128  lea  eax, [esi+76]
00DBE12C  push 1
00DBE12E  call [edx+2592]
00DBE139  push 0x41400000                 // 12.0f
00DBE13E  call [edx+2584]                 // blocking
00DBE15E  push "HerosOldHouse"
00DBE178  call [eax+288]
…
00DBE1F3  mov  al, [esi+80]
00DBE1F8  jne  00DBE21E
00DBE200  call [eax+28]                   // yield
00DBE20A  call 00CB7940
00DBE217  mov  al, [esi+80]
00DBE21C  je   00DBE200                   // spin
00DBE22F  call 00DBE3C0                   // PostAttack — later
```

`listing-00d80000.txt` in `00DBDE00–00DBF000`:

| VA | Op | Role |
|---|---|---|
| `00DBDED9` | `mov al, [esi+80]` | READ early-out |
| `00DBE1F3` | `mov al, [esi+80]` | READ after house |
| `00DBE217` | `mov al, [esi+80]` | READ spin |
| `00DBE973` | `mov al, [esi+80]` | `WatchBarrels` READ (other `this`) |

Zero `mov [reg+80], 1` in that window. `00DBF1B6`
`mov [esi+80], ebx` is **after** `00DBF000` and is not
this wait. **PROVEN** no first-seen writer.

---

## 3. Who writes `1` (later, same field)

`00DAC420` (`NOVI_Theresa` factory): `mov ebx, edx`
(quest) then `mov [esi+20], ebx`. `00DB97A0` is
Theresa `vtbl+4`; `mov ebp, ecx`. Tail:

```
00DBB21A  push "CS_OAKVALE_INTRO_THERESA"
00DBB238  call 00CBFB7D
00DBB248  push "Data\Video\1_raid_on_oak_vale_comp.xmv"
00DBB260  call [edx+1476]
00DBB2A4  mov  ecx, [ebp+20]              // parent S_QNOVI
00DBB2A7  mov  [ecx+80], 1                // AttackOver
```

`00DBB2A7` is **before** `00DBDE00`, so it is outside
the leftover scan range. Same `+80` the spin reads.
`FirstSeenAttackOverStoreRuns=false`.
`AttackOverStoreAfterRaidAvi=true`.
`PersistTable.AttackOverWriterKnown=true`.

`00D939D3` `mov [ecx+80], 1` sits next to
`TEXT_QST_036_GRANNY_OUTRO_10`. That is a later quest.
PARITY: “`+80=1` scans in `00CB–00DC` are later quests
(`Q_AwakeningTheOracle`, `Gate3Inner`), not this wait.”
**DISPROVEN** as the `HerosOldHouse` gate.

`ApplyPersist("AttackOver", true)` is a C# poke.
**DISPROVEN** as the writer.

---

## 4. No-save first Present does **not** reach this

`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`:

- `WorldPlus172` / `ActivatedQuests` / `Runtime.Quests`
  omit `Q_NewOakValeIntro`
- bind `00CD6E27` is `00CB5C90` **not** `00CB5AD0`
- **0** `Va==00DBDE40` (`StartOakValeSetup`)
- Gameflow `00893610` **waits** on the name
- first real region is **not** `StartOakVale` (Lookout;
  leftover **#4**)

`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`
locks `FirstSeenAttackOverStoreRuns=false` and
`AttackOverStore=00DBB2A7` as constants on that same
no-save walk. Pump never executes the store.

`WorldSceneTests` / `DataCatalogTests`
`FirstSeenStartsIntroCutscene=true` is the leftover
pairing: after a **proven** construct, `00DABAC0`
registers `NOVI_LiveFather` `00DAC2C0` **before**
`00DBDE40` map-wait; TNG construct reaches `00DB86B0`
→ `00CBFB7D("CS_OAKVALE_INTRO_FATHER")`. That is
**not** no-save first Present. Father CS does **not**
write `+80`. Raid / `00DBB2A7` are later.

Activator of `Q_NewOakValeIntro` remains **UNREAD**.
Do **not** invent `ActivateQuest` from Leave /
`Pump` / `user.ini` (`ActivateQuest("Gameflow")` only)
to reach the 12 s wait or the store.

---

## Timeline (native, only after a proven activate)

```
00CD6E27  bind S_QNOVI / 00DBEF70              // MATCH bind
????      00CB5AD0("Q_NewOakValeIntro")        // ACTIVATOR UNREAD
00DAADA0  AttackOver bind 0
00DABAC0  NOVI_* names (LiveFather / Theresa)
          E8 00DBDE40
00DB86B0  CS_OAKVALE_INTRO_FATHER              // FirstSeenStartsIntroCutscene
00DBDE40  12 s + HerosOldHouse + SPIN +80      // still 0
          // childhood deeds while spin holds
00DB97A0  MEET / deeds / CS_OAKVALE_INTRO_THERESA
00DBB260  raid AVI
00DBB2A7  [quest+80]=1                         // wakes spin
```

No-save New Game never enters this timeline. **PROVEN**.

---

## Host (do not grow)

| Host | Native | Class |
|---|---|---|
| `PreAttackGateOffset = 80` | `[esi+80]` | **MATCH** |
| `FirstSeenPlus80WrittenInStartOakVale = false` | no `mov` in wait | **MATCH** |
| `AttackOverStore = 00DBB2A7` | later `mov 1` | **MATCH** VA; **LEFTOVER** vs first Present |
| `FirstSeenAttackOverStoreRuns = false` | store after raid | **MATCH** |
| `FirstSeenStartsIntroCutscene = true` | Father CS if constructed | **LEFTOVER** vs no-save Present |
| `ActivateQuest` / `Pump` | no `00DBDE40` / no store | **MATCH** omit. Keep it. |
| `ApplyPersist(true)` | invented skip | **DISPROVEN** writer |

---

## Do not

- Invent `ActivateQuest("Q_NewOakValeIntro")`.
- Call `00DBDE40` from `Pump` / first `004189C2`.
- Write `AttackOver=1` from map-wait, 12 s return,
  `HerosOldHouse` lookup, or `StartNewGame`.
- Treat `00D939D3` / Oracle / Gate3 as this gate.
- Treat `00DAADA0` as the store.
- Collapse leftover **#4**.
- Treat `FirstSeenStartsIntroCutscene` as “first
  Present reached the `+80` spin.”

---

## Sources (absolute)

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00d80000.txt`
- `C:\FableCSharp\proofs\00DBDE40-after-activate\README.md`
- `C:\FableCSharp\proofs\00DBDE40-host-gap\README.md`
- `C:\FableCSharp\proofs\00DBB2A7-attackover-store\README.md`
- `C:\FableCSharp\proofs\intro-fiber-attackover\README.md`
- `C:\FableCSharp\proofs\raid-avi-attackover-live\README.md`
- `C:\FableCSharp\docs\PARITY.md` leftover 0b
- `C:\FableCSharp\docs\status\README.md` “Who writes `[quest+80]`”
