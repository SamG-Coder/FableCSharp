# Type 6 → `push 28` site; same `0042E3EE` as type 4 → 26 then 28

Investigation only. No production `src/` edits.

Authority: dump `Fable.exe` `0042E3EE` / `0042E498` / `0042E49D` /
`0042E4A4` / `0042E5AB` / `009F4ED0` / `009F4F10` /
`00A03D60` / `00A03B40` / `00AB5420` / `00AB5590` /
`00AB58E0` / `00A66B20` / `00A66FD0` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`,
`listing-00a00000.txt`, `listing-00a80000.txt`,
`listing-00a40000.txt`, `listing-009c0000.txt`;
`proofs/type6-action28/README.md`,
`proofs/type4-enqueue-ring/README.md`,
`proofs/type4-dinput-raw/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove type 6 = LMB up, type 4 = LMB down, or action 28
unarm vs action 26 post.

---

## Verdict

**Exact site:** `0042E3EE` type 6 is `0042E483 je 0042E498` →
`0042E49D push 28` → `jmp 0042E5AB` (`0055CB10`). Ctor is
`00A03D60` (`[record+40]=6`, device 3).

**Same poll as type 4:** one `0042E3EE` harvests once
(`[0x13B8388].vtbl+8`) then walks **every** mux slot
(`009F4ED0` first, `009F4F10` next). Type 4 and type 6 are
separate 52-byte records. If both are in `[mux+16]` this
walk, **both fire**. There is no “already did 26, skip 28”
gate.

**Order:** FIFO of the mux vector = mouse-device array =
`00AB58E0` sample order. Down then up in one `GetDeviceData`
buffer is type 4 then type 6 → **action 26 then action 28**.
`00A66FD0` on type 6 only erases the type-5 **hold list**
node; it does not drop the earlier type-4 array slot.

| Claim | Status |
| --- | --- |
| Type 6 classify is `dec; dec; je 0042E498` after type-4 `sub 3` | **PROVEN** |
| Exact `push 28` is `0042E49D` | **PROVEN** |
| Apply is `0042E5AB` `call [edx]` (`0055CB10`) then `jmp 0042E7F0` | **PROVEN** |
| `00A03D60` writes `[+40]=6`, `[+32]=3` | **PROVEN** |
| One raw sample fills both type 4 and type 6 | **DISPROVEN** — separate `00AB5420` arms |
| One `0042E3EE` walks all harvested records | **PROVEN** `009F4ED0` / `009F4F10` |
| Type 4 then type 6 in that walk → 26 then 28 | **PROVEN** |
| Type 4 consume skips a later type 6 | **DISPROVEN** |
| `00A66FD0` type 6 removes the type-4 array copy | **DISPROVEN** — erases type-5 list only |
| Same-frame LMB down+up can enqueue both | **PROVEN** shape (`00AB58E0` loop + 256 cap); live hit **UNREAD** |
| Reverse order (28 then 26) if up then down in the buffer | **PROVEN** as FIFO; live hit **UNREAD** |
| Type 5 hold in the same harvest also fires 26 | **DISPROVEN** |

---

## 1. Exact type-6 site in `0042E3EE` (**PROVEN**)

`00A03B40` is `mov eax, [ecx+40]`. Classify
(`listing-00400000.txt`):

```
0042E453  lea ecx, [ebp-80]
          call 00A03B40
          cmp eax, 17 / 10 …
0042E479  dec eax                   ; type-1
0042E47A  je  0042E4B0
0042E47C  sub eax, 3                ; type-4
0042E47F  je  0042E4A4              ; → push 26
0042E481  dec eax
0042E482  dec eax                   ; type-6
0042E483  je  0042E498
0042E485  dec eax                   ; type-7
0042E486  jne 0042E7F0
0042E48C  call 0041E5F2
          push 35
          jmp 0042E5AB
0042E498  call 0041E5F2
0042E49D  push 28                   ; THIS site
0042E49F  jmp 0042E5AB
0042E4A4  call 0041E5F2
0042E4A9  push 26
0042E4AB  jmp 0042E5AB
0042E5AB  mov edx, [eax]
          mov ecx, eax
          call [edx]                ; 0055CB10
          jmp 0042E7F0              ; next record
```

`eax` after `call 0041E5F2` is the action singleton.
`push 28` / `push 26` are the apply args. Same join.

Ctor (`listing-00a00000.txt`):

```
00A03D60  mov eax, [esp+4]
          fld qword [esp+8]
          mov [ecx+32], 0x3
          mov [ecx+40], 0x6
          copy [eax] → [ecx+24/+28]
          fstp [ecx+48]
          fld qword [esp+16]
          fstp [ecx+44]
          ret 20
```

Sole `.text` E8: `00AB55A8` inside `00AB5420` raw-4 arm
(`00AB5590`). Sibling type 4 is `00A03C80` (`[+40]=4`).

---

## 2. One `0042E3EE` is a multi-record loop (**PROVEN**)

