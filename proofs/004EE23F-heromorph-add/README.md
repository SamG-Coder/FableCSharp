# `004EE23F` first `+40` consume: `009B0AC0` `"Add Def Class"` `"CHeroMorphDef"`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `0041852D` ensure →
`00418585` `004EE23F`. Do **not** invent a
def parser / `00A38E50` / `game.bin` walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: Init Thing Components `004EE23F`
first `+40` consume is `004EE337` `0044C6B0`
→ `009B0AC0` `"Add Def Class"` `"CHeroMorphDef"`.
What args does `009B0AC0` take? What size does
`009AD6E0` LoadDef request against
`[this+40]=0x80000`? First leftover vs host
Note-only `004EE23F`?

Authority: `Fable.exe` dump
`listing-004c0000.txt` (`004EE23F`–
`004EE360`, `004E4219` / `004E373B`);
`listing-00980000.txt` (`009B0AC0`,
`009AD6E0`, `009AD5F0`, `00994570`,
`009AD2E0`, `00993B20`, `0099EBF0` /
`0099EC30`);
`listing-009c0000.txt` (`009FC4F0` /
`009FC210` / `009FC150` / `009E9F40`);
`listing-00440000.txt` (`0044C6B0`);
`e8.tsv` dest `009B0AC0` site `004EE33E`;
`src/Fable.Game/EngineLifecycle.cs`
(`InitGameStages` / `EnterGame` /
`AddFirstDefClass` / `EnsurePlayerManagerSingleton`)
read only.
Siblings `proofs/0044C6C2-plus40`,
`proofs/004EE23F-thing-components`,
`proofs/004EE23F-host-adddef`,
`proofs/0044C6B6-host-ensure`.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| `009B0AC0` args? | Thiscall. `ecx` = `[0x13B879C]` from `0044C6B0`. One stack arg (`ret 4`): pointer to `{ CString name, fn* factory }`. This site: name = `"CHeroMorphDef"`, factory = `0x4E4219`. | **PROVEN** |
| LoadDef request vs `+40=0x80000`? | `009FC4F0(n)` with `n = [esp+28] + 37`. `[esp+28]` is the `00993B20` holder `+8` written by `00994570` as the `009AD5F0` wchar-span length. Concrete integer for this class is **not** the factory `104`. | formula **PROVEN**; integer **UNREAD** |
| Factory `104` is that request? | **No.** `004E4219` is `00BFEA1A(104)` then `004E373B`. Budget check runs **before** `[edi+4]`. | **DISPROVEN** as `n` |
| First leftover vs Note-only `004EE23F`? | Stage leftover starts at `004E1B5D` (before this consume). First **`+40`** leftover is this `009B0AC0` → `009AD6E0` → `009FC4F0`. Host `AddFirstDefClass` is still Notes. Live consume **LEFTOVER**. | **PROVEN** leftover |
| Oakvale / def parse? | **No.** | **DISPROVEN** |

---

## Direct answers

**Args (`009B0AC0`):**

```
ecx     = 0044C6B0() = [0x13B879C]     ; 0xE0 singleton, +40=0x80000
[esp+4] = record*
            [record+0] CString         ; 4-byte Lionhead CString
            [record+4] factory         ; this site 0x4E4219
ret 4
```

Call site (`listing-004c0000.txt`):

```
004EE303  push edi                    ; -1
004EE304  push "CHeroMorphDef"
004EE309  lea ecx, [ebp-1244]
004EE30F  call 0099EBF0               ; CString ctor ret 8
004EE314  lea eax, [ebp-1244]
004EE31A  push eax
004EE31B  lea ecx, [ebp-1692]
004EE321  call 0099EC30               ; CString copy ret 4
004EE326  lea eax, [ebp-1692]
004EE32C  push eax                    ; record*
004EE32D  mov [ebp-1688], 0x4E4219    ; record+4 factory
004EE337  call 0044C6B0               ; eax = [0x13B879C]
004EE33C  mov ecx, eax
004EE33E  call 009B0AC0
```

`009B0AC0` (`listing-00980000.txt`):

```
009B0AC0  sub esp, 68
… log "Add Def Class" (0x0129B2B0) …
009B0AFE  mov ebp, [esp+88]           ; arg0 = record*
009B0B02  mov eax, [ebp+4]            ; factory
009B0B1F  mov eax, [ebp+0]            ; CString
…
009B0BE1  call 009AD2E0               ; map lookup by name
009B0BE6  mov ecx, [eax+8]
009B0BE9  mov esi, [ecx]              ; type index
009B0BEB  push esi
009B0BEE  call 009AD6E0               ; LoadDef(index)
009B0C51  ret 4
```

