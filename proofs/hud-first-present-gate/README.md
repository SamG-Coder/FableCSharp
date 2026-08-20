# First dest Present after Leave: `PLAYER_GUI` draw gate / empty `009DA9F0`

Investigation only. No production `src/` or `tests/` edits.

Question: does native **draw** `PLAYER_GUI` on the first
dest Present after Leave? Is that an empty `009DA9F0`
skip?

Do **not** invent HUD widgets. Do **not** name
`0065431D` / `0064xxxx` children as orbs / MiniMap /
health. Do **not** start Oakvale / `00DBDE40` /
`Q_NewOakValeIntro`. Do **not** treat frontend
`0042DF9E` as this Present.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: dump
`listing-00400000.txt` `00435000` / `00435070` /
`00435530` / `00434E10` / `00434C90` / `0043A380` /
`0043A080` / `0043B050`;
`listing-00480000.txt` `0049F180` / `0048DB46` /
`004A5E4D`;
`listing-00680000.txt` `0069ECD0`;
`listing-009c0000.txt` `009DA9F0` / `009DAA42` /
`009DB6E6`;
`e8.tsv` dests `0043A380` / `0043B050` / `0048DB46` /
`009DA9F0`;
`.rdata` `0x129BA3C` (`FrontendDx9Submit.UvEpsilon`
`0.0001f`);
`src/Fable.Game/EngineLifecycle.cs`
`InitCharactersAndQuests` / `ApplyDisplayCamera` /
`TickPlayerGui` / `DisplayFlushShouldDip`;
siblings `proofs/hud-after-leave`,
`proofs/hud-first-present-skip`,
`proofs/init-gui-0043A380`,
`proofs/player-gui-pc-338`,
`proofs/0049F180-first-children`;
`docs/runtime/FORWARD_TREE.md` §11;
`EngineLifecycleTests.After_004AEA70_eq_1_00417001_is_00435F70_Present`.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Native draw `PLAYER_GUI` on first dest Present after Leave? | **No.** Overlay miss, interface miss, `PLAYER_GUI+24` helper skipped, sprite builder not on this body, dest queue empty | **DISPROVEN** as draw |
| Empty `009DA9F0` skip? | **Yes.** `[+16020]==[+16024]` → `009DAA42 je 009DB6E6` no DIP | **PROVEN** |
| Is the skip a host hide of missing HUD dest? | **No.** Native producers are idle. Same empty skip as `hud-first-present-skip` | **DISPROVEN** as hide |
| Does Init GUI construct dest widgets? | **No.** `0043A380` reset on live `[0x13B8790]` | **DISPROVEN** |
| Does Init Characters supply the overlay/interface Thing? | **No.** `0049F180` `00487DC0` miss (no player Thing) | **PROVEN** miss; same gate as Present |
| `0043B050` on first `00435530`? | **No.** Only `.text` `E8` is `0069ECD0` | **DISPROVEN** first-seen |
| Host `PlayerGuiReady` means dest draw? | **No.** Flag after a reset Note | **LEFTOVER** |

---

## Verdict

**Native does not draw `PLAYER_GUI` on the first dest
Present after Leave. Empty `009DA9F0` skip.**

`PLAYER_GUI_PC` exists (Create Players `0043B570`,
then Init GUI `0043A380` reset). Existence is not
dest. First `00435F70` jmp `00435530` always
**calls** overlay `00435000` and interface
`00435070`, then drains `009DA9F0(1)`. First-seen
no-save takes every skip:

1. Overlay: `00487DD0` / `00A01B50` miss → skip
   `00639E40`.
2. Interface: `00487DC0` miss → skip `0057B43F`.
   Same miss as Init Characters.
3. `PLAYER_GUI+24` helper `0048DB46`: display
   `+240` ctor `-1.0` fails the
   `-eps > +240` test → `jp 00435840`. Not a dest
   producer. First-seen **not called**.
4. Sprite builder `0043B050` is **not** an `E8` of
   `00435530`.
5. Drain: begin==end → `009DB6E6`. No DIP.

Do not invent first-seen HUD dest, orbs, MiniMap,
or type-`0x22` quads on this frame.

| Claim | Status |
| --- | --- |
| `PLAYER_GUI_PC` singleton resident after Leave | **PROVEN** construct; **not** dest |
| Init GUI `0043A380` builds HUD dest | **DISPROVEN** |
| Overlay / interface apply on first Present | **PROVEN** skip (Thing miss) |
| `0048DB46` (`PLAYER_GUI+24`) on first Present | **PROVEN** skip (`+240=-1.0`) |
| `0043B050` type-`0x22` on first Present | **DISPROVEN** |
| `009DA9F0(1)` empty → `009DB6E6` | **PROVEN** / **MATCH** |
| Host empty skip hides native HUD dest | **DISPROVEN** |
| Host `PlayerGuiReady` / no live object | **LEFTOVER** |

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend              // 0042DF9E 2D tree gone
0042F491  Init Game
  Create Players 004166A8
    00487FB0 00BFEA1A(0x338) 0043B570  // PLAYER_GUI_PC
    004195AF [0x13B8790]
  00416953 Loading world FinalAlbion.wld
    00416BCA 0049F180(ecx=world, 0)
      "Init Characters"
        00449970 / 00487DC0 miss       // no player Thing
        00449D90 → 00489D40 ret 0
      "Init GUI" 0043A380              // reset, not ctor
      "Init Quests" 004B4260