```
0042E42C  mov ecx, [0x13B8388]
          call [eax+8]              ; harvest 009F57A0
0042E449  call 009F4ED0             ; first slot → [ebp-80]
          jmp 0042E803
0042E453  … classify / maybe 0055CB10 …
0042E7F0  mov ecx, [0x13B8388]
          lea eax, [ebp-80]
          push eax
          lea eax, [ebp-28]
          push eax
0042E7FE  call 009F4F10             ; next slot
0042E803  test al, al
          jne 0042E453
0042E815  call 009F4AC0             ; end walk
```

`009F4ED0` (`listing-009c0000.txt`): if `[mux+28]<=0` fail;
else copy slot 0 (`rep movsd` 13 dwords), cursor 0.

`009F4F10`: `cursor++`; while `cursor < [+28]` and
`00A03B40(slot)==0`, skip; else copy that slot. **Type 0
only.** Type 4 and type 6 both copy.

Harvest is **once** per `0042E3EE`, before the walk. Every
surviving mouse slot from this `00AB58E0` is already in
`[mux+16]` when classify starts.

---

## 3. Can both 26 and 28 fire? (**PROVEN** yes)

Producer (`00AB58E0`, `listing-00a80000.txt`):

```
00AB5923  call 00A66B10             ; [device+13316] = 0
loop:
  00AB4910 / 00AB4BB0               ; one sample
  test al, 1 / [+13373]
  00AB5420(sample, dest)
  test al, al
  je  loop
  00A66B20(dest)                    ; 00AB59CB append 52
  00A66FD0(dest)                    ; hold-list side effect
  jmp loop
```

Raw 1 → `00A03C80` type 4. Raw 4 → `00A03D60` type 6.
Each successful translate is its own `00A66B20` increment.
Cap 256 (`type4-enqueue-ring`). DINPUT buffer is also
`0x100` (`00AB5710`).

`00A66FD0` after a type-6 append (`00A67186 cmp eax, 6`):

```
walk [device+13324]
if node+12 type == 5:
  009E47E0 erase that node
ret 4
```

No write of `[+13316]`. No `00A03BE0` of the type-4 slot.
Type 4 stays in the array; harvest `009F57A0` copies it
and **also** copies the type-6 slot (`inc [mux+28]` each).

So if this poll’s sample buffer has LMB down then LMB up:

| Mux index | `+40` | `0042E3EE` arm | Action |
| ---: | ---: | --- | ---: |
| 0 | 4 | `0042E4A4` `push 26` | **26** |
| 1 | 6 | `0042E498` `push 28` | **28** |

Apply of 26 is synchronous (`call [edx]` before
`009F4F10`). Then 28 applies on the next iteration.

Type 5 (hold copy from `00A66ED0`, if the list still has
a node) is not an action here: after `dec; sub 3; dec; dec`
it misses `je 0042E498` and `jne 0042E7F0`. It cannot
stand in for 26 or 28.

A single sample cannot be both types. `00AB5420` is one
`[esi+8]` switch.

---

## 4. Order

| Buffer this harvest | `0042E3EE` applies |
| --- | --- |
| type 4 only | 26 only |
| type 6 only (release after a prior frame’s hold) | 28 only — **different** `0042E3EE` from that 26 |
| type 4 then type 6 | **26 then 28** |
| type 6 then type 4 (up then down in one `GetDeviceData`) | **28 then 26** |
| type 4, type 6, type 4 (double-click in one buffer) | 26, 28, 26 |

Held across frames is **not** the same poll: `00A66B10`
zeros the device count every `00AB58E0`. Frame N type 4
→ 26; later frames type 5 only; release frame type 6 → 28
in a **later** `0042E3EE`.

Whether a live click-release lands in one `GetDeviceData`
is **UNREAD** here (listing-only). The consume rule does
not care: if both records are present, both fire, in
enqueue order.

---

## 5. C# leftover (do not apply here)

| Site | Native | Host |
| --- | --- | --- |
| type 6 → 28 | `0042E49D` | `ActionType6=28` **MATCH** |
| one poll walks all | `009F4ED0` / `009F4F10` | `EngineInput.Pump` foreach **MATCH** |
| 26 then 28 same `Pump` | mux FIFO | queue order **MATCH** if host enqueues 4 then 6 |
| host queues type 6 | `00A03D60` | still none (`host-input-type4`) **LEFTOVER** |

Do **not** collapse type 4+6 into one action. Do **not**
map type 6 → 26. Do **not** assume 28 waits for a later
frame when both records were harvested together.

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
  (`0042E3EE`, `0042E498` / `0042E49D`, `0042E4A4`,
  `0042E5AB`, `0042E7F0` / `0042E7FE`)
- `listing-00a00000.txt` (`00A03B40`, `00A03D60`)
- `listing-00a80000.txt` (`00AB5590` / `00AB55A8`,
  `00AB58E0` / `00AB59CB` / `00AB59D7`)
- `listing-00a40000.txt` (`00A66FD0` type 4 vs type 6)
- `listing-009c0000.txt` (`009F4ED0`, `009F4F10`)
- `proofs/type6-action28/README.md`
- `proofs/type4-enqueue-ring/README.md`
- `proofs/type4-dinput-raw/README.md`
- `src/Fable.Game/FrontendInputMap.cs`
- `src/Fable.Game/EngineInput.cs`
