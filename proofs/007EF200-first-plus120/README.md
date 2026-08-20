# First `CTCExpression` whose nested `+120` is `Q_NewOakValeIntro`

Investigation only. No production `src/` edits.

Question: first Thing / def that **constructs** a
`CTCExpression` whose nested `[+120]` intern is
`Q_NewOakValeIntro`. `007EF200` `CTCExpression`
vtbl+28 reads that slot then `004B4A10`. `[esi+116]`
set → camera `0041649C` **instead** of activate.
Lookout TNG Oakvale names? `StartOakValeWest` TNG?
Is `Expression_Follow` always a quest?

Do **not** wire `007EF200` as Oakvale.
Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: ExeIndex
`listing-007c0000.txt` `007EF200`–`007EF4DB` /
`007EF070` / `007EF3A1` / `007EF600`;
`listing-00780000.txt` `007B5680` / `007B5740` /
`007B5AA4` (other class);
`listing-004c0000.txt` `004DB050` / `004DB06C` /
`004DB085` / `004DC78F` / `004DC7E8` / `004D4B72` /
`004F4988` / `004F4A50`;
`listing-00440000.txt` `0041649C` / `0045228F` /
`00456964` / `004569A7` / `0045D70B`;
`listing-00400000.txt` `0041649C`;
`listing-00600000.txt` `00629930` / `0062995D`;
`listing-00680000.txt` `00686960` / `006869D0` /
`006AC430`;
`listing-00840000.txt` `00843F50` / `00843FC0` /
`00846710`;
`listing-00cc0000.txt` `00CD9BE7` / `00CD6E27`;
`vtbl.tsv` `0x0124026C` / `0x012401F4` / `0x01233D1C`;
`rtti.txt` `CTCExpression` `0x0137A424` /
`CExpressionDef` `0x01376DCC`;
`xrefs-by-string.tsv` `Q_NewOakValeIntro` /
`Expression_Follow` / `CTCExpression`;
`compiled-defs/game/entries.tsv` `EXPRESSION` (39) /
`CActivateQuestDef` (6);
siblings `proofs/007EEF60-activate`,
`proofs/q-novi-activator-callers`,
`proofs/ctcexpression-quest-names`,
`proofs/cactivatequestdef-payloads`,
`proofs/008421C0-activate`,
`proofs/lookout-tng-walk`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| What is `007EF200`? | **`CTCExpression` vtbl `0x0124026C` slot 28.** `this` = component `0x8F`. Arg1 = Thing. | **PROVEN** |
| What does vtbl+28 read? | Nested `esi = [[this+12]]`. `[esi+116]≠0` → **`0041649C`** (`[0x13B86A0]`). Else `[esi+120]` vs empty intern `0x122D70E` → copy `00415DD0` → **`004B4A10(QM, &copy, 0, [esi+124])`**. | **PROVEN** |
| Hardcoded `Q_NewOakValeIntro` `0x012C5D14` in `007EF200`? | **No.** Name is runtime intern at nested `+120`. | **DISPROVEN** |
| First constructor whose `+120` **is** that intern? | **None recovered.** Type-register / attach / TNG / compiled `EXPRESSION` / `Expression_Follow` do not store it. | **DISPROVEN** as Lookout / TNG / Follow; live later Thing **UNREAD** |
| Lookout TNG Oakvale names? | **0.** No `Q_NewOakValeIntro`, no `StartCTCExpression`. | **PROVEN** |
| `StartOakValeWest` TNG `+120`? | **No.** Only `XXXSectionStart Q_NewOakValeIntro` → `ThingInstance.Section`. | **PROVEN** |
| Is `Expression_Follow` always a quest? | **No.** Same `00843F50` ctor also intern `Expression_Wait` / `Fish` / `Dig`. Global QST `Expression_*` are `AddQuest` **FALSE**. Bind table `00CD9BE7` is Gameflow-style, not `004B4A10`. | **DISPROVEN** |
| Wire `007EF200` as Oakvale activate? | **No.** | **DISPROVEN** |

---

## Verdict

`007EF200` is a **generic** Thing-component tick. It
activates **whatever** non-empty intern sits at nested
`[+120]`, and only when `[+116]` is **0**. That is not
a first no-save Oakvale constructor.

