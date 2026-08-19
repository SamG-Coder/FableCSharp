# `004EE23F` fourth `009B0AC0` / `0044C6B0` is `CTimeAppearanceFadeDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent a listing parser. Read
`listing-004c0000.txt` after `004EE632`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: After `CSmokeGeneratorDef`
`004EE632` `009B0AC0`, what is the **next**
`0044C6B0` / `009B0AC0` on `004EE23F`?
Confirm `CTimeAppearanceFadeDef` at `004EE704`.
Factory imm? CTC rows between?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
after `004EE632` (`e8` dest `009B0AC0` /
`0044C6B0` in `004EE23F` range);
`listing-00980000.txt` `009B0AC0`;
`e8.tsv` dests `0044C6B0` / `009B0AC0`;
`xrefs.tsv` `CTimeAppearanceFadeDef`;
`src/Fable.Game/EngineLifecycle.cs`
(`AddFirstDefClass` / `ThirdDefClassName`);
sibling `proofs/004EE23F-third-class`.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Next `0044C6B0` after `004EE62B`? | **`004EE6FD`**. | **PROVEN** |
| Next `009B0AC0` after `004EE632`? | **`004EE704`**. | **PROVEN** |
| Next class name? | **`CTimeAppearanceFadeDef`**. Push at `004EE6CA`. Factory imm `[ebp-1720]=0x4D84C8`. | **PROVEN** |
| `CTCTimeAppearanceFade` is that next pair? | **No.** That is a CTC `004D2EF0` row (`004EE64E`). No `0044C6B0` / `009B0AC0`. | **DISPROVEN** |
| CTC rows between third and fourth class? | **One:** `CTCTimeAppearanceFade` `004EE64E` / factory `0x4D4114`. | **PROVEN** |
| Host leftover after third class Note? | **Yes.** `AddFirstDefClass` Notes `CHeroMorphDef` then `CHighlightItemDef` then `CSmokeGeneratorDef`. Next Add Def Class is this pair. | **PROVEN** leftover |
| This site is Oakvale? | **No.** | **DISPROVEN** |

**Answer:** next same-fn Add Def Class is
**`CTimeAppearanceFadeDef`** at **`004EE704`**
(`0044C6B0` **`004EE6FD`**). Host
first+second+third class arms are **MATCH**
Note-only; this fourth class is **LEFTOVER**.

---

## 1. Third pair (already locked)

`listing-004c0000.txt` / sibling
`004EE23F-third-class`:

```
004EE5F8  push "CSmokeGeneratorDef"
004EE615  call 0099EC30
004EE621  mov [ebp-1736], 0x4DA82B
004EE62B  call 0044C6B0
004EE630  mov ecx, eax
004EE632  call 009B0AC0
```

Host `AddFirstDefClass` Notes this consume
after `CHighlightItemDef`. **MATCH** Note-only
for `CSmokeGeneratorDef`.

---

## 2. Next `0044C6B0` / `009B0AC0` on `004EE23F`

No invented scan. Listing after `004EE632`
is one CTC row, then the next def pair:

```
004EE64E  push "CTCTimeAppearanceFade"       ; 004D2EF0 / 0x4D4114
004EE6CA  push "CTimeAppearanceFadeDef"
004EE6E7  call 0099EC30
004EE6F3  mov [ebp-1720], 0x4D84C8
004EE6FD  call 0044C6B0
004EE702  mov ecx, eax
004EE704  call 009B0AC0
```

`e8.tsv` dest `009B0AC0` in this fn: first
`0x004EE33E`, second `0x004EE56C`, third
`0x004EE632`, next **`0x004EE704`**. Dest
`0044C6B0`: first `0x004EE337`, second
`0x004EE565`, third `0x004EE62B`, next
**`0x004EE6FD`**. Same four-insn shape as
the first three classes (copy name, store
factory, getter, Add Def Class).

Fifth same-fn pair (not this question) is
`CCreatureNavigationDef` `004EE8F8` /
`004EE92B` / `004EE932`. Four CTC rows
(`CTCPhysicsLight` `004EE720`,
`CTCPhysicsStandard` `004EE790`,
`CTCPhysicsControlled` `004EE80C`,
`CTCCreatureNavigation` `004EE87C`) sit
between this pair and that one.

