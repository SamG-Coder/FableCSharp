# First AddObject / create-thing script command after Leave

Investigation only. No production `src/` edits.

Do **not** start at `CS_OAKVALE_INTRO_FATHER` /
`Create CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH,…,VILL1`
/ `00DB86B0`. That path is later leftover `Q_NewOakValeIntro`
(`00DABAC0` → `00DBDE40` → TNG `NOVI_LiveFather`).
Leave is `0042F2A2`. First no-save pumps do not enter
`00CBFB7D`, so they never match `Create` / `ObjectCreate`.

There is **no** exe token `AddObject`. Creature-action RTTI
`CActionEventAddObject@NCreatureAction` (`0x0137F234`) is
not a runner verb.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources:

- `src/Fable.Game/Scripting/GlobalDispatcher.cs`
  (`ApplyCreate` / `ObjectCreate` / `CreateNear` / `CreateEffect`
  / `CreateLight` / `CrowdCreate`)
- `ScriptCommandMap.cs` (`Create` `00CCC246` / `ObjectCreate`
  `00CCC4FC`)
- `RegionTravel.cs` (`CreateOpcode`, `IntroCreate*`,
  `FirstSeenCreateDoesNotYield`)
- `docs/runtime/COMMAND_MAP.md`, `COMMAND_MAP.generated.md`
- `tools/Fable.ExeIndex/out/01-sections/script-runtime/`
  (`create-token-00ccc246`, `create-apply-00ccc3e6`,
  `create-vtbl364-008a9100`, `command-continue-join-00cd17f8`)
- `text-map/listing-00cc0000.txt` `00CCC246`–`00CCC64A`
- `text-map/listing-00880000.txt` `008A9100`
- `script-bank/0481-cs-oakvale-intro-father.md`,
  `exe-commands.md` (`0x012C1D14` `Create`, `0x012C1D04`
  `ObjectCreate`)
- `proofs/script-interpreter`, `script-global-cmds`,
  `script-entity-cmds`, `script-command-map`, `script-bindings`,
  `newgame-script`, `tng-first-def`, `tng-spawn`
- `WorldSceneTests` (`Create_villager_records_args_and_does_not_yield`,
  leftover father walk); `EngineLifecycleTests`
  (`No_save_does_not_activate_Q_NewOakValeIntro`)

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| First `Create` / `ObjectCreate` / `CreateNear` / `CreateEffect` / `CreateLight` / `CrowdCreate*` after Leave? | **none** — runner not on the tree | **PROVEN** |
| First Thing those cmds would spawn after Leave? | **none** | **PROVEN** |
| First *constructed* CThing after Leave? | TNG `NewThing` `TRACK_NODE_BASIC` `GuardTrack` via `0051FD80` | **PROVEN** spawn; **DISPROVEN** as a script verb |
| First leftover create-thing line (not Leave)? | `Create CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH,MK_OVI_ID_VS1,VILL1` | **PROVEN** leftover |
| First leftover `ObjectCreate`? | not on father def | **DISPROVEN** as leftover-first |
| Token `AddObject`? | **no** — not in `0x012C1500`–`0x012C2C00` | **DISPROVEN** |
| Host `GlobalDispatcher.ApplyCreate` after `DispatchFrontendMessage(15)`? | unused | **LEFTOVER** vs Leave |

`Create` is the host analog of token `00CCC246`
(`00BFEAF8` vs `"Create"` at `0x012C1D14`). Apply is
`00CCC3E6` → context `vtbl+364` `008A9100`. Family is
Global (no `target.`). Native only reaches it from runner
`00CBFB7D`. Leave starts quest factories, not that loop.

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend                 // no Create / no 00CCC246
0042F491 Init Game → 004184BD
  00416953 Load FinalAlbion.wld
    00CD6E27 00CB5C90 bind Q_NewOakValeIntro / S_QNOVI   BIND ONLY
    00507C30 START_INITIAL_QUESTS → world+172
  004B4260 six WLD names
    CS_PlayCutscene 00F01760 empty                      // no CCutsceneDef
    S_PSM / S_PSGT / S_HB HasStarted==false
  user.ini ActivateQuest("Gameflow")
    00CE75B0 Main; 00CE7670 state 0 yields
004189C2 first pumps
  00CB8220 → 00A44880
  no 00CBFB7D
later (E8 caller UNREAD) 00501450 LookoutPoint
  00521AE0 / 00520D00 NewThing loop
  0051FD80 TRACK_NODE_BASIC                             // TNG, not Create
  … later 006AC910 CREATURE_HERO ScriptName=Hero
later leftover (not this tree)
  00DB86B0 → 00CBFB7D
    Create …,VILL1                                      // first leftover Create
