# Press Start type-10 persist is 0; attach `00598EE6` writes `0xE5`

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `00598A1C` / `00598BA2` / `00598EE6` /
`0059B5D7` / `0054E3D0` / `0054E4F0` / `0054E280` / `0054E2FA`
(`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`,
`listing-00540000.txt`);
`frontend.bin` `UI_FRONTEND_PRESS_START_MENU` /
`UI_FRONTEND_BUTTON_INVISIBLE`
(`implementer/frontend/persist-scan.txt`);
`src/Fable.Game/EngineLifecycle.cs`
(`AttachFrontendTree`, `MaybeActivateNewGameFromInput`);
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Formats/Defs/FrontendUiDef.cs`;
`proofs/type10-plus352/README.md`;
`proofs/audit-lifecycle-input/README.md`;
`proofs/audit-messageid-parse/README.md`;
`proofs/list-type12-focus/README.md`;
`tests/Fable.Formats.Tests/FrontendInputTests.cs`,
`EngineLifecycleTests.cs`.

Do not re-prove type 4 → action 26, Return ≠ `0xE5`,
or `0059A238` consume (`0xE5` → `00599D5C`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

---

## Verdict

**Persist on the type-10 does not store `0xE5`.** File CRC
`0x53C644E4` (`def+224`) on `UI_FRONTEND_PRESS_START_MENU` is
**0**. Native `0xE5` is attach `00598EE6` `mov [eax],0xE5` into a
heap packet, then slot `0x14` `vtbl+284` `0054E4F0` stores that
packet* at **widget+352**. Ctor `0054E3D0` zeros +352; without
the attach write, type-10 action 26 is a no-op.

Host `AttachFrontendTree` (`root == PRESS_START && MessageId == 0`
→ `0xE5`) is therefore **required** as the C# analog of
`00598EE6`, not a persist fill-in. **Do not delete it.**

`MaybeActivateNewGameFromInput` screen-name fallback
(`mapped is null && action 26 && FrontendMenuRoot == PRESS_START`
→ hard `0xE5`) is **LEFTOVER**. Native has no such fork.
**Do not delete it yet:** `Frontend_press_start_type4_posts_0xE5_then_new_profile`
bootstraps with `null` (empty widgets). Install type 4 already
posts from the attach-patched root.

| Claim | Status |
| --- | --- |
| Type-10 PRESS_START persist `+224` / CRC `0x53C644E4` is **0** | **PROVEN** file hex `E444C653 00000000` |
| Persist already stores `0xE5` on that type-10 | **DISPROVEN** |
| Attach `00598EE6` writes packet `[0]=0xE5` then slot `0x14` `vtbl+284` | **PROVEN** listing |
| `0054E4F0` stores packet* at widget+352 (not persist) | **PROVEN** |
| Type-10 ctor zeros +352 | **PROVEN** `0054E3F3` |
| Action 26 skips post when +352 is 0 | **PROVEN** `0054E2FA` `test eax,eax` |
| Host attach patch is the C# analog of `00598EE6` | **MATCH** first-seen (field is `MessageId`, not a packet*) |
| Host patch can be dropped because persist is `0xE5` | **DISPROVEN** |
| Type-11 `UI_FRONTEND_BUTTON_INVISIBLE` persist **is** `0xE5` / 229 | **PROVEN** other widget |
| `MaybeActivate` PRESS_START fallback is native | **DISPROVEN** screen-name fork |
| Fallback can be deleted while no-install type-4 exists | **DISPROVEN** (test would fail) |
| Install type 4 needs the fallback if attach patch stays | **DISPROVEN** dead arm |

---

## 1. File persist on the type-10 is 0

`UI_FRONTEND_PRESS_START_MENU` `#620` type **10**, raw=1710
(`persist-scan.txt`). Sequential CRC walk of that blob is
desynced after styles (`export/frontend/persist-tail.txt` never
lists `0x53C644E4` on this entry). Byte scan / hex does.

Hex window (little-endian CRC then i32):

```
D6640323 00000000   ; sibling dword 0
E444C653 00000000   ; 0x53C644E4 + i32 0   ← MessageIdCrc
1E0AECEC 00000000
```

`FrontendUiDef.ReadPersistI32` / factory `MessageId` therefore
leave the root at **0**. `0055B040` copies `[def+224]` only when
nonzero; type-10 ctor never calls that helper
(`audit-messageid-parse`). Persist is **not** `0xE5`.

Same CRC on the list child `UI_FRONTEND_BUTTON_INVISIBLE` `#625`
type **11**:

```
E444C653 E5000000   ; 0x53C644E4 + i32 0xE5
```

Action CRC `0xF1A22807` @1089 is also **229 / `0xE5`**. That is
a **different** widget. Do not treat it as type-10 persist.

NEW_PROFILE / MAIN_MENU type-10 roots are also persist **0** and
are **not** written by `00598EE6`. C# does not patch those names
— **MATCH**.

---

## 2. Native attach write (`00598A1C`)

`00598A1C` builds Press Start into UI slot **`0x14`**:

```
00598BA2  push "UI_FRONTEND_PRESS_START_MENU"
00598BB7  mov [ebp+108], 0x14
00598BBE  call 0059B5D7
00598BD2  call 0041DB1D          ; factory
```

