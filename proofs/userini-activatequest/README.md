# `user.ini` `ActivateQuest("Gameflow")` after `004184BD`

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `Q_NewOakValeIntro`.
Do **not** treat `userst.ini` `SetStartingHolySite("NOVStartHSP")`
as a quest start.
Do **not** invent a second `ActivateQuest` name from this file.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: TLC install `C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\user.ini`
(and sibling `userst.ini`); install-wide grep `ActivateQuest`;
`listing-00400000.txt` (`004184BD` / `00418922`–`00418981` /
`004197B0` / `00419CE0` / `00419D90`);
`listing-00480000.txt` (`004A712B` / `004B4A10` / `004B4260`);
`listing-00880000.txt` (`00892E80` / `00892EA0` / `00892EC0`);
`listing-009c0000.txt` (`009EC890` / `009EC710` / `009EB430` /
`009EB260` / `009ECB70`);
`listing-006c0000.txt` (`006E7740`);
`proofs/ini-activate-quest/README.md`;
`docs/runtime/FORWARD_TREE.md` §2 after vtbl+32;
`docs/PARITY.md` Init Game suffix;
`EngineLifecycle` (`IniApplyFn` / `IniActivateQuestThunk` /
`ScriptManagerActivateQuestFn`);
`EngineLifecycleTests.UserIni_009EC890_RunScript_joystick_is_00999230_miss`.

---

## Verdict

**After `004184BD` vtbl+32, the ini walker is
`009EC890`. The `ActivateQuest` token is
`00419CE0`. That thunk is `[world+56]` vtbl+1104
`00892E80`, not `00CB5AD0`.**

First-seen TLC `user.ini` has **one** `ActivateQuest`
line, argument `"Gameflow"`. Install-wide grep of
`ActivateQuest` under the TLC root is that same
line. **PROVEN**. Other names do **not** appear in
the shipped / first-seen file. **DISPROVEN** as
content. The thunk is name-generic, so a later
edit of the same file **could** start another name
the same way. That case is **UNREAD**. Host must
not invent one.

Direct `00CB5AD0` from `009EC890` / `009EB430` is
**DISPROVEN**. `00419D90` only registers. `userst.ini`
is **not** this walk.

| Question | Answer | Class |
|---|---|---|
| Token path after `004184BD`? | `00418969` `0x122F01C` → `009EC890` → `009EC710` → `009EB430` | **PROVEN** |
| `ActivateQuest` handler? | `00419CE0` (`[cmd+20]` from `00419D90`) | **PROVEN** |
| Script-manager slot? | `[world+56]` `006E7740` vtbl `01260F0C+1104` **`00892E80`** | **PROVEN** |
| `00892E80` body? | `push 1; push 1; push name; 004B4A10` then `004B4260` → `00CB5AD0` | **PROVEN** |
| Other `ActivateQuest` names in shipped TLC `user.ini`? | **no** — one line, `"Gameflow"` | **DISPROVEN** |
| First-seen ini quest after Leave? | **`Gameflow` only** | **PROVEN** |
| Would a second line work? | yes, same thunk copies `[this]+8` | **UNREAD** as a later-edit |

---

## Timeline (no-save, after Leave)

```
0042F2A2  Leave frontend
0042F491  Init Game 00418DCA → 004184BD
  … named stages …
  Init World 004A6E30
    004A712B 00419D90                         // REGISTER only
      name "ActivateQuest" 0x0122F380
      [cmd]=0x122E65C  [cmd+20]=00419CE0
      009EC5E0 into [0x13CAA40]
  [game].vtbl+32 00416953 Load world
    004B4260([world+172]) six QST TRUE names  // not ini
  [0x13B8648]==0
    0049BA70 / 00416392 / 004AE9D0
    00418922 0x122F030 default_user.ini
      00999230 miss → skip 009EC890           // TLC absent
    00418969 push 0x122F01C user.ini
      00414C90 → ecx console
      call 009EC890                           // THIS WALKER
        00999230 exists
        009EC710 tokenize
          per token 009EB430 [ini+64] vtbl+4
            SetMaxAnisotropy     009EB260 unknown
            RunScript("joystick.ini")
              009ECB70 → 009EC890 → 00999230 miss
            SetMaxAnimatedMeshDist / SetMaxStaticMeshDist
            MaxThingDrawDist
            ActivateQuest("Gameflow")         // ONLY this name
              00419CE0
                [game].vtbl+36 004197B0  xor al,al  // never skip
                copy CString [cmd+8]               // "Gameflow"
                ecx = [[game+36]+56]               // 006E7740
                call [vtbl+1104]                   // 00892E80
                  [0x13B89FC] 004B4A10(name,1,1)
                  004B4A10 → 004B4260 → 004B00C0 → 00CB5AD0
            SetPlatform2DGain
            SetFullscreen(false)              // display, not a quest
    009A4EC0 seed 004167DA / +90592
```