No recovered constructor of `CTCExpression` fills that
slot with `Q_NewOakValeIntro`:

- Lookout (first no-save TNG) has **zero** Oakvale strings
  and **zero** `StartCTCExpression`.
- `StartOakValeWest` carries the name only as a **section
  bucket**, after that map exists — chicken-egg vs first
  Lookout Present.
- Compiled `EXPRESSION` rows are social names
  (`EXPRESSION_FOLLOW` …). None is `Q_*`.
- `Expression_Follow` is **DISPROVEN** as always-quest.

Host already skips inventing `ActivateQuest("Q_NewOakValeIntro")`.
**MATCH.** Do not add a `007EF200` Note that pretends this
tick ran Oakvale on no-save.

---

## Evidence

### 1. `007EF200` vtbl+28 (listing-007c0000)

```
007EF1FF  int3
007EF200  sub esp, 0x100
007EF20E  mov edi, [esp+272]          ; Thing
007EF215  mov ebx, ecx                ; CTCExpression
…
007EF2DC  mov [esp+20], 0x8F          ; component id
007EF303  mov esi, [ebp+12]           ; nested def*
007EF30E  mov eax, [esi+116]
          test eax, eax
          je  007EF36B               ; +116 set → camera, not activate
007EF327  mov edx, [esi+116]
          mov ecx, [0x13B86A0]
007EF361  call 0041649C
          jmp 007EF488
007EF36B  lea ebx, [esi+120]
          push 0x122D70E             ; empty intern
          call 005FA740
          je  007EF422               ; empty → skip 004B4A10
          call 00415DD0              ; copy +120
          mov al, [esi+124]
          push eax / push 0 / push &copy
          mov ecx, [0x13B89FC]
007EF3A1  call 004B4A10
          … [esi+126] → 008430B0 / 006644F0
007EF4DB  ret 4
```

`vtbl.tsv`: `0x0124026C` slot 28 = `0x007EF200`.
Ctor `004DB085` writes that vtbl. Slot 21 of sibling
def vtbl `0x012401F4` returns type **`0x8F`**; intern
`004D4B75` `"CTCExpression"`. RTTI `0x0137A424`.
`e8.tsv` dest `0x007EF200`: **0** — dispatch is vtbl,
not a named `E8`. **PROVEN** (sibling `007EEF60-activate`).

`0041649C` (`listing-00400000`): `this=[0x13B86A0]`,
`ret 4`. Camera / view request on the Game object
(`0049D8C0` / `00415FF2` / `004AE9A0` `[+80568]`).
**Not** `004B4A10`. **PROVEN.**

### 2. Who constructs `CTCExpression`

| Site | What it does | `+120` = Oakvale? |
|---|---|---|
| `004DC7E8` factory, alloc **20**, `004DB085` | empty component | **No** (no intern) |
| `004EE23F` / `004F4A50` `004D2EF0(0x4DC7E8)` | type register, `"CTCExpression"` | **No** (no Thing) |
| `004C9D60("CTCExpression")` at `00846762` | attach on a live Thing | name is later `[this+12]` |
| `00846710` vtbl `0x012743DC` slot 12 | copies **`[action+168]`** → `[CTC+12]` (refcount) | `+168` is a def ptr, not `0x012C5D14` |
| persist `007EF070` (`CExpressionDef` slot 1) | `004109A0("ExpressionDef")` → `006869D0` into `+12` | lookup **name**, not Oakvale literal |
| CTC persist slot 1 `00686960` | **`ret 8` stub** | does not write `+120` |

`00846710` also intern `"CTCEmoteIcon"`,
`"EXPRESSION_SNEER"`, `"EXPRESSION_USE"`. Social /
emote action, not `S_QNOVI`. **PROVEN** attach;
**DISPROVEN** as Oakvale constructor.

`004DC78F` sibling factory alloc **16** `004DB050`
(`CExpressionDef` vtbl `0x012401F4`) cannot host
`+116`/`+120`. Those dwords live on the **looked-up**
object stored at `[CTCExpression+12]`.

### 3. Nested `+116` / `+120` layout = compiled `EXPRESSION`

