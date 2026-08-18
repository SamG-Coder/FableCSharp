# First native water draw (after Leave)

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: when does native first **draw** water (`CEngineWaterRenderer` `vtbl+16` `00B783F0`)?
Is that after Leave + world/region load? What does C# do instead?

---

## Verdict

**`00B783F0` is the water DIP site. Layer bit `0x20000`. After Leave, first-seen is empty-out — no DIP.**

Load is not draw:

| VA | Job | First-seen New Game |
|---|---|---|
| `00B428E0` → `00B42750(1)` | open maps | `FinalAlbion.stb` **miss** (`[+52].vtbl+12` `al=0`) |
| `00B6D4D0` | sea name onto water | **skipped** (`test bl,bl; je 00B428CA`) |
| `00B41FA0` | `LoadWaterData` | always called; intern `0x1436EC8` **miss** → `00B420E4` |
| `00B783F0` | draw | ctor zeros → `je 00B7A865` bare `ret 4` |

Frontend **does** walk engine `vtbl+32` (`0042E0BB` → `00B27D90` → `00B25950` → water `vtbl+16`). Query `00B7ED70` returns **1**. Draw still empty: no patches, no type-3/4/5 enqueue, `+636=0` so bind `00B6DC40` returns 0.

Host `TryResolve` omit / `ScenePasses.Draws` skip of Water / one-shot soup is **not** the native site. Omitting `SEA_*`/`WATER_*` from landscape FG is **PROVEN**. Pretending `00B6D4D0` always runs, or that `+1448` is the intern pointer, is **DISPROVEN**.

---

## Recovered order (no-save New Game)

```
0042EC7C retail pump
  0042DF9E  frontend frame
    0042E0BB  [retail+88].vtbl+32 = 00B27D90
      00B25950
        phase 1  water vtbl+4  00B71FB0   cmp eax,eax collapse +508..+620
                                     then 00B6DC40  [+636]==0 → ret 0
        phase 2  layers +348..+352
          00B26EF0 bit 0x20000 attached [0x1436E54]
          00B2AB80 → water vtbl+16 00B783F0
            begin==end +508..+624 and not ([+630]&&[+645])
            je 00B7851D → 00B7A865  pop×4 / add esp,40 / ret 4
retail+41
  0042F2A2 Leave
    0042EBB6  teardown Present, not 0042DF9E
    FinalAlbion.wld
    00418DCA Init Game
      004184BD → vtbl+32 00416953 Load world
        004A1840 → display vtbl+208 00B23DC0 → 00B428E0
          CloseStaticMapFile 00B40000
            [+424]==0 → ret          // no 00B6DB80
          EnablePoolAllocation 00BDA070(1)
          OpenStaticMaps 00B42750(1)
            [+52].vtbl+12(98, +48) → al=0   FinalAlbion.stb absent
            00B3E820 current handle
            test bl,bl / je 00B428CA        // NO 00B6D4D0
          [+432]=3
          LoadWaterData 00B41FA0
            009CCDC0(+52, 0x1436EC8)        // __ENGINE_WATER_*
            je 00B420E4                     // no 00B6DAF0
004189C2 game pump
  first 00435530 dest empty
  no region, no 00BF4570, no water DIP
later 00501450 Lookout / 006C2170 / opened patches
  next 00B27D90
    00B71FB0 clears vectors again
    bit 0x4   00BE7BE0  type 3/4/5 → water +0x268 (+616)
    bit 0x40  00BF4570 → 00BF57D1 [obj+28]==4 → 00BF44A0 +0x244 (+580)
    bit 0x20000 00B783F0  nonempty only if those lists filled
```

Game caller of `012A0F3C+32` after Leave is **UNREAD** (same leftover as terrain). Pairing `00B25950` inside `00435530` is **DISPROVEN**.

---

## `00B6D4D0` sea name (listing)

`tools/Fable.ExeIndex/out/01-sections/landscape-trace/sea-name-onto-water-renderer-00b6d4d0.md`
and `listing-00b40000.txt` `00B42750`:

```
00B427F1  call [edx+12]          ; open 98
00B427F4  mov bl, al
…
00B4281B  call 00B3E820
00B42820  test bl, bl
00B42822  je 00B428CA            ; miss: no sea name, no 00B420F0
00B4282B  push 0x1436EC4         ; __ENGINE_SEA_STATIC_MAP_BANK_FILE__
00B42830  call 0099EC30          ; CString on stack
00B42835  mov ecx, [0x1436E54]
00B4283B  push edi               ; edi = this+52  (opened bank)
00B4283C  call 00B6D4D0
```

