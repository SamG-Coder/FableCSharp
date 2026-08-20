# `004EE23F` sixth `009B0AC0` / `0044C6B0` is `CInventoryItemDef`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave
this walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD` → `00418585` `004EE23F`.
Do **not** invent a listing parser. Read
`listing-004c0000.txt` after `004EE932`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: After `CCreatureNavigationDef`
`004EE932` `009B0AC0`, what is the **next**
`0044C6B0` / `009B0AC0` on `004EE23F`?
Confirm `CInventoryItemDef` at `004EF244`.
Factory imm? CTC rows between
(`CTCPhysicsNavigator` first)? Ctor size /
vtbl if `00BFEA1A` listing exists.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
after `004EE932` (`e8` dest `009B0AC0` /
`0044C6B0` in `004EE23F` range);
`listing-00440000.txt` factory `0044F644` /
ctor `0044C108`;
`listing-00980000.txt` `009B0AC0`;
`e8.tsv` dests `0044C6B0` / `009B0AC0`;
`xrefs.tsv` `CInventoryItemDef`;
`src/Fable.Game/EngineLifecycle.cs`
(`AddFirstDefClass` / `FifthDefClassName`);
sibling `proofs/004EE23F-fifth-class`.

---

## Verdict

| Question | Answer | Class |
| --- | --- | --- |
| Next `0044C6B0` after `004EE92B`? | **`004EF23D`**. | **PROVEN** |
| Next `009B0AC0` after `004EE932`? | **`004EF244`**. | **PROVEN** |
| Next class name? | **`CInventoryItemDef`**. Push at `004EF20A`. Factory imm `[ebp-1712]=0x44F644`. | **PROVEN** |
| `CTCPhysicsNavigator` is that next pair? | **No.** That is a CTC `004D2EF0` row (`004EE94E`). No `0044C6B0` / `009B0AC0`. | **DISPROVEN** |
| CTC rows between fifth and sixth class? | **Nineteen.** First is `CTCPhysicsNavigator` `004EE94E` / `0x4D29E1`. Last is `CTCInventoryItem` `004EF19A` / `0x4DC5C7`. | **PROVEN** |
| Factory ctor? | `0044F644`: `00BFEA1A(112)` then `jmp 0044C108`; vtbl **`01231DBC`**. | **PROVEN** |
| Host leftover after fifth class Note? | **Yes.** `AddFirstDefClass` Notes `CHeroMorphDef` … `CCreatureNavigationDef`. Next Add Def Class is this pair. | **PROVEN** leftover |
| This site is Oakvale? | **No.** | **DISPROVEN** |

**Answer:** next same-fn Add Def Class is
**`CInventoryItemDef`** at **`004EF244`**
(`0044C6B0` **`004EF23D`**). Host
first+second+third+fourth+fifth class arms
are **MATCH** Note-only; this sixth class is
**LEFTOVER**.

---

## 1. Fifth pair (already locked)

`listing-004c0000.txt` / sibling
`004EE23F-fifth-class`:

```
004EE8F8  push "CCreatureNavigationDef"
004EE915  call 0099EC30
004EE921  mov [ebp-1696], 0x4DA871
004EE92B  call 0044C6B0
004EE930  mov ecx, eax
004EE932  call 009B0AC0
```

Host `AddFirstDefClass` Notes this consume
after `CTimeAppearanceFadeDef`. **MATCH**
Note-only for `CCreatureNavigationDef`.

---

## 2. Next `0044C6B0` / `009B0AC0` on `004EE23F`

No invented scan. Listing after `004EE932`
is nineteen CTC rows, then the next def pair:

```
004EE94E  push "CTCPhysicsNavigator"     ; 004D2EF0 / 0x4D29E1
004EE9BE  push "CTCTargetingAI"           ; 0x4E0047
004EEA3A  push "CTCTargetingPlayer"       ; 0x4E3783
004EEAAA  push "CTCTargeted"              ; 0x4DBF1D
004EEB26  push "CTCSpecialAbilities"      ; 0x4D2D98
004EEB96  push "CTCUndeadSoul"            ; 0x4D7AE9
004EEC12  push "CTCSoundPlayer"           ; 0x4D30EE
004EEC82  push "CTCTalk"                  ; 0x4D2E0A
004EECFE  push "CTCInventory"             ; 0x4D2E74
004EED6E  push "CTCInventoryClothing"     ; 0x4E8967
004EEDEA  push "CTCInventoryWeapons"      ; 0x4D2EBA
004EEE5A  push "CTCInventoryAbilities"    ; 0x4D300F
004EEED6  push "CTCInventoryMagic"        ; 0x4D2F46
004EEF46  push "CTCInventoryStats"        ; 0x4D2FDC
004EEFC2  push "CTCInventoryExperience"   ; 0x4D3042
004EF032  push "CTCInventoryTrade"        ; 0x4D3075
004EF0AE  push "CTCInventoryQuests"       ; 0x4D30A8
004EF11E  push "CTCInventoryMap"          ; 0x4D2F13
004EF19A  push "CTCInventoryItem"         ; 0x4DC5C7
004EF20A  push "CInventoryItemDef"
004EF227  call 0099EC30
004EF233  mov [ebp-1712], 0x44F644
004EF23D  call 0044C6B0
004EF242  mov ecx, eax
004EF244  call 009B0AC0
```

`e8.tsv` dest `009B0AC0` in this fn: first
`0x004EE33E`, second `0x004EE56C`, third
`0x004EE632`, fourth `0x004EE704`, fifth
`0x004EE932`, next **`0x004EF244`**. Dest
`0044C6B0`: first `0x004EE337`, second
`0x004EE565`, third `0x004EE62B`, fourth
`0x004EE6FD`, fifth `0x004EE92B`, next
**`0x004EF23D`**. Same four-insn shape as
the first five classes (copy name, store
factory, getter, Add Def Class).

Seventh same-fn pair (not this question) is
`CLookDef` `004EF34C` / `004EF37F` /
`004EF386`. CTC rows after this pair start
at `CTCCreatureExpression` `004EF260`.

`xrefs.tsv`: string `CInventoryItemDef`
`0x012441D4` first at `004EF20B`
(`fn=0x004EE137` fold). RTTI
`.?AVCInventoryItemDef@@` `0x013761B4`.
Same factory imm `0x44F644` is also stored
for type name `"INVENTORY_ITEM"` at
`0044D053` (`listing-00440000.txt`).

---

## 3. CTC rows are not Add Def Class

The nineteen blocks between `004EE932` and
`004EF244` are the same `004D2EF0` shape
as the four CTC rows before the fifth class:

```
0099EBF0("CTCPhysicsNavigator")
006869C0
004D2EF0(0x4D29E1, 0, name)
004D9D2F
004E40C3(esi)
```

No `0044C6B0`. No `009B0AC0`. Treating
`CTCPhysicsNavigator` (or any later CTC
row in this gap) as the next `009B0AC0`
class is **DISPROVEN**. Contrast the four
CTC rows between fourth and fifth class
(`004EE23F-fifth-class`).

---

## 4. Factory `0x44F644` (not a parser)

`004EF233` stores `0x44F644` the same way
`004EE921` stores `0x4DA871` for
`CCreatureNavigationDef`. `listing-00440000.txt`:

```
0044F644  push 112
0044F646  call 00BFEA1A
0044F64B  test eax, eax
0044F64D  pop ecx
0044F64E  je 0044F657
0044F650  mov ecx, eax
0044F652  jmp 0044C108
0044F657  xor eax, eax
0044F659  ret
```

`0044C108` (`listing-00440000.txt`):

```
0044C108  push esi
0044C109  mov esi, ecx
0044C10B  call 0044C0C0
0044C110  fldz
0044C112  mov [esi], 0x1231DBC
0044C118  fstp [esi+41]
0044C11B  xor eax, eax
0044C11D  fldz
0044C11F  mov [esi+37], eax
0044C122  fstp [esi+45]
0044C125  mov [esi+49], al
0044C128  mov [esi+50], al
0044C12B  mov eax, esi
0044C12D  pop esi
0044C12E  ret
0044C12F  push 112
0044C131  pop eax
0044C132  ret
0044C133  push esi
0044C134  push -1
0044C136  push "CPhysicsDef"
```

Ctor size **112**, vtbl **`01231DBC`**.
Immediate neighbor `0044C133` pushes
`"CPhysicsDef"` (RTTI `CPhysicsDef`
`0x013761D4` sits after
`CInventoryItemDef` `0x013761B4`). LoadDef
payload / field walk **UNREAD**. Do not
invent `00A38E50`. `0044F65A` writes
vtbl `01230BA0` then `009FC550` — dtor
shape, not this factory.

---

## 5. Host leftover after fifth-class MATCH

`EngineLifecycle.AddFirstDefClass` runs only
when `InitGameStages` name is
`"Init Thing Components"`:

- `Note(0044C6B0)` / `Note(009B0AC0 CHeroMorphDef)` / `Note(004E4219)`
- `Note(004EE565)` / `Note(009B0AC0 CHighlightItemDef)` / `Note(004D8671)`
- `Note(004EE62B)` / `Note(009B0AC0 CSmokeGeneratorDef)` / `Note(004DA82B)`
- `Note(004EE6FD)` / `Note(009B0AC0 CTimeAppearanceFadeDef)` / `Note(004D84C8)`
- `Note(004EE92B)` / `Note(009B0AC0 CCreatureNavigationDef)` / `Note(004DA871)`
- `Note(009AD6E0)` / `Note(009FC4F0)` on each
- `FirstDefClass` / `SecondDefClass` / `ThirdDefClass` / `FourthDefClass` / `FifthDefClass` set

No sixth `009B0AC0`. No
`CInventoryItemDef`. No `0x44F644`.

| If host adds… | Leftover is… |
| --- | --- |
| first…fifth Note-only (current) | **`004EF23D` / `004EF244` `CInventoryItemDef`** |
| Note-only that sixth name | still `009AD6E0` / `009FC4F0` on this object (**not** MATCH) |
| live sixth Add Def Class | next omit is `CLookDef` `004EF386` |

`EnsurePlayerManagerSingleton` / first
`+40` consume stay **MATCH**. Fifth class
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
| `004EE932` / `004EE92B` | fifth Add Def Class `CCreatureNavigationDef` | **PROVEN**; host **MATCH** Note-only |
| `004EE94E` | `CTCPhysicsNavigator` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EE9BE` | `CTCTargetingAI` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EEA3A` | `CTCTargetingPlayer` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EEAAA` | `CTCTargeted` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EEB26` | `CTCSpecialAbilities` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EEB96` | `CTCUndeadSoul` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EEC12` | `CTCSoundPlayer` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EEC82` | `CTCTalk` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EECFE` | `CTCInventory` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EED6E` | `CTCInventoryClothing` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EEDEA` | `CTCInventoryWeapons` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EEE5A` | `CTCInventoryAbilities` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EEED6` | `CTCInventoryMagic` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EEF46` | `CTCInventoryStats` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EEFC2` | `CTCInventoryExperience` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EF032` | `CTCInventoryTrade` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EF0AE` | `CTCInventoryQuests` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EF11E` | `CTCInventoryMap` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EF19A` | `CTCInventoryItem` CTC row | **PROVEN**; **DISPROVEN** as next `009B0AC0` |
| `004EF20A` | push `"CInventoryItemDef"` | **PROVEN** |
| `004EF23D` | next `0044C6B0` | **PROVEN** |
| `004EF244` | next `009B0AC0` | **PROVEN** leftover |
| `0044F644` / `0044C108` / `01231DBC` | factory / ctor / vtbl; size 112 | **PROVEN** site; LoadDef **UNREAD** |
| `004EF386` | seventh Add Def Class `CLookDef` | **PROVEN** later |
| `AddFirstDefClass` | first five names | **MATCH** first five; sixth **LEFTOVER** |
| `00DBDE40` | Oakvale | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\rtti.txt`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\004EE23F-fifth-class\README.md`
- `C:\FableCSharp\proofs\004EE23F-thing-components\README.md`
- `C:\FableCSharp\proofs\004EE23F-host-adddef\README.md`
