# Remaining raid AVI → `00DBB2A7` AttackOver live path

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent `AttackOver=1`. Do **not** skip
`CS_OAKVALE_INTRO_THERESA`. Do **not** `SkipAvi` the
raid file. Do **not** grow `PumpScripts` to Note-execute
`00DBDE40` / `00DB97A0`. `AttackOverStoreAfterRaidAvi=true`.

Question: exact store site and value? What must precede
it (AVI, CS)? What is still remaining on the host live
path?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00d80000.txt` `00DBB218`–`00DBB2A7`
(assembly bytes `C6 41 50 01`);
`RegionTravel` (`AttackOverStore`,
`AttackOverStoreAfterRaidAvi`, `RaidPlayAvi`,
`TheresaCutscene`, `TheresaRaidAviSite`,
`TheresaRaidPlayAviSite`, `FirstSeenAttackOverStoreRuns`);
`PersistTable.AttackOverStore`;
`EngineLifecycle.PumpScripts` / `TickWorld` /
`QuestManagerPumpFn`;
`ScriptRuntime.Update` (“Does not write persist fields”);
`ScriptFactoryTable.PumpRunsDabaco=false`;
siblings `proofs/00DBB2A7-attackover-store`,
`proofs/leftover-20-playavi-pump`,
`proofs/raid-avi-attackover-live`,
`proofs/raid-avi-live-path`,
`proofs/raid-avi-attackover-order`,
`proofs/00DBDE40-host-gap`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Exact store site? | **`00DBB2A7`** `C6 41 50 01` `mov [ecx+80], 0x01`. `ecx` is `[ebp+20]` (parent `S_QNOVI`). | **PROVEN** |
| Exact stored value? | **Byte `1`**. Offset **80**. Not bind `00DAADA0`. Not a C# `true`. | **PROVEN** |
| Must Theresa CS precede the store? | **Yes.** `00DBB21A` push `CS_OAKVALE_INTRO_THERESA`; `00DBB238` `call 00CBFB7D`. Blocking. Must return. | **PROVEN** |
| Must the raid AVI precede the store? | **Yes.** `00DBB248` push `Data\Video\1_raid_on_oak_vale_comp.xmv`; `00DBB260` `call [edx+1476]`. `006286F0` must **return**. `AttackOverStoreAfterRaidAvi=true`. | **PROVEN** |
| May we invent `AttackOver=1` to skip CS / AVI? | **No.** That is not `00DBB2A7`. | **DISPROVEN** writer |
| May we skip Theresa CS and still store? | **No.** Skip of the CS runner still returns to the AVI push. Skip of persist skips both. | **DISPROVEN** skip |
| Does host Pump run this store live? | **No.** Constants MATCH. `PumpScripts` never `Runtime.Update` / never `00DBDE40`. | **DISPROVEN** live |
| Is the store inside `00DBDE40`? | **No.** That fn **reads** `[esi+80]` (`00DBE1F3` / spin `00DBE200`). Writer is later `00DB97A0`. | **DISPROVEN** |

---

## Verdict

**Native store is recovered to the instruction.
Host live path is still remaining because
`PumpScripts` never `Update` `00DBDE40`.**

```
00DBB218  push -1
00DBB21A  push "CS_OAKVALE_INTRO_THERESA"     ; MUST PLAY
00DBB238  call 00CBFB7D
00DBB248  push "Data\Video\1_raid_on_oak_vale_comp.xmv"
00DBB260  call [edx+1476]                    ; MUST PLAY — then RETURN
00DBB28D  call [edx+1492]                    ; fade (0,0,0,255) 0.5s
00DBB29E  call [eax+2784]                    ; music 25
00DBB2A4  mov ecx, [ebp+20]                  ; parent S_QNOVI
00DBB2A7  mov [ecx+80], 0x01                 ; AttackOver STORE
00DBB304  ret
```

`RegionTravel.AttackOverStoreAfterRaidAvi=true` is
the order lock. Filling the remaining gap with
`ApplyPersist("AttackOver", true)` / `Gate80=true`
is **DISPROVEN**. Filling it by skipping Theresa CS
is **DISPROVEN**.

---

## 1. Exact store (`listing-00d80000.txt`)

Assembly listing (bytes):

```
00DBB2A4  8B 4D 14                  mov ecx, [ebp+20]
00DBB2A7  C6 41 50 01               mov [ecx+80], 0x01
```

| Field | Value | Class |
|---|---|---|
| VA | `00DBB2A7` | **PROVEN** |
| Encoding | `C6 41 50 01` | **PROVEN** |
| Dest | `[ecx+80]` | **PROVEN** |
| Immediate | `0x01` | **PROVEN** |
| `ecx` | `[ebp+20]` parent quest | **PROVEN** |
| Persist name | `AttackOver` | **PROVEN** bind `00DAADA0` |
| Host `AttackOverStore` | `00DBB2A7` | **MATCH** VA |
| Host `AttackOverOffset` | 80 | **MATCH** |
| Host `AttackOverStoreAfterRaidAvi` | **true** | **MATCH** order |
| Host `FirstSeenAttackOverStoreRuns` | **false** | **MATCH** |
| `00DAADA0` | persist **bind** `004045C0("AttackOver", this+80)`, seed 0 | **DISPROVEN** as store |
| `00DAADD0` `mov [esi+80], bl` | reset 0 | **DISPROVEN** as store 1 |
| `00DBDE40` `00DBE1F3` / `00DBE217` | **READ** / spin | **DISPROVEN** as write |

Theresa factory `00DAC420` stores the quest at
`[thing+20]`. `00DBB2A4` loads that parent.
**PROVEN.**

`PersistStore` header still says “Writer UNREAD”.
That comment is **LEFTOVER** vs
`AttackOverWriterKnown=true`. The **VA** is known;
the **live call** is the remaining gap.

---

## 2. What must precede the store

One listing window. No collapse.

```
00DBB218  6A FF                     push -1
00DBB21A  68 C4 97 2D 01            push "CS_OAKVALE_INTRO_THERESA"
00DBB21F  lea ecx, [esp+64]
00DBB223  call 0099EBF0
00DBB228  push 1
00DBB22A  push 0
00DBB22C  push 0
00DBB22E  push 0
00DBB238  E8 40 49 F0 FF            call 00CBFB7D     ; CS MUST PLAY
00DBB23D  lea ecx, [esp+56]
00DBB241  call 0099EAE0
00DBB246  6A FF                     push -1
00DBB248  68 9C 97 2D 01            push "Data\Video\1_raid_on_oak_vale_comp.xmv"
00DBB256  mov ecx, [ebp+4]
00DBB259  mov edx, [ecx]
00DBB25F  push eax
00DBB260  FF 92 C4 05 00 00         call [edx+1476]   ; AVI MUST PLAY
00DBB28D  FF 92 D4 05 00 00         call [edx+1492]   ; fade
00DBB29C  6A 19                     push 25
00DBB29E  FF 90 E0 0A 00 00         call [eax+2784]   ; music
00DBB2A7  C6 41 50 01               mov [ecx+80], 1   ; THEN store
```

| Predecessor | Site | Why it must run | Class |
|---|---|---|---|
| Childhood deeds / `OBJECTIVE_05` / radius 2.0 | `00DB97A0` → `00DBB0E4` | Fall-in to this tail. Skipping deeds is not this store. | **PROVEN** order (`raid-avi-attackover-live`) |
| `CS_OAKVALE_INTRO_THERESA` | `00DBB21A` / xref `00DBB21B`; runner `00DBB238` | Blocking `00CBFB7D`. Compiled-def `0484` has **no** `PlayAVI`. | **PROVEN** CS |
| Raid AVI `1_raid_on_oak_vale_comp.xmv` | `00DBB248` / xref `00DBB249`; apply `00DBB260` `vtbl+1476` | Blocking `0088F890` → `006286F0`. Must **return** before `00DBB2A7`. | **PROVEN** AVI |
| Fade `vtbl+1492` black 0.5s | `00DBB28D` | After AVI return, before store. | **PROVEN** |
| Music `vtbl+2784(25)` | `00DBB29E` | After fade, before store. | **PROVEN** |

`TheresaRaidAviSite=00DBB21B` is the string
operand (xref), not the `push` opcode at
`00DBB21A`. `TheresaRaidPlayAviSite=00DBB249`
same for the `.xmv`. **MATCH.**

`RaidAviIsBanditRaid=false`. `CS_BANDITRAID_*`
is adult raid. **DISPROVEN** as this file.

Father opcode `dream_sequence_comp.xmv` is first
Game AVI **if** the quest is live. **DISPROVEN**
as this raid site (`leftover-20-playavi-pump`).

---

## 3. Do not skip Theresa CS

| Temptation | What native does | Class |
|---|---|---|
| Skip `00CBFB7D` / fire vector 1 | Runner **still returns** to `00DBB23D` then the AVI push | **DISPROVEN** as AVI skip |
| `ApplyPersist("AttackOver", true)` / `Gate80=true` | Writes `+80` from C#. Skips MEET, Theresa CS, raid AVI, `00DBDE40` childhood spin | **DISPROVEN** writer |
| `SkipAvi` / `PumpUntilSettled` on `BlockPump` | Opcode `00CCA26D` fixture analog. Raid site is **not** the opcode | **LEFTOVER** vs `00DBB260`. **Forbidden** live |
| `FABLE_SKIP_STARTUP_AVI` | Startup logos | **DISPROVEN** as Game PlayAVI |
| Native `"SKIP"` at `00DB98F5` / `00DBAE20` | Action name + radius wait | **DISPROVEN** as CS skip |
| Start `CS_OAKVALE_INTRO_THERESA` at TNG construct | First `00CBFB7D` in `00DB97A0` is MEET at `00DB9B28` | **DISPROVEN** |

Do **not** skip Theresa CS to “reach” persist.
Do **not** invent `AttackOver=1` because CS / AVI
are unimplemented.

---

## 4. Host gap: `PumpScripts` never `Update` `00DBDE40`

Native after construct (`00DABAC0` only `E8` of
`00DBDE40` is `00DAC295`):

```
00A44880  resume slot 2 = 00DABAC0
00DBDE40  map-ready, kid, watchers, 12 s, HerosOldHouse
00DBE1F3  mov al, [esi+80]
00DBE1F8  jne 00DBE21E
00DBE200  call [eax+28]             ; yield
00DBE217  mov al, [esi+80]
00DBE21C  je 00DBE200               ; SPIN until AttackOver=1
00DBE22F  call 00DBE3C0             ; PostAttack — AFTER store
```

The spin **waits for** `00DBB2A7`. It does not
write 1. Writer lives on Theresa `vtbl+4`
`00DB97A0`, same Game fiber pump.

Host `TickWorld`:

```
PumpQuests()     ; 004B4490 Notes; 00CB7950 Note; no Runtime.Update
PumpScripts()    ; 006E75C0 Notes; ScriptPumpWalked=0
```

`PumpScripts` body:

```
Note(006E75C0 … flag=1)
Note(004A6550 Init Scripts)
Note(vtbl+1580 / vtbl+1544)
Note(0059299D skip +60 empty)
ScriptPumpWalked = 0
ScriptPumpRan = true
```

No `Runtime.Update`. No `00A44880`. No
`00DABAC0`. No `00DBDE40`. No `00DB97A0`. No
`BeginAvi`. No `RaidPlayAvi`. No `AttackOverStore`.

| Host lock | Value | Class |
|---|---|---|
| `ScriptPumpWalked` on no-save Pump | **0** | **PROVEN** omit (`EngineLifecycleTests`) |
| `PumpRunsDabaco` | **false** | **MATCH** |
| `QuestManagerPumpFn` comment | host walk of `00CB7950` / `Runtime.Update` is leftover | **LEFTOVER** |
| `ScriptRuntime.Update` | `00A44880` analog; “Does not write persist fields” | **MATCH** comment; **LEFTOVER** vs Pump |
| `EngineLifecycle` grep `Runtime.Update` | comment only | **PROVEN** omit |
| `GamePlayAviOwnsPump` | **false** | leftover #20 (`leftover-20-playavi-pump`) |
| `FirstSeenAttackOverStoreRuns` | **false** | **MATCH** |

So the remaining live path is **not** “recover the
VA again”. The VA is recovered. Remaining is:

1. A proven activator of `Q_NewOakValeIntro`
   (not no-save Leave). **blocked-on-activator**
   (`raid-avi-live-path` / `00DBDE40-host-gap`).
2. A host analog of `00A44880` that actually
   resumes `00DABAC0` → `00DBDE40` **and** Theresa
   `00DB97A0`. Today `PumpScripts` never does that
   `Update`.
3. Play Theresa CS, **then** raid AVI, **then**
   `00DBB2A7`. Do not poke persist.

Do **not** close this remaining item by calling
`00DBDE40` from `PumpScripts`. Do **not** Note-execute
`00DB97A0`. Do **not** write `+80=1` when a host
12 s wait returns.

---

## 5. Remaining list (stop at first unproven)

Ordered. Do **not** satisfy these by inventing
`AttackOver=1` or skipping Theresa CS.

1. Proven activator of `Q_NewOakValeIntro`
   (`004B4A10` / `00CB5AD0`). **UNREAD** /
   blocked-on-activator.
2. Construct `00DBEF70` / `00DAAC00` / persist
   bind `00DAADA0` value **0**.
3. `00DABAC0` names including `NOVI_Theresa`
   `00DAC420` **before** `00DBDE40`.
4. **`PumpScripts` / `Runtime.Update` analog of
   `00A44880`** so `00DBDE40` actually runs.
   Host today: Notes `006E75C0`,
   `ScriptPumpWalked=0`. **LEFTOVER.**
5. `00DBDE40` map-ready / kid / 12 s /
   `HerosOldHouse` **spin READ `+80`**.
6. Childhood deeds through objective 05 / radius
   2.0 → `00DBB0E4`.
7. Live `00DB97A0` (`[thing+20]` = parent).
8. `00CBFB7D("CS_OAKVALE_INTRO_THERESA")` at
   `00DBB238`. **Play it.**
9. `vtbl+1476` `1_raid_on_oak_vale_comp.xmv` at
   `00DBB260`. **Play it.** `006286F0` returns.
10. **Then** `00DBB2A7` `mov [ecx+80], 1`.

After the store (not the gap to the write):

11. `00DBDE40` spin exits → PostAttack `00DBE3C0`
    → Maze `00DBEB20` → Give `00DBE295`.
12. **Stop.** Not Guild take `00D3BC60`.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `00DBB2A7` `C6 41 50 01` | store 1 at `[quest+80]` | `AttackOverStore=00DBB2A7` | **MATCH** VA. Live never |
| `00DBB238` `00CBFB7D` | Theresa CS must play | `TheresaCutscene` / `TheresaRaidAviSite` | **MATCH** name/VA. Not started |
| `00DBB260` `vtbl+1476` | raid AVI must play and return | `RaidPlayAvi` / `TheresaRaidPlayAviSite` | **MATCH** file. Site **not** wired |
| `AttackOverStoreAfterRaidAvi=true` | store after `006286F0` returns | same flag | **MATCH** order |
| `00DBE1F3` READ / spin | waits for the store | `FirstSeenPlus80WrittenInStartOakVale=false` | **MATCH** read. Fiber **not** run |
| `PumpScripts` `006E75C0` | native script-manager walk; fibers are `00A44880` | Notes only; `ScriptPumpWalked=0`; no `Runtime.Update` | **LEFTOVER** — remaining live gap |
| `ApplyPersist(true)` | — | C# poke | **DISPROVEN** writer |
| Theresa CS skip | still hits AVI | none | **DISPROVEN** skip |
| no-save omit quest | never this tail | `No_save_does_not_activate_Q_NewOakValeIntro` | **PROVEN** omit. Keep it |

---

## Do not

- Invent `AttackOver=1` / `ApplyPersist(true)` /
  `Gate80=true`.
- Skip `CS_OAKVALE_INTRO_THERESA` /
  `CS_OAKVALE_INTRO_THERESA_MEET*` / the raid AVI.
- Treat `AttackOverWriterKnown=true` as “host
  reached the store”.
- Treat `AttackOverStoreAfterRaidAvi=true` as a
  license to skip CS and jump to persist.
- Call `00DBDE40` from `Pump` / `PumpScripts` /
  `RequestNewGame`.
- Grow `PumpScripts` to Note-execute `00DB97A0`.
- Write `+80=1` inside a host `00DBDE40` analog
  when the 12 s wait returns.
- Play `CS_BANDITRAID_*` as this AVI.
- Start PostAttack / Maze / Give **before**
  `00DBB2A7`.
- Invent `ActivateQuest("Q_NewOakValeIntro")` on
  no-save Leave to “reach” the store.
