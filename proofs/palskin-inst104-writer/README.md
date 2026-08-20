# Who writes PALSKIN `[inst+104]+8`

Investigation only. Production `src/` was not edited.
No host change that invents type1 on kid **4300**.

Do **not** spawn `CREATURE_HERO_CHILD` on Lookout.
Do **not** collapse leftover **#4** (Lookout Present vs Oakvale
intro view). Kid **4300** is a `FirstSceneWorld` fixture, not
Pump.

Question: who writes PALSKIN **`[inst+104]+8`**? Type1 **`0x80`**
fills slots **10+14** only when that dword is **1**. First-seen
4300 is type0 (`0x100` then Flag1 `0x200`). Lookout walks `0x80`
empty.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: listings `00BD77FE` / `00BD780D` / `00BD2920` /
`00BD2B70` / `00BD6DF0` / `00B8FF00` / `00B324A0` / `00B991D0`
(`listing-00bc0000.txt`, `listing-00b80000.txt`,
`listing-00b00000.txt`);
`palskin-queue-slots-00bd7838-00bd780d.md`,
`palskin-draw-entry-00bd71b0-00bd71b0.md`,
`slot-dispatch-00b324a0-00b324a0.md`,
`vtbl.tsv` `012A7C04` / `012A3F4C` / `012A3D60` / `012A3D7C`;
siblings `proofs/palskin-type1-0x80-kid`,
`proofs/palskin-first-present-id`,
`proofs/palskin-type1-0x80-4300`,
`proofs/hero-palskin-first-submit`;
`docs/status/investigations/2026-08-18-palskin.md`;
`docs/status/README.md` leftover #4.

---

## Verdict

**Pointer writer is PROVEN. Type dword writer is UNREAD.**

`[inst+104]` is a **refcounted pointer**. PALSKIN instance
ctors **zero** it. `00BD2920` (vtbl+20 `00BD2B70`) **copies**
`[src+128]` onto it. The type dword is **`[that pointer]+8`**,
read at `00BD780D`. No listing store of **0** or **1** into
that `+8` is recovered. Do not treat ctor-zero of the *pointer*
as type0, and do not invent type1 on 4300 to fill `0x80`.

First-seen 4300 stays type0-shaped (`0x100` then Flag1 `0x200`).
Lookout first Present is Graphic **4299**; MainScene still
**walks** bit `0x80` with slot 14 empty. That is not a kid DIP.

| Claim | Class |
|---|---|
| `[inst+104]` is a pointer; `+8` of *that* is type 0/1 | **PROVEN** |
| PALSKIN ctor zeros `[inst+104]` | **PROVEN** |
| `00BD2920` copies `[src+128]` → `[inst+104]` | **PROVEN** |
| `00B324A0` `vtbl+20` is that copy (`00BD2B70`) | **PROVEN** |
| Writer of `[helper+8]` as 0 or 1 | **UNREAD** |
| Writer of `[src+128]` (thing-side packer) | **UNREAD** |
| `00B8FF00` `[this+8]` is that type dword | **DISPROVEN** (instance family id) |
| `00BCE740` / `00BD22DE` `[helper+8]` is that dword | **DISPROVEN** (queue opacity byte) |
| `push 104` types 5/21/22 are this helper | **DISPROVEN** |
| First-seen 4300 is type1 / `0x80` | **DISPROVEN** |
| Spawn CHILD on Lookout first Present | **DISPROVEN** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Who writes `[inst+104]` (the pointer)? | Ctor **0**. Then `00BD2920` from `[src+128]`. | **PROVEN** |
| Who writes `[inst+104]+8` (type 0/1)? | **Not dumped.** Helper ctor / packer field **UNREAD**. | **UNREAD** |
| Is type1 `0x80` first-seen 4300? | **No.** Type0 + Flag1 hair. | **MATCH** skip |
| Lookout `0x80`? | Registration **walks** it. Slot 14 empty. Graphic **4299**. | **PROVEN** walk. **DISPROVEN** as 4300 |
| 104-byte `00B8FF00` factory? | Family types **5 / 21 / 22**. Not PALSKIN 9/11/13. | **DISPROVEN** as this field |

---

## 1. The read — type is `[[inst+104]+8]`

