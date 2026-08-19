# Type 11 `0055BA20` / `0054DC30` first-seen subscribe is not “26 and 28”

Investigation only. No production `src/` edits.

Authority: dump `Fable.exe` `0054DC30` / `0054DCC0` / `0055AEB0` /
`0055AEF0` / `0055BA20` / `0054E0B0` / `0055B460` / `0055A5B0` /
`00557860` / `00557880` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
`0052DA20` in `listing-00500000.txt`;
`e8.tsv` (`0055BA20`, `0055AEB0`; **no** `0054DC30`);
`proofs/action26-subscribers/README.md`;
`proofs/action28-after-26/README.md`;
`proofs/cuidef-plus545/README.md`;
`proofs/type11-plus352-select/README.md`;
`proofs/type11-msg15/README.md`;
`proofs/newgame-plus380-first/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE**.

Do not re-prove type 4 → `push 26`, type 6 → `push 28`,
`0055CB10` broadcast when `[input+8]==0`, persist CRC
`0x53C644E4` → 15 / `0x126`, or `0x9E47F106` → `CUIDef+545`.

---

## Verdict

**No.** First-seen type 11 does **not** subscribe only actions
26 and 28.

Two different “subscribe” sites:

| Site | Role | Action ids first-seen |
| --- | --- | --- |
| `0055BA20` (type 33 ctor; type 11/34/38 go through here) | input `vtbl+8(inner)` → `0055CB10` **list node** | **none** |
| `0054DC30` (type 11 activate) | inner `vtbl+12` local map, if `[CUIDef+545]` | **26, 31, 28, 27, 32, 29** |
| `0055AEB0` (type 34/38 **enable**, not type 11) | same local `vtbl+12`, no `+545` | **26, 31, 27, 32** — **no 28**, **no 29** |

`type11-msg15` “`call [eax+12]` = `0055CB10` subscribe” is
**STALE** / **DISPROVEN**: that slot is local-map insert
`0052DA20` (`action26-subscribers`).

| Claim | Status |
| --- | --- |
| Type 11 ctor `0054E0B0` → type 34 `0055B460` → `0055BA20` | **PROVEN** |
| `0055BA20` `push`es 26 or 28 / calls inner `vtbl+12` | **DISPROVEN** |
| `0055BA20` registers `widget+4` via `0041E5F2` + input `vtbl+8` | **PROVEN** |
| `0054DC30` local-maps **26, 31, 28, 27, 32, 29** | **PROVEN** (gated on `[def+545]`) |
| That list is **only** 26 and 28 | **DISPROVEN** |
| Inner `vtbl+12` is `0052DA20` insert (26/28 = insert only; 25 also apply) | **PROVEN** shape; rdata dword **PARTIAL** |
| `e8.tsv` `E8 0054DC30` | **DISPROVEN** (vtbl only) |
| First-seen INVISIBLE `[CUIDef+545]` | **1** (`cuidef-plus545`) |
| First-seen NEW_GAME `[CUIDef+545]` | **UNREAD** |
| First-seen attach/layout **calls** `0054DC30` | **DISPROVEN** (`newgame-plus380-first` §5) |
| A later first-seen screen-show activate hits `0054DC30` | **UNREAD** (no `E8`; vtbl slot **PARTIAL**) |
| Type 11 first-seen uses enable `0055AEB0` | **DISPROVEN** (no `E8` from `0054E0B0` / `0055BA20`) |
| `0055AEB0` list is 26, 31, 27, 32 | **PROVEN** |
| `0055AEB0` already maps 28 | **DISPROVEN** |

**Answer:** ctor (`0055BA20`) subscribes **no** action ids.
Activate (`0054DC30`) maps **six** ids, **including** 26 and
28, **plus** 31 / 27 / 32 / 29, and only if `[def+545]`.
Enable `0055AEB0` is **26, 31, 27, 32** and is **not** the
type-11 first-seen list.

---

## 1. Dump `0055BA20` — list register, no action ids

Type 11 factory `0054E0B0` (`listing-00540000.txt`):

```
0054E0B0  push def
          mov  esi, ecx
          call 0055B460             ; type 34
          [esi]    = 01249554
          [esi+4]  = 01249530
          [esi+24] = 01249528
          … +408…+432 …
          call 0054DF50             ; Action CRC, not a 26-map
          ret 4