**Budget request:**

```
009AD7C2  mov ebp, [esp+28]           ; holder+8
009AD7C6  add ebp, 37
009AD7C9  push ebp
009AD7CC  call 009FC4F0               ; n vs [this+40]
```

`009FC4F0` reads `[this+36]` (used) and
`[this+40]` (cap `0x80000`) and returns
`used+n <= cap`. Then `009FC150` `inc [this+44]`.
`n` is **not** a static immediate on this path.

**Host leftover:** `EnterGame` still
`Note(004EE23F)` then `AddFirstDefClass()`
(more Notes + flags). No live `009B0AC0`
body, no `009FC4F0(n)`.

---

## 1. Site: first `+40` consume after ensure

`0044C6C2-plus40` already locked: first later
`+40` reader is `009FC4F0` inside `009AD6E0`,
first later caller `009B0AC0`, first later
site `004EE337`.

`e8.tsv` dest `009B0AC0` at `004EE33E` is
the first `004EE23F` use. Earlier PE sites
(`0042F627` FRONT_END, `00433B5C`,
`0044C8DA` on `0044C72B`) are **not** this
arm. **PROVEN** first consume **on this
walk after** `0041852D`.

No `0044C6B0` / `009FC4F0` between
`0044C71F` and `00418585`.

---

## 2. `009B0AC0` ABI

`0099EC30` is a 4-byte CString copy
(`[dest]=[src]`, `inc [src+13]`, `ret 4`).
`[ebp-1688] = [ebp-1692]+4`, so the pushed
object is name + factory.

`009B0AFE` `[esp+88]` after `sub esp, 68`
+ four pushes + cleaned log trio
(`0099EBF0` `ret 8`, `009E9F40` `ret 8`)
+ `0099E4B0` (no stack args) is the one
stdcall/thiscall arg. **PROVEN.**

`ecx` is **not** game. Parent `004EE23F`
never `mov ecx, esi` into this call.
**DISPROVEN** as game thiscall.

Inner `009AD6E0` arg is the **map index**
(`[[lookup+8]]`), not the CString*.
**PROVEN.**

Skip before LoadDef:

```
009B0BB1  cmp [0x138E189], 0
009B0BB7  jne do_LoadDef
009B0BB9  cmp [0x13CA7D8], 0
009B0BBF  jne skip_LoadDef          ; 009B0C30
```

First-boot values of those BSS bytes
**UNREAD** here. Listing path includes
LoadDef. Do not treat skip as the
first-seen arm without a writer.

`009ADC90` (register) and `009B9170`
(map insert at `this+120`) run on the
same record. This proof does **not**
parse that map.

---

## 3. LoadDef request vs `+40=0x80000`

`009AD6E0` `ret 4`. First stack arg is
the index (`[esp+88]` after `sub esp, 76`
+ two pushes).

Miss path (`[table[index]+12]==0`) builds
a `00993B20` holder at `[esp+20]` (4-reg
baseline): vtbl `0129A6B0`, `+4…+20` zero.
Then:

```
009AD774  push ebp                  ; index
009AD779  push &holder
009AD77C  mov [esp+28], 0x123675C   ; holder+0 vtbl
009AD784  mov [esp+52], 0           ; holder+24
009AD78C  call 009AD5F0             ; ret 8
```

`009AD5F0` looks up `manager+148`, decodes
a wchar span into `0x138E19C` (`00A3AB00`,
cap `0x8000`), then `00994570`:

```
00994570  [this+8]  = arg1          ; span length
          [this+12] = arg0          ; wchar*
          [this+20] = arg1
          [this+24] = arg0
          ret 8
```

`009D49B0` / `009AD2E0` use `[esp+16]`
(a different CString), not holder `+8`.

At `009AD7C2` the stack is back to the
4-reg baseline, so `[esp+28] = holder+8
= that span length`. `n = length + 37`.
**PROVEN** formula.

The span is a runtime decode of
`manager+136` keyed by the type index.
It is **not** `strlen("CHeroMorphDef")`
proven from this listing, and it is
**not** `104`. Concrete `n` for this
first class **UNREAD**. Do not invent
a def parser to close it.

`009FC4F0` (`listing-009c0000.txt`):

