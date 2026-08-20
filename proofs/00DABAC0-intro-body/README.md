# `00DABAC0` body before AttackOver; Father CS FOV 72

Investigation only. No production `src/` edits.

Do **not** invent a no-save fiber start of
`S_QNOVI`. Do **not**
`ActivateQuest("Q_NewOakValeIntro")`.
Do **not** re-enter `00DABAC0` from resume
`00A44660` / `009D87F0`. Do **not** treat
`00A446A0` `[watcher+16]` as persist
`00DAADA0`. Do **not** collapse Lookout FOV
70 into SHOT2 72.

Question: what does slot-2 run `00DABAC0`
do **before AttackOver**? Is Father CS FOV
72? If `S_QNOVI` existed, what is
first-seen?

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Authority:

- `listing-00d80000.txt` `00DAAC00` /
  `00DAACE0` / `00DAAD70` / `00DAADA0` /
  `00DABAC0`–`00DAC2A1` / `00DAC2C0` /
  `00DBDE40` / `00DBB2A7` / `00DB86B0`
- `listing-00cc0000.txt` `00CDD440` /
  `00CC9F39` / `00CC9E69`
- `listing-00a40000.txt` `00A44660` /
  `00A44690` / `00A446A0` / `00A44880`
- `listing-00b00000.txt` `00B23B50` /
  `00B314E0` (`+536`)
- ExeIndex `calls-s-qnovi-run-00dabac0`
  (0 `E8`), `s-qnovi-slot2-run-00dabac0`,
  `vtbl-s-qnovi-vtbl-012d7a28`,
  `native-sqnovi.md`,
  `0481-cs-oakvale-intro-father.md`
- `assembly/exe/00-index/vtbl.tsv`
  `0x012D7A28` slot 2
- TNG `startoak-tng.txt` `CAM_OVIF_SHOT2`
- Host `IntroQuestRun=00DABAC0`,
  `SqnoviReentersRunAfterYield=false`,
  `FirstSeenCallsUseCamera=false`,
  `PumpRunsDabaco=false`,
  `IntroCameraFovDegrees=72`,
  `FirstSeenFovTurns=0.2`
- Siblings `00CDD440-vtbl8-slot`,
  `intro-fiber-attackover`,
  `playcombatanim-intro`,
  `cs-oakvale-intro-father-lines`,
  `00DAAC00-sqnovi-no-save`,
  `usecamera-spline-first`,
  `00DBB2A7-attackover-store`

---

## Verdict

**`00DABAC0` does not write AttackOver.**
Before the `+80` **READ** it only registers
16 `NOVI_*` / `OVI_DeadFather` names, flushes,
and calls context `vtbl+256`. First-seen
`+80==0` skips `Q__OakValeIntro_PostAttack`.
Then objective + `StartBarrelTimer` +
`E8 00DBDE40`. The persist **bind** is
`00DAADA0` (S_QNOVI `vtbl+16`). The **store**
`+80=1` is later `00DBB2A7` (Theresa + raid
AVI). **DISPROVEN** as this fn.

Father CS is **not** started by `00DABAC0`.
TNG `NOVI_LiveFather` construct after the
name table runs `00DB86B0` →
`00CBFB7D("CS_OAKVALE_INTRO_FATHER")`.
`UseCamera CAM_OVIF_SHOT2` SNAP-binds via
`00B23B50` with camera `+536==0`. TNG spline
FOV `0.2` turns = **72°**. **PROVEN** leftover
SHOT2. **DISPROVEN** as no-save first Present
(`FirstSeenCallsUseCamera=false`; Lookout
helper 70°).

If `S_QNOVI` **existed** (after a **proven**
construct, **not** no-save): first enter is
`00CDD440` `jmp [vtbl+8]`. Resume
`00A44660` / `009D87F0` continues the parked
stack; it does **not** re-enter `00DABAC0`.
First-seen AttackOver stays **0**. Father CS
+ SHOT2 SNAP 72 run as leftovers on that
fiber. Do **not** invent this as Leave /
first Present.