Bank type `EXPRESSION`, 39 rows, raw **187**, runtime
factory `0045D70B` `push 0x90` → ctor `00456964`
vtbl `0x01233D1C`. Persist slot 18 `004569A7`:

| Off | Helper | 007EF200 use |
|---|---|---|
| `+116` | `00456AD9` (empty intern then list) | `test [esi+116]` → camera |
| `+120` | `0045228F` intern (`0044FC00` 4-byte) | CString → `004B4A10` |
| `+124` | `0043314A` **byte** | `004B4A10` arg3 |
| `+126` | `0043314A` **byte** | follow-on `008430B0` |

**PROVEN** offset match. Ctor zeros `+112`, writes
`+120 = -1` until persist. Empty intern skip in the
tick is `0x122D70E`, same as `"Activate Initial Quests"`.

`entries.tsv` instance names:

`EXPRESSION_FOLLOW`, `EXPRESSION_WAIT`,
`EXPRESSION_PICKPOCKET`, … `EXPRESSION_A`.
**No** `Q_NewOakValeIntro`. ASCII column empty.
`names.tsv` has **no** `CExpressionDef` type and
**no** `Q_NewOakValeIntro`. **PROVEN.**

Inflated intern **dword** inside a 187-byte body
equal to `0x012C5D14`: **UNREAD** (dump has length,
not hex). Instance **name** is still not that string.

### 4. Lookout TNG — 0 Oakvale names

