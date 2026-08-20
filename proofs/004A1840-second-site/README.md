# Second `004A1840` site `004A2A01`

Investigation only. No production `src/` edits.

Question: is `e8.tsv` dest `004A1840` site `004A2A01` on no-save
New Game? If not, when? Same QST flag **1** then **0**?

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Authority: `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv`
`004A1840`; `listing-00480000.txt` around `004A2A01` / `004A1840`
/ `004A21F0` / `004A3200` / `004A2D70` / `004A2F10`;
`listing-00400000.txt` `00416953`; `listing-00600000.txt`
`0062CF30`; `proofs/qst-first-load`; `proofs/qst-clear-004A08D0`;
`docs/runtime/FORWARD_TREE.md` §10.

`functions.tsv` `0x004A1840` size **2258** is a **bad merge**.
The frame ends `004A21DF` `ret 4` / `int3` pad. `004A21F0` is a
new function. Do not treat `004A2A01` as “inside `004A1840`”.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| On no-save New Game? | **No.** Only site on that walk is `00416ABA` | **PROVEN** |
| When is `004A2A01`? | `004A21F0` FableSav **HEADER** apply, after `WorldName` / `CurrentRegionName`, **before** save `ENTITIES` / `QUESTS`. Caller is `004A3200` “Loading save” (or in-game `004A2F10` when `world+248` ≠ 0) | **PROVEN** |
| Same QST flag 1 then 0? | **Yes.** Flags are **inside** `004A1840`, not at the `E8` site. `004A1931` push **1** (`FinalAlbion.qst` stem) then `004A1991` push **0** (`GlobalQuests.qst`). Same body as New Game | **PROVEN** |

`qst-first-load` row “Second `004A1840` site `004A2A01` on this
no-save walk | UNREAD” is now **DISPROVEN** as a New Game take.

---

## 1. `e8.tsv` callers of `004A1840`

Exactly **two** sites:

```
0x00416ABA	0x004A1840
0x004A2A01	0x004A1840
```

No other `E8`. **PROVEN.**

`00416ABA` is game vtbl+32 `00416953` “Loading world”:

```
00416971  lea edi, [esi+90588]
00416973  call 0099B220
00416978  test eax, eax
0041697A  jle 004169C8            ; empty → no-save
0041697E  push "Loading save"
…
004169AF  call 004A3200           ; not no-save
…
004169C8  push "Loading world"
…
00416AB3  mov ecx, [esi+36]       ; world
00416AB6  lea eax, [ebp-4]        ; +90576 WLD CString
00416AB9  push eax
00416ABA  call 004A1840
```

No-save `[game+90588]` empty **skips** `004A3200`. **PROVEN**
(`FORWARD_TREE` §10, `LoadWorld_00416953_no_save_is_004A1840_then_0049F180`).

`004A1840` itself **does not** `E8` `004A21F0` / `004A1840`.
It ends:

```
004A21D8  pop ebp
004A21D9  add esp, 0x184
004A21DF  ret 4
004A21E2  int3 … 004A21EF
004A21F0  sub esp, 0x170          ; next function
```

New Game cannot fall through or recurse into `004A2A01`.
**PROVEN.**

---

## 2. Containing function of `004A2A01`

`004A2A01` is in `004A21F0` (`ret 8`, two stdcall args: path
CString*, flag). It is a **FableSav** reader, not “Load Quests”.

```
004A21F0  sub esp, 0x170
004A21FD  mov esi, [esp+380]      ; path
004A2204  mov ebp, ecx            ; CWorld
004A220F  call 00409730           ; open/read
004A2214  test al, al
004A2216  jne 004A222F
          … ret 8 fail
004A2272  mov [ebp+256], 0x01
004A2333  mov edi, "FableSav"
004A2341  rep cmpsd
004A2343  jne 004A25F9            ; magic miss → no 004A1840
…
004A2689  push "HEADER"
004A26AB  call 009BAE70
004A26BB  test bl, bl
004A26BD  je 004A2A4B             ; no HEADER → skip 004A1840
004A26D1  push "WorldName"
004A26DD  call 00411010
004A2703  mov al, [ebp+258]
004A2709  test al, al
004A270B  jne 004A2A42            ; +258 ≠ 0 → skip 004A1840
          … TeleportingEnabled / SavingEnabled / CurrentRegionName …
004A29C5  push 0x1238508          ; CString intern (not in strings.tsv)
004A29D3  call 0099E480
004A29E0  call 0041A060           ; intern 0x122F3B4 "Data\Levels\"
004A29EE  call 0099BE70
004A29F9  call 0099BF30
004A29FE  push eax                ; reconstructed WLD CString*
004A29FF  mov ecx, ebp
004A2A01  call 004A1840
004A2A42  lea ecx, [esp+24]
004A2A46  call 0099EAE0           ; dtor WorldName
004A2A4B  …
004A2A67  push "ENTITIES"
004A2AC4  push "PLAYER"
004A2B0C  push "QUESTS"
004A2B50  call 004B6380           ; after 004A1840 returns
004A2B57  push "REGIONS"
004A2B9F  push "FACTIONS"
```

`004A2A01` is **world reload from the save HEADER**, then the
save’s `QUESTS` blob overlays. It is not a second New Game QST
pass. **PROVEN.**

