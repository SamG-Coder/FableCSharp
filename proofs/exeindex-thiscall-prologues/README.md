# Thiscall prologues merged near frontend / Leave / Init Game

Investigation only. No production `src/` edits. `tools/` was not
edited.

Question: `X86.IsFramePrologue` only accepts `push ebp`, so thiscall
`56 8B F1` at `00430900` is merged. How many **other** first-seen
thiscall prologues in `.text` are similarly merged near
frontend / Leave / Init Game?

Authority: `tools/Fable.ExeIndex` (`X86.IsFramePrologue`,
`Program.RunMapText` `FlushFn`), `functions.tsv`,
`listing-00400000.txt`, `listing-00580000.txt`, `e8.tsv`.
Siblings: `proofs/exeindex-fn-boundary-00430C80`,
`proofs/re-fn-boundary-fix`, `proofs/0042F491-init-game-callees`.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Thiscall prologue here means the two MSVC forms the sibling
boundary note already named:

```
56 8B F1     push esi; mov esi, ecx
53 56 8B F1  push ebx; push esi; mov esi, ecx
```

**First-seen** = the first instruction after INT3 pad (`CC` then
that sequence). Mid-body `56 8B F1` is not a start.

**Merged** = no `functions.tsv` row; swallowed by the previous
`IsFramePrologue` flush.

---

## Verdict

| Claim | Count | Class |
|---|---:|---|
| `IsFramePrologue` matches only `55 8B EC` / `55 8D 6C 24` / `55 8D AC 24` | — | **PROVEN** |
| `00430900` (`56 8B F1`) is a row in `functions.tsv` | 0 | **DISPROVEN** |
| `00430900` is merged into `0x00430345` (980 insns, next row `0x00431020`) | 1 | **PROVEN** |
| Other first-seen thiscalls **in that same tsv row** | **6** | **PROVEN** |
| Other first-seen thiscalls in the frontend / Leave / Init Game **neighborhood** (Init-frontend helpers through ENVIRONMENT Transfer), excluding `00430900` | **17** | **PROVEN** |
| First-seen thiscall starts **inside** Leave / Init Game frame `0042EC7C` | **0** | **PROVEN** |
| `005952C3` Init frontend site is a thiscall start | — | **DISPROVEN** (`add ecx, 32`) |
| Whole-`.text` count of every merged thiscall | — | **UNREAD** (not this question) |
| `004316CF` persist-dtor thiscalls first-seen on this walk | 7 sites | **PARTIAL** (family is next; not in the 00430800–00431200 window) |

**Answer:** **6** others share the `00430345` merge with
`00430900`. **17** others in the frontend / Leave / Init Game
neighborhood are the same class of miss.

---

## 1. Why `00430900` is merged

`X86.IsFramePrologue` (`X86.cs`):

```
if (data[i] != 0x55) return false;
```

`Program.RunMapText` flushes a `functions.tsv` row only when that
predicate is true. `INDEX.md` labels the tsv **frame prologues**.

`listing-00400000.txt`:

```
004308EE  ret
004308EF  int3
004308F0  mov eax, 0x260          ; size stub, not thiscall
004308F5  ret
004308F6  int3 … 004308FF int3
00430900  push esi                ; 56 8B F1
00430901  mov esi, ecx
…
0043101D  ret 4
00431020  push ebp                ; next frame
```

`functions.tsv`:

```
0x00430345	980	…	; swallows Transfer
0x00431020	26	…
```

No row `0x00430900`. New Game `fnmap.md` still lists
`0x00430900` (seed `range`) because `ScanRangeStarts` uses two
INT3s then `FindPrologue`. Text-map does not. **PROVEN.**

---

## 2. Same tsv row as `00430900` — 6 others

Frame `00430345` (`push ebp; mov ebp, esp`) through next frame
`00431020`. Every INT3-guarded thiscall in that span:

| VA | Bytes | Role in listing |
|---|---|---|
| `00430370` | `56 8B F1` | small ctor, vtbl `01230CBC` |
| `004303F0` | `56 8B F1` | dtor / scalar-deleting |
| `00430430` | `56 8B F1` | ctor, vtbl `01230E84` |
| `00430480` | `56 8B F1` | scalar-deleting → `004304A0` |
| `004304A0` | `56 8B F1` | dtor |
| `004304E0` | `53 56 8B F1` | ENVIRONMENT ctor, vtbl `01230EEC` |
| `00430900` | `56 8B F1` | Transfer (the example) |

`004308F0` is `B8 60 02 00 00` (size stub), not thiscall.

None of these seven VAs appear as `functions.tsv` keys. **6
other.** **PROVEN.**