`00B6D4D0`:

```
mov eax, [esp+4]        ; edi = bank+52
mov [ecx+1448], eax     ; NOT intern 0x1436EC4
lea edx, [esp+8]        ; intern CString
add ecx, 0x5AC          ; +1452
call 0099EFB0           ; copy name string
ret 8
```

Bind `00B6DC40` later `0099F570(+1452, +636)` then `009CCDC0([+1448])` — prefix+region on that bank. StartOakVale blob is 129966 bytes, first u32 **7363**.

| Claim | Status |
|---|---|
| Mode 1 always calls `00B6D4D0` | **DISPROVEN** (`test bl,bl`) |
| First-seen miss skips it | **PROVEN** |
| `+1448` is intern `0x1436EC4` | **DISPROVEN** (bank `+52`) |
| `+1452` is intern name string | **PROVEN** |
| Bind needs `[+636]≠0` | **PROVEN** (`je 00B6DC64` then `+1464==0` → `00B6DD06 xor al,al`) |
| First-seen `00B23F00` (engine vtbl+56) | **DISPROVEN** as E8 / as OpenStaticMaps / MainScene `call [r+0x38]` |

---

## `00B41FA0` LoadWaterData (listing)

`loadwaterdata-full-00b41fa0-00b41fa0.md`. Only `E8` from `00B429CB`.

```
00B41FA8  lea ebp, [esi+52]
00B41FAB  push 0x1436EC8         ; __ENGINE_WATER_STATIC_MAP_BANK_FILE__
00B41FB2  call 009CCDC0
00B41FBB  je 00B420E4            ; miss: pop / add esp,88 / ret
… stream copy …
00B4206E  mov ecx, [0x1436E54]
00B4207E  call 00B6DAF0          ; first u32; 00B6D6E0 cmp eax,8
```

No water-prefix STB entries. Type-8 dwords land on a stack local and `add esp,8` drops them. `al` ignored.

Hit-path ingest does **not** fill draw vectors +508..+624. Sea 7363 is a different intern (`0x1436EC4`) and a different reader (`00BE91E0`).

---

## Draw `00B783F0` empty-out

`water-draw-empty-check-00b783f0` / `water-draw-full-00b783f0`:

Vectors (begin==end): `+508/+512`, `+520/+524`+`+532/+536`, `+544/+548`+`+556/+560`+`+568/+572`, `+580/+584`, `+592/+596`, `+604/+608`+`+616/+620`.
Mesh-ready: `[+630] && [+645]`.

All false → `je 00B7851D` → `00B7A865` (`ret 4`). Second gate `00B72180` only from `00B78584`.

Ctor `00B73760` (`ebx=0`) writes `+508..+624` and `0099E4B0` at `+636`. `+630/+645` stay 0.

Same-frame order after Leave when `00B27D90` runs:

1. Component `vtbl+4` **clears** +508..+620, then bind (no-op).
2. Landscape `0x4` / `0x40` may **push** onto `+0x244` (580) / `+0x268` (616).
3. Water `0x20000` **reads** those vectors.

So the 7363 mesh is not the first-seen fill. Per-cell type 4 / BG type 3–5 is.

---

## Frontend `0042DF9E` vs water

Same walk as terrain (`proofs/terrain-first-draw`):

| Claim | Status |
|---|---|
| Does not `E8 00B783F0` | **PROVEN** |
| Never reaches water `vtbl+16` | **DISPROVEN** (`0042E0BB` → `00B2AB80` bit `0x20000`) |
| Issues first water DIP | **DISPROVEN** (empty-out) |

---

## First water DIP after Leave

| Step | VA | Status |
|---|---|---|
| Leave stops `0042DF9E` | `0042F2A2` | **PROVEN** |
| `00B428E0` is load, not draw | Close → Open → `00B41FA0` | **PROVEN** |
| First-seen STB miss skips `00B6D4D0` | `00B42822` | **PROVEN** |
| `00B41FA0` intern miss | `00B420E4` | **PROVEN** |
| First `00435530` dest empty | no region | **PROVEN** |
| Water draw site | `00B783F0` bit `0x20000` | **PROVEN** |
| First-seen empty-out | `00B7A865` | **PROVEN** |
| Game `00B27D90` after Leave | not in `00435530` | **UNREAD** |
| First nonempty `00B783F0` | type-3/4/5 enqueue after patches | **UNREAD** as clock |
| 7363 sea mesh on first Oakvale frame | needs `+636` | **DISPROVEN** first-seen |

