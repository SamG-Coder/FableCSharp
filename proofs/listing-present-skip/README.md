# Leftover slots still get `vtbl+8`: what actually stops DIP?

Investigation only. No production `src/` edits.

Question: after `00596763` switches current slot,
`00595222` still `vtbl+8` every `[ui+84]` `node+20`.
How do leftover slot widgets stop presenting (Press
Start on New Profile, New Profile on Main Menu)?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`0052C730` / `0052C7E0` / `0052C780` / `0052C870` /
`0052C8B0` / `0052CAF0` / `0052CF40`–`0052D362` /
`0052F180` / `0052F1A0` / `0052F1D0` / `0052F900`–
`0052FFBF` / `00530260`–`005303E0` / `00531EC0`
`00532200` `0053258F` / `005334A0` `0053358B` /
`005339B0` `00533A50` / `00533B30`);
`listing-00540000.txt`
(`00547360` / `00547380` / `00547500` / `00547600` /
`00547C90` / `00549B20` / `00549F60` / `0054EF00`);
`listing-00580000.txt` (`00595222` / `00596763`);
`listing-00400000.txt` (`0041AFA0` / `0041BEB0` /
`0041B800`);
`listing-009c0000.txt` (`009DA9F0`);
`functions.tsv` `0x00531EC0` / `0x00596763`;
`proofs/00595222-visible-skip/README.md`;
`proofs/0052CF40-selectstate-6/README.md`;
`proofs/0052CF40-vtbl188-forward/README.md`;
`proofs/00596763-switch/README.md`;
`proofs/type12-highlight-plus348/README.md`;
`implementer/frontend/14-container.md`.

Do not re-prove: `00595222` walks all slots and skips
only null `node+20`; `SelectState(6)` does not write
`+302` / host `Visible`; `00530260` skip is `vtbl+400`
=`[+300]` bit 7 and `vtbl+420`=`[+302]` bit 0;
`vtbl+188` `0041C5A0` stores `+320` then child
`vtbl+192`; attach `SelectState(5)` is not type-18
ActiveChild 5; type-6 leftover `+204` is not dest
width. Do not invent clip CRC. Do not invent DIP /
dest pixels.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| `0052CF40` after `+332=arg` writes `+144..+151` / `+368` / `+340`? | **No.** Stores `+332`, zeros `+312/+308`, rotates `+324/+344`, optional `+328=arg` when `vtbl+176` true, `+336` duration, child `vtbl+188`. | **PROVEN** |
| Who writes colour `+144..+151`? | Ctor zeros; `005339B0` `+144..+147=0xFF`; tick `0052C7E0` style bit `0x10` → `+144..+147=0xFF`; pack `0052F900`/`0052FE4A` → dword `+148` (`+148..+151`); parent `00531EC0` pushes `&this+148` into child `vtbl+88`. | **PROVEN** |
| Who writes `+340`? | `0052C730` `=1`. Tick `0052C7E0` `vtbl+196` → byte `+340`. | **PROVEN** |
| Who writes widget `+368`? | Type-0 ctor `0041B800` `=0`. Leaf present `0041AFA0` `=0` on nonzero dest, `=1` on the alpha-0 / dest-zero arm. Not `0052CF40`. | **PROVEN** type 0; type 6 uses `+394` |
| `0052C7E0` tick: `vtbl+196` → `+340`; style `+328` via `vtbl+540`; bits `0x10/0x20/0x40`? | **Yes**, in that order, then `00531EC0`. If `+340==0`, `vtbl+544`. | **PROVEN** |
| `00531EC0` parent `+200`? | Own `+176`: if `vtbl+208==0`, `vtbl+204(this)` then `cmp this,[child+200]`. | **PROVEN** `00532200` |
| `+332==6` skips `vtbl+8` / `0041BEB0` / `009DA9F0`? | **No such compare.** `0052CF40` `cmp ebp,6` is the select jump table only. `00530260` / `0041AFA0` / `0054EF00` / `0041BEB0` / `009DA9F0` never load `+332`. | **DISPROVEN** as present skip |
| Type 16 `+348` vs type 18 `+332` as present index? | **Neither.** Type 16 `+348` is a ctor-zeroed vector (`00549B62`); no own `ret 20` `vtbl+8`. Type 18 `+348` is the States `{id,dur}` vector; `+332` is the shared style key. Both inherit `00530260`. | **DISPROVEN** as slot hide |

