# First reader of `[0x13B8A54]` after `004CDB46`

Investigation only. No production `src/` / `tests/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI` / `Father.Speak`.
After Leave this walk is `FinalAlbion.wld` →
`"Init Game"` → `00418DCA` → vtbl+4 `004184BD`
then first vtbl+8 `004189C2` / type-1 `004A5A40`.
Do **not** treat later `004CDC40` / `004CEA60` /
`00A01920` / `Speak` as this fill.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: after `004CDB46 call 00A39010` fills
`[0x13B8A54]`, who is the first reader of that
singleton on the Init Game / first `004189C2`
pump walk? Is it `004CDF91` / `00A38420`? Does
any first-seen site look up a symbol and queue
a spoken line?

Authority: Fable.exe dump listings as BSS
xrefs (`0x13B8A54` is **absent** from
`xrefs.tsv`); `listing-004c0000.txt`
(`004CDB41`, `004CDF91`–`004CE035`,
`004CE3A3`–`004CE402`, `004CE960`,
`004CDC40`, `004CE1B0`, `004CE550`,
`004CEA60`); `listing-00a00000.txt`
(`00A38420`–`00A38487`, `00A39010`–
`00A39187`, `00A38500`–`00A3854B`,
`00A01920`–`00A0193F`, `00A01A0C`–
`00A01A4F`); `listing-01200000.txt`
(`0121A630`, `01228DD0`);
`listing-00400000.txt` (`004184BD`,
`004189C2`); `listing-00480000.txt`
(`004A5A40`); `listing-00700000.txt`
(`00717AA0` / `00717C50` / `00717D40`);
`listing-00900000.txt` (`009046A0` /
`00904754`);
`e8.tsv` dests `00A39010` / `00A38420` /
`004CDC40` / `004CE550` / `004CEA60` /
`004CEBB0`;
siblings `proofs/004CDB10-00A39010`,
`proofs/dialogue-first`,
`proofs/audio-initgame-first`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| First reader on Init Game `004184BD` after `004CDB46`? | **None.** `004CDB10` only drops temps and `ret`. No later named child loads `ecx=0x13B8A54`. | **PROVEN** empty |
| First reader on first `004189C2` / `004A5A40` pump? | **None.** Direct `E8` lists and the only `00A38420` sites miss this tree. | **PROVEN** empty |
| Is that first reader `004CDF91` / `00A38420`? | **No on this walk.** It **is** the first leftover *site* (lowest VA after the fill). | **DISPROVEN** as this walk. **PROVEN** as first leftover site |
| What does that site read? | `00A38420` hashes the CString and walks **`this+4` / `this+8`**, not the `+20` list. | **PROVEN** |
| `+20` list involved? | **Yes on the fill only.** `00A39010` clears / refills `[this+20]` then `00A38E50` `"enum"` parse. Lookup does not walk `+20`. | **PROVEN** fill; **DISPROVEN** as the lookup |
| First-seen site looks up a symbol? | **Yes.** `00A38420` returns `al` + dword id into the out-arg. | **PROVEN** |
| First-seen site queues a spoken line? | **No.** No `Speak` / `vtbl+52` / `vtbl+1096`. `004CEA60` may later `00A01920` a `"SND_"` **id**. That is not a play. | **DISPROVEN** as speech |

**Answer:** first reader VA on the asked walk
is **none**. First leftover lookup is
`004CDF91 call 00A38420` (`ecx=0x13B8A54`).
It reads the **`+4` hash**, not the `+20`
list. It does **not** queue a spoken line.
Do not start Oakvale / Speak as New Game.

---

## Direct answers

| Item | Value |
|---|---|
| First reader VA (this walk) | **none** |
| First leftover reader VA | `004CDF91` → `00A38420` |
| What it reads | `[0x13B8A54+4]` / `[+8]` hash (`009B21F0`); out dword = `[node+4]` |
| `+20` list? | fill only (`00A39010` `lea esi,[ebx+20]`) |
| Speech queued? | **no** |

Overall: **PROVEN**.

---

## 1. Complete `0x13B8A54` immediates

`xrefs.tsv` has **zero** rows for
`0x013B8A54`. Dump xrefs are the listing
`mov ecx, 0x13B8A54` sites (all
`text-map` listings):

| VA | Role | When |
|---|---|---|
| `0121A630` | static ctor `00A38500` | pre-Init Game |
| `004CDB41` | fill `00A39010` | Init Subtitled Message |
| `004CDF91` | **first** `00A38420` | inside `004CDC40` |
| `004CDFB1` / `004CDFD8` / `004CE000` / `004CE035` | more `00A38420` | same `004CDC40` |
| `004CE3A3` / `004CE3C7` / `004CE402` | `00A38420` | inside `004CE1B0` |
| `004CE960` | `00A38420` | inside `004CE550` |
| `01228DD0` | atexit `jmp 004D1D50` | process exit |

No other listing hits. **PROVEN** closed
set.

`e8.tsv` dest `00A39010`: `004CDB46`,
`00A01A4F`. Second site is sound
`mov ecx,[esi]` after a **heap**
`00A38500` (`00A01A0C`). **DISPROVEN** as
this singleton.

`e8.tsv` dest `00A38420` (10):

| Site | `ecx` |
|---|---|
| `004CDF96` … `004CE03A` | `0x13B8A54` (`004CDC40`) |
| `004CE3A8` … `004CE407` | `0x13B8A54` (`004CE1B0`) |
| `004CE969` | `0x13B8A54` (`004CE550`) |
| `00A01936` | `[bank+4]` (`00A01920`) — **not** this BSS |

Every singleton lookup is `00A38420` with
that immediate. **PROVEN**.

---

