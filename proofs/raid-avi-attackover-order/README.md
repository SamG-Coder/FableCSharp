# Raid AVI → AttackOver → PostAttack → Maze → Give order

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent `AttackOver=1`. Do **not** skip
cutscenes. Do **not** skip the raid AVI. Do **not**
run Give `00DBE295` before AttackOver **and**
PostAttack **and** Maze.

Question: when does native play the raid AVI? Who
sets `AttackOver`? What may the host skip?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00d80000.txt` `00DB97A0` tail
`00DBB218`–`00DBB304`, `00DBDE40` `00DBDED9` /
`00DBE1F3`–`00DBE2D3`, `00DBE3C0`, `00DBEB20`;
`listing-00cc0000.txt` PlayAVI `0088F890` /
`006286F0`; compiled-def `0484-CS_OAKVALE_INTRO_THERESA`
(vector 0 has **no** `PlayAVI`);
`RegionTravel` (`RaidPlayAvi`, `AttackOverStore`,
`AttackOverStoreAfterRaidAvi`, `RaidAviIsBanditRaid`,
`FirstSeenAttackOverStoreRuns`);
`NewGameScript.GiveAfterPostAttackAndMaze`;
`EngineLifecycle.QuestGiveAfterAttackOver`;
siblings `00DBB2A7-attackover-store`,
`raid-avi-attackover-live`, `raid-avi-live-path`,
`leftover-20-playavi-pump`, `00DBDE40-after-activate`,
`00893570-give-presenters`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| When does native play the raid AVI? | After childhood deeds + radius 2.0 fall-in `00DBB0E4`. **After** blocking `00CBFB7D("CS_OAKVALE_INTRO_THERESA")`. **Then** `vtbl+1476` `Data\Video\1_raid_on_oak_vale_comp.xmv` at `00DBB260`. Not first-seen. Not opcode. Not `CS_BANDITRAID_*`. | **PROVEN** |
| Who sets `AttackOver`? | **Only** `00DBB2A7` `mov [ecx+80],1` on parent `S_QNOVI` (`ecx=[ebp+20]`), **after** that AVI returns, then fade `vtbl+1492` and music `vtbl+2784(25)`. | **PROVEN** |
| Is `00DAADA0` / `00DBDE40` the writer? | **No.** Bind + **READ** / spin. No `mov 1` in `00DBDE00–00DBF000`. | **DISPROVEN** |
| Is Give `00DBE295` after AttackOver only? | **No.** After AttackOver **and** PostAttack `00DBE3C0` **and** Maze `00DBEB20`. `GiveAfterPostAttackAndMaze=true`. | **PROVEN** |
| May the host skip Theresa CS / raid AVI? | **No.** Skip CS still returns to PlayAVI. Skip persist skips both. `SkipAvi` is opcode fixture. `FABLE_SKIP_STARTUP_AVI` is startup logos. | **DISPROVEN** skip |
| Does no-save / host Pump reach this order? | **No.** Quest never constructed. Constants MATCH. Live omit **PROVEN**. | **DISPROVEN** live |

---

## Verdict

**Native order is locked by one listing window plus
the `00DBDE40` continuation. Host must not collapse
it.**

```
00DB97A0  (NOVI_Theresa vtbl+4, already running)
  deeds / MEET / MEET_YES / OBJECTIVE_05
  00DBA3E8  WaitForUnderRadius 2.0
  00DBA400  jne 00DBB0E4                 ; deeds done
00DBB0E4  HERO / Theresa lookup
00DBB218  CS_OAKVALE_INTRO_THERESA
00DBB238  call 00CBFB7D                  ; MUST PLAY
00DBB248  Data\Video\1_raid_on_oak_vale_comp.xmv
00DBB260  call [edx+1476]                ; MUST PLAY — 0088F890 / 006286F0
00DBB28D  vtbl+1492 fade (0,0,0,255) 0.5s
00DBB29E  vtbl+2784(25)
00DBB2A7  mov [ecx+80], 1                ; AttackOver STORE
00DBB304  ret

