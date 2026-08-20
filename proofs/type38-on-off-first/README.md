# Type 38 ON/OFF first-seen — not `SelectsChild`

Investigation only. No production `src/` edits.

Question: type 38 `CAcceptButton` (ctor `00558B90`
vtbl `0124B04C`) on New Profile has children
`UI_SPRITE_ACCEPT_ON` and `UI_SPRITE_ACCEPT_OFF`
sharing dest `579,646,989,749`. Cancel has
`BACK_ON` / `BACK_OFF` at one dest. Host presents
both. Do **not** add type 38 to `SelectsChild`
without proof.

1. Type 38 `vtbl+8`. Is it `00530260`?
2. Who presents only ON or only OFF at first-seen?
   `+332`? `+348`? Armed? `SelectState`?
3. First-seen New Profile: `ACCEPT_ON` or
   `ACCEPT_OFF`?
4. Same for type 11?

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`00558B90` / `00558C70` / `00558D80` / `0054E0B0` /
`0054DB50` / `0054DC30` / `00558770` / `0055AEB0` /
`0055BAE0` / `0055BA20` / `0055ACB0` / `0055C0DE` /
`00551329`),
`listing-00500000.txt` (`00530260` / `0052C730` /
`0052CF40` / `005331A0` / `00533288`),
`listing-00400000.txt` (`0041AFA0`);
`out/00-index/sections.txt`;
`export/frontend/new-profile-dests.txt`,
`main-menu-dests.txt`;
`src/Fable.Formats/Defs/FrontendWidgetType.cs`;
`src/Fable.Game/FrontendWidgetFactory.cs`
`SelectsChild` / `IsPresented`;
`src/Fable.Game/EngineLifecycle.cs`
`DrawContainerWalk` / `CompositeFrontendPresent`;
`proofs/type16-18-present-child/README.md`;
`proofs/type11-plus352-select/README.md`;
`proofs/type38-vtbl284/README.md`;
`proofs/14-container.md` via
`implementer/frontend/14-container.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **MATCH** / **LEFTOVER**.

Do not re-prove type 16/18 persist-child-0,
`00530260` skip = `vtbl+400`/`vtbl+420`, or
type 11/38 `+352` as the click u8. Do not invent
the `.rdata` dword. `read_file` rejects
`Fable.exe`. `ExeIndex vtbl` was **not** run this
pass.

---

## Verdict

**Do not add type 38 (or type 11) to
`SelectsChild`.** That predicate is persist
**index 0** (type 18 `+332`, type 16 `+348`).
Type 38 persist child 0 is **`UI_ACCEPT_TEXT`**,
not ON/OFF. Accept vs Cancel even store ON/OFF
in **opposite** persist order.

Type 38 `0124B04C+8` is **UNREAD**. There is
**no** type-38-local draw in the `00558B90`
cluster. The only `.text` `E8 00530260` in
`listing-00540000` is type-2 `00551329`. Type 38
inherits type 5 (`0055BA20` → `0052CC50`) whose
`vtbl+8` **is** `00530260`. Candidate only.

First-seen type 38 does **not** exclusive-walk
`+176`. `+332=0` (`0052C730` via `00558D80` →
`0055B880`). Widget `+352` (inner `+348`) stays
**0** until `0055C0DE`. Armed `+364` stays **0**.
Enable `0055AEB0` / `SelectState([def+516])` is
**not** the ctor.

Host `IsPresented` draws **both** overlapping
sprites because `SelectsChild(38)==false`.
Which sprite native first-seen presents is
**UNREAD** (persist `+392` clip on the two type-0
defs not dumped; no type-38 hide walk).

| Claim | Status |
| --- | --- |
| Type 38 ctor `00558B90` → `0055B460` then `[esi]=0124B04C` | **PROVEN** |
| `0124B04C+8` dword is `00530260` | **UNREAD** (no `vtbl` dump) |
| Type 38 cluster contains a local `+176` draw | **DISPROVEN** |
| Type 5 `vtbl+8` = `00530260`; type 38 inherits type 5 then **overwrites the whole table** | **PROVEN** inherit; final slot **UNREAD** |
| `00530260` exclusive-walks `ActiveChild` / ON vs OFF | **DISPROVEN** |
| `SelectsChild(38)` as the ON/OFF picker | **DISPROVEN** |
| First-seen `+332=0` on type 38 (`00558D80` → `0052C730`) | **PARTIAL** (body is layout; rdata `+172` **UNREAD**) |
| Type 38 widget `+348` is type-16 selected-child | **DISPROVEN** (`0055BAE0` copies `+332`; inner `+348` is the `+352` u8) |
| Ctor / attach writes Accept `+352=1` | **DISPROVEN** |
| First-seen armed `+364=1` | **DISPROVEN** (ctor 0; action 26 skipped) |
| First-seen presented child is `ACCEPT_ON` | **UNREAD** |
| First-seen presented child is `ACCEPT_OFF` | **UNREAD** |
| Host `IsPresented` submits both dests | **MATCH** leftover vs intended exclusive |
| Type 11 New Profile / Main Menu has the same ON/OFF pair | **DISPROVEN** |
| Type 11 `01249554+8` = `00530260` | **UNREAD** |
| Add type 11 to `SelectsChild` | **DISPROVEN** (same index-0 shape; no ON/OFF kids) |

Dump still needed:

```
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x0124B04C 8
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x01249554 8
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x0124BD2C 8
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x0124BFB4 8
```

Slot `[2] +8`. Until those print, do **not** set
`DrawsChildList(38)` or `SelectsChild(38)`.

---

## 1. Persist tree (New Profile)

`export/frontend/new-profile-dests.txt` (factory
walk, persist children):

```
UI_ACCEPT_NEW_PROFILE     t=38  dest=579,672,579,672
  UI_ACCEPT_TEXT          t=6   dest=784,680,784,680
  UI_HELPER_BUTTON_MOUSE_AREA  t=0  dest=579,672,979,720  TS_BUTTON_R
  UI_SPRITE_ACCEPT_ON     t=0   dest=579,646,989,749  FE_BUTTON_ACCEPT_ON
  UI_SPRITE_ACCEPT_OFF    t=0   dest=579,646,989,749  FE_BUTTON_ACCEPT_OFF
