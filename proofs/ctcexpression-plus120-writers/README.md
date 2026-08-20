# Who writes `CTCExpression+120` (no-save Lookout Present)

Investigation only. No production `src/` or `tests/`
edits.

Question: who first writes a **non-empty** CString at
`CTCExpression+120` after no-save New Game / Lookout
first Present? Is that string ever `Q_NewOakValeIntro`
intern `0x012C5D14`?

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
`00CD6E27` is bind-only. `00CE7670` only waits.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: Fable.exe via ExeIndex
`listing-007c0000.txt` (`007EF200` / `007EF36B` /
`007EF389` `00415DD0` / `007EF3A1` `004B4A10` /
`007EF070` / `007EEFE0`);
**not** `listing-00780000.txt` (that map ends
`007BFFFF`; `007EF200` lives in `007C0000`);
`listing-004c0000.txt` (`004DB050` / `004DB06C` /
`004DB085` / `004DAC61` / `004DAC79` / `004DA939` /
`004DC78F` / `004DC7E8`);
`listing-00440000.txt` (`00456964` / `0045699A` /
`004569A7` / `00456A54` / `0045228F` / `0044FC00` /
`0045D637` / `0045D70B` / `0044D6C5`);
`listing-00400000.txt` (`00415DD0`);
`listing-00680000.txt` (`00686800` / `006869D0` /
`006AC430`);
`disp.tsv` disp `120`; `vtbl.tsv` `0x0124026C` /
`0x012401F4` / `0x01233D1C`; `rtti.txt`
`CTCExpression` `0x0137A424` / `CExpressionDef`
`0x01376DCC`; `strings.tsv` / `xrefs-by-string.tsv`
`Q_NewOakValeIntro` `0x012C5D14`; compiled-defs
`game/entries.tsv` `EXPRESSION` size **187**;
siblings `proofs/007EEF60-activate`,
`proofs/ctcexpression-quest-names`,
`proofs/q-novi-activator-callers`.

---

## Verdict

**Nobody writes a CString at `CTCExpression+120`.**
The component is **20 bytes**. Offset `+120` is not
on it.

`007EF200` **reads** `[esi+120]` after
`esi = [component 0x8F + 12]`. That nested object
**MATCH**es compiled **`EXPRESSION`** (vtbl
`0x01233D1C`, factory `0045D70B` `push 0x90`),
**not** the 16-byte `CExpressionDef` wrapper.

Writers of **`EXPRESSION+120`** (the CString
`007EF200` copies into `004B4A10`):

| Site | What it writes | When | Class |
|---|---|---|---|
| `0045699A` ctor | dword **`-1`**, not intern `0x122D70E` | factory `0045D70B` | **PROVEN** not a CString intern |
| `00456A54` persist | `0045228F` → `0044FC00` intern pointer | `0044C72B` Compile / `game.bin` | **PROVEN** first CString writer |
| `0045D6A4` copy | `mov [esi+120], [edi+120]` | vtbl `0x01233D1C` slot 19 | **PROVEN** dword copy |
| `007EF36B` / `00415DD0` | **copy-out** to stack | thing tick | **DISPROVEN** as a writer of `+120` |

That persist runs at **Init Definition Manager
Compile**, **before** New Game and **before**
Lookout first Present.

After Lookout Present, no recovered `.text` store
fills this slot. `007EF200` only **reads**.

Is the intern ever `0x012C5D14` `"Q_NewOakValeIntro"`?
**DISPROVEN** as a `.text` immediate store.
**UNREAD** as a `game.bin` payload byte. Compiled
`EXPRESSION` **instance names** are
`EXPRESSION_FOLLOW` … social rows, **not** `Q_*`.
`0x012C5D14` xrefs are bind + Gameflow wait only.

Host must **not** invent `ActivateQuest("Q_NewOakValeIntro")`
from this VA.

---

## Status table