---

## 1. `009EC890` — file walker, not the quest call

`004184BD` after vtbl+32 and `[0x13B8648]==0`:

```
00418922  mov edi, 0x122F030          ; "default_user.ini"
00418933  call 00999230
00418946  je 00418969                 ; TLC miss
0041895C  call 009EC890               ; skipped
00418969  push 0x122F01C              ; "user.ini"
0041897A  call 00414C90
00418981  call 009EC890               ; unconditional site
```

`user.ini` is **not** existence-gated at `00418969`.
The gate is inside `009EC890`:

```
009EC890  sub esp, 92
009EC8B5  mov ecx, esi                ; path CString
009EC8BC  call 00999230
009EC8C7  je 009EC9FB                 ; miss → no tokens
009EC9AB  call 009EC710
```

`009EC710` walks tokens and `009EC7C8 call 009EB430`.
`009EB430` looks up `[ini+64]`. Hit → `call [edx+4]`.
Miss → `"unknown input - "` `009EB260`.
`cmp [esp+16], 2` else `"command line was not a string"`.

`009EC890` `.text` sites: `00414C50` (`default_userst.ini`),
`00414C7F` (`userst.ini`, Parse Command Line), `0041895C`,
`00418981` (this walk), `009ECC53` (`009ECB70` `RunScript`),
plus `0061ABC7` / `0061AD2B` (not this suffix). **PROVEN**.

`RunScript` is **not** a quest:

```
009ECB70  mov eax, [edx]
009ECB87  push ".ini"
009ECC53  call 009EC890
```

TLC has **no** `joystick.ini`. Nested walker misses.
It cannot inject another `ActivateQuest`. **PROVEN**.

---

## 2. `00419CE0` — handler stored at register time

`00419D90` (one `E8`: `004A712B` `"Init Global Console"`):

```
00419DAE  push "ActivateQuest"
00419DF0  mov [esi], 0x122E65C
00419DF6  mov [esi+20], 0x419CE0
00419E39  call 009EC5E0
```

Live apply (`listing-00400000.txt`):

```
00419CE0  push ecx
00419CE2  mov esi, ecx                ; command object
00419CE4  mov ecx, [0x13B86A0]        ; game
00419CEC  call [eax+36]               ; 004197B0
00419CEF  test al, al
00419CF1  jne 00419D2D                ; skip (never: xor al,al)
00419CFE  add ecx, 8
00419D06  call 0099EFB0               ; copy argument CString
00419D11  mov eax, [edx+36]           ; world
00419D14  mov ecx, [eax+56]           ; script manager
00419D1E  call [edx+1104]             ; 00892E80
00419D2F  ret
```

No immediate `"Gameflow"` in the thunk. The name is
whatever `[cmd+8]` holds from the ini argument.
**PROVEN** generic; first-seen argument is `"Gameflow"`.

`004197B0` is `xor al, al; ret`. Broader vtbl+36 role
is still **UNREAD** (FORWARD_TREE slot 9). Here it is a
never-skip gate. **PROVEN**.

Zero `.text` `E8` to `00419CE0`. The ini path is
`009EB430` vtbl+4 / `[cmd+20]`, not a direct call.
**PROVEN**.

---

## 3. `00892E80` — vtbl+1104, then `004B4A10(1,1)`

Init Scripts ctor writes the vtbl used here:

```
006E7769  mov [esi], 0x1260F0C
```

`00892E80` and siblings (`listing-00880000.txt`):

