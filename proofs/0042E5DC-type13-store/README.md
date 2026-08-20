# `0042E5DC` type 13 stores `+176/+180` then `push 25` / `0055CB10`

Investigation only. No production `src/` edits.

Authority: `Fable.exe` dump
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0042E3EE` / `0042E5DC` / `0041E5F2` / `0041E3F6`),
`listing-00540000.txt` (`0055CB10` / `0055C6F0` / `0055A510` /
`0055C650`),
`listing-00a00000.txt` (`00A03FB0` / `00A03B40`),
`listing-00a80000.txt` (`00AB5B3D`);
`src/Fable.Game/EngineInput.cs` `ApplyEvent`;
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Client/Program.cs`;
`proofs/type13-vs-type4/README.md`,
`proofs/mouse-pointer-action25/README.md`,
`proofs/action26-subscribers/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

Do not re-prove type 4 → `push 26`, `0055CB10` broadcast shape, or
Press Start register order.

---

## Verdict

**`+176/+180` are a float XY pair on the `0041E5F2` action singleton
(`[0x13B8710]`, size `0xD0`, vtbl `01230134`). They are not action
ids and not a host invention.**

`0042E5DC` copies type-13 `record+12/+16` (first two dwords of the
`00A03FB0` 12-byte xyz) onto that pair, then `push 25` into
`input.vtbl+0` = `0055CB10`. Type-32 apply `0055C6F0` later
**overwrites** the same pair with pointer-sprite deltas.

Host `EngineInput.ApplyEvent(TypeMouse)` only `Dispatch(25)`. It
does **not** write `+176/+180`. That is a **LEFTOVER gap**, not
leftover extra host stores. Live `Program.cs` never queues type 13.

| Claim | Status |
| --- | --- |
| `0042E3EE` type 13 (`sub eax,13` / `je 0042E5DC`) | **PROVEN** |
| `0042E5DC` copies `record+12/+16` → singleton `+176/+180` | **PROVEN** |
| Then `push 25` / `jmp 0042E5AB` / `call [edx]` = `0055CB10` | **PROVEN** |
| Dest object is `0041E5F2` → `[0x13B8710]` (`EngineInput`) | **PROVEN** |
| `record+12/+16` are type-13 xyz dwords 0/1 (`00A03FB0`) | **PROVEN** |
| Those xyz are floats from `00AB5B3D` (`fld` / deadzone) | **PROVEN** |
| `+176/+180` are action ids / `0055CB10` arguments | **DISPROVEN** — action is the `push 25` |
| `+176/+180` are camera letterbox (`00B30B50`) | **DISPROVEN** — different object |
| Ctor `0041E3F6` zeros `+176/+180` | **DISPROVEN** — zeros `+184/+188`, not this pair |
| `0055C6F0` action 25 overwrites the same pair with float Δ | **PROVEN** |
| `0055A510` action 25 reads `[singleton+176]` as float | **PROVEN** |
| `EngineInput.ApplyEvent(TypeMouse)` writes `+176/+180` | **DISPROVEN** — **LEFTOVER** gap |
| Host classify type 13 → action 25 | **MATCH** |
| Live host queues type 13 | **DISPROVEN** — **LEFTOVER** producer |

**Answer:** `+176/+180` are the action-singleton motion XY fields
seeded by type 13 before `0055CB10(25)`. They are **not** leftover
`ApplyEvent` stores. The leftover is that host `ApplyEvent` omits
them.

---

## 1. Dump `0042E3EE` → `0042E5DC`

Record lives at `[ebp-80]` (`lea ecx, [ebp-80]` then
`00A03B40` = `[record+40]`).

```
0042E456  call 00A03B40            ; type
0042E45B  cmp eax, 17
…
0042E46A  cmp eax, 10
0042E46D  jg 0042E560              ; type still in eax
…
0042E560  sub eax, 13
0042E563  je 0042E5DC              ; type == 13
```

`record+12` = `[ebp-80]+12` = `[ebp-68]`.
`record+16` = `[ebp-64]`.

```
0042E5DC  mov eax, [ebp-68]        ; record+12
0042E5DF  mov [ebp-20], eax
0042E5E2  mov eax, [ebp-64]        ; record+16
0042E5E5  mov [ebp-16], eax
0042E5E8  call 0041E5F2            ; eax = [0x13B8710]
0042E5ED  mov ecx, [ebp-20]
0042E5F0  mov [eax+176], ecx
0042E5F6  mov ecx, [ebp-16]
0042E5F9  mov [eax+180], ecx
0042E5FF  call 0041E5F2
0042E604  push 25
0042E606  jmp 0042E5AB
0042E5AB  mov edx, [eax]
0042E5AD  mov ecx, eax
0042E5AF  call [edx]               ; vtbl+0 = 0055CB10
```

Temps at `[ebp-20]/[ebp-16]` survive `0041E5F2`. Second getter
reloads `eax` for the shared apply join (same join as type 4
`push 26`).

`0041E5F2`:

```
0041E5F2  mov eax, [0x13B8710]
          test eax, eax
          jne ret
          push 0xD0
          call 00BFEA1A
          call 0041E3F6            ; vtbl 01230134
          mov [0x13B8710], eax
