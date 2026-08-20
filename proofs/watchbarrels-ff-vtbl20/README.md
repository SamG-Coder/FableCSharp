# FF `call [vtbl+20]` after live kill — does it hit `00DB7DB0`?

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent barrel smash physics, radius, anim,
health, or LMB-near-barrel. Smash is a **live Thing
kill** (`004C9B80`). `WatchBarrels` only **polls**
`[quest+116]`. Writer is `NOVI_Barrel` `vtbl+20`
`00DB7DB0`. Direct `E8 00DB7DB0` is 0.

Question: the exact `FF` site that does
`call [vtbl+20]` on a stored `0x012D94F0` object.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00c80000.txt` `00CB7900` /
`00CB7950`–`00CB79F6` / `00CB7C40` / `00CB7E50` /
`00CB8170` / `00CB8220` / `00CB8960`;
`listing-00f00000.txt` `00F35A00` / `00F35A30` /
`00F35AA0`; `listing-004c0000.txt` `004C9B80`;
`listing-00d80000.txt` `00DB7D00` / `00DB7DB0`;
`ff.tsv` `00CB79B4` / `00CB79C6`; `e8.tsv`
`00F35A00` (2 hits); `calls-by-dest.tsv`
`00CB7950` / `004C9B80` / `00CB8960`;
`vtbl.tsv` `0x012D94EC` / `0x012D7A28`;
`src/Fable.Game/ScriptFactoryTable.BarrelSmash*`;
`RegionTravel.WatchBarrelsSmash*`;
siblings `proofs/watchbarrels-smash-vtbl20`,
`proofs/oakvale-childhood-objectives`,
`proofs/watchbarrels-00DBE890`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Exact `FF` gone-path sites? | **`00CB79B4`** (`FF 52 14`) and **`00CB79C6`** (`FF 50 14`) inside `00CB7950`. | **PROVEN** |
| Any other `E8` of `00F35A00`? | **No.** Only `00CB796B` and `00CB799C`. | **PROVEN** |
| Does `004C9B80` itself `call [vtbl+20]`? | **No.** Dead bits + drop lists. | **PROVEN** |
| Is `00CB79B4` WatchBarrels watcher `+20`? | **No.** That vtbl is `0x012D7A3C+20` = `00CDD410` `ret`. | **DISPROVEN** |
| Is `00CB79B4` CTC `004C73E0` / `004C96FC`? | **No.** | **DISPROVEN** |
| Does `esi` at those `FF`s listed as `0x012D94F0`? | **Not in the listing.** Candidate walk is `00CB81A6` `[run+8]+8`. | **UNREAD** ecx |
| Host `BarrelSmashCaller=00CB7950`? | Same dispatcher const. | **MATCH** |
| Radius `2.0` / `00DB7E10`? | Instruction text. | **DISPROVEN** as smash |

---

## Verdict

**The gone-dispatcher `FF` sites are `00CB79B4` and
`00CB79C6`.** Those are the only `call [reg+20]`
after `00F35A00` reports the bound object gone.
`004C9B80` is the live kill, not the call.

`call [esi.vtbl+20]` is `00DB7DB0` **iff** `esi`
stored vtbl is `0x012D94F0`. The `00CB7C40` walk
passes Main watchers — **DISPROVEN** as that vtbl.
The `00CB8170` walk (`00CB81A6`) passes
`[record+8]` from the name table `00CB8960`
fills. That is the **candidate** for the 28-byte
`NOVI_Barrel` pointer. No insn at the `FF` plants
or compares `0x012D94F0`. **ecx at the `FF` stays
UNREAD.** Do not invent smash to close it.

---

## Evidence → Original → Host → Gap

### 1. `ff.tsv` near `00CB7950` / `00F35A00`

```
0x00CB79B4  call [edx+20]  disp=20  fn=0x00CB7900
0x00CB79C6  call [eax+20]  disp=20  fn=0x00CB7900
```

Walker tags `00CB7900` (nearby). Body is
`00CB7950` (`listing-00c80000.txt`).

`e8.tsv` dest `00F35A00`: **exactly two**

| Site | In |
|---|---|
| `00CB796B` | first gone-check |
| `00CB799C` | gone-check after `vtbl+4` start |

No third `00F35A00`. No `FF` `+20` in `004C9B80`.

---

### 2. Original — `00CB7950` gone path

```
00CB7950  esi = arg                  ; script
          [this+44] = esi            ; run+44
          if [esi+40]  → clear +44, al=0, ret
          call 00F35A00              ; 00CB796B
          if al == 0:                ; GONE
            [esi+5] = 1
            00F35A30                 ; clear +44/+48
            call [esi.vtbl+20]       ; 00CB79C6  FF 50 14
          else if [esi+41] == 0:
            call [esi.vtbl+4]        ; START
            call 00F35A00            ; 00CB799C
            if al == 0:              ; GONE after start
              [esi+5] = 1
              00F35A30
              call [esi.vtbl+20]     ; 00CB79B4  FF 52 14
```

`00F35A00` (`listing-00f00000.txt`):

```
00F35A00  ecx = [ecx+44]             ; bound ptr
          if ecx == 0: al = 1        ; no bind = not-gone
          else jmp [ecx.vtbl+0]      ; alive?
