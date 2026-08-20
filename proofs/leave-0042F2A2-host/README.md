# Host `RequestNewGame` / `EnterGame` vs `0042F2A2` first children

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`Q_NewOakValeIntro` / `S_QNOVI` / kid `CREATURE_HERO_CHILD`.
After msg 15 the next site is Leave `0042F2A2` inside
`0042EC7C`. First WLD name is `FinalAlbion.wld`. Oakvale is
later leftover `NewRegion 4` / slot-2 `00DABAC0`.

Question: after msg 15, what are `0042F2A2` **first
children**? Host `RequestNewGame` / `EnterGame` leftover
vs that **exact order**?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: ExeIndex `listing-00400000.txt` `0042F297`–
`0042F508` (`0042F2A2` / `00404490` / `004131A0` /
`0042EBB6` / `00418DCA`); `listing-009c0000.txt`
`009D8240` / `009D8250`; `EngineLifecycle.RequestNewGame`
/ `EnterGame` / `Pump`;
`EngineLifecycleTests.Frontend_00595582_new_game_message_leaves_without_RequestNewGame`,
`New_game_is_leave_frontend_then_FinalAlbion_wld`.

Siblings: `proofs/audit-lifecycle-newgame` (msg 15 →
Leave, not Oakvale); `proofs/audio-after-leave`
(`vtbl+72(500)`); `proofs/type11-msg15` (who posts 15);
`docs/runtime/FORWARD_TREE.md` §4; `docs/PARITY.md`
Leave / Init Game.

`0042F2A2` is **not** a function. It is the
`push "Leave frontend"` site on the `[esi+41]!=0` arm
of retail pump `0042EC7C`. First children = first-level
`E8` / `call [eax+…]` from that site through
`0042F508 ret 4` on the New Game take.

---

## Verdict

**`RequestNewGame` is a collapsed first-child list.
`EnterGame` is the next sibling (`0042F491`), not an
Oakvale jump. Same-pump order MATCHES native.**

After msg 15, `0042EC7C` already ran this frame’s
frontend Present. It then takes `0042F297` and walks
the list below. Host `Pump` (`RetailNewGameFlag`)
calls `RequestNewGame` then `EnterGame` before
`PresentToHost`. Leave notes still precede Init Game
notes. **PROVEN.**

Leftover vs **first-child exact order** is not
Oakvale. It is:

| Host | Native first child | Class |
|---|---|---|
| `RequestNewGame` Notes `00404490` / `004131A0` / `0042F44D` / `0042EBB6` | same sites, same order | **MATCH** |
| Notes `009BE420` / `009BEEB0` as if `0042F2A2` children | grandchildren of `0042EBB6` | **LEFTOVER** hoist |
| No `vtbl+72(0x1F4)` | `0042F2D8` if `[0x13B8394]` | **LEFTOVER** absence (**PARTIAL** host) |
| Skip `0099EBF0` / `0099EAE0` / `0099B7D0` / `0099B6A0` / `0099B510` | CString plumbing | **LEFTOVER** skip (no store) |
| Skip `009D8240` / `009D8250` | both are `ret` | **MATCH** skip |
| `EnterGame` inlines `004184BD` (Init World / `LoadWorld` / cameras / players) | one child: `[ebx].vtbl+4` | **LEFTOVER** inline |
| `00418EC6` Note **after** `LoadWorld` | ctor write **before** vtbl+4 | **LEFTOVER** order |
| `Pump` `PresentToHost` after `EnterGame` | no Present after `00418DCA` | **LEFTOVER** extra |
| `RequestNewGame` alone → `Stage=LeaveFrontend` | `0042EC7C` never returns between Leave and Init Game | **LEFTOVER** split (tests) |

**Answer:** implement no new `0042F2A2` body. Do not
promote `0042EBB6` clear/Present or `004184BD` stages
to first children. Do not fill the `ret` pair. Do not
start Oakvale from this site.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First children after msg 15? | Ordered list in §2. Bank-swap arm **skipped** (`[0x13B8616]==0`). | **PROVEN** |
| Host leftover **side effect** at `RequestNewGame`? | `WorldFileName`, `FrontendBatch=null`, stage. No `00404490` zeros, no fade, no CString. | **PROVEN** collapse |
| `EnterGame` leftover vs this list? | Yes: inlines `004184BD`. Native first child is **one** vtbl+4. | **PROVEN** leftover |
| Same-pump `RequestNewGame` then `EnterGame` vs native? | **MATCH** `0042EBB6` then `0042F491`. | **PROVEN** |
| Oakvale / Hero / `00DBDE40` as a first child? | **No.** Zero `E8` on this arm. | **DISPROVEN** |

