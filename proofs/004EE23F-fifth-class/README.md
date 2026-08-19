# `004EE23F` fifth `009B0AC0` / `0044C6B0` is `CCreatureNavigationDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent a listing parser. Read
`listing-004c0000.txt` after `004EE704`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: After `CTimeAppearanceFadeDef`
`004EE704` `009B0AC0`, what is the **next**
`0044C6B0` / `009B0AC0` on `004EE23F`?
Confirm `CCreatureNavigationDef` at `004EE932`.
Factory imm? CTC rows between
(`CTCPhysicsLight` / `Standard` / `Controlled` /
`CTCCreatureNavigation`)?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
after `004EE704` (`e8` dest `009B0AC0` /
`0044C6B0` in `004EE23F` range);
`listing-00980000.txt` `009B0AC0`;
`e8.tsv` dests `0044C6B0` / `009B0AC0`;
`xrefs.tsv` `CCreatureNavigationDef`;
`src/Fable.Game/EngineLifecycle.cs`
(`AddFirstDefClass` / `FourthDefClassName`);
sibling `proofs/004EE23F-fourth-class`.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Next `0044C6B0` after `004EE6FD`? | **`004EE92B`**. | **PROVEN** |
| Next `009B0AC0` after `004EE704`? | **`004EE932`**. | **PROVEN** |
| Next class name? | **`CCreatureNavigationDef`**. Push at `004EE8F8`. Factory imm `[ebp-1696]=0x4DA871`. | **PROVEN** |
| `CTCPhysicsLight` / `Standard` / `Controlled` / `CTCCreatureNavigation` is that next pair? | **No.** Those are CTC `004D2EF0` rows. No `0044C6B0` / `009B0AC0`. | **DISPROVEN** |
| CTC rows between fourth and fifth class? | **Four:** `CTCPhysicsLight` `004EE720` / `0x4D294B`; `CTCPhysicsStandard` `004EE790` / `0x4D297B`; `CTCPhysicsControlled` `004EE80C` / `0x4D29AE`; `CTCCreatureNavigation` `004EE87C` / `0x4D291B`. | **PROVEN** |
| Factory ctor? | `004DA871`: `00BFEA1A(56)` then `0044C0C0`; vtbl **`0123E98C`**. | **PROVEN** |
| Host leftover after fourth class Note? | **Yes.** `AddFirstDefClass` Notes `CHeroMorphDef` then `CHighlightItemDef` then `CSmokeGeneratorDef` then `CTimeAppearanceFadeDef`. Next Add Def Class is this pair. | **PROVEN** leftover |
| This site is Oakvale? | **No.** | **DISPROVEN** |

**Answer:** next same-fn Add Def Class is
**`CCreatureNavigationDef`** at **`004EE932`**
(`0044C6B0` **`004EE92B`**). Host
first+second+third+fourth class arms are
**MATCH** Note-only; this fifth class is
**LEFTOVER**.

---

## 1. Fourth pair (already locked)

`listing-004c0000.txt` / sibling
`004EE23F-fourth-class`:

```
004EE6CA  push "CTimeAppearanceFadeDef"
004EE6E7  call 0099EC30
004EE6F3  mov [ebp-1720], 0x4D84C8
004EE6FD  call 0044C6B0
004EE702  mov ecx, eax
004EE704  call 009B0AC0
```

Host `AddFirstDefClass` Notes this consume
after `CSmokeGeneratorDef`. **MATCH** Note-only
for `CTimeAppearanceFadeDef`.

---

## 2. Next `0044C6B0` / `009B0AC0` on `004EE23F`

No invented scan. Listing after `004EE704`
is four CTC rows, then the next def pair:

```
004EE720  push "CTCPhysicsLight"            ; 004D2EF0 / 0x4D294B
004EE790  push "CTCPhysicsStandard"         ; 004D2EF0 / 0x4D297B
004EE80C  push "CTCPhysicsControlled"       ; 004D2EF0 / 0x4D29AE
004EE87C  push "CTCCreatureNavigation"      ; 004D2EF0 / 0x4D291B
004EE8F8  push "CCreatureNavigationDef"
004EE915  call 0099EC30
004EE921  mov [ebp-1696], 0x4DA871
004EE92B  call 0044C6B0
004EE930  mov ecx, eax
004EE932  call 009B0AC0
```

`e8.tsv` dest `009B0AC0` in this fn: first
`0x004EE33E`, second `0x004EE56C`, third
`0x004EE632`, fourth `0x004EE704`, next
**`0x004EE932`**. Dest `0044C6B0`: first
`0x004EE337`, second `0x004EE565`, third
`0x004EE62B`, fourth `0x004EE6FD`, next
**`0x004EE92B`**. Same four-insn shape as
the first four classes (copy name, store
factory, getter, Add Def Class).