| Question | Answer | Class |
|---|---|---|
| `.text` `E8` of `00DABAC0`? | **0** | **PROVEN** |
| First enter? | `00DAAD76` → `00CDD440` `jmp [S_QNOVI.vtbl+8]` | **PROVEN** leftover vs no-save |
| No-save first `00CDD440` dest? | Gameflow `00CE7670` | **DISPROVEN** as `00DABAC0` |
| Resume re-enters via `[vtbl+8]`? | **No.** `00A44660` → `009D87F0([fiber+16])` | **DISPROVEN** |
| Before `+80` READ? | 16× `00CB8230` + `00CB8930` + `vtbl+256` | **PROVEN** |
| Does `00DABAC0` write `+80=1`? | **No** | **DISPROVEN** |
| Persist bind? | `00DAADA0` `004045C0("AttackOver", this+80)` seed 0 | **PROVEN** bind; not this `this` |
| Store? | `00DBB2A7` after Theresa CS + raid AVI | **PROVEN** later |
| `00DAC295`? | only `E8` of `00DBDE40` | **PROVEN** |
| Father CS from this fn? | **No.** TNG `00DAC2C0` → `00DB86B0` | **DISPROVEN** as a call here |
| SHOT2 FOV 72? | TNG `0.2` turns × 360 | **PROVEN** leftover |
| Bind SNAP `+536=0`? | `00B23B50` → `00B314E0` `je 00B31502` | **PROVEN** leftover |
| First leftover camera bind? | `NoLoadUseCamera CAM_OVI_ID_STANDUP` | **PROVEN** leftover |
| STANDUP FOV is 72? | spline type **PROVEN**; property dump | **UNREAD** |
| No-save first Present FOV? | Lookout helper **70°** | **DISPROVEN** as 72 |
| First-seen if `S_QNOVI` existed? | names + `00DBDE40` + Father CS; `+80` still 0 | **PROVEN** leftover; **DISPROVEN** no-save |

---

## Timeline

### No-save New Game (this walk)

```
00CD6E27  bind Q_NewOakValeIntro / S_QNOVI / 00DBEF70
004B4260  Init Quests — name not in world+172
user.ini  ActivateQuest("Gameflow")
type-1    00CDD440 jmp [Gameflow.vtbl+8] = 00CE7670
          00893610 "Q_NewOakValeIntro" miss → yield
Present   Lookout helper FOV 70
```

`00DAAC00` / `00DABAC0` / `00DBDE40` /
`NOVI_*` / `CS_OAKVALE_INTRO_FATHER` /
`00B23B50` are **not** on this list.
**PROVEN** (`No_save_does_not_activate_*`,
`PumpRunsDabaco=false`,
`FirstSeenCallsUseCamera=false`).

Do **not** invent a fiber start here.

### After a **proven** later construct (leftover)

Activator still **UNREAD**. This is **not**
Leave.

```
00DBEF70  alloc 0x10C
00DAAC00  [esi]=012D7A28          // slot 2 = 00DABAC0
00DAACE0  Main watcher
          +52 = 00CDD440
          +56 = S_QNOVI
type-1    watcher +16 = 00DAAD70  // NOT persist 00DAADA0
          call [esi+52]
            00CDD440 jmp [S_QNOVI.vtbl+8]
                     00DABAC0     // FIRST enter; 0 E8
00DABAC0
  00CB8230 ×16  NOVI_LiveFather … OVI_DeadFather
  00CB8930 flush
  [esi+64] vtbl+256
  READ [esi+80]                   // AttackOver; first-seen 0
  TEXT_QUEST_OAKVALE_INTRO_OBJECTIVE_01
  StartBarrelTimer 00CDD450 / +52=00DB4F70
  00DAC295 E8 00DBDE40            // still on this stack
TNG (names already registered)
  NOVI_LiveFather 004C97B0 → 00DAC2C0
    00DB86B0 00CBFB7D("CS_OAKVALE_INTRO_FATHER")
      … CameraPause FALSE
      NoLoadUseCamera CAM_OVI_ID_STANDUP
      UseCamera CAM_OVIF_SHOT2
        00B23B50 SNAP +536==0     // FOV 0.2 turns = 72°
00DBDE40
  map-wait "StartOakVale"
  READ [esi+80]                   // still 0
  CREATURE_HERO_CHILD + three watchers
  vtbl+2584(12.0)
  HerosOldHouse
  SPIN [esi+80]                   // leftover; no mov here
yield   [ctx+28] → 00A44690 → 009D8650
resume  00A44880 → 00A44660 → 009D87F0
        continues inside 00DBDE40 / CS runner
        NOT jmp [S_QNOVI.vtbl+8]
later   00DBB2A7 [quest+80]=1      // Theresa + raid; not first-seen
```

