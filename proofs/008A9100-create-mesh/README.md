# `008A9100` Create mesh (`vtbl+364`)

Investigation only. No production `src/` edits.

Do **not** treat this as first Leave spawn. After no-save
Leave the runner `00CBFB7D` is not entered
(`proofs/script-addobject`, `npc-first-create`). This body
is leftover apply for `Create` once the intro fiber runs.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: who calls `008A9100` from intro fiber /
`00CBFB7D` / `00DB86B0`? What does it construct? Host
equivalent / gap?

Sources:

- `assembly/exe/01-sections/text-map/listing-00880000.txt` `008A9100`
- `listing-00800000.txt` `00833800` / `00831F80` / `00830360`
- `listing-00cc0000.txt` `00CCC246`–`00CCC4F9`
- `listing-00500000.txt` `00513160`; `listing-00980000.txt` `009AD410`
- `listing-004c0000.txt` `004C7E50` / `004C9CA0`; `listing-00640000.txt` `00662880`
- `assembly/exe/00-index/vtbl.tsv` `0x01260F0C` slot 91
- `tools/Fable.ExeIndex/out/01-sections/script-runtime/`
  `create-vtbl364-008a9100-008a9100.md`,
  `create-token-00ccc246-00ccc246.md`,
  `create-apply-00ccc3e6-00ccc3e6.md`,
  `00db86b0-calls-runner-00db88db-00db88db.md`,
  `cutscene-runner-exact-00cbfb7d-00cbfb7d.md`
- `script-bank/0481-cs-oakvale-intro-father.md`
- `src/Fable.Game/Scripting/GlobalDispatcher.cs` `ApplyCreate`
- `Scripting/ExecutionContext.cs` `World.Spawn`
- `ScriptRuntime.cs` `IScriptHost.Create`
- `RegionTravel.cs` `CreateOpcode` / `CreateApplyFn` / `IntroCreate*`
- `proofs/script-addobject`, `npc-first-create`, `script-entity-cmds`
- `WorldSceneTests.Create_villager_records_args_and_does_not_yield`

---

## Verdict

**Intro fiber does not `E8 008A9100`. PROVEN.**

`00DB86B0` at `00DB88F8` calls runner `00CBFB7D` with
`CS_OAKVALE_INTRO_FATHER`. The runner token `"Create"`
`00CCC246` apply `00CCC3E6` is `call [esi+364]` on
`[0x143E8F8]` (`CGameScriptInterface` vtbl `01260F0C`).
Slot 91 (`364/4`) is **`008A9100`. PROVEN.**

First leftover `Create` on that def:

`Create CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH,MK_OVI_ID_VS1,VILL1`

`008A9100` is **not** a MeshFile loader. It looks up the
type def, then **`00833800` always allocates `CThingAICreature`
size `0x1D8` via `004C7380` + ctor `00831F80`**, copies the
marker pose to `thing+232`, inserts (`00662880` /
`008388D0`), and activates (`004C9CA0`). **PROVEN**
construct class. Graphic `.msh` ID for this def is
**UNREAD**.

Host equivalent is `GlobalDispatcher.ApplyCreate` →
`World.Spawn` (record `ThingInstance`). It does **not**
run `00833800` / `00831F80` / `004C9CA0`. **DIVERGE**
mesh; **EQUIVALENT** args + CompleteNow.

| Claim | Class |
|---|---|
| `vtbl.tsv` `01260F0C` slot 91 = `008A9100` | **PROVEN** |
| Direct `E8 008A9100` from `00DB86B0` / `00CBFB7D` | **DISPROVEN** (none in listings) |
| Intro path `00DB86B0` → `00CBFB7D` → `00CCC246` → `00CCC3E6` → `vtbl+364` | **PROVEN** leftover |
| `Create` last stack arg is `edi==0` so in-fn extras `0074DBB0`/`006644F0` skip | **PROVEN** |
| Always `CThingAICreature` `0x1D8` / `00831F80` | **PROVEN** |
| Graphic mesh ID / CTC graphic attach | **UNREAD** |
| Host `World.Spawn` records type/name/pos; no native Thing | **DIVERGE** mesh |

---

## Timeline (leftover intro fiber, not Leave)