UI_CANCEL                 t=38  dest=32,672,32,672
  UI_CANCEL_TEXT          t=6
  UI_HELPER_BUTTON_MOUSE_AREA  t=0
  UI_SPRITE_BACK_OFF      t=0   dest=32,646,442,749
  UI_SPRITE_BACK_ON       t=0   dest=32,646,442,749
```

Type 38 dest is **degenerate** (x0==x1, y0==y1).
The pixels are the type-0 children. ON and OFF
share one dest. Cancel stores **OFF then ON**;
Accept stores **ON then OFF**. Persist index is
not a stable ON=0 / OFF=1 map.

`SelectsChild` + `FirstSeenState=0` would keep
child **0** = `UI_ACCEPT_TEXT` / `UI_CANCEL_TEXT`
and hide the mouse area **and** both sprites.
That is not an ON/OFF toggle.

---

## 2. Type 38 `vtbl+8` — dword UNREAD

Ctor (`listing-00540000`):

```
00558B90  push def
          call 0055B460              ; type 34 → 33 → 5
          mov [esi],    0124B04C     ; outer
          mov [esi+4],  0124B024     ; inner
          mov [esi+24], 0124B01C
          ret 4
```

`0055BA20` (type 33) `call 0052CC50` then
overwrites to `0124BFB4`. Type 34 overwrites to
`0124BD2C`. Type 38 overwrites **again**. Runtime
`call [vtbl+8]` uses **`0124B04C+8` = `0124B054`**.

`.rdata` `rva=file=0xE2D000`. File of that VA is
`0xE4B054`. `listing-01200000.txt` ends in
`.text` (`0122CFFF`). `read_file` rejects the
exe. No `WriteVtblPart` of `0124B04C`.

Type 38 unique `.text` next to the ctor:

| VA | Body |
| --- | --- |
| `00558C10` | walk `vtbl+208` until type **10**; `vtbl+568` |
| `00558C70` | `0052CF40` then state switch (arg `≤6`) |
| `00558D80` | `jmp 0055B880` → `jmp 0052C730` |
| `00558D90` | inner apply; action 30 posts `+360` |

**No** `+176` walk. **No** `call 00530260`.
**No** `call 0041AFA0`.

`e8.tsv` `00530260` in this listing: **one** site,
`00551329`, inside type-2 (`0124A224`) draw
`ret 20` after its own child pass. Not type 38.

`00530260` itself (`listing-00500000`) walks
**every** owned `+176` then `+188` child. Skip is
`parent!=this && !vtbl+400`, then `vtbl+420`
twice. **No** `+332` / `+348` / `+352` / `+364`.

So: if the unread dword **is** `00530260`, native
first-seen still draws **both** sprites unless a
child `+302` bit 0 (persist `def+392` at
`00533288`) or `vtbl+400` is already set. Type 38
`.text` never writes `+302` and never walks
`+176` to hide a sibling.

If the dword is `0041AFA0`, Accept itself has
empty dest and the sprites would **not** be
reached from that call — first-seen Accept
graphics would vanish. That conflicts with a
visible button, but is **not** a listing proof
of the pointer.

**Answer to (1):** not proven `00530260`.
**PARTIAL** inherit-from-5. **UNREAD** rdata.

---

## 3. `+332` / `+348` / armed / `SelectState`

### `+332`

`00558D80` → `0055B880` → `0052C730`:
`+324/+328/+332=0`. Same first-seen zeros as
type 5/10/12. `00530260` does not load `+332`
(`proofs/0052CF40-selectstate-6`).

Type 38 `SelectState` **is** `00558C70`:

```
00558C70  push arg
          call 0052CF40              ; +332=arg; child vtbl+188
          cmp edi, 6
          ja  ret
          jmp [0x558CF4+…]           ; includes inner vtbl+12(25)
