# First script fiber / thread create after Leave

Investigation only. No production `src/` edits.

Do **not** start at `S_QNOVI` / `00DBDE40` / `00A447D0` as the
first-seen create. That recreate slot is later and **0 `E8`**.
Oakvale is leftover vs Leave / Init Game / first `004B4260`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `docs/runtime/FORWARD_TREE.md` §§7–11, 15;
`tools/Fable.ExeIndex/out/01-sections/script-runtime/`
(`microthread-ctor-00a44740`, `microthread-create-00a447d0`,
`microthread-fiber-entry-00a446a0`, `microthread-update-00a44880`,
`watcher-ctor-00cdd450`, `calls-microthread-create-00a447d0`);
`listing-00500000.txt` `00507C30`;
`listing-006c0000.txt` `006C26B0`;
`listing-00480000.txt` `004B4260` / `004B3CE0`;
`listing-00c80000.txt` `00CB5AD0` / `00CB7900`;
`listing-00cc0000.txt` `00CDD380` / `00CE1A30` / `00CE75B0`;
`ScriptScheduler.cs` / `ScriptRuntime.cs` / `EngineLifecycle.cs`;
`EngineLifecycleTests` (`Init_quests_004B4260_activates_wld_initial_list`,
`Activate_quests_00CB5AD0_starts_factory_scripts`);
`proofs/camera-after-leave`, `proofs/newgame-script`,
`proofs/script-factory-tables`.

---

## Verdict

Three different “creates.” Do not collapse them.

| Event | First no-save site after Leave | vs `00CB5AD0` | Class |
|---|---|---|---|
| First `00A44740` / `009D8710(00A446A0)` | `00507C30` `+188==0` → `006C26B0` (36-byte world-map object, stack `0xFA00`) | **before** | **PROVEN** |
| First *quest watcher* fiber | `Q_SunnyvaleMaster` `00CDD380` → `00CDD450` → `00A44740` (60-byte, stack 64, flag 1) | **inside** `004B4260` after the lookup loop | **PROVEN** |
| `00A447D0` “create” | vtbl slot only (watcher `+12`, S_QNOVI `+32`); 0 `E8` | **not first-seen** | **DISPROVEN** as first create |

`00CB5AD0` is **lookup**. It does not allocate a fiber.
Fiber create for quests is `004B3CE0` → `00CB7900` `jmp [vtbl+4]`
→ `Main` → `00CDD450`. That is still the same `004B4260`
“QuestManager: Activate Quest” call, **after** every name has
been `00CB5AD0`’d and `004BB720` queued.

Frontend / Leave does **not** create a script fiber. **PROVEN**
absence (`00CBFB7D` / `00CDD450` / `00507C30` not on `0042EC7C`).

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend                 // no 00A44740
0042F491 Init Game 004184BD
  Init World 004A6E30
    004A6550 Init Scripts 006E7740 → world+56
    00CB5D80 / 00CD52D0 table FILL      // bind only; no fiber
  00416953 Load world FinalAlbion.wld
    004A1840
      world-map vtbl+12 00507C30
        [map+188]==0                    // first-seen
          alloc 36
          006C26B0 → 00A44740           // FIRST FIBER
            00A445D0 base
            vtbl 0125D9B0
            +16 = 009D8710(00A446A0)
            [0x13D283C]++
          0050FB90 store map+188
        "Load .wld file"
        START_INITIAL_QUESTS → world+172  // names only
      004A0D90 AddQuest → world+184
    [0x13B8648]==0
      0049F180 Init Characters / Init GUI
      004B4260([world+172])             // FIRST QUEST ACTIVATE
        loop:
          "QuestManager: Activate Quest"
          004B00C0 predicate
          [manager+120] 00CB5AD0        // lookup; eax=record or 0
          004BB720 enqueue 12-byte
        004B3CE0 walk queue             // FIRST QUEST FIBER
          [0x1375454]=1 .data
          factory construct + run
          004B3FEC 00CB7900
            [vtbl+12] then jmp [vtbl+4]
          Q_SunnyvaleMaster first:
            00CDBA10 persist zeros
            vtbl+4 00CDD380 Main
              00CDD450 → 00A44740       // first script watcher
              00CB7E50 attach
        PersonalScriptMain 00CDE380
        PersonalScript_GlobalThings …
        HeroBoasts 00CE1A30
        V_HeroDolls / CS_PlayCutscene
      00416BCF +90584 empty skip 004B4A10
    user.ini 009EC890
      ActivateQuest("Gameflow")
        00419CE0 [world+56] vtbl+1104 00892E80
        004B4A10 → 004B4260 → 00CB5AD0
        00CE75B0 Main 00CDD450          // later watcher
