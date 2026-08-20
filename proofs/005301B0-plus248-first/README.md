# `005301B0` first-seen type-6 `+248` — authored 320 or remapped 512?

Investigation only. Production `src/` and `tests/` were not edited.
Do not invent `512` as a native dword.

Question: first-seen type-6 `UI_PRESS_START_TEXT` widget
`+248` bits. Authored persist `320`, or remapped `512`?
Root inherit `+264` is the `1.6` analog. Native dump of
the dword stays **UNREAD** unless a process dump is found.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`0052E580` / `0052F190` / `0052F230` / `0052F250` /
`0052F3A0` / `0052F3B0` / `0052F5C0` / `0052FFD0` /
`005301B0` / `005301C7` / `00531EC0` / `005331A0` /
`005339B0`);
`implementer/frontend/fn-0052F3B0-exact.txt`,
`fn-0052F3A0-exact.txt`, `02-layout.md`,
`16-resolution.md`;
`export/frontend/persist-tail.txt`,
`press-start-dests.txt`, `press-start-frame.txt`;
`src/Fable.Game/FrontendLayout.cs`
(`ComputeDestOrigin`);
`src/Fable.Formats/Defs/FrontendUiDef.cs`;
`tests/Fable.Formats.Tests/FrontendUiDefTests.cs`,
`FrontendLayoutTests.cs`;
`proofs/0041AC20-dest-formula`;
`proofs/persist-flag-names`;
`proofs/type6-plus204-writer`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Do not re-prove type-6 ctor skipping `0041AC20`, leftover
`+204=0`, or `0041AFA0` dest 4-tuple (type-6 draw does not
write one).

---

## Verdict

**PARTIAL. First store is authored `320`. Final `+248`
is inherit analog, not `0052E580` `512`. Native dword
UNREAD.**

`0052FFD0` is the dest-origin writer. Two first stores:

| VA | Path | Value stored |
| --- | --- | --- |
| `005301B0` | `vtbl+468` true → `0052E580(+52)` | viewport remap of persist pos |
| `005301C7` | `vtbl+468` false → copy `+52` | authored persist pos |

Type-6 persist `def+521=0` → `+302` bit 7 stays 0 →
`vtbl+468` `0052F3B0` returns 0 → **no** `0052E580` on
this origin. First store is `005301C7`: `+248 = +52`.
`+52` is persist `PositionX` **`320`**.

That is not the dword after `0052FFD0` returns. When
`vtbl+408` absolute is false (this widget), the same
function overwrites `+248`:

```
+248 = +248 * inherit(+272) + parentDest(+256)
```

Parent `UI_PRESS_START_SWAP` dest origin is `0`. Inherit
`+272` is parent dest scale `+264`. Root `def+520=1`
makes root `+264` the `1.6` analog (`1/640*1024`).
Product analog `320 * 1.6 + 0` equals `512` at first-seen
`1024×768`. That `512` is **not** a listing immediate and
**not** the `005301B0` remap store.

No native dump of widget `+248` was found
(`export/native/` is screenshots; listings have no live
dword). Do not lock `0x44000000`.

| Claim | Status |
| --- | --- |
| `0052FFD0` writes dest origin `+248/+252` | **PROVEN** `005301B0` / `005301C7` then `00530211` / `00530235` |
| Type-6 first-seen takes `005301B0` / `0052E580` | **DISPROVEN** — `def+521=0` |
| `005301C7` first store is persist `+52` | **PROVEN** |
| Persist `UI_PRESS_START_TEXT` `PositionX/Y` | **PROVEN** `320` / `240` (`persist-tail` `f32=320` / `240`) |
| Persist `def+521` remap origin on this widget | **PROVEN** `0` |
| Persist `def+520` remap size on this widget | **PROVEN** `0` |
| Persist `def+191` absolute on this widget | **PROVEN** `0` |
| Root `def+520=1` dest scale `+264` | **PROVEN** formula; **`1.6` analog** (dword **UNREAD**) |
| Final `+248` after inherit | **PARTIAL** analog `320 * +272 + +256`; product **not** dumped |
| Native first-seen `+248` bits | **UNREAD** |
| Listing immediate dest / origin `512` | **DISPROVEN** |
| Host `ComputeDestOrigin` vs this listing | **MATCH** |
| Host dump / tests lock `512,384` | **LEFTOVER** analog, not a native dword |