---

## 1. How msg 15 reaches `0042F2A2`

Same `0042EC7C` invocation:

```
0042EC7C  retail pump
  0042E3EE / 0042DC94 / 0059A238
    msg 15 → 0059A2DA [ui+28].vtbl+16
           → 00594F28 [esi+41]=1          // ActivateNewGame
  0042DF9E … 009BEEB0                     // this frame's frontend Present
  0042F297  cmp [esi+41], bl
  0042F29A  je  0042F4EC                  // not this path
  0042F2A0  push -1
  0042F2A2  push "Leave frontend"         // THIS SITE
  …
  0042F508  ret 4
```

Host pairing:

```
ActivateNewGame()                         // flag only; Stage stays Frontend
Pump() Frontend:
  PumpFrontendFrame()                     // 0042E3EE … 009BEEB0
  if RetailNewGameFlag:
    RequestNewGame()                      // 0042F2A2 … 0042EBB6
    EnterGame()                           // 0042F491 … 004184BD
  PresentToHost()                         // leftover extra; see §4
```

`ActivateNewGame` is **not** a `0042F2A2` child.
**PROVEN.**

---

## 2. Exact first children (no-save, `[0x13B8616]==0`)

Stores that are **not** children, still on the arm:

| Site | Store | Host |
|---|---|---|
| `0042F2AA` | `[0x1375448]=0` | **MATCH** Note `01375448=0` |
| `0042F2DB` | `cmp [0x13B8616]` → `je 0042F42F` | **MATCH** skip Note |
| `0042F4D8` | `[[ebp+124]] = game` | host game object |
| `0042F4DA` | `[0x13B7D58] = esi` (old retail) | **MATCH** Note `013B7D58` inside `EnterGame` |

Taken calls, in listing order:

| # | Call | Target | Role | vs host |
|---:|---|---|---|---|
| 1 | `0042F2B0` | `0099EBF0` | CString `"Leave frontend"` | skip **LEFTOVER** |
| 2 | `0042F2BA` | `009D8240` | **`ret`** | skip **MATCH** |
| 3 | `0042F2C2` | `0099EAE0` | CString dtor | skip **LEFTOVER** |
| 4 | `0042F2D8` | `[eax+72](0x1F4)` | fade `[0x13B8394]` 500 ms | **LEFTOVER** absence |
| — | `0042F2E1` | skip `009A78D0` / `009A8840` | BSS first-seen 0 | **MATCH** |
| 5 | `0042F42F` | `00404490` | zero `[0x13CA81C]` / `[0x13CA818]` | Note only |
| 6 | `0042F437` | `004131A0` | stack path-record ctor | Note only |
| 7 | `0042F446` | `0099B7D0` | copy `[esi+132]` → `[ebp-24]` | skip **LEFTOVER** |
| 8 | `0042F455` | `0099EBF0` | CString `"FinalAlbion.wld"` | **MATCH** as `0042F44D` |
| 9 | `0042F460` | `0099B6A0` | CString move | skip **LEFTOVER** |
| 10 | `0042F469` | `0099B7D0` | copy into record | skip **LEFTOVER** |
| 11 | `0042F471` | `0099B510` | CString clear | skip **LEFTOVER** |
| 12 | `0042F479` | `0099EAE0` | CString dtor | skip **LEFTOVER** |
| 13 | `0042F480` | `009D8250` | **`ret`** | skip **MATCH** |
| 14 | `0042F48A` | `0042EBB6` | teardown (`ecx=esi`) | **MATCH** Note |
| 15 | `0042F499` | `0099EBF0` | CString `"Init Game"` | `EnterGame` **MATCH** |
| 16 | `0042F4A4` | `009D8240` | **`ret`** | skip **MATCH** |
| 17 | `0042F4AC` | `0099EAE0` | CString dtor | skip **LEFTOVER** |
| 18 | `0042F4B6` | `00BFEA1A(0x161E8)` | alloc | skip **LEFTOVER** |
| 19 | `0042F4C7` | `00418DCA` | game ctor (`vtbl 0122F180`) | **MATCH** Note |
| 20 | `0042F4D2` | `[eax+4]` = `004184BD` | start | **MATCH** then **LEFTOVER** inline |
| 21 | `0042F4E3` | `004131D0` | path-record dtor | skip **LEFTOVER** |

