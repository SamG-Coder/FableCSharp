# First reader of `004CD670` tables after bind

Investigation only. No production `src/` / `tests/`
edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI` / `Father.Speak`.
After Leave this walk is `FinalAlbion.wld` →
`"Init Game"` → `00418DCA` → vtbl+4 `004184BD`
then first vtbl+8 `004189C2` (dummy prefix).
Do **not** treat later `00842400` / `008D9220`
/ `00716DE0` `CActionTalkToThing` / `Speak` as
this bind.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**DIVERGE** / **MATCH**.

Question: after Init Conversation Attitude
`004CD670` binds `STANDARD_TALK_*` /
`CONVERSATION_*` via `0099EFE0` into
`[0x13B8A2C]` / `[0x13B8A38]` / `[0x13B8A44]`
and sets `[0x13B8A28]=1`, who first **READS**
those tables on the `004184BD` walk or first
`004189C2` pump? Getters `004CD9A0` /
`004CD9B0` / `004CD9C0` return
`table+index*4`. First-seen caller?

Authority: Fable.exe dump
`listing-004c0000.txt` (`004CD670`–
`004CD999`, `004CD9A0`–`004CD9C8`);
`listing-00400000.txt` (`004184BD`
`00418692` / `004189C2`);
`listing-00840000.txt` (`00842400`–
`0084244E`);
`listing-008c0000.txt` (`008D9220`–
`008D93B2`);
`listing-00700000.txt` (`007150ED` /
`0071721C` / `00717894` / `0073A350`);
`listing-01200000.txt` (`0121A460` /
`0121A4F0` / `0121A580`, dtors
`01228D00` / `01228D40` / `01228D80`);
`e8.tsv` dests `004CD9A0` / `004CD9B0` /
`004CD9C0` / `00842400` / `008D9220` /
`008D92C0`;
`functions.tsv` `004184BD` / `004189C2` /
`00416953` / `0041735A`;
`xrefs.tsv` `STANDARD_TALK_*` /
`CONVERSATION_*` (`fn=0x004CD670` only);
siblings `proofs/004CD670-attitude`,
`proofs/004CD670-host-bind`,
`proofs/004CDB10-00A39010`,
`proofs/13B8A54-first-reader`,
`proofs/dialogue-first`,
`proofs/creature-after-leave`,
`proofs/dummy-pumps-before-region`.

`proofs/004CD670-attitude` is present.
It stops at the bind and leaves the
first-seen *reader* **UNREAD**. This
note answers that remainder.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| First reader on `004184BD` after `004CD670`? | **None.** Bind writes; next named child is `"Init Player Manager"` `0041732A`. No getter `E8`. | **PROVEN** empty |
| First reader on first `004189C2` pump? | **None.** Dummy `004FC180` / `0040D2A0` / `00B239A0`. No getter `E8`. | **PROVEN** empty |
| Getter `004CD9A0` (`[0x13B8A2C]`) first-seen caller? | **None ever.** `e8.tsv` dest `004CD9A0`: **0**. | **PROVEN** unused as `E8` |
| Getter `004CD9B0` first leftover site? | `00842419` in ctor `00842400`. Index = stack arg1 (runtime). | **PROVEN** site; **DISPROVEN** this walk |
| Getter `004CD9C0` first leftover site? | `008D924A` in ctor `008D9220`. `ecx=1`. | **PROVEN** site; **DISPROVEN** this walk |
| Speech queued from the bind or first leftover getter? | **No.** Bind is `0099EFE0` assign. Leftover sites copy a CString / set vtbl. No `Speak` / `vtbl+52` / `vtbl+1096`. | **DISPROVEN** |

**Answer:** first reader VA on the asked
walk is **none**. Index **n/a**. Speech
**not** queued. First leftover *sites*
are `00842419` (`004CD9B0`) and
`008D924A` (`004CD9C0`, index **1**).
Those live in later creature talk /
mode ctors (`00716DE0`
`CActionTalkToThing`, factory
`0073A350`). Do not start Oakvale.

Overall: **PROVEN** empty on this walk.

---

## Direct answers

| Item | Value |
|---|---|
| First reader VA (this walk) | **none** |
| Index | **n/a** |
| Speech queued? | **no** |
| First leftover `004CD9A0` | **none** (`E8` dest empty) |
| First leftover `004CD9B0` | `00842419` / `00842400`; index = arg1 |
| First leftover `004CD9C0` | `008D924A` / `008D9220`; index **1** (`CONVERSATION_HAPPY` slot) |

---

## 1. Bind vs getters

`listing-004c0000.txt`:

```
004CD670  mov al, [0x13B8A28]
004CD677  jne 004CD999          ; already filled
… 0099EFE0 ×42 into the three bases …
004CD992  mov [0x13B8A28], 0x01
004CD999  ret

