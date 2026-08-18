# First fade after Leave: `00B239A0` vs frontend vs script `FadeOut`

Investigation only. No production `src/` edits.

Do **not** start at leftover `CS_OAKVALE_INTRO_FATHER` /
`FadeOut 0.5,0` / `00CBFB7D`. That is later
`Q_NewOakValeIntro`, not Leave / Init Game / first
`004189C2`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER**.

Sources: `src/Fable.Game/EngineLifecycle.cs` (`DisplayEngineFade*`,
`ApplyFirstPumpAviAndFade`, `RequestNewGame`);
`ScriptRuntime.cs` / `GlobalDispatcher.cs` / `ScriptCommandMap.cs`;
`docs/runtime/FORWARD_TREE.md` §8; `docs/PARITY.md` first-pump row;
`proofs/script-global-cmds/README.md`; `proofs/audio-frontend/README.md`;
`proofs/landscape-first-draw/README.md`;
`EngineLifecycleTests.First_pump_004189C2_is_0040D2A0_then_00B239A0_not_a_region`;
listing `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`004189C2`, `0042EC7C`, `0042F2A2`, `0042EBB6`);
`listing-00b00000.txt` (`00B239A0`, `00B23A10`, `00B26360` tail,
`00B27D90`, `00B277A0`);
vtbl `012A0F3C+220` = `012A1018` in
`newgame-trace/vtbl-engine-636-setter-table-012a1000.md`.

---

## Verdict

**First fade *call* after Leave is display-engine
`00B239A0(12, 20.0f)`, not a script command and not
the Leave black Present.**

It is a five-instruction setter on the `012A0F3C` engine
(`[game+40]+44`): `+24=1`, `+28=12`, `+32=20.0f` from
`0x122F160`. It does **not** pack overlay black, does
**not** write display `+188`/`+216`, and does **not**
`E8` `00434C00` / `006496BC` / `008907E0`.

Frontend already has the *same three fields* from Init
Engine `00B26360` (`+24=1`, type **0**, `20.0f`). Retail
`0042EC7C` also *loads* `0x122F160` into PlayAVI slot
records. Those are **not** `vtbl+220`. First `00B239A0`
changes type `0 → 12` and keeps 20 s.

Script `FadeOut` / `FadeIn` / `StayFadedOut` are
**DISPROVEN** as first-seen after Leave.

| Question | Answer | Class |
|---|---|---|
| First `00B239A0` after Leave? | first `004189C2` tail, after dummy `004FC180` + `0040D2A0`/`0040BC80` | **PROVEN** |
| Args? | `push 12` / `fld [0x122F160]` → `+28=12`, `+32=20.0f`, `+24=1` | **PROVEN** |
| Called on Leave `0042F2A2` / `0042EBB6`? | no | **DISPROVEN** |
| Called on retail frontend pump? | no `call [eax+220]` on `0042EC7C` | **DISPROVEN** |
| Same as script `FadeOut` `vtbl+1488`? | no | **DISPROVEN** |
| Frontend already has engine fade fields? | Init Engine `00B26360` type **0**, 20 s, `+24=1` | **PROVEN** |
| What first `00B239A0` changes vs frontend? | type `0 → 12` only (duration already 20) | **PROVEN** |
| Layer pre-pass sees `+24` on frontend `00B27D90`? | yes (`0042E0BB`) | **PROVEN** |
| What type `12` *draws*? | passed as `edx` into layer `vtbl+12` ctx | **PARTIAL** |
| Script fade after Leave / first Present? | runner not on the tree | **DISPROVEN** |

---

## Timeline (no-save New Game)

```
0042EC7C retail
  fld [0x122F160]              // 20.0f into PlayAVI slot +20
  006286F0 ×3                  // not 00B239A0
  0042E204 Init Engine
    00B26340 / 00B260B0        // vtbl 012A0F3C at retail+88
    00B26360 tail              // +24=1  +28=0  +32=20.0f (0x41A00000)
  0042DF9E / 0042E0BB
    00B27D90                   // [engine+24] → 00B277A0  (type 0)
0042F2A2 Leave frontend
  [0x13B8394].vtbl+72(0x1F4)   // 500 ms audio; not display
  0042EBB6 +41 skip audio stop
  009BE420 + 009BEEB0          // instant black Present
  no vtbl+220
