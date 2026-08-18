# ScriptBindings.cs first bind after Leave

Investigation only. No production `src/` edits.

Do **not** start at `00DB86B0` / `Hero`+`Father` / `00CD3D2E`.
That is later leftover `Q_NewOakValeIntro` (`00DABAC0` →
`NOVI_LiveFather`), not Leave / Init Game / first no-save Present.

Do **not** treat `00CB5C90` / `00CD52D0` as `ScriptBindings`.
That is `QuestFactoryTable` (already
`proofs/script-factory-tables/README.md`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/Scripting/ScriptBindings.cs`,
`ScriptRuntime.BindScene` / `BindRuntimeHero`,
`GlobalDispatcher` (`RegisterActor` / `Create` / `Crowd*`),
`ExecutionContext.FindThing`, `QuestFactoryTable.cs`,
`docs/runtime/FORWARD_TREE.md` §§7–11;
`proofs/script-factory-tables`, `proofs/script-interpreter`,
`proofs/newgame-script`, `proofs/cutscene-first`;
`EngineLifecycleTests` (`Init_quests_004B4260_*`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`,
`Install_banks_and_startup_videos_exist` Resolve `HERO`/`Hero`);
ExeIndex `listing-00c80000.txt` `00CBF9DE`,
`listing-00cc0000.txt` `00CC669B` / `00CE180D` / `00CEE92E`,
`listing-00480000.txt` `004AC860`,
`script-runtime` `00CD3D2E` / `008ABD10` / `00CDBF70` / `006E7740`.

---

## Verdict

**Leave does not write the live name environment.**

`ScriptBindings` is the host analog of:

| Role | Native |
|---|---|
| Resolve | `00CBF9DE` → `HERO` is `[0x143E8F8]` **vtbl+280** + `008AB980`; else persist `00CD2B86` then **vtbl+288** |
| Cutscene / Create actor map | `00CDBF70` ctor, `00CD3D2E` insert, `008ABD10` slot |
| RegisterActor | `00CC662D` / apply `00CC669B` → **`004AC860`** (only `E8`) |

None of those `E8`s sit on the no-save Leave tree.

First *host* bind is `BindRuntimeHero` at `006AC910` spawn
(after `006C2170` TNG). That is **not** `00CD3D2E`.
Native `HERO` is a getter (`vtbl+280`), filled when
`CThingPlayerCreature` exists.

| Question | Answer | Class |
|---|---|---|
| First `00CD3D2E` / `008ABD10` / `004AC860` after Leave? | **none** | **PROVEN** absence |
| First `00CB5C90` after Leave? | Init World `00CD52D0` row 1 `Q_SunnyvaleMaster` | **PROVEN** — **not** this type |
| First host `ScriptBindings.Bind*` after Leave? | `BindScene` then `BindHero` at `006AC910` | **PROVEN** host |
| Native pair of that `BindHero`? | player create `006AC910`; resolve `00CBF9DE` / vtbl+280 | **PROVEN** getter / **UNREAD** writer |
| `00DB883A` `Hero`/`Father`? | leftover Oakvale | **DISPROVEN** first-seen |

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  Init World 004A6E30
    004A6550 / 004A6638  006E7740 world+56 vtbl 01260F0C
                         // interface ctor; +60 empty list; no name bind
    004A6677  00CB5D80 / 00CD52D0 / 161× 00CB5C90
                         // QuestFactoryTable — NOT ScriptBindings
  00416953 Load FinalAlbion.wld
    0049F180 Init Characters / Init GUI
    004B4260 START_INITIAL_QUESTS
      00CB5AD0 six WLD names + fibers
      no 00CD3D2E / 00CBF9DE / 004AC860
    user.ini ActivateQuest("Gameflow")
      00CE75B0 Main; 00CE7670 state 0 yield
004189C2 first pumps
  dummy index 0; no TNG; Bindings still empty vs native
00501450 / 006C2170 apply Lookout ContainsMap
  00521AE0 / 0051FD80 / 004CA010 / 00662880
    TNG ScriptName lives on the Thing (vtbl+288 later)
  HOLY_SITE GuildArrivalHSP → 006AC910
    host BindRuntimeHero: BindScene then BindHero(HERO, Hero)