**Answer:** not `0052E580` remapped `512`. First store is
authored `320`. Post-inherit `+248` is the `1.6` analog
(would equal `512` at `1024×768`). Native bits stay
**UNREAD**. Do not invent them.

**Overall: PARTIAL.**

---

## 1. `0052FFD0` stores (`listing-00500000.txt`)

`0052FFD0`…`00530251`. First-seen ctor zeros `+152/+156`
(`0053383B` / `00533841`); `005339B0` zeros `+152` again.
`fld [esi+152]; fcomp [esi+156]; test ah, 0x05` equal →
`jp 0053017B` skip lerp. Then:

```
0053017B  mov edx, [esi+52]
0053017E  mov eax, [esi+56]
00530181  mov [esi+84], edx
0053018B  call [edx+468]          ; remap origin?
00530191  test al, al
00530193  je 005301C1
00530195  mov ecx, [esi+52]
          … push +52/+56 …
005301A9  call 0052E580           ; x/640*vpW
005301AE  mov ecx, [eax]
005301B0  mov [esi+248], ecx      ; remap store
005301B6  mov edx, [eax+4]
005301B9  mov [esi+252], edx
005301BF  jmp 005301D3
005301C1  mov eax, [esi+52]
005301C4  mov ecx, [esi+56]
005301C7  mov [esi+248], eax      ; authored store
005301CD  mov [esi+252], ecx
005301D3  mov edx, [esi]
005301D7  call [edx+408]          ; absolute?
005301DD  test al, al
005301DF  jne 0053024D            ; skip inherit
00530205  fld [esi+272]
0053020B  fmul [esi+248]
00530211  fstp [esi+248]
00530217  fld [esi+276]
0053021D  fmul [esi+252]
00530223  fstp [esi+252]
00530229  fld [esi+256]
0053022F  fadd [esi+248]
00530235  fstp [esi+248]
0053023B  fld [esi+260]
00530241  fadd [esi+252]
00530247  fstp [esi+252]
0053024D  pop esi
00530251  ret 4
```

`0052E580` (`0052E591` `test [0x13B8768]`) is only on
the `005301B0` arm.

---

## 2. Type-6 first-seen takes `005301C7`, not `005301B0`

`vtbl+468` is `0052F3B0` (`fn-0052F3B0-exact.txt`):

```
0052F3B0  movzx eax, [ecx+302]
0052F3B7  shr eax, 7
0052F3BA  ret
```

`005331A0` copies persist remap origin:

```
005332AE  mov al, [ecx+521]
005332B4  test al, al
005332B6  je 005332BE
005332B8  or [ebx+302], dl         ; dl=0x80
```

`UI_PRESS_START_TEXT` persist (`FrontendUiDefTests`
`Press_Start_remap_bits_come_from_def_520_521` /
`persist-flag-names`): `ScaleOriginByte=0` CRC
`0xB466D948`. Type-6 ctor `0054F5C0` → `0052CC50` →
`005334A0` → this copy. Bit 7 stays 0 → `vtbl+468`
false → `je 005301C1` → `005301C7`.

`+52` is persist Position. `005339B0`:

```
005339D1  mov [esi+52], ecx        ; layout0 +8
005339D7  mov [esi+56], edx
```

`persist-tail.txt` `UI_PRESS_START_TEXT`:

```
@0159  0x1EDB8A31 PositionX  i32=1134559232  f32=320
@0167  0x69DCBAA7 PositionY  i32=1131413504  f32=240
@0175  0xE78E700E ZoomX      f32=1
```

`1134559232 = 0x43A00000 = 320.0f`. First store bits
are that dword, not `512`.

`vtbl+408` absolute is `0052F190` (`+300` bit 6 from
`def+191`). Persist `AbsoluteCrc` `0x38BBD87F` on this
widget is `0` (`FrontendUiDefTests`). Inherit arm
runs.

---

## 3. Inherit analog — not a listing `512`

