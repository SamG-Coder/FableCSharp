# Type `0x33` on `[iface+4]+96` — not the 52-byte `QM+56` slot

Investigation only. No production `src/` edits.

Question: Gameflow wait `00893570` succeeds only when
`008ABED0` finds a type-`0x33` object on `[iface+4]+96`
whose `004AF3C0` CString equals `Q_NewOakValeIntro`.
What is type `0x33`? Who inserts it onto that list when a
quest is actually constructed (`004B3CE0`)? Is that the
52-byte `QM+56` slot type? How would a `00CB5AD0` hit
create this object?

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")` as
the wait un-blocker. That path posts kind **`0x37`**, not
`0x33`.

Do **not** treat the 52-byte `004B0310` / `004B4063` slot
as a type-`0x33` event. `[slot+8]` is the run pointer.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: ExeIndex
`assembly/exe/01-sections/text-map/listing-00880000.txt`
(`00893570` / `00893610` / `00892F80` / `008ABED0` /
`00892E80`),
`listing-00680000.txt` (`00687540` / `00687310` /
`006AF1C0` / `006AF180` / `00686A70`),
`listing-006c0000.txt` (`006E7740` / `006E7510` /
`006E7530`),
`listing-00480000.txt` (`004B3CE0` / `004B4040` /
`004B4063` / `004B1D30` / `004B4590` / `004B4A10` /
`004AF3C0` / `004AF740` / `004A6550`),
`listing-00c80000.txt` (`00CB5AD0`),
`listing-005c0000.txt` (`005E7B77`),
`assembly/exe/00-index/vtbl.tsv` `0x01260F0C` slot 25 /
slot 276 / slot 288, vtbl `0x0125BE8C`;
siblings `proofs/gameflow-state0-wait`,
`proofs/004B3CE0-factory0`,
`proofs/004B4063-stub-layout`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| What is type `0x33`? | **Event-node kind 51** on the world event list. `00687540` payload, vtbl `0125BE8C`. `[obj+8]==0x33`, `[obj+52]` = `0049D870` WorldFrame, `[obj+60]` = `QM+44` catalog index. | **PROVEN** |
| Same as 52-byte `QM+56` slot? | **No.** Slot is `00BFEA1A(52)`, `[+0]` id, `[+8]` run, name CString at `+48`. No type dword at `+8`. No `+60`. Linked on `QM+56`, not `[world+96]`. | **DISPROVEN** |
| Who inserts `0x33` inside `004B3CE0`? | **Nobody.** Factory arm posts **`00687540(55, 50)`** = kind **`0x37`**. Factory-0 stub posts **no** event. | **PROVEN** omit |
| Insert site for kind `0x33`? | `00687540` with first arg `0x33`, `ecx=[world+96]`. Quest-manager site: **`004B1D30`** (`CGameScriptInterface` vtbl slot 288 **`00892F80`**, `vtbl+1152`). Later UI site: **`005E7B77`**. | **PROVEN** |
| Does `00CB5AD0` hit create type `0x33`? | **No.** Hit → `004BB720` factory ≠ 0 → `004B3CE0` live arm → 52-byte `004B0310` on `QM+56` + **kind `0x37`** on `[world+96]`. Kind `0x33` is a different post. | **DISPROVEN** |
| Same list as the wait? | **Yes.** `[iface+4]+96` = `[world+96]` = `[QM+124]+96`. | **PROVEN** |

**Type identity: event kind `0x33` (51). Insert on construct: none. Insert site: `004B1D30` → `00687540(0x33)`.**

---

## Verdict

`008ABED0` walks a circular list of **96-byte event nodes**
(`00BFEA0E(96)`, payload at `node+8`, copy ctor
`00687310`). Type is **`[payload+8]`**. Kind `0x33` is
one event class on that list, next to `0x34`…`0x38` and
the construct post `0x37`.

