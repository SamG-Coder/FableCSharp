# After `ApplyPlayerOwner`: first leftover on `004473A0`

Investigation only. No production `src/` / `tests/`
edits. Do **not** start Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"`
`004184BD`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: After host `ApplyPlayerOwner` at
`0041732A`, Init Player Interface `004473A0`
still has leftover `Register(ActionInputListener)`
/ `00488D20` notes. Confirm first leftover is
still `00A0D4A0` list init **vs** the listener
register. Smallest MATCH slice?

Authority: existing `proofs/004473A0-player-iface`,
`proofs/0041732A-host-owner`;
`listing-00440000.txt` `004473A0`;
`listing-00400000.txt` `0041732A` /
`004186E2`–`00418736`;
`listing-00480000.txt` `00488D10` /
`00488D20`;
`listing-00a00000.txt` `00A0D4A0` /
`00A0D4F0`;
`src/Fable.Game/EngineLifecycle.cs`
(`EnterGame` Init Player Interface arm,
`ApplyPlayerOwner`);
`src/Fable.Game/PlayerInterface.cs`
(`Construct` / `Register` /
`ActionInputListener.FactoryFn`).
Do not re-prove frontend `0042E3EE` vs
`00446A30`.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| `ApplyPlayerOwner` still leftover on this arm? | **No.** `"Init Player Manager"` `0041732A` only. | **MATCH** there; **DISPROVEN** leftover here |
| Host still `Register(ActionInputListener)` on `Construct`? | **Yes.** Live `PlayerInterface.Construct`. | **LEFTOVER** |
| Host still Notes `00488D20` / `00687A70` here? | **Yes.** `EnterGame` Interface arm. | **LEFTOVER** |
| First leftover **on this named arm**? | **`Register` + `00488D20` notes.** Not `00A0D4A0`. | **PROVEN** leftover |
| First leftover is `00A0D4A0`? | **No.** That is the first **omit**, not an extra. | **DISPROVEN** as leftover |
| First child of native `004473A0`? | **`00A0D4A0`** at `004473A5`. | **PROVEN** |
| Smallest MATCH slice still omitted? | **`00A0D4A0` list init** (base `0099A2F0`, vtbl `0129CA38`, zeros `+4…+16` and `+1780`, ten `004038C0` from `+28`). | **PARTIAL** omit |
| Native `004473A0` calls `00488D20` / `00488D10` / `00687A70`? | **No.** Zero `E8`. `00488D20` is `push edi` inside `00488D10`. | **DISPROVEN** |
| Oakvale on this ctor? | **No.** | **DISPROVEN** |

---

## Verdict

**First leftover is still the listener register,
not `00A0D4A0`.**

`ApplyPlayerOwner` closed the old Interface-arm
`0044A3B0` DIVERGE (`0041732A-host-owner`).
What remains on `"Init Player Interface"` is
the same pair `004473A0-player-iface` named:

- **LEFTOVER** (host extra): `Construct` →
  `Register(new ActionInputListener())` plus
  Notes `00488D20 00687A30 vtbl 0123758C +4`
  and `00687A70 00A0D2B0 00A0D4F0`.
- **PARTIAL** (first native omit / smallest
  MATCH slice): first child `00A0D4A0`.

Do **not** treat list init as leftover extra.
Do **not** grow the MATCH slice into
`Register` / `00488D20`. Native first-seen
`+4` list is **empty** after `00A0D4A0`.
The one-listener list is invented on this
stage.

Named-stage note `004473A0 size 0x898 vtbl
01231BDC game+32` and flags `[+1948]=0` /
`[+2196]=0` stay **MATCH**.

---

## 1. Host after `ApplyPlayerOwner` (read only)

`InitGameStages` order **MATCH**:

```
("Init Player Manager",   InitPlayerManagerFn),  // 0041732A
("Init Player Interface", 0x004473A0),
```

`EnterGame` loop:

```
if (name == "Init Player Manager")
    ApplyPlayerOwner();          // 0044C6B0 / 0044A3B0 / 004193A0
if (name == "Init Player Interface")
{
    Player.Construct();          // Register leftover
    Note(004473A0 size 0x898 vtbl 01231BDC game+32);
    Note(00488D20 00687A30 vtbl 0123758C +4);
    Note(00687A70 00A0D2B0 00A0D4F0);
}
```

`Construct` (unchanged):

```
Present = true
Disabled = false            // +1948
FallbackArmed = false       // +2196
OwnerDefaultResult = 0
Register(new ActionInputListener())   // leftover
```

No `Note(ListInitFn)` / `00A0D4A0`. No
`[+1788]=[game+28]`. No `00BFEA1A(0x898)`
object. No `00415FBC` / `00446EF0` tail.