004CD9A0  mov eax, [0x13B8A2C]
004CD9A5  lea eax, [eax+ecx*4]
004CD9A8  ret

004CD9B0  mov eax, [0x13B8A38]
004CD9B5  lea eax, [eax+ecx*4]
004CD9B8  ret

004CD9C0  mov eax, [0x13B8A44]
004CD9C5  lea eax, [eax+ecx*4]
004CD9C8  ret
```

`004CD670` **reads** the three base
pointers only to `0099EFE0` **write**
each slot. That is the bind
(`004CD670-attitude`). It is **not** a
post-bind consumer. Zero `E8` from
`004CD670` to `004CD9A0` / `004CD9B0` /
`004CD9C0`. **PROVEN**.

`e8.tsv` dests:

| Dest | Sites |
|---|---|
| `004CD9A0` | **none** |
| `004CD9B0` | `00842419` |
| `004CD9C0` | `008D924A`, `008D92A1`, `008D92E1`, `008D9335`, `008D9397` |

`xrefs.tsv` `STANDARD_TALK_*` /
`CONVERSATION_*` are `fn=0x004CD670`
only. **PROVEN** closed `E8` set.

Listing hits of the three BSS bases
outside the bind / getters:

| VA | Role |
|---|---|
| `0121A460` / `0121A4F0` / `0121A580` | CRT empty fill (pre-Init) |
| `01228D00` / `01228D40` / `01228D80` | atexit dtor |

No other `listing-*.txt` load of
`0x13B8A2C` / `0x13B8A38` / `0x13B8A44`.
Post-bind **read** of a bound slot is
only via the getters (then the caller
uses `eax`). **PROVEN**.

---

## 2. Not on `004184BD` after the bind

`00418692 call 004CD670`. Next named
sibling is `"Init Player Manager"`
`0041732A` (`004186E4`).

`functions.tsv` `004184BD` after
`004CD670`: `0041732A`, `004473A0`,
`004193C4`, `0049E740`, `0041735A`,
`00417418`, `004166A8`, `00417A58`,
`004174F1`, `0049BA70`, `00416392`,
`004AE9D0`, `009EC890`. **No**
`004CD9A0` / `004CD9B0` / `004CD9C0` /
`00842400` / `008D9220`.

`00416953` (same walk, vtbl+32):
`004A1840`, `0049F180`, `004B4A10`,
`004BBC00`. **No** getter dests.

`creature-after-leave`: no-save
`0049F180` is bind + failed
`00489D40`. First *creature Thing*
is later Lookout `0051FD80`, **not**
this Init Game suffix. **PROVEN** no
creature ctor that could reach
`00842400` / `008D9220` here.

`dialogue-first`: Init World
`006E6150` Script Conversation
Manager is `[+8]=self` empty.
`006E60F0` walks nothing.
**PROVEN**.

`004CD670` itself has no second
reader. Flag write is this fn only;
re-entry on this walk is unused
(sole `E8` is `00418692`). **PROVEN**.

---

## 3. Not on first `004189C2`

`functions.tsv` `004189C2`:
`004AE9C0`, `004FB150`, `004FC180`,
`0040D2A0`, `0040BC80`, `009F2660`,
`009F26B0`, `0098E1B0`, `009A6460`,
`009F8BA0`, `004162B5`, … **No**
getter dests.

First iteration (`dummy-pumps-before-region`
/ PARITY): dummy `004FC180` index 0
→ `0040D2A0` / `0040CEC0` →
`0040BC80` → vtbl+220 `00B239A0(12,
20.0)` → `009F2660`. Not a region.
First inner does **not** `00CB8220`.
**PROVEN**.

Type-1 `004A5A40` / `00CB8220` is a
**later** dummy inner, not the first
`004189C2`. Still no getter on that
tree (`dialogue-first` empty
conversation / speech-gain).
**DISPROVEN** as first pump.

---

## 4. Leftover getter sites (not this walk)

### `004CD9B0` — `00842419`

```
00842400  … ret 20 …
00842412  mov ecx, [esp+24]   ; after pushes = arg1
00842416  push edx
00842417  push 1              ; arg to 00662FA0, not the index
00842419  call 004CD9B0       ; ecx = index = arg1
00842422  push eax            ; slot*
00842425  call 00662FA0       ; copy CString
00842438  call 00693B30
0084243E  mov [esi], 0x12737CC
```

`e8.tsv` dest `00842400`: `007150ED`,
`0071721C`.

`007150ED` (end of a `add esp,0xB8`
anim/AI body): `push 0; push 0; … push
[ebx+24]; push creature; call
00842400`. Index = `[obj+24]`.
**PARTIAL** (runtime field).

`0071721C` is inside `00716DE0`
(`functions.tsv` strings
`CActionTalkToThing` /
`CActionTalkedTo` / `CONV_GENERAL`).
Index = `[table+edx*8+4]`. **PARTIAL**.

`push 1` at `00842417` is **not** the
getter index. **PROVEN**.

### `004CD9C0` — `008D9220` / `008D92C0` / `008D9350`

```
008D9220  …
008D922E  mov ecx, 0x1
008D9233  mov [esi], 0x1285990
008D924A  call 004CD9C0       ; index 1
008D9252  call 0073E8F0       ; store into [this+36]
```

`[0x13B8A44]+1*4` is the bind slot
`CONVERSATION_HAPPY` (`004CD670-attitude`
§4). **PROVEN** index; name is the
bound ASCII, **not** a spoken line.

`008D92A1` / `008D9335` / `008D9397`
re-get after `006E3180` when the
computed index changes. `008D92E1`
uses ctor arg as index.

`e8.tsv` dest `008D9220`: **only**
`0073A364`. That site is factory
`0073A350` (`push 52` `00BFEA1A` then
`008D9220`). **Zero** `.text` `E8` of
`0073A350` (vtbl / mode factory;
neighbors `0073A340` return `0x1B`).
**PROVEN** leftover factory; **DISPROVEN**
Init Game `E8`.

`e8.tsv` dest `008D92C0`: **only**
`00717894` in `00716DE0` (alloc 52,
index `[esi+104]` from `006E3180`).
`008D9350` has **zero** `E8` (instance
method).

`00716DE0` also lists later
`004CEA60` / `004CDB70` /
`004CEBB0`. Those are the talk
pipeline (`13B8A54-first-reader`),
**not** reached on this walk.

---

## 5. Speech is not queued

| Candidate | On bind? | On leftover getter? | On this walk? |
|---|---|---|---|
| `0099EFE0` CString assign | yes (write) | no | yes — bind only |
| `00662FA0` / `0073E8F0` copy slot | no | yes | no |
| Script `Speak` `00CC25FD` / `vtbl+52` | no | no | no |
| Guild `vtbl+1096` | no | no | no |
| `004CDC40` / `004CEA60` / `00A01920` | no | later `00716DE0` only | no |

`dialogue-first`: first-seen after
Leave is **no spoken line**. This
reader question does not change that.
**DISPROVEN** speech.

---

## 6. Host leftover

Host Notes `"Init Conversation
Attitude"` and (sibling
`004CD670-host-bind`) may run the
`0099EFE0` fill. It must **not** invent
a first reader, a getter call, or a
spoken line on Init Game / first
`004189C2`.