```

`00F35A30` zeros `[this+44]` and `[this+48]`.

**PROVEN** as the only gone → `vtbl+20` dispatcher.
Slot is **arg** `vtbl+20`, not Thing `vtbl+20`, not
CTC `vtbl+20`.

---

### 3. Original — live kill `004C9B80` (not the `FF`)

```
004C9B80  if already +145 bit0 / +146 bit2 / +145 bit6: ret
          or [thing+146], 4
          optional vtbl+52 / vtbl+44
          vtbl+48
          or [thing+145], 1          ; dead
          004C8C00
          005202B0; 0051E000         ; drop world lists
          ret 4
```

Zero `call [reg+20]`. Kill sets bits. Later
`00CB8220` → `00CB7C40` / `00CB8170` → `00CB7950`
may see gone. **PROVEN** kill. **DISPROVEN** as
the `FF` site.

`Remove` opcode `00CD0116` → `vtbl+432`
`008910D0` → this fn (`RegionTravel.RemoveInner`).
Player attack/use may also `E8` it (many
`calls-by-dest` hits). Those are legal **later**
kills. Not a radius heuristic.

---

### 4. Who is `esi` at the `FF`?

Only `E8` of `00CB7950`:

| Site | Walk | Arg |
|---|---|---|
| `00CB7C57` | `00CB7C40` `[run+4]` circular | `[node+8]` |
| `00CB81A6` | `00CB8170` `[run+8]` vector | `[record+8]` |

`00CB8220` is `00CB7C40` then `jmp 00CB8170`.

`00CB7C40` nodes come from `00CB7E50` (WatchBarrels
`00DBDFA9` / gold / markers). Watcher stored vtbl
`0x012D7A3C`. `+20` = `00CDD410` `ret`.
**DISPROVEN** as `00DB7DB0`. This is the
`watchbarrels-smash-vtbl20` §6b result. It stays.

`00CB8170` walks the same `[+8]` vector
`00CB8960` fills on construct (`004AFAD3` /
`004B271E`, `ecx` = name-table / run). Factory
`00DB7D00` plants **`0x012D94F0`** on the 28-byte
inner object (`abs.tsv` one site `00DB7D34`).
Whether `[record+8]` **is** that inner pointer
at `00CB81A6` is **not** a listed `mov`. Name-table
hit path also reads `[esi+60]` (`00CB899F`), which
does not fit a 28-byte alloc. **UNREAD** whether
the `00CB7950` arg is the `0x012D94F0` object or a
larger wrapper whose `+20` is something else.

`00DB7DB0` is **only** `vtbl.tsv` `0x012D94EC`
slot 6 = stored `0x012D94F0+20`. No second vtbl.

---

### 5. Host

| Const | Value | Listing | Class |
|---|---|---|---|
| `BarrelSmashCaller` | `00CB7950` | gone dispatcher | **MATCH** fn |
| `BarrelThingGoneFn` | `00F35A00` | only from that fn | **MATCH** |
| `BarrelKillFn` / `RemoveInner` | `004C9B80` | live kill | **MATCH** |
| `BarrelSmashFlagWriter` | `00DB7DB0` | `0x012D94F0+20` | **MATCH** |
| `WatchBarrelsSmashHasDistance` | false | no float in writer | **MATCH** |
| `RegionTravel` “`FF` site still UNREAD” | — | ecx at `00CB79B4` | **MATCH** unread |

Host does **not** run `00CB7950` on a live
`NOVI_Barrel` (`00DBDE40-host-gap`). Do not
`quest+116=1` from Pump.

---

### 6. Gap

1. **`esi` at `00CB79B4` / `00CB79C6`.** Prove or
   disprove it is the 28-byte `0x012D94F0` object
   (or a wrapper that **is** that vtbl / forwards
   `+20`). Candidate: `00CB81A6` `[record+8]` after
   `00CB8960` `00DB7D00`. Not the `00CB7C57`
   watcher.
2. **`00F35A00` `jmp [bound.vtbl+0]`.** Bound ptr
   is `[arg+44]` from `00F35AA0` after start
   `00F35B10`. Alive vs dead body **UNREAD** as a
   concrete Thing slot (do not invent health).
3. Do **not** close with radius / LMB / kickable /
   action 26–28. Those stay **DISPROVEN**
   (`oakvale-childhood-objectives`).

---

## Classifications (short)

1. **Gone-path `FF` sites are `00CB79B4` and
   `00CB79C6` — PROVEN.** Only `call [vtbl+20]`
   after `00F35A00`.
2. **`004C9B80` is live kill, not the `FF` —
   PROVEN.**
3. **Watcher `00CB7C57` → `00CDD410` — DISPROVEN**
   as `00DB7DB0`.
4. **`ecx` = `0x012D94F0` at those `FF`s — UNREAD.**
   `00CB81A6` is the leftover candidate.
5. **Host dispatcher consts — MATCH.** Live invoke
   still absent.

Until `esi` at `00CB79B4` is listed as the barrel
object, do **not** write a smash helper.
