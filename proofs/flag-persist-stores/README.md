# FlagStore / PersistStore first write after Leave

Investigation only. No production `src` edits.
Do **not** start at Oakvale / `S_QNOVI` / `AttackOver` / `00DAADA0`.
That path is later `Q_NewOakValeIntro` (`00DABAC0` → `00DBDE40`),
not Leave / Init Game / first no-save 3D Present.

Do **not** treat CUIDef file persist (`00631C60` / `005331A0`) or
CWorld `HEADER` persist (`TeleportingEnabled` …) as `PersistStore`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER**.

Sources: `FlagStore.cs` / `PersistStore.cs` / `PersistTable` /
`QuestFactoryTable` / `GlobalDispatcher` / `EngineLifecycle`;
`docs/runtime/FORWARD_TREE.md` §§2, 4–6, 10;
`docs/status/README.md` (`00CDC070`);
`proofs/camera-after-leave/README.md`;
ExeIndex `listing-00400000` / `00880000` / `00cc0000` / `01200000`.

---

## Timeline (no-save New Game)

```
004012CE  CRT static ctors
  0121F0E0  alloc 24 → [0x13BAE2C]   // FlagStore map (empty)
  0121F120  alloc 24 → [0x13BAE38]   // gossip sibling
  0121F160  alloc 32 → [0x13BAE44]   // script-state map
00402510  bootstrap
  Setup Language → 004045C0 LeftAlignText / NoHangulWordWrap / DisableCapsLock
                  // CPersistContext, not PersistStore
0042EC7C  frontend pump (2D UI). No 008ADF10. No quest persist.
0042F2A2  Leave frontend
004184BD  Init Game → Init World → 00416953 load FinalAlbion.wld
  0049F180  Init Characters  (CWorld HEADER 004045C0 — not PersistStore)
  0043A380  Init GUI
  004B4260  Init Quests  START_INITIAL_QUESTS
    Q_SunnyvaleMaster first
      00CDBA10  zeros slot bytes + _LIKE/_HATE     // first PersistStore value write
      00CDC070  vtbl persist bind 004045C0/00410BE0 // named transfer; 0 E8
    PersonalScriptMain / PersonalScript_GlobalThings / HeroBoasts /
    V_HeroDolls / CS_PlayCutscene
  user.ini ActivateQuest Gameflow
    00CE6CF0  vtbl+2868 → 008A9DB0 → 008AE660 [0x13BAE44]  OV_INTRO…
              // not FlagStore
004189C2  first pumps. S_PSM / S_GF / S_HB HasStarted=false.
          SetFlag / WaitFlag not first-seen.
```

`00DAADA0` `AttackOver` / `S_QNOVI` are **not** on this list. **PROVEN**.

---

## Native objects vs C#

| C# | Native | Class |
|---|---|---|
| `FlagStore` | Named byte map **`[0x13BAE2C]`**. Lookup/insert `008ADF10` → node+20. SetFlag `00CCA4C8` `mov [eax],0/1`. Wrappers `008A96C0` / `008AE060` (0 E8; vtbl). | **PROVEN** |
| sibling map | `[0x13BAE38]` `IsGossipForPlayer` (`007ABC0E` persist name). Same `008ADF10` helper. **Not** in C# `FlagStore`. | **PROVEN** map. **UNREAD** first write after Leave. |
| script states | `[0x13BAE44]` insert `008AE660` via `008A9DB0` / Gameflow vtbl+2868. First names `OV_INTRO`…. **Not** `FlagStore`. | **PROVEN** |
| `PersistStore` | Flattened named slots on quest objects. Bool `004045C0`, int `00410BE0`. First recovered table is `Q_SunnyvaleMaster` `00CDC070` / zeros `00CDBA10`. | **PROVEN** as table. Not one heap singleton. |
| `004045C0` | Generic persist **transfer** (`00404500` name CRC, `[esi+24]` mode jmp). Language, CWorld HEADER, quest slots all use it. | **PROVEN**. Not “the persist store”. |
| CUIDef persist | `00631C60` widget file (`0043314A` u8). Frontend.bin. | **DISPROVEN** as `PersistStore` |
| `008ADF90` | 12-byte vector grow. Cutscene runner uses it. | **DISPROVEN** as FlagStore |