```
009FC4F2  mov edi, [esp+12]         ; n
009FC4F9  call 009FC210             ; used+n vs [this+40]
009FC4FE  mov eax, [esi+36]
009FC501  mov edx, [esi+40]         ; 0x80000
009FC504  add eax, edi
009FC506  cmp edx, eax
009FC508  sbb al, al
009FC50A  inc al                    ; used+n <= cap
009FC50E  ret 4
```

`009FC150` only `inc [this+44]`. Neither
stores `n` into `+40`. Cap stays
`0x80000`. **PROVEN.**

Factory (after the check):

```
004E4219  push 104
004E421B  call 00BFEA1A
004E4227  jmp 004E373B              ; vtbl 01242BCC
004E377F  push 104 / pop eax / ret  ; size getter
```

`104` is the `CHeroMorphDef` heap size.
**PROVEN** as `new` / size-getter.
**DISPROVEN** as the `009FC4F0` request.

---

## 4. Host leftover vs Note-only `004EE23F`

`InitGameStages[0]` is
`("Init Thing Components", 0x004EE23F)`.

`EnterGame`:

```
EnsurePlayerManagerSingleton();   // 0044C6B6 site MATCH; +40=0x80000 noted
foreach InitGameStages:
    Note(apply, name);            // Note-only 004EE23F
    if (name == "Init Thing Components")
        AddFirstDefClass();       // more Notes
```

`AddFirstDefClass` Notes `0044C6B0` /
`009B0AC0` / `004E4219` / `009AD6E0` /
`009FC4F0` and sets
`FirstDefClassRegistered`. No live
record, no index, no `n`, no
`[this+36]` bump.

| If host is… | First leftover is… | Class |
| --- | --- | --- |
| Note-only `004EE23F` (the apply) | `004E1B5D` map seed, then eight one-shots, then `CTCHeroMorph` `004D2EF0`, **then** this `009B0AC0` | **PROVEN** (`004EE23F-thing-components`) |
| Note-only `004EE23F` + Note `009B0AC0` (current `AddFirstDefClass`) | live `009B0AC0` body / `009AD6E0` / `009FC4F0(n)` | **PROVEN** leftover; **not** MATCH |
| live `+40` + real `n` for this class | this consume **MATCH**; next leftover is the rest of the `CTC*` / `C*Def` walk | hypothetical |

First leftover **of the named stage** is
**not** the `+40` consume. First leftover
**that reads `+40`** is this site.

`00416005` `"Init Definition Manager"` is
the next **named** sibling, not this fn.

---

## 5. What this does **not** say

- `009B0AC0` parses `CHeroMorphDef` bytes
  from `game.bin`. **DISPROVEN** here.
- `n = 104` or `n = 13+37`. **UNREAD** /
  **DISPROVEN** as proven integers.
- Host live budget MATCH. **DISPROVEN**.
- `004EE23F` attaches `CTCHeroMorph` to a
  player Thing. **DISPROVEN**
  (`004EE23F-thing-components`).
- This site is Oakvale. **DISPROVEN**.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004EE23F` | named apply | **PROVEN**; host apply Note **MATCH**; body **LEFTOVER** |
| `004EE304` / `004EE33E` | `"CHeroMorphDef"` / first `009B0AC0` | **PROVEN** |
| `0044C6B0` | getter `[0x13B879C]` | **PROVEN** |
| `009B0AC0` | `"Add Def Class"`; `ret 4`; record `{CString, factory}` | **PROVEN** ABI |
| `0x4E4219` / `004E373B` | factory `new(104)` / ctor | **PROVEN** heap size; **DISPROVEN** as `n` |
| `009AD6E0` | `LoadDef(index)` | **PROVEN** |
| `009AD5F0` / `00994570` | holder `+8` = span length | **PROVEN** write |
| `009FC4F0` / `009FC210` | `n` vs `[this+40]=0x80000` | **PROVEN** |
| `n = [esp+28]+37` | request | **PROVEN** formula |
| concrete `n` | first class integer | **UNREAD** |
| `0x138E189` / `0x13CA7D8` | LoadDef skip | **UNREAD** first-boot |
| `AddFirstDefClass` | host Notes | **PROVEN** leftover vs live |
| `004E1B5D` | first stage leftover vs Note-only apply | **PROVEN** earlier than this consume |
| `00DBDE40` | Oakvale | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-009c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\0044C6C2-plus40\README.md`
- `C:\FableCSharp\proofs\004EE23F-thing-components\README.md`
- `C:\FableCSharp\proofs\004EE23F-host-adddef\README.md`