---

## 1. First enter is `00CDD440`; resume is not

`vtbl.tsv` / ExeIndex `012D7A28`:

| Off | Dest | Role |
|---:|---|---|
| +0 | `00DBEFA0` | dtor |
| +4 | `00DAACE0` | Main |
| **+8** | **`00DABAC0`** | slot 2 run |
| +12 | `00DAADD0` | reset; clears `+80` |
| +16 | `00DAADA0` | persist bind AttackOver |
| +24 | `00A44880` | update |
| +36 | `00DAAD70` | watcher `+16` thunk |

`calls-s-qnovi-run-00dabac0`: **0** `.text`
`E8`. First enter (`listing-00cc0000`):

```
00CDD440  mov eax, [ecx]
00CDD442  jmp [eax+8]
```

`ecx` is the factory (`watcher+56`). After
construct that vtbl is `012D7A28`, dest
`00DABAC0`. Watcher thunk
(`listing-00d80000`):

```
00DAAD70  mov ecx, [esi+56]       // S_QNOVI
00DAAD76  call [esi+52]           // 00CDD440
00DAAD79  mov [esi+5], 1          // AFTER 00DABAC0 RETURNS
```

Fiber `00A446A0` `this` is the **watcher**.
`[watcher+16]=00DAAD70`. S_QNOVI `vtbl+16`
`00DAADA0` is a different object.
`FiberCallsPersistThenRun=false`. **PROVEN**.

Yield park (`listing-00a40000`):

```
00A44690  call 009D8650
00A44660  mov [0x13D2838], ecx
00A44672  mov ecx, [ecx+16]
00A44675  call 009D87F0           // continue after park
00A44921  call 00A44660           // type-1 resume
```

`009D87F0` continues **inside**
`00A44840` / `006E7410` / `00DBDE40` /
Father runner. It does not
`jmp [S_QNOVI.vtbl+8]`.
`SqnoviReentersRunAfterYield=false`.
**PROVEN**.

No-save first `00CDD440` dest is Gameflow
`00CE7670`. **DISPROVEN** as this body.
Sibling `00CDD440-vtbl8-slot`.

---

## 2. What `00DABAC0` does before AttackOver

`listing-00d80000.txt` int3-bounded
`00DABAC0`–`00DAC2A1`. `esi=this` (quest).

### Name table (before any `+80` touch)

Each row: `00BFEA1A(0x28)`, vtbl
`0x012D8370`, CString name, `[+8]=quest`,
`[+16]=factory`, `[+20]=1`, then
`00CB8230`.