Sixth same-fn pair (not this question) is
`CInventoryItemDef` `004EF20A` / `004EF23D` /
`004EF244`. Many CTC rows sit between this
pair and that one (`CTCPhysicsNavigator`
`004EE94E` first).

`xrefs.tsv`: string `CCreatureNavigationDef`
`0x012441E8` first at `004EE8F9`
(`fn=0x004EE137` fold). RTTI
`.?AVCCreatureNavigationDef@@` `0x01379D20`.

---

## 3. CTC rows are not Add Def Class

The four blocks between `004EE704` and
`004EE932` are the same `004D2EF0` shape:

```
0099EBF0("CTCPhysicsLight")
006869C0
004D2EF0(0x4D294B, 0, name)
004D9D2F
004E40C3(esi)
```

Then `CTCPhysicsStandard` / `0x4D297B`,
`CTCPhysicsControlled` / `0x4D29AE`,
`CTCCreatureNavigation` / `0x4D291B`.

No `0044C6B0`. No `009B0AC0`. Treating
any of those four as the next `009B0AC0`
class is **DISPROVEN**. Contrast the one
CTC between third and fourth class
(`004EE23F-fourth-class`) and the four
CTC rows between first and second
(`004EE23F-second-class`).

---

## 4. Factory `0x4DA871` (not a parser)

`004EE921` stores `0x4DA871` the same way
`004EE6F3` stores `0x4D84C8` for
`CTimeAppearanceFadeDef`. `listing-004c0000.txt`:

```
004DA871  push esi
004DA872  push 56
004DA874  call 00BFEA1A
004DA879  mov esi, eax
004DA87B  test esi, esi
004DA87D  pop ecx
004DA87E  je 004DA891
004DA880  mov ecx, esi
004DA882  call 0044C0C0
004DA887  mov [esi], 0x123E98C
004DA88D  mov eax, esi
004DA88F  pop esi
004DA890  ret
```

Ctor size **56**, vtbl **`0123E98C`**.
LoadDef payload / field walk **UNREAD**.
Do not invent `00A38E50`.

---

## 5. Host leftover after fourth-class MATCH

`EngineLifecycle.AddFirstDefClass` runs only
when `InitGameStages` name is
`"Init Thing Components"`:

- `Note(0044C6B0)` / `Note(009B0AC0 CHeroMorphDef)` / `Note(004E4219)`
- `Note(004EE565)` / `Note(009B0AC0 CHighlightItemDef)` / `Note(004D8671)`
- `Note(004EE62B)` / `Note(009B0AC0 CSmokeGeneratorDef)` / `Note(004DA82B)`
- `Note(004EE6FD)` / `Note(009B0AC0 CTimeAppearanceFadeDef)` / `Note(004D84C8)`
- `Note(009AD6E0)` / `Note(009FC4F0)` on each
- `FirstDefClass` / `SecondDefClass` / `ThirdDefClass` / `FourthDefClass` set

No fifth `009B0AC0`. No
`CCreatureNavigationDef`. No `0x4DA871`.

| If host adds… | Leftover is… |
| --- | --- |
| first+second+third+fourth Note-only (current) | **`004EE92B` / `004EE932` `CCreatureNavigationDef`** |
| Note-only that fifth name | still `009AD6E0` / `009FC4F0` on this object (**not** MATCH) |
| live fifth Add Def Class | next omit is `CInventoryItemDef` `004EF244` |

`EnsurePlayerManagerSingleton` / first
`+40` consume stay **MATCH**. Fourth class
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
| `004EE704` / `004EE6FD` | fourth Add Def Class `CTimeAppearanceFadeDef` | **PROVEN**; host **MATCH** Note-only |
| `004EE720` | `CTCPhysicsLight` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EE790` | `CTCPhysicsStandard` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EE80C` | `CTCPhysicsControlled` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EE87C` | `CTCCreatureNavigation` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EE8F8` | push `"CCreatureNavigationDef"` | **PROVEN** |
| `004EE92B` | next `0044C6B0` | **PROVEN** |
| `004EE932` | next `009B0AC0` | **PROVEN** leftover |
| `004DA871` / `0123E98C` | factory / vtbl; size 56 | **PROVEN** site; LoadDef **UNREAD** |
| `004EF244` | sixth Add Def Class `CInventoryItemDef` | **PROVEN** later |
| `AddFirstDefClass` | first four names | **MATCH** first four; fifth **LEFTOVER** |
| `00DBDE40` | Oakvale | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\rtti.txt`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\004EE23F-fourth-class\README.md`
- `C:\FableCSharp\proofs\004EE23F-thing-components\README.md`
- `C:\FableCSharp\proofs\004EE23F-host-adddef\README.md`