later WorldFrame>1  004AEA70=1
  type-1 004A5E4D 0043A080 +164=0      // tick, not dest
    00A01B50 miss skip 0064A32E
  00435F70 jmp 00435530                // THIS PRESENT
    00435000  00487DD0 miss skip 00639E40
    00435070  00487DC0 miss skip 0057B43F
    +240=-1.0  jp 00435840             // skip 0048DB46 / 00434C90
    009D9C80 / 009DA9F0(1) empty
    009DAA42 je 009DB6E6               // no DIP
    no 0043B050 / no 0069ECD0
```

---

## 1. Gate: no player Thing (Init Characters = Present)

`0049F180` after Leave (`listing-00480000.txt`):

```
0049F18D  push "Init Characters"
0049F1B3  mov  ecx, [esi+12]
0049F1B6  call 00449970
0049F1BD  call 00487DC0
0049F1C4  je   0049F1CF              ; miss → bind, no Thing
…
0049F1EA  push "Init GUI"
0049F20E  mov  ecx, [0x13B8790]
0049F214  call 0043A380
```

No-save first `00487DC0` is 0. Init GUI still runs
because the singleton was stored during Create
Players. Reset is not dest (`init-gui-0043A380`).

First Present overlay / interface use the **same**
Thing lookup:

`00435000` (`listing-00400000.txt`):

```
00435001  mov  ecx, [ecx+12]
00435004  call 00449960
0043500B  call 00487DD0              ; +44 jmp 00A01B50
00435010  test eax, eax
00435012  je   0043505E              ; miss → ret
          [eax+145] bit0 / [eax+48] 0x4000
00435058  call 00639E40              ; text, not taken
```

`00435070`:

```
00435079  mov  ecx, [eax+28]         ; [0x13B86A0]+28
0043507C  call 00449970
00435083  call 00487DC0
00435088  test eax, eax
0043508A  je   004350C9              ; miss → ret
004350C4  call 0057B43F              ; type 0x22 apply, not taken
```

`00435530` always:

```
004357D0  call 00435000
004357D8  call 00435070
```

First-seen: both miss. **PROVEN skip.** Overlay
string and interface type-`0x22` rec are **not**
built. That is the HUD apply gate, not a
`PLAYER_GUI` dest walk.

---

## 2. Gate: `PLAYER_GUI+24` helper is not dest, and is skipped

Immediately after overlay / interface
(`listing-00400000.txt`):

```
004357DD  fld  [0x129BA3C]           ; 0.0001f
004357E3  fchs                       ; -0.0001
004357E5  fcomp [esi+240]
004357ED  test ah, 0x05
004357F0  jp   00435840              ; skip helper
004357F2  mov  ecx, [0x13B8790]
004357F8  add  ecx, 24
004357FB  call 0048DB46              ; cmp [ecx+229]
00435800  test al, al
00435802  jne  00435840
          …
0043583B  call 00434C90              ; fade reset, not dest
```

Display ctor `00434E10`:

```
00434F29  mov [esi+240], 0xBF800000  ; -1.0f
```

`-0.0001 > -1.0` → `test ah,5` zero → `jp 00435840`.
First-seen **does not** call `0048DB46` and **does
not** call `00434C90`.

`0048DB46` (`listing-00480000.txt`) is a flag
query on the `+24` helper (`cmp [ecx+229]`).
`00434C90` writes display `+236` / `+240=-1` /
`[0x1375CDC]`. Neither enqueues `+16020`. Neither
is `0043B050`. **DISPROVEN** as `PLAYER_GUI` dest
draw.

`e8.tsv`: `0048DB46` from `004357FB` plus three
later sites. First Present takes none of the
later ones.

---

## 3. Gate: sprite builder is not on this Present

`e8.tsv`: **one** `.text` `E8` of `0043B050`:

```
0069ECD0  call 0043B050              ; ecx=[0x13B8790]
```

`0069ECD0` is **not** a callee of `00435530`
(`functions.tsv` `0x004354C0` list has
`00435000` / `00435070` / `0048DB46` /
`009DA9F0`, no `0043B050` / `0069ECD0`).
Zero `.text` `E8` of `0069ECD0` (vtbl). First
take **UNREAD**. Not this frame.

Do not invent the `0041BEB0` type-`0x22` records
`0043B050` would build. They are later.

Tick `0043A080` (`004A5E4D`, type-1 after
`004A5E10` inc WorldFrame) is **not** dest
Present. First-seen arg `[world+164]=0`. Inside:

```
0043A12E  mov  ecx, [edi+8]
0043A131  call 00449970
0043A136  lea  ecx, [eax+52]
0043A139  call 00A01B50
0043A13E  test eax, eax
0043A140  je   0043A156              ; miss skip 0064A32E
```

Same Thing miss. Tick of resident pointers is
not a `009DA9F0` record. **DISPROVEN** as dest.

---

## 4. Drain gate: empty `009DA9F0`

`00435530` after the closed `009DD8F0` debug
gates (`hud-first-present-skip`):

```
00435D40  call 009D9C80
00435D4B  push 1
00435D4D  call 009DA9F0
```

`009DA9F0` (`listing-009c0000.txt`):

```
009DA9F7  mov  edx, [ebp+16020]
009DA9FD  mov  ecx, [ebp+16024]
009DAA03  sub  ecx, edx
          count * 0x88888889 / 60
