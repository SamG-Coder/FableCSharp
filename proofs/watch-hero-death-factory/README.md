# `Global_WatchForHeroDeath` bind `00CD9A12` / factory `00EE90A0`

Investigation only. No production `src/` edits.

Do **not** treat `00CD9A12` as its own registrar. It is a
mid-`00CD52D0` site (`xrefs.tsv` `fn=0x00CD9A12` is a false
frame). The fill is one function `00CD52D0`–`00CDB35C`.

Do **not** start at `S_QNOVI` / `00DBDE40`. This name is QST
`AddQuest` TRUE row 9 on first `004B4260` (`world+172`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `listing-00cc0000.txt` `00CD52D0` /
`00CD9A04`–`00CD9A62`; `listing-00ec0000.txt` `00EE8E80`–
`00EE9108` (no `listing-00ee0000.txt`);
`listing-00480000.txt` `004B3CE0` / `004B4260`;
`listing-00c80000.txt` `00CB5AC0` / `00CB5AD0` / `00CB5C90` /
`00CB7900` / `00CB8110` / `00CBFAB8`;
`00-index/strings.tsv` `0x012F75D4` / `0x012F54A0`;
`proofs/factory0-enqueue`, `fiber-first`, `script-factory-tables`,
`world-plus172-activate`, `gameflow-main-first-tick`;
`QuestFactoryTable.WatchForHeroDeath*`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| What is `ebx` at the run store? | **`0`** | **PROVEN** |
| Is the run `00CDBD20`? | No. Early rows use `ebp = 00CDBD20`. This row stores `ebx` | **DISPROVEN** |
| First `00EE90A0` body after `004B4260`? | `00BFEA1A(68)` / `00CB8110` / `[+64]=edx` / vtbl `012F5470` | **PROVEN** |
| Empty script `0x122D70E` — any fiber? | **Yes.** Bind script name is empty; `00CB7900` still runs Main `00EE8EB0` → `00CDD450("Main")` / `00A44740` | **PROVEN** |
| Size / vtbl? | **68** / **`012F5470`**. Watcher overwrite `012F5484` | **PROVEN** |

`QuestFactoryTable` already stores run `0`. Size / vtbl /
Main are still `0` in C#. This note does not edit that.

---

## Verdict

`00CD9A52` `mov [esp+36], ebx` writes **run = 0**, not
`00CDBD20`. `ebx` is `xor ebx, ebx` at `00CD52E9` and is
never rewritten inside `00CD52D0`. Persist is `bl` = **0**.

Factory `00EE90A0` is non-zero, so first `004B4260` (QST TRUE
row 9) is the factory arm of `004B3CE0`, not the 52-byte
stub. Persist 0 → `004AFA10` reuses Sunnyvale SharedRun
(skips `call [record+4]`, which would be NULL). Then
`call [record+0]` = `00EE90A0`, then `00CB7900`.

Empty `0x122D70E` is the script-name CString. It is **not**
a “no fiber” gate. Same shape as `CS_PlayCutscene`.

---

## Timeline (no-save, native `world+172`)

```
004A6677  00CB5D80 / 00CD52D0          // FILL only
  00CD52E9  xor ebx, ebx               // ebx stays 0
  00CD532D  mov ebp, 00CDBD20          // SharedRun for named rows
  …
  00CD99DA  "Registering Global Quest Scripts"
  00CD9A09  push 0x122D70E             // 00CB5AC0 discard
  00CD9A1C  push "Global_WatchForHeroDeath"
  00CD9A4A  [esp+32] = 00EE90A0        // factory
  00CD9A52  [esp+36] = ebx             // run = 0
  00CD9A46  [esp+48] = bl              // persist = 0
  00CD9A62  call 00CB5C90
  00CD9A89  xor edx, edx / 00CBFAB8    // script-def ptr 0
004B4260([world+172])                  // FIRST USE
  row 9 00CB5AD0 hit
  004BB720
  004B3CE0
    [record+16]==0 → 004AFA10          // reuse SharedRun
    call 00EE90A0                      // FIRST 00EE90A0
    00CB7900  [vtbl+12] then jmp [vtbl+4]
      00EE8EB0 Main → 00CDD450("Main") // fiber
```

WLD `START_INITIAL_QUESTS` does **not** contain this name.
Host `WorldPlus172` does. **PROVEN** (`world-plus172-activate`).

---

## 1. `ebx` at the run store

`listing-00cc0000.txt`:

```
00CD52E9  xor ebx, ebx
00CD532D  mov ebp, 0xCDBD20
…
00CD985D  mov [esp+36], ebp        // V_TempleOfLight — SharedRun
00CD98F6  mov [esp+36], ebp        // V_TravellingHeroes
00CD998F  mov [esp+36], ebp        // V_TrophyDealer
00CD99EA  push ebx                 // log arg = 0, not the bind
00CD9A4A  mov [esp+32], 0xEE90A0
00CD9A52  mov [esp+36], ebx        // run
00CD9AEB  mov [esp+36], ebx        // next global row, still 0
```

Next `mov ebx` / `xor ebx` after `00CD52E9` is `00CDB386`,
past `00CDB35C`. **PROVEN** `ebx = 0` for the whole fill
after the prologue.

`ebp` is still `00CDBD20`. They chose `ebx` on purpose.

If persist were 1, `004B3CE0` would `call [record+4]` =
NULL. Persist `bl` and run `ebx` stay paired at 0 so that
arm is skipped.

---

## 2. Bind record

Same `00CB5C90` packing as the rest of `00CD52D0`:

| Slot | This row | Sunnyvale |
|---|---|---|
| `[esp+32]` factory | `00EE90A0` | `00CDD550` |
| `[esp+36]` run | `ebx` = **0** | `ebp` = `00CDBD20` |
| `[esp+44]` | `edi` = 1 | 1 |
| `[esp+48]` persist | `bl` = **0** | immediate `1` |
| quest CString | `Global_WatchForHeroDeath` | `Q_SunnyvaleMaster` |
| script CString | empty `0x122D70E` | empty `0x122D70E` |

`00CB5AC0` (`listing-00c80000.txt`) is **not** a script-def
insert:

```
00CB5AC0  lea ecx, [esp+4]
00CB5AC4  call 0099EAE0          // CString dtor
00CB5AC9  ret 4
```

The `0x122D70E` push at `00CD9A09` is discarded. The bind
script name is the **second** empty push (`00CD9A3D`) into
`00CB5C90`.

After the bind, `xor edx, edx` / `00CBFAB8` writes **0** at
`0x143E910`. No `S_*` token. **PROVEN**.

QST `Persistent` on this name is a **file** bit
(`WorldSceneTests`). It is **not** `[esp+48]`. Do not
collapse them.

---

## 3. First `00EE90A0` after `004B4260`

`listing-00ec0000.txt`:

```
00EE90A0  push esi
00EE90A1  push edi
00EE90A2  push 68
00EE90A4  mov edi, edx
00EE90A6  call 00BFEA1A
00EE90AB  mov esi, eax
00EE90AD  add esp, 4
00EE90B0  test esi, esi
00EE90B2  je 00EE90C9
00EE90B4  mov ecx, esi
00EE90B6  call 00CB8110          // base; temp vtbl 012C1648
00EE90BB  mov [esi+64], edi      // edx = [manager+124]+56
00EE90BE  pop edi
00EE90BF  mov [esi], 0x12F5470
00EE90C5  mov eax, esi
00EE90C7  pop esi
00EE90C8  ret
```

Incoming `ecx` (SharedRun from `004AFA10`) is unused. The
factory always allocates a **new** 68-byte object.

`004B3CE0` (`listing-00480000.txt`) for `[edi+4] != 0` and
`[0x1375454] == 1`:

```
004B3F0E  mov cl, [eax+16]       // persist
004B3F15  je 004B3F30            // 0 → 004AFA10
…
004B3F43  mov eax, [ecx]         // [record+0] = 00EE90A0
004B3F47  call eax
004B3FEC  call 00CB7900
```

`00CB7900`:

```
call [vtbl+12]
jmp [vtbl+4]                    // Main
```

This is **not** factory 0. **DISPROVEN** as a 52-byte stub
(`factory0-enqueue`).

---

## 4. Empty script still has a fiber

`00EE8EB0` is the Main pattern (`00CE75B0` / `00CDD380` /
`00F017F0` / `00DAACE0`):

```
00EE8EB0  sub esp, 8
          00BFEA1A(60)
          0099EBF0 "Main"
          00CDD450               // 00A44740 fiber, stack 64, 0.1f
          [esi] = 0x12F5484
          [esi+52] = 00CDD440    // later jmp [factory.vtbl+8]
          [esi+56] = edi         // factory object
          00CB7E50(0x122D70E)    // attach; empty name is normal
          ret
```

`0x122D70E` on `00CB7E50` is the attach CString used by
every proven Main. It does **not** mean “no watcher.”

So: empty **bind** script ⇒ no `CCutsceneDef` / no `S_*`.
Construct still creates a **Main** fiber. Same as
`CS_PlayCutscene`. **PROVEN** pattern. Raw
`012F5470+4` dword is **UNREAD** (rdata, no text-map
listing). Slot identity follows `00CB7900` + this being
the only Main-shaped method on the factory object.

Second `00CDD450` site `00EE8F50` (`"WatchForHeroDeath"`,
`+52 = 00EE8FE0`) is **not** `00CB7900` Main. `00CB7900`
does not call `vtbl+8`. Whether the first type-1 pump
spawns that named watcher is **UNREAD**.

`00EE8FE0` (named-watcher body) polls `[this+64]` vtbl
`+280` / `+300` / `+244` / `+28` and `00CB7940`
(hero-exists). Not reached from construct. **PROVEN**
absence of an `E8` from `00EE8EB0`.

---

## 5. Size / vtbl

| Object | Size | vtbl | Site |
|---|---:|---|---|
| Factory quest | **68** | **`012F5470`** | `00EE90A2` / `00EE90BF` |
| Base before overwrite | 68 | `012C1648` | `00CB8110` |
| Main watcher | 60 | `012F5484` | `00EE8EB0` after `00CDD450` |
| Sibling ctor | 68 | `012F54B4` | `00EE90F0` — not this bind |

`00EE90D0` is the scalar-deleting dtor (`00CBD510` /
`00BFE9BC`), same shape as base `00CBD4F0` (`vtbl+0`).

`00EE8EA0` is a lone `ret` next to Main (base `vtbl+12`
`00CBD4D0` is also `ret`). Treat as empty seed **PARTIAL**
until `012F5470` dwords are dumped.

String `WatchForHeroDeath` sits at `0x012F54A0`
(`strings.tsv`), after the watcher vtbl.

---

## 6. What this is not

| Claim | Class |
|---|---|
| Run is `00CDBD20` / `ebp` | **DISPROVEN** (`ebx` = 0) |
| Empty `0x122D70E` skips `00CB7900` / fiber | **DISPROVEN** |
| `00CB5AC0` registers a script def | **DISPROVEN** (CString dtor) |
| Factory 0 stub / no `00CB7900` | **DISPROVEN** (`00EE90A0` ≠ 0) |
| Persist 1 constructs a new run via `[record+4]` | **DISPROVEN** (`bl` = 0 → `004AFA10`) |
| Factory uses incoming SharedRun as `this` | **DISPROVEN** (fresh 68-byte alloc) |
| `CCutsceneDef` / `HasStarted("S_*")` | **DISPROVEN** (no script name) |
| `00EE8F50` is first-seen construct Main | **DISPROVEN** |
| `listing-00ee0000.txt` is the authority | **DISPROVEN** (file missing; use `listing-00ec0000.txt`) |
| Host WLD six-name list is this walk | **DIVERGE** vs QST TRUE nine (if still used) |

---

## Classifications (short)

1. **`ebx` at `00CD9A52` is 0 — PROVEN.** Not `00CDBD20`.
2. **Factory `00EE90A0` is 68 bytes, vtbl `012F5470` — PROVEN.**
3. **First post-`004B4260` call is that alloc + `00CB8110` +
   `00CB7900` — PROVEN.** Persist 0 reuses SharedRun.
4. **Empty bind script still creates a Main fiber — PROVEN**
   (`00EE8EB0` / `00CDD450`). Named `WatchForHeroDeath`
   watcher is a later site.
5. **Raw `012F5470` slot dwords — UNREAD.**
