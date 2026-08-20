# Who calls `NOVI_Barrel` `vtbl+20` `00DB7DB0`?

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent barrel smash physics, animation
events, health, or a breakable-object contact. The
first-seen WatchBarrels callback `00DBE890` **polls**
quest `+116`. It does **not** smash. The byte is
written **1** by `NOVI_Barrel` script `vtbl+20`
`00DB7DB0`. Who **calls** that slot was UNKNOWN
(`proofs/watchbarrels-00DBE890`). This note dumps
the writer and the nearby `call [reg+20]` sites.

Do **not** treat `00DB7E10` radius `2.0` as smash
distance. That start is instruction text.

Do **not** treat CTC `004C73E0` / thing `004C96FC`
or watcher `00CB79B4` as this slot.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00d80000.txt` `00DB7D00` /
`00DB7DA0` / `00DB7DB0` / `00DB7DF0` / `00DB7E10` /
`00DBE890`–`00DBEB16` / `00DAEA70` / `00DAAD70`;
`listing-00cc0000.txt` `00CDEBB0`–`00CDEE0A` /
`00CDD410`; `listing-00c80000.txt` `00CB7950` /
`00CBE2FF`; `listing-004c0000.txt` `004C73E0` /
`004C96F0` / `004C9D60` / `004C9F30`;
`listing-00f00000.txt` `00F35B10`–`00F35B84`;
`vtbl.tsv` `0x012D94EC` / `0x012D7A28`;
`e8.tsv` / `abs.tsv` / `calls-by-dest.tsv` /
`ff.tsv`; `src/Fable.Game/RegionTravel.cs`
`WatchBarrels*`; `ScriptFactoryTable.Barrel*`;
siblings `proofs/watchbarrels-00DBE890`,
`proofs/novi-factory-starts`,
`proofs/00DBDE40-host-gap`,
`proofs/collision-first-seen`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Who **E8**-calls `00DB7DB0`? | **Nobody.** Zero `.text` `E8`. `abs.tsv` has no `00DB7DB0` imm. | **PROVEN** |
| Who **vtbl**-calls `00DB7DB0`? | Still **UNKNOWN**. Slot is stored `0x012D94F0+20` only. | **UNREAD** |
| Is smash detect inside `00DBE890`? | **No.** Poll `[quest+116]`, local `edi`. | **DISPROVEN** |
| Is `00DB7E10` smash detect? | **No.** `00CBE2FF` r=`2.0` then break-barrels text. | **DISPROVEN** |
| Is `00CDEE00` the writer? | **No.** Event-1 thunk is `push 1; call [vtbl+0]` dtor. | **DISPROVEN** |
| Is CTC `004C73E0` / `004C96FC` this slot? | **No.** Thing+68 **component** `vtbl+20`. Barrel `+8` is a thing-ref, not a flag. | **DISPROVEN** |
| Is `00CB79B4` this slot? | **No.** Watcher `0x012D7A3C+20` = `00CDD410` (`ret`). | **DISPROVEN** |
| Smash interaction distance? | **None** in `00DB7DB0` / `00DBE890`. | **PROVEN** absent |
| What **is** the `2.0`? | `00DB7E10` instruction radius (`00CBE2FF` `dist^2 < r^2`). Not smash. | **PROVEN** as start; **DISPROVEN** as smash |
| Host childhood barrel deed? | Constants **MATCH**. Live fiber / writer / `00DAEA70` **absent**. | **MATCH** data; **PROVEN** host gap |
| First-seen takes the deed? | **No.** In-house, no control, beetle/deed branches later. | **DISPROVEN** / **LEFTOVER** |

---

## Verdict

**`00DB7DB0` is the `+116=1` writer. Its caller is
still UNREAD.** Do not name that site physics, anim,
health, or opcode.

`WatchBarrels` `00DBE890` is first-seen **on the
Oakvale fiber** (`00DBDE40` watcher 1 of 3). It
waits `vtbl+300("NOVI_Barrel")` then yield-loops
on `[quest+116]`. First rising edge calls
`00DAEA70(0)` (`DID_FIRST_BAD_DEED`). That is the
childhood barrel deed. Host does not run the fiber
and does not call `00DAEA70`. First-seen no-save
does not smash.

The only recovered **distance** on this object is
`NOVI_Barrel` **start** `00DB7E10`: context
`vtbl+280` (hero) vs thing-ref, `00CBE2FF`
`dist^2 < 2.0^2`, then
`TEXT_QST_048_INSTRUCTION_BREAK_BARRELS[_PC]`.
That is the “come close, show text” wait. It is
**not** the smash detector.

---

## Evidence → Original → Host → Gap

### 1. Dump `00DB7DB0` — **PROVEN** writer, **UNREAD** caller

Original (`listing-00d80000.txt`):

```
00DB7DB0  push esi
          mov esi, ecx
          mov ecx, [esi+20]          ; quest
          mov al, 1
          mov [ecx+116], al
          mov edx, [esi+20]
          lea ecx, [esi+8]           ; thing-ref
          mov [edx+117], al
          mov eax, [ecx]
          call [eax+24]              ; thing-ref vtbl+24
          mov ecx, [esi+20]
          add ecx, 118
          ; copy 12 bytes [eax]..[eax+8] → quest+118
          pop esi
          ret