| Claim | Class | Evidence |
|---|---|---|
| `007EF200` listing | **PROVEN** | `listing-007c0000.txt`. `listing-00780000.txt` has **0** `007EF*` |
| `007EF200` class | **PROVEN** | ctor `004DB085` `mov [esi], 0x124026C`; vtbl slot 28; type-id `004DB06C` `0x8F`; intern `004D4B72` `"CTCExpression"` |
| `CTCExpression` alloc size | **PROVEN** | factory `004DC7E8` `push 20` then `004DB085`. Parent size `004DAC79` returns **14** |
| `CExpressionDef` alloc size | **PROVEN** | factory `004DC78F` `push 16` then `004DB050`. Zeros `[esi+12]` only |
| `CTCExpression` has a field at `+120` | **DISPROVEN** | 20-byte object. Ctor chain `00686800` / `004DA939` / `004DAC61` / `004DB085` never stores `+120` |
| `007EF200` `esi` | **PROVEN** | `ebp` = `0x8F` component; `mov esi, [ebp+12]` |
| Nested `+116/+120/+124/+126` | **MATCH** | `007EF30E` dword `+116`; `lea ebx, [esi+120]` CString; `+124` / `+126` bytes. Same offsets on `EXPRESSION` persist `004569A7` |
| Nested object class | **PROVEN** `EXPRESSION` | `0044D6C5` `"EXPRESSION"` + factory `0045D70B` (`abs.tsv` `0044D6E4`); ctor vtbl `0x01233D1C`; size slot `004569A1` `0x90` |
| Sibling name `CExpressionDef+120` | **PARTIAL** | Wrapper is 16 bytes. `+120` is on the looked-up `EXPRESSION` at `[+12]`, not on the wrapper |
| `00415DD0` at `007EF389` | **PROVEN** copy-**out** | `ecx = &esi+120` source; dest = `[esp+16]`. Then `004B4A10`. Does **not** store into `+120` |
| `disp.tsv` `007EF*` `+120` stores | **PROVEN** none | sole hit `007EF36B` `lea ebx, [esi+120]` (read) |
| First CString writer of `EXPRESSION+120` | **PROVEN** | persist `00456A54` / `0045228F` / `0044FC00` during `0044C72B` Compile |
| That persist is after Lookout Present | **DISPROVEN** | Compile is Init Definition Manager, before Leave / New Game / `006B3FF0` |
| `.text` `push 0x012C5D14` into `+120` | **DISPROVEN** | `xrefs.tsv` `0x012C5D14`: `00CD6E28` / `00CD6E87` bind; `00CE791E` / `00CE7978` / `00CE79CA` wait. **0** other `.text` |
| `game.bin` `EXPRESSION` name = Oakvale | **DISPROVEN** | `entries.tsv` 39 rows, all `EXPRESSION_*` / `NULLDEF_EXPRESSION`. Size 187, **0** named extra fields dumped |
| `LookoutPoint.tng` supplies the name | **DISPROVEN** | sibling `ctcexpression-quest-names`: **0** `Q_NewOakValeIntro` / `StartCTCExpression` |
| `004B4A10` at `007EF3A1` on first Present | **DISPROVEN** as first no-save activate | empty intern skip; no Lookout `0x8F`; sibling `q-novi-activator-callers` |
| Host invents `ActivateQuest(Q_NewOakValeIntro)` | **DISPROVEN** | no `src/` `CTCExpression`; test `No_save_does_not_activate_Q_NewOakValeIntro` |

---

## 1. `CTCExpression` has no `+120`

Ctor `004DB085` (`listing-004c0000.txt`):

```
004DB085  push esi
          push [esp+8]
          mov esi, ecx
          call 004DAC61
004DB091  mov [esi], 0x124026C
          mov eax, esi
          pop esi
          ret 4
```

`004DAC61` → `004DA939` → `00686800`:

```
00686800  call 0099A2F0
          mov [esi+4], arg0          ; owner Thing
          mov [esi+8], 0 / [esi+9], 0
          mov [esi], 0x125BE0C
004DA939  and [esi+12], 0
          mov [esi+16], 1            ; parent size 13
004DAC79  mov eax, 14                ; mid size
004DC7E8  push 20                    ; CTCExpression factory
          call 00BFEA1A
          call 004DB085
```

**PROVEN** 20-byte component: `+0` vtbl, `+4` owner,
`+8/+9` flags, `+12` pointer, `+16` byte. No CString
at `+120`.

`CExpressionDef` factory `004DC78F` `push 16` /
ctor `004DB050` zeros `[esi+12]`. Also too small
for `+120`.

---

## 2. `007EF200` reads nested `EXPRESSION+120`

`listing-007c0000.txt` (int3-bounded `007EF200` …
`ret 4` `007EF4DB`):