Later in the same function, after other menus:

```
00598EC3  push 16
00598EC5  call 00BFEA1A          ; packet
00598ED1  call 0042BE50          ; [packet]=0
00598EDE  call 0042AA29          ; wrapper {packet*, ctrl*}
00598EE3  mov eax, [ebp-56]
00598EE6  mov [eax], 0xE5        ; packet[0] = 0xE5
00598EF2  mov [ebp+108], 0x14
00598EF9  call 0059B5D7          ; slot 0x14 widget
00598F00  mov eax, [ecx]         ; widget vtbl
00598F05  push edx               ; &wrapper
00598F06  call [eax+284]
```

Type-10 `012497E4+284` = `0054E4F0` (generic `0122F5D4+284` =
`0052F040` `ret 4` is **DISPROVEN** here):

```
0054E4F0  mov ebx, [eax]         ; packet*
          mov edi, [eax+4]       ; ctrl*
0054E530  mov [esi+352], ebx
0054E536  mov [esi+356], edi
```

Ctor first zeros the slot:

```
0054E3D0  xor eax, eax
0054E3F3  mov [esi+352], eax
```

Action 26 (`0054E2FA`) posts `&widget+352` only if the packet*
is nonzero. Persist 0 + skipped attach ⇒ type-10 never posts.

C# collapses persist `+224` and type-10 +352 into one
`FrontendWidget.MessageId`. The attach analog therefore writes
that field, not a heap packet. First-seen id **MATCH**es;
layout is **LEFTOVER**.

---

## 3. Host attach patch — keep

`EngineLifecycle.AttachFrontendTree`:

```
if built.Count > 0 &&
   rootName == UI_FRONTEND_PRESS_START_MENU &&
   built[0].MessageId == 0
  built[0].MessageId = 0xE5
```

Called from `AttachPressStartWidgets` after
`InitFrontendUi` `00598A1C` notes.

| If deleted | Effect |
| --- | --- |
| Factory root stays 0 | `MessageFromWidgets` skips type-10 (`MessageId==0`) |
| Install still has INVISIBLE persist `0xE5` | First-visible leftover can still post `0xE5` from type **11** (**PARTIAL** vs native type-10 attach) |
| `Type4_drives_lifecycle_…` asserts root `MessageId==0xE5` | **fails** |
| Native type-10 +352 | stays 0 without `00598EE6` analog |

Keep the name gate on PRESS_START only. Do not copy `0xE5`
onto NEW_PROFILE / MAIN_MENU type-10 roots.

---

## 4. `MaybeActivateNewGameFromInput` fallback — do not delete yet

```
mapped = TryMapEvent(type, key, _frontendWidgets)
         → ActionFromEvent → MessageFromWidgets (action 26 only)
if mapped is null && action==26 && FrontendMenuRoot==PRESS_START
  mapped = 0xE5
```

Native posts stored +352 / persist from `0055CB10` listeners.
There is no `cmp` of the screen string.

| Fixture | Widgets | Fallback |
| --- | --- | --- |
| `Bootstrap(install)` | attach-patched root `MessageId=0xE5` | dead (`TryMapEvent` already 0xE5) |
| `Bootstrap(null)` | empty | **the** type-4 → `0xE5` path |

`Frontend_press_start_type4_posts_0xE5_then_new_profile` is the
no-install lock. Deleting the fallback fails that test.
`Queue_drives_lifecycle_without_a_key` injects `0xE5` and does
not use it.

Drop the fork only after no-install fixtures carry an attach
write (or stop testing type 4 with empty widgets). Do not
replace it with Return / Enter.

---

## 5. Two C# `0xE5` sources on Press Start (install)

Factory DFS: root first, then children. After the attach patch,
`MessageFromWidgets` hits the type-10 first.

Without the patch, the next visible type 11/38 with nonzero id
is `UI_FRONTEND_BUTTON_INVISIBLE` persist `0xE5` (Visible
first-seen, `Clip=false`). That is a **second** leftover vs
native: type-10 register on `0055CB10` is **UNREAD**; INVISIBLE
register via type-33/`0055BA20` is **PROVEN**
(`action26-subscribers`, `list-type12-focus`). Do not delete
the attach analog on the theory that INVISIBLE is enough.

---

## Tests / leftover

- `FrontendInputTests.Type4_action_26_posts_stored_widget_message`
  locks `AttachWriteE5=0x00598EE6`, `Type10StoreMsgFn=0x0054E4F0`,
  offset **352**.
- `Type4_drives_lifecycle_0xE5_then_0x126_then_15` requires
  install root `MessageId==0xE5` (attach patch).
- `Frontend_press_start_type4_posts_0xE5_then_new_profile`
  requires the `MaybeActivate` fallback.

## Do not invent

- Persist `0xE5` on the type-10 PRESS_START def.
- `.text` `mov […], 0x126`.
- Enter / Return → `0xE5`.
- Patching NEW_PROFILE / MAIN_MENU roots.
- Deleting the attach analog because INVISIBLE also holds `0xE5`.
- Deleting the empty-widget fallback without moving that fixture.
