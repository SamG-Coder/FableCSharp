# Type 9 is not RMB up; `0042E3EE` maps it to nothing

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `00A03E10` / `00A03EC0` / `00A03D90` /
`00AB4910` / `00AB4BB0` / `00AB5420` / `0042E3EE` / `0055CB10`;
listings `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a00000.txt`,
`listing-00a80000.txt`, `listing-00400000.txt`, `listing-00540000.txt`,
`listing-00980000.txt`;
`proofs/type7-action35/README.md`,
`proofs/type4-dinput-raw/README.md`,
`proofs/type13-vs-type4/README.md`,
`proofs/host-input-type4/README.md`;
`src/Fable.Game/FrontendInputMap.cs`,
`src/Fable.Game/EngineInput.cs`,
`src/Fable.Client/Program.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

Do not re-prove type 4 → action 26 / `0xE5`, type 13 → action 25, or
type 7 → action 35 widget bodies.

---

## Verdict

**No.** `00A03E10` is event type 9, but type 9 is **middle-mouse up**,
not RMB up. `0042E3EE` does **not** map type 9 to any action. It is
the unused **release** sibling of type 7 / action 35.

| Claim | Status |
| --- | --- |
| `00A03E10` writes `[record+40]=9`, `[+32]=3` | **PROVEN** |
| Sole `.text` caller of `00A03E10` is `00AB55D7` inside `00AB5420` | **PROVEN** |
| Type 9 is RMB up | **DISPROVEN** — RMB up is raw **5** → `00A03EC0` type **12** |
| Type 9 is MMB / BUTTON2 / `WM_MBUTTON*` **up** | **PROVEN** DINPUT + Win32 |
| `0042E3EE` type 9 → action | **DISPROVEN** — `jne 0042E7F0`, no `push`, no `0055CB10` |
| Type 9 shares type 7’s action 35 | **DISPROVEN** — only type **7** reaches `0042E48C` `push 35` |
| Type 9 is the up-pair of type 7 (same button) | **PROVEN** raw 6 vs raw 3 |
| First-seen Press Start / New Profile / Main Menu emit type 9 | **DISPROVEN** unless the user releases MMB; even then classifier no-ops |
| Host queues type 9 | **DISPROVEN** — RMB is debug look; no `QueueInput` for 9 |
| C# `ActionFromEvent(9)` | **MATCH** `null` |

RMB up is type **12**. RMB down is type **10** / action **27**.

---

## 1. `00A03E10` is event type 9 (device 3)

```
00A03E10  mov eax, [esp+4]
          fld qword [esp+8]
          mov [ecx+32], 3
          mov [ecx+40], 9
          ; [+24]/[+28] from ptr
          fstp [ecx+48]
          fld qword [esp+16]
          mov [ecx+28], eax
          fstp [ecx+44]
          ret 20
```

Same **up** shape as type 6 (`00A03D60`, LMB up) and type 12
(`00A03EC0`, RMB up): two timestamp qwords, `ret 20`. Down siblings
(`00A03C80` / `00A03D90` / `00A03E40`) are `ret 12` and `fst`/`fstp`
the same double into `+48`/`+44`.

`functions.tsv` / listing: the only `call 00A03E10` is `00AB55D7`.

---

## 2. Who builds type 9 (not RMB)

`00AB5420` `[esi+8]` = raw, `lea ecx, [eax-1]`, index `0xAB56EC`,
jump `0xAB56C4` (bytes already recovered in `type4-dinput-raw`):

| `[esi+8]` | Site | Ctor | `[record+40]` |
| ---: | --- | --- | ---: |
| 1 | `00AB54F0` | `00A03C80` | 4 LMB down |
| 2 | `00AB553E` | `00A03E40` | 10 RMB down |
| 3 | `00AB5517` | `00A03D90` | 7 MMB down |
| 4 | `00AB5590` | `00A03D60` | 6 LMB up |
| **5** | `00AB55EE` | **`00A03EC0`** | **12 RMB up** |
| **6** | **`00AB55BF`** | **`00A03E10`** | **9 MMB up** |

### DINPUT (`00AB4910`)

`dwOfs` 12/13/14 = `DIMOFS_BUTTON0/1/2`.

| Button | `dwOfs` | Site | down raw | up raw | down type | up type |
| --- | ---: | --- | ---: | ---: | ---: | ---: |
| LMB | 12 | `00AB4A72` `add 4` | 1 | 4 | 4 | 6 |
| **RMB** | **13** | `00AB4A8D` `add 5` | **2** | **5** | **10** | **12** |
| **MMB** | **14** | `00AB4AA2` `add 6` | **3** | **6** | **7** | **9** |

BUTTON2 (`00AB4AA2`): `and dl, 0x80; neg; sbb; and -3; add 6` then
`mov [edi+8], edx`. Down → 3, up → 6 → `00A03E10`.

BUTTON1 (`00AB4A8D`): same mask, `add 5`, store at `00AB4B26`.
Up → 5 → `00A03EC0` type **12**, never type 9.

### Win32 (`00AB4BB0` when `[this+13372]≠1`)

Getters: LMB `009A4FC0` `[+221]`, MMB `009A4FD0` `[+222]`,
RMB `009A4FE0` `[+223]`. Edge vs `[this+13356/+13357/+13358]`:

| Slot | Encode | down / up raw | types |
| --- | --- | --- | --- |
| LMB `+221` | `dec; neg; sbb; and 3; inc` | 1 / 4 | 4 / 6 |
| MMB `+222` | `dec; neg; sbb; and 3; add 3` (`00AB4EFA`) | **3 / 6** | **7 / 9** |
| RMB `+223` | `dec; neg; sbb; and 3; add 2` (`00AB4F4C`) | **2 / 5** | **10 / 12** |

Both backends agree: type 9 is **middle up**. Calling it RMB up is
**DISPROVEN**.

RTTI: one class `CInputTypeMouseButtonEvent`. Button identity is
`[record+40]`, not a distinct ctor family.

---

## 3. `0042E3EE` mapping for type 9: none

`00A03B40` then (`listing-00400000.txt`):

```
cmp eax, 17
jg  0042E67E
je  0042E608          ; 17 analog
cmp eax, 10
jg  0042E560          ; 13 / 14 / 15
je  0042E557          ; 10  push 27
dec eax
je  0042E4B0          ; 1   last-key + push 33
sub eax, 3
je  0042E4A4          ; 4   push 26
dec eax / dec eax
je  0042E498          ; 6   push 28
dec eax
jne 0042E7F0          ; *** type 9 lands here ***
0042E48C  call 0041E5F2
          push 35     ; only type 7
          jmp 0042E5AB