00DBDE40  (S_QNOVI slot 2; was spinning)
00DBE1F3  READ [esi+80]                  ; now 1 → fall through
00DBE22F  call 00DBE3C0                  ; PostAttack  MUST RUN
00DBE236  call 00DBEB20                  ; Maze        MUST RUN
00DBE28B  call [eax+2620]                ; 00891880 name
00DBE295  call [edi+1152]                ; Give        AFTER all three
```

Give `00DBE295` is **not** “after AttackOver”.
It is after AttackOver **and** PostAttack **and**
Maze. Inventing `AttackOver=1` skips the AVI
**and** jumps `00DBDE40` from `00DBDED9` to
`00DBE21E` (PostAttack / Maze / Give without
childhood). That is a skip. **DISPROVEN.**

---

## 1. When native plays the raid AVI

`00DB97A0` is Theresa `vtbl+4`. First named work
is `M_TriggerOutro`. Childhood MEET / chocolate /
`OBJECTIVE_05` run in this same start. After
that, `00DBA3E3` tests `[esp+23]`; if set,
`00CBE2FF` radius **2.0** (`0x40000000`). True
→ `00DBB0E4`.

Raid tail (`listing-00d80000.txt`):

```
00DBB218  push -1
00DBB21A  push "CS_OAKVALE_INTRO_THERESA"
00DBB238  call 00CBFB7D
00DBB246  push -1
00DBB248  push "Data\Video\1_raid_on_oak_vale_comp.xmv"
00DBB256  mov ecx, [ebp+4]          ; script context
00DBB259  mov edx, [ecx]
00DBB260  call [edx+1476]           ; PlayAVI
```

| Fact | Class |
|---|---|
| Raid file is `1_raid_on_oak_vale_comp.xmv` | **PROVEN** |
| Site is `00DBB260` `vtbl+1476`, not opcode `00CCA26D` | **PROVEN** |
| Compiled-def `0484` vector 0 has **no** `PlayAVI` | **PROVEN** |
| `00CBFB7D` **returns** to `00DBB23D` then the AVI | **PROVEN** — CS skip does **not** skip AVI |
| Player is blocking `0088F890` → `006286F0` | **PROVEN** (`leftover-20-playavi-pump`) |
| `006286F0` must **return** before `00DBB2A7` | **PROVEN** |
| `RaidAviIsBanditRaid=false` | **PROVEN** |
| First-seen Game AVI (if quest live) is father opcode `dream_sequence_comp.xmv` | **PROVEN** later / **DISPROVEN** first-seen |
| No-save Pump never constructs the quest → never this AVI | **DISPROVEN** live |

`CS_BANDITRAID_*` is the adult raid family.
**DISPROVEN** as this file.

---

## 2. Who sets AttackOver

```
00DBB2A4  mov ecx, [ebp+20]         ; parent S_QNOVI
00DBB2A7  mov [ecx+80], 0x01
00DBB304  ret
```

| Site | Role | Class |
|---|---|---|
| `00DAADA0` `004045C0("AttackOver", this+80)` | persist **bind**, seed 0 | **DISPROVEN** as store |
| `00DAADD0` `mov [esi+80], bl` (`bl=0`) | reset | **DISPROVEN** as store 1 |
| `00DAC158` in `00DABAC0` | **READ**; true tears down PostAttack name then still calls `00DBDE40` | **DISPROVEN** as write |
| `00DBDED9` | **READ**; true → `00DBE21E` (skip kid / 12 s / house spin) | **DISPROVEN** as write |
| `00DBE1F3` / `00DBE217` | **SPIN READ** until 1 | **DISPROVEN** as write |
| `00DBB2A7` | **the** `mov [ecx+80],1` | **PROVEN** |

`AttackOverStoreAfterRaidAvi=true`.
`FirstSeenAttackOverStoreRuns=false`.
`FirstSeenPlus80WrittenInStartOakVale=false`.

`ecx` is `[ebp+20]`. Theresa factory `00DAC420`
stores the quest at `[thing+20]`. **PROVEN**
parent.

Host `PersistTable.AttackOverStore=00DBB2A7` is
the recovered **VA**, not a live instruction.
`ApplyPersist("AttackOver", true)` is a C# poke
used by fixtures. **DISPROVEN** as the writer.

---

## 3. Give `00DBE295` is after AttackOver **and** PostAttack **and** Maze

`00DBDE40` after the spin (`listing-00d80000.txt`):

```
00DBE1F3  mov al, [esi+80]
00DBE1F8  jne 00DBE21E
00DBE200  call [eax+28]             ; yield
00DBE20A  call 00CB7940
00DBE217  mov al, [esi+80]
00DBE21C  je 00DBE200
00DBE21E  call 00CB7940
00DBE22F  call 00DBE3C0             ; PostAttack
00DBE236  call 00DBEB20             ; Maze
00DBE247  call [edx+1488]           ; fade 0.5s
00DBE28B  call [eax+2620]           ; 00891880 → [QM+136]+48
00DBE295  call [edi+1152]           ; Give
00DBE2D3  ret
```

`00DBE3C0` pushes `Q__OakValeIntro_PostAttack`
(`vtbl+1104` activate), tears down
`Q_NewOakValeIntro_PreAttack` (`vtbl+1120`),
waits `vtbl+2584(0x41B80000)`, sets
`ENVIRONMENT_OV_POSTATTACK`, objective 06.

`00DBEB20` waits `M_PostAttackStart`, then
`00DBEE5C` `00CBFB7D("CS_OAKVALEINTRO_HESDEADJIM")`.
That CS **must play**. Last Maze opcode is
`PlayMusic MUSIC_SET_NULL,FALSE` (`00CC8EAC`).
Still Oakvale. Guild take `00D3BC60` is **OUT**.

`00DBE295` is the unique `vtbl+1152` in parent
`00DB8680` (covers `00DBDE40`). It Gives the
ticking slot name **after both calls return**.

| Claim | Class |
|---|---|
| `00DBE295` after `00DBB2A7` | **PROVEN** (spin cannot exit earlier) |
| `00DBE295` after `00DBE22F` PostAttack | **PROVEN** (listing) |
| `00DBE295` after `00DBE236` Maze | **PROVEN** (listing) |
| Host `GiveAfterPostAttackAndMaze=true` | **MATCH** |
| Host `QuestGiveAfterAttackOver=00DBE295` | **MATCH** VA; name is incomplete vs dump order |
| Start Give / PostAttack / Maze **before** `00DBB2A7` | **DISPROVEN** order |

Comments that say only “Give after AttackOver”
are **PARTIAL**. The dump order is the three
gates, in that sequence.

---

## 4. Host skip

Native has **one** legal skip of the **player**,
not of the site:

| Mechanism | What it does | Allowed on live raid? |
|---|---|---|
| `006286F0` DIK 1 / 57 / 28 / 62 | Ends the **already started** blit loop; still returns to fade + `00DBB2A7` | Native player skip. Not a host poke. Still **after** CS. |
| Theresa CS vector 1 | Inside `00CBFB7D`. Runner **still returns** to `00DBB248` | **No** — does not skip AVI. Do not fire it to “reach” persist. |
| Native `"SKIP"` at `00DB98F5` / `00DBAE20` | Action name + radius wait | **DISPROVEN** as CS skip |
| `FABLE_SKIP_STARTUP_AVI` | Host `FinishStartupVideo` logos | **DISPROVEN** as Game PlayAVI |
| `ScriptRuntime.SkipAvi` / `PumpUntilSettled` on `BlockPump` | Opcode `00CCA26D` fixture analog | **LEFTOVER** vs `00DBB260`. **Forbidden** on live raid |
| `ApplyPersist("AttackOver", true)` / `Gate80=true` | Writes `+80` from C# | **DISPROVEN** writer. Skips MEET, Theresa CS, raid AVI, childhood spin |
| `00DBDED9` already-true branch | Save/load / invented 1 | First-seen **false**. Using it as New Game skip is **DISPROVEN** |

Host Pump:

- `GamePlayAviOwnsPump=false`. `PumpGame` always
  walks `00435530` after `WorldFrame>1`.
- No `RaidPlayAvi` / `00DB97A0` / `AttackOverStore`
  Note-execute.
- `IScriptHost.PlayAvi` is the **opcode** path
  (`dream_sequence`). Raid `00DBB260` is not wired.
- First-seen no-save never activates
  `Q_NewOakValeIntro`.

So the host does **not** skip this order live —
it **never enters** it. Filling the gap with
`AttackOver=1` or `SkipAvi` is the forbidden skip.

---

## 5. First-seen vs this later order

Do not collapse.

**First-seen** (`AttackOver` still 0):

```
00DAADA0     bind AttackOver = 0
00DABAC0     NOVI_* names; E8 00DBDE40
00DB86B0     CS_OAKVALE_INTRO_FATHER
             PlayAVI dream_sequence_comp.xmv     ; OPCODE
