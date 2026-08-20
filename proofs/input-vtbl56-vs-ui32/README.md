# Input vtbl+56 (`0041E6D3`) vs UI vtbl+32 (`0059A238`)

Investigation only. No production `src/` edits.

Authority: `Fable.exe` dumps
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041E6D3` / `0041E3F6` / `0042E3EE`),
`listing-00540000.txt` (`0055CB10` / `0054E2FA` / `0054DBC0` /
`0055AD60` / `00540320` / `00558DE0`),
`listing-00580000.txt` (`0059A238` / `00595582`);
`functions.tsv` sizes; `src/Fable.Game/FrontendInputMap.cs`,
`EngineInput.cs`, `EngineLifecycle.cs`;
`implementer/frontend/05-input.md`;
`proofs/action26-subscribers/README.md`,
`proofs/type10-plus352/README.md`,
`proofs/list-type12-focus/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE**.

Do not re-prove type 4 is LMB, Return ≠ `0xE5`, or Leave /
`FinalAlbion.wld`.

---

## Verdict

`0042E3EE` / `0055CB10` is **not** “eventually `0041E6D3`”.
It is a **different path** from UI vtbl+32 `0059A238`.

| Claim | Class |
| --- | --- |
| Input vtbl `01230134+0` apply is `0055CB10` | **PROVEN** (`0042E5AF` `call [edx]`) |
| Input vtbl `01230134+56` is `0041E6D3` | **PROVEN** (`FrontendInputMap.InputVtblMessageFn`) |
| `0042E3EE` type 4 → `push 26` → **vtbl+0**, never +56 | **PROVEN** |
| `0055CB10` only `listener.vtbl+8` then `vtbl+4` | **PROVEN** |
| Type-10 apply `0054E2FA` posts `&widget+352` to **UI vtbl+32** | **PROVEN** |
| That hop is `00595582` then `call [edx+32]` = `0059A238` | **PROVEN** |
| `0055CB10` / `0054E2FA` call `0041E6D3` | **DISPROVEN** |
| `0059A238` is the frontend UI message consumer (`012521A8+32`) | **PROVEN** |
| `0041E6D3` with `[0x13B86A0]==0` **forwards** the same packet to UI vtbl+32, then still runs its own switch | **PROVEN** |
| First-seen Press Start type 4 calls `0059A238` (via `0054E2FA` if type 10 apply runs) | **PROVEN** site |
| First-seen Press Start type 4 calls `0041E6D3` | **DISPROVEN** |
| List `005403D2` (key 1) / `005405ED` (key 28) call input vtbl+56 | **PROVEN** sites; first-seen Press Start **DISPROVEN** (empty `+352`; type 1 not type 4) |
| Type-38 `00558DE0` also calls vtbl+56 | **PROVEN** site; no type 38 on Press Start |
| `docs/PARITY.md` “`0041E6D3` is the consumer” of posted `0xE5` | **STALE** — consumer is `0059A238`; `0041E6D3` is an alternate fan-in |

**First-seen Press Start:** type 4 → `0055CB10(26)` → type-10 `0054E2FA` → `0059A238`(`0xE5`). `0041E6D3` is not on that call stack.

---

## 1. Two slots on the same input object

Ctor `0041E3F6`:

```
0041E400  call 0042BE7B
0041E405  mov [esi], 0x1230134      ; vtbl 01230134
```

`0041E5F2` returns `[0x13B8710]` (that object).

| Slot | Offset | Callee | First-seen Press Start |
| --- | ---: | --- | --- |
| Apply action | vtbl+0 | `0055CB10` | type 4 `push 26` |
| Post packet | vtbl+56 | `0041E6D3` | not used |

`functions.tsv`: `0041E6D3` size **11396** (in-game GUI fan-in).
`0059A238` size **673** (frontend UI switch).

---

## 2. Dump `0042E3EE` — type 4 is vtbl+0

```
0042E456  call 00A03B40            ; type = [record+40]
…
0042E47C  sub eax, 3
0042E47F  je 0042E4A4              ; type 4
0042E4A4  call 0041E5F2
0042E4A9  push 26
0042E4AB  jmp 0042E5AB
…
0042E5AB  mov edx, [eax]
0042E5AD  mov ecx, eax
0042E5AF  call [edx]               ; vtbl+0 = 0055CB10
```

Type 1 (Return / any key) is `push 33` into the **same** `call [edx]`.
Neither arm is `call [edx+56]`.

---

## 3. Dump `0055CB10` — no `0041E6D3`

```
0055CB10  this = esi
0055CB14  if [esi+8] != 0:          ; focused
              listener.vtbl+8(action)   ; accept
              listener.vtbl+4(action)   ; apply
              return
          else:
              walk list +12 else +4
              same accept then apply
```

Zero `E8 0041E6D3`. Zero `call [reg+56]`. Apply is the widget **inner**
vtbl+4 (`012497BC+4` = `0054E280` for type 10; `0054DBC0` for type 11;
`0055AD60` for type 38).

---

## 4. Dump `0054E2FA` — posts UI vtbl+32, not input vtbl+56

Type-10 inner apply `0054E280`, action 26 = case 0:

```
0054E2FA  mov eax, [edi+348]       ; inner+348 = widget+352 packet*
0054E300  test eax, eax
0054E303  lea esi, [edi+348]
0054E309  je 0054E318
0054E30B  call 00595582            ; UI singleton [0x13B8B5C]
0054E310  mov edx, [eax]
0054E312  push esi                 ; &packet*
0054E313  mov ecx, eax
0054E315  call [edx+32]            ; 012521A8+32 = 0059A238
```