---

## Verdict

**Leftover slots do not drop out of `00595222`. They stop
enqueuing a DIP inside the leaf, not at the slot walk.**

`00596763` `vtbl+192(6)` is `0052CF40` on the old
type-10 root. That writes style key `+332=6` and
forwards `vtbl+188` to own `+176`. The next tick
`0052C7E0` (via `00531EC0` child `vtbl+4`) looks up
**style `+328`** with `vtbl+540` and applies dword-0
bits `0x10` (force `+144..+147=0xFF`), `0x20` (zero
`+76/+80`), `0x40` (scale `+116/+120=1.0`). Packed
present colour is `+148..+151`. Type-0 `0041AFA0`
tests **`+151`**: alpha 0 (or dest-zero) takes the
`+368==1` early `ret 20` **before** `0041BEB0`.
`009DA9F0` only consumes records already filled.

There is **no** `cmp +332, 6` that skips `vtbl+8` or
the DIP helpers. Type 18 does **not** exclusive-draw
child `[+332]`. Type 16 `+348` is not a present index.

Whether persist **style 6** actually has alpha 0 / bit
`0x20` on Press Start / New Profile is **UNREAD**
(`vtbl+540` dest dword is past this listing set; no
style-6 field dump here). That is the remaining hole
for *why* leftover `+151` becomes 0.

| Claim | Status |
| --- | --- |
| After switch, `00595222` still `vtbl+8` leftover slots | **PROVEN** (do not re-prove) |
| `0052CF40` `mov [this+332], arg` | **PROVEN** |
| `0052CF40` writes `+144..+151` / `+340` / `+368` | **DISPROVEN** |
| Animated arm `vtbl+176` true → `+328=arg` | **PROVEN** `0052D0CA` |
| Tick `0052C7E0` `vtbl+196` → `+340` then `vtbl+540(+328)` | **PROVEN** |
| Style `test [style], 0x10/0x20/0x40` as above | **PROVEN** |
| `vtbl+540` / `vtbl+196` / `vtbl+544` rdata dests | **UNREAD** |
| `0052F900` is `vtbl+544` | **PARTIAL** (colour pack to `+148`; not bound) |
| `0052C780` is `vtbl+196` | **PARTIAL** (fade-complete vs `+152..+172`) |
| `00531EC0` sets parent `+200` | **PROVEN** |
| `00531EC0` pushes parent `+148` → child `vtbl+88` | **PROVEN** |
| `00530260` tests `+332` | **DISPROVEN** |
| `0041AFA0` `+151==0` / `+368==1` skips `0041BEB0` | **PROVEN** |
| `0054EF00` `+151==0` / dest vs `+394==1` skips glyphs | **PROVEN** |
| `0041BEB0` is type `0x22` record fill, `ret 68` | **PROVEN** |
| `009DA9F0` tests widget `+332` | **DISPROVEN** |
| Type 18 `+332` is ActiveChild present index | **DISPROVEN** |
| Type 16 `+348` is present index in `vtbl+8` | **DISPROVEN** |
| Style-6 persist colour / flags | **UNREAD** |
| Clip CRC names | not claimed |

---

## 1. `0052CF40` after `+332=arg` (`PROVEN` / `DISPROVEN` writers)

`listing-00500000.txt` `0052CF40` `ret 4`. `ebp` = arg.

```
0052CF49  cmp [esi+332], ebp
          je  0052D35E
0052CF58  mov [esi+332], ebp
          xor eax, eax
          mov [esi+312], eax
          mov [esi+308], eax
          ; clear +316 list
0052CF93  cmp ebp, 6
          mov eax, [esi+324]
          mov ecx, [esi+328]
          mov [esi+344], eax
          mov [esi+324], ecx
          ja  0052CFDD              ; still vtbl+540
          ; jmp [0x52D368+group] → vtbl+564 or +560
0052CFE2  call [eax+540]            ; style*(arg)
          call [edx+176]            ; vtbl+176(arg)
          jne 0052D0CA
          ; non-animated: +336=+320, child vtbl+188, ret
0052D0CA  mov [esi+328], ebp        ; animated only
          ; +336 from style+28 or +320; 0052D740; child +188
```

Child walk (four copies): parent `vtbl+208==this`; type-8
skip only if parent `+332` is **1 / 3 / 4** (not 6);
then `vtbl+188(+332,+336)`.

