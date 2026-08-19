# `004EE23F` second `009B0AC0` / `0044C6B0` is `CHighlightItemDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent a listing parser. Read
`listing-004c0000.txt` after `004EE33E`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: After first `004EE23F` Add Def Class
`CHeroMorphDef` (`004EE33E` `009B0AC0`), what
is the **next** `009B0AC0` / `0044C6B0` on the
same function? Next class name? Still first-seen
leftover after host now registers only
`CHeroMorphDef`?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
after `004EE33E` (`e8` dest `009B0AC0` /
`0044C6B0` in `004EE23F` range);
`listing-00980000.txt` `009B0AC0`;
`e8.tsv` dests `0044C6B0` / `009B0AC0`;
`xrefs.tsv` `CHighlightItemDef`;
`src/Fable.Game/EngineLifecycle.cs`
(`AddFirstDefClass` / `FirstDefClassName`);
siblings `proofs/0044C6C2-plus40`,
`proofs/004EE23F-thing-components`,
`proofs/004EE23F-host-adddef`.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Next `0044C6B0` after `004EE337`? | **`004EE565`**. | **PROVEN** |
| Next `009B0AC0` after `004EE33E`? | **`004EE56C`**. | **PROVEN** |
| Next class name? | **`CHighlightItemDef`**. Push at `004EE532`. Factory imm `[ebp-1704]=0x4D8671`. | **PROVEN** |
| `CTCSimpleAppearanceMorph` is that next pair? | **No.** That is a CTC `004D2EF0` row (`004EE35A`). No `0044C6B0` / `009B0AC0`. | **DISPROVEN** |
| Host leftover after first-class register? | **Yes.** `AddFirstDefClass` Notes only `CHeroMorphDef`. Next Add Def Class is this pair. | **PROVEN** leftover |
| This site is Oakvale? | **No.** | **DISPROVEN** |

**Answer:** next same-fn Add Def Class is
**`CHighlightItemDef`** at **`004EE56C`**
(`0044C6B0` **`004EE565`**). Host first-class
arm is **MATCH**; this second class is
**LEFTOVER**.

---

## 1. First pair (already locked)

`listing-004c0000.txt`:

```
004EE304  push "CHeroMorphDef"
004EE321  call 0099EC30
004EE32D  mov [ebp-1688], 0x4E4219
004EE337  call 0044C6B0
004EE33C  mov ecx, eax
004EE33E  call 009B0AC0
```

`009B0AC0` logs `"Add Def Class"` then
`009AD6E0` / `009FC4F0` (`0044C6C2-plus40`).
Host `AddFirstDefClass` Notes this consume
only. **MATCH** for `CHeroMorphDef`.

---

## 2. Next `0044C6B0` / `009B0AC0` on `004EE23F`

No invented scan. Listing after `004EE33E`
is four CTC rows, then the next def pair:

```
004EE35A  push "CTCSimpleAppearanceMorph"   ; 004D2EF0 / 0x4D2A14
004EE3CA  push "CTCAtmosPlayer"             ; 004D2EF0 / 0x4D642D
004EE446  push "CTCRandomAppearanceMorph"   ; 004D2EF0 / 0x4D2A44
004EE4B6  push "CTCHighlightItem"           ; 004D2EF0 / 0x4D4548
004EE532  push "CHighlightItemDef"
004EE54F  call 0099EC30
004EE55B  mov [ebp-1704], 0x4D8671
004EE565  call 0044C6B0
004EE56A  mov ecx, eax
004EE56C  call 009B0AC0
```

`e8.tsv` dest `009B0AC0` in this fn: first
`0x004EE33E`, next **`0x004EE56C`**. Dest
`0044C6B0`: first `0x004EE337`, next
**`0x004EE565`**. Same four-insn shape as
the first class (copy name, store factory,
getter, Add Def Class).

Third same-fn pair (not this question) is
`CSmokeGeneratorDef` `004EE5F8` /
`004EE62B` / `004EE632`.

`xrefs.tsv`: string `CHighlightItemDef`
`0x0124422C` first at `004EE533`
(`fn=0x004EE137` fold). RTTI
`.?AVCHighlightItemDef@@` `0x01379340`.

---

## 3. CTC rows are not Add Def Class

Each CTC block is:

```
0099EBF0(name)
006869C0
004D2EF0(factory, 0, name)
004D9D2F
004E40C3(esi)
```

No `0044C6B0`. No `009B0AC0`. Treating
`CTCSimpleAppearanceMorph` as the next
`009B0AC0` class is **DISPROVEN**.

---

## 4. Factory `0x4D8671` (not a parser)

`004EE55B` stores `0x4D8671` the same way
`004EE32D` stores `0x4E4219` for
`CHeroMorphDef`. `listing-004c0000.txt`:

```
004D8671  push esi
004D8672  push 72
004D8674  call 00BFEA1A
004D8682  call 0044C0C0
004D8687  mov [esi], 0x123BD14
004D8690  ret
```

Ctor size **72**, vtbl **`0123BD14`**.
LoadDef payload / field walk **UNREAD**.
Do not invent `00A38E50`.

---

## 5. Host leftover after first-class MATCH

`EngineLifecycle.AddFirstDefClass` runs only
when `InitGameStages` name is
`"Init Thing Components"`:

- `Note(0044C6B0)`
- `Note(009B0AC0 CHeroMorphDef)`
- `Note(004E4219)`
- `Note(009AD6E0)` / `Note(009FC4F0)`
- `FirstDefClass = "CHeroMorphDef"`

No second `009B0AC0`. No
`CHighlightItemDef`. No `0x4D8671`.

| If host adds… | Leftover is… |
| --- | --- |
| first class only (current) | **`004EE565` / `004EE56C` `CHighlightItemDef`** |
| Note-only that second name | still `009AD6E0` / `009FC4F0` on this object (**not** MATCH) |
| live second Add Def Class | next omit is `CSmokeGeneratorDef` `004EE632` |

`EnsurePlayerManagerSingleton` / first
`+40` consume stay **MATCH**. This is the
first leftover **Add Def Class** after
that host arm. Whole remaining
`004EE23F` walk is still leftover
(`004EE23F-thing-components`).

---

## 6. Not Oakvale

No `00DBDE40` / region / TNG / hero create
on this pair. Parent is `004EE23F`.
**DISPROVEN.**

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `004EE33E` / `004EE337` | first Add Def Class `CHeroMorphDef` | **PROVEN**; host **MATCH** |
| `004EE35A` | `CTCSimpleAppearanceMorph` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EE3CA` / `004EE446` / `004EE4B6` | next three CTC names | **PROVEN** |
| `004EE532` | push `"CHighlightItemDef"` | **PROVEN** |
| `004EE565` | next `0044C6B0` | **PROVEN** |
| `004EE56C` | next `009B0AC0` | **PROVEN** leftover |
| `004D8671` / `0123BD14` | factory / vtbl | **PROVEN** site; LoadDef **UNREAD** |
| `004EE632` | third Add Def Class `CSmokeGeneratorDef` | **PROVEN** later |
| `AddFirstDefClass` | only `CHeroMorphDef` | **MATCH** first; second **LEFTOVER** |
| `00DBDE40` | Oakvale | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\0044C6C2-plus40\README.md`
- `C:\FableCSharp\proofs\004EE23F-thing-components\README.md`
- `C:\FableCSharp\proofs\004EE23F-host-adddef\README.md`