`008ADF10` (**PROVEN**):

```
ecx = map (usually 0x13BAE2C)
007ACBB0(name) lookup
miss / name mismatch → 008ACA90 insert, default byte 0
return esi+20
```

SetFlag `00CCA4C8` (**PROVEN**): required args else `00CD17FD`. `IsTrue(arg2)` and `[ebp-39]` skip rewrite → `00CC907D`. `IsFalse(arg1)` → `008ADF10; mov [eax],0` else `mov [eax],1`. Always `jmp 00CC907D`.

WaitFlag `00CCB893` (**PROVEN**): `008ADF10; cmp [eax],bl`. Match `00CD17FD`. Else leftover `00CCB8CE`. Insert-on-miss writes 0. Not a timer. Not persist.

---

## 1. Writes during frontend?

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D UI; no cutscene interpreter | **PROVEN** | `camera-after-leave`; `0042DF9E`; `00CBFB7D` not on `0042EC7C` |
| `call 008ADF10` in `listing-004*.txt` / `listing-005*.txt` | **DISPROVEN** | zero sites. SetFlag/WaitFlag live in `00CC*` |
| `call 004045C0` on retail/frontend pump `0042xxxx` | **DISPROVEN** | zero sites |
| Bootstrap `004045C0` language | **PROVEN** and **before** frontend | `00402785` `LeftAlignText`; `NoHangulWordWrap`; `DisableCapsLock` |
| `0059787F` `004045C0` TeleportingEnabled / SavingEnabled / … | **PROVEN** CWorld `HEADER`. **DISPROVEN** as `PersistStore`. **DISPROVEN** as Press Start pump (no `E8` from `0042EC7C`). Same field list as `0049F652` Init Characters. | |
| Flag maps exist during frontend | **PROVEN** construct. **DISPROVEN** as written. | CRT `0121F0E0` empty head: `[eax]=0`, `+4=0`, `+8/+12=self` |
| Host `ScriptRuntime` / `FlagStore` / `PersistStore` on frontend frames | **DISPROVEN** live path | `Runtime` allocated in `InitCharactersAndQuests` after Leave. `RequestNewGame` does not construct it. |

**Answer:** no FlagStore write and no PersistStore slot write during frontend. Language `004045C0` is earlier bootstrap. CUIDef persist is a different store.

---

## 2. First PersistStore write after Leave

Not `AttackOver`. First *constructed* persist-slot object is `Q_SunnyvaleMaster` (first `START_INITIAL_QUESTS` name).

| Order | Site | What | Class |
|---|---|---|---|
| 1 | `00CDBA10` run.vtbl+8 from `004B3CE0` | `xor ebx,ebx` then `[esi+17]` … `[esi+80]` … `[esi+292]` = 0. `_LIKE` / `_HATE` at `0x143E938` / `0x143E93C`. `+17` is `HauntedBarrowFieldsCompleted`. | **PROVEN** first value write |
| 2 | `00CDC070` persist vtbl (0 E8) | `004045C0` / `00410BE0` names starting `PostSavePosition` / `HauntedBarrowFieldsCompleted`. Defaults via `0040E240` / `0040E160`. | **PROVEN** serializer. **UNREAD** whether no-save activate walks it or only save/load. Host `Note`s it on activate. |
| later | `00DAADA0` | `004045C0("AttackOver", this+80)` stack default 0. Writer of `+80=1` still UNREAD. | **LEFTOVER** vs this site |

`PersistTable.Sunnyvale` is 38 slots. `00CDC070` also transfers names **not** in that table (`PostSavePosition`, `ArcheryStateCurrent`, `TeddySolution`, …). Host table is **PARTIAL**.

