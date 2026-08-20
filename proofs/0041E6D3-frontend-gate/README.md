# `0041E6D3` `[0x13B86A0]==0` always takes UI `vtbl+32` on first-seen frontend

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listing
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041E6D3` / `004184BD` / `004175E5` / `00412F90` / `0042F491` /
`00418DCA` / `0042BE50` / `004069E0`);
`listing-00580000.txt` (`00595582` / `0059A238`);
`functions.tsv` (`0041E6D3` size 11396, `004184BD` size 378);
`docs/runtime/FORWARD_TREE.md` §3 / §4 / §6;
`proofs/input-vtbl56-vs-ui32/README.md`;
`proofs/vtbl584-post-hop/README.md`;
`proofs/0055B9D0-post-dword/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove type 4 → `0055CB10(26)`, persist `0x53C644E4` →
`0x126` / 15, or `0059A238` consume of `0xE5` / `0x126` / 15.

---

## Verdict

**Yes.** On first-seen frontend, `[0x13B86A0]` is **0** for the
whole retail pump. Any live entry to `0041E6D3` therefore takes

```
0041E6FB  [0x13B86A0]==0
0041E705  00595582                  ; UI singleton [0x13B8B5C]
0041E70F  call [edx+32]             ; 012521A8+32 = 0059A238
```

The in-game skip (`jne 0041E718`) cannot fire until
`004184BD` writes the game pointer, and that write is **after**
Leave (`0042F2A2` → `0042F491` Init Game).

| Claim | Status |
| --- | --- |
| `0041E6D3` gates UI on `[0x13B86A0]` | **PROVEN** |
| `==0` → `00595582` then UI `vtbl+32` | **PROVEN** |
| That `vtbl+32` is `0059A238` (`012521A8+32` = `012521C8`) | **PROVEN** |
| `jne 0041E718` skips **only** the UI hop | **PROVEN** |
| `0041E718` is exclusive in-game code | **DISPROVEN** — it is the **join** + id switch |
| `.text` writers of `[0x13B86A0]`: `004184D1` (game) and `00417663` (0) | **PROVEN** (two `mov [0x13B86A0]` only) |
| First-seen RunModes takes retail `0042EA8F`, not `00418DCA` | **PROVEN** |
| First-seen frontend therefore `[0x13B86A0]==0` | **PROVEN** |
| Live packet (`[boxed+12]≠0`) + first-seen frontend → UI hop | **PROVEN** |
| Dead packet (`[boxed+12]==0`) still takes UI | **DISPROVEN** — `je 00426DFC` |
| First-seen Press Start type 4 enters `0041E6D3` | **DISPROVEN** (`input-vtbl56-vs-ui32`) |

**Answer:** first-seen frontend **always** takes the UI path
**when `0041E6D3` is entered with a live packet.** The gate cannot
choose `0041E718` until Init Game. Type-4 Press Start does not
use this function at all.

---

## 1. Dump `0041E6D3` — gate then join

`listing-00400000.txt`:

```
0041E6D3  push ebp
          lea ebp, [esp-116]
          sub esp, 0x800
0041E6E6  mov edi, [ebp+124]       ; arg = pair* / &packet*
0041E6EC  mov eax, [edi]
0041E6EE  mov al, [eax+12]         ; [boxed+12]
0041E6F1  test al, al
0041E6F5  je 00426DFC              ; dead: no UI, no switch
0041E6FB  mov esi, [0x13B86A0]
0041E701  test esi, esi
0041E703  jne 0041E718             ; skip UI hop
0041E705  call 00595582
0041E70A  mov edx, [eax]
0041E70C  push edi
0041E70D  mov ecx, eax
0041E70F  call [edx+32]            ; 0059A238
0041E712  mov esi, [0x13B86A0]
0041E718  mov ecx, [ebp+124]
0041E71B  mov edi, [ecx]
0041E71D  mov eax, [edi]           ; id
0041E71F  mov edx, 0xD8
          … huge switch …
```