---

## 2. Native first-seen (unchanged)

`listing-00400000.txt` only `.text` `E8` of
`004473A0` is still `00418729`:

```
004186E4  call 0041732A          ; ApplyPlayerOwner MATCH
004186E9  … "Init Player Interface"
00418714  push 0x898
00418719  call 00BFEA1A
00418723  push [esi+28]          ; owner already live
00418726  mov ecx, eax
00418728  push esi
00418729  call 004473A0
00418732  push eax
00418733  lea ecx, [esi+32]
00418736  call 004193C4
```

`listing-00440000.txt` first child:

```
004473A0  esi = this
004473A5  call 00A0D4A0          ; FIRST child
004473AA  ecx = arg2             ; [game+28]
004473B2  [esi+1788] = ecx
004473C0  [esi] = 0x1231BDC
004473C6  [esi+1784] = arg1      ; game
          … 0099A2F0 / 0099AED0 / 00A04410 / 00A0D300 …
004474A2  [esi+1948] = 0
004474A8  [esi+2196] = 0
004474BA  cmp [0x13B8648], 0
004474C0  jne 004474DE           ; first-seen ==0 taken
004474C8  call 00415FBC
004474D9  call 00446EF0
004474E3  ret 8
```

Zero `E8` of `00488D20`, `00488D10`,
`00687A30`, `00687A70`, `00A0D2B0`,
`00A0D4F0`, `0044A3B0`.

`00A0D4A0` (`listing-00a00000.txt`):
`0099A2F0`, `[this]=0129CA38`, zeros
`+4/+8/+12/+16`, ten `004038C0(40,4,004159A0)`
records from `+28` step `0xB0`, `[+1780]=0`.
That **empties** the `+4` list host later
pretends to fill.

`00488D20` is still `push edi` inside
`00488D10` (`listing-00480000.txt`). Only
`E8` of `00488D10` is `0048A38C` (Create
Players slots). Not this stage.

---

## 3. MATCH vs leftover vs omit

| Host | Native first-seen | Class |
| --- | --- | --- |
| `ApplyPlayerOwner` on Manager | `004186E4` `0041732A` | **MATCH** |
| stage apply `004473A0` | only `E8` `00418729` | **MATCH** |
| `Note` size `0x898` vtbl `01231BDC` `game+32` | alloc + store | **MATCH** identity; object **PARTIAL** |
| `Disabled` / `FallbackArmed` false | `[+1948]=0` `[+2196]=0` | **MATCH** |
| `00A0D4A0` | first child | **PARTIAL** — first omit; smallest MATCH |
| `[+1788]=owner` `[+1784]=game` | stores | **PARTIAL** |
| `0099A2F0` / `0099AED0` / `00A04410` / `00A0D300` | later children | **PARTIAL** |
| `[0x13B8648]==0` → `00415FBC` / `00446EF0` | tail | **PARTIAL** |
| `Register(ActionInputListener)` | no listener | **LEFTOVER** — first leftover |
| `Note(00488D20 …)` / `Note(00687A70 …)` | no such calls | **LEFTOVER** |
| `OwnerDefaultResult=0` as ctor `+24` | owner has no `+24` store | **LEFTOVER** (owner proof) |

`0044A3B0` as **arg** at `+1788` is **MATCH**
identity. Host still does not store it on the
Interface object (**PARTIAL**).

---

## 4. Smallest MATCH slice

After `ApplyPlayerOwner`, the next native
work that is **not** leftover extra:

1. Keep existing `Note(004473A0 …)` and
   `Present` / `Disabled` / `FallbackArmed`.
2. Call / Note `00A0D4A0` on the new
   `0x898` blob (`ListInitFn`).
3. Leave `+4` list **empty**. Do **not**
   `Register`.

Out of this slice: later children
`0099A2F0` / `00A0D300` / `00446EF0`,
Create Players `00488D10`, pump
`00446A30`.

---

## Do not invent

- `00A0D4A0` as leftover extra (it is omit).
- `Register` / `00488D20` as MATCH on this arm.
- `00488D20` as a function.
- `0044A3B0` constructed inside Player Interface.
- Oakvale / `00DBDE40` / `S_QNOVI`.
- Frontend `0042E3EE` as this object.

---

## Open

- `00446EF0` first-seen arg
  `[0044C6B0()+208]+60` (**PARTIAL**; unread
  past the getter hop).
- Whether Create Players `00488D10` ever
  `00687A70` onto `game+32` `+4` (**UNREAD**;
  not this stage).
- Inner objects at `+1796` / `+1832`
  (**UNREAD**).