```

No float. No radius. No health. No `E8`.
`functions.tsv` `0x00DB7DB0` callees empty except
inner `[eax+24]`. `e8.tsv` / `calls-by-dest.tsv`
dest `00DB7DB0`: **0**. `abs.tsv` `00DB7DB0`: **0**.

`vtbl.tsv` `0x012D94EC` (typeinfo; stored vtbl
`0x012D94F0`):

| disp | VA | Role |
|---|---|---|
| +0 | `00DB7DF0` | dtor → `00F35B40` |
| +4 | `00DB7E10` | start (radius + text) |
| +8 | `00CDEBB0` | `ret` |
| +12 | `00DB7DA0` | `return [this+20]` (quest) |
| +16 | `00CDEBC0` | `ret 4` |
| +20 | `00DB7DB0` | smash **notify** |
| +24 | `00CDEBE0` | `ret` |

`abs.tsv` plants `0x012D94F0` at **one** site:
`00DB7D34` inside factory `00DB7D00`. Siblings
(`NOVI_BarrelThug` `0x012D9234+20`,
`NOVI_CreatedBeetle` `0x012D9560+20`, base
`0x012C3208+20`) leave slot `+20` as `00CDEBD0`
(`ret`). Barrel **overrides** it.

Host (`RegionTravel` / `ScriptFactoryTable`):

| Const | Value | Class |
|---|---|---|
| `WatchBarrelsSmashFlagOffset` | 116 | **MATCH** |
| `WatchBarrelsSmashFlagWriter` / `BarrelSmashFlagWriter` | `00DB7DB0` | **MATCH** |
| `WatchBarrelsSmashFlagVtbl` / `BarrelSmashFlagVtbl` | 20 | **MATCH** |
| Comment “who calls the slot is UNREAD” | still true | **MATCH** unread |

Gap: host never **calls** the writer. Constants
only.

---

### 2. Dump factory `00DB7D00` — construct, not smash

```
00DB7D00  push 28 / 00BFEA1A
          [esi+4] = [ebx+64]         ; context
          [esi]   = 0x12C3224        ; base
          004ABE90 on [esi+8]        ; thing-ref
          [esi]   = 0x12D94F0        ; NOVI_Barrel vtbl
          [esi+20] = ebx             ; quest
          [esi+24] = extra
          alloc 12
          [eax]   = 1
          [eax+4] = 00CDEE00         ; event-1 release
          [eax+8] = object
```

`00CDEE00` (`listing-00cc0000.txt`):

```
00CDEE00  test ecx, ecx
          je  ret
          mov eax, [ecx]
          push 1
          call [eax]                 ; vtbl+0 dtor
          ret
```

**DISPROVEN** as `+116` writer. Factory registers
**only** event 1. No second record for `vtbl+20`.

`00F35B40` (barrel dtor tail) plants base
`0x12C3224` and tears down `+8`. Not a notify.

---

### 3. Dump `00DBE890` — poller, not detector

Attach is `00DBDE40` after `CREATURE_HERO_CHILD`
(`proofs/watchbarrels-00DBE890`). Fiber `00DAAD70`:

```
00DAAD70  mov ecx, [esi+56]     ; quest
          call [esi+52]         ; 00DBE890
          mov [esi+5], 1
```

Callback (`listing-00d80000` / ExeIndex
`watchbarrels-callback-00dbe890`):

```
vtbl+300("NOVI_Barrel") → 12-byte vector
N = (end-begin)/12
[esi+116] = 0
edi = 0
loop:
  if [esi+80]  → dtor vector, ret          ; AttackOver
  if [esi+116] == 0 → yield vtbl+28, loop
  inc edi
  [esi+116] = 0
  if edi == 1     → 00DAEA70(0)            ; first deed
  if edi == N-1   → vtbl+288 / vtbl+2340("OBJECT_GOLD_1")
  if edi >  N-4   → vtbl+364 beetle + vtbl+1064(..., 2.0f)
  yield, loop
