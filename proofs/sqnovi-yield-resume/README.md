# S_QNOVI yield / resume: `00CB6EA0` / `00A44880` / `00CDD440`

Investigation only. No production `src/` edits.

Do **not** treat the 24-byte VM walk as the
`[vtbl+8]` dispatcher. Do **not** invent a
second `call [S_QNOVI.vtbl+8]` that re-enters
`00DABAC0` after a yield.

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**DIVERGE**.

Authority: Fable.exe listings + ExeIndex
`script-runtime` + `vtbl.tsv`.

- `listing-00c80000.txt` `00CB6EA0` /
  `00CB70E0` / `00CB7780` / `00CB6CE0` /
  `00CBE0C0` / `00CBE765`
- `listing-00a40000.txt` `00A44880` /
  `00A446A0` / `00A44660` / `00A44690` /
  `00A44840`
- `listing-00cc0000.txt` `00CDD430` /
  `00CDD440` / `00CDD450`
- `listing-006c0000.txt` `006E7410` /
  `006E7740` / `006E77E0`
- `listing-00d80000.txt` `00DAACE0` /
  `00DAAD70` / `00DABAC0` / `00DAAC00` /
  `00DBDE40` / `00DAC295`
- ExeIndex `script-walk-00cb6ea0` /
  `microthread-update-00a44880` /
  `s-qnovi-slot1-main` / `s-qnovi-slot2-run` /
  `vtbl-s-qnovi-vtbl-012d7a28` /
  `vtbl-watcher-vtbl-012d7a3c`
- `assembly/exe/00-index/vtbl.tsv`
  `0x01260F0C` slot 7
- PARITY 0b leftovers; `proofs/gameflow-main-first-tick`;
  `proofs/00DBB2A7-attackover-store`

---

## Verdict table

| Question | Answer | Class |
|---|---|---|
| `00CB6EA0` body? | 24-byte record walk; `+24` step; per-item `00CB6CE0` | **PROVEN** |
| Does `00CB6EA0` `call [vtbl+8]`? | **No.** Copies then `E8 00CB6CE0` | **DISPROVEN** |
| `00CB6EA0` `E8` sites? | `00CB710A` / `00CB7128` (both in `00CB70E0`) | **PROVEN** |
| `00A44880` body? | scheduler pump; dt `009E1BC0` → `+8`; resume `00A44660` | **PROVEN** |
| `00A44880` `.text` `E8`? | **0** (vtbl only: watcher `+4`, quest `+24`) | **PROVEN** |
| Resume of a parked `00DABAC0`? | `00A44921` `call 00A44660` → `009D87F0([fiber+16])` | **PROVEN** |
| Re-enter `00DABAC0` via `[S_QNOVI.vtbl+8]` after yield? | **No** | **DISPROVEN** |
| First enter of `00DABAC0`? | `00DAAD76` `call [esi+52]` → `00CDD440` `jmp [eax+8]` | **PROVEN** |
| Exact `call [vtbl+8]` *while* `00DABAC0` is yielded? | `006E741F` on `[0x13D2838]` (watcher `vtbl+8` = `00A44840`) | **PROVEN** |
| `[0x143E8F8].vtbl+28` dest? | **`006E7410`** (`01260F0C` slot 7) | **PROVEN** |
| Who writes `[0x143E8F8]`? | `006E77E0` `call 00CBE0C0`; `ecx` = Init Scripts `006E7740` (`vtbl 01260F0C`) | **PROVEN** |
| Main watcher callback `00CDD440`? | `mov eax,[ecx]; jmp [eax+8]` | **PROVEN** |
| `[quest+80]=1` in this pump? | not these three fns; leftover is `00DBB2A7` | **UNREAD** here |

---

## Timeline (`S_QNOVI` fiber, leftover vs Leave)

No-save Leave never constructs this object
(`proofs/newgame-script`). The yield/resume
shape is the same pump used by Gameflow /
Sunnyvale.

```
00DAACE0  slot1 Main
  00CDD450("Main")                    // fiber 00A44740
  vtbl 012D7A3C
  +52 = 00CDD440
  +56 = S_QNOVI
  00CB7E50 attach
type-1 00CB7950 +41=0
  [watcher.vtbl+4] 00A44880           // FIRST 00A44880 on this fiber
    00A44660  [0x13D2838]=watcher
              009D87F0([watcher+16])
      00A446A0
        [watcher.vtbl+16] 00DAAD70    // NOT persist 00DAADA0
          ecx=[esi+56]                // S_QNOVI
          call [esi+52]               // 00CDD440
            00CDD440  jmp [S_QNOVI.vtbl+8]
                      00DABAC0        // FIRST slot 2
              register NOVI_*
              00DAC295 E8 00DBDE40
                [esi+64].vtbl+28      // 14 sites
                == [0x143E8F8].vtbl+28
                == 006E7410
                  006E741F call [fiber.vtbl+8]
                           00A44840 → 00A44690 → 009D8650
        [esi+5]=1                     // after 00DABAC0 RETURNS
later type-1 00A44880
  [0x13D2838]==0
  009E1BC0 fstp [this+8]
  00A44921 call 00A44660              // RESUME (not vtbl+8)
    009D87F0 continues after 00A44690
    back in 006E7410 / 00DBDE40
```