Root `UI_FRONTEND_PRESS_START_MENU` persist
`ScaleSizeByte=1` (`def+520`), `ScaleOriginByte=0`.
`0052F5C0` when `vtbl+464` true and not absolute:

```
0052F7AC  fdiv [0x1375CD4]        ; 640
0052F7B2  fmul [0x13B876C]        ; vpW
0052F7B8  fmul [esi+272]
0052F7BE  fstp [esi+264]
```

Root `+280==0` → `005339B0` `+272=1`. Persist zoom `1`.
First-seen `[0x13B8768]=1`, vp `1024×768`
(`0041E3F6` / `004299A8`). Root `+264` analog
`1/640*1024 = 1.6`. Native `+264` dword **UNREAD**.

`00531EC0` then `vtbl+460` `0052F250` writes
child `+272` from parent `+264`. Parent of the text
is `UI_PRESS_START_SWAP` (`press-start-frame.txt`):
persist `0,0`, remap size/origin `0`, dest origin `0`.
SWAP `+264` = inherit `1.6 * 1`. TEXT `+272` analog
`1.6`. TEXT `+256` from SWAP dest origin `0`.

Final TEXT `+248` analog:

```
320 * 1.6 + 0 = 512
240 * 1.6 + 0 = 384
```

Same arithmetic as `0052E580(320)` at `1024×768`, but
this widget never calls `0052E580`. Applying
`pos/640*vpW` on the type-6 origin would be invented
for `def+521=0`.

Type-6 draw `0054EF00` reads this final `+248` as a
pen (`proofs/0041AC20-dest-formula`). It does not
write a dest 4-tuple.

---

## 4. Host `ComputeDestOrigin` (**MATCH**)

```209:238:src/Fable.Game/FrontendLayout.cs
    public static (float X, float Y) ComputeDestOrigin(
        FrontendWidgetLayout widget,
        float inheritScaleX,
        float inheritScaleY,
        float parentDestX,
        float parentDestY,
        FrontendViewport viewport)
    {
        float x;
        float y;
        if (widget.ScaleOriginToViewport)
        {
            var scaled = ApplyResolutionScale(
                widget.PositionX, widget.PositionY, viewport);
            x = scaled.X;
            y = scaled.Y;
        }
        else
        {
            x = widget.PositionX;
            y = widget.PositionY;
        }

        if (!widget.Absolute)
        {
            x = x * inheritScaleX + parentDestX;
            y = y * inheritScaleY + parentDestY;
        }

        return (x, y);
    }
```

`ScaleOriginToViewport` is persist `def+521`. False on
this widget → skip `ApplyResolutionScale` (`0052E580`).
Not absolute → `320 * inherit + parentDest`. Host
`FrontendLayoutTests.Press_Start_root_remapSize_scales_child_origin_to_viewport`
and `export/frontend/press-start-dests.txt`
`UI_PRESS_START_TEXT t=6 dest=512,384,512,384` lock
that analog. They are **not** a native `+248` dump.

---

## 5. Unread sites

Do not fill these with `512`.

| Site | What is missing |
| --- | --- |
| Widget `+248` after `0052FFD0` `ret 4` | Native first-seen dword |
| Widget `+264` after `0052F7BE` | Native first-seen `1.6` analog |
| Process dump | `export/native/` screenshots only |
| Type-6 vtbl `01249CCC` `+408` / `+468` dwords | Cluster `0052F190` / `0052F3B0` is **PARTIAL** unless rdata slot dumped |

`005301B0` remains a real store — just not this widget’s
first-seen path.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00500000.txt`
- `C:\FableCSharp\implementer\frontend\fn-0052F3B0-exact.txt`
- `C:\FableCSharp\implementer\frontend\02-layout.md`
- `C:\FableCSharp\implementer\frontend\16-resolution.md`
- `C:\FableCSharp\export\frontend\persist-tail.txt`
- `C:\FableCSharp\export\frontend\press-start-dests.txt`
- `C:\FableCSharp\src\Fable.Game\FrontendLayout.cs`
- `C:\FableCSharp\proofs\0041AC20-dest-formula\README.md`
