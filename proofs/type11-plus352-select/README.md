# Type 11/38 `+352` is a selected u8, not the type-10 attach slot

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listing
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055AD60` / `0055AF60` / `0054E4F0` / `0054E0B0` / `00558B90` /
`0055B460` / `0055BA20` / `0055B040` / `0055BF10` / `0055C0DE` /
`0054DBC0` / `0054DC30` / `0055AEB0` / `00558C70`);
`proofs/0055B9D0-post-dword/README.md`;
`proofs/type11-msg15/README.md`;
`proofs/type10-plus352/README.md`;
`proofs/type12-highlight-plus348/README.md`;
`proofs/who-posts-0x126/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove persist CRC `0x53C644E4` → file i32 `0x126` / 15.
Do not treat type-10 `&widget+352` (`0054E2FA`) as the type 11/38
id that `0055AF60` posts.

---

## Verdict

**`[inner+348]` / `widget+352` on type 11/38 is a selected `u8`.
It is not the type-10 attach message slot. It is not both.**

The numeric offset `352` is shared. The objects are not. Type 10
stores a packet* dword there (`0054E4F0`). Type 33/34/11/38 stores
a one-byte click gate there (`0055BA20` `mov [esi+352], al`).
Action 26 reads that byte and skips `0055AF60` when it is 0.

| Claim | Status |
| --- | --- |
| `0055AD60` `ecx` is inner = `widget+4` | **PROVEN** |
| Action 26 `mov al,[esi+348]` / `test al` is **`widget+352` u8** | **PROVEN** |
| Zero → skip outer `vtbl+584` / `0055AF60` | **PROVEN** |
| Type-10 `0054E4F0` writes packet* at type-10 `+352` | **PROVEN** (`type10-plus352`) |
| Type 11/38 persist id lives at **`+372` list** (`0055B520`) | **PROVEN** (`0055B9D0-post-dword`) |
| Type 11/38 `+352` holds `0x126` / 15 / attach `0xE5` | **DISPROVEN** |
| Type 11 ctor `0054E0B0` / type 38 ctor `00558B90` write `+352=1` | **DISPROVEN** (they only inherit the type-33 **zero**) |
| Only `+352=1` store on this family is `0055C0DE` | **PROVEN** in `listing-00540000.txt` |
| `0055C0DE` is inside 0-arg `0055BF10` (hit-test / take selection) | **PROVEN** body; vtbl slot **PARTIAL** (no rdata, no `E8`) |
| First-seen New Profile Accept / Main Menu New Game attach writes `+352=1` | **DISPROVEN** |
| First-seen those widgets have `+352≠0` before the click | **UNREAD** (ctor left 0; `0055BF10` must have run) |

**Answer:** selected flag. Not the type-10 attach message slot.

**Who writes `+352` before the first-seen click:** type-33 ctor
`0055BA20` writes **0**. Nothing in construct / persist / screen
attach writes **1** on `UI_ACCEPT_NEW_PROFILE` or
`UI_FRONTEND_BUTTON_NEW_GAME`. The only recovered `=1` is
`0055C0DE` when `0055BF10` takes selection.

---

## 1. Action 26 gate is a byte, not a packet*

`0055AD60` (`ecx` = inner; type 38 inner `0124B024+4` and type 11
`0054DBC0` both land here):

```
0055AD62  mov edi, [esp+12]       ; action
0055AD66  lea eax, [edi-26]
0055AD69  cmp eax, 6
0055AD6C  mov esi, ecx             ; inner = widget+4
0055AD6E  ja  0055AE79
0055AD74  jmp [0x55AE88+eax*4]
0055AD7B  mov al, [esi+348]        ; widget+352
          test al, al
          je  0055AE3D             ; no vtbl+584
          lea ecx, [esi-4]
          call [eax+584]           ; 0055AF60
          [esi+364] = 1
          call 0055B9D0            ; nop for 26
```

`mov al` / `test al` is a **u8**. Type-10 action 26 is
`mov eax,[edi+348]` / `test eax` / `lea esi,[edi+348]` /
`push esi` → UI `vtbl+32` (`0054E2FA`). Same inner displacement,
different width and consumer.

If the byte is 0, `0055AF60` never runs and persist `0x126` / 15
never reaches `0059A238` on that apply (`0055B9D0-post-dword`).

---

## 2. Same offset, two layouts

### Type 10 (Press Start attach) — message packet*

Ctor `0054E3D0` size `0x16C`. `012497E4+284` = `0054E4F0`:

```
0054E4F0  eax = arg                 ; &{packet*, ctrl*}
          ebx = [eax]
          edi = [eax+4]
          …
0054E530  mov [esi+352], ebx        ; packet*
0054E536  mov [esi+356], edi        ; ctrl*
```

Attach `00598EE6` writes `0xE5` into the **packet**, then slot
`0x14` `vtbl+284`. `+352` is never the immediate `0xE5`.

New Profile / Main Menu type-10 roots are **not** that attach.
Their persist `+224` is 0. `0054E4F0` is **not** on type 11/38
vtbl `01249554` / `0124B04C`.

### Type 33/34/11/38 — selected u8

Type 33 ctor `0055BA20` (type 34 `0055B460` always calls it):

```
0055BA29  call 0052CC50
          xor eax, eax
          [esi]     = 0124BFB4
          [esi+4]   = 0124BF90
          [esi+24]  = 0124BF88
0055BA46  mov [esi+348], eax        ; dword (not the gate)
0055BA4C  mov [esi+352], al         ; u8 gate = 0
          [esi+356] = 0
          [esi+360] = 0
          input.vtbl+8(inner)
```

Type 34 then zeros `+364…+392` and `0055B040` copies `[def+224]`
through `vtbl+284` onto the list at **`+372`** (`0055B520`).
Destructor `0055B760` frees that list. There is no type-10-style
dword at `+352`.

`0055ACB0` (type-34 tick wrapper) also loads `+352` as a **byte**
(`mov al,[ecx+352]`).

---

## 3. Type 11 / 38 ctors do not arm the gate

Type 11 `0054E0B0` (`UI_FRONTEND_BUTTON_NEW_GAME`, alloc `0x1B4`):

```
0054E0B8  call 0055B460             ; zeros +352
          [esi]    = 01249554
          [esi+4]  = 01249530
          [esi+24] = 01249528
          ; +408…+432 extra lists
          call 0054DF50             ; def+196, not +224 / not +352
```

Type 38 `00558B90` (`UI_ACCEPT_NEW_PROFILE`, alloc `0x194`):

```
00558B98  call 0055B460             ; zeros +352
          [esi]    = 0124B04C
          [esi+4]  = 0124B024
          [esi+24] = 0124B01C
          ret 4
```

No store of `+352` after the base ctor. Persist 15 / `0x126` is
already on `+372` before any click. The gate stays **0**.

Activate / select-state also do **not** write it:

| Site | When | Writes `+352`? |
| --- | --- | --- |
| Type 11 `0054DC30` | `vtbl+192(3)` then local 26/31/28/27/32/29 | **no** |
| Type 11 `0054DCC0` | `vtbl+192(4)` + erase | **no** |
| Type 38 `0055AEB0` | `0055BAE0` then local 26/31/27/32 | **no** (`0055BAE0` copies `+332` → **`+348`**) |
| Type 38 `00558C70` | `vtbl+192`; may `inner+12(25)` | **no** |
| Type 11 tick `0054DB50` | `0055AC90` if `def+545` | **no** |
| Type 38 tick `00558770` | `0055AC90` first | **no** |

`0054DE70` (type 11, `def+545`) **clears** `+352` after `vtbl+576`.

---

## 4. The only `+352=1` writer is `0055C0DE`

Grep of `listing-00540000.txt` for `mov [esi+352], 0x01`:
**one hit**, `0055C0DE`. No second `=1` in the type-33 cluster.

That store sits in 0-arg `0055BF10`:

```
0055BF19  call 0041E5F2
          test [input+164]; jne leave
          inner.vtbl+8(25); je leave
          ; if input+184 (type-32 pointer) present, build a rect
0055C00C  mov al, [esi+352]
          test al, al
          jne already                 ; keep 1
          call [vtbl+568]             ; hit-test
          je  fail
          call 0055BB40               ; lose to a higher widget on 0x13B8AD4
          jne fail
          call [vtbl+572]
0055C0DE  mov [esi+352], 0x01
          call [vtbl+532]
          ; insert self on 0x13B8AD4
```

Peer already-selected widgets get `+352=0` at `0055C0BA`. Leave
path `0055C14D` also writes 0. `0055BDE0` is `+352=0` then
`vtbl+532` (type-34 deactivate `0055AF33` calls it).

No `.text` `E8 0055BF10` (`e8.tsv` empty). Dispatch is a **vtbl
call**. Slot id **PARTIAL**. Body is “take selection if action-25
accept and hit-test wins”, not “store persist id”.

---

## 5. First-seen New Profile Accept

Tree (`01-widget-construction.md`):

