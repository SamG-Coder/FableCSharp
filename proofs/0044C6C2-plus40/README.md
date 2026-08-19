# `0044C6C2` vtbl `01232C24` / `[this+40]=0x80000`: who reads `+40`?

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `0041852D`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `0044C6C2` sets vtbl `01232C24` and
`[this+40]=0x80000` via `009FC520`. Who later
reads `+40`? Type name of `01232C24`? Host
leftover after ensure?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`004184BD` `0041852D`–`00418585`, `00416005`,
`0041732A`, `0042F5E8`);
`listing-00440000.txt` (`0044C6B0`–`0044C72A`,
`0044A3B0`);
`listing-004c0000.txt` (`004EE337`);
`listing-00980000.txt` (`0099B6B0`, `009B0470`,
`009B0AC0`, `009AD6E0`, `009ACB10`);
`listing-009c0000.txt` (`009FC150` / `009FC170` /
`009FC210` / `009FC4F0` / `009FC520` / `009FC570`);
`listing-00cc0000.txt` (`00CD3F00`);
`e8.tsv` dests `009FC210` / `009FC4F0` / `009FC520`
/ `009B0470` / `009B0AC0` / `0044C6B0`;
`functions.tsv` `0044C6C2`;
`out/00-index/rtti.txt` / `strings.tsv` /
`xrefs.tsv`;
`src/Fable.Game/EngineLifecycle.cs`
(`EnsurePlayerManagerSingleton` / `InitGameStages`);
siblings `proofs/0044C6B6-first-omit`,
`proofs/0044C6B6-host-ensure`,
`proofs/morph-first`.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Who later reads `+40`? | **`009FC4F0`** and **`009FC210`**. First later site: `"Init Thing Components"` `004EE337` `0044C6B0` → **`009B0AC0`** `"Add Def Class"` → **`009AD6E0`** `"CDefinitionManager::LoadDef"` → `009FC4F0` / `009FC210`. | **PROVEN** |
| Same-call ctor read? | `009FC520` stores `0x80000` then `jmp 009FC210` with request `0`. Not “later”. | **PROVEN** |
| Type name of `01232C24`? | Exact RTTI **UNREAD** (no COL, `01232C30` not in ASCII `strings.tsv`). **PARTIAL**: `CDefinitionManager`-derived. | **UNREAD** name / **PARTIAL** family |
| Host leftover after ensure? | Ensure **MATCH** at `0041852D`. Leftover is **`009B0470` / live `+40` / `009B0AC0`+`009AD6E0`+`009FC4F0`**, then `00416005` `[vtbl+8]`. Not Note-only MATCH for those consumers. | **PROVEN** leftover |
| This site is Oakvale? | **No.** | **DISPROVEN** |

---

## Direct answers

`009FC520` (`listing-009c0000.txt`):

```
009FC520  mov eax, [esp+4]
009FC524  mov [ecx+40], eax
009FC527  mov [esp+4], 0x0
009FC52F  jmp 009FC210
```

`009FC210` compares `[this+36]+request` to **`[this+40]`**
(cap). `009FC4F0` calls that, then reads **`[esi+40]`**
again and returns `used+n <= cap`.

`009FC570` (first insn of `009B0470`) writes
`[this+40]=0x7FFFFFFF`. `0044C6C2` then caps it to
`0x80000` (512 KiB).

---

## 1. Ctor (already locked)

`0044C6C2` (`listing-00440000.txt`):

```
0044C6C9  push 0x1232C30
0044C6D1  call 0099B6B0
0044C6D6  push 0x44C6AF
0044C6E1  call 009B0470          ; vtbl 0129B28C, +40=MAX
0044C6F0  mov [esi], 0x1232C24
0044C6F6  mov [esi+208..220], 0
0044C708  push 0x80000
0044C715  call 009FC520          ; [this+40]=0x80000
```

`0044C71F` / `00450142` stores the `0xE0` object at
`[0x13B879C]`. Only `.text` `E8` of `0044C6C2` is
`00418547`. **PROVEN.**

---

## 2. Who later reads `+40`

`.text` `E8` of dest `009FC210`: `009FC4F9`,
`00A23FAF`, `00A24E8C`, `00A26BA6`. The `00A23xxx`
sites add `0x26C` onto a **different** object.

`.text` `E8` of dest `009FC4F0` that uses **this**
singleton: `009AD7CC` inside `009AD6E0`.

`009AD6E0` (`listing-00980000.txt`):

```
009AD6E9  mov ebx, ecx           ; this = singleton
…
009AD742  push "CDefinitionManager::LoadDef 1"
…
009AD7C6  add ebp, 37
009AD7C9  push ebp
009AD7CA  mov ecx, ebx
009AD7CC  call 009FC4F0          ; READ +40
009AD7D3  call 009FC150          ; inc [this+44]  (not +40)
```

`xrefs.tsv`: `CDefinitionManager::LoadDef 1/2/3`
are all in `fn=0x009AD6E0`.

Caller of `009AD6E0` on this object: `009B0AC0`
(`mov edi, ecx` then `call 009AD6E0`). Log
`"Add Def Class"` (`0x0129B2B0`).