```

`00CCC246` / `ObjectCreate` / `CActionEventAddObject` /
Oakvale `VILL1` are **not** on the Leave list. **PROVEN.**

---

## 1. What the create-thing family is

`C:\FableCSharp\src\Fable.Game\Scripting\GlobalDispatcher.cs`

Host `if Eq(verb)` for **global** spawn lines. Family is not
a native opcode class. `ScriptLine.Parse` sets
`Family = Global` when the unquoted head has **no** `.`.

Native runner chain (`listing-00cc0000.txt`):

| Token | VA | String | Factory | Join |
|---|---|---|---|---|
| `CreateNear` | `00CCBEE7` | `0x012C1D3C` | `vtbl+368` | `00CC864B` |
| `Create` | `00CCC246` | `0x012C1D14` | `vtbl+364` `008A9100` | `00CD17F8` |
| `ObjectCreate` | `00CCC4FC` | `0x012C1D04` | `vtbl+392` | `00CC864B` |
| `CrowdCreateMixed` | `00CCC64D` | `0x012C1CF0` | per-item `vtbl+364` | — |
| `CrowdCreate` | `00CCC92F` | `0x012C1CE4` | `vtbl+300` then `vtbl+364` | — |
| `CreateEffect` | `00CCBB9A` | `0x012C1D54` | `vtbl+400` | `00CC864B` |
| `DummyEffect` | `00CCBD62` | `0x012C1D48` | `vtbl+404` | `00CC864B` |
| `CreateLight` | `00CCB933` | `0x012C1D64` | `vtbl+408` | `00CC864B` |

`AddObject` is **not** in this table. RTTI
`CActionEventAddObject@NCreatureAction` (`0x0137F234`) is
an `NCreatureAction` event class. **DISPROVEN** as
`00CBFB7D` token.

`TNG` `NewThing` (`00520D00` → `0051FD80`) is a **file
block**, not a runner verb. First after Leave is
`TRACK_NODE_BASIC` (`proofs/tng-first-def`). Do not rename
it `Create`.

Host `World.Spawn` / `Runtime.AddThing` / `BindCreated` is
the leftover apply façade. Spawn mesh body of `008A9100`
is **PARTIAL** (`PARITY` 0b “Create `008A9100` mesh UNREAD”).

---

## 2. After Leave — no create-thing command

| Claim | Class | Evidence |
|---|---|---|
| `00CBFB7D` on Leave / Init Game / first pumps | **DISPROVEN** | `script-interpreter` §2A; no `E8` |
| `CS_PlayCutscene` runs a `CCutsceneDef` | **DISPROVEN** | factory `00F01760` size 72; `ScriptName==null` |
| `S_PSM` / `S_PSGT` / `S_HB` / `S_GF` interpreter | **DISPROVEN** | `HasStarted==false`; Gameflow yields |
| Any `Create` / `ObjectCreate` / `CreateNear` / `CreateEffect` | **DISPROVEN** | no runner; `BindCreated` unused (`script-bindings`) |
| `ScriptRuntime.StartNewGame` as Leave | **DIVERGE** | leftover Oakvale VM (`newgame-script`) |
| First live Thing is a script `Create` | **DISPROVEN** | TNG `0051FD80` / later `006AC910` |

Things that *do* exist after Leave are **not** script-created:

| Object | After Leave | From `Create` / `ObjectCreate`? |
|---|---|---|
| `TRACK_NODE_BASIC` `GuardTrack` | first `0051FD80` | **DISPROVEN** (TNG) |
| Lookout `CREATURE_HERO` / `ScriptName=Hero` / 4299 | later `006AC910` | **DISPROVEN** |
| `NOVI_LiveFather` / leftover `VILL1` | not constructed | **DISPROVEN** |

**Answer:** after Leave there is **no** create-thing script
command and **no** Thing those commands would name.

---

## 3. Native `Create` `00CCC246` (leftover opcode)

Dump: `create-token-00ccc246-00ccc246.md` +
`listing-00cc0000.txt` `00CCC246`–`00CCC4F9`.

```
00CCC246  push "Create"
          00BFEAF8 vs [ebp+96]
          miss → 00CCC4F9 ObjectCreate
00CCC29A  arg0 type / arg1 marker / arg2 name required
          empty any → jmp 00CD17FD
          copy name; arg4 suffix 0099EFB0
          IsTrue(arg5) → 00CD3187 already-bound? skip spawn
          marker: HERO → vtbl+280 else vtbl+288
          004AA980 pos
00CCC3E6  call [esi+364]                 // 008A9100
          004AB130 valid? else cleanup
          empty|IsTrue(arg3) → extras 008ADF90
          IsFalse(arg6) → skip bind (still activate)
          else:
            vtbl+2048(handle,2)
            vtbl+32(…,4)
            vtbl+1896                     // heading leftover
            00CD3D2E / 008ABD10           // persist bind
          vtbl+2148 activate
          jmp 00CD17F8                    // CreateJoin; no yield