```

**PROVEN** edge on `+116`. **DISPROVEN** physics /
anim / health inside this fn. Smash count is
**local** `edi`. Beetle is a later branch;
`FirstSeenWatchBarrelsSpawnsBeetle=false`.

Host: `WatchBarrelsCallback=00DBE890`, interval
`0.1f` / 64 / 1, thing `"NOVI_Barrel"` — **MATCH**
data. Live `00A446A0` → `00DAAD70` → `00DBE890`
does **not** run (`proofs/00DBDE40-host-gap`).
**PROVEN** host gap.

---

### 4. Dump `00DAEA70` — childhood barrel deed

```
00DAEA70  inc [esi+88]                 ; deed counter
          fld [0x143E90C+3428]; fchs
          context vtbl+624             ; morality (sign-flip)
          if [+88]==1 && [+84]==0:
            TEXT_QST_048_SCRMSG_DID_FIRST_BAD_DEED
            vtbl+460, wait vtbl+160
            TEXT_QST_LOG_BASICS_MAP
            [esi+252] = 1
          else if [esi+252]==0:
            TEXT_QST_048_SCRMSG_DID_BAD_DEED
```

WatchBarrels `push 0` so `+252` is on the quest.
`[+88]` is the deed count (also Guard wander).
Morality float at `vtbl+624` is **PARTIAL** (only
`fchs` recovered).

Host `src/Fable.Game`: **no** `00DAEA70`, **no**
`DID_FIRST_BAD_DEED`, **no** quest `+88` deed
increment. `TickNamedQuestMain` else-arm Notes
`00CB7950` + yield. Not this fn. **PROVEN** host
gap.

First-seen does not take it: in `HerosOldHouse`,
`FirstSeenHandsPlayerControl=false`, WatchBarrels
still `setle` on `vtbl+300` until barrels exist.
Deed / gold / beetle are **LEFTOVER** of the
Oakvale fiber, not first Present.

---

### 5. Interaction distance

| Site | Number | What it is | Smash? |
|---|---|---|---|
| `00DB7DB0` | — | flag + 12-byte copy | **no distance** **PROVEN** |
| `00DBE890` poll | — | `[+116]` byte | **no distance** **PROVEN** |
| `00DB7E10` start | `push 0x40000000` (`2.0f`) | `00CBE2FF` | **DISPROVEN** as smash |
| beetle leftover | `vtbl+1064(..., 2.0f)` | spawn scale/dist | **LEFTOVER** (`FirstSeenWatchBarrelsSpawnsBeetle`) |

`00DB7E10` (`listing-00d80000`):

```
push 0x40000000                 ; 2.0 stays on stack
call [context.vtbl+280]         ; returns other thing (hero)
mov edx, eax
mov ecx, ebp                    ; thing-ref at [this+8]
call 00CBE2FF                   ; ret 4 consumes 2.0
je  00DB7FB7                    ; not under radius → yield
… vtbl+24 (PC?) …
TEXT_QST_048_INSTRUCTION_BREAK_BARRELS[_PC]
vtbl+460, wait vtbl+160
```

`00CBE2FF` (`listing-00c80000`): both args
`vtbl+300` then pos `vtbl+24`;
`dx,dy,dz` → `dist^2`; `fcompp` vs `r*r`;
`fnstsw` / `test ah,0x41`; success iff
**strict** `dist^2 < r^2`. **PROVEN** opcode math
(`proofs/collision-first-seen`).

This `2.0` is the **instruction** radius on
barrel **start** `vtbl+4`. WatchBarrels never
calls `00CBE2FF`. `00DB7DB0` never reads a
position except the 12-byte copy **after** the
flag is already 1.

Do **not** invent a smash radius.

---

### 6. Nearby `call [reg+20]` — **DISPROVEN** as this slot

#### 6a. CTC attach `004C73E0` (only `E8` of that helper)

`e8.tsv`: one site, `004C9F30` inside named
component attach `004C9D60`.

```
004C9D60  … 00686F10 by name …
          push edi                  ; new CTC*
          mov ecx, ebp              ; Thing
          call 004C73E0
```

`004C73E0`:

```
esi = arg                         ; CTC*
if [esi+8] != 0: ret
[thing+148] & 7:
  1 → arg.vtbl+16
  2 → +16 then +20
  3 → +16, +20, +24