```

Walk type **9**:

```
9  < 17, < 10
dec → 8          not type 1
sub 3 → 5        not type 4
dec dec → 3      not type 6
dec → 2          jne 0042E7F0
```

`0042E7F0` is the next `009F4F10` record. No `0041E5F2`, no `push`,
no `0042E5AB` `call [edx]` (`0055CB10`).

Type **12** (actual RMB up) takes the `>10` arm (`sub eax,13` / two
`dec`) and also `jne 0042E7F0`. Release events 9 and 12 are both
silent on this pump.

Dispatched siblings on the same chain:

| `[+40]` | Site | Action |
| ---: | --- | ---: |
| 4 LMB down | `0042E4A4` | 26 |
| 6 LMB up | `0042E498` | 28 |
| **7 MMB down** | **`0042E48C`** | **35** |
| **9 MMB up** | **`0042E7F0`** | **none** |
| 10 RMB down | `0042E557` | 27 |
| 12 RMB up | `0042E7F0` | none |

---

## 4. Related to type 7 / action 35?

**Same mouse button, opposite edge. Not the same action.**

| | Type 7 | Type 9 |
| --- | --- | --- |
| Ctor | `00A03D90` `ret 12` | `00A03E10` `ret 20` |
| Raw | 3 (down) | 6 (up) |
| Device | 3 | 3 |
| `0042E3EE` | `push 35` → `0055CB10` | skip |
| First-seen menus | no 10/11/38 subscriber for 35 (`type7-action35`) | never reaches `0055CB10` |

`0055CB10` is the input singleton `vtbl+0` (`01230134`). It only
runs when `0042E5AB` is reached. Type 9 never gets there, so no
listener accept/apply, no type-10 `0054E280`, no type-11/38
`0055AD60`. Action 35’s only recovered subscriber is the options
`CKeyRedefiner` (`00557AF0` / `00557EB0`), and that path is the
**down** (type 7), not the up.

Do not treat type 9 as “action 35 release”. The classifier does not
pair them.

---

## 5. Frontend first-seen

Press Start / New Profile / Main Menu trees have no MMB producer
on an idle first frame. Host `Program.cs` queues type 1 (keys) and
type 4 (LMB edge). RMB is `looking = debugFly && MouseButton.Right`
only — never `QueueInput`. Middle button is unread.

If a native MMB-up did arrive on those screens:

```
00AB5420  raw 6 → 00A03E10  type 9
0042E3EE  jne 0042E7F0
0055CB10  not called
```

No `0xE5` / `0x126` / 15 / `UI_CANCEL`. Same no-op as a first-seen
type 7 that has no redefiner subscriber, except type 9 does not even
broadcast 35.

---

## 6. C# leftover (no `src/` change)

| Site | Native | Host |
| --- | --- | --- |
| `00A03E10` type 9 | MMB up | no constant |
| `ActionFromEvent(9)` | no action | **MATCH** `null` |
| `ActionFromEvent(7)=35` | **MATCH** | unused by `MessageFromWidgets` |
| `EngineInput` comment “Type 7 is RMB down” | **DISPROVEN** (`type7-action35`) | leftover comment |
| `EngineInput.ApplyEvent` type 9 | skip | skip (**MATCH**) |
| `EngineInput.ApplyEvent` type 7 | `0055CB10(35)` | **dropped** (existing leftover) |
| Live RMB | type 10 / 12 | debug look only |

Do not map RMB up → type 9. RMB up → type 12 → no `0042E3EE` action.

---

## Sources

- `listing-00a00000.txt` (`00A03E10`, `00A03EC0`, `00A03D90`)
- `listing-00a80000.txt` (`00AB4910` `00AB4A8D`/`00AB4AA2`,
  `00AB4BB0` `00AB4EFA`/`00AB4F4C`, `00AB5420` `00AB55BF`)
- `listing-00400000.txt` (`0042E453`–`0042E7F0`)
- `listing-00540000.txt` (`0055CB10`)
- `listing-00980000.txt` (`009A4FC0` / `009A4FD0` / `009A4FE0`)
- `proofs/type7-action35/README.md` (type 7 ≠ RMB; action 35
  subscribers)
- `proofs/type4-dinput-raw/README.md` (raw → ctor table)