The 52-byte object `004B3CE0` always allocates is the
**named quest slot** on `QM+56`. It is **not** typed
`0x33`. The wait never reads `QM+56`
(`gameflow-state0-wait`: `004AF610` **DISPROVEN** as this
predicate).

`004B3CE0` **does** insert onto the wait’s list, but the
kind is **`55` (`0x37`)**, delay `50`. `00893570` asks
for **`0x33`**, so construct / `ActivateQuest` /
`00CB5AD0` hit **cannot** satisfy the wait.

To put a type-`0x33` node whose `[+60]` names
`Q_NewOakValeIntro` on that list, something must call
`00687540(0x33, 50, …, 004AF740(name))` with
`ecx=[world+96]`. The quest-manager writer is
`004B1D30`. `00CB5AD0` is not on that path.

---

## Timeline (no-save vs the missing node)

```
004A6550 Init Scripts
  006E7740 CGameScriptInterface
    [iface+0] = 01260F0C
    [iface+4] = world                    // ctor arg2
  004B4590 QuestManager
    [QM+124] = world                     // ctor arg0
    [QM+120] = factory table
user.ini ActivateQuest("Gameflow")
  00892E80 vtbl+1104 → 004B4A10 → 004B4260
    00CB5AD0 hit → 004B3CE0 factory arm
      004B0310 52-byte slot → QM+56
      00CB7900 fiber
      00687540(55, 50)                   // kind 0x37, NOT 0x33
first type-1 00CE7670 state 0
  vtbl+100 00893570("Q_NewOakValeIntro")
    ecx = [iface+4]+96 = [world+96]
    008ABED0 type 0x33 → 0
    yield
```

`Q_NewOakValeIntro` is already in `QM+44` from QST
`AddQuest(..., FALSE)`. Membership is **not** a type-`0x33`
node. **PROVEN.**

---

## 1. Type `0x33` is `[event+8]`, found by `008ABED0`

`listing-00880000.txt` `00893570`:

```
esi = CGameScriptInterface (vtbl 01260F0C)
006E7510 / 006E7530        ; QM+136 context +24 / +28
ecx = [esi+4]              ; world
ecx = [ecx+96]             ; event list
[key+0] = 0x33
[key+4] = 006E7510-1       ; range lo
[key+8] = 006E7530         ; range hi
call 008ABED0
hit: eax = payload (node+8)
     [eax+60] → 004AF3C0(QM, index) → CString
     compare to arg  ("Q_NewOakValeIntro")
```

`008ABED0`:

```
walk [list+4] circular
payload = node+8
006AF1C0: [payload+8] == type          ; 0x33
006AF180: [payload+52] in [lo, hi]     ; WorldFrame window
return payload or 0
```

`006E7510` / `006E7530` (`listing-006c0000`):
`[0x13B89FC]+136` → `+8` → `+44` → `[+24]` / `[+28]`.
Empty context → 0. First-seen wait still misses at
`008ABED0` even before the name compare
(`gameflow-state0-wait`). **PROVEN.**

`004AF3C0`: `QM+44[index]` if `0 <= index < (end-begin)/4`,
else sentinel `0x13BD804`. **Index into the AddQuest
catalogue**, not a walk of `QM+56`. **PROVEN.**

Sibling GET `00893610` uses the **same** type `0x33` and
copies the CString (`0099EFB0`) with no name compare.
`00893690` uses type **`0x34`**. Nearby lookups use
`0x26`…`0x28`. **PROVEN** family of typed events, not
one slot class.

---

## 2. The list is `[world+96]`, same as `[QM+124]+96`

`006E7740` (`listing-006c0000`):

```
[this]    = 0x1260F0C
[this+4]  = arg2
```

`004A6633` `push esi` (world) as that arg2, then
`call 006E7740`. **`[iface+4] = world`.** **PROVEN.**

`004B4590` (`listing-00480000`) after ten `00BFEA0E`
pushes (`add esp, 40` is later at `004B470E`):

