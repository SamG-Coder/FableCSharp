# Init Game first named stage `"Init Thing Components"` `004EE23F`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI` / `hero_swap_*.tng`.
After Leave this walk is `FinalAlbion.wld`
(`0042F44D`) → `"Init Game"` `0042F491` →
`00418DCA` → vtbl+4 `004184BD`. Do **not** treat
later `0049F180` / `00449D90` / `006AC910` Thing
spawn as this site.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: Init Game first named stage
`"Init Thing Components"` `004EE23F`. Host only
Notes the name. First-seen body? First-seen
callees? Real work leftover?

Authority: Fable.exe dump
`listing-004c0000.txt` (`004EE23F`–`004F9144`;
there is **no** `listing-004e0000.txt` — `004E*`
lives in the `004c` map);
`listing-00400000.txt` `004184BD` `0041852D`–
`004185D9`;
`e8.tsv` dest `004EE23F` and first dests of
that body;
`functions.tsv` `004184BD` / folded `004EE137`;
`xrefs.tsv` `"Init Thing Components"`;
`docs/runtime/FORWARD_TREE.md` §6;
`src/Fable.Game/EngineLifecycle.cs`
(`InitGameStages` / `EnterGame` /
`EnsurePlayerManagerSingleton`) read only.
Siblings `proofs/0044C6B6-first-omit`,
`proofs/0044C6B6-host-ensure`,
`proofs/0044C6C2-plus40`,
`proofs/004CDB10-subtitled-body`,
`proofs/initgame-after-leave-order`,
`proofs/morph-first`,
`proofs/hero-inventory-first`.

---

## Verdict

**Yes: `004EE23F` is the first named apply on
`004184BD`.** Parent logs `"Init Thing
Components"` then `call 004EE23F` with **no**
`ecx=game`. Sole `.text` `E8` of `004EE23F` is
`00418585`.

Body is **not** the log trio. After `_chkstk`
`00BFEA30` (`eax=0x2FB4`) it seeds a name map
(`004E1B5D`), runs eight one-shot inits (three
are `ret` stubs), then walks hundreds of
`CTC*` / `C*Def` type records into that map
(`006869C0` / `004D2EF0` / `004D9D2F` /
`004E40C3` and `0044C6B0` / `009B0AC0`).
Tail is `0073B130` then optional `004EBACE`.
`004EE23F`…`004F9144` `ret`. No file I/O. No
Thing instance.

Host `InitGameStages[0]` is
`("Init Thing Components", 0x004EE23F)`.
`EnterGame` only `Note(apply)`. No map seed,
no `004D2EF0` table, no `009B0AC0` Add Def
Class. **LEFTOVER** Note-only.

`EnsurePlayerManagerSingleton` already
**MATCH**es the earlier unnamed
`0044C6B6` / `0044C6C2` / `0044C71F` site.
This named stage is the first leftover
**after** that ensure.

| Claim | Class |
|---|---|
| `00418585` `call 004EE23F` is first named apply on `004184BD` | **PROVEN** |
| Parent string at `0041855B` / `xrefs` `0x0122F148` | **PROVEN** |
| Other `E8` of `004EE23F` | **DISPROVEN** — only `00418585` |
| `ecx=game` thiscall | **DISPROVEN** — no `mov ecx, esi` |
| Host `InitGameStages` notes the name | **PROVEN** **MATCH** |
| Host `EnterGame` runs the body | **DISPROVEN** — **LEFTOVER** Note-only |
| Body is log-only (`0041863D` class) | **DISPROVEN** |
| First-seen work is type-register, not spawn | **PROVEN** |
| `functions.tsv` `004EE137` size 10457 is this fn | **DISPROVEN** fold / size; listing `004EE23F`…`004F9144` |
| Nested under Init Definition / Graphics | **DISPROVEN** — previous sibling of `00416005(1)` |
| Oakvale / `00DBDE40` | **DISPROVEN** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First named `004184BD` stage? | **Yes.** After unnamed `0044C6B6` ensure. Next named is `"Init Definition Manager"` `00416005(1)`. | **PROVEN** |
| Host only Notes the name? | **Yes.** `InitGameStages[0]` + `Note(apply)`. No `if (name == "Init Thing Components")` arm. | **PROVEN** leftover |
| First-seen body? | Stack `0x2FB4` → `004E1B5D` map → eight one-shots → `CTCHeroMorph` first CTC → `CHeroMorphDef` first `009B0AC0` → long `CTC*`/`C*Def` walk → `CHasNameDef` → `0073B130` → `004EBACE` → `ret`. | **PROVEN** shape; `0073B130` payload **PARTIAL** |
| First-seen callees? | Direct first `E8`s below. Eight family inits are **only** `E8` of those dests. | **PROVEN** |
| Real work leftover? | **Yes.** Entire register walk. Not log-only. Adding Note-only already **MATCH**es host. Adding the table closes this site; next named leftover is `00416005` (also Note-only; `EnsureDefs` is later / lazy). | **PROVEN** work; full name list **PARTIAL** (index strings) |