First no-save TNG open is `LookoutPoint.tng`
(`004FDBC0` / leftover #50). Sibling
`ctcexpression-quest-names` / `lookout-tng-walk`:

```
XXXSectionStart Gameflow;     // M_Maze, M_LadyGameflow
XXXSectionStart NULL;
XXXSectionStart Q_FireHeart;
XXXSectionStart Q_GuildTraining;
XXXSectionStart Q_WaspBoss;
…
```

Grep `Q_NewOakValeIntro`: **0**.
Grep `StartCTCExpression` / `CTCExpression` /
`ExpressionDef` / `QuestName`: **0**.

CTC blocks: physics, editor, camera, village, …
**Not** `CTCExpression`. First two `NewThing`s are
`MARKER_BASIC` `M_Maze` / `M_LadyGameflow`.
**PROVEN** this file cannot construct a
`CTCExpression` whose `+120` is Oakvale.

### 5. `StartOakValeWest` TNG — section only

WLD map 203, `LoadedOnPlayerProximity TRUE`,
ContainsMap of region `StartOakVale` (index 4).
**Not** first no-save region (`LookoutPoint`).

```
XXXSectionStart Q_NewOakValeIntro;            // line 20100
XXXSectionStart Q_NewOakValeIntro_PreAttack;
XXXSectionStart Q__OakValeIntro_PostAttack;
```

Host `ThingFile` stores `XXXSectionStart` as
`ThingInstance.Section`. First thing in that
section: `MARKER_BASIC` `MK_OVI_ID_HERO` —
physics + editor only. **No** quest-name field.
`CAM_OVIF_SHOT2` / `NOVStartHSP` live in **NULL**,
before the intro section.

Native prox parse may **open** this file during
`00507C30` (CurrentRegion still unset). That is
not first Present, not `007EF200` on `[thing+145]`,
and does not write `CExpressionDef`/`EXPRESSION+120`.
**PROVEN** section token; **DISPROVEN** as `+120`.

### 6. `Expression_Follow` is **DISPROVEN** as always-quest

`00843F50` `E8` sites (`cactivatequestdef-payloads`):

| Site | Intern | Class |
|---|---|---|
| `00629979` | `"Expression_Follow"` `0x01259170` | AI / creature action, arg4=`0` |
| `00629A09` | `"Expression_Wait"` | same |
| `007F0232` | `"Expression_Fish"` | same |
| `007F0410` | `"Expression_Dig"` | same |
| `007B5AA4` / `007EF66C` | **`[CActivateQuestDef+40]`** | later use-item |

`00629930` (`listing-00600000`): hero Thing via
`0049D850` / `00487DC0`, skip if `[thing+145]&1`,
then `00843F50(..., "Expression_Follow", 0)` →
`006644F0`. That queues **`CCreatureAction_ActivateQuest`**
with a **generic name string**, then `00843FC0` →
`004B4A10([this+168])`. The slot accepts expression
**and** quest names. **DISPROVEN** as always-quest.

`00CD9BE7` / `00CD9C46` bind `"Expression_Follow"`
the same way `00CD6E27` binds `Q_NewOakValeIntro`
(`00CB5C90` / `00CB5AC0`) — **bind**, not activate.

`GlobalQuests.qst` rows 3–9: `Expression_*` `AddQuest`
**FALSE** → catalog `world+184` / `QM+44` only.
`004B4260` does **not** take them (`qst-first-quest`).
**PROVEN.**

### 7. listing-00780000 is a **different** class

`007B5680` (`push "CActivateQuestDef"`) → `007B5AA4`
`00843F50` is **`CTCCarriedActionUseActivateQuest`**,
not `CTCExpression`. Persist `007B5740` intern at
**`def+40`**, bool `+44`. Six 16-byte game.bin rows;
payload intern **UNREAD**. None of the six `00843F50`
immediates is `0x012C5D14`. **Do not collapse** this
with `007EF200`.

`007EF600` (after `007EF200` in the 007c listing)
**calls** `007B5680` — use-item path on a live Thing,
still not no-save Oakvale.

---

## Original (no-save New Game)

```
004EE23F  register CTCExpression factory 004DC7E8     // no instance
004A0D90  AddQuest FALSE Q_NewOakValeIntro            // +184 / QM+44
          AddQuest FALSE Expression_* (GlobalQuests)
0049F24E  004B4260([world+172])                       // TRUE names only
00416BCF  +90584 empty skip 004B4A10
004FDBC0  LookoutPoint.tng                            // 0 Oakvale, 0 CTCExpression
00501450  first region Lookout
006B3FF0  first Present
007EF200  needs live 0x8F + nested +120 non-empty     // not here
00843FC0  needs queued CActivateQuestDef action       // not here
00CE7670  wait Q_NewOakValeIntro == 0
```

`StartOakValeWest` TNG / `XXXSectionStart Q_NewOakValeIntro`
is **after** that map is current — not the constructor
that would first `004B4A10` the name on Lookout.

---

## Host

`EngineLifecycle.InitCharactersAndQuests` Notes
`"004B4A10 not Q_NewOakValeIntro"` and the `+90584`
empty skip. `ActivateNamedQuest` walks `world+172`
only. No `CTCExpression` type. No
`ActivateQuest("Q_NewOakValeIntro")`.
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`.
**MATCH.**

Do **not**:

- wire `007EF200` as Oakvale activate
- treat Lookout leftover #50 TNG as that constructor
- collapse leftover #4 (Lookout Present vs Oakvale view)
- treat `Expression_Follow` as always-quest
- invent a first `CTCExpression+120` from
  `StartOakValeWest` section tokens

---

## Gap

| Item | Class |
|---|---|
| Live `[EXPRESSION+120]` / `[CTC+12]+120` on first spawned `0x8F` | **UNREAD** |
| Inflated intern u32 in 39×187 `EXPRESSION` bodies | **UNREAD** hex; instance names **PROVEN** not `Q_*` |
| Inflated intern in six `CActivateQuestDef` 16-byte rows | **UNREAD** (sibling) |
| Persist Lionhead **field name** of `EXPRESSION+120` | **UNREAD** (not TNG `QuestName`) |
| First Thing after a region that actually ticks `007EF200` with non-empty `+120` | **UNREAD**; **DISPROVEN** as no-save Lookout |

Until a live `+120` equals intern `0x012C5D14`, the
no-save Oakvale activator stays **UNKNOWN** and must
not be invented from this VA.

---

## Timeline (this hunt)

```
CTCExpression ctor 004DB085 / factory 20 bytes
  persist ExpressionDef name → [this+12]
  007EF200 vtbl+28
    [nested+116] ≠ 0  → 0041649C camera
    [nested+120] empty → skip
    [nested+120] set   → 004B4A10(name, 0, [+124])
Lookout TNG            0 Oakvale, 0 CTCExpression
StartOakValeWest TNG   XXXSectionStart only
EXPRESSION game.bin    social names, not Q_*
Expression_Follow      00843F50 generic name; FALSE QST
007B5680               other class (listing-00780000)
```