Three exits from the prologue:

| Condition | Next | UI `vtbl+32` |
| --- | --- | --- |
| `[boxed+12]==0` | `00426DFC` epilogue | no |
| `[0x13B86A0]≠0` | `0041E718` | no |
| `[0x13B86A0]==0` | `00595582` → `[edx+32]` then `0041E718` | yes |

`0041E718` is **not** “in-game only”. It is the fall-through
after the hop **and** the `jne` target. Frontend still runs the
id switch (`cmp eax, 0xD8` / `jmp [0x426E0E+…]`). Later cases
re-test the singleton (e.g. `0041F754`, `00420826 test esi`).
Those are **not** the entry gate.

`0042BE50` inits `[boxed+12]=1`. Persist list nodes
(`0055B520` / `widget-plus372-list`) copy that boxed pair.
A live `vtbl+56` post therefore does not take the dead-packet
exit.

---

## 2. UI getter and `vtbl+32`

`listing-00580000.txt`:

```
00595582  mov eax, [0x13B8B5C]
          test eax, eax
          jne 005955AA
          alloc 0xE0 → 005953E2          ; vtbl 012521A8
          mov [0x13B8B5C], eax
          ret
```

`EngineLifecycle.FrontendUiVtbl = 0x012521A8`. Slot +32 is
VA `012521C8` = `0059A238` (`FORWARD_TREE`,
`Frontend_0059A238_*` tests).

`0059A238` double-derefs the same pair `0041E6D3` pushed:

```
0059A281  mov eax, [ebp+8]         ; pair*
0059A284  mov eax, [eax]           ; boxed*
0059A286  mov ecx, [eax]           ; id
```

First-seen frontend already constructed the UI
(`0042E98F` / `00595582`). The getter returns
`[0x13B8B5C]`; it does not skip `vtbl+32`.

---

## 3. Who writes `[0x13B86A0]`

Every `listing-*.txt` `mov [0x13B86A0]`:

| Site | Insn | Value | Function |
| --- | --- | --- | --- |
| `004184D1` | `mov [0x13B86A0], esi` | game `this` | `004184BD` GameStart (`0122F180+4`) |
| `00417663` | `mov [0x13B86A0], ebx` | **0** (`xor ebx, ebx` at `004175EA`) | `004175E5` GameMode teardown |

No other store. No `lea` / `push 0x13B86A0` writer in
`.text`. Ctor `00418DCA` writes vtbl `0122F180` and zeros
fields; it does **not** publish the singleton.

`004184BD` callers: no `E8`. Only `call [vtbl+4]` after a
`00418DCA` alloc:

```
0042F491  "Init Game"
0042F4B1  push 0x161E8
0042F4C7  call 00418DCA
0042F4D2  call [eax+4]             ; 004184BD → 004184D1
```

That block is **after** Leave `0042EBB6` (`0042F48A`).

The other `00418DCA` + `[edx+4]` site is RunModes
`00412F90` when a skip-frontend flag is set (`§4`). That is
**not** first-seen frontend.

Clear `004175E5` callers: `00418B61`, `00418B90` (GameMode
dtor / fail). No GameMode object exists on first-seen
frontend, so the clear never runs there either.

BSS / PE default of `0x13B86A0` is 0. Combined with the two
stores: the dword is 0 from process start until the first
`004184BD`, then game, then 0 again on GameMode teardown.

---

## 4. First-seen frontend never publishes the singleton

`00412F90` RunModes (`FORWARD_TREE` §3):

```
[0x13B8648] ≠ 0  → 00418DCA + vtbl+4     ; skip frontend
else [0x13B8605] ≠ 0 → same
else [0x13B8642] ≠ 0 → 00496070 LOAD
else                 → 0042EA8F retail    ; first-seen
```