009DAA42  je   009DB6E6              ; empty: no DIP
```

No overlay rec, no interface rec, no
`009DB700` (only `E8` sites `009DC00E` /
`009DD93D`, both gated off first-seen).
begin==end. **PROVEN empty skip.**

Nonempty would be `00A058C0` then vtbl+332.
That path is **not** first-seen. No
`cmp …,0x22`. `PLAYER_GUI` is not a type
switch on this drain.

---

## Host

`InitCharactersAndQuests` Notes the Init
Characters miss and Init GUI reset, then
`PlayerGuiReady=true`. No `[0x13B8790]`
object, no `0043A380` recopy, no dest.

`ApplyDisplayCamera` (`00435F70` jmp
`00435530`):

```
Note(… "00435000 skip 00639E40");
Note(… "00435070 skip 0057B43F");
DisplayFlushShouldDip(0, 0);         // always false
Note(… "009DA9F0(1) [+16020] empty dest");
Note(… "009DA9F0 skip DIP 009DB6E6");
```

**MATCH** overlay skip, interface skip, empty
drain. **PARTIAL** notes: host does not walk
`0048DB46` / `+240=-1.0` `jp 00435840`, nor
the closed `009DD8F0` sites. Dest pixels are
still none.

`TickPlayerGui` is `Note(0043A080 +164=0)`
only. **LEFTOVER** vs the listing tick;
**MATCH** that it is not dest.

Test
`After_004AEA70_eq_1_00417001_is_00435F70_Present`
locks overlay skip, interface skip,
`empty dest`, `SubmittedLayerBits` empty,
`LayerFlushCount==0`.

---

## Gap

| Native first dest Present | Host | Class |
| --- | --- | --- |
| Overlay `00639E40` miss | Note skip | **MATCH** |
| Interface `0057B43F` miss | Note skip | **MATCH** |
| `0048DB46` `PLAYER_GUI+24` | omitted | dest **MATCH** (idle); notes **PARTIAL** |
| `0043B050` / `0069ECD0` | none | **MATCH** absent |
| `009DA9F0(1)` empty `009DB6E6` | `DisplayFlushShouldDip(0,0)` | **MATCH** first-seen |
| Live `[+16020]` | never stored | first-seen **MATCH**; later **LEFTOVER** |
| `PLAYER_GUI` object | `PlayerGuiReady` flag | **LEFTOVER** |
| HUD dest pixels | none | **MATCH** |

Host always-empty flush is a stand-in
*implementation*. On **this** no-save first
Present the native `PLAYER_GUI` draw path is
also idle, so the empty skip is not covering
up missing HUD widgets.

---

## Open

| Item | Class |
| --- | --- |
| First later take of `0069ECD0` → `0043B050` | **UNREAD** (not this Present) |
| First `00639E40` / `0057B43F` after a live Thing | **UNREAD** (`hud-after-leave`) |
| First `0048DB46` take after `+240` is not `-1.0` | **UNREAD** |
| Names of `0065431D` / `0064xxxx` children | **UNREAD** — do not invent |

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0042F2A2` | Leave; frontend 2D gone | **PROVEN** |
| `0049F180` | Init Characters / Init GUI | **PROVEN** Thing miss; GUI reset |
| `0043A380` | Init GUI reset | **DISPROVEN** as dest factory |
| `0043B570` | `PLAYER_GUI_PC` ctor | **PROVEN** construct; **not** first-seen draw |
| `00435530` | game Present body | **PROVEN** order; HUD dest empty |
| `00435000` | overlay lookup | **PROVEN** miss |
| `00435070` | interface lookup | **PROVEN** miss |
| `0048DB46` | `PLAYER_GUI+24` flag | **PROVEN** skip first-seen |
| `0043B050` | later type-`0x22` build | **DISPROVEN** first-seen |
| `0043A080` | WorldFrame tick | **DISPROVEN** as dest |
| `009DA9F0` | drain; empty `009DB6E6` | **PROVEN** / **MATCH** |
| `00DBDE40` | Oakvale HUD feeder | **DISPROVEN** here |

**Answer: no. Native does not draw `PLAYER_GUI` on
the first dest Present after Leave. Empty
`009DA9F0` skip.**