```

Arg is a **CTC component** from thing+68
(`{type, ptr}` records; copy helper `004CC290`
stride 8). `004C96FC` is the same walk on
`[esi+68]` then `call [ecx.vtbl+20]` after
`[ecx+8]==0`.

`NOVI_Barrel` `+8` is an **embedded** thing-ref
(`004ABE90`; dtor plants `0x1238C8C`). Low byte
`0x8C ≠ 0`, so this dispatcher would **skip**
even if someone passed the 28-byte script object.

**DISPROVEN** as `00DB7DB0`. Do not rename CTC
lifecycle smash.

#### 6b. Quest/watcher tick `00CB79B4` / `00CB79C6`

`00CB7950` (`listing-00c80000`): `[run+44]=arg`,
then `vtbl+4` start or `vtbl+20`. Callers are
`00CB7C57` / `00CB81A6` (quest Main walk), not
barrel construct.

Watcher stored vtbl `0x012D7A3C` =
`0x012D7A28+20`. Slot `+20` on that object is
`vtbl.tsv` `0x012D7A28` index 10 = `00CDD410`
(`ret`). Tick slot is `+16` = `00DAAD70`.

**DISPROVEN** as `00DB7DB0`.

#### 6c. SneakTo / creature `vtbl+20`

`00CC0E5A` / `004C72B0` `mov al,1; ret 4`.
Creature thing slot, not `0x012D94F0`.
**DISPROVEN.**

#### 6d. What remains

Any other `ff.tsv` `call [reg+20]` whose `ecx`
is the 28-byte `0x012D94F0` object. Not listed.
Until that `FF` site is named, caller stays
**UNREAD**. Do not fill it with physics.

---

### 7. Host map

| Piece | Original | Host | Class |
|---|---|---|---|
| Writer VA / slot / `+116` | `00DB7DB0` / 20 / 116 | same consts + tests | **MATCH** |
| Factory / start | `00DB7D00` / `00DB7E10` | `ScriptFactoryTable.Barrel*` | **MATCH** data |
| Watcher attach | `00DBDE40` `00CDD450` ×3 | `WatchBarrels*` consts | **MATCH** data |
| Live `00DBE890` | fiber `00DAAD70` | not run | **PROVEN** gap (`00DBDE40-host-gap`) |
| Invoke `00DB7DB0` | UNREAD `call [vtbl+20]` | none | **UNREAD** + gap |
| First deed `00DAEA70` | `inc +88`, first-bad text | **absent** | **PROVEN** gap |
| Instruction r=`2.0` | `00DB7E10` / `00CBE2FF` | not implemented | **LEFTOVER** start |
| First-seen smash / beetle / gold | later thresholds | `FirstSeenWatchBarrelsSpawnsBeetle=false` | **MATCH** omit |
| Player smash on first Present | needs control + barrels | `FirstSeenHandsPlayerControl=false` | **DISPROVEN** |

---

### 8. Gap (do not close by inventing smash)

1. **Caller of `0x012D94F0+20`.** Scan remaining
   `call [reg+20]` for `ecx` = this 28-byte
   script object. Not CTC `004C73E0`, not
   `00CB7950`, not `00CDEE00`.
2. **Live WatchBarrels.** Blocked on proven
   `Q_NewOakValeIntro` activate then a real
   `00DBDE40` fiber. Do **not** wire smash or
   `00DAEA70` into `Pump` / `TickNamedQuestMain`.
3. **Deed apply.** `00DAEA70` `vtbl+624` body
   and `+88` readers (Guard) are **PARTIAL** /
   follow-on. Not this gap.
4. **Do not** add a host smash helper, radius,
   health check, or anim-event from this note.

---

## Classifications (short)

1. **`00DB7DB0` writes `+116/+117` and copies
   12 bytes to `+118` — PROVEN.** Zero `E8`.
2. **Caller of barrel `vtbl+20` — UNREAD.**
   CTC / watcher / SneakTo / event-1
   **DISPROVEN**.
3. **`00DBE890` polls; `00DAEA70(0)` is first
   deed — PROVEN** as branches. First-seen does
   not take them (**LEFTOVER**).
4. **Interaction distance for smash — none
   PROVEN.** `2.0` is start-text `00CBE2FF`
   (**DISPROVEN** as smash).
5. **Host — MATCH constants, PROVEN gap** on
   fiber, writer invoke, and childhood deed.

---

## Next UNREAD

**The `FF` site that does `call [vtbl+20]` on a
`0x012D94F0` object.** That call is the smash
detector. Until it is listed, do **not** write
physics, collision, anim-event, or breakable
health.