`00CB6EA0` is **not** on this list. It walks
the 24-byte bind vector when `00CB7780` /
`00CB70E0` start scripts. Slot 2 is invoked
later through `00CDD440`, not inside the walk.

---

## 1. `00CB6EA0` — 24-byte list walk

`listing-00c80000.txt` / ExeIndex
`script-walk-00cb6ea0-00cb6ea0.md`.
Int3-bounded `00CB6EA0`–`00CB6F0D`.

```
00CB6EA0  push ecx / ebx / ebp
00CB6EA3  mov ebp, edx                // end
00CB6EA5  cmp ecx, ebp
00CB6EA9  mov [esp+16], ecx           // begin
00CB6EAD  je  00CB6F08
00CB6EAF  lea ebx, [ecx+24]           // RECORD = 24
00CB6EB2  cmp ebx, ebp
00CB6EB4  je  00CB6F08
00CB6EB6  lea edi, [ebx+12]
00CB6EC0:
  push arg
  sub esp, 24                         // copy of record
  0099EC30  CString at +0
  [esi+4] = [edi-8]                   // +4
  [esi+8] = [edi-4]                   // +8
  0099EC30  CString at +12
  [esi+16] = [edi+4]
  [esi+20] = [edi+8]                  // byte
  ecx = [esp+44]
  edx = ebx
  call 00CB6CE0                       // per-item
  add ebx, 24
  add edi, 24
  cmp ebx, ebp
  jne 00CB6EC0
00CB6F0D  ret 4
```

Record: CString + 2 dwords + CString +
dword + byte = **24**. Neighbour
`00CB70E0` / `00CB7780` use
`0x2AAAAAAB` (`1/24`) for
`(end-begin)/24`. **PROVEN**.

`00CB6CE0` (`listing-00c80000`):
name-compare `00429950`, then
`00CB62F0` / `00CB6420` **memmove** of
the same 24-byte fields. **No**
`call [r+8]`. **DISPROVEN** as the
slot-2 invoke.

Callers (`calls-script-walk-00cb6ea0`):

| Site | Parent |
|---|---|
| `00CB710A` | `00CB70E0` count > 16 (walk from `begin+384`) |
| `00CB7128` | `00CB70E0` count ≤ 16 |

`00CB70E0` itself has one `E8`:
`00CB77C8` from `00CB7780`. **PROVEN**.

---

## 2. `00CDD440` — Main watcher callback

`listing-00cc0000.txt` int3-bounded
`00CDD440`–`00CDD444`:

```
00CDD430  push esi
00CDD431  mov esi, ecx
00CDD433  mov ecx, [esi+56]           // factory
00CDD436  call [esi+52]               // callback
00CDD439  mov [esi+5], 1
00CDD43E  ret

00CDD440  mov eax, [ecx]
00CDD442  jmp [eax+8]                 // factory vtbl+8
```

S_QNOVI `00DAACE0` (`listing-00d80000`):

```
00DAAD1C  call 00CDD450               // "Main", 0.1f / 64 / 1
00DAAD21  mov [esi], 0x12D7A3C
00DAAD27  mov [esi+52], 0xCDD440
00DAAD2E  mov [esi+56], edi           // S_QNOVI
00DAAD49  call 00CB7E50
```

Clone `00DAAD70` is watcher `vtbl+16`
(`012D7A3C` slot 4):

```
00DAAD70  mov ecx, [esi+56]
00DAAD76  call [esi+52]               // 00CDD440
00DAAD79  mov [esi+5], 1
00DAAD7E  ret
```

S_QNOVI vtbl `012D7A28`:

| Off | Dest | Role |
|---:|---|---|
| +0 | `00DBEFA0` | dtor |
| +4 | `00DAACE0` | attach Main |
| **+8** | **`00DABAC0`** | slot 2 run |
| +12 | `00DAADD0` | reset (clears `+80`) |
| +16 | `00DAADA0` | persist bind `AttackOver` |
| +24 | `00A44880` | update |
| +28 | `00A44840` | wait helper |

Base slot 2 `00CBD4C0` is `ret`.
S_QNOVI overrides `00DABAC0`.
**PROVEN**.

`00DABAC0` has **0** `.text` `E8`
callers. The VM enters it only through
this thunk. After `00DABAC0` **returns**,
`00DAAD79` sets `+5=1` so
`00A446A0` does **not** loop
`call [watcher.vtbl+8]`. **PROVEN**.

---

## 3. `00A44880` — pump / resume

`listing-00a40000.txt` int3-bounded
`00A44880`–`00A4492B`.
`calls-microthread-update-00a44880`:
**hits 0**.

