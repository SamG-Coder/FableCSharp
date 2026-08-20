# Host leftover at first `006B8410` vs `InitWorldCameras`

Investigation only. No production `src/` / `tests/` edits.
Do **not** start Oakvale / `00DBDE40` / `CAM_OVIF_SHOT2`.

Question: `006B8410` world-camera first-seen vs host
`InitWorldCameras`. **MATCH** or leftover? First leftover
field after current host?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `proofs/006B8410-worldcamera` (thunk / empty
reset / no host Note); `proofs/audit-worldcamera` §3.1;
`proofs/0049F180-first-children`;
`EngineLifecycle.InitWorldCameras` (read only);
`WorldCamera.Construct` (read only);
ExeIndex `listing-00480000.txt` `0049F1DC`–`0049F1EA`;
`listing-00680000.txt` `006B4900` / `006B4B02`–`006B4B4A`
/ `006B8410` / `006B84B0`;
`listing-00880000.txt` `00880A40` / `00881210` /
`00881370`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First-seen `006B8410` vs host `InitWorldCameras` tracked fields? | **MATCH.** Thunk is an empty reset of `[WorldCamera+6500]+144`. Ctor already left counts 0 and dummy heads self-linked. Host `Construct` never stores that bank, so there is no net first-seen store to copy. | **MATCH** / **PROVEN** |
| Same site as `InitWorldCameras`? | **No.** Host ctor is Init World `006B4900`. Native `006B8410` is later unique `0049F1E5` under `0049F180`. Folding the reset into `InitWorldCameras` is the wrong function. | **DISPROVEN** as Init World work |
| Host leftover **side effect**? | **No.** Omitting the call does not diverge modeled vtbl / `+24` / `+61` / `+68` / SlotA. Adding the reset is leftover theater. | **PROVEN** skip; **DISPROVEN** leftover mutation |
| First leftover field after current host? | **`WorldCamera+4`.** `006B4900` `mov [esi+4], eax` (world*). Host `Construct` stores vtbl then `+24` / `+61` / `+68` / SlotA. That is the first ctor store host dropped. | **PROVEN leftover** |
| First leftover field **`006B8410` names**? | **`WorldCamera+6500`**, then bank **`+144+20`** (count, already 0). Sibling alloc **`+6496`** is leftover too, but is not this `ecx`. | **PROVEN leftover** (do not implement first-seen) |

---

## Verdict

**MATCH on first-seen state. Leftover to implement the
call or the colour-filter bank. First leftover field
after current host: `WorldCamera+4`.**

Native first-seen is `0049F1DC` `[world+24]+6500` →
`006B8410` `add ecx, 0x90` / `jmp 00881210`. Counts
`+20/+124/+164` are ctor 0 → three `je` skips. Dummy
heads at `+4/+8/+12` and `+108/+148` self-point → no
`00BFEA14`. Tail `00880A40` re-zeros scalars the ctor
already wrote. **PROVEN** empty reset
(`006B8410-worldcamera`).

Host `InitWorldCameras` Notes `006B4900` / `0069AE80` /
`006FD8C0` and calls `WorldCamera.Construct()`. No
`006B8410`, no `00881210`, no `+6496` / `+6500`. That
omit is a **MATCH** skip of a no-op, not a missing
first-seen store.

Do **not** grow `InitWorldCameras` with a colour-filter
reset. Do **not** treat `006B8410` as a second ctor.

---

## 1. Host site (`InitWorldCameras`)

`EnterGame` `"Init World"` → `InitWorldCameras()`:

```
Note(WorldCameraCtor, … "006B4900 world+24 size 0x1970 …");
WorldCamera.Construct();
WorldCameraPresent = true;
Note(GameCameraManagerCtor, … "0069AE80 …");
GameCameraManager.Construct();
Note(GameCameraCtor, … "006FD8C0 …");
GameCamera.Construct();
```

`WorldCamera.Construct` (current host fields):