**Answer:** first leftover **on this named site**
is the body (`004E1B5D` then `004D2EF0` /
`009B0AC0` walk). Host leftover **after** the
`0044C6B6` ensure is this work. Do not start
Oakvale.

---

## 1. Site: first named child of `004184BD`

`listing-00400000.txt`:

```
0041852D  call 0044C6B6
00418532  test al, al
00418534  jne 00418557
00418536  push 0xE0
0041853B  call 00BFEA1A
00418547  call 0044C6C2
00418552  call 0044C71F
00418557  or edi, -1
0041855A  push edi
0041855B  push "Init Thing Components"
00418563  call 0099EBF0
00418578  call 009E9F40
00418580  call 0099EAE0
00418585  call 004EE23F          ; THIS SITE — no ecx=esi
0041858A  push edi
0041858B  push "Init Definition Manager"
…
004185D5  push 1
004185D7  mov ecx, esi
004185D9  call 00416005
```

`esi` is game (`004184D1` `[0x13B86A0]=esi`).
`004184BD` does **not** pass it into
`004EE23F`. Contrast later `00416C8A` /
`004168DC` / `00416005`.

`e8.tsv` dest `004EE23F`: **only** `00418585`.

`functions.tsv` `004184BD` callees:
`…009ED190,0044C6B6,00BFEA1A,0044C6C2,0044C71F,0099EBF0,…004EE23F,0099EBF0,…00416005…`.

`xrefs.tsv`: `"Init Thing Components"`
`0x0122F148` only at `0041855C` `fn=004184BD`.

---

## 2. First-seen body (`004EE23F`–`004F9144`)

Index row `0x004EE137` size 10457 folds three
functions (`004EE137` `ret 20`, `004EE219`
`ret 4`, then this). **PARTIAL** index.
Listing start:

```
004EE23F  push ebp
004EE240  mov eax, 0x2FB4
004EE245  lea ebp, [esp-120]
004EE249  call 00BFEA30            ; _chkstk
004EE24E  push esi
004EE24F  push edi
004EE250  push [PropA]
004EE256  mov esi, movePropA       ; dump label — map this
004EE25B  push [movePropA]
004EE261  mov ecx, esi
004EE263  call 004E1B5D            ; ret 8; [this+4] insert
004EE268  call 00749D60            ; ret
004EE26D  call 007330F0            ; ret
004EE272  call 007899C0            ; ["eyExA"]=0xBF800000
004EE277  call 007FFFA0            ; [0x13BAF18]=0xBF800000
004EE27C  call 006CBA00            ; ret
004EE281  call 0075C0E0            ; [0x13BAD7C]=0
004EE286  call 008162F0            ; four 00818000 slots
004EE28B  call 00802370            ; ["pbrk"]=1; [0x13BAF21]=0
004EE290  or edi, -1
004EE294  push "CTCHeroMorph"      ; first CTC
…
004EE2A5  call 006869C0            ; xor eax,eax; ret
004EE2B6  call 004D2EF0            ; {factory 0x4D28BB, name}
004EE2C6  call 004D9D2F
004EE2CE  call 004E40C3            ; ecx=esi map
…
004EE304  push "CHeroMorphDef"     ; first 009B0AC0
004EE337  call 0044C6B0            ; [0x13B879C]
004EE33E  call 009B0AC0
```

Epilogue:

```
004F8E61  push "CHasNameDef"
…
004F9129  call 0073B130            ; only E8 of dest
004F912E  cmp [flag], 0
004F9135  je 004F913E
004F9137  mov ecx, esi
004F9139  call 004EBACE
004F913E  pop edi
004F913F  pop esi
004F9140  add ebp, 120
004F9143  leave
004F9144  ret
```

`004EE23F`…`004F9144` is `0xAF06` bytes.
No `ecx=game`. `esi` stays the map object
for every `004E40C3` / tail `004EBACE`.
Dump labels `PropA` / `movePropA` /
`etWindowLongA` / `eyExA` / `pbrk` /
`rchr` are IAT-adjacent names, not type
strings. Exact `.data` VAs behind those
labels **PARTIAL**.

---

## 3. First-seen callees (`e8.tsv` in-range)

Order of first `E8` dests (repeat helpers
omitted after first):

| # | Site | Dest | Role | Keep? | First `E8`? |
|--:|---|---|---|---|---|
| 1 | `004EE249` | `00BFEA30` | stack probe | plumbing | this walk **yes** (PE also `00403485`) |
| 2 | `004EE263` | `004E1B5D` | map insert `ret 8` | **work** | this walk **yes**. Other site `004E347C` is `push [ecx+4]/[ecx]` wrapper, **0** `E8` of `004E3477` |
| 3 | `004EE268` | `00749D60` | `ret` | stub | **only** |
| 4 | `004EE26D` | `007330F0` | `ret` | stub | **only** |
| 5 | `004EE272` | `007899C0` | store `0xBF800000` | **work** | **only** |
| 6 | `004EE277` | `007FFFA0` | `[0x13BAF18]=0xBF800000` | **work** | **only** |
| 7 | `004EE27C` | `006CBA00` | `ret` | stub | **only** (`creature-move-first`) |
| 8 | `004EE281` | `0075C0E0` | `[0x13BAD7C]=0` | **work** | **only** |
| 9 | `004EE286` | `008162F0` | four `00818000` | **work** | **only** |
| 10 | `004EE28B` | `00802370` | `["pbrk"]=1`; `[0x13BAF21]=0` | **work** | **only** |
| 11 | `004EE29C` | `0099EBF0` | `"CTCHeroMorph"` | name | earlier on `004184BD` log |
| 12 | `004EE2A5` | `006869C0` | `xor eax,eax; ret` | stub | **first in PE** |
| 13 | `004EE2B6` | `004D2EF0` | factory+name record | **work** | **first in PE** |
| 14 | `004EE2C6` | `004D9D2F` | table pack | **work** | this walk **yes** |
| 15 | `004EE2CE` | `004E40C3` | insert into `esi` map | **work** | this walk **yes** (PE also `004E6DA4`) |
| 16 | `004EE321` | `0099EC30` | CString copy | plumbing | many earlier PE sites |
| 17 | `004EE337` | `0044C6B0` | `[0x13B879C]` | getter | first **use** after ensure |
| 18 | `004EE33E` | `009B0AC0` | Add Def Class | **work** | this walk **yes** (frontend `0042F627` is **not** this arm) |
| later | `0042DAE0` | def-name pack | **work** | first use on this walk at `CClockDef` / `CHeroDef` family |
| last | `004F9129` | `0073B130` | post-table fill | **work** | **only** |
| last | `004F9139` | `004EBACE` | map commit if flag | **work** | this walk **yes** |

CTC row pattern (first-seen `CTCHeroMorph`
`004EE294`):

```
0099EBF0(name)
006869C0            ; always eax=0
004D2EF0(factory, 0, name)
004D9D2F
004E40C3(esi)
0099EAE0 ×4
```

`C*Def` row (`CHeroMorphDef` `004EE304`):

```
0099EBF0(name)
0099EC30
0044C6B0
009B0AC0
```

`004D2EF0` (`listing-004c0000.txt`):
`[this]=arg0`, `[this+4]=arg1`,
`0099EC30` name at `+8`, `ret 12`.
First factory imm is `0x4D28BB`
(`CTCHeroMorph`).

`006869C0` is **not** a live type-id
hasher. **DISPROVEN** vs older
“type id” wording (`hero-inventory-first`).

`0073B130` size 22666 in `functions.tsv`
(`00743270` loop). First-seen here.
Inner table **UNREAD**.