| # | Name | Factory | Store |
|--:|---|---|---|
| 1 | `NOVI_LiveFather` | `00DAC2C0` | `00DABB0C` |
| 2 | `NOVI_Theresa` | `00DAC420` | `00DABB6F` |
| 3 | `NOVI_Guard` | `00DAC580` | `00DABBD3` |
| 4 | `NOVI_Villager` | `00DADE50` | `00DABC37` |
| 5 | `NOVI_Bully` | `00DAEC60` | `00DABC9B` |
| 6 | `NOVI_Victim` | `00DAEDE0` | `00DABCFF` |
| 7 | `NOVI_TeddyGirl` | `00DAEF50` | `00DABD63` |
| 8 | `NOVI_AffairMan` | `00DB0880` | `00DABDCA` |
| 9 | `NOVI_AffairWoman` | `00DB1DB0` | `00DABE33` |
| 10 | `NOVI_AffairWife` | `00DB29A0` | `00DABE9D` |
| 11 | `NOVI_BookTrader` | `00DB3E30` | `00DABF07` |
| 12 | `NOVI_BarrelMan` | `00DB51B0` | `00DABF71` |
| 13 | `NOVI_BarrelThug` | `00DB6B40` | `00DABFDB` |
| 14 | `NOVI_Barrel` | `00DB7D00` | `00DAC045` |
| 15 | `NOVI_CreatedBeetle` | `00DB7FF0` | `00DAC0AF` |
| 16 | `OVI_DeadFather` | `00DB81B0` | `00DAC119` |

`DabacoRegistersBeforeSetup=true`.
`IntroQuestTngHasNoviNames=false`. Living
NPCs live on PreAttack TNG. **PROVEN**.

Tail before the READ:

```
00DAC146  mov ecx, esi
00DAC148  call 00CB8930           // name flush
00DAC14D  mov ecx, [esi+64]
00DAC152  call [edx+256]
```

No `UseCamera`. No `00CBFB7D`. No
`00DB86B0`. **PROVEN** absence.

### AttackOver READ (not write)

```
00DAC158  mov al, [esi+80]
00DAC15B  test al, al
00DAC15D  je  00DAC198            // first-seen 0 → skip
00DAC161  call 00CB7940
00DAC168  jne 00DAC29A            // abort → ret (skip 00DBDE40)
          "Q__OakValeIntro_PostAttack"
          call [eax+1120]
00DAC198  … objective + barrel timer + 00DBDE40
```

First-seen: ctor zeros / `00DAADA0` seed 0 /
slot 3 clear. Path falls through. **PROVEN**.

Persist bind is **not** in this fn:

```
00DAADA0  add ecx, 80
          push "AttackOver"
          mov [esp+15], 0
          call 004045C0
```

Store is **not** in this fn
(`FirstSeenPlus80WrittenInStartOakVale=false`):

```
00DBB2A7  mov [ecx+80], 1         // after Theresa + raid AVI
```

### After the false READ (still before the write)

```
00DAC1BA  "TEXT_QUEST_OAKVALE_INTRO_OBJECTIVE_01"
          [ctx] vtbl+2620 / +1184
00DAC22B  "StartBarrelTimer"
00DAC247  call 00CDD450
00DAC24C  mov [edi], 0x12D7A3C
00DAC252  mov [edi+52], 0xDB4F70
00DAC259  mov [edi+56], esi
00DAC274  call 00CB7E50
00DAC293  mov ecx, esi
00DAC295  call 00DBDE40           // only E8 of setup
00DAC2A1  ret
```

`00DBDE40` (still on `00DABAC0` stack)
map-waits `"StartOakVale"`, **READ**s `+80`
again, then kid + three watchers + PreAttack
+ `vtbl+2584(12.0)` + `HerosOldHouse` +
**SPIN** `+80`. Sibling
`intro-fiber-attackover` /
`00DBDE40-after-activate`. **PROVEN**
callee; **LEFTOVER** vs first Present.

---

## 3. Father CS is TNG construct, not this call

`00DAC2C0` is the LiveFather **factory**
stored at record `+16`. `00DABAC0` does
not `E8` it. Construct:

```
TNG CREATURE_HERO_FATHER / NOVI_LiveFather
004C97B0 → 00CB8960 → 00DB8520 → 00DAC2C0
  vtbl 0x012D8388
fiber 00DB8630 [+52].vtbl+4 = 00DB86B0
  00CBFB7D("CS_OAKVALE_INTRO_FATHER")
```