004189C2 first pumps
  00CB8220 / 00CB7950 +41=0
  vtbl+4 00A44880 PUMP                  // not create
  00A44660 resume [0x13D2838]
```

`Q_NewOakValeIntro` / `S_QNOVI` / `00A447D0` as first create
are **not** on this list. **PROVEN**.

---

## 1. Dump: script-runtime microthread

From `tools/Fable.ExeIndex/out/01-sections/script-runtime/` v59.

### `00A44740` ctor — actual first create

```
00A44740  push args (flag, stack, 0.1f) → 00A445D0
          [this] = 0129D440
          [this+16] = 0
          009D8710(edx=this, ecx=00A446A0) → [this+16]
          miss → "Failed to create fibre" / 00BFEB84
          [0x13D283C]++
          ret 12
```

**PROVEN.** This is the function that calls the fibre allocator.

### `00A447D0` recreate — not first-seen

```
00A447D0  if [this+16]  009D8640 destroy
          009D8710(00A446A0) → [this+16]
          [this+5] = 0                  // not +41
          miss → "Failed to create Microthread"
```

`calls-microthread-create-00a447d0`: **hits 0**.
`calls-microthread-fiber-00a446a0`: **hits 0** (entry is passed
to `009D8710`, never `E8`’d).

Watcher vtbl `012D7A3C+12` = `00A447D0`.
S_QNOVI vtbl `012D7A28+32` = `00A447D0`.
`00CB78D0` writes `+41`; `00A447D0` writes `+5`. **PROVEN**.

### `00A446A0` fiber entry

```
if [this+5]==0:
  [this+4]=1
  [vtbl+16] setup
  loop: if [this+5]==0  [vtbl+8] run
[this+4]=0
009D8650  park
```

### `00A44880` pump (later)

```
if [0x13D2838]  yield-path 00A44690
else
  00A44930 has-work
  009E1BC0 → [this+8] dt
  00A44660 resume [this+16] via 009D87F0
```

Create is **not** this function. First `00A44880` is type-1
`00CB7950` after activate. **PROVEN**.

### Direct `00A44740` `E8` sites

| Site | Wrapper | Object | First-seen after Leave |
|---|---|---|---|
| `006C26BF` | `006C26B0` flag 0, stack `0xFA00`, 0.1f, vtbl `0125D9B0`, size 36 | world-map `+188` | **first** — `00507C30` prologue |
| `0088C173` | `0088C160` flag 0, stack `0x7D00`, vtbl `012780C4` | thing `00833095` | **PARTIAL** order vs TNG inside `00507C30` (after `+188`) |
| `00CDD45C` | `00CDD450` flag 1, stack 64, 0.1f, vtbl `012C2F9C` | quest `Main` watcher | first **script** — `004B3CE0` |
| `00CE112D` | in-place watcher (flag 1, stack 64) | same family | later |
| `00E95F1C` | clone of `00CDD450` | V_HeroDolls-class | later WLD quest |

---

## 2. Relative to quest activate

`00CB5AD0` (`listing-00c80000.txt`):

```
ecx = [manager+120]
00CB65D0 search
string cmp / 00429950
hit → lea eax, [edi+4]
miss → eax = 0
```

No alloc. No `00A44740`. **PROVEN**.

`004B4260` (`listing-00480000.txt`):

```
for each [world+172] name:
  log "QuestManager: Activate Quest"
  004B00C0
  00CB5AD0
  004BB720
004B4386  call 004B3CE0          // once, after the loop
```

`004B3CE0` at `004B3FEC` `call 00CB7900`:

```
00CB7900  call [vtbl+12]
          jmp [vtbl+4]            // Main
```

First WLD name is `Q_SunnyvaleMaster`. Its `Main` is `00CDD380`
(`listing-00cc0000.txt` immediately after factory vtbl `012C2F64`):

```
00CDD380  alloc 60
          00CDD450("Main")        // 00A44740
          vtbl 012C2F78
          +52 = 00CDD440
          00CB7E50 attach