```
007EF215  mov ebx, ecx               ; this = CTCExpression
          …
007EF2C7  mov ebx, [ebx+4]           ; Thing
007EF2DC  mov [esp+20], 0x8F
          call 004365B0              ; component 0x8F
007EF2FA  mov ebp, [eax+4]           ; 0x8F instance
007EF303  mov esi, [ebp+12]
007EF30E  mov eax, [esi+116]
          test eax, eax
          je  007EF36B               ; else camera 0041649C
007EF36B  lea ebx, [esi+120]
          push 0x122D70E             ; empty intern
          call 005FA740
          je  skip                   ; empty → no 004B4A10
007EF386  push edx                   ; dest = stack CString
          mov ecx, ebx               ; source = +120
007EF389  call 00415DD0
          mov al, [esi+124]
          push eax / push 0 / push &copy
007EF3A1  call 004B4A10
```

`00415DD0` (`listing-00400000.txt`):

```
00415DD0  mov eax, [ecx]             ; source intern
          push eax
          push dest
          mov ecx, 0x13CA828
          call 009D49B0              ; intern table copy into dest
          ret 4
```

**PROVEN** reader of `[esi+120]`, writer of a
**stack** CString. Not a store to the object.

`disp.tsv` in `007EF000`–`007EF4DB`: **one** disp-120
row, the `lea` at `007EF36B`. **PROVEN** no store
in the tick.

---

## 3. Nested object is compiled `EXPRESSION`

`0044C72B` bank registrar (`listing-00440000.txt`):

```
0044D6C5  push "EXPRESSION"
          …
0044D6E4  mov [ebp-16], 0x45D70B     ; factory
          call 009B0AC0
```

Factory:

```
0045D70B  push 0x90
          call 00BFEA1A
          jmp  00456964              ; ctor
0045696C  mov [esi], 0x1233D1C
          or  eax, -1
          … stores -1 at +60..+108 …
          and [esi+112], 0
0045699A  mov [esi+120], eax         ; -1, not 0x122D70E
```

Persist slot 18 `004569A7`:

```
00456A49  lea eax, [esi+116]
          call 00456AD9              ; pointer / ref persist
00456A54  lea eax, [esi+120]
          call 0045228F              ; CString persist
00456A5F  lea eax, [esi+124]
          call 0043314A              ; byte
          … +125 byte …
00456A75  lea eax, [esi+126]
          call 0043314A              ; byte
```

`0045228F` load arm: `0044FC00` `mov [dest], intern`.
**PROVEN** CString intern store at `EXPRESSION+120`.

Copy slot 19 `0045D637`:

```
0045D6A4  mov eax, [edi+120]
0045D6A7  mov [esi+120], eax
```

Layout **MATCH**es `007EF200` use of `+116` /
`+120` / `+124` / `+126`.

`CExpressionDef` persist `007EF070` does **not**
write `+120`. It persist-names `"ExpressionDef"`
(`004109A0`) and, when the name is non-empty vs
`0x122D70E`, stores a looked-up pointer at
`[this+12]` via `006869D0` / `00593666`. Slot 4
`007EEFE0` lazy-fills the same `+12` from
`[owner+112]` / `005DA240` / `005F81BE`.

So `[0x8F + 12]` is the `EXPRESSION*` (or a
refcount copy of it). The CString `007EF200`
copies is **`EXPRESSION+120`**.

`game.bin` row size is **187**, factory alloc
**144**. Extra bytes after `0x90` are **UNREAD**
here (not needed for `+120`).

---

## 4. All `.text` stores onto this `+120`

`disp.tsv` has hundreds of unrelated `+120` stores
(UI, cameras, `fstp`, `mov [ebp+120]` locals in
`004804DE`, …). Filtered to **this object**:

| VA | Insn | Object |
|---|---|---|
| `0045699A` | `mov [esi+120], eax` (`-1`) | `EXPRESSION` ctor |
| `00456A54` | `lea eax, [esi+120]` → `0045228F` | `EXPRESSION` persist |
| `0045D6A4` | `mov [esi+120], [edi+120]` | `EXPRESSION` copy |
| `007EF36B` | `lea ebx, [esi+120]` | **read** only |

No `lea ecx, [EXPRESSION+120]` / `call 00415DD0`
with dest = `+120`. `00415DD0` at `007EF389` has
`ecx` = source.

`CTCExpression` methods never `mov [this+120]`.
**PROVEN.**