| Host field | Native offset | First-seen |
|---|---|---|
| `VtblValue` | `+0` | `0125D53C` **MATCH** |
| `CameraTickTimer` | `+24` | `−1.0` **MATCH** |
| `PoseSkipFlag` | `+61` | 0 **MATCH** |
| `Seeded` | `+68` | 0 **MATCH** |
| `SlotA` | `+3084…+3108` | param 0, weights 0.2, V0/V1 `(1,0,0)` **MATCH** |
| `SlotB` / `Output` | `+6188` / `+6292` | host zeros; ctor does not write Output **MATCH** first-seen zeros |

No `+4`. No `+84` / `+3188` follow banks. No `+6376`.
No `+6496` / `+6500`.

`InitCharactersAndQuests` Notes `0049F180` / bind / GUI
/ quests only. Still no `006B8410`. **PROVEN** no Note
/ no call at either site (`006B8410-worldcamera` §3).

---

## 2. Native first-seen (`0049F1E5`)

`listing-00480000.txt`:

```
0049F1DC  mov ecx, [esi+24]         // WorldCamera
0049F1DF  mov ecx, [ecx+6500]
0049F1E5  call 006B8410
0049F1EA  push "Init GUI"
```

After Init World `006B4900`, before `"Init GUI"`.
**DISPROVEN** as a child of `InitWorldCameras`.

`listing-00680000.txt`:

```
006B8410  add ecx, 0x90             // +144
006B8416  jmp 00881210
```

`00881210` (`this` = `[+6500]+144`):

```
[edi+20]==0  → skip +16
[edi+124]==0 → skip +120
[edi+164]==0 → skip +160
3× walk [edi+4] / [edi+108] / [edi+148]
  head==head → no 00BFEA14
jmp 00880A40                       // re-zero scalars
```

Ctor of that `this` is Init World `006B84B0` →
`00881370`: vtbl `01278058`, three dummy circular
heads, counts **0**, then the same `00880A40`.
Insert helpers `006B84E0` / `006B8550` are **not** on
this walk. First `006B8640` is later seed `006B3FF0`
(`004A5DF3`), after dummy pumps. **PROVEN** empty.

`00880A40` first store is `this+28` (0). Host has no
such field. Re-running it first-seen does not change
ctor values. **MATCH** vs post-`Construct` modeled
state; **LEFTOVER** if implemented as new host
storage.

---

## 3. First leftover field after current host

Walk `006B4900` stores in **offset** order against
`Construct`. Host MATCH prefix is `+0`, then a hole,
then `+24` / `+61` / `+68`, then SlotA.

| Offset | Native first-seen | Host | Class |
|---|---|---|---|
| `+0` | vtbl `0125D53C` | `VtblValue` | **MATCH** |
| **`+4`** | world* (`[esp+36]`) | **none** | **first leftover** |
| `+8` / `+12` | from `[world+4]+216` packet | none | leftover |
| `+16` | 0 | none | leftover |
| `+24` | `−1.0` | `CameraTickTimer` | **MATCH** |
| `+32…+48` | copied floats | none | leftover |
| `+52` / `+56=0x7B` / `+60` / `+62` / `+64` / `+69=1` | ctor | none | leftover |
| `+61` / `+68` | 0 / 0 | `PoseSkipFlag` / `Seeded` | **MATCH** |
| `+72/+76/+80` | 0 then later vector fill | none | leftover |
| `+84` (6× `0x1F4` `008864A0`) | follow bank | noted, not stored | leftover bank |
| `+3084…+3108` | SlotA | `SlotA` | **MATCH** |
| `+3168` | 0 (follow-spring gate) | not stored | leftover |
| `+3188` (6× `008864A0`) | follow bank B | none | leftover bank |
| `+6188` | SlotB zeros | `SlotB` | **MATCH** |
| `+6292…+6352` | Output (lerp later) | `Output` zeros | **MATCH** first-seen |
| `+6376/+6380/+6384` | 0 | none | leftover after last host slot |
| `+6484/+6488/+6492` | 0 | none | leftover |
| `+6496` | `008852E0` alloc | none | leftover sibling |
| **`+6500`** | `006B84B0` size `0x160` | none | **`006B8410` ecx** |
| `+6504` | `00A01B10` / dword `01238C6C` | none | leftover |