```
[QM+120] = arg1   ; factory table (00CB5C70)
[QM+124] = arg0   ; world
[QM+128] = arg2   ; CGameScriptInterface
```

`004A6692` `push esi` (world) as arg0, `call 004B4590`.
**`[QM+124] = world`.** **PROVEN.**

So:

```
[iface+4]+96  = [world+96]
[QM+124]+96   = [world+96]
```

`00687540` `this` at construct and at GiveQuest is that
list. `008ABED0` `this` is the same object. **PROVEN.**

---

## 3. How `00687540` builds the node

`listing-00680000.txt`:

```
ecx = list ([world+96])
arg0 = type                 ; 0x33 / 0x37 / …
arg1 = 50                   ; delay (host EventPostDelay)
arg7 = catalog index        ; 004AF740 result
template vtbl = 0125BE8C    ; EventNodeVtbl
[template+8]  = arg0        ; TYPE
[template+52] = 0049D870    ; WorldFrame
[template+60] = arg7        ; QM+44 index
00BFEA0E(96)
00687310 copies template → node+8
splice onto [list+4] circular
ret 32
```

`00687310` stores `[dst+8] = [src+8]` (type) and
`[dst+60] = [src+60]` (index). That is exactly the pair
`00893570` reads. **PROVEN.**

Node size **96**. Payload starts at `+8`, so
`[payload+60]` is allocation offset `68`. The 52-byte
quest slot **cannot** hold this layout. **PROVEN**
different object.

---

## 4. `004B3CE0` posts kind `0x37`, not `0x33`

Live factory arm (`listing-00480000` `004B400A`):

```
push edi                     ; 12-byte rec (name)
call 004AF740                ; QM+44 index
eax = [QM+124]
ecx = [eax+96]               ; [world+96]
push index
push 0,0,0,0
lea edx, [esp+76]
push edx
push 50                      ; 0x32
push 55                      ; 0x37
call 00687540
```

Only reached when `[rec+4] != 0` and `[0x1375454] != 0`.
That is the `00CB5AD0` **hit** arm (`004B0310` +
`00CB7900` + this post). **PROVEN.**

Factory-0 stub `004B4063`: `00BFEA1A(52)`, name `+48`,
16-byte node on `QM+56`. **No** `00687540`.
(`004B3CE0-factory0` / `004B4063-stub-layout`.) **PROVEN.**

Host `EventPostKind = 55` / `EventPostDelay = 50` matches
this construct post, **not** kind `0x33`. **MATCH** for
`0x37`; **DISPROVEN** as the wait’s type.

---

## 5. Who *does* insert type `0x33`

### `004B1D30` (quest Give)

`listing-00480000.txt`:

```
004AF740(name) → index
ecx = [QM+124]+96            ; [world+96]
00687540(53, 50, …, index)   ; kind 0x35
if out-byte == 0:
  00687540(51, 50, …, index) ; kind 0x33  ← 004B1DC4
then 004B0160 / 004B0C80     ; card / helper
```

`00892F80` (`listing-00880000`): `ecx=[0x13B89FC]`,
`call 004B1D30`. `vtbl.tsv` `01260F0C` **slot 288**
(`+1152`). Script Give / “quest is now visible”
writer. **PROVEN** site.

This function does **not** call `004B4260` / `004B3CE0` /
`00CB5AD0`. Give ≠ construct. **PROVEN.**

First-seen Gameflow wait never calls slot 288.
**PROVEN** omit on that tick (`00CE7670` only
`vtbl+100`).

### `005E7B77` (later)

`listing-005c0000.txt`: when an inner `eax==1` arm
runs, `004AF740` then `00687540(51, 50)` with
`ecx` from `00686A70` → `[eax+96]`. `eax==2` posts
kind `0x34` instead. Not `004B3CE0`. Not first-seen
Leave. **PROVEN** second writer; **UNREAD** as
Oakvale’s first giver.

Other `push 51` hits (`004D7E7C` GetType ret,
`008C24DC` `0073A8A0`, `00E1AC1C` `vtbl+2792`) are
**not** `00687540` onto `[world+96]`. **DISPROVEN**
as this list insert.