```
00892E80  mov eax, [esp+4]
00892E84  mov ecx, [0x13B89FC]
00892E8A  push 1
00892E8C  push 1
00892E8E  push eax
00892E8F  call 004B4A10
00892E94  ret 4

00892EA0  … push 1; push 1; call 004B4260     ; not this slot
00892EC0  … push 0; push 1; call 004B4A10     ; (1,0), not this slot
```

`004B4A10` (`ret 12`) builds a one-name vector and
`004B4A5A call 004B4260`. `004B4260` per name:

```
004B42D7  call 004B00C0
004B42E4  mov ecx, [edi+120]
004B42E8  call 00CB5AD0
004B4386  call 004B3CE0
```

`Gameflow` is QST `AddQuest(..., FALSE)` so it is in
`[manager+44]` (gate allows) and **not** in
`[world+172]` (already walked). Ini activate is the
**7th** no-save start, after the six TRUE names.
**PROVEN** (`proofs/qst-first-quest`, `proofs/ini-activate-quest`).

Direct `00CB5AD0` from `009EB430` is **DISPROVEN**
(one `.text` `E8`: `004B42E8` inside `004B4260`).

---

## 4. Shipped / first-seen `user.ini`

TLC install root, first-seen file:

```
SetMaxAnisotropy(4);
RunScript("joystick.ini");
SetMaxAnimatedMeshDist(64);
SetMaxStaticMeshDist(128);

MaxThingDrawDist 128;

ActivateQuest("Gameflow");

SetPlatform2DGain(0.6);

SetFullscreen(false);
```

Install-wide grep of `ActivateQuest` (all files under
the TLC root): **one** hit, that line. **PROVEN**.

| File | Present? | `ActivateQuest` |
|---|---|---|
| `user.ini` | yes | **1**, `"Gameflow"` |
| `default_user.ini` | no | none |
| `joystick.ini` | no | none |
| `userst.ini` | yes, Parse Command Line only | **0** |
| `default_userst.ini` | no | none |

`userst.ini` has `SetLevel` / `SetStartingHolySite("NOVStartHSP")`
and **zero** `ActivateQuest`. It is applied at `00414C66`
**before** frontend / Leave / `00419D90`. A hypothetical
`userst.ini` `ActivateQuest` would miss `[ini+64]`.
**DISPROVEN** as this path (`proofs/ini-activate-quest`).

`SetFullscreen(false)` / anisotropy are display tokens on
the same walker. They are **not** quest names. Graphics
lines may be a local edit of a Steam-rooted file; that
does not add a second `ActivateQuest`. **PROVEN** for
quest names; other-line provenance **PARTIAL**.

---

## 5. Host vs native

| Host | Native | Class |
|---|---|---|
| `FinishInitGameAfterWorld` `009EC890 user.ini` | `00418981` | **PROVEN** |
| `DispatchUserIniCommand("ActivateQuest")` → `00419CE0` / `00892E80` / `004B4A10` | this dump | **PROVEN** |
| `ActivateNamedQuest("Gameflow")` | `004B4260` / `00CB5AD0` | **PROVEN** |
| Comment on `ApplyUserIniCommands`: “vtbl+1104 is UNREAD — do not start a quest here” | thunk **does** start Gameflow | **LEFTOVER** |
| Invent `ActivateQuest(Q_NewOakValeIntro)` from this file | not in shipped `user.ini` | **DISPROVEN** |
| Apply `userst.ini` after `004184BD` | `00414C66` is command line | **DISPROVEN** |

---

## Classifications (short)

1. **Token path after `004184BD` — PROVEN.**
   `00418969` `user.ini` `009EC890` → `009EC710` →
   `009EB430`. `ActivateQuest` is `00419CE0` →
   `00892E80` → `004B4A10(1,1)` → `004B4260` →
   `00CB5AD0` `"Gameflow"`.
2. **Other shipped `ActivateQuest` names — DISPROVEN.**
   TLC tree has one line. First-seen is **only**
   `Gameflow`.
3. **Generic thunk — PROVEN; extra names UNREAD.**
   `00419CE0` copies `[cmd+8]`. A later-edit second
   line would use the same three VAs. Do not invent it.
4. **`009EC890` as `00CB5AD0` — DISPROVEN.**
   Walker only dispatches registered commands.
5. **`00419D90` as the live call — DISPROVEN.**
   Register only; live is `00419CE0`.