0055B460  call 0055BA20
          [esi]    = 0124BD2C       ; live only until type 11 overwrite
          … zero +364…+392 …
          call 0055B040             ; persist +224/+228 lists
          ret 4
```

Type 33 ctor:

```
0055BA20  mov  eax, [esp+4]
          push esi / edi
          push eax
          mov  esi, ecx
          call 0052CC50             ; type 5 → children first
          xor  eax, eax
          lea  edi, [esi+4]         ; inner
          mov  [esi],    0x124BFB4
          mov  [edi],    0x124BF90
          mov  [esi+24], 0x124BF88
          mov  [esi+348], eax
          mov  [esi+352], al        ; selected u8 = 0
          mov  [esi+356], eax
          mov  [esi+360], eax
          call 0041E5F2             ; input*
          mov  edx, [eax]
          push edi
          mov  ecx, eax
          call [edx+8]              ; input.vtbl+8(inner)
          pop  edi
          mov  eax, esi
          pop  esi
          ret  4
```

No `push 26`. No `push 28`. No `call [inner.vtbl+12]`.
First-seen type 11 is a `0055CB10` **node** from this call.
It does **not** yet own a local action set.

Sibling copy-ctor `0055BA80` is the same register, same
zeros.

---

## 2. Dump `0054DC30` — type 11 activate local map

`ecx` = **outer**. No `.text` `E8` (`e8.tsv` empty). Slot
identity **PARTIAL** (no `01249554` rdata); shape matches
type-8/12 activate `0053D540`.

```
0054DC30  push ecx / ebx / esi
          mov  esi, ecx
          call [eax+432]            ; this CUIDef*
          mov  bl, [edx+545]
          … COM-ptr release …
          test bl, bl
          je   0054DCB2             ; skip everything
          push 3
          call [edx+192]            ; SelectState(3)
          add  esi, 4               ; inner
          push 26
          call [eax+12]
          push 31
          call [edx+12]
          push 28
          call [eax+12]
          push 27
          call [edx+12]
          push 32
          call [eax+12]
          push 29
          call [edx+12]
0054DCB2  pop  esi / ebx / ecx
          ret
```

Order in the listing is **26, 31, 28, 27, 32, 29**.

Deactivate `0054DCC0` is the erase twin (`push 4` /
`vtbl+16`, same six ids).

`cuidef-plus545`: the gate is **this** def `+545` persist
`u8` (`0x9E47F106`), not the parent list. First-seen
`UI_FRONTEND_BUTTON_INVISIBLE` file byte is **1**.
`UI_FRONTEND_BUTTON_NEW_GAME` file byte is **UNREAD**.

If activate runs with `+545==1`, type 11 already local-maps
**28** (LMB-up). That is **not** “26 and 28 only”.

Attach/layout does **not** call this body
(`newgame-plus380-first`: type-12 `vtbl+192(3)` is a
different path, actions 25/22/4). Whether a later
first-seen show-walk hits the type-11 activate slot is
**UNREAD**.

---

## 3. Dump `0055AEB0` — enable list (not type 11)

```
0055AEB0  push esi
          mov  esi, ecx
          call 0055BAE0             ; copy +332 → +348; SelectState([def+516])
          add  esi, 4
          push 26
          call [eax+12]
          push 31
          call [edx+12]
          push 27
          call [eax+12]
          push 32
          call [edx+12]
          pop  esi
          ret                       ; ends; 28/29 absent