```
UI_FRONTEND_NEW_PROFILE_SCREEN          type 10
└── … UI_NEW_PROFILE_MENU               type 12
    └── helpers include
        UI_ACCEPT_NEW_PROFILE           type 38   persist 0x126
```

Before the first action-26 click:

1. `00558B90` → `0055B460` → `0055BA20` → **`+352 = 0`**.
2. `0055B040` boxes `0x126` onto **`+372`**. Does not touch `+352`.
3. Type-10 root is **not** Press Start `00598EE6` / `0054E4F0`.
   Even if it were, that dword lives on the **type-10** object.
4. `0055AEB0` / `00558C70` may subscribe 26 (and sometimes 25).
   Neither stores `+352=1`.

So the Accept widget’s gate is still the ctor **0** until
`0055BF10` runs. First-seen attach does **not** arm the click.

Whether a prior type-13 / pointer tick has already taken
selection on Accept is **UNREAD** (no first-seen call of
`0055BF10` recovered). If the byte is still 0, action 26 is
`0055B9D0` only.

---

## 6. First-seen Main Menu New Game

```
UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE     type 10
└── UI_FRONTEND_LIST_MAIN_MENU_…                   type 12
    └── [0] UI_FRONTEND_BUTTON_NEW_GAME            type 11   persist 15
```

Before the first action-26 click:

1. `0054E0B0` → `0055B460` → `0055BA20` → **`+352 = 0`**.
2. Persist 15 is on **`+372`**, not `+352`.
3. List highlight index `list+348` is forced **0** at attach
   (`0054D6F1`, `type12-highlight-plus348`). That is a **different
   object** and a **dword index**, not the type-11 gate.
4. Attach `0054D660` does **not** call child `vtbl+192(3)`.
   Type-11 `0054DC30` would still not write `+352`.
5. Type-10 `0054E280` is not a first-seen `0055CB10` node
   (`action26-subscribers`). Its `+352` packet* is 0 on this
   screen anyway.

Same as Accept: the New Game **button** `+352` is ctor 0 until
`0055C0DE`. List highlight 0 does not substitute for that byte.

---

## 7. What is **not** this slot

| Object | `+352` meaning | Relation |
| --- | --- | --- |
| Type 10 | packet* (`0054E4F0`) | **DISPROVEN** as type 11/38 gate |
| Type 11/34/38 | u8 selected (`0055BA20` / `0055C0DE`) | **this** |
| Type 8/12 | copy of `+48` (`0054D6E8`) / highlight is **`+348`** | different widget |
| Type 12 list `0055AD60` this-adjust | would be **list** `+352` if the list inner applies 26 | not persist 15 (`type12-action26`) |

Do **not** store type-11/38 `MessageId` at C# offset 352.
Do **not** treat `0054E4F0` as type 11/38 `vtbl+284`.

---

## 8. C# leftover (do not apply here)

`FrontendInputMap.Type10StoredMsgOffset = 352` is the type-10
packet*. `MessageFromWidgets` posts the first visible type
10/11/38 `MessageId` on action 26 with **no** `+352` gate.

Native first-seen 15 / `0x126` require that gate ≠ 0. Factory
copy of persist `+224` is the **`+372` list**, already **MATCH**
as the posted id once `0055AF60` runs.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0055AD7B` | action 26: `u8 [inner+348]` then `vtbl+584` | **PROVEN** |
| `0055AF60` | posts `[widget+372]`, never loads `+352` | **PROVEN** |
| `0054E4F0` | type-10 packet* at `+352` | **PROVEN**; **DISPROVEN** for type 11/38 |
| `0054E0B0` | type 11 ctor | **PROVEN**; no `+352=1` |
| `00558B90` | type 38 ctor | **PROVEN**; no `+352=1` |
| `0055B460` / `0055BA20` | type 34/33; `+352 = 0` | **PROVEN** |
| `0055B040` / `0055B520` | persist → `+372` | **PROVEN** |
| `0055BF10` / `0055C0DE` | only `+352=1` | **PROVEN** store; slot **PARTIAL** |
| First-seen attach writer of `+352=1` on Accept / New Game | — | **DISPROVEN** |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv` (no `0055BF10`)
- `proofs/0055B9D0-post-dword/README.md`
- `proofs/type11-msg15/README.md`
- `proofs/type10-plus352/README.md`
- `proofs/type12-highlight-plus348/README.md`
- `proofs/type12-action26/README.md`
- `proofs/who-posts-0x126/README.md`
- `proofs/action26-subscribers/README.md`
- `implementer/frontend/01-widget-construction.md`
