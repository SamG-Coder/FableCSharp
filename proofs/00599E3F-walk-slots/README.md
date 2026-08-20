# `00599E3F` after `00596763`: `[ui+84]` walk, not `[ui+32]` current

Investigation only. No production `src/` edits.

Question: Tick `00599E3F` after `00596763`: does it
walk all `[ui+84]` slots like `00595222`, or only
`[ui+32]` current? Host `TickFrontendWidgets`
leftover?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`00599E3F` `00599ED2` `0059A0C4` / `00595222` /
`00596763` / `00596917` / `0059B5D7` / `0059B039` /
`004292C0`);
`proofs/00596763-switch/README.md`;
`proofs/00595222-first-node/README.md`;
`src/Fable.Game/EngineLifecycle.cs`
(`TickFrontendWidgets`, `BindNewProfileFromArmedTick`,
`AttachFrontendTree`, `LayoutFrontendWidgets`).

Do not re-prove `00596763` as `[ui+32]` push_back /
`+152`/`+156` (not a slot rebuild), persist Type=10
on PRESS_START / NEW_PROFILE, or `0059B5D7` node
layout (`+16` key / `+20` value).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Walk after `00596763`? | Same in-order `[ui+84]` walk as `00595222`. | **PROVEN** `0059A0C4` |
| Only `[ui+32]` current? | **No.** `[ui+32]` is the switch deque (`0059B039` back), used before the walk. | **DISPROVEN** |
| Host `TickFrontendWidgets`? | Notes the VA, then layouts **one** persist tree. After `0xE5` that tree is `Clear()` + NEW_PROFILE. | **LEFTOVER** |

---

## Verdict

**It walks every non-null `[ui+84]` slot, like
`00595222`.** After `00596763` (same tick via
`00596917`, or a later tick) the tick always
falls through to `0059A0C4`:

`[ui+84]` sentinel → `[head+8]` leftmost →
`[node+20].vtbl+4(dt)` if non-null →
`004292C0` successor until the sentinel.

That is the draw walk with **`vtbl+4`** instead
of **`vtbl+8`**. No `cmp` against `[ui+32].back()`,
`[ui+156]`, or a slot key. Null values skip.

Host `TickFrontendWidgets` **Notes**
`00599E3F [ui+84] vtbl+4` then
`LayoutFrontendWidgets` on `_frontendWidgets`.
`BindNewProfileFromArmedTick` still
`AttachFrontendTree` `Clear()` + one factory
walk. Native keeps Options `0x1`, Press Start
`0x14`, New Profile `0x17`, … and ticks them
all. Host leftover is the **list**, not the
Note string.

| Claim | Status |
| --- | --- |
| Same-tick `00599E3F` → `00596917` → `00596763` when `[ui+160]≠0` | **PROVEN** `00599ED2` |
| After that switch, control reaches `0059A0C4` | **PROVEN** (`je` / `jmp`) |
| `0059A0C4` is `[ui+84]` in-order, `[node+20].vtbl+4`, next `004292C0` | **PROVEN** |
| `00595222` is the same walk with `vtbl+8` | **PROVEN** |
| Walk filters to `[ui+32]` current / `[ui+156]` | **DISPROVEN** |
| `[ui+32]` is the tick iterator | **DISPROVEN** (deque; used at `00599E90`) |
| Host Note names `[ui+84]` `vtbl+4` | **MATCH** (string only) |
| Host ticks every native slot after `00596763` | **DISPROVEN** leftover (`Clear`) |

---

## 1. Same tick: switch then walk

`00599E3F`–`0059A235` `ret 4`. `esi` = UI.
`[ebp+8]` = dt (the `fld` / `fstp [esp]` arg).

Armed bind (`listing-00580000.txt`):

```
00599E88  cmp [esi+160], bl
          je 00599F04
00599E90  lea edi, [esi+32]
          call 0059B039           ; deque --end → back()
          cmp [eax], ebx
          je 00599F04
          … [back].vtbl+196 / vtbl+56 …
00599ECA  mov [esi+160], bl
00599ED2  call 00596917           ; slot 0x17 → 00596763
```

