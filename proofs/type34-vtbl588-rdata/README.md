# Type 11/34/38 `.rdata` `vtbl+584` / `+588` — is the dword `0055ACF0` / `0055AF60`?

Investigation only. No production `src/` edits.

Authority: `Fable.exe` identity `42D7DBDF-0106C000-16666624`
(`tools/Fable.ExeIndex/out/01-sections/text-map/INDEX.md`);
`out/00-index/sections.txt`;
`listing-00540000.txt` (`0054DBC0` / `0054DD50` / `0054DDB0` /
`0054E0B0` / `00557850` / `00558B90` / `0055A5D0` / `0055A660` /
`0055A726` / `0055A73B` / `0055ACF0` / `0055AD60` / `0055AF60` /
`0055B460`);
`listing-01200000.txt` (`.text` tail only);
ExeIndex `vtbl` reader (`Program.cs` `RunVtbl` /
`WriteVtblPart`); landscape-trace
`vtbl-cenginelandscaperenderer-012a2b54.md` (rdata **does**
print when dumped);
`src/Fable.Formats/Defs/FrontendWidgetType.cs` type 38
`AcceptVtbl = 0124B04C`;
`src/Fable.Game/FrontendInputMap.cs` `Type34ClickFn` /
`Plus228PostFn`;
`proofs/vtbl584-post-hop/README.md`,
`proofs/type34-vtbl524/README.md`,
`proofs/0055A726-plus228-jmp/README.md`,
`proofs/00557AF0-caller/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE**.

Do not re-prove action 26 → `vtbl+584` or action 28 →
`vtbl+588` (`0055AD60`). Do not re-prove `0055AF60` posts
`[+372]` / `0055ACF0` posts `[+380]`.

---

## Verdict

**This pass did not print the `.rdata` dwords.** `read_file`
rejects `Fable.exe` (binary). `listing-01200000.txt` is still
`.text` INT3 pad (`.text` ends `0x0122CFFF`; `.rdata` starts
`0x0122D000`). No `WriteVtblPart` / stdout `vtbl` dump of
`0124B04C` / `01249554` / `0124BD2C` exists under
`tools/Fable.ExeIndex/out/`.

| Question | Answer |
| --- | --- |
| Type 38 `0124B04C+588` = **`0055ACF0`**? | **PARTIAL** — unique 0-arg `+380` / unmap-28 body; no type-38 local clone. Dword **UNREAD**. |
| Type 34 `0124BD2C+588` = **`0055ACF0`**? | **PARTIAL** — same; `0055ACF0` lives in the type-34 cluster. Dword **UNREAD**. |
| Type 11 `01249554+588` = **`0055ACF0`**? | **PARTIAL** — competing local **`0054DDB0`** also 0-arg posts `[+380]`. Dword **UNREAD**. |
| Type 38/34 `+584` = **`0055AF60`**? | **PARTIAL** — unique 0-arg `+372` click; type 40 thunks it (`00557850 jmp 0055AF60`). Dword **UNREAD**. |
| Type 11 `01249554+584` = **`0055AF60`**? | **PARTIAL** — competing local **`0054DD50`**. Dword **UNREAD**. |

`FrontendInputMap.Type34ClickFn = 0055AF60` and
`Plus228PostFn = 0055ACF0` name the **bodies**, not the rdata
pointers.

`type6-action28` “`vtbl+588` posts no UI message” is **STALE**
if the live slot is `0055ACF0` (or type-11 `0054DDB0`): both
`call [vtbl+524]([+380])`. First-seen still skips the call
when `[+364]==0`.

Dump (stdout; does not persist unless `WriteVtblPart`):

```
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x0124B04C 160
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x01249554 160
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x0124BD2C 160
```

Need slots `[146] +584` and `[147] +588`.

---

## 1. Where the tables live

`sections.txt`: `.rdata` `rva=file=0xE2D000` `size=1339392`.
Image base `0x00400000` (exeId `SizeOfImage=0x0106C000`).
File offset of a `.rdata` VA is `VA-0x400000`.

Ctors (`listing-00540000.txt`):

```
0055B471  mov [esi], 0x124BD2C      ; type 34 outer
0055B477  mov [esi+4], 0x124BD08
0054E0BF  mov [esi], 0x1249554      ; type 11 after call 0055B460
0054E0C5  mov [esi+4], 0x1249530
00558B9D  mov [esi], 0x124B04C      ; type 38 after call 0055B460
00558BA3  mov [esi+4], 0x124B024
```

Type 11/38 inherit type 34 then overwrite the three vtbl
dwords. Runtime `call [outer+584/+588]` uses the **final**
table, not `0124BD2C`.

| Vtbl | Type | Role | `+584` VA (slot 146) | file | `+588` VA (slot 147) | file |
| --- | ---: | --- | --- | --- | --- | --- |
| `0124B04C` | 38 | `AcceptButton` / `00558B90` | `0124B294` | `0xE4B294` | `0124B298` | `0xE4B298` |
| `01249554` | 11 | `CFrontEndButton` / `0054E0B0` | `0124979C` | `0xE4979C` | `012497A0` | `0xE497A0` |
| `0124BD2C` | 34 | base (`0055B460`); persist-time | `0124BF74` | `0xE4BF74` | `0124BF78` | `0xE4BF78` |

RTTI `CFrontEndButton@NUISystem` is `0137C128` (`.data`). COL
for these tables was **not** dumped.

ExeIndex `vtbl` **can** read this range: landscape-trace
printed `012A2B54` (`[0] 00B6CAB0` …). These three VAs were
never requested.

---

## 2. `0055AD60` — `+584` is click, `+588` is unarm

Inner apply (`ecx = widget+4`):

```
0055AD60  mov edi, [esp+12]         ; action
          lea eax, [edi-26]
          cmp eax, 6
          ja  0055AE79
          jmp [0x55AE88+eax*4]