00DBDE40     map-wait, kid, watchers, 12 s
             HerosOldHouse, SPIN +80
```

**Later** (this file):

```
00DB97A0     Theresa CS MUST PLAY
00DBB260     raid AVI MUST PLAY
00DBB2A7     AttackOver = 1
00DBE3C0     PostAttack MUST RUN
00DBEB20     Maze CS MUST PLAY
00DBE295     Give
             STOP — not 00D3BC60
```

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `00DBB260` `call [edx+1476]` | blocking raid AVI | `RaidPlayAvi` / `TheresaRaidPlayAviSite=00DBB249` | **MATCH** file + VA. Site **not** wired |
| `0484` no `PlayAVI` | native-after-CS | opcode `00CCA26D` is father dream | **PROVEN** not opcode |
| `00DBB2A7` after AVI + fade | store | `AttackOverStore` / `AttackOverStoreAfterRaidAvi=true` | **MATCH** order. Live never |
| `00DBE22F` / `00DBE236` / `00DBE295` | PostAttack, Maze, Give | `GiveAfterPostAttackAndMaze=true` | **MATCH** flag. Fiber **not** run |
| `00DBDED9` `jne 00DBE21E` | invented 1 skips childhood | `ApplyPersist(true)` fixtures | **DISPROVEN** as New Game |
| `006286F0` DIK skip | ends player, still stores | `SkipAvi` on opcode | **LEFTOVER** vs raid site |
| `FABLE_SKIP_STARTUP_AVI` | — | `FinishStartupVideo` | **DISPROVEN** as this AVI |
| no-save omit quest | never this tail | `No_save_does_not_activate_Q_NewOakValeIntro` | **PROVEN** omit. Keep it |

---

## Do not

- Invent `AttackOver=1` / `ApplyPersist(true)` /
  `Gate80=true` to skip deeds, Theresa CS, or the
  raid AVI.
- Skip `CS_OAKVALE_INTRO_THERESA_MEET*`,
  `CS_OAKVALE_INTRO_THERESA`, or
  `CS_OAKVALEINTRO_HESDEADJIM`.
- `SkipAvi` the raid file. Opcode skip is
  `dream_sequence` fixtures only.
- Treat `FABLE_SKIP_STARTUP_AVI` as Game PlayAVI.
- Start Give `00DBE295` / PostAttack `00DBE3C0` /
  Maze `00DBEB20` **before** `00DBB2A7`.
- Treat “Give after AttackOver” as sufficient
  order. Give is after AttackOver **and**
  PostAttack **and** Maze.
- Play `CS_BANDITRAID_*` as this AVI.
- Enter `00D3BC60` / `GuildArrivalHSP`.
- Invent `ActivateQuest("Q_NewOakValeIntro")` on
  no-save Leave to “reach” the AVI.
- Write `+80=1` inside a host `00DBDE40` analog
  when the 12 s wait returns.