## 2. Fill vs `+20` vs `+4`

`00A38500` (`0121A630`) already constructed
the BSS object:

```
[this+0]  vtbl 0x129CF84
[this+4]  hash header (zero)
[this+8]  hash sentinel
[this+20] list node (alloc 20; [+8]/[+12]=self)
[this+24] list count 0
```

`004CDB46` `00A39010` (`ecx=0x13B8A54`,
`ret 8`):

```
lea ebp, [ebx+4]
call 00A39900              ; lock +4
cmp [ebx+24], 0
jne 004CF810 on [ebx+20]   ; clear +20 list
… path / token rewrite …
call 00A38E50(this, buf)   ; "enum" parse
```

`00A385A0` (`lea ecx,[esi+20]`) is only
`E8` from `00A38905` / `00A38946` /
`00A38A0A` — nested in that parse.
**PROVEN** `+20` is the fill list.
**DISPROVEN** as a post-fill walk on
this tree.

`00A38420` (`ret 8`):

```
hash CString via 004014A0
lea esi, [ebx+4]
call 009B21F0
cmp eax, [ebx+8]   ; miss → al=0
mov edx, [eax+4]
mov [out], edx     ; hit → al=1
```

**PROVEN** first leftover reader is the
**`+4` map**, populated by the fill, not
a `+20` walk.

After `004CDB46` the owner only
`0099B510` ×2 / `0099EAE0` and `ret`.
No second load of the singleton.
**PROVEN**.

---

## 3. `004CDF91` is not on this walk

Funnel (all singleton `00A38420`):

```
004CEA60
  004CEABB call 004CE550
    ANIM:     no 00A38420
    CAM_SHOT: 004CE79A 004CE1B0 → 004CE3A3
    CAM:      004CE826 004CDC40 → 004CDF91   ← first VA
    else:     004CE960 00A38420
    004CE914 004CE4E0 → 004CE513 004CDC40
  then 00A01920 / "SND_" id (not play)
```

`e8.tsv` dest `004CE550`: **only**
`004CEABB`. Dest `004CEA60`:
`004CEC88`, `004CECF4` (`004CEBB0`),
`00717CC4`. Dest `004CEBB0`:
`00717B18`, `00717C04`, `00717DC3`.

Those live in `00717AA0` / `00717C50` /
`00717D40` (`vtbl 0x12659C8`). Callers
are `00904754` / `00904834` / `00904919`
(`009046A0` family, `00A01B50` +
`vtbl+300`, alloc `0xA0`). **No** `.text`
`E8` of `009046A0`.

`functions.tsv` / `e8.tsv`:

| Parent | `004CEA60` / `00A38420` / `004CDC40`? |
|---|---|
| `004184BD` Init Game | no (`004CDB10` only) |
| `00416953` Loading world | no |
| `004189C2` pump | no |
| `00417747` inner tick | no |
| `004A5A40` type-1 | no (site `00629270` / world tick) |

`audio-initgame-first`: `004CEA60` is
**DISPROVEN** first-seen. `dialogue-first`:
first-seen after Leave is **no spoken
line**; `006E60F0` / `006E37D0` empty;
first text is journal
`00CBE87F TEXT_QST_LOG_STORY_10`.

`004CDF91` is the first **leftover**
lookup site. It is **not** the first
reader on Init Game / first pump.
**DISPROVEN** as this walk.

Which `004CE550` arm would fire first
*if* a later caller reached it is
**UNREAD** (depends on the first def
prefix). Lowest VA is still
`004CDF91`.

---

## 4. Symbol lookup, not speech

`004CDF91` (and the sibling
`00A38420` sites) resolve enum **ids**
into a subtitle / cam record
(`004D18E0` on hit). `004CEA60` may
then `00A01920` a `"SND_"` **bank
symbol**. `00A01920` is a 31-byte
lookup (`audio-initgame-first` §1).
Play is later `[0x13B8394].vtbl+36`
via `0041CEB3`, which has **zero**
`E8` and is not on this tree.

| Candidate | On first leftover site? | On this walk? |
|---|---|---|
| `00A38420` symbol id | yes | no |
| Script `Speak` `00CC25FD` / `vtbl+52` | no | no |
| Guild `vtbl+1096` | no | no |
| `00A01920` play | id only, later | no |
| `004CDB70` `"UNKNOWN"` ctor | miss path of `004CEA60` | no |

**PROVEN** lookup. **DISPROVEN** queued
line. Same as `dialogue-first`.

---

## 5. Host leftover

Host already Notes `"Init Subtitled
Message"` and (sibling
`004CDB10-host-register`) may run the
`00A39010` fill. It must **not** invent
a first reader, a `+20` walk, or a
spoken line on Init Game / first pump.

Adding `004CDF91` / `004CEA60` /
`Speak` as New Game work would
**DIVERGE**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004CDB46` `00A39010` | fill `[0x13B8A54]` | **PROVEN** |
| `[this+20]` | fill list | **PROVEN** on fill |
| `[this+4]` / `[+8]` | lookup map | **PROVEN** |
| Init Game / first `004189C2` reader | none | **PROVEN** empty |
| `004CDF91` `00A38420` | first leftover reader | **PROVEN** site; **DISPROVEN** this walk |
| `004CDC40` / `004CE1B0` / `004CE550` | only consumers | **PROVEN** funnel |
| `004CEA60` | owner of that funnel | **DISPROVEN** first-seen |
| Spoken line from first-seen site | — | **DISPROVEN** |
| `00A01A4F` | other `00A38500` heap | **DISPROVEN** as this BSS |
| `01228DD0` | atexit | **DISPROVEN** first-seen |
| Oakvale / `00DBDE40` / Speak | — | **DISPROVEN** |
