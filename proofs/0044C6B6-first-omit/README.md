# First omitted `004184BD` child `0044C6B6` before Init Thing Components

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` (`0042F44D`) →
`"Init Game"` `0042F491` → `00418DCA` →
`[vtbl+4]` `004184BD`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `0044C6B6` is the first omitted
`004184BD` child before `"Init Thing Components"`.
What does it call? Host leftover vs Note-only?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`004184BD` `00418507`–`00418585`, `0041732A`,
`004166A8`, `004175E5` `0041773F`);
`listing-00440000.txt` (`0044C6B0`–`0044C72A`,
`00450142`);
`listing-009c0000.txt` (`009FC520`);
`listing-01200000.txt` (`01228BA2`);
`e8.tsv` dests `0044C6B6` / `0044C6C2` / `0044C71F`;
`functions.tsv` `004184BD` / `0044C6C2`;
`docs/runtime/FORWARD_TREE.md` §6;
`src/Fable.Game/EngineLifecycle.cs`
(`InitGameStages` / `EnterGame` / `CreatePlayers`
/ `PlayerManagerGetter`);
siblings `proofs/initgame-after-leave-order`,
`proofs/004168DC-after-graphics`,
`proofs/004166A8-create-players-work`.

---

## Verdict

**`0044C6B6` itself calls nothing.** It is a
six-byte leaf: `xor eax,eax` / `cmp [0x13B879C],eax`
/ `setne eax` / `ret`. Not the getter `0044C6B0`
(`mov eax,[0x13B879C]; ret`).

The omitted **site** is `0041852D` in `004184BD`,
after `[game+104]` and **before** the
`"Init Thing Components"` log / `004EE23F`.
First-seen `[0x13B879C]` is 0, so `al=0` and the
parent **does** run `00BFEA1A(0xE0)` → ctor
`0044C6C2` → store `0044C71F` (`00450142` writes
the pointer).

Host has **no** `Note(0044C6B6)` and no 0xE0
ensure. `InitGameStages` starts at Thing
Components. That is a **LEFTOVER omit of work**,
not a leftover Note and not Note-only. Later
host `Note(0044C6B0)` at `"Create Players"` /
pump is the **getter**, after this hole.

| Claim | Class |
| --- | --- |
| First omitted `004184BD` child before Thing Components is `0044C6B6` | **PROVEN** |
| `0044C6B6` `E8` callees | **none** — **PROVEN** |
| Only `.text` `E8` of dest `0044C6B6` is `0041852D` | **PROVEN** |
| First-seen `[0x13B879C]==0` → parent takes alloc/`0044C6C2`/`0044C71F` | **PROVEN** |
| Only `.text` `E8` of dests `0044C6C2` / `0044C71F` are `00418547` / `00418552` | **PROVEN** |
| `0044C6C2` first `E8`s | `0099B6B0` / `009B0470` / `0099B510` / `009FC520` — **PROVEN** |
| `0044C71F` first `E8` | `00450142` → `[0x13B879C]=obj` — **PROVEN** |
| Host `EnterGame` / `InitGameStages` runs this site | **DISPROVEN** — **LEFTOVER** omit |
| Host leftover is a `Note(0044C6B6)` | **DISPROVEN** — no note |
| Note-only would MATCH native | **DISPROVEN** — work is alloc+ctor+store |
| This site is Oakvale / `00DBDE40` | **DISPROVEN** |
| This ctor is `"Init Player Manager"` `0044A3B0` (44 bytes, `game+28`) | **DISPROVEN** |

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| What does `0044C6B6` call? | **Nothing.** Presence check on `[0x13B879C]`. | **PROVEN** |
| What does the omitted `004184BD` site call first-seen? | `00BFEA1A(0xE0)` then `0044C6C2` then `0044C71F`. | **PROVEN** |
| `0044C6C2` then? | `0099B6B0` / `009B0470` / `0099B510`; vtbl `01232C24`; zeros `+208..+220`; `009FC520(0x80000)` → `[this+40]`. | **PROVEN** stores; `009FC210` tail **PARTIAL** |
| Host leftover vs Note-only? | **LEFTOVER omit of the work.** Not Note-only. Not a leftover note. | **PROVEN** |

---

## 1. Site: first unnamed child before Thing Components

`listing-00400000.txt` `004184BD`:

```
00418507  mov eax, [0x13B8390]
00418518  fild [eax+456]
0041852A  fstp [esi+104]
0041852D  call 0044C6B6              ; THIS SITE
00418532  test al, al
00418534  jne 00418557               ; skip if already live
00418536  push 0xE0
0041853B  call 00BFEA1A
00418545  mov ecx, eax
00418547  call 0044C6C2
00418550  mov ecx, eax
00418552  call 0044C71F
00418557  or edi, -1
0041855B  push "Init Thing Components"
00418585  call 004EE23F
```

`functions.tsv` `004184BD` callees:
`…009ED190,0044C6B6,00BFEA1A,0044C6C2,0044C71F,0099EBF0,…004EE23F…`.

`e8.tsv`:

| dest | site |
| --- | --- |
| `0044C6B6` | only `0041852D` |
| `0044C6C2` | only `00418547` |
| `0044C71F` | only `00418552` |

`00417742` `jmp 0044C71F` is teardown of
`004175E5` (`xor ecx,ecx` then store 0). Not
this walk. `01228BA2` `jmp 0044F3DE` is CRT
release of `0x13B879C`. Not first-seen.

No `"Init …"` string on this site.

---

## 2. `0044C6B6` body — no calls

`listing-00440000.txt`:

```
0044C6B0  mov eax, [0x13B879C]
0044C6B5  ret
0044C6B6  xor eax, eax
0044C6B8  cmp [0x13B879C], eax
0044C6BE  setne eax
0044C6C1  ret
```

Two functions. Host `PlayerManagerGetter` is
`0044C6B0`, not this check.

No writer of `[0x13B879C]` runs before
`00418552`. Only store wrapper is `0044C71F` →
`00450142` `mov [esi], eax` with
`esi=0x13B879C`. First-seen BSS is 0, so
`00418534` is **not** taken. **PROVEN.**

---

## 3. First-seen ensure

`0044C6C2` (`listing-00440000.txt`):

```
0044C6C9  push 0x1232C30
0044C6D1  call 0099B6B0
0044C6D6  push 0x44C6AF              ; empty ret
0044C6E1  call 009B0470
0044C6E9  call 0099B510
0044C6F0  mov [esi], 0x1232C24
0044C6F6  mov [esi+208..220], 0
0044C708  push 0x80000
0044C715  call 009FC520
```

`009FC520` (`listing-009c0000.txt`):
`[ecx+40]=arg` (`0x80000`) then `jmp 009FC210`.
That tail is **PARTIAL** here.

`0044C71F`:

```
0044C71F  push ecx                   ; 0xE0 object
0044C720  mov ecx, 0x13B879C
0044C725  call 00450142
```

`00450142`: `[0x13B879C]=obj`; if non-null,
alloc 12, `[+0]=1` `[+4]=0044EAFD` `[+8]=obj`,
store wrapper at `[0x13B87A0]`.

Later `"Init Player Manager"` `0041732A` is a
**different** `00BFEA1A(44)` / `0044A3B0` into
`game+28`. It **reads** this singleton via
`0044C6B0`. **DISPROVEN** as the same ctor.

Type name behind vtbl `01232C24` / push
`01232C30` is **UNREAD** in `strings.tsv` /
`rtti.txt` on this pass. Host label
`PlayerManagerVa` is a name, not a listing
RTTI hit.

---

## 4. Host leftover vs Note-only

`EnterGame` notes `004184BD` / `009E9EF0` /
`009ED190` then `foreach InitGameStages`.
First stage is `("Init Thing Components",
0x004EE23F)`. No `0044C6B6` / `0044C6C2` /
`0044C71F` constant. No 0xE0 alloc.

`CreatePlayers()` (named stage, **after**
Display) notes `0044C6B0 [0x13B879C]` then
`0044A530`. Pump notes the same getter +
`009AC9E0`. Those are **later getter notes**,
not this ensure.

| If host adds… | Then leftover is… |
| --- | --- |
| **Note-only** `0044C6B6` | still `00BFEA1A` / `0044C6C2` / `0044C71F` (**not** MATCH) |
| **real work** at `0041852D` | this hole **MATCH**; first walk omit stays later (`004168DC` work, already a sibling leftover) |

Contrast `"Adding Console Variables"`
`0041863D`: log-only, Note-only **MATCH**.
This site is **not** log-only.

---

## 5. Not Oakvale

No `00DBDE40` / region / TNG / hero on this
site. After Leave the parent is `004184BD`.
**DISPROVEN.**

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0041852D` | first omitted `004184BD` child | **PROVEN** |
| `0044C6B6` | `[0x13B879C]!=0` leaf | **PROVEN**; host **LEFTOVER** omit |
| `0044C6B0` | getter; not this site | **PROVEN**; host later note **MATCH** getter |
| `00BFEA1A` | `0xE0` first-seen | **PROVEN** |
| `0044C6C2` | ctor, vtbl `01232C24` | **PROVEN**; type name **UNREAD** |
| `009FC520` | `[this+40]=0x80000` | **PROVEN**; `009FC210` **PARTIAL** |
| `0044C71F` / `00450142` | store `[0x13B879C]` | **PROVEN** |
| `004EE23F` | next named child | **PROVEN** |
| `0041732A` / `0044A3B0` | later 44-byte owner | **DISPROVEN** as this ctor |
| `00DBDE40` | Oakvale | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-009c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-01200000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\docs\runtime\FORWARD_TREE.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\initgame-after-leave-order\README.md`