```

`EngineInput.ObjectSize = 0xD0`, `SingletonVa = 0x013B8710`,
`ActionApply = 0x0055CB10`. Offsets 176/180 sit on that object,
next to ctor-zeroed `+184` (type-32 widget*) and `LastKey` 192.

Ctor `0041E3F6` writes `+184/+188` to 0 and `+196` to `-1.0f`.
It does **not** store `+176/+180`. First writers are this site
and `0055C6F0`.

---

## 2. What `record+12/+16` are

Type-13 ctor `00A03FB0`:

```
00A03FB8  mov [ecx+32], 3          ; mouse-like device
00A03FBF  mov [ecx+40], 0xD        ; type 13
          lea eax, [ecx+12]
          ; 12 bytes from src ptr → +12/+16/+20
          ; +24/+28 from a second ptr (screen pair)
```

Sole `.text` call is `00AB5B3D`. That producer `fld`s
`[esp+20/24/28]`, deadzone-skips, optionally mixes analog at
`this+18508/+18512/+18516`, then:

```
00AB5B26  lea ecx, [esp+40]        ; 12-byte xyz
00AB5B35  call [edx+8]             ; dest +24/+28
00AB5B3D  call 00A03FB0
00AB5B49  call 00A66B20            ; enqueue
```

So `+176/+180` receive **motion X/Y bit-copies** (floats; no
`fld` at `0042E5DC`). Z at `record+20` is not stored on the
singleton here.

RTTI: `CInputTypeMouseMovementEvent` (`type13-vs-type4`).

---

## 3. Who consumes the pair

`0055CB10` itself never reads `+176/+180`. It walks
`listener.vtbl+8` then `vtbl+4` with the pushed action 25.

Type-32 `UI_MOUSE_POINTER` apply `0055C6F0` (action 25 only):

```
0055C6F0  cmp eax, 25
          jne ret 4
          ; snapshot inner+48/+52
          ; clamp pointer vs display (009A4EC0 / 009BEDC0)
          fld  [esi+48]            ; new X
          fsub [esp+4]             ; − old X
          fstp [esp+12]
          fld  [esi+52]
          fsub [esp+8]
          fstp [esp+16]
          call 0041E5F2
          mov [eax+176], ecx       ; float ΔX
          mov [eax+180], edx       ; float ΔY
```

Ctor `0055C650` also `mov [eax+184], esi` (widget*, not this
pair) and local-maps 25 via `0052DA20`.

Type-35 slider apply `0055A510` (`cmp edi,25`) reads the pair
as float (`fld [esp+8]` after `mov ecx, [eax+176]`) for thumb
math. First-seen Press Start has no type 35.

Do not treat camera `00B30B50` `+176/+180` (letterbox 1024×768)
as these fields.

---

## 4. Host leftover

```
EngineInput.ApplyEvent:
  TypeMouse → Dispatch(ActionMouse)   ; 25 only
  ; no +176 / +180 fields
```

`FrontendInputMap.ActionFromEvent(13) = 25` **MATCH**es
`0042E604`. `MessageFromWidgets` still posts only 26/28.

`Program.cs` queues type 4 / 6 on LMB edges. Mouse move is
F2-look `debugCam` only. No `QueueInput(TypeMouse)`.

| Site | Native | Host | Class |
| --- | --- | --- | --- |
| type 13 → 25 | `0042E5DC` | `ApplyEvent` / `ActionFromEvent` | **MATCH** |
| store `+176/+180` | yes, before apply | missing | **LEFTOVER** gap |
| queue type 13 | `00AB5B3D` | none | **LEFTOVER** producer |
| LMB → type 13 | no (`00A03C80` type 4) | type 4 / 6 | **MATCH** (not this pair) |

---

## Do not invent

- `+176/+180` as the `0055CB10` action or as `input+184`.
- Host cursor pixels as `00A03FB0` `+12` xyz.
- Type 13 as Press Start click / `0xE5`.
- `ApplyEvent` stores that are not in `EngineInput.cs`.