`00BD71B0` (`edi` = instance, `[esp+680]`):

```
00BD77FE  mov eax, [edi+104]
          test eax, eax
          je  00BD7958              ; NULL pointer → no queue at all
00BD780D  mov eax, [eax+8]          ; type dword
          sub eax, 0
          je  00BD789C              ; type 0 → slot 8 (+ Flag1 slot 9)
          dec eax
          jne 00BD7958              ; not 0/1 → skip
; type 1:
          00B84720(10)              ; slot 10 → bit 0x100
          00B84720(14)              ; slot 14 → bit 0x80 after sky
```

Helper layout implied by the copy (`00BD2920`):

| Off | Field |
|---|---|
| +0 | vtbl (`call [edx+4]` dtor) |
| +4 | refcount (`inc` / `dec`) |
| +8 | **type dword** 0 or 1 |

NULL `[inst+104]` skips **both** type0 and type1. First-seen
PALSKIN on `0x100` therefore requires a **non-null** pointer.
Type1 `0x80` still needs `[ptr+8]==1`.

---

## 2. PALSKIN ctor zeros the pointer — PROVEN

Factory `00BD6DF0` (renderer vtbl+4, types **9 / 11 / 13**):

```
00BD6DF0  sub eax, 9
          je  00BD7013              ; type 9,  alloc 0x128
          sub eax, 2
          je  00BD6F15              ; type 11, alloc 0x128
          sub eax, 2
          jne 00BD7104
; type 13:
          push 0x128
          00BFEA1A
          00B8FF00(13, 8)           ; INSTANCE+8 = 13 (family id)
          mov [esi+104], ebx        ; ebx=0  ZERO the helper pointer
          vtbl 012A79B4 then 012A7C04
```

Type 9 (`00BD7013`) and type 11 (`00BD6F2A`) do the same
`mov [esi+104], ebx`. 2D siblings 37/38 (`00BD6B90`, size
`0x188`) also zero `+104`.

Base ctor `00B8FF00` (vtbl `012A3D7C`):

```
00B8FF09  mov [eax+8], ecx          ; arg0 = family type (9/11/13)
00B8FF11  mov [eax+4], 1            ; instance refcount
00B8FF18  mov [eax], 0x12A3D7C
00B8FF3A  mov [eax+39], 0xFF        ; opacity
; no store at +104
```

**DISPROVEN:** `00B8FF00` `[this+8]` is `[inst+104]+8`. That
`+8` is the **instance family id** (9/11/13), compared at
`00B324D1` `cmp [esi+8], ebp`. Queue type is a **different
object**.

Opacity copy `00B991D0` / `00B991F5` writes `+39` / `+80` /
`+128` (mesh) / `+152`… It does **not** write `+104`.

Static-like factory `00B9AB30` (size `0xB0`, vtbl `012A3F4C`)
does not store `+104`. Its vtbl+20 is `00B8FD80` **`ret 8`**
(no-op). That is **not** the PALSKIN instance class.

---

## 3. Pointer install — `00BD2920` from `[src+128]`

```
00BD2A35  mov ecx, [esi+104]
          cmp ecx, [edi+128]
          je  already
          ; release old: dec [ecx+4]; dtor vtbl+4
00BD2A4E  mov [esi+104], 0
00BD2A51  mov eax, [edi+128]
00BD2A59  mov [esi+104], eax        ; COPY pointer
          je  null
00BD2A5E  inc [eax+4]
```

`esi` = instance (`ecx` of `00BD2920`).
`edi` = source record (`arg0`): type at `[src+0]`, helper at
`[src+128]`, opacity byte at `[src+24]` → `inst+39`.

Caller `00BD2B70` (vtbl `012A7C04` **slot 5** / `+20`):

```
00BD2B70  this.00BD2920(arg0, arg1)
          00BECDE0(this+0x128, src+156)
          00BECBA0(this+?, src+144)
          00BEBE90(this+?, src+0xA8)
```

Slot dispatch `00B324A0`:

```
ebp = [arg1]                      ; family type
factory = [0x1436E84 + type*4 + 16]
if existing && [inst+8]==type
    call [inst.vtbl+20]           ; 00BD2B70
else
    call [factory.vtbl+4]         ; 00BD6DF0  (zeros +104)
    call [new.vtbl+20]            ; 00BD2B70  (copy src+128)
```

