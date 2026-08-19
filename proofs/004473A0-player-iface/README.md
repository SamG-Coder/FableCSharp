# Init Player Interface `004473A0` vs host `Player.Construct`

Investigation only. No production `src/` / `tests/` edits.

Question: `"Init Player Interface"` `004473A0`. Host
`EnterGame` calls `Player.Construct` and notes
`004473A0` / `0044A3B0` / `00488D20`. MATCH vs leftover?
First-seen body vs host `Construct`?

Do **not** start Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` `004184BD`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00440000.txt`
(`004473A0` / `0044A3B0` / `0044C6B0` / `00446EF0`);
`listing-00400000.txt` (`004184BD` `004186E2`–
`00418736`, `0041732A`, `004193C4`, `00415FBC`);
`listing-00480000.txt` (`00488D10` / `0048A210`);
`listing-00680000.txt` (`00687A30` / `00687A70`);
`listing-00a00000.txt` (`00A0D4A0` / `00A0D2B0` /
`00A0D4F0` / `00A0D300`);
`e8.tsv` dests `004473A0` / `0044A3B0` / `00488D10`;
`src/Fable.Game/EngineLifecycle.cs`
(`EnterGame` Init Player Interface arm);
`src/Fable.Game/PlayerInterface.cs`
(`Construct` / `ActionInputListener`);
siblings `proofs/audit-playerinterface`,
`proofs/input-vtbl56-vs-ui32`,
`proofs/initgame-after-leave-order`,
`proofs/004166A8-create-players-work`.

Do not re-prove frontend `0042E3EE` vs game
`00446A30`. That split is `audit-playerinterface`.

---

## Verdict

**First leftover: `Register(new ActionInputListener())`
and the `00488D20` / `00687A70` notes.** Native
`004473A0` never calls those. `00488D20` is not a
function (`push edi` inside `00488D10`).

The named-stage note `004473A0 size 0x898 vtbl
01231BDC game+32` is **MATCH**. Host `Construct` is
**PARTIAL** against the first-seen body (list init
and the `[0x13B8648]==0` tail are omitted).
`0044A3B0` is the previous stage (`0041732A` →
`game+28`); this ctor only **stores** it at `+1788`.

| Claim | Class |
| --- | --- |
| Only `.text` `E8` of `004473A0` is `00418729` in `"Init Player Interface"` | **PROVEN** |
| Alloc `0x898`, vtbl `01231BDC`, `004193C4` → `game+32` | **PROVEN** / host note **MATCH** |
| Args: `this` = new blob; `push esi` (game) then `push [esi+28]` (owner) | **PROVEN** |
| First child of `004473A0` is `00A0D4A0` | **PROVEN**; host **PARTIAL** (omitted) |
| `[+1788]=owner`, `[+1784]=game`, `[+1948]=0`, `[+2196]=0` | **PROVEN**; flags **MATCH** `Construct` |
| `004473A0` calls `00488D20` / `00687A30` / `00687A70` | **DISPROVEN** |
| `00488D20` is the listener factory entry | **DISPROVEN** — entry is `00488D10` |
| Only `E8` of `00488D10` is `0048A38C` (`0048A210`, Create Players slots 0–3) | **PROVEN** |
| Host `Construct` `Register(ActionInputListener)` on this arm | **LEFTOVER** — **first leftover** |
| Host notes `00488D20 00687A30 vtbl 0123758C +4` / `00687A70 00A0D2B0 00A0D4F0` here | **LEFTOVER** |
| Only `E8` of `0044A3B0` is `00417345` in `"Init Player Manager"` | **PROVEN** |
| Host notes `0044A3B0` under Player Interface | **DIVERGE** (one stage late); as **arg** **MATCH** |
| `0044A3B0` size 44, vtbl `01231CD0`, `+12/+16/+20=0` | **PROVEN** |
| `0044A3B0` writes `+24=0` | **DISPROVEN** (no store); lookup miss is **PARTIAL** |
| `0044A3B0` first-seen `"hero_swap_1.tng"` … `_4.tng` at `+32` | **PROVEN**; host this arm **PARTIAL** |
| First-seen `[0x13B8648]==0` tail `00415FBC` / `00446EF0` | **PROVEN**; host **PARTIAL** |
| Oakvale / `00DBDE40` on this ctor | **DISPROVEN** |
| Frontend `0042E3EE` is this object | **DISPROVEN** (`audit-playerinterface`) |

