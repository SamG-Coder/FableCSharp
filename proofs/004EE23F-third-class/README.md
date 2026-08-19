# `004EE23F` third `009B0AC0` / `0044C6B0` is `CSmokeGeneratorDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent a listing parser. Read
`listing-004c0000.txt` after `004EE56C`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: After `CHighlightItemDef`
`004EE56C` `009B0AC0`, what is the **next**
`0044C6B0` / `009B0AC0` on `004EE23F`?
Confirm `CSmokeGeneratorDef` at `004EE632`.
Factory imm? CTC rows between? Host leftover
after second class?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
after `004EE56C` (`e8` dest `009B0AC0` /
`0044C6B0` in `004EE23F` range);
`listing-00980000.txt` `009B0AC0`;
`e8.tsv` dests `0044C6B0` / `009B0AC0`;
`xrefs.tsv` `CSmokeGeneratorDef`;
`src/Fable.Game/EngineLifecycle.cs`
(`AddFirstDefClass` / `SecondDefClassName`);
sibling `proofs/004EE23F-second-class`.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Next `0044C6B0` after `004EE565`? | **`004EE62B`**. | **PROVEN** |
| Next `009B0AC0` after `004EE56C`? | **`004EE632`**. | **PROVEN** |
| Next class name? | **`CSmokeGeneratorDef`**. Push at `004EE5F8`. Factory imm `[ebp-1736]=0x4DA82B`. | **PROVEN** |
| `CTCSmokeGenerator` is that next pair? | **No.** That is a CTC `004D2EF0` row (`004EE588`). No `0044C6B0` / `009B0AC0`. | **DISPROVEN** |
| CTC rows between second and third class? | **One:** `CTCSmokeGenerator` `004EE588` / factory `0x4D28EB`. | **PROVEN** |
| Host leftover after second-class Note? | **Yes.** `AddFirstDefClass` Notes `CHeroMorphDef` then `CHighlightItemDef`. Next Add Def Class is this pair. | **PROVEN** leftover |
| This site is Oakvale? | **No.** | **DISPROVEN** |

**Answer:** next same-fn Add Def Class is
**`CSmokeGeneratorDef`** at **`004EE632`**
(`0044C6B0` **`004EE62B`**). Host
first+second class arms are **MATCH**
Note-only; this third class is **LEFTOVER**.

---

## 1. Second pair (already locked)

`listing-004c0000.txt` / sibling
`004EE23F-second-class`:

```
004EE532  push "CHighlightItemDef"
004EE54F  call 0099EC30
004EE55B  mov [ebp-1704], 0x4D8671
004EE565  call 0044C6B0
004EE56A  mov ecx, eax
004EE56C  call 009B0AC0
```

Host `AddFirstDefClass` Notes this consume
after `CHeroMorphDef`. **MATCH** Note-only
for `CHighlightItemDef`.

---

## 2. Next `0044C6B0` / `009B0AC0` on `004EE23F`

No invented scan. Listing after `004EE56C`
is one CTC row, then the next def pair:

```
004EE588  push "CTCSmokeGenerator"          ; 004D2EF0 / 0x4D28EB
004EE5F8  push "CSmokeGeneratorDef"
004EE615  call 0099EC30
004EE621  mov [ebp-1736], 0x4DA82B
004EE62B  call 0044C6B0
004EE630  mov ecx, eax
004EE632  call 009B0AC0
```

`e8.tsv` dest `009B0AC0` in this fn: first
`0x004EE33E`, second `0x004EE56C`, next
**`0x004EE632`**. Dest `0044C6B0`: first
`0x004EE337`, second `0x004EE565`, next
**`0x004EE62B`**. Same four-insn shape as
the first two classes (copy name, store
factory, getter, Add Def Class).

Fourth same-fn pair (not this question) is
`CTimeAppearanceFadeDef` `004EE6CA` /
`004EE6FD` / `004EE704`. One CTC
(`CTCTimeAppearanceFade` `004EE64E`) sits
between this pair and that one.

`xrefs.tsv`: string `CSmokeGeneratorDef`
`0x01244218` first at `004EE5F9`
(`fn=0x004EE137` fold). RTTI
`.?AVCSmokeGeneratorDef@@` `0x01379CFC`.

---

## 3. CTC row is not Add Def Class

The one block between `004EE56C` and
`004EE632` is:

```
0099EBF0("CTCSmokeGenerator")
006869C0
004D2EF0(0x4D28EB, 0, name)
004D9D2F
004E40C3(esi)
```

No `0044C6B0`. No `009B0AC0`. Treating
`CTCSmokeGenerator` as the next `009B0AC0`
class is **DISPROVEN**. Contrast the four
CTC rows between first and second class
(`004EE23F-second-class`).

---

## 4. Factory `0x4DA82B` (not a parser)

`004EE621` stores `0x4DA82B` the same way
`004EE55B` stores `0x4D8671` for
`CHighlightItemDef`. `listing-004c0000.txt`:

```
004DA82B  push esi
004DA82C  push 48
004DA82E  call 00BFEA1A
004DA83C  call 0044C0C0
004DA841  mov [esi], 0x123E924
004DA84A  ret
```

Ctor size **48**, vtbl **`0123E924`**.
LoadDef payload / field walk **UNREAD**.
Do not invent `00A38E50`.

---

## 5. Host leftover after second-class MATCH

`EngineLifecycle.AddFirstDefClass` runs only
when `InitGameStages` name is
`"Init Thing Components"`:

- `Note(0044C6B0)` / `Note(009B0AC0 CHeroMorphDef)` / `Note(004E4219)`
- `Note(004EE565)` / `Note(009B0AC0 CHighlightItemDef)` / `Note(004D8671)`
- `Note(009AD6E0)` / `Note(009FC4F0)` on both
- `FirstDefClass` / `SecondDefClass` set

No third `009B0AC0`. No
`CSmokeGeneratorDef`. No `0x4DA82B`.

| If host adds… | Leftover is… |
| --- | --- |
| first+second Note-only (current) | **`004EE62B` / `004EE632` `CSmokeGeneratorDef`** |
| Note-only that third name | still `009AD6E0` / `009FC4F0` on this object (**not** MATCH) |
| live third Add Def Class | next omit is `CTimeAppearanceFadeDef` `004EE704` |

`EnsurePlayerManagerSingleton` / first
`+40` consume stay **MATCH**. Second class
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
| `004EE56C` / `004EE565` | second Add Def Class `CHighlightItemDef` | **PROVEN**; host **MATCH** Note-only |
| `004EE588` | `CTCSmokeGenerator` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EE5F8` | push `"CSmokeGeneratorDef"` | **PROVEN** |
| `004EE62B` | next `0044C6B0` | **PROVEN** |
| `004EE632` | next `009B0AC0` | **PROVEN** leftover |
| `004DA82B` / `0123E924` | factory / vtbl; size 48 | **PROVEN** site; LoadDef **UNREAD** |
| `004EE704` | fourth Add Def Class `CTimeAppearanceFadeDef` | **PROVEN** later |
| `AddFirstDefClass` | `CHeroMorphDef` + `CHighlightItemDef` | **MATCH** first two; third **LEFTOVER** |
| `00DBDE40` | Oakvale | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\rtti.txt`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\004EE23F-second-class\README.md`
- `C:\FableCSharp\proofs\004EE23F-thing-components\README.md`
- `C:\FableCSharp\proofs\004EE23F-host-adddef\README.md`
