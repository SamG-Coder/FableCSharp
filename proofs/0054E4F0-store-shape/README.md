# `0054E4F0` stores packet* at type-10 +352, not dword `0xE5`

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `0054E4F0` / `0054E3D0` / `0054E410` /
`0054E450` / `0054E280` (`0054E2FA`)
(`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`);
`00598A1C` / `00598EE6` / `0059A238`
(`listing-00580000.txt`);
`0042AA29` / `0042BE50`
(`listing-00400000.txt`);
`src/Fable.Game/EngineLifecycle.cs`
(`WriteType10AttachMessage`, `AttachPressStartWidgets`);
`src/Fable.Game/FrontendInputMap.cs`
(`Type10StoreMsgFn`, `Type10StoredMsgOffset`,
`MessageFromType10Attach`);
`src/Fable.Game/IEngineHost.cs` (`FrontendWidget.MessageId`);
`implementer/frontend/05-input.md`;
`proofs/type10-plus352/README.md`;
`proofs/press-start-e5-attach/README.md`;
`proofs/00598A1C-only-e5/README.md`;
`proofs/slot-0x14-lookup/README.md`;
`tests/Fable.Formats.Tests/FrontendInputTests.cs`.

Do not re-prove type 4 → action 26, persist `+224` is 0 on
`UI_FRONTEND_PRESS_START_MENU`, `00598EE6` lives only in
`00598A1C`, or `0059A238` `0xE5` → `00599D5C`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

---

## Verdict

**Exact store at type-10 `vtbl+284` `0054E4F0`:**

```
widget+352 = packet*     ; ebx = [wrapper+0]
widget+356 = ctrl*       ; edi = [wrapper+4]
```

**Not** `widget+352 = 0xE5`. The attach site writes the
immediate into **`[packet+0]`** before the call. Action 26
posts `&widget+352` (a `packet**`). `0059A238` double-derefs
to the id.

Host `WriteType10AttachMessage` writes `FrontendWidget.MessageId
= 0xE5`. That is the **id**, not the packet*. First-seen posted
value **MATCH**es. The field shape is **LEFTOVER**.

`implementer/frontend/05-input.md` “stores id at widget+352” is
**STALE** on the dword. Offset 352 is right; the stored word is
the pointer.

| Claim | Status |
| --- | --- |
| `0054E4F0` is type-10 `vtbl+284` | **PROVEN** body (`this`=widget, `ret 4`); `.rdata` dword `012497E4+284` **UNREAD** this pass |
| `mov [esi+352], ebx` stores wrapper `[0]` | **PROVEN** listing |
| Wrapper `[0]` is heap packet* (`00BFEA1A` / `0042BE50`) | **PROVEN** `0042AA29` `mov [esi], eax` |
| Attach writes `0xE5` at `packet[0]`, not at +352 | **PROVEN** `00598EE6` `mov [eax], 0xE5` after `eax=[ebp-56]` |
| `+352` holds dword `0xE5` | **DISPROVEN** |
| `+356` is ctrl* with refcount | **PROVEN** `inc [edi]` / dtor `dec [eax]` |
| Ctor zeros +352 / +356 | **PROVEN** `0054E3F3` |
| Action 26 posts `&widget+352` | **PROVEN** `0054E2FA` |
| `0059A238` needs a `packet**` (two loads) | **PROVEN** `mov eax,[ebp+8]` / `[eax]` / `[eax]` |
| Immediate `0xE5` at +352 would make the second load `[0xE5]` | **DISPROVEN** as a store shape |
| Host `MessageId = 0xE5` is native packet* | **DISPROVEN** leftover |
| First-seen posted id is still `0xE5` | **MATCH** |

**Answer:** packet*. Host `MessageId` is leftover vs that
pointer.

---

## 1. `0054E4F0` body (widget `this`, `ret 4`)

```
0054E4F0  mov eax, [esp+4]       ; &wrapper
0054E4F4  push ebx
0054E4F5  mov ebx, [eax]         ; wrapper[0]
0054E4F7  push esi
0054E4F8  push edi
0054E4F9  mov edi, [eax+4]       ; wrapper[4]
0054E4FC  mov esi, ecx           ; this = widget
0054E4FE  mov eax, [esi+356]     ; old ctrl*
0054E504  cmp eax, edi
0054E506  je  0054E540           ; same ctrl → skip both stores
0054E508  test eax, eax
0054E50A  je  0054E52E
          … release old ctrl* (dec / maybe free) …
0054E52E  test edi, edi
0054E530  mov [esi+352], ebx     ; +352 = wrapper[0]
0054E536  mov [esi+356], edi     ; +356 = wrapper[4]
0054E53C  je  0054E540
0054E53E  inc [edi]
0054E540  pop edi
0054E541  pop esi
0054E542  pop ebx
0054E543  ret 4
```

No load of `[ebx]`. No `0xE5` immediate. The stored dword
at +352 is **ebx**, which is **wrapper[0]**.

Same-ctrl skip (`je 0054E540`) leaves the previous pair.
First-seen ctor left +356 = 0, so the attach call stores.

Generic `0122F5D4+284` `0052F040` `ret 4` is a different
slot (**DISPROVEN** here). Type 11/38 `+352` is a selected
**u8** (`type11-plus352-select`), not this pair.

---

## 2. Wrapper `[0]` is packet*, not the id

Attach `00598A1C` (`listing-00580000.txt`):