Must-after-load for a **nonempty** water DIP: **PROVEN** (no bank, no enqueue, ctor zeros). First-seen after Leave is empty: **PROVEN**.

---

## C# vs native

| Host | Native | Class |
|---|---|---|
| `SetStaticMapFileForUse` Notes `00B41FA0` after miss | always after `00B42750` | **PROVEN** timing |
| No `00B6D4D0` Note on first-seen | miss skips it | **PROVEN** match |
| `LandscapeTextures`: `+1448` = “sea intern” | `+1448` = bank `+52`; intern **name** at `+1452` | **DISPROVEN** comment |
| Docs / `FORWARD_TREE`: mode 1 always `00B6D4D0` | gated on `vtbl+12` | **DISPROVEN** |
| `OpenStaticMapsForCurrentRegion` opens maps, no `00B6D4D0` / no `00B41FA0` | hit path does both | **DIVERGE** (host fill, not `00B428E0`) |
| `TryResolve` + `FirstSeenWaterDrawShouldSubmit=false` drops `SEA_*`/`WATER_*`/`*LAKE*` from FG | not landscape `00BF4570` albedo | **PROVEN** as FG omit |
| `ScenePasses.Draws` excludes `Water` | `00B2AB80` still **calls** `00B783F0` | **DIVERGE** (call vs skip); DIP empty first-seen **MATCH** |
| `FlushSubmittedLayers` on `009DA9F0` | 2D `+16020`; walk is `00B25950` | **DISPROVEN** pairing |
| `SubmitCurrentWorld` land+C3D+sky soup | layers `0x4` → `0x40` → `0x20` → `0x2000` → **`0x20000`** | **DISPROVEN** as native |
| `CollectVisibleCells` / `ToTileTriangles` drop water faces (`Faces.Count==0`) | holes in FG; water is the other renderer | **PROVEN** FG / **DIVERGE** if type-4 should enqueue |
| `WorldGeometry.Build` lists `StartOakVale_Sea_*` (SeesMap; often `IsSea=false`) | maps may open as headers | **PARTIAL** (WLD flag vs material prefix) |
| `IsLoadableWaterBank` requires u32 `8` | `00B6D6E0` same | **PROVEN** |
| Never read 7363 / `00BE91E0` | first-seen bind never reaches it | **PROVEN** omit |
| One-shot `WorldSubmitted` | redraw every `00B27D90` | **TEMPORARY BRIDGE** |

---

## Classification table

| Claim | Status |
|---|---|
| First water DIP site is `00B783F0` on bit `0x20000` | **PROVEN** |
| That DIP is after Leave, not frontend widgets | **PROVEN** as “no frontend DIP” / **PARTIAL** as first nonempty clock |
| `LoadWaterData` `00B41FA0` is ingest, not draw | **PROVEN** |
| First-seen `00B41FA0` finds a water bank | **DISPROVEN** |
| First-seen `00B6D4D0` runs | **DISPROVEN** |
| `+1448` is intern `0x1436EC4` | **DISPROVEN** |
| First-seen `00B783F0` submits | **DISPROVEN** |
| `0042DF9E` never reaches water `vtbl+16` | **DISPROVEN** |
| `009DA9F0` is the 3D / water walker | **DISPROVEN** |
| Host soup / `442` landscape water is native | **DISPROVEN** |
| Type-8 dwords persist on the renderer | **DISPROVEN** |
| 7363 is accepted by `00B6D6E0` | **DISPROVEN** |
| First-seen `+636` setter | **DISPROVEN** |
| Game caller of engine `vtbl+32` after Leave | **UNREAD** |
| First nonempty `00B783F0` after Lookout/Oakvale patches | **UNREAD** |
| Native Lookout/Oakvale STB re-open that would take `00B6D4D0` | **UNREAD** (same as terrain) |

Dumps: `tools/Fable.ExeIndex/out/01-sections/water/`, `landscape-trace/loadwaterdata-00b41fa0.md`, `landscape-trace/sea-name-onto-water-renderer-00b6d4d0.md`, `newgame-trace/water-*`, `listing-00b40000.txt` `00B42750` / `00B41FA0` / `00B6D4D0`, `listing-00b00000.txt` `00B26EF0`. Host: `LandscapeTextures.cs`, `EngineLifecycle.SetStaticMapFileForUse`, `ScenePasses`, `WorldGeometry`.