```
00DABAC0  S_QNOVI run  register NOVI_LiveFather
00DB86B0  CS_OAKVALE_INTRO_FATHER start
  00DB88DB  push "CS_OAKVALE_INTRO_FATHER"
  00DB88F8  call 00CBFB7D                 // runner; xor edi,edi
    … PlayMusic / FadeOut / … / Hero.PlayAnimation CS_LOOK_LEFT
    00CCC246  token "Create"
    00CCC29A  type, marker, name required else 00CD17FD
    00CCC3C1  push edi                    // 0 = last 008A9100 flag
              004AA980 marker pos
    00CCC3E6  call [esi+364]              // 008A9100
              004AB130 valid?
              empty|IsTrue(arg3) 008ADF90 extras
              not IsFalse(arg6) 00CD3D2E bind
              vtbl+2148 activate
    00CCC4F4  jmp 00CD17F8                // CompleteNow; no yield
    next    VILL1.WalkTo MK_OVI_ID_VW1
later     Remove VILL1
```

Leave never reaches this list. **PROVEN** leftover.

---

## 1. Callers

`008A9100` has **no** `E8` site in `text-map`. Only vtbl
install:

| Vtbl | Slot | Byte off | Target |
|---|---:|---:|---|
| `01260F0C` (`CGameScriptInterface` / `[0x143E8F8]`) | 91 | 364 | `008A9100` |

`vtbl.tsv` has **one** row for `0x008A9100`. **PROVEN**
unique pairing.

### Intro fiber (`00DB86B0`)

`00DB88F8 call 00CBFB7D` with name
`CS_OAKVALE_INTRO_FATHER` (`00db86b0-calls-runner`).
`00DB86B0` never loads `esi+364`. **DISPROVEN** as a
direct caller.

### Runner (`00CBFB7D`)

`00CBFB8F xor edi, edi`. Token compare at `00CCC246`
(`push "Create"` `0x012C1D14`). Apply:

```
00CCC3BA  eax = [0x143E8F8]
00CCC3BF  esi = [eax]                    // vtbl 01260F0C
00CCC3C1  push edi                       // 0
00CCC3C2  push name                      // ebp-592 (arg2 + suffix)
00CCC3CF  call 004AA980                  // marker → pos
00CCC3DA  push eax                       // pos
00CCC3DB  push type                      // ebp+40
00CCC3DF  push out                       // ebp-2092
00CCC3E6  call [esi+364]                 // 008A9100
```

**PROVEN** only intro-fiber site.

Same slot is also used later by leftover
`CrowdCreateMixed` (`00CCC7A8` / `00CCC7EE`),
`CrowdCreate` (`00CCCAA1`), and non-intro
`[reg+364]` sites (`00CFEC51` Necropolis, quest
factories, …). Those are **not** this cutscene line.

---

## 2. Args (`ret 20` = 5 dwords, thiscall)

`008A9100` `sub esp, 0x150` then reads `[esp+344]`
**before** the four saved regs = **arg1**.

| Stack | At apply `00CCC3E6` | Intro leftover |
|---|---|---|
| `ecx` | `[0x143E8F8]` | script iface |
| arg0 | out handle (`ebp-2092`) | written `0x1238C8C` + ptrs |
| arg1 | type CString* (`ebp+40`) | `CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH` |
| arg2 | pos from `004AA980` | `MK_OVI_ID_VS1` navigator |
| arg3 | name CString* (`ebp-592`) | `VILL1` |
| arg4 byte | `edi` | **0** |

Script `Create` args
`type,marker,name[,extra][,suffix][,unique][,IsFalse]`:

| Script | Native before `008A9100` |
|---|---|
| empty type/marker/name | `jmp 00CD17FD` — **no call** |
| arg4 suffix | `0099EFB0` onto name |
| IsTrue(arg5) | `00CD3187` already-bound → skip spawn |
| arg1 marker | `HERO` → `vtbl+280` else `vtbl+288`; `004AA980` pos |
| arg3 extra | **after** return: empty\|IsTrue → `008ADF90` |
| IsFalse(arg6) | skip `00CD3D2E` bind |

Intro line has no extra/suffix/unique/IsFalse.
**PROVEN** three-arg spawn + extras + bind.

---

## 3. Body (`listing-00880000.txt` `008A9100`–`008A9311`)

```
ecx = this
009AD410([this+16], type)     // hash map [mgr+104/+108]; miss 009E5170
0099A2D0                      // empty handle scratch
00513160([this+16], id, out)  // CThingDef*; al=0 → skip to write-out
                              // ebp = def
pack two empty CStrings 0x122D70E
0099EFB0(name onto packed)    // arg3
00833800(ecx=id, edx=pos,
         [def+284], packed)   // ALWAYS AICreature
004C7E50 + 008AB980           // wrap handle (QM [0x13B89FC]+4)
arg4==0 → skip 0074DBB0 / 006644F0
[this+76] times call [vtbl+28]
copy scratch → arg0 handle (vtbl 0x1238C8C)
eax = arg0
ret 20
```