```

`00CBFB7D` / `00CC0768` `00CD3D2E` / `00DB883A` are
**not** on this list. **PROVEN**.

---

## 1. What `ScriptBindings.cs` actually is

`C:\FableCSharp\src\Fable.Game\Scripting\ScriptBindings.cs`

Live **thing-name** slots. Comment on the type is accurate:

globals (`HERO`), `RegisterActor`, Create aliases,
`CrowdAcquire` + `SPECTATORCS0..n`, invocation-local overwrite.

| Method | Kind | Native pair | First after Leave? |
|---|---|---|---|
| `BindHero` | Global `HERO` + `thing.ScriptName` | `00CBF9DE` `HERO` → vtbl+280; `"Hero"` is the Thing name, not a second table | host **yes** at `006AC910`; native getter **yes** once player exists; `00CD3D2E` **no** |
| `BindSceneThing` | Scene | TNG `ScriptName` on `0051FD80` construct; resolve vtbl+288 | host **yes** (ContainsMap names before `BindHero`); native **no** `00CD3D2E` at load |
| `RegisterActor` | Registered | `00CC662D` / `00CC669B` / `004AC860` | **DISPROVEN** (`004AC860` only from `00CC669B`) |
| `BindCreated` | Created | `00CCC29A` / `00CCC3E6` `vtbl+364`; not `IsFalse(arg6)` → `00CD3D2E` | **DISPROVEN** (no runner) |
| `BindCrowd` / index | Crowd | `CrowdAcquire` `00CCCEA7`; `CrowdCreate` `00CCC92F` + `00CD3D2E` | **DISPROVEN** |
| `BindAcquired` | Acquired | later acquire verbs | **DISPROVEN** |
| `Unbind` | — | `Remove` / `RemoveThing` | **DISPROVEN** |
| `Local` enum | Local | invocation overwrite **UNREAD** as a first-seen helper | unused first-seen |

`FindThing` = `Bindings.Resolve` then `Runtime.FindThingByName`.
That is the `00CBF9DE` split (map / HERO / world name). **PROVEN**
as a pairing. **DISPROVEN** as a Leave callee.

---

## 2. Native resolve (`00CBF9DE`) — not a bind

`listing-00c80000.txt`:

```
00CBF9DE  push ebp
  ebx = name
  [ebx]==0 → 5-byte cmpsb vs "HERO"
  [ebx]!=0 → 00411570 vs "HERO"
  match → [0x143E8F8] vtbl+280 → 008AB980
  else 00CD2B86 persist/actor map
    hit → [eax+20].vtbl+48
    miss → [0x143E8F8] vtbl+288(name)
  004ABE90 copy into out-handle