```
00A44880  push ebp / sub esp, 72
00A44886  mov [ebp-72], ecx
00A44889  cmp [0x13D2838], 0
00A44890  je  00A448D5
          ; nested: 00A44C20 ×2, 00A44690, restore
00A448D5:
00A448E9:
  00A44930  has-work? → ret
  00A44A70 / 00A44A80  pop fiber
  009E1BC0
  fstp [this+8]                       // dt
  mov ecx, [ebp-8]
  call 00A44660                       // RESUME
  jmp 00A448E9
```

`00A44660`:

```
00A4466A  mov [0x13D2838], ecx
00A44672  mov ecx, [ecx+16]
00A44675  call 009D87F0               // continue after yield
00A4467A  mov [0x13D2838], 0
```

`00A446A0` fiber entry (`listing-00a40000`):

```
00A446EF  call [eax+16]               // watcher +16 = 00DAAD70
00A44714  call [edx+8]                // watcher +8  = 00A44840
00A44717  jmp 00A446CE                // until +5
00A44726  call 009D8650               // park
```

Watcher `012D7A3C`:

| Off | Dest |
|---:|---|
| +4 | `00A44880` |
| +8 | `00A44840` |
| +12 | `00A447D0` |
| +16 | `00DAAD70` |

PARITY’s “fiber calls persist
`00DAADA0` then run `00DABAC0`”
is **DISPROVEN** for this watcher:
`vtbl+16` is `00DAAD70`, not
`00DAADA0`. Persist is S_QNOVI
`+16`, a different `this`.

`00A44840` (`listing-00a40000`):

```
00A4484A  call 00A4B220
          al!=0 → return 0
00A4485A  call 00A44690               // 009D8650 park
          00A4B220 again
          al!=0 → 0 else 1
```

This is the **fiber** `[vtbl+8]` dest
while `00DABAC0` is on the stack.
It is **not** `00DABAC0`. **PROVEN**.

---

## 4. `[0x143E8F8].vtbl+28` = `006E7410`

`006E7740` Init Scripts
(`listing-006c0000.txt`):

```
006E7769  mov [esi], 0x1260F0C
006E77DA  mov [0x143E8F0], esi
006E77E0  call 00CBE0C0
```

`00CBE0C0` (`listing-00c80000.txt`):

```
00CBE0C0  mov [0x143E8F8], ecx
00CBE0C6  ret
```

So `[0x143E8F8]` is that manager.
`vtbl.tsv` `0x01260F0C` slot **7**
(byte **+28**) = **`006E7410`**.
**PROVEN**.

Opcode sites load the same global
then `call [eax+28]`, e.g.
`00CBE75D` / `00CBE765`.
StartOakVale uses `[esi+64]`:
`00DAAC10 mov [esi+64], eax` from
factory `00DBEF70` (`push edi` =
ctor arg, the same context).
First yield `00DBDE81`–`00DBDE86`:

```
00DBDE81  mov ecx, [esi+64]
00DBDE84  mov eax, [ecx]
00DBDE86  call [eax+28]
```

14 `call [r+0x1C]` hits in
`00DBDE00`–`00DBF000`. **PROVEN**
same slot as `[0x143E8F8].vtbl+28`.

`006E7410` (`listing-006c0000.txt`):

```
006E7410  mov ecx, [0x13D2838]
006E7416  mov al, [ecx+5]
006E7419  test al, al
006E741B  jne 006E7451                // already done
006E741D  mov eax, [ecx]
006E741F  call [eax+8]                // fiber vtbl+8
006E7422  mov ecx, [0x13D2838]
006E7428  test [ecx+5]
006E742D  jne 006E7451
006E743E  … 0049D870 …               // after resume
006E7451  ret
```

On S_QNOVI Main, `[0x13D2838]` is
the watcher (`00A44660`),
`[eax+8]` is `00A44840`, which
parks via `00A44690`.
**That** is the exact
`call [vtbl+8]` site during a
yielded `00DABAC0`. Dest is
**not** `00DABAC0`. **PROVEN**.

Resume is the next `00A44880` →
`00A44660` → `009D87F0`, which
continues **after** `00A44690`
inside `00A44840`, then returns
to `006E7410`, then to
`00DBDE89`. **PROVEN**.

`0049D870` is the
`[ecx+5]==0` tail **after**
that resume, not the first park.
**PROVEN** (same as type-1
Gameflow note).

---

## 5. What stays UNREAD

| Item | Class |
|---|---|
| Who writes `[quest+80]=1` after `vtbl+2584(12)` / `HerosOldHouse` | **UNREAD** in these three listings. Not a `mov` in `00CB6EA0` / `00A44880` / `00CDD440` / `006E7410`. Other proof names `00DBB2A7` (later raid/Theresa). |
| `00A4B220` predicate inside `00A44840` | **UNREAD** body this pass |
| First-seen Leave ever reaching `00DABAC0` | **DISPROVEN** (`Q_NewOakValeIntro` not activated) |

---

## Sources (absolute)

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00c80000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a40000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00cc0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-006c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\script-walk-00cb6ea0-00cb6ea0.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\microthread-update-00a44880-00a44880.md`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