`00595582` is the UI getter (`mov eax,[0x13B8B5C]`, ctor `005953E2`
vtbl `012521A8`). This is the recovered Press Start user post of
attach-stored `0xE5` (`00598EE6` / `0054E4F0`).

Type-11 `0054DBC0` action 26 only `call 0055AD60` if parent `+545`.
`0055AD60` case 0 is `call [outer+584]` then arm `[+364]=1` — **no**
`00595582`, **no** `0041E6D3`, **no** UI vtbl+32.

---

## 5. Dump `0059A238` — the posted-message consumer

```
0059A281  mov eax, [ebp+8]         ; &packet*
0059A284  mov eax, [eax]           ; packet*
0059A286  mov ecx, [eax]           ; id
…
0059A2C5  je 0059A2DA              ; 15 → [retail+41]=1
…
0059A6BE  sub ecx, 0xE5
0059A6C4  je 0059A77F              ; call 00599D5C
…
0059A6E5  …                        ; 0x126 → 00851920
```

This is UI vtbl+32. First-seen Press Start id is **`0xE5`**.

---

## 6. Dump `0041E6D3` — parallel fan-in, not the type-4 walk

```
0041E6D3  push ebp
          lea ebp, [esp-116]
          sub esp, 0x800
0041E6E6  mov edi, [ebp+124]       ; arg = &packet* (same shape)
0041E6EC  mov eax, [edi]
0041E6EE  mov al, [eax+12]
0041E6F1  test al, al
0041E6F5  je 00426DFC              ; dead packet
0041E6FB  mov esi, [0x13B86A0]     ; game singleton
0041E701  test esi, esi
0041E703  jne 0041E718             ; in-game: skip UI hop
0041E705  call 00595582
0041E70C  push edi
0041E70F  call [edx+32]            ; same 0059A238
0041E712  mov esi, [0x13B86A0]
0041E718  mov ecx, [ebp+124]
0041E71B  mov edi, [ecx]
0041E71D  mov eax, [edi]           ; id — huge in-game switch
```

So `0041E6D3` **can** consume a posted pair on frontend by calling
`0059A238`, then **also** switches on the id itself (`cmp eax, 0xD8`,
`jmp [0x426E0E+…]`, later another `00595582` / vtbl+32 at `0042083A`).
That does **not** put it on the `0042E3EE` type-4 stack.

Who **does** `call [edx+56]` with `ecx` = `0041E5F2()`:

| Site | When | First-seen Press Start |
| --- | --- | --- |
| `005403EF` | type-12 key **1** (Escape), walk `[list+352]` | **DISPROVEN** — sentinel empty (`[head]==head`) |
| `005405ED` | type-12 key **28** (Return), walk `[list+348]` | **DISPROVEN** as type 4; type 1 / action 33 |
| `00558DFF` | type-38 `00558DE0` walk list `&node+8` | **DISPROVEN** — no type 38 on Press Start |

Those are posters **into** `0041E6D3`. They are not `0055CB10`.

---

## 7. First-seen Press Start: who calls whom

```
0042EC7C  frontend frame
  0042E3EE  poll
    type 4 → 0055CB10(26)                 ; vtbl+0
      type-10 0054E280 → 0054E2FA
        00595582 → 0059A238(&+352)        ; 0xE5 packet
      type-11 INVISIBLE 0054DBC0 → 0055AD60
        vtbl+584 / arm; no UI message
      type-12 list: not action 26
    type 1 → 0055CB10(33)
      last-key / 00597BF2; not 0xE5
  00599E3F  same-frame tick after 0059A238
```

| Callee | Caller on first-seen Press Start type 4 |
| --- | --- |
| `0055CB10` | `0042E3EE` `call [edx]` | **PROVEN** |
| `0059A238` | `0054E2FA` `call [edx+32]` | **PROVEN** if type-10 apply runs |
| `0041E6D3` | none on this path | **DISPROVEN** |

Type-10 register onto the `0055CB10` list stays **UNREAD** (ctor has
no `input.vtbl+8`; see `action26-subscribers`). That uncertainty is
“does `0054E2FA` run”, not “does it go through `0041E6D3`”. If
type-10 is missing from the list, type-11 INVISIBLE still only **arms**
on 26. Neither leftover invents a vtbl+56 hop.

---

## 8. C# / docs

| Native | C# / docs |
| --- | --- |
| Type 4 → `0055CB10` → `0054E2FA` → `0059A238` | `DispatchFrontendMessage` is the `0059A238` analog. **MATCH** consumer |
| `0041E6D3` locked as `InputVtblMessageFn` | Constant only; host does not call it | **MATCH** as unused on type 4 |
| `PARITY.md` / `FORWARD_TREE.md` “native key UNREAD (`0041E6D3` is the consumer)” | **STALE** — native **device** still UNREAD; **consumer** of the posted pair is `0059A238`. `0041E6D3` is input vtbl+56, a second poster that also reaches `0059A238` when game==0 |
| `05-input.md` “`0041E6D3` … Not the Press Start poster (`0054E280` calls UI directly)” | **MATCH** this dump |

---

## Do not invent

- Type 4 → `0041E6D3`.
- `0055CB10` as UI message consumer.
- First-seen Escape/Return list walk as Press Start `0xE5`.
- A DIK for type 4.
- Lionhead name for CRC `0x53C644E4`.