---

## 4. First names (not instances)

`functions.tsv` string island on the folded
row (artifact `etWindowLongA` dropped):

1. `CTCHeroMorph` / `CHeroMorphDef`
2. `CTCSimpleAppearanceMorph`
3. `CTCAtmosPlayer`
4. `CTCRandomAppearanceMorph`
5. `CTCHighlightItem` / `CHighlightItemDef`
6. `CTCSmokeGenerator` / `CSmokeGeneratorDef`
…
- inventory: `CTCInventory` `004EECFE` …
- `CHeroDef` `004F08F0`
- last named: `CHasNameDef` `004F8E61`

Siblings already locked first *name* use
for morph / inventory / bones. This proof
locks the **stage**: those names are
children of `004EE23F`, not of
`00416005` / `0049F180`.

No `game.bin`. No `PLAYER_HERO` Thing.
**DISPROVEN** as create.

---

## 5. Host leftover

`InitGameStages` first row:

```
("Init Thing Components", 0x004EE23F),
("Init Definition Manager", 0x00416005),
```

`EnterGame`:

```
EnsurePlayerManagerSingleton();   // 0044C6B6 site MATCH
foreach InitGameStages:
    Note(apply, name, "InitGame", name);
    // no arm for "Init Thing Components"
```

| If host adds… | Leftover is… |
|---|---|
| Note-only (current) | whole body — `004E1B5D` + table + `0073B130` |
| Note + name list only | still `004D2EF0` / `009B0AC0` inserts (**not** MATCH) |
| live map + CTC/`C*Def` records | this site **MATCH**; next named leftover `00416005` |

`EnsureDefs` / `game.bin` is **not** this
fn. It is the next named stage’s analog
and is **not** invoked at
`"Init Thing Components"`.

Walk-first unnamed hole `0044C6B6` is
already hosted (`0044C6B6-host-ensure`).
**DISPROVEN** as still the first leftover
after `EnterGame` ensure.

---

## 6. What this does **not** say

- `004EE23F` attaches `CTCHeroMorph` to a
  player Thing. **DISPROVEN**.
- `00416005(1)` is nested under this stage.
  **DISPROVEN** — next sibling.
- `FORWARD_TREE` §6 row is the apply VA.
  **MATCH** (name only).
- New Game is `00DBDE40`. **DISPROVEN**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004184BD` | vtbl+4 parent | **PROVEN** |
| `0041855B` / `00418585` | log + apply | **PROVEN** |
| `0044C6B6` / `0044C6C2` / `0044C71F` | previous unnamed | **PROVEN**; host ensure **MATCH** |
| `004EE23F` | this fn | **PROVEN** on walk; host **LEFTOVER** |
| `004EE137` / `004EE219` | prior helpers | **DISPROVEN** as this apply |
| `00BFEA30` | `_chkstk` | **PROVEN** plumbing |
| `004E1B5D` | map seed | **PROVEN** first work |
| `00749D60` / `007330F0` / `006CBA00` | `ret` | **PROVEN** stubs |
| `007899C0` / `007FFFA0` / `0075C0E0` / `00802370` | scalar inits | **PROVEN** |
| `008162F0` | four-slot register | **PROVEN** call; slots **PARTIAL** |
| `006869C0` | `xor eax,eax; ret` | **PROVEN** stub |
| `004D2EF0` / `004D9D2F` / `004E40C3` | CTC record + insert | **PROVEN** |
| `0044C6B0` / `009B0AC0` | def class | **PROVEN** first consume after ensure |
| `004EE294` `CTCHeroMorph` | first CTC name | **PROVEN** |
| `004EE304` `CHeroMorphDef` | first `009B0AC0` name | **PROVEN** |
| `004F08F0` `CHeroDef` | later def name | **PROVEN** name; factory **UNREAD** here |
| `0073B130` | tail fill | **PROVEN** only-`E8`; body **UNREAD** |
| `004EBACE` | flag-gated commit | **PROVEN** call |
| `00416005` | next named leftover | **PROVEN** sibling |
| `00DBDE40` | later quest | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00680000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-006c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00700000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00740000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00780000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-007c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00800000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00bc0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\docs\runtime\FORWARD_TREE.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