Def miss (`je 008A9291`) still writes an empty handle.
**PROVEN.**

### `00833800` — the construct mesh

```
004C7380(0x1D8)
00831F80                      // CThingAICreature ctor (vtbl 0127293C)
copy 12 bytes pos → thing+232
00830360(thing, type, pos, def+284, name)
  00662880 → 008388D0 insert
  004C7990 + 00513160 bind def onto thing+224
fld [def+80] → thing+176/+180
0099EFB0 brain name at thing+400 from [def+284]+12
[thing+96] vtbl+12 via 006A06E0   // navigator pose
004C9CA0 activate
  vtbl+32 / +36 / +40
  or world insert 005202B0 / 0051E000
```

Same size/ctor as TNG `AICreature` (`proofs/npc-first-create`).
**DISPROVEN** as generic `NewThing` / `PlayerCreature`
`006AC910`. **DISPROVEN** as MeshFile open.

`008A9100` does **not** take a graphic mesh id. The
script type string ends in `_MESH` because that is the
**def name**, not a `.msh` record. Which graphic id
`008388D0` / CTC then attaches is **UNREAD**.

In-fn extras (`arg4!=0` → `0074DBB0` + `006644F0`) are
**DISPROVEN** for script `Create` (`push edi` / Crowd
`push 0`). Script extras are the later `008ADF90` on
the returned handle.

`[this+76]` × `vtbl+28` after spawn is **PROVEN** as a
loop; the concrete `vtbl+28` body on `[0x143E8F8]` stays
**UNREAD** (PARITY 0b). First-seen `Create` still
**does not yield** (`jmp 00CD17F8`;
`FirstSeenCreateDoesNotYield=true`).

---

## 4. Host equivalent / gap

| Native | Host | Class |
|---|---|---|
| token `00CCC246` / apply `00CCC3E6` | `GlobalDispatcher` `"Create"` → `ApplyCreate` | **EQUIVALENT** dispatch |
| required type,marker,name | empty any → `Continue` | **EQUIVALENT** |
| suffix arg4 / unique arg5 | `ApplyCreate` | **EQUIVALENT** |
| marker pos | `FindThing` / `RegionTravel.PositionOf` | **PARTIAL** (no `vtbl+280/288`) |
| `vtbl+364` `008A9100` | `ctx.World.Spawn` | **DIVERGE** — record only |
| `00833800` `CThingAICreature` | `ThingInstance` `{Kind="CTC", DefinitionType=type, ScriptName=name}` | **DIVERGE** |
| `004C9CA0` / world insert | `Runtime.AddThing` list | **DIVERGE** |
| empty\|IsTrue(arg3) `008ADF90` | `props["Extra"]="1"` + `Effects` | **PARTIAL** flag only |
| not IsFalse(arg6) `00CD3D2E` | `Bindings.BindCreated` | **EQUIVALENT** bind intent |
| `vtbl+2048` / `+32` / `+1896` / `+2148` | omitted | **UNREAD** vs host |
| `jmp 00CD17F8` no yield | `CommandResult.Continue` | **EQUIVALENT** |
| graphic mesh / CTC | none | **UNREAD** / **DIVERGE** |

`IScriptHost.Create` (`ScriptRuntime.cs`) is a thinner
façade: always `Spawn(..., extras:false)` and always
`BindCreated`. Interpreter leftover uses `ApplyCreate`.
**PARTIAL** vs the opcode.

`RegionTravel.CreateApplyFn = 0x008A9100` is the vtbl
target, not a host implementation. Comment “spawn body
UNREAD — record only” is the host gap this proof
narrows: construct class is now **PROVEN**; graphic
mesh id remains **UNREAD**.

Tests:
`WorldSceneTests.Create_villager_records_args_and_does_not_yield`
prove opcode / vtbl / CompleteNow / recorded args.
They do **not** prove `00831F80` ran.

---

## Classifications (short)

1. **Intro callers of `008A9100` — only `00CCC3E6`
   `call [esi+364]`. PROVEN.** `00DB86B0` /
   `00CBFB7D` reach it through that apply, not `E8`.
2. **Leftover mesh is `CThingAICreature` `VILL1` at
   `MK_OVI_ID_VS1`. PROVEN** type/name/ctor.
   **UNREAD** graphic id.
3. **Host `ApplyCreate`/`World.Spawn` is the record
   façade. DIVERGE** vs `00833800`/`004C9CA0`.
4. **Not Leave. PROVEN leftover** vs first TNG
   `0051FD80`.

Do not start New Game at `Create …,VILL1`. Do not name
`008A9100` a MeshFile loader.