**First leftover field after current host:**
`WorldCamera+4`.

That is the first ctor dword after the vtbl host
already stores. It is **not** a `006B8410` write.

If the question is “first leftover **after the last
host-modeled offset** (`Out4` `+6352`)”: **`+6376`**.
Still not this thunk.

If the question is “first leftover **this function
would need**”: **`WorldCamera+6500`**, then
`[+6500]+144+20`. First-seen value of that count is
**0**. Filling it now is leftover theater.

---

## 4. What is **not** a `006B8410` leftover

| Host / native action | When | Owner | vs `006B8410` |
|---|---|---|---|
| `WorldCamera.Construct` vtbl / `+24` / axes / `+68` | `InitWorldCameras` | `006B4900` | **DISPROVEN** as this body |
| `+6496` `008852E0` alloc | Init World, before `0049F180` | `006B4900` | sibling; **DISPROVEN** as this `ecx` |
| `+6500` `006B84B0` / `00881370` | Init World | ctor of this `ecx` | **DISPROVEN** as the reset |
| `GameCameraManager` / `GameCamera` | same `InitWorldCameras` | `0069AE80` / `006FD8C0` | **DISPROVEN** |
| `006B8640` copy on `+6500` | first `006B3FF0` | later seed | **DISPROVEN** as first-seen |
| `006B42F0` / `00B23EC0` colour apply | WorldFrame>1 `0049E080` | apply tail | **DISPROVEN** |
| Oakvale `CAM_OVIF_SHOT2` | later intro | not this child | **DISPROVEN** |

Host `ApplyWorldCamera` hero+V4+70° bind is a later
**DIVERGE** (`audit-worldcamera`). Not this Note.

---

## 5. What would be leftover (do not implement)

| Host action | Class |
|---|---|
| Keep omitting `006B8410` at `InitWorldCameras` | **MATCH** skip of a no-op |
| Note-only `006B8410` under `InitWorldCameras` | **DIVERGE** timing (wrong parent) |
| Note-only under `InitCharactersAndQuests` | listing pair only; still no store |
| Alloc `+6496` / `+6500` / run `00881210` first-seen | **LEFTOVER** empty-reset theater |
| Walk `+16/+120/+160` or free `+4` nodes here | **LEFTOVER** (counts 0; dummy heads) |
| Store `00880A40` `+28…+200` on the host camera | **LEFTOVER** (ctor already set them) |
| Implement `WorldCamera+4` as this thunk | **DISPROVEN** (ctor field, not reset) |
| Implement `006B8640` / `00B23EC0` here | **DISPROVEN** (later seed / apply) |

Live (non-first-seen) fill of the `+144` lists after
`006B8550` is **UNREAD** here and is not Init World.

---

## Classifications (short)

1. **First-seen `006B8410` vs `InitWorldCameras`
   tracked fields: MATCH. PROVEN.** Empty reset of
   ctor-empty `[+6500]+144`.
2. **Same function as `006B4900`: DISPROVEN.** Unique
   `0049F1E5` after `"Init Characters"` bind.
3. **No host leftover side effect. PROVEN.** No Note,
   no call, no modeled store.
4. **First leftover field after current host:
   `WorldCamera+4`. PROVEN.** First ctor store
   `Construct` drops.
5. **First leftover `006B8410` names: `+6500` then
   bank `+144+20==0`. PROVEN leftover.** Do not
   implement the skipped arms first-seen.
6. **Treat `006B8410` as construct / colour apply /
   Oakvale seed. DISPROVEN.**