Path arg: `0041A060` prefix `Data\Levels\` plus intern
`0x1238508` via `0099E480` / `0099BE70` / `0099BF30`. Intern
payload is **UNREAD** (object VA, not an ASCII row). HEADER
`WorldName` at `[esp+24]` is live across the call (dtor
`004A2A42`). Whether that name is the stem vs a fixed intern
is **PARTIAL**. Stem still feeds `0049D770` `.qst` the same
way as New Game.

---

## 3. Who calls `004A21F0` (`e8.tsv`)

Four sites, all save-related:

| Site | Parent | `world+258` | Reaches `004A2A01`? |
|---|---|---|---|
| `004A32EA` | `004A3200` “Loading save” | not set here (ctor 0) | **yes** if HEADER + magic hit |
| `004A340D` | `004A3200` fallback | same | **yes** if HEADER + magic hit |
| `004A3017` | `004A2F10` (`world+248` state) | not set here | **yes** if HEADER + magic hit |
| `004A2DC2` | `004A2D70` | **`mov [esi+258], 1` then call** | **no** (`jne 004A2A42`) |

`004A3200` callers (`e8.tsv`): **only**

```
0x004169AF	0x004A3200    ; 00416953, [+90588] nonempty
0x0062CF30	0x004A3200    ; UI 00621A20 (load/save lists), not Init Game
```

`004A2F10` sole `E8`: `004A5BD2` when `[world+248] != 0`
(save-load machine during world update). No-save ctor leaves
`+248` 0 → skip. **PROVEN** skip on New Game.

`004A3200` success path then `004A2D70`, which re-enters
`004A21F0` with `+258=1` so the **second** FableSav apply does
**not** `004A1840` again. **PROVEN.**

No-save New Game: empty `+90588` → no `004A3200` → no
`004A21F0` → no `004A2A01`. **PROVEN.**

---

## 4. Same QST flag 1 then 0?

Yes. Not a property of `00416ABA` vs `004A2A01`. Both enter
the same `004A1840` head:

```
004A18DD  push "Load Quests"
004A1903  call 0049DDD0          ; STB stem from path
004A190D  call 0049D770          ; Data\Levels\ + stem + .qst
004A1915  call 00999230
004A191C  je 004A1965
004A1931  push 1
004A193A  mov ecx, esi           ; world
004A193C  call 004A0D90          ; clear then parse
004A1965  push 0x1238F38         ; Data\Levels\GlobalQuests.qst
004A1975  call 00999230
004A197C  je 004A19C8
004A1991  push 0
004A199A  mov ecx, esi
004A199C  call 004A0D90          ; append, no 004A08D0
```

`004A0D90` `test al, al` / `je`: flag **1** → `004A08D0`
clear `world+184` / `+172` / `+196`; flag **0** skips.
**PROVEN** (`qst-clear-004A08D0`).

On save load that means: **wipe and refill** quest-name
vectors from the WLD-stem `.qst` + GlobalQuests, **then**
HEADER `QUESTS` `004B6380` applies saved state. New Game never
reaches that overlay. **PROVEN** order; save `004B6380` body
**UNREAD** here.

Missing `.qst` still skips `004A0D90` (`je`). TLC ships both
files, so a `FinalAlbion.wld` stem still does 1 then 0.
**PROVEN** for that stem; other `WorldName` stems **PARTIAL**.

---

## 5. No-save vs save (short)

```
no-save New Game
  00416953  [+90588] empty
    00416ABA  004A1840(world, FinalAlbion.wld)     ONLY site
      004A0D90(..., 1)  FinalAlbion.qst
      004A0D90(..., 0)  GlobalQuests.qst
      WAD / 00507C30 / Set Static Map
    0049F180(0)
  004A2A01 never

Loading save
  00416953  [+90588] nonempty
    004A3200(name, 1)
      004A21F0(path, 0)           +258==0
        HEADER … 004A2A01 004A1840
          same 004A0D90 1 then 0
        ENTITIES / PLAYER / QUESTS 004B6380 / …
      004A2D70
        +258=1
        004A21F0                  skips 004A1840
```

Host `LoadWorld` Notes `004A3200 Loading save skipped` and
only the `00416ABA` site. Match for no-save. Save walk is
host **UNREAD**.

---

## Classifications (short)

1. **`004A2A01` on no-save New Game — DISPROVEN.**
   Sole New Game `E8` is `00416ABA`. `004A1840` returns
   `004A21DF`; no recurse.
2. **When — PROVEN:** `004A21F0` FableSav HEADER, from
   `004A3200` (vtbl+32 “Loading save” or UI `0062CF30`) or
   `004A2F10` (`world+248` machine). Gated on FableSav magic,
   HEADER hit, and `[world+258]==0`.
3. **QST flags 1 then 0 — PROVEN same body.** Site does not
   pass the flag. `004A1931` / `004A1991` always.
4. **`004A2D70` second `004A21F0` — PROVEN skip** of
   `004A1840` (`+258=1`).
5. **Reconstructed WLD stem at `004A2A01` — PARTIAL.**
   Prefix `Data\Levels\` **PROVEN**; intern `0x1238508`
   payload **UNREAD**.