Bank-swap children (`0099B6B0` / `0041A100` / `009A78D0` /
`009A8840` / …) are **not** first-seen. Implementing them
on New Game is **LEFTOVER**.

`009D8240` / `009D8250` (`listing-009c0000.txt`):

```
009D8240  ret
009D8250  ret
```

**PROVEN** no-ops. Host silence is **MATCH**, not a hole.

Fade gate (`0042F2C7`):

```
mov ecx, [0x13B8394]
cmp ecx, ebx
je  0042F2DB
push 0x1F4
call [eax+72]
```

Constants exist (`LeaveFrontendAudioVtbl=72`,
`LeaveFrontendAudioMs=0x1F4`). `RequestNewGame` never
calls them. **LEFTOVER** absence. Singleton live after
pre-Leave `0042DED5` is **PROVEN**
(`proofs/audio-after-leave`). Host has no
`[0x13B8394]` analog. **PARTIAL**.

---

## 3. `RequestNewGame` vs the list

```
Note(0042F2A2, "Leave frontend");
Note(01375448=0);
Note(013B8616 skip 009A78D0/009A8840);
Note(00404490);
Note(004131A0);
WorldFileName = FinalAlbion.wld;
Note(0042F44D, FinalAlbion.wld);
Note(0042EBB6 +41 skip audio stop);
Note(009BE420 clear);
Note(009BEEB0 Present);
Stage = LeaveFrontend;
FrontendBatch = null;
FrontendPresentRgba = null;
```

### MATCH

Note order `0042F2A2` → `01375448` → skip bank →
`00404490` → `004131A0` → `0042F44D` → `0042EBB6`
is the listing order of the **named** children.
`WorldFileName` is the host stand-in for the stack
record that `00418DCA` later copies at `00415E17`
into `game+90576`. Filename **MATCH**. Copy site is
a **00418DCA** grandchild, not a `0042F2A2` first
child.

### Leftover hoist (`0042EBB6` children)

`0042EBB6` first children (New Game `+41!=0`):

```
[+60] release
004336B0                 // 00433D3A at 0x13B8760
0042DD28                 // drop retail+180 / +88 / +64 / +72 …
[0x13B838C+56]=1
skip vtbl+64 / vtbl+72(0) / 00991750 / 009918F0
0042DBD8                 // +177 helper off
00412450 / 00411B00      // +108 / +120
009BE420 + 009BEEB0      // iff [0x13B8390]
```

Host lifts only the last pair onto `RequestNewGame`.
That is **LEFTOVER** vs `0042F2A2` first children
(one child: `0042EBB6`). Relative to `0042EBB6` the
clear/Present **order** is **MATCH**. The skipped
audio **stop** Note is **MATCH**. The skipped
`004336B0` / `0042DD28` / `0042DBD8` bodies are
**UNREAD** as named teardown; do not invent them
as first children of `0042F2A2`.

### Leftover no-ops

`00404490` (listing):

```
eax = [0x13CA81C]
if eax:
  dec [eax]; if 0: call [eax+4]; 00BFE9BC
[0x13CA81C]=0
[0x13CA818]=0
```

Host Note only. First-seen BSS **UNREAD**. If both
dwords are already 0, native is two stores of 0.
Treating this as a file-open or WLD parse is
**DISPROVEN**.

`004131A0` constructs four empty strings on
`[ebp-36]`. Host Note only. **LEFTOVER** skip of a
stack object whose only consumer is `00418DCA`.

`FrontendBatch = null` is not a first child. It is
the host analog of teardown dropping the 2D batch.
**PARTIAL** pairing.

---

## 4. `EnterGame` vs the list

`0042F491` is **after** `0042EBB6` in the same
function. It is a first child of this arm, not of
a later pump.

Native:

```
0042F48A  call 0042EBB6
0042F491  push "Init Game"
          0099EBF0 / 009D8240(ret) / 0099EAE0
0042F4B6  00BFEA1A(0x161E8)
0042F4C7  call 00418DCA          // includes 00415E17 + 00418EC6
0042F4D2  call [eax+4]           // 004184BD ONLY
0042F4E3  call 004131D0
          al=1; ret
```

