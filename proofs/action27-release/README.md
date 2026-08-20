# Action 27 is `0055AE88[1]` = `0055AE01`, not persist release

Investigation only. No production `src/` edits.

Authority: dump `Fable.exe` `0055AD60` / table `0x55AE88` /
`0055AF60` / `0054DBC0` / `0054E280` / `0054DC30` /
`0055AEB0` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
`0042E3EE` in `listing-00400000.txt`;
`proofs/action26-subscribers/README.md`,
`proofs/type6-action28/README.md`,
`proofs/input-type10-mmb/README.md`,
`proofs/vtbl584-post-hop/README.md`,
`proofs/0055B9D0-post-dword/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE**.

Do not re-prove type 4 → action 26, type 6 = LMB up, type 10 =
RMB down (not MMB), or `0059A238` consume of `0xE5` / `0x126` / 15.

---

## Verdict

**`action26-subscribers` “action 26 only arms `+364`; persist 15 /
`0x126` is later action 27” is DISPROVEN.** Action 26 already
posts through outer `vtbl+584` / `0055AF60`. Action 27 is **not**
that hop.

`0055AD60` `jmp [0x55AE88+eax*4]` with `eax = action-26`. Table
dword **1** is **`0055AE01`**, not `0055ADB2`. Case 27 is
selected hover-in (`vtbl+592`, `[inner+384]=1`). It does **not**
call `0055AF60` and does **not** call `vtbl+524`.

| Claim | Status |
| --- | --- |
| `lea eax,[edi-26]` / `cmp eax,6` / `jmp [0x55AE88+eax*4]` | **PROVEN** |
| Table `[0] 7B AD 55 00` = `0055AD7B` (action 26) | **PROVEN** |
| Table `[1]` = `0055ADB2` (code-order “case 27”) | **DISPROVEN** — dword is `01 AE 55 00` = **`0055AE01`** |
| Action 27 calls `0055AF60` | **DISPROVEN** (no `E8`; `0055AF60` is the 0-arg `+584` body) |
| Action 27 calls `vtbl+524` | **DISPROVEN** — `0055AE01` is `call [outer.vtbl+592]` |
| Action 26 only arms; first persist post is 27 | **DISPROVEN** / **STALE** (`vtbl584-post-hop`) |
| `0055ADB2` (`fld` debounce + `push [esi+372]` / `vtbl+524`) is action 27 | **DISPROVEN** — that dword is table **`[5]` = action 31** |
| `0042E3EE` type 10 → `push 27` | **PROVEN** `0042E557` |
| `0042E3EE` type 6 → `push 28` | **PROVEN** `0042E498` |
| LMB-up action 28 is release-of-accept / persist post | **DISPROVEN** — 28 is unarm `vtbl+588` (`type6-action28`) |
| Type-10 **widget** `0054E280` action 27 is the same case | **DISPROVEN** — that is `0054E2B8` / `00597BF2(0)` |
| `vtbl+592` rdata dword = `0055AFD0` | **PARTIAL** (0-arg sibling; no rdata listing) |

**Does 27 call `0055AF60` / `vtbl+524`?** Neither.

**LMB-up type 6 / action 28 vs input type 10 / action 27:** different
physical buttons and different `0055AD60` slots. 27 is RMB-down
hover-in. 28 is LMB-up unarm. Persist 15 / `0x126` is action **26**
(`0055AF60` → `vtbl+524([widget+372])`), not 27 and not 28.

---

## 1. `0055AD60` switch (ecx = inner = `widget+4`)

```
0055AD60  push esi / edi
0055AD62  mov edi, [esp+12]          ; action
0055AD66  lea eax, [edi-26]
0055AD69  cmp eax, 6
0055AD6C  mov esi, ecx
0055AD6E  ja  0055AE79               ; 0055B9D0 only
0055AD74  jmp [0x55AE88+eax*4]
```

No index byte (unlike type-10 `0054E280` / `0x54E33C`). Slot
`eax` **is** `action-26`.

Type 11 inner `0054DBC0` only reaches this call when parent
`[def+545]` is set (`0054DC10`–`0054DC1C`). Type 38 apply is this
function directly.

---

## 2. Dump table `0x55AE88` — listing bytes, not code order

The listing decodes the seven dwords as junk (`jnp` / `push ebp` /
`add` / `scasb`). Linear sweep from `0055AE88` through the
`int3` pad at `0055AEA5` recovers the aligned dwords.

```
0055AE88  jnp 0055AE37          ; 7B AD
0055AE8A  push ebp              ; 55
0055AE8B  add [ecx], al         ; 00 01
0055AE8D  scasb                 ; AE
0055AE8E  push ebp              ; 55
0055AE8F  add dh, bl            ; 00 DE
0055AE91  lodsd                 ; AD
0055AE92  push ebp              ; 55
0055AE93  add [ebx-82], dl      ; 00 53 AE
0055AE96  push ebp              ; 55
0055AE97  add [ecx-82], bh      ; 00 79 AE
0055AE9A  push ebp              ; 55
0055AE9B  add [edx+536892845], dh
                                ; 00 B2 + disp 0x200055AD = AD 55 00 20