First-seen defaults: all three flags 0 (`FORWARD_TREE` header;
`particles-game`). Retail ctor `0042EA8F` / pump `0042EC7C`
never `mov [0x13B86A0]`.

Flag writers in `listing-00400000.txt` (`00413A8C` /
`00413B55` → `0x13B8648`; `00413C3B` → `0x13B8605`) are
command-line / setup, not the no-save Press Start walk.

Leave is the first time this process hits Init Game:

```
0042EC7C  [retail+41] ≠ 0
  0042F2A2  Leave frontend
    0042EBB6  teardown
    0042F491  Init Game
      00418DCA
      vtbl+4 004184BD
        [0x13B86A0] = game          ; FIRST non-zero
```

Until that store, every `0041E6D3` live entry takes
`00595582` → `0059A238`. Independent corroboration:
`004069E0` on New Profile (`00851770`) also reads
`[0x13B86A0]==0` and falls through to UTF-16 `0x122DE80`
`"Default"` (`FORWARD_TREE` §4).

---

## 5. Who enters `0041E6D3` on frontend (not type 4)

`0041E6D3` is input vtbl `01230134+56`
(`FrontendInputMap.InputVtblMessageFn`). Type 4 is
`call [edx+0]` = `0055CB10`, never +56
(`input-vtbl56-vs-ui32`).

Known `call [edx+56]` posters:

| Site | Role | First-seen Press Start |
| --- | --- | --- |
| `00558DFF` (`00558DE0`) | type-11/38 `vtbl+524` walk `&node+8` | no type 38; type-11 INVISIBLE `+224` empty |
| `005403EF` | type-12 key 1, list `+352` | empty sentinel |
| `005405ED` | type-12 key 28, list `+348` | type 1 / action 33, not type 4 |

When those posters **do** run on frontend (New Game click
15 / Accept `0x126` via `0055AF60` → `00558DE0`), the
`§1` gate is still `[0x13B86A0]==0`, so the posted boxed
id reaches `0059A238`.

---

## 6. What this does **not** say

- `0041E6D3` is the Press Start `0xE5` consumer.
  **DISPROVEN** — that is `0054E2FA` → `0059A238` directly.
- `0041E718` means “skip the id switch on frontend”.
  **DISPROVEN** — frontend runs the switch after the hop.
- A non-zero `[0x13B86A0]` during first-seen menus.
  **DISPROVEN** by the two writers + RunModes split.
- Skip-frontend flags (`0x13B8648` / `0x13B8605`) still
  count as “first-seen frontend”. Those never attach retail
  UI; they call `004184BD` immediately.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0041E6D3` | input `vtbl+56`; UI hop iff game==0 | **PROVEN** |
| `0041E703` | `jne 0041E718` skip UI | **PROVEN** |
| `0041E718` | join + id switch | **PROVEN** as join; **DISPROVEN** as in-game-only |
| `0041E6F5` | dead `[boxed+12]` → `00426DFC` | **PROVEN** |
| `00595582` | UI getter `[0x13B8B5C]` | **PROVEN** |
| `0059A238` | UI `012521A8+32` | **PROVEN** |
| `0x13B86A0` | game singleton | **PROVEN** |
| `004184D1` | only non-zero writer | **PROVEN** |
| `00417663` | only zero writer | **PROVEN** |
| `0042F4D2` | first first-seen `004184BD` | **PROVEN** (after Leave) |

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
  (`0041E6D3`, `004184BD`, `004175E5`, `00412F90`, `0042F491`,
  `00418DCA`, `0042BE50`, `004069E0`)
- `listing-00580000.txt` (`00595582`, `0059A238`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/functions.tsv`
- `docs/runtime/FORWARD_TREE.md`
- `proofs/input-vtbl56-vs-ui32/README.md`
- `proofs/vtbl584-post-hop/README.md`
- `proofs/0055B9D0-post-dword/README.md`
- `proofs/widget-plus372-list/README.md` (`[boxed+12]=1`)