First later site after the ensure (`listing-004c0000.txt`):

```
004EE304  push "CHeroMorphDef"
004EE337  call 0044C6B0
004EE33C  mov ecx, eax
004EE33E  call 009B0AC0
```

That is the first child of `"Init Thing Components"`
`004EE23F` that touches the singleton. **PROVEN.**
No `0044C6B0` / `009FC4F0` between `0044C71F` and
`00418585`.

`009ACB10` (`0041602E`, `"Init Definition Manager"`)
is `mov ecx, [ecx+88]; jmp 009E5250`. **DISPROVEN**
as a `+40` reader.

---

## 3. Type name of `01232C24`

| Evidence | Class |
| --- | --- |
| `0044C6C2` writes vtbl `01232C24` | **PROVEN** |
| `009B0470` first writes `0129B28C` (string island: `CDefinitionManager::LoadDef*`, `Definition Manager: Load Binary Def`, `Add Def Class`) | **PROVEN** neighborhood |
| Methods on this object include `CDefinitionManager::LoadDef` / `Add Def Class` | **PROVEN** |
| `00416005` `"Init Definition Manager"` uses `0044C6B0` then `[vtbl+8]` / `009ACB10` | **PROVEN** use |
| ASCII / RTTI link `01232C24` → `.?AV…` | **UNREAD** (`.rdata` vtbl not in `listing-01200000`; `01232C30` absent from `strings.tsv`) |
| `rtti.txt` `CPlayerManager` `0x01376174` | **UNREAD** as this vtbl (no COL) |
| Host `PlayerManagerVtbl = 0x01232C24` | name, not a listing RTTI hit |
| `0044A3B0` (`"Init Player Manager"`, 44 bytes, `game+28`) vtbl **`01231CD0`** sits between `PlayerCharacterUID` and `hero_swap_4.tng` | **DISPROVEN** as `01232C24` |

Sibling `009B0470` derived ctors (same base, other
vtbls): `00433693` `0123117C` size `0xD0` (frontend
`0042F5E8` / `FRONT_END`); `00CD3F00` `012C2648`
size `0xD0` (`Registering Script Defs`). This
object is the `0xE0` one with extra `+208..+220`
and cap `0x80000`. Derived name
(`CGameDefinitionManager` / `CPlayerManager` /
other) stays **UNREAD**.

`push 0x1232C30` is the `0099B6B0` source copied
to `[this+184]`. Treat as instance name, not
proven type name.

---

## 4. Host leftover after ensure

`EnsurePlayerManagerSingleton` (`EnterGame`,
before `InitGameStages`):

- `Note(0044C6B6)` / miss → `Note(0044C6C2 … +40=0x80000)` /
  `Note(0044C71F)` / `PlayerManagerPresent=true`.
- No `0xE0` object, no `009B0470`, no live `+40`.

`0044C6B6-host-ensure`: that **site** is **MATCH**.

After the ensure, native still:

1. `004EE23F` `009B0AC0` / `009AD6E0` / `009FC4F0`
   (**first `+40` consumer**). Host only
   `Note(004EE23F, "Init Thing Components")`.
2. `00416005` `[vtbl+8]` + `009ACB10`. Host
   `EnsureDefs` / `game.bin` is a later analog,
   not this cap.

| If host adds… | Leftover is… |
| --- | --- |
| Note-only `009B0AC0` | still `009AD6E0` / `009FC4F0` (**not** MATCH) |
| live `+40=0x80000` + LoadDef budget | this consume **MATCH**; next omit is `00416005` work |

---

## 5. Not Oakvale

No `00DBDE40` / region / TNG / hero create on
`0044C6C2` or `009FC4F0`. Parent is `004184BD`.
**DISPROVEN.**

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0044C6C2` | ctor; vtbl `01232C24`; `009FC520(0x80000)` | **PROVEN** |
| `009FC520` | `[this+40]=arg`; `jmp 009FC210` | **PROVEN** |
| `009FC570` | base `[this+40]=MAX` | **PROVEN** |
| `009FC210` | reads `+40` cap | **PROVEN** |
| `009FC4F0` | later `+40` reader | **PROVEN** |
| `009AD6E0` | `CDefinitionManager::LoadDef` | **PROVEN** |
| `009B0AC0` | `Add Def Class`; first later caller | **PROVEN** |
| `004EE337` | first later site | **PROVEN** |
| `01232C24` | vtbl | **PROVEN**; type name **UNREAD**; family **PARTIAL** |
| `0129B28C` | `009B0470` vtbl / `CDefinitionManager` island | **PARTIAL** |
| `01231CD0` | 44-byte `"Init Player Manager"` | **DISPROVEN** as `01232C24` |
| `01376174` | RTTI `CPlayerManager` | **UNREAD** vs this vtbl |
| `EnsurePlayerManagerSingleton` | site **MATCH**; `+40` consume **LEFTOVER** | **PROVEN** |
| `00DBDE40` | Oakvale | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-009c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\rtti.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\0044C6B6-first-omit\README.md`
- `C:\FableCSharp\proofs\0044C6B6-host-ensure\README.md`