```

Args: `type,marker,name[,extra][,suffix][,unique][,IsFalse]`.

| Piece | Class |
|---|---|
| Token / three-required / no `vtbl+28` | **PROVEN** (`FirstSeenCreateDoesNotYield=true`) |
| `vtbl+364` = `008A9100` | **PROVEN** pairing |
| `008A9100` → `00513160` def lookup → `00833800` construct | **PROVEN** call; mesh / CTC attach **PARTIAL** |
| Unique skip `00CD3187` / extras `008ADF90` / bind `00CD3D2E` | **PROVEN** script-layer |
| Host `ApplyCreate` records `World.Spawn` + `BindCreated` | **EQUIVALENT** args; **PARTIAL** vs `vtbl+2048`/`+32`/`+1896`/`+2148` |

`00CD17F8` is `call 0099EAE0` then falls into loop continue
`00CD17FD`. **PROVEN** CompleteNow.

`ObjectCreate` (`00CCC4FC`): same three required args,
`vtbl+392` (not 364), **no** extras / unique / `00CD3D2E`,
`jmp 00CC864B`. Host still `BindCreated` — **DIVERGE** vs
this apply (native does not insert the actor map here).

`CreateNear` is `vtbl+368` (not 364/392). `CreateEffect` is
`vtbl+400`. Do not collapse them.

---

## 4. Leftover first *interpreter* create (not Leave)

When `Q_NewOakValeIntro` later runs, `00DABAC0` registers
`NOVI_LiveFather` → fiber `00DB8630` → `00DB86B0` pushes
`CS_OAKVALE_INTRO_FATHER` into `00CBFB7D`.

Dump: `0481-cs-oakvale-intro-father.md`. Host walk:
`WorldSceneTests` after `Hero.PlayAnimation CS_LOOK_LEFT`.

Head of def+60 is Global (`PlayMusic` / `FadeOut` /
`CameraPause`) then Entity (`Hero.Teleport` …). First
create-thing line on that def:

| Order | Raw | Verb | Type | Marker | Name | `GlobalDispatcher` |
|---:|---|---|---|---|---|---|
| after `CS_LOOK_LEFT` | `Create CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH,MK_OVI_ID_VS1,VILL1` | `Create` | villager mesh | `MK_OVI_ID_VS1` | **VILL1** | `ApplyCreate`; Continue; extras (arg3 empty); bind (arg6 empty) |

Constants: `RegionTravel.IntroCreateType` /
`IntroCreateMarker` / `IntroCreateName`. Same-tick
`VILL1.WalkTo MK_OVI_ID_VW1` then yield on `GamePause 0.8`.
Later `Remove VILL1`.

No `ObjectCreate` / `CreateNear` / `CreateEffect` /
`CrowdCreate` on this def. **PROVEN** leftover.

Do **not** pair leftover `VILL1` to Lookout TNG or to
`006AC910` Hero. **DISPROVEN.**

Bank-index `CS_ATTRACT_1` also starts with `Create`, but
that is frontend attract, **before** Leave. **DISPROVEN**
as after-Leave.

---

## 5. C# vs native (Leave path)

| Host | Native after msg 15 | Class |
|---|---|---|
| `GlobalDispatcher.ApplyCreate` | unused | **LEFTOVER** vs Leave |
| `World.Spawn` / `Runtime.AddThing` | unused | **LEFTOVER** vs Leave |
| `Bindings.BindCreated` | no `00CD3D2E` | **DISPROVEN** as Leave bind |
| `StartNewGame` → father `Create …,VILL1` | invented Oakvale VM | **DIVERGE** |
| empty Creates / no `ObjectCreate` | no `00CBFB7D` | **EQUIVALENT** absence |
| `0051FD80` NewThing | same TNG walk | **PROVEN** (not this verb) |

Tests that assert leftover `intro.Executed` contains the
VILL1 `Create` are **PROVEN** as Oakvale VM behaviour and
**DISPROVEN** as what Leave starts.

---

## Classifications (short)

1. **First create-thing script command after Leave — none.
   PROVEN.** `Create` / `ObjectCreate` live only inside
   `00CBFB7D`.
2. **There is no `AddObject` runner token. PROVEN** absence
   vs exe ASCII. `CActionEventAddObject` is creature-action
   RTTI, **DISPROVEN** as this command.
3. **First leftover create — `Create …,VILL1` on
   `CS_OAKVALE_INTRO_FATHER`. PROVEN leftover.** Not
   `ObjectCreate`. Not Lookout `NewThing`.
4. **Native apply is `vtbl+364` `008A9100` then
   `jmp 00CD17F8` (no yield). PROVEN** script-layer.
   Mesh construct **PARTIAL**.
5. **TNG `NewThing` / `006AC910` Hero after Leave are
   spawns, not this opcode. PROVEN** other path.

Do not start New Game at `Create …,VILL1`. Do not treat
first `0051FD80` as `Create`.