**Answer:** first leftover is **`00488D20` /
`ActionInputListener` register** on the Init Player
Interface arm. First omitted native child is
**`00A0D4A0`**.

---

## 1. Parent: `"Init Player Interface"`

`listing-00400000.txt` / `e8.tsv` dest `004473A0`
= only `00418729`:

```
004186E2  call 0041732A          ; "Init Player Manager"
004186E9  … "Init Player Interface"
00418714  push 0x898
00418719  call 00BFEA1A
00418723  push [esi+28]          ; owner from 0044A3B0
00418726  mov ecx, eax
00418728  push esi               ; game
00418729  call 004473A0
00418732  push eax
00418733  lea ecx, [esi+32]
00418736  call 004193C4          ; store at game+32
```

Host `InitGameStages` name + apply **MATCH**.
`EnterGame` then `Player.Construct()` plus four
`Note`s.

---

## 2. First-seen `004473A0` body

`listing-00440000.txt`. `thiscall` + two stdcall
dwords (`ret 8`). `ecx` = new `0x898`.

```
004473A0  esi = this
004473A5  call 00A0D4A0          ; +0 vtbl 0129CA38; +4 list zero
004473AA  ecx = arg2             ; [game+28]
004473AE  eax = arg1             ; game
004473B2  [esi+1788] = ecx       ; owner
004473B8  edi = esi+1796
004473C0  [esi] = 0x1231BDC      ; PlayerInterface vtbl
004473C6  [esi+1784] = eax       ; game
004473CC  call 0099A2F0          ; edi
004473D4  [edi] = 0x122D06C
004473DA  call 0099AED0          ; edi+24
004473EA  call 00A04410          ; esi+1832
          zeros: +1928 … +2012, +1792/1793/1824
0044749D  call 00A0D300          ; esi+2016 event
004474A2  [esi+1948] = 0         ; Disabled
004474A8  [esi+2196] = 0         ; FallbackArmed
004474AE  [esi+1956] = 0
004474B4  [esi+2192] = 0
004474BA  cmp [0x13B8648], 0
004474C0  jne 004474DE           ; first-seen taken: ==0
004474C2  ecx = [esi+1784]       ; game
004474C8  call 00415FBC          ; jmp 0044C6B0 → [0x13B879C]
004474CD  edx = [eax+208]
004474D3  eax = [edx+60]
004474D9  call 00446EF0          ; this = player interface
004474E3  ret 8
```

Zero `E8` of `00488D20`, `00488D10`, `00687A30`,
`00687A70`, `00A0D2B0`, `0044A3B0`.

`00A0D4A0` is `PlayerInterface.ListInitFn`: base
`0099A2F0`, vtbl `0129CA38`, zeros `+4…+16` and
`+1780`, ten `004038C0` records from `+28`. That
is the `+4` list host later pretends to fill with
one listener.

---

## 3. Host `EnterGame` / `Construct`

```
EnterGame
  … foreach InitGameStages
    "Init Player Interface":
      Player.Construct()
        Present=true
        Disabled=false            ; +1948
        FallbackArmed=false       ; +2196
        OwnerDefaultResult=0      ; claimed +24
        Register(ActionInputListener)   ← leftover
      Note 004473A0 size 0x898 vtbl 01231BDC game+32
      Note 0044A3B0 game+28 size 44 +12 empty +24=0
      Note 00488D20 00687A30 vtbl 0123758C +4
      Note 00687A70 00A0D2B0 00A0D4F0
```