`xrefs.tsv`: string `CTimeAppearanceFadeDef`
`0x01244200` first at `004EE6CB`
(`fn=0x004EE137` fold). RTTI
`.?AVCTimeAppearanceFadeDef@@` `0x01379258`.

---

## 3. CTC row is not Add Def Class

The one block between `004EE632` and
`004EE704` is:

```
0099EBF0("CTCTimeAppearanceFade")
006869C0
004D2EF0(0x4D4114, 0, name)
004D9D2F
004E40C3(esi)
```

No `0044C6B0`. No `009B0AC0`. Treating
`CTCTimeAppearanceFade` as the next `009B0AC0`
class is **DISPROVEN**. Contrast the four
CTC rows between first and second class
(`004EE23F-second-class`) and the one CTC
between second and third (`004EE23F-third-class`).

---

## 4. Factory `0x4D84C8` (not a parser)

`004EE6F3` stores `0x4D84C8` the same way
`004EE621` stores `0x4DA82B` for
`CSmokeGeneratorDef`. `listing-004c0000.txt`:

```
004D84C8  push esi
004D84C9  push 56
004D84CB  call 00BFEA1A
004D84D9  call 0044C0C0
004D84DE  mov [esi], 0x123B7CC
004D84E7  ret
```

Ctor size **56**, vtbl **`0123B7CC`**.
LoadDef payload / field walk **UNREAD**.
Do not invent `00A38E50`.

---

## 5. Host leftover after third-class MATCH

`EngineLifecycle.AddFirstDefClass` runs only
when `InitGameStages` name is
`"Init Thing Components"`:

- `Note(0044C6B0)` / `Note(009B0AC0 CHeroMorphDef)` / `Note(004E4219)`
- `Note(004EE565)` / `Note(009B0AC0 CHighlightItemDef)` / `Note(004D8671)`
- `Note(004EE62B)` / `Note(009B0AC0 CSmokeGeneratorDef)` / `Note(004DA82B)`
- `Note(009AD6E0)` / `Note(009FC4F0)` on each
- `FirstDefClass` / `SecondDefClass` / `ThirdDefClass` set

No fourth `009B0AC0`. No
`CTimeAppearanceFadeDef`. No `0x4D84C8`.

| If host adds… | Leftover is… |
| --- | --- |
| first+second+third Note-only (current) | **`004EE6FD` / `004EE704` `CTimeAppearanceFadeDef`** |
| Note-only that fourth name | still `009AD6E0` / `009FC4F0` on this object (**not** MATCH) |
| live fourth Add Def Class | next omit is `CCreatureNavigationDef` `004EE932` |

`EnsurePlayerManagerSingleton` / first
`+40` consume stay **MATCH**. Third class
is **MATCH** Note-only. This is the first
leftover **Add Def Class** after that
host arm. Whole remaining `004EE23F` walk
is still leftover (`004EE23F-thing-components`).

---

## 6. Not Oakvale

No `00DBDE40` / region / TNG / hero create
on this pair. Parent is `004EE23F`.
**DISPROVEN.**

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004EE632` / `004EE62B` | third Add Def Class `CSmokeGeneratorDef` | **PROVEN**; host **MATCH** Note-only |
| `004EE64E` | `CTCTimeAppearanceFade` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EE6CA` | push `"CTimeAppearanceFadeDef"` | **PROVEN** |
| `004EE6FD` | next `0044C6B0` | **PROVEN** |
| `004EE704` | next `009B0AC0` | **PROVEN** leftover |
| `004D84C8` / `0123B7CC` | factory / vtbl; size 56 | **PROVEN** site; LoadDef **UNREAD** |
| `004EE932` | fifth Add Def Class `CCreatureNavigationDef` | **PROVEN** later |
| `AddFirstDefClass` | `CHeroMorphDef` + `CHighlightItemDef` + `CSmokeGeneratorDef` | **MATCH** first three; fourth **LEFTOVER** |
| `00DBDE40` | Oakvale | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\rtti.txt`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\004EE23F-third-class\README.md`
- `C:\FableCSharp\proofs\004EE23F-thing-components\README.md`
- `C:\FableCSharp\proofs\004EE23F-host-adddef\README.md`