---

## 6. `00CB5AD0` hit does not create type `0x33`

`00CB5AD0` (`listing-00c80000`): name search on
`[manager+4]` / `00CB65D0`. Hit returns `rec+4`
(factory). Miss returns 0.

`004B4260`:

```
00CB5AD0
  hit  → [rec+4]=factory; 004BB720
  miss → [rec+4]=0;       004BB720
then once 004B3CE0
```

`004B4A10` (`00892E80` ActivateQuest, vtbl slot 276 /
`+1104`) only calls `004B4260`. No `004B1D30`. No
`push 0x33`. **PROVEN.**

So a hit for `Q_NewOakValeIntro` would:

1. bind factory `00DBEF70` / script `S_QNOVI`
   (`00CD6E27` already filled the table);
2. `004B3CE0` live arm: 52-byte `004B0310` on `QM+56`;
3. `00CB7900` start `00DABAC0`;
4. `00687540(55, 50)` kind **`0x37`** with that name’s
   `004AF740` index.

`00893570` still needs kind **`0x33`**. Construct does
not write it. **DISPROVEN** as the wait’s object.

`004AF610` becoming true (name on `QM+56`) is a
**different** predicate (`vtbl+1136` `00892F40`).
Do not treat slot presence as a type-`0x33` node.
**DISPROVEN** equivalence.

---

## 7. 52-byte `QM+56` slot vs event payload

| | Event kind `0x33` | Quest slot (`004B0310` / stub) |
|---|---|---|
| Alloc | `00BFEA0E(96)` node | `00BFEA1A(52)` |
| Vtbl | `0125BE8C` at payload+0 | none (`[+0]` = id) |
| Type | `[+8] = 0x33` | `[+8] = run` or 0 |
| Name | via `[+60]` → `QM+44` | CString at `+48` |
| List | `[world+96]` | `QM+56` 16-byte nodes |
| Writer at construct | **no** (kind `0x37` instead) | **yes** |

**DISPROVEN** as the same type.

---

## What this is not

| Claim | Class |
|---|---|
| Type `0x33` is the 52-byte `QM+56` slot | **DISPROVEN** |
| `004B3CE0` inserts type `0x33` | **DISPROVEN** (inserts `0x37` or nothing) |
| `00CB5AD0` hit creates the wait’s object | **DISPROVEN** |
| `ActivateQuest` / `004B4A10` posts `0x33` | **DISPROVEN** (only `004B4260`) |
| `[iface+4]+96` is a different list than `[QM+124]+96` | **DISPROVEN** (both world+96) |
| `QM+44` membership is enough for `00893570` | **DISPROVEN** (`gameflow-state0-wait`) |
| First-seen Gameflow posts `0x33` for Oakvale | **DISPROVEN** miss |
| `005E7B77` is the no-save first giver | **UNREAD** as Oakvale; not construct |

---

## Classifications (short)

1. **Type `0x33` — PROVEN event kind 51 on
   `[world+96]`.** `00687540` / `00687310`, vtbl
   `0125BE8C`. `[+8]` type, `[+52]` WorldFrame,
   `[+60]` `QM+44` index. `008ABED0` + `004AF3C0`
   name compare. **Not** the 52-byte `QM+56` slot.
2. **Insert at `004B3CE0` — DISPROVEN for `0x33`.**
   Live arm posts **`0x37` (55)**; stub posts
   nothing. Slot goes on `QM+56`.
3. **Insert site for `0x33` — PROVEN `004B1D30`
   (`00892F80` vtbl+1152) → `00687540(0x33)`.**
   Second writer `005E7B77`. Neither is
   `00CB5AD0` / `004B4A10`.
4. **`00CB5AD0` hit — DISPROVEN as creator of this
   object.** Hit constructs the factory slot and a
   kind-`0x37` event. Wait still needs kind `0x33`.