`ConstructStartsCutscene=true` only on that
row. Theresa `00DB97A0` first named
`M_TriggerOutro`. DeadFather `00DB8300`
`007E73F0("CS_DEAD_DAD")`, not `00CBFB7D`.
**DISPROVEN** as construct cutscenes.

Vector 0 (sibling
`cs-oakvale-intro-father-lines`): first
line `PlayMusic MUSIC_SET_NULL` (head
`FadeOut 0.5,0` special-case **miss**).
`PlayCombatAnimation TURNING_AC90` is
later (`playcombatanim-intro`). First
camera **command** is `CameraPause FALSE`.
First camera **bind** is
`NoLoadUseCamera CAM_OVI_ID_STANDUP`
(PC 18). `UseCamera CAM_OVIF_SHOT2` is
after Speak / InteractiveSpeak.

---

## 4. Father CS FOV 72? SHOT2 SNAP yes; no-save no

`UseCamera` leftover (`listing-00cc0000` /
`00b00000`):

```
00CC9F39  push "UseCamera"
          lookup TNG ScriptName
00B23B50  mov edi, [esp+12]       // helper
          mov ecx, [0x1436EA0]
          call 00B2FBF0           // [cam+12] = helper
          push 1
          call 00B314E0
00B314E9  mov al, [esi+536]
00B314EF  test al, al
00B314F1  je  00B31502            // SNAP helper +0/+12/+24
          call 00B31160           // PLAY — not taken
```

Ctor `00B31742` zeros `+536`.
`00B2FC10` (`+536=1`) has **0** `E8`.
`FirstSeenSplineEnabled=false`. **PROVEN**
SNAP. Sibling `usecamera-spline-first`.

SHOT2 TNG (`startoak-tng.txt` +
`WorldSceneTests`):

```
CAMERA_POINT_SCRIPTED_SPLINE  CAM_OVIF_SHOT2  (40.091, 130.258, 15.756)
CTCCameraPointScriptedSpline.FOV              = 0.2
CTCCameraPointScriptedSpline.KeyCameras[0].FOV = 0.2
HeroIsSubject = FALSE
```

`00B314E0` helper+44 × `360 × 1/360 × 2π`
(`00A0BE90`). `0.2` turns × 360 = **72°**.
`LandscapeFrustum.FirstSeenFovTurns=0.2`.
`RegionTravel.IntroCameraFovDegrees=72`.
`IntroFirstSeenCamera=CAM_OVIF_SHOT2`.
**PROVEN** leftover Oakvale intro view.

Not these:

| Item | Class |
|---|---|
| No-save first Present FOV 72 | **DISPROVEN** (Lookout helper 70° / `0x3E471B48`) |
| `FirstSeenCallsUseCamera` | **false** — no `00B23B50` on Leave |
| First leftover bind is SHOT2 | **DISPROVEN** (`NoLoadUseCamera CAM_OVI_ID_STANDUP`) |
| STANDUP FOV 72 | type spline **PROVEN**; property | **UNREAD** |
| SHOT3 non-spline FOV 72 | `CAMERA_POINT_SCRIPTED` | **UNREAD** here |
| Spline PLAY on SHOT2 `UseCamera` | `+536==0` | **DISPROVEN** |

`00DABAC0` itself never touches FOV.
**PROVEN**.

---

## 5. First-seen **if** `S_QNOVI` existed

Counterfactual after a **proven**
`00CB5AD0("Q_NewOakValeIntro")`. Activator
on no-save is still **UNREAD**. Do **not**
close that gap from Pump / Leave / ini.

Would run:

1. Ctor `00DAAC00` vtbl `012D7A28`.
2. One `00CDD440` into `00DABAC0`.
3. Name table **before** map-wait.
4. AttackOver **READ** 0; skip PostAttack
   name.
5. Objective + `StartBarrelTimer`.
6. `00DBDE40` map-wait / 12 s / `+80` spin
   (value still 0).
7. TNG Father-only CS; SHOT2 SNAP FOV 72
   leftover; `WaitActiveDialog` leftover
   inside the runner.