`00596763` rewires `[ui+32]` / `[ui+152]` /
`[ui+156]` and does **not** change `[ui+84]`
(`proofs/00596763-switch`). Then:

```
00599F04  mov eax, [esi+96]
          cmp eax, ebx
          je 0059A0C4             ; first-seen: +96 still 0
          … 0059697A / 00596763 …
0059A070  jmp 0059A0C4
```

First-seen Press Start: `[ui+96]==0` → jump.
After `00851700` from `00596917`, later ticks
may take the `+5` arm and still **`jmp 0059A0C4`**.
The slot walk is not skipped by the switch.

---

## 2. `0059A0C4` vs `00595222`

Tick (`0059A0C4`–`0059A0EF`):

```
0059A0C4  mov eax, [esi+84]       ; sentinel
          mov edi, [eax+8]        ; leftmost
          cmp edi, eax
          je 0059A0EF             ; empty
0059A0CE  mov ecx, [edi+20]       ; widget*
          cmp ecx, ebx
          je 0059A0E1             ; null value: skip
          fld [ebp+8]
          mov eax, [ecx]
          push ecx
          fstp [esp]
          call [eax+4]            ; vtbl+4
0059A0E1  push edi
          call 004292C0           ; in-order successor
          mov edi, eax
          cmp edi, [esi+84]
          pop ecx
          jne 0059A0CE
```

Draw (`00595222`–`0059525A` `ret 8`):

```
00595225  mov eax, [ebx+84]
          mov esi, [eax+8]
          cmp esi, eax
          je empty
00595230  mov ecx, [esi+20]
          test ecx, ecx
          je next
          … push 0,0,0,arg,arg …
          call [eax+8]            ; vtbl+8
0059524A  push esi
          call 004292C0
          cmp esi, [ebx+84]
          jne 00595230
```

Same header, same first node, same null skip,
same `004292C0` increment. Difference is the
vtbl slot and the args (dt vs two draw args).
No type-10 test. No “current screen only.”

After `00596763`, slot `0x14` is still in the
map. Its `[node+20]` is non-null, so this tick
**still calls Press Start `vtbl+4`**, then
Options `0x1`, New Profile `0x17`, and every
other filled cell. First node is still key **0**
(null → skip).

---

## 3. What `[ui+32]` actually does here

`lea edi, [esi+32]` appears only in the
`[ui+160]` prelude (`00599E90`). `0059B039`
is deque `--end` → `back()`, the widget
`00596763` just pushed (or Press Start before
the first switch). That pointer is for
`vtbl+196` / `vtbl+56` and a slot-`0x13`
identity `cmp`, not the iterator.

After the walk:

```
0059A0EF  mov ecx, [esi+152]      ; old, set by 00596763
          …
0059A119  mov ecx, [esi+156]      ; new
          push 5
          call [eax+192]
          … then zero +152/+156
```

That is the **transition** pair from the
switch, after every slot has already ticked.

---

## 4. Host leftover

`TickFrontendWidgets`:

```
Note("00599E3F [ui+84] vtbl+4")
if FrontendUiArmed
    BindNewProfileFromArmedTick()   // 00596917 / 00596763
LayoutFrontendWidgets()             // _frontendWidgets
Note("004292C0")
```

`BindNewProfileFromArmedTick` →
`AttachFrontendTree(NEW_PROFILE)`:

```
_frontendWidgets.Clear()
built = Factory.Build(rootName)
_frontendWidgets.AddRange(built)
BindFrontendSlot(0x17)
```

`LayoutFrontendWidgets` loops that list.
Native `0059A0C4` still visits every
non-null `[ui+84]` value, including
`0x14` Press Start. Host has already
dropped it.

The Note string is MATCH. The walked set
after `00596763` is **LEFTOVER**.

---

## Do not invent

- `00599E3F` ticking only `[ui+156]` /
  `[ui+32].back()`.
- A second `[ui+84]` walk that starts
  after `00596763` with a current-only
  filter.
- Host `Clear` as the native post-switch
  tick set.
- `.text` comparing slot key to `0x17`
  at `0059A0CE`.

**Proposed (do not apply here):** keep the
slot map; `TickFrontendWidgets` walks every
non-null value with `vtbl+4`, same order as
`00595222`.
