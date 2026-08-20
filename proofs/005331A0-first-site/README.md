# First `005331A0` after Press Start factory is the `0054E3D0` child walk

Investigation only. No production `src/` edits.

Question: when is `005331A0` first called after Press Start
factory? During `0054E3D0` / `0052CC50` child walk, not
`00596917`? Host rebuild leftover?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
(`005331A0` / `005334A0` / `0052CC50` / `0054E3D0` /
`00533720`);
`listing-00500000.txt` (`005334A0` `005336F6`,
`00533720` `00533982`, `0052CC50` / `0052CCA0`,
`00531EC0` / `0052C84F`);
`listing-00540000.txt` (`0054E3D0` / `0054C3A0`);
`listing-00400000.txt` (`0041D512`);
`listing-00580000.txt` (`00598A1C` / `00598BD2` /
`00596917`);
`functions.tsv` `0x00596917` / `0x00531EC0` (bad merge);
`implementer/frontend/fn-005331A0-exact.txt`,
`fn-0052CC50-exact.txt`, `fn-005334A0-exact.txt`;
`proofs/press-start-label-scan`;
`proofs/00598A1C-only-e5`;
`proofs/type10-subscribe-first`;
`src/Fable.Game/EngineLifecycle.cs`
(`InitFrontendUi`, `AttachFrontendTree`,
`BindNewProfileFromArmedTick`);
`src/Fable.Game/FrontendWidgetFactory.cs`.

Do not re-prove `0xE5` attach, persist `+212` /
`vtbl+520`, or the `UI_PRESS_START_TEXT` name scan.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| First `.text` `E8` of `005331A0`? | **Only two sites:** `005336F6` (type-4 ctor `005334A0`) and `00533982` (type-4 copy-ctor `00533720`). | **PROVEN** `e8.tsv` |
| First-seen after Press Start factory? | **During** type-10 ctor: `0041D512` `0054E3D0` → `0054E3D8` `0052CC50` → `0052CC58` `005334A0` → **`005336F6`**. Before `0054E3DF` vtbl override. | **PROVEN** |
| Is that `00596917`? | **No.** `00596917` callees are `0059B5D7` / `00596763` / `00BFEA1A` / `00851700` / `00851770`. No `005331A0`. | **DISPROVEN** |
| Host leftover? | **Yes, the rebuild.** `BindNewProfileFromArmedTick` `Clear`s and `Build`s, then notes every child as `005331A0`. Native `00596917` switches an already-built slot. | **LEFTOVER** |

---

## Verdict

**During `0054E3D0` / `0052CC50`, not `00596917`. Host
rebuild notes are leftover.**

First-seen `00598A1C` arg 0 skips `UI_FRONTEND_MEDIA_PLAYER_ERROR`.
First factory is `00598BD2` `"UI_FRONTEND_PRESS_START_MENU"`
slot `0x14`. That is type 10:

```
0041D4FC  push 0x16C
0041D512  call 0054E3D0
0054E3D8  call 0052CC50
0052CC58  call 005334A0
005336F6  call 005331A0          ; first site
0054E3DF  mov [esi], 0x12497E4   ; type-10 vtbl after return
```

`005331A0` then walks `[def+112]..[def+116]` and
`0041D21B` each persist child (DFS). Child ctors that
go through type 4/5 (`0052CC50` / `005334A0`) call
`005331A0` again. Those later hits are still inside
the same Press Start factory, not a later attach.

`00596917` does not factory and does not call
`005331A0`. New Profile was already built later in
the same `00598A1C` slot fill (`00598FCF` slot `0x17`).

Host `FrontendWidgetFactory.Build` is the first-seen
walk analog (**MATCH**). `AttachFrontendTree` then
notes every non-root child as `005331A0` after **any**
root, including the `00596917` rebuild. Those notes
are **LEFTOVER**.

| Claim | Status |
| --- | --- |
| `.text` `E8 005331A0` sites are only `005336F6` / `00533982` | **PROVEN** `e8.tsv` |
| `005336F6` is the last insn of type-4 ctor `005334A0` | **PROVEN** `ret 4` at `00533712` |
| `00533982` is copy-ctor `00533720` via `0052CCA0` | **PROVEN** |
| First-seen path is `0054E3D0` → `0052CC50` → `005334A0` → `005336F6` | **PROVEN** |
| First factory in first-seen `00598A1C` is Press Start | **PROVEN** (`je 00598B90` skips media error) |
| `005331A0` walks persist children via `0041D21B` | **PROVEN** `fn-005331A0-exact.txt` |
| `00596917` calls `005331A0` | **DISPROVEN** |
| `00531EC0` dest layout calls `005331A0` | **DISPROVEN** (bad `functions.tsv` merge) |
| Host factory walk on first Press Start | **MATCH** `FrontendWidgetFactory` |
| Host `005331A0` notes after `00596917` rebuild | **LEFTOVER** |
| Host summary `005331A0 children=N` after layout | **LEFTOVER** (walk already ran in ctor) |

---

## 1. Listing callers of `005331A0`

`e8.tsv` dest `0x005331A0` (complete):

```
0x005336F6	0x005331A0
0x00533982	0x005331A0
```

No other `E8`. No site in `00596917` / `00598A1C` /
`00531EC0`.

