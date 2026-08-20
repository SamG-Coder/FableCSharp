# `0059A119` `vtbl+192`(5) is `0052CF40` `+332=5` on New Profile

Investigation only. No production `src/` edits.

Question: After `00596763`, same tick `0059A119` push 5 /
`vtbl+192` on `[ui+156]`. Confirm it is `0052CF40`
`+332=5` on New Profile type-10. Any other first-seen
arg 5 writers?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`00599E3F`–`0059A235` / `00596763` / `00596917` /
`005952C3` / `00595845`);
`listing-00500000.txt` (`0052CF40`–`0052D362` /
`0052C730` / `0052CAF0`);
`listing-00540000.txt` (`0054E3D0` / `0054CBF0`
`0054CE4B` / `00548FA2`);
`listing-00400000.txt` (`0042F015` `"Init frontend"`);
`e8.tsv` (`0042F015` → `005952C3`);
`proofs/00596763-switch/README.md`;
`proofs/00599E3F-walk-slots/README.md`;
`proofs/0052CF40-selectstate-6/README.md`;
`proofs/0052C730-host-state/README.md`;
`proofs/00595222-first-node/README.md`.

Do not re-prove persist Type=10 on PRESS_START /
NEW_PROFILE, `00596763` as `[ui+32]` push_back /
`+152`/`+156`, or `+332=6` as a `+302` hide.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| After `00596763`, same tick `0059A119` `push 5` / `vtbl+192` on `[ui+156]`? | **Yes.** Same `00599E3F` (`00599E3F`–`0059A235` `ret 4`): `00599ED2` → `00596917` → `00596763` stores `[ui+156]=` slot `0x17` and `[ui+152]=` old back; walk `0059A0C4`; then `0059A119`. | **PROVEN** |
| Is that `0052CF40` `+332=5`? | Shared type-10 select body. `0052CF40` `mov [this+332], ebp` with `ebp` = stack arg. Type-10 ctor `0054E3D0` writes vtbl `012497E4` and has **no** `+192` override (type 12 / 18 wrap then `E8` `0052CF40`). | **PROVEN** body / store; rdata dword **UNREAD** |
| Target is New Profile type-10? | `[ui+156]` is the incoming widget from `00596917` key `0x17` `UI_FRONTEND_NEW_PROFILE_SCREEN`. Persist / factory type 10 (`0054E3D0`). First-seen `+332` was `0` (`0052C730`). | **PROVEN** |
| Any other first-seen arg-5 writers? | **Yes, one other `vtbl+192`(5) site:** Init frontend `005952C3` / `005952CF` on `[ui+32].back()` = Press Start, before any frame and before `00596763`. **No** earlier `+192`(5) on New Profile itself. Child `vtbl+188`(5) from `0052CF40` can write child `+332=5` (`0052CAF0` candidate). Type-12 `0054CE4B` / `00548FA2` `0052CF40`(5) and later-menu `push 5`/`+192` sites are **not** this first-seen switch. | **PROVEN** `005952C3`; New Profile first `+192`(5) **PROVEN** as `0059A119`; child `+188` **PARTIAL**; other sites **DISPROVEN** as first-seen |

---

## Verdict

**Yes: same-tick `0059A119` is New Profile type-10
`vtbl+192`(5) → `0052CF40` → `+332=5`.**

`00596763` leaves `[ui+152]`/`[ui+156]` set. The
same `00599E3F` then, if `[ui+152]≠0` and old
`vtbl+196` or `vtbl+56` `[eax+3]==0`, calls new
`[ui+156].vtbl+192(5)`, registers the new inner,
and **zeros** `+152`/`+156`. So this apply is
one-shot per switch.

The only other first-seen `push 5` / `vtbl+192`
writer is `"Init frontend"` `005952C3` on Press
Start (then already `+332=5` until `00595845` /
`00596763` set 6). New Profile stays `+332=0`
until `0059A119`.