```

Ctor does **not** call it. `0052CF40` early-outs
when `+332` already equals the arg, so a later
`SelectState(0)` is a no-op.

Enable `0055AEB0` **does** call `0055BAE0`:

```
0055BAE4  mov eax, [esi+332]
          mov [esi+348], eax         ; dword copy of SelectState
          vtbl+432 → CUIDef*
          vtbl+192([def+516])        ; SelectState(persist +516)
          vtbl+524([+356])
```

Type 38 ctor never `E8 0055AEB0`
(`proofs/type38-subscribe-actions`). First-seen
enable on Accept is **UNREAD**. `def+516` on
`UI_ACCEPT_NEW_PROFILE` is **UNREAD** (INVISIBLE
file i32 is 3; do not copy).

### `+348` — two slots, neither is ON/OFF index

| Object | `+348` | First-seen |
| --- | --- | --- |
| Type 16 | selected **child index**; `00549B20` writes 0 | persist child 0 |
| Type 38 **widget** | `0055BAE0` copies **`+332`** (dword) | 0 if enable has not run |
| Type 38 **inner** (`widget+4+348` = `+352`) | selected **u8** click gate | ctor **0** (`0055BA4C`) |

`proofs/type11-plus352-select`: only `+352=1`
store in this family is `0055C0DE` (hit-test
take-selection). Attach does not arm it.

### Armed

Action 26 `0055AD7B`: if inner `+348` (`+352`) is
0, skip `vtbl+584` and do **not** set
`inner+364=1`. First-seen therefore **unarmed**.
`vtbl+532` after `+352` flips (`0055C0DE` /
`0055BDE0` / `0055C14D`) is **later**, not
construct.

**Answer to (2):** none of `+332` / type-38
`+348` / armed / first-seen `SelectState` is
recovered as an ON/OFF child pick. `SelectsChild`
is the wrong shape.

---

## 4. First-seen ON vs OFF

**UNREAD** which type-0 is presented.

What is recovered:

- Both sprites are persist children with the
  **same** dest (`new-profile-dests.txt`).
- Host `CompositeFrontendPresent` walks every
  widget with nonempty dest and
  `IsPresented`. `SelectsChild` is 16/18 only, so
  both sprites submit. That is the leftover the
  question names.
- `DrawContainerWalk` does **not** recurse type
  38 (`DrawsChildList` is 5/10/12/16/18). The
  overlapping submit is the **flat** present
  walk, not a type-38 `ActiveChild`.
- Native child skip at first-seen, if any, is
  persist `def+392` → widget `+302` bit 0
  (`00533288`) or `def+504` → `vtbl+400`.
  `persist-scan.txt` has **names** for
  `UI_SPRITE_ACCEPT_ON/OFF` and **no hex** for
  those blobs. Clip bytes **UNREAD**.
- Type 0 draw `0041AFA0` does not compare a
  parent ON/OFF name. It uses its own
  `+151/+368/+360`.

Do not claim first-seen `ACCEPT_OFF` because
`+352=0`. That byte is the click gate, not a
sprite index.

**Answer to (3):** not recovered. Do not invent
ON or OFF.

---

## 5. Type 11

Ctor `0054E0B0`: `0055B460` then `[esi]=01249554`.
`01249554+8` **UNREAD** (same dump gap).

New Profile `UI_NEW_PROFILE_BUTTON` (type 11)
children (`new-profile-dests.txt`): type 5 group,
type 2 tables, type 6 text, type 37 edit. **No**
`UI_SPRITE_*_ON/OFF`.

Main Menu `UI_FRONTEND_BUTTON_NEW_GAME` (type 11):
type 5, type 6, type 2 `UI_BUTTON_BIG`, type 0
`UI_BUTTON_MOUSE_AREA` (`TS_BUTTON_R`). **No**
Accept-style ON/OFF pair.

Type 11 layout `0054DB50`: if `[def+545]` then
`0055AC90`, else `0052C730`. `+545` on NEW_GAME
is **UNREAD**. Neither arm walks ON/OFF sprites.

Type 11 tick `00558770` (uses `+404/+408`; type 11
size `0x1B4`, **not** type 38 size `0x194`):
on first tick, if `+404==0` and `[def+408]`,
`0041D21B` construct and append `+176`. Same for
`+408` / `[def+412]`. That is a **cached extra
child**, not persist `ACCEPT_ON`. File
`[def+408]` on NEW_GAME **UNREAD**.

Activate `0054DC30` maps 26/31/**28**/27/32/29
when `[def+545]`; it does not hide sprites.

**Answer to (4):** not the same ON/OFF pair.
Do not add type 11 to `SelectsChild`. `vtbl+8`
still **UNREAD**.

---

## 6. Host leftover (not a native proof)

`FrontendWidgetType.SelectsChild` = type 18/16
only. `ApplyFirstSeenState` hides `k!=0` only
there. `IsPresented` then submits every remaining
nonempty dest. Type 38 children stay visible →
both ON and OFF.

Making `SelectsChild(38)` true would hide the
wrong child (text). Making `DrawsChildList(38)`
true without a recovered `vtbl+8` is also
unproven.

---

## Leftovers

- `.rdata` `0124B04C+8` / `01249554+8` /
  `0124BD2C+8` / `0124BFB4+8`.
- Persist `+392` / `+504` on
  `UI_SPRITE_ACCEPT_ON`, `ACCEPT_OFF`,
  `BACK_ON`, `BACK_OFF`.
- Whether first-seen `0055AEB0` runs on Accept.
- `0124B04C+172` dword vs `00558D80`.
- `0124B04C+532` body after `+352` flips
  (later hover, not this first-seen attach).
- Type 11 `[def+408]/[def+412]` first-tick child.

Until the `vtbl` dwords print, type 38 ON/OFF
stays **UNREAD** as a present-child, and
**DISPROVEN** as `SelectsChild`.