No `mov`/`or` of `+144`..`+151`, `+340`, or `+368` in
`0052CF40`–`0052D362`.

`cmp ebp, 6` is **select** grouping (`ja` if arg `>6`),
not a present skip.

---

## 2. Tick `0052C7E0`: `+340`, style bits (`PROVEN`)

```
0052C7E0  call [eax+196]
          mov [esi+340], al
          push [esi+328]
          call [edx+540]            ; style*
          je  0052C848
          test [eax], 0x10
          je  0052C825
          mov cl, 0xFF
          mov [esi+144], cl
          mov [esi+145], cl
          mov [esi+146], cl
          mov [esi+147], cl         ; not +148..+151
0052C825  test [eax], 0x20
          je  0052C838
          mov [esi+80], 0
          mov [esi+76], 0
0052C838  test [eax], 0x40
          je  0052C848
          mov eax, 0x3F800000
          mov [esi+120], eax
          mov [esi+116], eax
0052C848  push dt
          call 00531EC0
          mov al, [esi+340]
          test al, al
          jne 0052C868              ; skip vtbl+544
          call [edx+544]
```

`+328` is the **live** style key the tick applies. It
equals 6 only after the animated `0052D0CA` arm (or a
later writer). Non-animated `0052CF40` leaves `+328`
and still stores `+332=6`.

`0052C730` (layout `vtbl+172` prefix) sets first-seen
`+340=1` and `+324/+328/+332=0`.

`0052C870` / `0052C8B0` query `0052F1A0`/`0052F1B0`
(`jmp vtbl+404`) then style bits `0x20`/`0x40`. They
are not present.

`0052C780` (candidate `vtbl+196`, **PARTIAL**):
`vtbl+540(+328)` then `+152<+156` or `+160<+164` or
`+168<+172` → return 0 else 1. That matches “fade not
done” → `+340=0` → `vtbl+544`.

---

## 3. Colour `+144..+151` writers (`PROVEN`)

| Site | Bytes | When |
| --- | --- | --- |
| `005334A0` / copy `0053380B` | `+144..+151=0` | type-4 ctor |
| `005339B0` | `+144..+147=0xFF` | after style-0 pos/colour from map `+36` |
| `0052C7E0` bit `0x10` | `+144..+147=0xFF` | every tick if style dword0 has `0x10` |
| `0052FE4A`–`0052FFA2` | pack `+144..+147` × `+132..+135` → `[esi+148]` dword | inside `0052F900` (`ret 4`) |
| `0052FFAF` | `[esi+148]=[+132]` | `vtbl+404` true arm |
| `00531EC0` `0053258F` / `00532BF1` | `lea +148`; child `vtbl+88` | inherit packed colour |

`0052F900` interpolates `+168` toward `+172` and the
`+140..+143` / `+144..+147` pairs, then writes
**`+148..+151`**. That is the colour the leaf tests.
Binding it to `vtbl+544` is **PARTIAL** (call shape
matches; rdata **UNREAD**).

---

## 4. `00531EC0` parent `+200` (`PROVEN`)

After the `+284` linked layout, own `+176`:

```
00532200  child = [this+176][i]
          call child.vtbl+208          ; get parent
          test eax, eax
          jne 00532227
          push esi
          call child.vtbl+204          ; set parent = this
00532230  cmp esi, [ecx+200]
          jne  … borrowed path (vtbl+400)
          ; else layout vtbl+80 / +456 / +72
0053258F  lea edx, [esi+148]
          call child.vtbl+88
          push dt
          call child.vtbl+4            ; tick
```

Same parent gate on `+188`. This is layout/tick, not a
`+332==6` skip. Leftover slot children still get
`vtbl+4` from the all-slot tick walk
(`00599E3F` / `0059A0C4`).

---

## 5. Present skip is leaf alpha / `+368`, not `+332` (`PROVEN` / `DISPROVEN`)

### `00530260` (type 5/10/12/18 `vtbl+8`)

Walk `+176` then `+188`. Skip child if
`parent!=this && !vtbl+400`, or `vtbl+420` twice.
Then `call [edx+8]` `ret 20`. **No `+332`.**

### Type 0 `0041AFA0` (`listing-00400000.txt`)