| Claim | Status |
| --- | --- |
| `0059A119` is inside tick `00599E3F` after `00596763` | **PROVEN** |
| `ecx` = `[esi+156]`, `push 5`, `call [eax+192]` | **PROVEN** `0059A119`–`0059A123` |
| `[ui+156]` is slot `0x17` New Profile | **PROVEN** (`00596763-switch`) |
| New Profile is type 10 | **PROVEN** (`00595222-first-node`) |
| Type-10 `+332` store is `0052CF40` | **PROVEN** body; `.rdata` `012497E4+192` **UNREAD** |
| First-seen New Profile `+332` before this is 0 | **PROVEN** (`0052C730`) |
| Gate can skip `0059A119` if old `+196` false **and** `[style+3]≠0` | **PARTIAL** (site always present; first-seen take assumed after `+152` store; `+196`/`+56` bodies **UNREAD**) |
| `005952C3` is first-seen `vtbl+192`(5) on Press Start | **PROVEN** `0042F015` |
| `005952C3` writes New Profile `+332` | **DISPROVEN** (deque back is `0x14`) |
| Type-12 `0054CE4B` / `00548FA2` first-seen on this switch | **DISPROVEN** (needs type-12 `+192`(5); parent `0052CF40` forwards `+188` not `+192`) |
| `[ui+164]` / `[ui+168]` arms are also `+192`(5) | **DISPROVEN** (no `push 5`) |
| Type-10 `.rdata` `012497E4+192 == 0052CF40` | **UNREAD** (`listing-01200000` ends `0122CFFE`) |

---

## 1. Same tick: `00596763` then `0059A119`

`00599E3F`–`0059A235` `ret 4`. `esi` = UI.

Armed New Profile (`[ui+160]≠0` from empty
`00595845`):

```
00599E88  cmp [esi+160], bl
          je  00599F04
          … [ui+32].back() vtbl+196 / vtbl+56 …
00599ECA  mov [esi+160], bl
00599ED2  call 00596917          ; slot 0x17 → 00596763
```

`00596763` (`listing-00580000.txt`):

```
005967C5  push 6
005967C9  call [eax+192]         ; old = [ui+32].back()
          …
00596812  mov [esi+156], eax     ; incoming 0x17
00596818  mov [esi+152], edi     ; old Press Start
```

`00596917` then `00851700` / `00851770` (edit box)
and returns. First-seen `[ui+96]` is still 0 →
`je 0059A0C4` (`00599E3F-walk-slots`). After the
`[ui+84]` `vtbl+4` walk:

```
0059A0EF  mov ecx, [esi+152]
          cmp ecx, ebx
          je  0059A161            ; skip if no pending switch
          call [eax+196]
          test al, al
          jne 0059A119
          … old vtbl+56; cmp [eax+3], bl
          jne 0059A161
0059A119  mov ecx, [esi+156]
0059A11F  mov eax, [ecx]
0059A121  push 5
0059A123  call [eax+192]
          … input vtbl+8 / vtbl+24 on new +4 …
0059A155  mov [esi+152], ebx
0059A15B  mov [esi+156], ebx
```

`[ui+164]` (`0059A161`) and `[ui+168]`
(`0059A1CB`) use the same `+196`/`+56` gate but
**never** `push 5` / `vtbl+192`. They clear those
cursors and may set `[ui+28+41]`.

`00596763` just wrote `+152`/`+156`, so the null
check does not skip. First-seen take of the
`+196`/`+56` arm is **PARTIAL** (no listing of
those slots on type 10). The **site** is
unconditional once the gate is true.

---

## 2. `0052CF40` `+332=5` on type-10 New Profile

`0052CF40`–`0052D362` `ret 4`. `ebp` = arg.

```
0052CF49  cmp [esi+332], ebp
          je  0052D35E            ; no-op if already 5
0052CF58  mov [esi+332], ebp      ; +332 = 5
          xor eax, eax
          mov [esi+312], eax
          mov [esi+308], eax
          ; free +316 list
          … optional vtbl+560/+564 if arg ≤ 6 …
          push ebp
          call [vtbl+540]
          ; then own +176: child vtbl+188(+332, +336)
```