`005334A0` ends:

```
005336F0  mov [esi+220], ebx
005336F6  call 005331A0
005336FB  pop edi
…
00533712  ret 4
00533715  int3 …
00533720  push ebx               ; next function
```

`00533720` is the copy-ctor (same field init, then
`00533982  call 005331A0`). Type-5 copy `0052CCA0`
calls it (`0052CCA8`). First-seen uses `0052CC50`,
not `0052CCA0`.

`functions.tsv` `0x00531EC0` size **2310** lists both
`005331A0` hits. That is a **bad merge**. `00531EC0`
starts after `int3` pad, is dest layout (`fld [ebp+8]`
/ `vtbl+148`), and is called from tick `0052C84F`.
It does not `E8` `005331A0`. Do not treat first
`005331A0` as a `00531EC0` / `0052C7E0` tick.

---

## 2. First-seen stack is the type-10 ctor

Sole `.text` `E8 0054E3D0` is factory type 10
(`0041D512`). `0054E3D0`:

```
0054E3D0  mov eax, [esp+4]       ; def
0054E3D8  call 0052CC50
0054E3DF  mov [esi], 0x12497E4
0054E408  ret 4
```

`0052CC50` (18 insns):

```
0052CC58  call 005334A0
0052CC5D  mov [esi], 0x1245DE4   ; type 5; type 10 overwrites
```

So the first `005331A0` runs **inside** `0054E3D0`,
**before** the type-10 vtbls. It is not a post-factory
pass and not `00596917`.

Inside `005331A0` (`fn-005331A0-exact.txt`): persist
`+212` / flags / styles, then

```
005333AF  mov edx, [ecx+116]
005333B2  sub edx, [ecx+112]
005333CD  push eax
005333CE  call 0041E5F2
005333D5  call 0041D21B          ; child ctor
00533413  call [edx+236]         ; append
```

Press Start persist children (already listed in
`action26-subscribers` / factory tests) therefore
construct during this walk. Type 5 forest / title
go `0052CC50` again. Type 12 list is
`0054C3A0` → `0053B63E` → `0052CC50` → same
`005336F6`. Nested, still the Press Start factory.

---

## 3. `00598A1C` first factory is Press Start

`00598A46  cmp [ebp+124], bl` / `je 00598B90`.
First-seen arg 0 skips `UI_FRONTEND_MEDIA_PLAYER_ERROR`
(`00598A7D`). Then:

```
00598BA2  push "UI_FRONTEND_PRESS_START_MENU"
00598BB7  mov [ebp+108], 0x14
00598BD2  call 0041DB1D
```

That is the first `0041DB1D` on this path. First
`005331A0` is the type-10 ctor of that root.

The same `00598A1C` later factories other slots
(Profiles, Delete, New Profile, …). Those also run
`005331A0` during **their** ctors. They are still
`00598A1C` populate, not `00596917`.

---

## 4. `00596917` is a slot switch

`functions.tsv` `0x00596917` (44 insns): no
`005331A0`. Listing (`press-start-label-scan`):

```
00596921  push 23                ; slot 0x17
00596930  call 0059B5D7
0059693B  call 00596763          ; switch current menu
00596962  call 00851700
00596970  call 00851770          ; UI_NEW_PROFILE_EDIT_BOX
```

Slot `0x17` was factory-built earlier in `00598A1C`.
No child walk. No `0054E3D0`.

---

## 5. Host: factory MATCH, rebuild leftover

`InitFrontendUi` → `AttachPressStartWidgets` →
`AttachFrontendTree` → `FrontendWidgetFactory.Build`.
`Build` / `AttachChildren` walks persist
`ChildIndices` via `0041D21B` types. That is the
first-seen `005331A0` analog. **MATCH.**

After `Build` returns, `AttachFrontendTree` notes
every non-root child:

```
Note(ChildAttachFn, … "005331A0 child {name} type {type}")
```

`InitFrontendUi` later notes
`"005331A0 children={FrontendChildCount}"` **after**
`ApplyFrontendScaleInit` / `LayoutFrontendWidgets`.
Native walk already finished at `005336F6`, before
type-10 vtbls and before dest layout `00531EC0`.
Those Notes are timing leftovers, not a second native
call.

`BindNewProfileFromArmedTick` (`00596917` analog)
calls `AttachFrontendTree(NEW_PROFILE)` again:
`Clear` + `Build` + the same per-child `005331A0`
notes. Native does not re-run `005331A0` there.
**LEFTOVER** rebuild. Same shape on
`CommitNewProfileFromArmedEdit` / Main Menu attach.

Do not treat host `005331A0` notes after `00596917`
as proof that New Profile re-runs Press Start ctor.

---

## Do not invent

- A third `.text` `E8` of `005331A0` (only two).
- `005331A0` as a `00531EC0` / tick dest pass.
- `00596917` / `0059697A` calling `005331A0`.
- First-seen media-error factory (arg 0 skips it).
- Copy-ctor `00533982` as the first-seen site.

**Proposed (do not apply here):** keep factory child
walk on first Press Start. Drop `AttachFrontendTree`
rebuild on `00596917` (switch the already-built slot).
If child Notes stay, emit them from the factory walk
on first construct only, not after later attach.