```
0041AFA8  mov al, [edi+151]
          test al, al
          jbe 0041AFE1                 ; alpha == 0
          ; dest +124/+128 vs 0:
          ;   nonzero → [+368]=0; jmp 0041B065 (draw)
0041AFE1  mov al, [edi+368]
          cmp al, 1
          jne 0041B05F
          ; +368==1: optional deref, then ret 20  — no 0041BEB0
0041B05F  mov [edi+368], 1
0041B065  ; size/uv/colour +148..+151 → 0041BEB0
```

Ctor `0041B800`: `mov [esi+368], al` with `eax=0`.

Alpha 0 uses the `+368==1` early-out. First such frame
sets `+368=1` and may still fall through; later frames
skip `0041BEB0`. Nonzero dest zeros `+368` and always
draws.

### Type 6 `0054EF00`

Same `+151` test; skip flag is **`+394`** (not `+368`).
`cmp [esi+394],1` / `je` out; else `+394=1` and emit
`0x27` glyphs. No `+332`.

### `0041BEB0`

`mov [eax], 0x22` then copies rect/uv/colour from the
stack. `ret 68`. No widget `this`, no `+332`.

Site from `0041AFA0`: `0041B4E6 call 0041BEB0`.

### `009DA9F0` (`listing-009c0000.txt`)

`this` is the D3D submit object (`[ebp+16020]` list
length). No load of a UI widget `+332`. It cannot
implement “skip leftover slot.”

### `+332==6` in `.text` widget listings

`listing-00500000.txt`: the only `cmp …, 6` next to
this machinery is `0052CF93 cmp ebp, 6`.
`listing-00540000.txt`: no `cmp [esi+332]`.
Type-18 wrap `00547C90` `cmp edi, 3` (not 6) after
`call 0052CF40`.

---

## 6. Type 16 `+348` vs type 18 `+332` (`DISPROVEN` as present index)

### Type 18 (`00547600`, vtbl `012485AC`)

Ctor zeros vector `+348/+352/+356`, then `00547500`
fills stride-8 `{state id, duration}` from persist
`def+480/+492`.

`vtbl+4` `00547380`: `0052C7E0`, then lookup
`+324` in that vector, or time-advance and
`vtbl+192([+348][i].id)`. That selects a **style
key**, not `vtbl+8` child `i`.

`vtbl+192` `00547C90`: `call 0052CF40` then extra
work **only if arg==3**. Arg 6 does not write the
`+348` byte used at `00547E1E`.

`vtbl+8` is shared `00530260` (no type-18 `ret 20`
in `00547360`–`00547E3A` except vector `00547AA0`).
It does not index `+176` by `+332`.

Attach `SelectState(5)` ≠ type-18 ActiveChild 5 is
already out of scope; this path does not revive that.

### Type 16 (`00549F60`, vtbl `01248A8C`)

`00549B20` (from ctor): `mov [esi+348/+352/+356], 0`
— vector begin/end/cap, plus `+360` byte. No
`ret 20` between `00549F60` and type 15 `0054C050`
except allocator `0054A377`. `vtbl+8` is inherited
`00530260`, which does not load `+348`.

Type 12 highlight `+348` → `+356[i]` is a **list**
index (`proofs/type12-highlight-plus348`), not type 16
and not a leftover-slot present gate.

---

## 7. How leftover slots stop presenting (chain)

```
00596763  old.vtbl+192(6) = 0052CF40
          +332 = 6
          vtbl+540(6); maybe +328 = 6
          child vtbl+188(6, dur)

00599E3F  every [ui+84] vtbl+4 = 0052C7E0
          vtbl+196 → +340
          style*(+328) bits 0x10/0x20/0x40
          00531EC0: parent +200, colour vtbl+88, child tick

0042E085  00595222 still vtbl+8 every non-null slot
00530260  still child vtbl+8 unless clip bits

0041AFA0 / 0054EF00
          if packed +151 == 0 (and skip-flag == 1):
              ret without 0041BEB0
009DA9F0  never sees that widget
```

Host `AttachFrontendTree` `Clear()` + draw-current-only
is leftover vs the native all-slot walk. Native hide is
**style → packed alpha / dest → leaf DIP skip**, not
`Visible` / `+302` / `+332==6` / type-18 child index.

Style-6 persist fields remain **UNREAD**.
Host `LeafDipSkipped` is `0041AFA0`/`0054EF00`
packed `+151==0`. Dest-zero already skips
sprites. Not current-slot-only draw.