```

Disable `0055AEF0` erases the **same four** via `vtbl+16`.
Sibling `0055AF30` erases **28** and **29** only — those
ids are added later (`0055AF60` `push 28` /
`inner.vtbl+12`, not this enable).

`.text` callers of `0055AEB0` (`e8.tsv`):

| Site | After the call |
| --- | --- |
| `00557863` | `vtbl+192(3)` |
| `00557883` | `vtbl+192(4)` |
| `0055A5B0` | `jmp [input+184].vtbl+596` (type-35 cluster) |

None of those are type 11 ctor / first-seen Main Menu /
Press Start (`0055A640-fn`, `type7-action35`). Type 11
overwrites vtbl after `0055B460`; it does **not** `E8`
`0055AEB0`.

So: **do not** treat the enable list as the type-11
first-seen map. Type 38 `UI_ACCEPT_NEW_PROFILE` enable is
this four-id set (`action28-after-26`).

---

## 4. `vtbl+12` is local map `0052DA20`

`listing-00500000.txt`:

```
0052DA20  ; ecx = inner, arg = action
          lea  esi, [edi+4]         ; tree at inner+4
          call 0052DF20             ; find
          call 0052E230             ; insert if missing
          cmp  [esp+28], 25
          jne  0052DA65             ; 26/27/28/29/31/32: insert only
          call [inner.vtbl+4](25)   ; action 25 only
          ret  4
```

Identity of type-11 inner `01249530+12` as this body is
**PARTIAL** (no rdata dword). Same call shape as type 38
enable and type 8/12 activate.

`0055CB10` is input `vtbl+0` (apply walk). Ctor
`0055BA20` uses input `vtbl+8`. Activate/enable use inner
`vtbl+12`. Three slots.

---

## 5. First-seen type 11 vs “26 and 28?”

Physical producers (`0042E3EE`): type 4 → 26 (LMB down),
type 6 → 28 (LMB up). Those ids **are** on the type-11
activate map, but they are **not** the whole map, and they
are **not** what `0055BA20` writes.

| When | Type 11 local 26 | Type 11 local 28 | Type 38 local 26 | Type 38 local 28 |
| --- | --- | --- | --- | --- |
| After ctor `0055BA20` | no | no | no | no |
| After activate `0054DC30` if `+545` | **yes** | **yes** (+ 31, 27, 32, 29) | n/a | n/a |
| After enable `0055AEB0` | n/a | n/a | **yes** | **no** (+ 31, 27, 32 only) |
| After a later selected 26 (`0055AF60`) | insert 28 again (idempotent) | already there | insert 28 | **first** 28 |

Ctor leaves `[widget+352]=0`. First-seen action 26 on type
11 still reaches `0054DBC0` (INVISIBLE `+545==1`) but skips
`0055AF60` (`type11-plus352-select`). That skip does not
change the **subscribe** set.

Press Start first `0055CB10` node is INVISIBLE
(`action26-subscribers`). Main Menu first node is
`UI_FRONTEND_BUTTON_NEW_GAME`. Both are type 11 → this
ctor. Neither ctor-maps 26 or 28.

---

## 6. C# leftover

Host `FrontendInputMap` does not keep a per-widget local
action tree. Native type 11 after a successful
`0054DC30` accepts 26 **and** 31/28/27/32/29 on that map.
Native type 38 after `0055AEB0` does **not** accept 28
until `0055AF60`. Do not collapse both to “26 and 28”.

---

## Do not invent

- `0055BA20` as an action-id subscribe.
- Type-11 first-seen map = **only** 26 and 28.
- `0055AEB0` as type-11 enable / first-seen list.
- Inner `vtbl+12` as `0055CB10`.
- `0054DC30` as an `E8` from ctor or attach layout.
- NEW_GAME `+545` without a persist dump.
- Lionhead names for the six action ids beyond the
  already-proven type-4 / type-6 / type-10 producers
  (26 / 28 / 27).