```

| Claim | Class |
|---|---|
| `HERO` is special-cased to vtbl+280 | **PROVEN** |
| Other names are vtbl+288 (and/or `00CD2B86`) | **PROVEN** |
| `004A93C0` is the HERO *table* | **DISPROVEN** (CString compare helper used by many verbs) |
| `00CBF9DE` runs on Leave / first pumps | **DISPROVEN** (only `00CC*` interpreter / later verbs) |
| `006E7740` writes HERO | **DISPROVEN** (vtbl `01260F0C`; `+60` empty list; no name) |

Who *writes* the pointer vtbl+280 returns is **UNREAD**.
Create `006AC910` is the first no-save player object.
Treat host `BindHero` as that identity, not as `00CD3D2E`.

---

## 3. Native bind (`00CD3D2E` / `008ABD10` / `00CDBF70`)

Cutscene / Create **actor map**. Generic.

| VA | Role | First `E8` after Leave |
|---|---|---|
| `00CDBF70` | 36-byte map ctor | **UNREAD** as a first-seen *use*; sites in first-seen *files* are later bodies |
| `00CD3D2E` | insert name → slot | **DISPROVEN** on Leave tree |
| `008ABD10` | write slot / handle | same |
| `008AB1E0` / `0099A3B0` | CString / handle copy around the slot | helpers |

Zero `E8` from `004*` / `005*` / `006*` thing construct.
`0051FD80` / `004CA010` / `00662880` / `006AC910` do **not**
call `00CD3D2E`. **PROVEN**.

`004AC860` is **not** this map. Body: `004AC380` then
`[out]=ptr`, `[out+4]=dl`. Only caller `00CC669B`
(`RegisterActor`). **PROVEN**.

---

## 4. Later leftovers that *do* bind names (not Leave)

Do not file these as first-seen.

| Site | Names | When |
|---|---|---|
| `00CC0768` + `008ABD10` | runner local (`[ebp+124]>1`) | inside `00CBFB7D` *before* `.WaitTask` — leftover if father starts |
| `00CE180D` | `"HERO"` then `CS_STANDING_STONE` | `00CE15E0` (`Q_MinionCamp` / `GenerateKeyHere` / `OBJECT_SILVER_KEY`) — not first `004B4260` tick |
| `00CEE92E` | `"Hero"` then `CS_FABLE_CREDITS` | late Gameflow; **skipped** if `00CB7940` hero-exists |
| `00DB883A` / `00DB886D` | `Hero` / `Father` | `00DB86B0` Oakvale leftover |
| `00CCC4AC` / `00CCCB28` / … | Create / Crowd aliases | interpreter verbs |

`00CB7940` is `[this+44]` then `[hero+5]` — a **predicate**,
not a bind. **PROVEN**.

---

## 5. Host after Leave vs native

`EngineLifecycle.BindRuntimeHero` (`SpawnHero` after HSP):

```
Runtime.BindScene(_regionThings, null);  // every ScriptName
Runtime.Bindings.BindHero(Hero);         // HERO + "Hero"
World.Positions["HERO"] / ["Hero"] = HSP
```

ContainsMap order is Bridge → Lookout → Guild, then appended
hero. First host `Bind` is the first TNG `ScriptName` in that
walk, then `HERO`/`Hero`. Exact first TNG name is **UNREAD**
here (many NewThings have empty `ScriptName`).

| Host | Native after Leave | Class |
|---|---|---|
| `006E7740` construct only | same; no slots | **PROVEN** |
| `00CB5C90` 161-row fill | `QuestFactoryTable` | **PROVEN** other type |
| `BindScene` of 464 TNG + hero | Thing `ScriptName` field; vtbl+288 later | **PARTIAL**. Host table is extra |
| `BindHero` at `006AC910` | player object; vtbl+280 getter | **PROVEN** identity / **DIVERGE** as `00CD3D2E` |
| `InstallRecoveredBindings` / `NOVI_*` | not on Leave | **DIVERGE** (`StartNewGame` only) |
| `RegisterActor` / Create / Crowd | no `00CBFB7D` | **PROVEN** unused |
| Test `Resolve("HERO")==life.Hero` | host policy after spawn | **PROVEN** host; **PARTIAL** vs native getter |

`LoadQuestsAndActivate` still does not bind (quests before TNG).
Spawn is the first host write. **PROVEN**.

---

## Classifications (short)

1. **`ScriptBindings` ≠ `00CB5C90`. PROVEN.**
   Quest fill is `QuestFactoryTable`. Live names are
   `00CBF9DE` / vtbl+280/288 and `00CD3D2E` actor maps.
2. **First native name-env *write* after Leave — none.
   PROVEN.** No `00CD3D2E`, `008ABD10`, `004AC860`,
   `00CBF9DE` on Leave / `004B4260` / first pumps.
3. **First host bind — `BindScene` then `BindHero` at
   `006AC910`. PROVEN.** Pair to player create + HERO
   getter, **not** Oakvale `00CD3D2E`.
4. **`00DB86B0` `Hero`/`Father`, `00CE180D` `HERO`+standing
   stone, `00CEE92E` credits — LEFTOVER.**
5. **vtbl+280 writer — UNREAD.** Do not invent a
   `00CD3D2E("HERO")` from Leave.