Ctor `-1` is **not** empty intern `0x122D70E`.
If persist were skipped, `005FA740` would see
non-empty and could take `004B4A10` with a
garbage intern. Compile persist **does** run
(`0044C72B` `009B08C0` / `009B0AC0`
`"EXPRESSION"`). After load, `+120` is whatever
`game.bin` stored (empty intern or a real name).

---

## 5. Not `Q_NewOakValeIntro`

`xrefs.tsv` `0x012C5D14`:

```
00CD6E28  00CD5170   bind S_QNOVI
00CD6E87  00CD5170   bind
00CE791E  00CE7670   Gameflow wait
00CE7978  00CE7670   wait
00CE79CA  00CE7670   wait
```

**PROVEN** complete `.text` set. None is
`0045699A` / `00456A54` / `0045D6A4` /
`007EF200` / `00415DD0` / `004B4A10`.

`abs.tsv` has **0** `0x012C5D14` in
`00456964`–`00456AD6` or `007EF200`–`007EF4DB`.

`game.bin` `EXPRESSION` names (sibling
`ctcexpression-quest-names`): social
`EXPRESSION_FOLLOW` / `WAIT` / `FLIRT` / … **0**
`Q_*`. Payload CString at persist slot `+120`
is **UNREAD** (dump column “0 extra named
fields”).

Lookout TNG: **0** `StartCTCExpression` /
`ExpressionDef` / `QuestName` / `Q_NewOakValeIntro`.
**PROVEN** (sibling). First Present leftover **#4**
is Lookout `006B3FF0`, not Oakvale intro.

---

## 6. Timing (no-save)

```
004185D9  Init Definition Manager
  0044C72B  009B0AC0 "EXPRESSION" factory 0045D70B
            009B08C0 compiled open
            00456964 ctor  +120 = -1
            004569A7 persist 00456A54  +120 = intern   // first CString
Leave / New Game / user.ini ActivateQuest("Gameflow")
004A0D90  AddQuest FALSE Q_NewOakValeIntro → +184 only
00CE7670  wait IsActive(Q_NewOakValeIntro) = 0 → yield
Lookout first Present  006B3FF0
  007EF200  not on Lookout TNG 0x8F
            even if a creature later attaches 0x8F:
            +120 already filled at Compile; tick only reads
```

**PROVEN** first CString write is Compile persist,
not Present. **DISPROVEN** as a first-Present
writer of `Q_NewOakValeIntro`.

---

## Evidence → Original → Host → Gap

**Evidence.** Factory sizes, `007EF200` one-deref
`[+12]+120`, `EXPRESSION` persist/copy/ctor,
`00415DD0` copy-out, intern xrefs.

**Original.** Native fills `EXPRESSION+120` from
`game.bin` at Compile. Thing tick `007EF200`
activates `004B4A10` only when that CString is
non-empty vs `0x122D70E`. No-save Lookout Present
does not run that activate with
`Q_NewOakValeIntro`.

**Host.** `EngineLifecycle` Notes
`"004B4A10 not Q_NewOakValeIntro"`. No
`ActivateQuest("Q_NewOakValeIntro")`. No
`CTCExpression` type in `src/`. **MATCH** skip.
Do **not** wire a Present-time writer.

**Gap.** Live intern at `EXPRESSION+120` after
`game.bin` load is **UNREAD**. Creature-def attach
of `0x8F` on Lookout things is **UNREAD** (TNG
has none; `00846710` / `004C9D60` later).

---

## Next unread site

1. **`game.bin` `EXPRESSION` persist field for
   `+120`.** 39 rows, size 187, dump shows 0 named
   extras. Hex the CString intern after the
   `+116` pointer / before the `+124` byte. Compare
   to `0x012C5D14` and to empty `0x122D70E`.
2. **`007EAC10` (CTC vtbl slot 4) vs `[this+12]`.**
   Ends `007EAE40` looking up `"CActionUseDef"`
   (compiled size **29**, cannot own `+120`) into
   `esi+12`. Conflicts with `007EF200` treating
   `[0x8F+12]` as `EXPRESSION*` (`0x90`). Prove
   dest object / whether slot 4 runs on no-save.
3. **First live `0x8F` after Lookout region.**
   Not TNG `StartCTC*`. Creature sub-object attach
   `00846710` — listing-00840000. Still must not
   invent Oakvale activate.

Until (1) shows intern `0x012C5D14`, the no-save
activator stays **UNKNOWN** and must not be
invented.