| Native first-seen | Host | Class |
| --- | --- | --- |
| `00BFEA1A(0x898)` + vtbl `01231BDC` + `game+32` | note only; `Player` already lives on `EngineLifecycle` | **MATCH** identity; object **PARTIAL** |
| `00A0D4A0` | none | **PARTIAL** — first omit |
| `[+1788]=[game+28]`, `[+1784]=game` | none | **PARTIAL** |
| `0099A2F0` / `0099AED0` / `00A04410` / `00A0D300` | none | **PARTIAL** |
| `[+1948]=0` `[+2196]=0` | `Disabled` / `FallbackArmed` | **MATCH** |
| `[0x13B8648]==0` → `00415FBC` / `00446EF0` | none | **PARTIAL** |
| no listener | `Register(ActionInputListener)` | **LEFTOVER** |
| no `00488D20` | `Note(PlayerListenerFactoryFn)` | **LEFTOVER** |
| no `00687A70` | `Note(PlayerListenerRegisterFn)` | **LEFTOVER** |

`PumpPlayerInterface` also calls `Construct()`
after `WorldFrame>1`. Native pump is
`[game+32].vtbl+4` `00446A30` on an object already
built here. Extra `Construct` there is a second
host no-op once `Present`.

---

## 4. `0044A3B0` is not this ctor

`e8.tsv` dest `0044A3B0` = only `00417345`:

```
0041732A  push 44
00417330  call 00BFEA1A
0041733C  call 0044C6B0
00417345  call 0044A3B0
0041734F  lea ecx, [esi+28]
00417352  call 004193A0
```

`0044A3B0`: vtbl `01231CD0`, `[+4]=game`,
`[+8]=0044C6B0()`, `[+12/+16/+20]=0`, vector at
`+32`, first string `"hero_swap_1.tng"` then `_2`
`_3` `_4`. **No** write to `+24`.

Host note under Player Interface is **DIVERGE**
vs `"Init Player Manager"` (`initgame-after-leave-order`
row 7). As the pointer `004473A0` stores at `+1788`,
the object is **MATCH**. `+24=0` bundles
`00449990` miss into the owner ctor (**PARTIAL**).
`hero_swap_*.tng` is **not** Create Players
(`004166A8-create-players-work`).

---

## 5. `00488D20` leftover VA; factory is Create Players

`listing-00480000.txt`:

```
00488D10  sub esp, 16
00488D13  push ebx
00488D14  push esi
00488D15  mov esi, ecx
00488D17  mov eax, [esi+28]
…
00488D20  push edi               ; not a prologue
00488D21  push 40
00488D33  call 00BFEA1A
00488D4A  call 00687A30
00488D4F  mov [edi], 0x123758C   ; first of many vtbls
          stores at esi+256, +260, +264, …
```

`e8.tsv` dest `00488D10` = only `0048A38C`.
`0048A210` is `0044A289` inside `0044A1A0`
(Create Players `0044A530` slots). Gate
`[esi+520]` from blob `+20`: slots **0–3** call
the factory; slot **4** skips.

`00687A30` is a 40-byte listener base
(vtbl `0125BE98`). Dedicated `0123758C` ctor is
`00486340` (no `.text` `E8` recovered). Register
thunk `00687A70` → `00A0D2B0` → `00A0D4F0` is
**not** called from `004473A0` or `00488D10`
(`00688D19` / `00688E9B`).

Host one-listener `+4` list at Init Player
Interface is invented. Native first `0123758C`
alloc on this walk is later, on a **slot** object,
not `game+32`.

---

## 6. Do not invent

- `004473A0` as listener factory.
- `00488D20` as a function.
- `0044A3B0` constructed inside Player Interface.
- `hero_swap_*.tng` as Hero spawn / Create Players.
- Oakvale / `00DBDE40` / `S_QNOVI`.
- Frontend `0042E3EE` / input vtbl+56 `0041E6D3`
  as this object (`input-vtbl56-vs-ui32`).

---

## Open

- `00446EF0` first-seen arg `[0044C6B0()+208]+60`
  (**PARTIAL**; body unread past the `0044C6B0`
  hop).
- Whether Create Players `00488D10` ever calls
  `00687A70` onto `game+32` `+4` (**UNREAD**; not
  this stage).
- `0099A2F0` / `0099AED0` / `00A04410` inner
  objects at `+1796` / `+1832` (**UNREAD**).