8. Yields resume via `00A44660` /
   `009D87F0`, not a second slot-2 enter.
9. Theresa / DeadFather / `00DBB2A7` stay
   **later**. First-seen AttackOver
   **false**.

Would **not** run on no-save first Present.
`QuestFactoryTable.Find` is the **bind**
row (`factory 00DBEF70`, run `00DABAC0`,
persist `false`). It is **not** an
instance. **PROVEN** bind;
**DISPROVEN** construct.

---

## Host

| Host | Native | Class |
|---|---|---|
| `IntroQuestRun = 00DABAC0` | slot 2 | **MATCH** VA; **not executed** no-save |
| `IntroQuestRunCallsSetup = 00DAC295` | only `E8` of `00DBDE40` | **MATCH** |
| `SqnoviMainWatcherThunk = 00CDD440` | first enter | **MATCH** |
| `SqnoviReentersRunAfterYield=false` | resume `00A44660` | **MATCH** |
| `FiberCallsPersistThenRun=false` | watcher `+16` ≠ `00DAADA0` | **MATCH** |
| `PumpRunsDabaco=false` | 0 `E8`; Pump skips | **MATCH** |
| `BindSqnoviFactory` names + persist | table only; no `E8 00DBDE40` | **MATCH** bind; fiber **LEFTOVER** |
| `FirstSeenPlus80WrittenInStartOakVale=false` | no `mov` in wait | **MATCH** |
| `FirstSeenAttackOverStoreRuns=false` | `00DBB2A7` later | **MATCH** |
| `ConstructStartsCutscene` Father only | `00DB86B0` | **MATCH** leftover |
| `IntroCameraFovDegrees=72` / turns `0.2` | SHOT2 TNG | **MATCH** leftover; **DISPROVEN** Leave |
| `FirstSeenCallsUseCamera=false` | no `00B23B50` no-save | **MATCH** |
| `FirstSeenSplineEnabled=false` / SNAP `+536` | `00B314E0` `je` | **MATCH** leftover bind |
| `ActivateQuest("Q_NewOakValeIntro")` as New Game | invents the object | **DISPROVEN** as first Present |

---

## Gap

| Item | Class |
|---|---|
| No-save construct of `S_QNOVI` | **DISPROVEN** (never on this walk) |
| Who later `00CB5AD0`s the name | **UNREAD** |
| `call [vtbl+8]` that **resumes** a yielded `00DABAC0` | **UNREAD** (PARITY 0b; resume is `009D87F0`) |
| STANDUP / SHOT3 FOV numbers | **UNREAD** as 72 |
| `00CB8930` / `vtbl+256` inner bodies | **PARTIAL** (order **PROVEN**) |

Do **not**:

- Start a `00DABAC0` fiber from Leave /
  Pump / first Present.
- `ActivateQuest("Q_NewOakValeIntro")` to
  reach Father CS FOV 72 on no-save.
- Re-enter slot 2 from `00A44660`.
- Write `AttackOver=1` from this body.
- Collapse Lookout 70° into SHOT2 72°.

---

## Sources (absolute)

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00d80000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00cc0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a40000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00b00000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\calls-s-qnovi-run-00dabac0-00dabac0.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\s-qnovi-slot2-run-00dabac0-00dabac0.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\vtbl-s-qnovi-vtbl-012d7a28-012d7a28.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-bank\native-sqnovi.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-bank\0481-cs-oakvale-intro-father.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\startoak-tng.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\proofs\00CDD440-vtbl8-slot\README.md`
- `C:\FableCSharp\proofs\intro-fiber-attackover\README.md`
- `C:\FableCSharp\proofs\playcombatanim-intro\README.md`
- `C:\FableCSharp\proofs\cs-oakvale-intro-father-lines\README.md`
- `C:\FableCSharp\proofs\00DAAC00-sqnovi-no-save\README.md`
- `C:\FableCSharp\proofs\usecamera-spline-first\README.md`
- `C:\FableCSharp\proofs\00DBB2A7-attackover-store\README.md`
