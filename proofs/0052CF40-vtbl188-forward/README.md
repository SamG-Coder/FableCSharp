# `0052CF40` after `+332=arg` forwards `vtbl+188` to `+176` children

Investigation only. No production `src/` edits.

Question: `0052CF40` after `+332=arg` forwards `vtbl+188`
to `+176` children. What is `vtbl+188`? Does state 6 on
a type-10 skip child **draw**? Type 8 skip only when
parent `+332` is 1/3/4 — does 6 skip?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`0052CF40`–`0052D362` / `0052CAF0`–`0052CBE0` /
`00530260`–`005303E0` / `0052F1D0` / `0053D3B0`);
`listing-00540000.txt` (`00547C90` / `0054CBF0`);
`listing-01200000.txt` (ends `0122CFFE`);
`proofs/0052CF40-selectstate-6/README.md`;
`proofs/0052C730-host-state/README.md`;
`implementer/frontend/14-container.md`.

Do not re-prove `00596763` as old-current `vtbl+192(6)`,
or persist Type=10 on PRESS_START. Do not invent hide.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

---

## Verdict

| Claim | Status |
| --- | --- |
| After `mov [this+332], arg`, `0052CF40` walks own `+176` and calls child `vtbl+188(+332, +336)` | **PROVEN** |
| `vtbl+188` ABI is thiscall, 2 args: state then duration (`+336`) | **PROVEN** |
| `vtbl+188` is draw (`vtbl+8` / `00530260`) | **DISPROVEN** |
| `vtbl+188` writes `+302` / host `Visible` | **DISPROVEN** (no `+302` in `0052CF40`) |
| Type-10 `+332=6` makes `00530260` skip child `vtbl+8` | **DISPROVEN** (`00530260` never loads `+332`) |
| Type-8 child skip when parent `+332` is 1 / 3 / 4 | **PROVEN** (`vtbl+260==8` then those three `cmp`) |
| Type-8 child skip when parent `+332` is **6** | **DISPROVEN** (6 is not 1/3/4; fall through to `+188`) |
| Default `vtbl+188` body is `0052CAF0` (ret 8, `+332=arg0`, child `vtbl+168`) | **PARTIAL** (ABI / no `E8`; rdata dword **UNREAD**) |
| `vtbl+188` is a wrapper that `call`s `vtbl+192` | **UNREAD** (not in `0052CAF0`; `.rdata` past listing) |
| Type-10/5/8 `.rdata` `vtbl+188` dword | **UNREAD** (`012497E4` / `01245DE4` / `012462E4` past `listing-01200000`) |

**State 6 on a type-10 does not skip child draw. Type 8
is not skipped on 6.**

`vtbl+188` is the **child select-forward** slot (state +
duration), not the draw slot. Parent `+332=6` still
issues that call to every owned non-type-8 child, and
to type-8 children too.

---

## 1. Forward site (`PROVEN`)

`0052CF40` `ret 4`. `ebp` = arg. Early-out if
`[this+332] == arg`. Else `mov [esi+332], ebp`, clear
`+316` style list, optional `vtbl+560/+564` when
`arg ≤ 6`, then `vtbl+540(arg)` / `vtbl+176(arg)`.

Non-animated arm (`vtbl+176` false) and every animated
arm that walks children use the same gate:

```
0052D050  child = [this+176][i]
          if child.vtbl+208 != this: skip          ; parent
          if child.vtbl+260 == 8:                  ; type
            if [this+332] == 1 || 3 || 4: skip
          child.vtbl+188( [this+332], [this+336] )
```

Same block at `0052D131`, `0052D220`, `0052D2D0`.
`+336` is duration (`[+320]` or style `+28`).

`0052CF40` never loads `+302` and never calls `vtbl+8`.

---

## 2. What `vtbl+188` is

**Call shape (PROVEN):** `push duration; push state;
call [edx+188]`. Two stack args, `this` = child.

**Role (PROVEN as not-draw):** type-10/5/12/18 draw is
`vtbl+8` `00530260`. `+188` here is a **field**
(second child vector, walked after `+176`) on that
path — a different meaning of “+188”. The **vtable**
slot `+188` in `0052CF40` is the 2-arg select
forward.

**Candidate body `0052CAF0` (PARTIAL):** no `.text`
`E8`/`jmp` to it (vtbl-only). `ret 8`. Writes
`[this+332] = arg0`, clears `+316`, `vtbl+540` /
`vtbl+176`, then walks `+176` with
`child.vtbl+168(state, arg1)`. It does **not**
`call [vtbl+192]` / `0052CF40`. So “`+188` → `+192`”
in `14-container.md` is **not** listing-proven.

Type 8 `vtbl+192` is `0053D3B0` (1 arg, then
`0052CF40`). Type 12 `0054CBF0` and type 18
`00547C90` also wrap `0052CF40`. Those run only if
someone calls **`+192`**, not if default `+188` is
`0052CAF0`.

`.rdata` `012497E4+188` / `01245DE4+188` /
`012462E4+188` sit past `listing-01200000`
(`0122CFFE`). **UNREAD**.

---

## 3. State 6 on type 10 does not skip child draw

Type 10 `vtbl+8` `00530260` (`ret 20`):

```
parent = child.vtbl+208
if parent != this && !child.vtbl+400: skip
if child.vtbl+420: skip                 ; twice
else child.vtbl+8(...)
```

Then the same tests on the `+188` **vector**. No
`[this+332]` / `[child+332]` in `00530260`–
`005303E0`. `vtbl+420` is `0052F1D0` = `[+302] & 1`
(persist `def+392` at `00533288` only).

`+332=6` is a style key on this object. It is not a
draw hide. Do not invent `Visible=false`.

---

## 4. Type 8 skip is 1/3/4 only — 6 does not skip

```
0052D070  call [edx+260]
          cmp eax, 8
          jne 0052D090          ; not type 8 → +188
0052D07B  mov eax, [esi+332]
          cmp eax, 1 / 3 / 4
          je  0052D0AF          ; skip +188
0052D090  call [edx+188]
```

Arg 6 fails all three `cmp`. Type-8 children **do**
get `vtbl+188(6, duration)`. First-seen `+332=0`
also fails the skip (`0052C730`).

Type-10 first-seen kids are types 5 / 18 / 12 / 6 /
32, not 8. The type-8 gate is unused on that tree
even when `+332=6`.
