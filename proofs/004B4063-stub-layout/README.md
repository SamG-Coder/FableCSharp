# `004B4063` 52-byte factory-0 stub: field map

Investigation only. No production `src/` edits.

Question: `004B4063` 52-byte factory-0 stub — field map,
name `+48`, `[+4]=[+8]=0`, `[+36]=1`, link
`[manager+56]`. Host `CreateFiber` leftover vs this stub?

Do **not** invent a factory / `Main` / fiber for
`ChapterAndSceneManager` / `NPCDeath`. They have **no**
PE string and **no** `00CD52D0` row.

Do **not** invent a `Started=false` skip of this alloc
or of `[manager+56]`. That flag is not this branch
(`proofs/factory0-construct`).

Do **not** start at `S_QNOVI` / `00DBDE40` /
`Q_NewOakValeIntro`. Those names are not on the no-save
`world+172` walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: Fable.exe
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00480000.txt`
`004B3CE0` / `004B3EA9` / `004B3EE7` / `004B4063`–
`004B4116` / `004B3F4B` / `004B0310` / `004B03F0`;
`listing-00980000.txt` `0099EC30`;
`listing-00bc0000.txt` `00BFEA1A` / `00BFEA0E`;
`proofs/factory0-construct`;
also `004B3CE0-factory0`, `factory0-enqueue`,
`factory0-stub-vs-fiber`;
`ScriptRuntime.ActivateQuest` /
`EngineLifecycle.ActivateNamedQuest`.

`factory0-construct` closed stub vs skip vs
`Started=false`. This note is the **byte map** of the
52-byte object and the host `CreateFiber` leftover
against that map.

---

## Verdict table

| Question | Answer | Class |
|---|---|---|
| 52-byte stub? | **Yes.** `004B4063` `push 52` / `00BFEA1A`. Same size as live `004B0310`. | **PROVEN** |
| Field map? | `[+0]=id`, `[+4…+24]=0`, `[+36]=1`, `[+37]=[rec+9]`, `[+40]=[+44]=0`, name CString at `+48`. | **PROVEN** |
| Name at `+48`? | **Yes.** `lea ecx,[esi+48]` / `0099EC30` copies `[rec+0]` CString (4-byte ptr). | **PROVEN** |
| `[+4]=[+8]=0`? | **Yes.** `ebx` is 0 (`004B3EA9` `xor ebx,ebx` before `je 004B4063`). No factory / no run. | **PROVEN** |
| `[+36]=1`? | **Yes.** `004B409F` `mov [esi+36], 0x01`. Same byte as live `004B0310`. | **PROVEN** |
| Link `[manager+56]`? | **Yes.** 16-byte node `00BFEA0E(16)` spliced onto sentinel `[ebp+56]`. Payload `{quest, wrapper}`. | **PROVEN** |
| `[+28]` / `[+32]`? | **Unwritten** by stub and by `004B0310`. Allocator contents. | **UNREAD** |
| Host `CreateFiber` leftover vs this stub? | **Yes, mutation.** `ActivateQuest` always `CreateFiber` / `Scheduler.Create` before `Find`. Stub has no `00CB7900` / no `[+8]` run. | **LEFTOVER** / **DIVERGE** |

**52-byte named slot. Not a fiber. Host fiber is leftover.**

---

## Timeline (no-save, first `004B4260`)

```
004B4386  call 004B3CE0
  second loop 004B3E82:
    004AF610 already-on-+56?  jne 004B417A   // skip only
    004B3EA9  xor ebx, ebx
    004B3EE4  cmp [edi+4], ebx
    004B3EE7  je 004B4063                   // factory 0
      ChapterAndSceneManager  stub
      NPCDeath                stub