```
00598EC3  push 16
00598EC5  call 00BFEA1A          ; 16-byte heap
00598ECF  mov ecx, eax
00598ED1  call 0042BE50          ; [packet]=0, rest ctor
00598EDA  push eax               ; packet*
00598EDB  lea ecx, [ebp-56]
00598EDE  call 0042AA29          ; wrapper {packet*, ctrl*}
00598EE3  mov eax, [ebp-56]      ; wrapper[0] = packet*
00598EE6  mov [eax], 0xE5        ; packet[0] = 0xE5
00598EF2  mov [ebp+108], 0x14
00598EF9  call 0059B5D7          ; slot 0x14 → type-10*
00598EFE  mov ecx, [eax]
00598F00  mov eax, [ecx]         ; widget vtbl
00598F02  lea edx, [ebp-56]
00598F05  push edx               ; &wrapper
00598F06  call [eax+284]         ; 0054E4F0
```

`0042AA29` (`listing-00400000.txt`):

```
0042AA29  mov eax, [esp+4]       ; packet*
0042AA30  mov esi, ecx           ; wrapper
0042AA32  mov [esi], eax         ; wrapper[0] = packet*
          … alloc 12-byte ctrl* …
0042AA58  mov [esi+4], eax       ; wrapper[4] = ctrl*
```

So `ebx` in `0054E4F0` is the **16-byte heap**, not `0xE5`.

If wrapper[0] were already `0xE5`, `00598EE6` `mov [eax], 0xE5`
would store through address `0xE5`. That is **DISPROVEN**.
The immediate is a write **through** the pointer.

`0042BE50` starts with `and [esi], 0`. The id is not in the
packet until `00598EE6`.

---

## 3. +352 is a pointer slot, not an id dword

Ctor `0054E3D0` (vtbl `012497E4`):

```
0054E3DD  xor eax, eax
0054E3F3  mov [esi+352], eax     ; packet* = 0
0054E3F9  mov [esi+356], eax     ; ctrl*  = 0
0054E3FF  mov [esi+360], eax
```

Copy-ctor `0054E410` zeros the same pair. Dtor `0054E450`
releases **ctrl*** then `mov [esi+352], 0`. Refcount lives
on +356 (`dec [eax]` / `inc [edi]`). An id dword at +352
does not need that.

Action 26 (`0054E280` `ecx` = inner = widget+4):

```
0054E2FA  mov eax, [edi+348]     ; widget+352
0054E300  test eax, eax
0054E303  lea esi, [edi+348]     ; &widget+352
0054E309  je  0054E318           ; skip if packet* == 0
0054E312  push esi
0054E315  call [edx+32]          ; 0059A238
```

`test eax,eax` is “is there a packet?”, not “is the id 0?”.
Ctor 0 + skipped attach ⇒ no post.

`0059A238`:

```
0059A281  mov eax, [ebp+8]       ; &packet*
0059A284  mov eax, [eax]         ; packet*
0059A286  mov ecx, [eax]         ; packet[0] = id
```

Two loads. `+352 = 0xE5` would make the second load
`[0xE5]`. That is **DISPROVEN**.

Layout:

```
widget+0      012497E4     type-10 vtbl
widget+4      012497BC     inner (0054E280 this)
widget+352    packet*      0054E4F0 ebx / 0054E2FA test
widget+356    ctrl*        0054E4F0 edi
[packet+0]    0xE5         00598EE6 only
```

---

## 4. Host `MessageId` leftover vs packet

Native attach:

1. alloc 16-byte packet
2. wrap `{packet*, ctrl*}`
3. `[packet] = 0xE5`
4. slot `0x14` `vtbl+284` → `+352/+356`

Host `WriteType10AttachMessage`
(`EngineLifecycle.cs`, only from `AttachPressStartWidgets`):

```
Note(… 00598EE6 slot 0x14 vtbl+284 0054E4F0 +352 0xE5)
if widgets[0].Type == 10 && MessageId == 0
  widgets[0].MessageId = 0xE5
```

`FrontendWidget.MessageId` is an `int` (`IEngineHost.cs`).
There is no heap packet, no +356, no `vtbl+284` call.

`MessageFromType10Attach` returns that `int` on the first
visible type-10 with `MessageId != 0`. Native posts
`&+352` and lets `0059A238` load `[packet]`. First-seen
Press Start id is still `0xE5` (**MATCH**). The stored
word is **LEFTOVER**.

Call-site (only Press Start attach) is already **MATCH**
(`00598A1C-only-e5`). Do not put the write in
`AttachFrontendTree`. Do not treat `MessageId` as
`widget+352` when inventing a later packet object.

`05-input.md` line “Type-10 `012497E4+284` = `0054E4F0`
stores id at widget+352” collapses the two words. Offset
and fn **MATCH**. Stored type is packet*.

---

## Tests / leftover

`FrontendInputTests` locks `Type10StoreMsgFn=0x0054E4F0`
and `Type10StoredMsgOffset=352`. It does not lock a
packet* vs dword at that offset. Host still posts the
stand-in `MessageId`.

Keep `WriteType10AttachMessage`. Do not change it to
store `0xE5` “at +352” as a raw dword and then pass
that dword to a consume that double-derefs.

## Do not invent

- `mov [widget+352], 0xE5` in `.text`.
- Treating type-10 +352 like type 11/38 selected u8.
- Treating type-10 +352 like type-34 list `+372`.
- Persist `0xE5` on the type-10 PRESS_START def.
- Host packet* / ctrl* pair (not present; leftover is the
  collapsed `MessageId`).
- Deleting the attach analog because the field name is
  `MessageId`.