0055AD7B  ; action 26
          test [esi+348], al
          je  skip
          lea ecx, [esi-4]
          call [eax+584]            ; 0-arg outer
          mov [esi+364], 1
          call 0055B9D0
0055ADDE  ; action 28
          test [esi+364], al
          je  stamp
          lea ecx, [esi-4]
          call [edx+588]            ; 0-arg outer
          mov [esi+364], 0
```

Type 38 inner **is** `0055AD60`. Type 11 inner `0054DBC0`
forwards to it when `[def+545]≠0`:

```
0054DC15  push [esp+12]
          mov ecx, esi
          call 0055AD60
```

So type 11/38 action 26/28 **do** hit the outer slots. Which
function the slot holds is the rdata question.

---

## 3. Expected `+584` = `0055AF60` (type 34/38)

`0055AF60` is 0-arg (`ret`, not `ret 4`). Unique in the
type-34 cluster: latch `[+364]`, `vtbl+192([def+524])`,
`push [this+372]`, `call [vtbl+524]`, `inner.vtbl+12(28)`.

```
0055AF60  push ecx
          push esi
          mov esi, ecx
          mov eax, [esi+328]
          mov [esi+364], eax
          …
          push [esi+372]
          call [eax+524]
          push 28
          call [edx+12]
          ret
```

Type 40: `00557850 jmp 0055AF60`. Type 35 wrap: `0055A5D3
call 0055AF60`. Type 38 `.text` after `00558BB4` has **no**
local 0-arg `+372` poster.

Wanted dword at `0124B294` / `0124BF74`: **`0055AF60`**.

Type 11 also has a slim 0-arg poster:

```
0054DD50  ; if [def+545]
          push [esi+372]
          call [vtbl+524]
          ret
```

No SelectState, no map-28. If `0124979C` is `0054DD50`,
action 26 still posts `+372` through `+524`; only the
side effects drop. **Cannot** pick between `0055AF60` and
`0054DD50` without the dword.

---

## 4. Expected `+588` = `0055ACF0` (type 34/38)

`0055ACF0` is 0-arg. Inverse of the click body:

```
0055ACF0  push esi
          mov esi, ecx
          push [esi+364]
          call [eax+192]            ; SelectState
          lea ecx, [esi+4]
          push 28
          call [edx+16]             ; unmap 28
          push [esi+380]
          call [eax+524]            ; post +228 list
          ret
```

`.text` uses: `00557AF4 call 0055ACF0`; type-35 `0055A660`
tails `0055A726` / `0055A73B jmp 0055ACF0`. No `E8` /
`jmp` from `0055AF60` or `0055AD60`.

Type 38 has **no** local `+380` poster. Type 34 cluster owns
`0055ACF0` (immediately before `0055AD60`).

Wanted dword at `0124B298` / `0124BF78`: **`0055ACF0`**
(or a 5-byte `jmp` to it; none sits in this cluster).

Type 11 competing slim body:

```
0054DDB0  ; if [def+545]
          push [esi+380]
          call [vtbl+524]
          ret
```

Same post, no unmap-28 / SelectState. Next sibling
`0054DE10` posts `[+392]` (hover list). That trio is the
type-11 local `+584/+588/+592` **shape**. So
`012497A0 == 0055ACF0` is **not** the only live candidate.

Type 35 `0124BA94+588` should be **`0055A660`** (wrap + tail
`jmp 0055ACF0`), not `0055ACF0` itself. Not dumped here.

---

## 5. What a printed dump must show

| Slot | Expected | Competing |
| --- | --- | --- |
| `0124B04C+584` / `0124BD2C+584` | `0055AF60` | none in type 38/34 |
| `0124B04C+588` / `0124BD2C+588` | `0055ACF0` | none in type 38/34 |
| `01249554+584` | `0055AF60` | `0054DD50` |
| `01249554+588` | `0055ACF0` | `0054DDB0` |

Also print `[131] +524` while there (`0124B258` /
`01249760` / `0124BF38`); `type34-vtbl524` wants
`00558DE0`.

If type 38/34 `+588` is **not** `0055ACF0`, the ABI story
(action 28 unarm posts `[+380]`) still holds only if the
printed body does that `push [+380]; call [+524]`. If it is
a nop / `0055B9D0`, `plus380-poster` / `0055A726-plus228-jmp`
need a rewrite.

---

## 6. What this is not

| Claim | Status |
| --- | --- |
| `0055ACF0` is `vtbl+524` | **DISPROVEN** — it *calls* `+524` |
| `0055ACF0` is `vtbl+584` / action 26 | **DISPROVEN** — that body is `0055AF60` |
| `0055AF60` leads to `0055ACF0` | **DISPROVEN** (`action28-after-26`) |
| First-seen Accept / New Game runs `+588` | **DISPROVEN** when `[+364]==0` (ctor) |
| `.rdata` dword printed this pass | **UNREAD** |

---

## Leftovers

- Run the three `ExeIndex vtbl` commands; paste `[146]` /
  `[147]` (and `[131]`) into this file and flip **UNREAD** →
  **PROVEN** / **DISPROVEN**.
- Same for type 35 `0124BA94+584/+588` (`0055A5D0` /
  `0055A660`).
- Inner tables `0124B024` / `01249530` / `0124BD08` are
  action maps, not these outer slots.