```

Factory arm (`[+4]!=0`) is `004B3F4B` `push 52` then
`004B0310` + `00CB7900` + `00687540`. Stub never
reaches it (`factory0-construct`).

---

## 1. Field map (`listing-00480000.txt` `004B4063`)

`ebx=0` at entry. `eax` id is `[ebp+132]++` then:

```
004B4063  mov eax, [ebp+132]
004B406D  inc eax
004B406E  push 52
004B4070  mov [ebp+132], eax
004B4076  call 00BFEA1A              // jmp [0x1440150]
…
004B4084  mov al, [edi+9]
004B408B  mov [esi], ecx             // id (not a vtbl)
004B408D  mov [esi+4], ebx           // 0  factory
004B4090  mov [esi+8], ebx           // 0  run / fiber this
004B4093  mov [esi+12], ebx
004B4096  mov [esi+16], ebx
004B4099  mov [esi+20], ebx
004B409C  mov [esi+24], ebx
004B409F  mov [esi+36], 0x01
004B40A3  mov [esi+37], al           // [rec+9]; 1 on Init Quests
004B40A7  lea ecx, [esi+48]
004B40AA  mov [esi+40], ebx
004B40AD  mov [esi+44], ebx
004B40B0  call 0099EC30              // push edi; name only
```

| Off | Store | Meaning |
|---:|---|---|
| `+0` | `[manager+132]++` | id |
| `+4` | `0` | no factory record |
| `+8` | `0` | no run object |
| `+12` | `0` | no `+8` wrapper (live `004B0310` puts one here) |
| `+16` | `0` | |
| `+20` | `0` | |
| `+24` | `0` | |
| `+28` | — | unwritten |
| `+32` | — | unwritten |
| `+36` | `1` | same as live |
| `+37` | `[rec+9]` | `1` on this walk (`004B4260` arg2) |
| `+40` | `0` | live may store extra ptr |
| `+44` | `0` | live may store `+40` wrapper |
| `+48` | CString | `[rec+0]` via `0099EC30` |

Fill matches inlined `004B03F0` (same zeros / `[+36]=1` /
name `+48`). Size matches live `004B0310`. Difference is
`[+4]/[+8]` contents, not size (`factory0-construct`).

`0099EC30` (`listing-00980000.txt`): `[dst]=0` then
`[dst]=[src]` and `inc [string+13]`. Four-byte CString
fits `52-48`. **PROVEN** name at `+48`.

`00BFEA1A` is IAT alloc. Whether it zeros `+28`/`+32`
is **UNREAD**. Neither stub nor live ctor writes them.

---

## 2. Link `[manager+56]`

```
004B40C7  mov [eax], 0x1             // wrapper
004B40CD  mov [eax+4], 0x4BAEF0
004B40D4  mov [eax+8], esi           // quest stub
…
004B40E5  mov esi, [ebp+56]          // sentinel
004B40E8  push 16
004B40EA  call 00BFEA0E              // jmp [0x1440158]
004B40EF  lea ecx, [eax+8]
004B40FD  mov [ecx], edx             // quest*
004B4105  mov [ecx+4], edx           // wrapper*
004B410A  inc [edx]
004B410C  mov ecx, [esi+4]
004B410F  mov [eax], esi
004B4111  mov [eax+4], ecx
004B4114  mov [ecx], eax
004B4116  mov [esi+4], eax            // splice tail
```

Same list as live factory objects. Later type-1
`004B4490` visits the node and `je` when `[quest+8]==0`
(`factory0-type1-tick`). That walk is not this construct.

`00CB8690` on this arm needs `[obj+8]!=0`. Stub stores
0. **PROVEN** no fiber helper.

Skip of the 52-byte alloc is **only** `004AF610`
already-in-`+56`. First-seen those two names are not
on `+56`. **PROVEN** stub, not skip.

---

## 3. Host `CreateFiber` leftover vs this stub

Native factory 0 never calls `00CB7900` / `00A44740`.
`[+8]` stays 0. That **is** the object.

Host `ScriptRuntime.ActivateQuest` (every
`ActivateNamedQuest` name, including the two stubs):

```
CreateFiber(name, persist);
quest = new QuestInstance(++_questId, name, persist);
state = Scheduler.Create(name, persist);
quest.AttachFiber(state);
if (QuestFactoryTable.Find(name) is { } bind)
    quest.StartFactory(…);            // miss for these two
```

`ActivateNamedQuest` then always `EventPosts++` /
`00687540` Note.

| Host | Native stub | Class |
|---|---|---|
| `CreateFiber` | no `00CB7900`; `[+8]=0` | **LEFTOVER** / **DIVERGE** |
| `Scheduler.Create` / `AttachFiber` | no watcher | **LEFTOVER** / **DIVERGE** |
| `EventPosts++` | no `00687540` | **LEFTOVER** / **DIVERGE** |
| `QuestInstance` still exists | 52-byte named slot on `+56` | **MATCH** existence |
| `Find` miss / `Started=false` | `[+4]=[+8]=0` | **MATCH** no-run, **not** skip |
| `Started` as `[+36]` | stub stores **1** | **DISPROVEN** |

Host lock `Fibers.Count==10` / `EventPosts==10` counts
the two stubs as fibers. Native first-seen is **8**
fibers / **8** posts (`factory0-construct` /
`factory0-stub-vs-fiber`). The extra two **are** this
layout: named `[manager+56]` slots without a run.

Do **not** implement a skip from `Started=false`.
Do **not** invent a factory so `CreateFiber` becomes
true. Do **not** treat `CreateFiber` as this stub.

---

## What this is not

| Claim | Class |
|---|---|
| Factory 0 is skipped (no 52-byte alloc) | **DISPROVEN** |
| `[+4]` / `[+8]` hold a factory / run | **DISPROVEN** |
| `[+0]` is a vtbl / fiber `this` | **DISPROVEN** |
| `[+36]` is 0 / host `Started=false` | **DISPROVEN** |
| Name lives somewhere other than `+48` | **DISPROVEN** |
| Stub is not linked on `[manager+56]` | **DISPROVEN** |
| Host `CreateFiber` MATCH this stub | **DISPROVEN** |
| `Started=false` skip of this layout | **DISPROVEN** (`factory0-construct`) |
| Second registrar fills those two names | **UNREAD** — do not invent |

---

## Classifications (short)

1. **`004B4063` field map is the 52-byte named slot
   — PROVEN.** Name `+48`. `[+4]=[+8]=0`. `[+36]=1`.
   Linked on `[manager+56]`.
2. **`[+28]` / `[+32]` unwritten — UNREAD.**
3. **Host `CreateFiber` on those names is leftover
   vs this stub — PROVEN LEFTOVER / DIVERGE.**