`e8.tsv` has **zero** sites with dest `004304E0` or `00430900`
(vtbl). Dest `00430370` is hit from many later `00455xxx`
helpers, not from `0042EC7C`. First-seen *call* of Transfer is
**UNREAD** in E8; first-seen *prologue* after INT3 is **PROVEN**.

---

## 3. Neighborhood: frontend / Leave / Init Game

String sites (`listing-00400000.txt`) all live in one frame:

```
0042EC7C  push ebp                ; functions.tsv 668 insns
0042EF6F  push "Init frontend"
0042EF9C  call 0042DB40           ; only E8 thiscall on this arm
0042F2A2  push "Leave frontend"
0042F491  push "Init Game"
0042F5A9  push ebp                ; next frame
```

No INT3 + `56 8B F1` / `53 56 8B F1` between `0042EC7C` and
`0042F5A9`. **0** thiscall starts inside Leave / Init Game.
**PROVEN.**

`005952C3` (trace-frontend “Init frontend”) is `add ecx, 32` at
`listing-00580000.txt`. It sits inside frame `00594FDB` (333
insns). Not a thiscall start. **DISPROVEN.**

### 3a. Init-frontend helper cluster — 9

Frame `0042D5B1` (`push ebp`) → next frame `0042DBFA` (876
insns). INT3-guarded thiscalls:

| VA | Form |
|---|---|
| `0042D830` | `56 8B F1` |
| `0042D910` | `56 8B F1` |
| `0042D9A0` | `56 8B F1` |
| `0042D9E0` | `56 8B F1` |
| `0042DAB0` | `56 8B F1` |
| `0042DB20` | `56 8B F1` |
| `0042DB40` | `56 8B F1` |
| `0042DB60` | `56 8B F1` |
| `0042DBB0` | `56 8B F1` |

`0042DB40` is first-seen from `"Init frontend"` (`e8.tsv`
`0042EF9C → 0042DB40`). `0042DBD8` is `push esi; lea esi,
[ecx+177]` — not thiscall.

All nine missing from `functions.tsv`. **PROVEN.**

### 3b. Post-Leave FRONT_END — 1

Frame `0042F722` (`"FRONT_END"`) → `00430096`.

```
0042FB7F  int3
0042FB80  push esi
0042FB81  mov esi, ecx
```

Same release shape as `0042DBB0`. Merged. **PROVEN.**

### 3c. Persist helper immediately before the Transfer merge — 1

Frame `0043024E` → `00430345`.

```
004302FF  int3
00430300  push esi
00430301  mov esi, ecx
```

`e8.tsv` dest `00430300` includes `00431DD3` / `00431E53`
(persist family). Merged into `0043024E` (134 insns). **PROVEN.**

### Neighborhood total

```
  9  Init-frontend helpers in 0042D5B1
+ 1  0042FB80 in 0042F722
+ 1  00430300 in 0043024E
+ 6  others in 00430345 with 00430900
= 17 other merged first-seen thiscall prologues
```

`00430900` itself is the 18th site in that window.

---

## 4. Adjacent, not counted

Frame `004316CF` (1524 insns, next `00432AA8`) swallows more
INT3 thiscalls: `004317B0`, `004317D0` (`53 56 8B F1`),
`00431BE0`, `00431C30`, `00431DD0`, `00431DF0`, `00431E40`.
`00431ED0` / `00432180` are `push ebx; push esi; push edi` /
`push 80` — **not** the two-byte thiscall forms.

Those seven sit after persist frames `00431020`…`00431606` and
outside New Game range `environment-persist`
`(0x00430800, 0x00431200)`. Same bug class. **PARTIAL** as
first-seen on the frontend / Leave / Init Game walk.

`00418DCA` (game ctor, first-seen `E8` from `0042F4C7`) is
merged into `00418C3B` (1095 insns) but starts `53 56 57` with
**no** INT3 after `00418DC9 ret`. Not a first-seen thiscall
prologue. **DISPROVEN** for this count.

---

## Claims

| Claim | Status |
|---|---|
| `IsFramePrologue` accepts `56 8B F1` | **DISPROVEN** |
| Text-map splits `00430900` | **DISPROVEN** |
| Same-row other thiscalls | **6 PROVEN** |
| Neighborhood other thiscalls | **17 PROVEN** |
| Leave / Init Game frame contains a thiscall start | **DISPROVEN** |
| `0042DB40` first-seen from Init frontend | **PROVEN** (`e8.tsv`) |
| Unguarded `56 8B F1` in `IsFramePrologue` | **DISPROVEN** as a fix (mid-body false split; see sibling) |