0055AEA1  scasb                 ; AE
0055AEA2  push ebp              ; 55
0055AEA3  add ah, cl            ; 00 | (next pad)
0055AEA5  int3
```

`536892845 = 0x200055AD` locks the `00 B2 AD 55 00 20` run.
Seven LE dwords (`0055AE88`…`0055AEA3`):

| `eax` | Action | Bytes | Dest |
| ---: | ---: | --- | --- |
| 0 | **26** | `7B AD 55 00` | **`0055AD7B`** |
| 1 | **27** | `01 AE 55 00` | **`0055AE01`** |
| 2 | **28** | `DE AD 55 00` | **`0055ADDE`** |
| 3 | **29** | `53 AE 55 00` | **`0055AE53`** |
| 4 | **30** | `79 AE 55 00` | **`0055AE79`** |
| 5 | **31** | `B2 AD 55 00` | **`0055ADB2`** |
| 6 | **32** | `20 AE 55 00` | **`0055AE20`** |

Prior notes (`type6-action28`, `input-type10-mmb`,
`vtbl584-post-hop`) assigned 27→`0055ADB2` / 29→`0055AE01` /
30→`0055AE20` / 31→`0055AE53` / 32→`0055AE70` from **.text
layout**, not from these dwords. That map is **STALE**.

`0055AE70` (timestamp then `0055B9D0`) is only a **join**, never a
table dest. Action 32 is `0055AE20`. Action 30 is the default
`0055AE79` (`0055B9D0` only).

If dword1 were `0055ADB2`, byte `0055AE8C` would be `B2` and
`0055AE8B` would disassemble as `add [edx+disp32], dh`, not
`add [ecx], al`. It does not.

---

## 3. Action 27 body `0055AE01` — not `0055AF60`, not `+524`

```
0055AE01  mov al, [esi+348]          ; inner+348 = widget+352
          test al, al
          je  0055AE70               ; stamp + 0055B9D0
          mov edx, [esi-4]
          lea ecx, [esi-4]           ; outer
          call [edx+592]             ; 0-arg
          mov [esi+384], 1           ; hover armed
          jmp 0055AE70
0055AE70  [esi+396] = [esi+44]
0055AE79  push edi
          call 0055B9D0              ; cmp arg,25 → vtbl+580; else ret 4
          ret 4
```

No `E8 0055AF60`. No `push [esi+372]`. No `call [vtbl+524]`.
`0055B9D0` is a no-op for action 27.

`0055AF60` is the 0-arg click used as **`vtbl+584`** (action 26
`0055AD8F`). It `push [outer+372]` / `call [vtbl+524]` then
`inner.vtbl+12(28)`. Action 27 never enters it.

Sibling `0055AFD0` is the same 0-arg shape with `[def+528]` /
`[outer+392]` / subscribe 29. That **ABI** matches `call [vtbl+592]`.
Slot pointer **PARTIAL** (no rdata). Even if `+592` is `0055AFD0`,
the list is **`+392` / `[def+236]`**, not the `+372` / `[def+224]`
list that carries 15 / `0x126` when that persist arm is filled.

---

## 4. What the other slots actually do

```
0055AD7B  ; 26
          if [esi+348]==0: 0055AE3D
          lea ecx,[esi-4]; call [vtbl+584]     ; 0055AF60
          [esi+364]=1
          0055B9D0

0055ADDE  ; 28
          if [esi+364]==0: 0055AE70
          lea ecx,[esi-4]; call [vtbl+588]
          [esi+364]=0
          → 0055AE70

0055AE53  ; 29
          if [esi+384]==0: 0055AE70
          lea ecx,[esi-4]; call [vtbl+596]
          [esi+384]=0
          → 0055AE70

0055AE79  ; 30
          0055B9D0 only

0055ADB2  ; 31
          fld [esi+44]; fsub [esi+396]; fcomp [esi+392]
          if debounce fail: 0055AE79
          if [esi+364]==0: 0055AE3D
          mov eax, [esi+372]                   ; INNER +372 = widget+376
          jmp 0055AE30

0055AE20  ; 32
          if [esi+384]==0: 0055AE3D
          mov eax, [esi+388]                   ; inner+388 = widget+392
          ; fall through
0055AE30  mov edx, [esi-4]
          lea ecx, [esi-4]
          push eax
          call [edx+524]