CWorld `0049F180` / `0049F652` `004045C0` (`TeleportingEnabled` default `bl`) runs **earlier** in the same `00416953` (Init Characters before Init Quests). That is **not** `PersistStore`.

**Answer:** first PersistStore *value* write after Leave is `00CDBA10` zeros on `Q_SunnyvaleMaster`. First *named transfer* is `00CDC070` if the persist context visits on construct (UNREAD as first-seen call). `AttackOver` is not first.

---

## 3. First FlagStore write after Leave

| Site | When | Class |
|---|---|---|
| CRT `0121F0E0` | empty map object | **PROVEN** construct. Not a named-byte write. |
| Gameflow `00CE6CF0` | after initial quests / user.ini `ActivateQuest` | **DISPROVEN** as FlagStore. Writes `[0x13BAE44]` via `008AE660`. |
| SetFlag `00CCA522` / `00CCA5A6` | needs `00CBFB7D` (or same opcode table) on a started `CCutsceneDef` | **DISPROVEN** first-seen. `HasStarted(S_PSM/S_GF/S_HB)==false`. `CS_PlayCutscene` factory empty. |
| WaitFlag `00CCB922` insert-0 | same interpreter | **DISPROVEN** first-seen |
| `008A96C0` / `008AE060` | write helpers | **UNREAD** first caller (0 E8; vtbl) |
| Gossip persist `007ABBD0` `ActiveGossipCategories` | serializes the **whole** `[0x13BAE2C]` | **UNREAD** as first after Leave. Empty map → no name insert. |

First *named byte* store into `[0x13BAE2C]` on the no-save Leave path is **UNREAD**. It is **not** frontend and **not** Gameflow seed.

**Answer:** FlagStore is live (empty) from CRT. After Leave the first proven sibling write is script-state `008AE660` (`OV_INTRO`), which C# does not put in `FlagStore`. First `008ADF10` data write is later than first Present / first quest construct.

---

## 4. C# vs native on this path

| Site | What it does | Class |
|---|---|---|
| `new FlagStore()` / `new PersistStore()` at `ScriptRuntime` ctor | empty dictionaries | Host-only. Native maps already exist from CRT. |
| `EngineLifecycle.Runtime` | created in `InitCharactersAndQuests` after Leave | **EQUIVALENT** timing vs frontend (null before Leave) |
| `ActivateQuest` + `Persist.Install(Sunnyvale)` | defaults 0 / false | **EQUIVALENT** to `00CDBA10` zeros. Label `00CDC070` is the bind, not the zeroing. |
| `CreateFiber(..., persist=questName)` | may `SetBool("Q_SunnyvaleMaster", false)` | **LEFTOVER** extra slot. Native zeros fields on the object, no dictionary key by quest name. |
| `InstallRecovered` / `AttackOver` | `StartNewGame` / `FirstSceneWorld` | **LEFTOVER**. Live `EnterGame` does not call `StartNewGame`. No `S_QNOVI` on first activate. |
| `PersistTable.AttackOverWriterKnown=false` | `+80` writer UNREAD | **PROVEN** still |
| `Flags.Set` from SetFlag | unused on first no-save pumps | **EQUIVALENT** (nothing to write) |

---

## Classifications (short)

1. **Frontend FlagStore / PersistStore write — DISPROVEN.** Maps exist empty from CRT. `008ADF10` and quest `00CDC070` are not on `0042EC7C`. CUIDef persist is a different system. Language `004045C0` is bootstrap.

2. **First PersistStore write after Leave — `00CDBA10` zeros on `Q_SunnyvaleMaster`. PROVEN.** Named bind `00CDC070` is the serializer (call on no-save **UNREAD**). `AttackOver` / `00DAADA0` **DISPROVEN** as this site.

3. **First FlagStore (`[0x13BAE2C]`) named write after Leave — UNREAD.** Gameflow `008AE660` is `[0x13BAE44]`. SetFlag/WaitFlag **DISPROVEN** first-seen.

4. **C# during frontend — DISPROVEN** (`Runtime` null). **LEFTOVER** AttackOver install on `StartNewGame` only.