Adding `00842400` / `008D9220` /
`Speak` as New Game work would
**DIVERGE**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `00418692` `004CD670` | bind | **PROVEN** |
| `[0x13B8A28]=1` | once-flag | **PROVEN** |
| `[0x13B8A2C]` / `[0x13B8A38]` / `[0x13B8A44]` | CRT vectors | **PROVEN** |
| `004CD9A0` / `004CD9B0` / `004CD9C0` | `lea` getters | **PROVEN** shape |
| Init Game / first `004189C2` reader | none | **PROVEN** empty |
| `004CD9A0` `E8` | — | **DISPROVEN** (no site) |
| `00842419` `004CD9B0` | first leftover B0 | **PROVEN** site; **DISPROVEN** this walk |
| `008D924A` `004CD9C0` index 1 | first leftover C0 | **PROVEN** site; **DISPROVEN** this walk |
| `00716DE0` / `0073A350` | leftover parents | **DISPROVEN** first-seen |
| Spoken line | — | **DISPROVEN** |
| `01228D00` / `40` / `80` | atexit | **DISPROVEN** first-seen |
| Oakvale / `00DBDE40` / Speak | — | **DISPROVEN** |

---

## Remaining UNREAD

- Semantic names for slot indices
  (do not invent).
- Runtime value of `00842400` arg1 on
  the first *later* hit.
- Which leftover site (`00842419` vs
  `008D924A`) would run first *if* a
  creature talk / mode ctor ran.
- `008D9350` vtbl slot id.

This note stops at “no reader on
`004184BD` / first `004189C2`.”