Type-10 ctor `0054E3D0`: `0052CC50` (vtbl
`01245DE4`) then overwrite `012497E4`. Nearby
cluster (`0054E3D0`–`0054E4F0`) has **no**
`0052CF40` thunk. Type 12 `0054CBF0` and type 18
`00547C90` **do** wrap `E8 0052CF40`. Shared
select body is therefore `0052CF40` for type 10
(`0052CF40-selectstate-6`). `.rdata`
`012497E4+192` is past `listing-01200000`.
**UNREAD**.

`0054E4B0` `vtbl+172` → `0052C730` zeros
`+324/+328/+332`. Factory-built slot `0x17` is
therefore `+332=0` until this call. Early-out
does not fire.

Arg 5 is **not** in the type-8 child-skip set
`{1,3,4}`. New Profile children still get
`vtbl+188`(5, duration). That is not a second
`push 5` / `+192` site. Default `+188` candidate
`0052CAF0` writes `[this+332]=arg0` and forwards
`vtbl+168`, **not** `+192` (`0052CF40-vtbl188-forward`).
`.rdata` `+188` **UNREAD**.

---

## 3. Other first-seen arg-5 writers

### First-seen `vtbl+192`(5) sites

| Site | Object | When | First-seen? |
| --- | --- | --- | --- |
| `0042F015` `call 005952C3` → `005952CF` `push 5; call [eax+192]` | `[ui+32].back()` after `add ecx, 32` | `"Init frontend"` after `00598A1C` bind | **Yes.** Back is Press Start `0x14` (`0059672A`). |
| `0059A121` `push 5; call [eax+192]` | `[ui+156]` | same tick as `00596763` | **Yes.** New Profile `0x17`. |
| `005958D2` `push 6` (empty `00595845`) | `[ui+32].back()` | `0xE5` empty-profile, **before** `00599E3F` | Arg **6**, not 5. |
| `005967C5` `push 6` | old back | this switch | Arg **6**. |

`e8.tsv`: **only** `0042F015` calls `005952C3`.
That is the Init frontend tail (`0042EF6F`
string, after `0042DED5(0)`).

`005952C3` cannot be New Profile: `[ui+32]`
holds Press Start until `00596763` `0059B61C`
push_back.

### Direct `E8 0052CF40` with `push 5` (not this path)

| Site | Role | First-seen Press Start → New Profile |
| --- | --- | --- |
| `0054CE4B` | type-12 `0054CBF0` case 5 | **DISPROVEN**. Needs type-12 `+192`(5). Parent type-10 `0052CF40` calls child `+188`. |
| `00548FA2` | type-18-ish case (`sub eax,2` after 0/3) then `0052CF40`(5) | **DISPROVEN** as this switch. |
| `0053A8B2` | later widget (`+522` persist) | **DISPROVEN** (not type-10 switch). |
| `0042D650` / `0042D6A7` / `0042D72A` | wrappers: `push ebx` then `0052CF40` | Only if those vtbls are used **and** arg is 5. Type 10 does not install them. |

### Other `push 5` / `call [vtbl+192]` in `0058xxxx`

`0058768F`, `0058806E` (`PC_CLOSE_CLICKABLE`),
`005893F0`, `00589C43`, `0058AF01`, `0058E1A2`
(`TEXT_GUI_MENU_REFRESH_RATE_HZ`), `0058F979`,
`00590159`: options / clickable / refresh-rate
menus. First-seen empty path never opens those
before New Profile (`0059A238-first-consumes`:
`0xE5` → `00596917` only). **DISPROVEN** as
first-seen writers.

`005606A4`, `0056BCBD`, `00577698`, `005780D6`:
later `0055`/`0056`/`0057` UI. Same.

`0053D7F5` walks `+176` children `+192`(5) behind
`[+408]`. Not the type-10 switch.

---

## Do not invent

- `0059A119` writing `+302` / host `Visible`.
- Lionhead names for style keys 0–6.
- `.rdata` `012497E4+192` dword.
- `005952C3` targeting slot `0x17`.
- `[ui+164]`/`[ui+168]` as a second `+192`(5).
- Type-12 case 5 as the New Profile root write.

**Proposed (do not apply here):** after
`00596763`, apply `SelectState(5)` to the new
current (slot `0x17`) once, then clear the
pending pair. Keep Init frontend
`SelectState(5)` on Press Start as a **separate**
first-seen write.
`