0042F491 Init Game → 004184BD
004189C2 first pump
  dummy 004FC180
  0040D2A0 / 0040BC80
  [game+40]+44 vtbl+220 00B239A0(12, 20.0f)   // type 0 → 12
  009F2660 / 009F26B0
  inner 004162B5 …
  WorldFrame<=1: skip 00435530
  later first dest: +216=0 skip 00434CD0
```

`00CD0987` / `00CC4B22` / `00CBFDD0` are **not** on this
list. **PROVEN.**

---

## 1. What `00B239A0` is

`012A0F3C+220` (`012A1018`). Listing:

```
00B239A0  mov eax, [esp+4]     ; type
00B239A4  mov edx, [esp+8]     ; seconds (bitwise)
00B239A8  mov [ecx+24], 1
00B239AC  mov [ecx+28], eax
00B239AF  mov [ecx+32], edx
00B239B2  ret 8
```

Site `00418A7C` (inside `004189C2`, once, before the
inner `0098E1B0` loop):

```
fld  [0x122F160]               ; 20.0f
mov  eax, [esi+40]             ; game+40
mov  ecx, [eax+44]
mov  eax, [ecx]
push ecx
fstp [esp]
push 12
call [eax+220]                 ; 00B239A0
```

Sibling slots on the same object:

| VA | vtbl | Role |
|---|---|---|
| `00B239A0` | +220 | force `+24`, write type + seconds |
| `00B23A10` | +224 | if `+24` already 1, **return**; else `+36=1` and type `2`/`0` from `+44`/`+40` |
| `00B23A70` | +228 | `al=1` only when `+24==0` and `+36==0` |

After Init Engine, `+24` is already 1, so `00B23A10`
would no-op. GamePump uses `00B239A0` to overwrite type.

No `.text` `E8` to `00B239A0` (vtbl only). Other
`call [eax+220]` hits are other objects.

---

## 2. Frontend fade (before Leave)

Three *different* frontend-adjacent fades. None is
`00B239A0`.

### A. PlayAVI slot 20.0f (`0042EC7C`)

`fld [0x122F160]` then `fst [ebp+20]` / `fstp [ebp+52]`
into the three `006286F0` records (lionhead / Microsoft /
`intro_comp`). Third slot also stores `0x41200000` (10.0f)
at `+84`. **Same float, not the display-engine setter.**
**PROVEN.**

### B. Init Engine field defaults (`00B26360` tail)

```
00B27719  mov [esi+24], 1
00B2771D  mov [esi+32], 0x41A00000   ; 20.0f
00B27724  mov [esi+28], ebx          ; type 0
00B27727  mov [esi+36], 0
00B2772A  mov [esi+40], 0x40200000   ; 2.5f  (00B23A10 type-0 duration)
00B27731  mov [esi+44], 0x3D888889
```

`0042E0BB` → `00B27D90` (`012A0F3C+32`):

```
mov al, [ebp+24]
jne 00B27E7C          ; → 00B277A0
```

Frontend *already* pre-passes because `+24=1`. Type in
that pre-pass is **0**. **PROVEN.**

`00B277A0` (`LandscapeFrustum.PrePassUpdate`): if `+24`,
`00B3B4A0` on `[0x1436E98]` (shader-cache release, not
an overlay quad); later `fld [ebx+32]` as duration
(floor `0x3C23D70A` ≈ 0.01 if `<= [0x1230A5C]`);
`dt/duration` clamped 0..1; `mov edx, [ebx+28]` copied
into the layer ctx (`[esp+340]`) before `vtbl+12`.
What a layer does with type `0` vs `12` is **PARTIAL**.

### C. Leave audio + black Present (not a 20 s fade)

`0042F2A2`: `[0x13B8394].vtbl+72(0x1F4)` = 500 ms audio.
`0042EBB6`: New Game `+41!=0` skips stop; `009BE420`
clear RGB 0 / A `0xFF` then `009BEEB0`. Instant dest
clear. **DISPROVEN** as `00B239A0`. **DISPROVEN** as
script overlay.

---

## 3. Script fade commands (not Leave)

Authority: ASCII `0x012C19A0` / `0x012C19A8` / `0x012C19B0`.
Apply is `CGameScriptInterface`, **not** engine `vtbl+220`.

| Verb | Token | Apply | Record | First after Leave? |
|---|---|---|---|---|
| `FadeOut` | `00CD0987` | `vtbl+1488` `008907E0` pack `(0,0,0,255)` → `vtbl+1492` → `00434C00` (`+188=1`, `+201=1`, `+192=sec`, `+216=1`) | display +188 overlay | **DISPROVEN** |
| `FadeIn` | `00CC4B22` | `vtbl+1496` `0088E4C0` clear `+216` → `00434C90` | falling overlay | **DISPROVEN** |
| special-case | `00CBFDD0` | same `vtbl+1488(0.5,0)` if `commands[0]=="FadeOut 0.5,0"` | father `[0]` is PlayMusic → skip | **DISPROVEN** leftover |
| `StayFadedOut` | `00CD087E` | runner-local flag | not `00B239A0` | **DISPROVEN** |
| `FadeThingIn/Out` | `00CC782E` / `00CC762F` | mesh `vtbl+2040` | not screen | **DISPROVEN** |

Overlay tick `00434870` / draw `006496BC` (type `0x22`)
gate on **`+188`**. `00B239A0` never writes `+188`.

Host `GlobalDispatcher` `FadeOut`/`FadeIn` →
`ScriptRuntime.ApplyFadeOut` is that overlay. It is
**LEFTOVER** vs Leave (`proofs/script-global-cmds`).
`FirstSeenFadeSpecialCaseRuns=false`.

---

## 4. Other game fades, also not `00B239A0`

| Site | When | Class |
|---|---|---|
| `00434CD0` dest fade | first `00435530` when `display+232>0`; `+216=0` and `01375CDC=0` → `009D8250` empty | **PROVEN** skip first dest. Different object. |
| `0041649C` | `00418289` only if frontend query **and** GUI block | **DISPROVEN** as first no-save (query false). Body is world/display dispatch `00434A30`, not `00B239A0`. Host name “fade” is leftover. |
| `0062F800` / `0062F8B0` | frontend audio clocks after `0042DED5(0)` | **PROVEN** audio. Not display-engine. |

---

## 5. C# vs native

| Host | Native | Class |
|---|---|---|
| `ApplyFirstPumpAviAndFade` notes `00B239A0(12, 20)` | first `004189C2` tail | **PROVEN** timing |
| `DisplayEngineFadeKind=12` / `Time=20` | `+28` / `+32` | **PROVEN** store |
| no ctor type-0 note at Init Engine | `00B26360` writes type 0 / 20 / `+24` before Leave | **PARTIAL** (host silent) |
| no layer ctx type 12 | `00B277A0` `[ebx+28]` → `vtbl+12` | **UNREAD** in host |
| `RequestNewGame` notes `009BE420` | Leave black Present | **PROVEN** |
| `GlobalDispatcher` Fade* | leftover father | **LEFTOVER** vs Leave |
| overlay `TickFade` / `FadeColor` | `00434870` / `006496BC` | **LEFTOVER** vs first pump |

Do **not** implement first-seen fade as a 20 s black
quad, as `FadeOut 0.5,0`, or as Leave `009BE420`.
Those are three other systems.

---

## Classifications (short)

1. **First fade *call* after Leave — `00B239A0(12, 20.0f)`
   on `[game+40]+44`. PROVEN.** Not a region. Not script.
2. **`00B239A0` body — setter `+24/+28/+32` only. PROVEN.**
3. **Frontend already owns the same fields (type 0, 20 s)
   from Init Engine, and `00B27D90` already pre-passes.
   PROVEN.** First post-Leave call only switches type to 12.
4. **Retail `0x122F160` on `0042EC7C` — PlayAVI slot
   duration. DISPROVEN as `vtbl+220`.**
5. **Leave visual — instant `009BE420` + audio 500 ms.
   DISPROVEN as display-engine fade.**
6. **Script `FadeOut`/`FadeIn`/`StayFadedOut` after Leave —
   DISPROVEN.** Overlay `+188` is a different record.
7. **What type 12 changes in the 3D submit — PARTIAL.**
   Pre-pass + layer ctx copy are proven; first DIP that
   keys off `12` is UNREAD.