```

`0055B9D0-post-dword` is right that `push [inner+372]` is
**`widget+376`**, not the list `0055AF60` posts. That push is
action **31**, not 27.

`0042E3EE` has **no** `push 31` (`listing-00400000.txt`). Who
applies 31 on the first-seen frontend is **UNREAD** (local map
insert only: type 11 `0054DC87`, type 38 `0055AEC7`).

---

## 5. Input type 10 / action 27 vs type 6 / action 28

`0042E3EE` (`00A03B40` then):

```
0042E46A  cmp eax, 10
0042E473  je  0042E557
…
0042E481  dec / dec          ; type 6
0042E483  je  0042E498
0042E498  call 0041E5F2
          push 28
          jmp 0042E5AB       ; input.vtbl+0 = 0055CB10
0042E557  call 0041E5F2
          push 27
          jmp 0042E5AB
```

| Event | `[record+40]` | Physical (`input-type10-mmb` / `type6-action28`) | Action | `0055AD60` dest | Effect on type 11/38 |
| --- | ---: | --- | ---: | --- | --- |
| LMB down | 4 | `DIMOFS_BUTTON0` / `WM_LBUTTONDOWN` | **26** | `0055AD7B` | `vtbl+584` / `0055AF60` post `[widget+372]`; arm `[+364]` |
| RMB down | **10** | `DIMOFS_BUTTON1` / `WM_RBUTTONDOWN` | **27** | `0055AE01` | if `[+348]`: `vtbl+592`, `[+384]=1` |
| LMB up | 6 | BUTTON0 / LMB release | **28** | `0055ADDE` | if armed: `vtbl+588`, `[+364]=0` |
| RMB up | 12 | BUTTON1 up | *(none)* | — | `0042E560` arm does not keep 11/12 (`jne 0042E7F0`) |

Same `0055CB10` walk as 26 (`action26-subscribers`): focused
`[+8]` exclusive, else broadcast `+12` else `+4`.

Type 11 activate `0054DC30` local-maps **26, 31, 28, 27, 32, 29**.
Type 38 enable `0055AEB0` maps **26, 31, 27, 32** — **not 28**.
27 is on both maps. 28 is type-11 (and the `0055AF60` post-click
`vtbl+12(28)` on type 38).

First-seen `[+364]=0` (ctor `0055B460`). First-seen `[+348]` is
the selection byte: 0 → action 27 takes `je 0055AE70` and posts
nothing. That matches `input-type10-mmb` (no `0059A238` on first
RMB). A later 27 on a **selected** widget still does not take the
`+372` persist list.

---

## 6. Widget type 10 is a different 27

CUIDef / persist type 10 (`0054E280`) is not input type 10.

```
0054E2A2  lea eax, [ebx-26]
          cmp eax, 8
          movzx eax, [eax+0x54E33C]   ; 00 01 03 03 03 03 03 02 02
          jmp [0x54E32C+eax*4]
0054E2B8  ; index 1 = action 27
          push 0
          call 00595582
          call 00597BF2
          jmp 0054E319                ; debounce
```

Type-10 ctor does not `input.vtbl+8`. First-seen Press Start /
New Profile / Main Menu roots are therefore **not** `0055CB10`
nodes. If 27 were applied here anyway, slot `0x14` + arg 0 is a
no-op (`input-type10-mmb`). Attach `0xE5` stays action **26**
`0054E2FA`.

---

## 7. C# leftover (do not apply here)

| Site | Native | Host |
| --- | --- | --- |
| type 10 → 27 | `0042E55C` | `ActionType10=27` **MATCH** |
| type 6 → 28 | `0042E49D` | `ActionType6=28` **MATCH** |
| 27 → 15 / `0x126` / `0xE5` | no on 11/38/10 first-seen | `MessageFromWidgets` only action 26 **MATCH** |
| “27 = armed persist release” | table `[1]` is hover-in | **LEFTOVER** in `action26-subscribers` / `type6-action28` |

Do **not** treat action 27 as LMB-up. Do **not** post persist
15 / `0x126` from 27. Do **not** keep `0055ADB2` as case 27.

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
  (`0055AD60`, `0055AE88`…`0055AEA5`, `0055AF60`, `0055AFD0`,
  `0055B9D0`, `0054DBC0`, `0054DC30`, `0054E280`, `0055AEB0`)
- `listing-00400000.txt` (`0042E498` `push 28`, `0042E557` `push 27`)
- `proofs/action26-subscribers/README.md` (26 arms; **STALE** “later 27”)
- `proofs/type6-action28/README.md` (28 = unarm; **STALE** 27 = `0055ADB2`)
- `proofs/input-type10-mmb/README.md` (type 10 = RMB down → 27)
- `proofs/vtbl584-post-hop/README.md` (26 already posts via `0055AF60`)
- `proofs/0055B9D0-post-dword/README.md` (inner `+372` ≠ outer `+372`)