`00418EC6 mov [esi+90593],1` is the **tail of the
ctor**, before vtbl+4. `004184BD` then does
`[0x13B86A0]=game`, `009E9EF0` / `009E9F90` /
`00416832`, named stages, vtbl+32 `00416953`.

Host `EnterGame`:

```
Note 0042F491 / 00418DCA / 004184BD / 013B86A0
foreach InitGameStages          // 004184BD body
InitWorldCameras / CreatePlayers / LoadWorld
GameRenderEnabled = true
FinishInitGameAfterWorld
Note 00418EC6                   // AFTER LoadWorld
```

| Claim | Class |
|---|---|
| `EnterGame` starts at `0042F491` | **MATCH** |
| `00418DCA` then `004184BD` | **MATCH** as first children |
| Init World / `00416953` / cameras / players as `0042F2A2` children | **LEFTOVER** (they are `004184BD` / `004A6E30`) |
| `00418EC6` after `LoadWorld` | **LEFTOVER** order vs ctor tail |
| `EnterGame` from still-`Frontend` calls `RequestNewGame` first | **MATCH** vs native (Leave then Init Game) |
| `Pump` `PresentToHost` after that pair | **LEFTOVER** extra Present |
| Tests that call `RequestNewGame` and stop | **LEFTOVER** yield; next `Pump` `LeaveFrontend` → `EnterGame` with **no** `PresentToHost` |

Same-pump extra Present is host scheduling, not a
second native `009BEEB0` in `0042EC7C`. Do not add a
black Present after Init Game to “match” it.

---

## 5. What is **not** leftover at this site

These run **before** `0042F2A2` (same pump) or
**inside** `004184BD` (after first child #20).
Collapsing them into `RequestNewGame` is
**DISPROVEN**.

| Action | Owner |
|---|---|
| Msg 15 / `[esi+41]=1` | `0059A238` / `00594F28` (`ActivateNewGame`) |
| Frontend `0042DF9E` / `009BEEB0` | earlier `0042EC7C` |
| `Init Sound` / `Init Atmos` / `Init Scripts` | `004184BD` |
| `00416953` / `004A1840` / `FinalAlbion.wld` parse | vtbl+32 after ctor |
| `004B4260` / `004B2890` / user.ini `Gameflow` | after Leave, inside Init Game |
| `00501450` Lookout / Hero 4299 | later, not this arm |
| `00DBDE40` / `Q_NewOakValeIntro` activate | **DISPROVEN** here |

`NewGameScript` / `ScriptRuntime.StartNewGame` /
`FirstSceneWorld` remain **LEFTOVER** / **DIVERGE**
vs this walk (`audit-lifecycle-newgame`). They are
not first children and not a host `RequestNewGame`
side effect.

---

## 6. Path (msg 15 → first children only)

```
0059A238 msg 15 → [esi+41]=1
0042EC7C
  frontend Present
0042F2A2 Leave frontend
  0099EBF0 / 009D8240(ret) / 0099EAE0
  [0x1375448]=0
  vtbl+72(500)                    // host skip
  skip 009A78D0 / 009A8840
  00404490
  004131A0
  0099B7D0 / 0099EBF0 FinalAlbion.wld / 0099B6A0 / …
  009D8250(ret)
  0042EBB6                        // +41 skip stop; 009BE420+009BEEB0
0042F491 Init Game
  00BFEA1A(0x161E8)
  00418DCA                        // 00415E17 path; 00418EC6 +90593=1
  [ebx].vtbl+4 004184BD           // NOT expanded here
  004131D0
0042F508 ret 4
```

Host maps the bold named row to `RequestNewGame` and
the Init Game row to `EnterGame`. That split is
**MATCH**. Expanding either row is leftover.

---

## Classifications (short)

1. **`0042F2A2` first children are the §2 list.
   PROVEN.** Listing `0042F2A2`–`0042F508`.
2. **`RequestNewGame` named order MATCH. PROVEN.**
   Fade / CString / `00404490` stores skipped.
3. **`009BE420` / `009BEEB0` as first children.
   LEFTOVER hoist.** They belong to `0042EBB6`.
4. **`009D8240` / `009D8250` skip. MATCH.** Both
   `ret`.
5. **`EnterGame` is `0042F491`, then one
   `004184BD`. PROVEN.** Inlined stages are
   leftover vs first-child exact order.
6. **Same-pump Leave then Init Game. MATCH.**
   Extra `PresentToHost` after that is leftover.
7. **Oakvale / `00DBDE40` as a first child.
   DISPROVEN.**