```

Same pattern later: `PersonalScriptMain` `00CDE380`,
`HeroBoasts` `00CE1A30`, `Gameflow` `00CE75B0`.

So:

| Question | Answer | Class |
|---|---|---|
| Does `00CB5AD0` create a fiber? | No | **DISPROVEN** |
| When is the first *quest* fiber? | Tail of the same `004B4260`, inside `004B3CE0` / `00CB7900`, first queued name | **PROVEN** |
| When is the first *any* fiber after Leave? | `00507C30` world-map `006C26B0`, during `004A1840`, **before** `0049F180` and **before** `004B4260` | **PROVEN** |
| Is `00A447D0` that site? | No | **DISPROVEN** |

`00416BCF` “Activate Initial Quests” is empty `+90584` and
**skips** `004B4A10`. It does not create another fiber.
Gameflow is a **second** `004B4260` from user.ini, not the first.

---

## 3. Host `ScriptScheduler` / `Scripting/`

`C:\FableCSharp\src\Fable.Game\Scripting\ScriptScheduler.cs`

| Host | Native analog | Class |
|---|---|---|
| `Scheduler.Create` | intended `00A447D0`; live first-seen is `00A44740` | **DIVERGE** name |
| `FiberState.DtAtPlus8` | `00A44880` `fstp [ecx+8]` | **PROVEN** layout |
| `Pump` → `resume` | `00A44880` → `00A44660` | **PARTIAL** (host pumps every interpreter per fiber) |
| `QuestInstance.AttachFiber` | `00CB7E50` attach after `00CDD450` | **PARTIAL** |
| `ScriptRuntime.ActivateQuest` | `00CB5AD0` + `004B3CE0` + `00CB7900` | **PARTIAL** (creates fiber in the same host call as lookup) |
| `CreateFiber` + `Scheduler.Create` (two lists) | one native object | **DIVERGE** |
| `InstallRecoveredBindings` / `ScriptFiberTable` `S_QNOVI` | not on Leave | **LEFTOVER** |
| `EngineLifecycle.ActivateNamedQuest` | `004B4260` WLD list + user.ini Gameflow | **PROVEN** pairing of *when* quests start |
| Host fiber at `00507C30` / `006C26B0` | none | **DIVERGE** (missing) |

Tests after `EnterGame`: `Runtime.Scheduler.Fibers.Count == 7`
(6 WLD + Gameflow). That counts **quest** fibers only and
labels them `00A447D0`. Native already created the world-map
fibre before those 7. Host does not model `map+188`.

`ScriptRuntime.StartNewGame` → `InstallRecoveredBindings`
still plants `S_QNOVI`. Leave path does not. **DIVERGE**.
`EngineLifecycle` does not call it. **PROVEN**.

`NewGameScript.CreateFiber = 00A447D0` is the vtbl recreate
constant, not the first-seen ctor. **LEFTOVER** pairing.

---

## 4. What not to treat as first fiber

| Claim | Class |
|---|---|
| Frontend / Leave creates a cutscene or `S_QNOVI` fiber | **DISPROVEN** |
| First create is `00A447D0` | **DISPROVEN** (0 `E8`; ctor already filled `+16`) |
| First create is `00A44880` | **DISPROVEN** (pump) |
| First create is Gameflow `00CE75B0` | **DISPROVEN** (7th WLD+ini construct) |
| First create is `00DBDE40` / Oakvale | **DISPROVEN** |
| `00CB5AD0` creates the thread | **DISPROVEN** (lookup) |
| First *script watcher* is Sunnyvale `Main` `00CDD450` during `004B3CE0` | **PROVEN** |
| First *microthread* after Leave is world-map `006C26B0` in `00507C30` | **PROVEN** |

`006C26B0` body as a *script* runner (what `[vtbl+8]` / `+16`
do on vtbl `0125D9B0`) is **UNREAD**. It is still the same
`00A44740` fibre object. Do not invent it as `CCutsceneDef`.

---

## Classifications (short)

1. **First fiber after Leave — `00507C30` → `006C26B0` → `00A44740`. PROVEN.** Before Init Quests.
2. **First script/quest watcher fiber — `Q_SunnyvaleMaster` `00CDD380` / `00CDD450` inside `004B3CE0`. PROVEN.** After every `00CB5AD0` in that `004B4260`, not inside the lookup.
3. **`00A447D0` as first create — DISPROVEN.** Recreate vtbl; 0 `E8`.
4. **Host `ScriptScheduler.Create` at `ActivateQuest` — PARTIAL vs quest watchers, DIVERGE vs missing `006C26B0` and vs `00A447D0` label.**