Engine vtbl+92 `00B23BC0` is one stdcall into `00B324A0`.
`00B32E90` is the other, **hard-coded type 4** (not PALSKIN).

Sibling copier `00B9D8E0` (another family) copies **`[src+60]`**
→ `[inst+104]`. Same refcount shape. **Not** PALSKIN 9/11/13.

---

## 4. Type dword `[helper+8]` — UNREAD

No recovered store of integer **0** or **1** into
`[ptr+8]` where `ptr` is `[inst+104]`.

Tried and **DISPROVEN** as that dword:

| Site | What it writes | Why not |
|---|---|---|
| `00B8FF00` `[eax+8]` | family **9/11/13** | Instance field, `00B324D1` |
| `00BCE740` ctor | no `+8` | Queue helper 0x24 / vtbl `012A6C5C` |
| `00BD22DE` `mov [esi+8], dl` | opacity **byte** from `inst+39` | Same queue helper, then `00B84720` |
| `00B8FFA0` `[eax+8]=arg` | small object vtbl `012A3D60` | **Zero** `E8` callers |
| `push 104` `00BC14A6` / `00BC4D56` / `00BC68F7` | `00B8FF00` types **22 / 21 / 5** | Size-104 *instances*, not PALSKIN 9/11/13 (`0x128`) |

Thing-side packer that allocates a type `0x9`/`0xB`/`0xD`
**record** (the `src` with `+128`) and calls `00B23BC0` /
`00B324A0` stays **UNREAD**. It is not `0041BEB0` (frontend
type `0x22`). That packer is the natural writer of
`[src+128]` and, if the helper is constructed there, of
`[helper+8]`.

Ctor-zero of **the pointer** is not a type0 dword. If
`00BD2B70` never runs, `00BD77FE` skips the whole queue.

---

## 5. First-seen 4300 type0; Lookout `0x80` empty

Do **not** invent `[helper+8]==1` on 4300.

Kid **4300** file is type0-shaped: body Flag1=0 → slot **8** /
`0x100`; `Young Hero Hair` Flag1=1 → extra slot **9** /
`0x200` after sky. Type1 would **drop** slot 9 and land hair
on `0x80`. First-seen MATCH cannot be both
(`palskin-type1-0x80-kid`).

Lookout first Present PALSKIN Graphic is **4299**
(`palskin-first-present-id`). Registration still **walks**
bit `0x80` (index 25). Slot 14 empty = type is not 1, or
helper NULL for type1. Empty drain ≠ kid DIP. Do not spawn
`CREATURE_HERO_CHILD` to explain that walk.

Leftover **#4** stays **open**: Lookout Present (4299) vs
Oakvale intro view (fixture 4300). This writer hunt does
**not** collapse those ledgers.

---

## Host vs native

| Host | Native | Class |
|---|---|---|
| No `[inst+104]` object | ctor 0, then `00BD2920` | **UNREAD** as a live dword |
| `DrawnPasses` never `0x80` | type1 only if `[ptr+8]==1` | **MATCH** skip on 4300 |
| Pump PALSKIN **4299** | Lookout adult | **MATCH** Graphic. **DISPROVEN** as 4300 |
| FirstSceneWorld 4300 type0+Flag1 | same shape | **MATCH** skip of `0x80`. **LEFTOVER** as Present |
| Leftover #4 two ledgers | Lookout vs Oakvale view | **MATCH** open |

---

## Do not

- Invent type1 / slot 14 / `0x80` geometry on kid **4300**.
- Spawn `CREATURE_HERO_CHILD` on Lookout.
- Collapse leftover **#4**.
- Call `00B8FF00` `[inst+8]` the queue type.
- Call `00BD22DE` opacity byte the queue type.
- Call size-104 type-5/21/22 factories this helper.
- Fold type1 into `DrawnPasses` without a recovered
  `[helper+8]==1`.
- Treat ctor-zero of the pointer as a proven type0 dword.

Next leftover is still the **thing-side packer** that builds
the type `0x9`/`0xB`/`0xD` record (`[src+128]` / `[helper+8]`),
not a Pump CHILD create and not a Duration default.
