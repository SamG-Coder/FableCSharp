# Resource manager — bank / archive / handle / cache / unload

Investigation only. 2026-08-18. TLC `Fable.exe`
(`42D7DBDF-0106C000-16666624`). No `EngineLifecycle.cs` edits.

Bodies walked this pass with `tools/Fable.ExeIndex` (`fn --exact`,
`vtbl`, `calls`): `009A8150`, `009AC700`, `009A76D0`, `009A7F80`,
`009A7CA0`, `00A09F20`, `00A27030`, `009D56C0`, `009D06F0`,
`009D6FD0`, `009CFBC0`, `009CE050`, `009AD410`, `009E5170`,
`009B21F0`, `0049E620`, `004BBFD0`, `00416C8A`, `009F83D0`,
`009FEA20`, `009FD4E0`, `004BBFC0`, `009BE8B0`, `00B40000`,
`00B40070`, `00B42750`, `00B42530`, `00B3EF40`, `00B3E820`,
`00B41E50`, `00B420F0`, `009CCDC0`, `006C2170`, `00A243B0`,
`00A24330`, `00A26660`, `009FD910`, `009FF8B0`, `009D5230`,
`00BDC4F0`, `00BDDD50`. Vtbls `0129CE94` (mesh) and `0129C8CC`
(graphic).

Statuses: **PROVEN** body+callees, **PARTIAL** path known / inner
body not fully walked, **UNREAD** not dumped here, **DISPROVEN**
claimed behaviour is not in the body.

Use this to replace host convenience loading. Native is fast
because open is **names + directories + headers + TNG text**.
C3D / DXT stay **handles** until draw. Banks survive region
change. Maps do not.

---

## Verdict

Native resource ownership is a **four-layer** stack:

| Layer | Lives | What it holds |
|---|---|---|
| Bank manager `0x13CA79C` | process | Name pairs only (`009A8150`) |
| BIG archive handle | process, after first named open | File + sub-bank directory |
| Named bank object (`MBANK_ALLMESHES` / `GBANK_MAIN`) | process, after Init World / Init Graphics | Directory tables + per-id cache slots |
| Static-map slot / patch | **region** | LEV/STB header + current/neighbour patch objects |

`009AD410` is **not** a parse. It is a hash → 12-byte shared
handle. `00A243B0` / `009FD910` are the later id → blob paths.
`00B40000` unloads **map slots**, not banks.

Host already matches the open spine (`RegisterRetailBankTable`
names-only, `MeshBank.Open` directory, `PresentWorld`
`expandGeometry: false`). It still DIVERGE on (1) neighbour
tessellate-as-soup, (2) DXT→RGBA, (3) extra WAD/`GameBin`
opens, (4) no patch-object lifetime, (5) `CloseStaticMapFile`
clearing lists only.

Do **not** fix load by parsing every C3D or decoding
`textures.big`. That is the opposite of this tree.

---

## 1. Shared handle / refcount — **PROVEN**

Every long-lived object in this pass is a 12-byte block:

```
+0  int32  refcount
+4  fn*    dtor   (thiscall on +8)
+8  void*  object
```

Acquire: `inc [ptr]`. Release: `dec [ptr]`; if 0 then
`call [ptr+4](ptr+8)` then `00BFE9BC` free. Seen at:

| Site | Stores |
|---|---|
| `00A09F20` miss | alloc 12, `[+0]=1`, `[+4]=0x428AE7`, `[+8]=bank 0x460` |
| `0049E620` | world `+60` object / `+64` handle |
| `009F83D0` miss | same shape; dtor `0x419036`; object `0x30C` graphic bank |
| `00B3E820` | map-manager `+280` / `+284` current archive |
| `00B41E50` / `00B3EF40` | map slot `+60` / `+64` STB blob |
| `009D56C0` | bank `+272` archive after `009A7F80` |
| `00A24330` | mesh parsed-object slot at bank `+944` |

`009D6FD0` is the generic “assign shared handle to `{ptr,ref}`
pair”: release old, store new, `inc` new.

This is the native cache key. A **handle** is not a parsed C3D
and not a D3D texture.

---

## 2. Bank lifetime

### 2.1 Retail name table `009A8150` — **PROVEN**

```
009A8150(this=0x13CA79C, name_a, name_b)
  ecx += 24
  009AC700 insert-or-find in manager+24 map
  0099EFB0 copy name into the node
  ret 8
```

`009AC700`: `009AAE00` lookup; hit returns `node+20`. Miss
builds two interned strings and `009ABDB0` inserts. **No file
I/O.** `009A76D0` is only `mov [ecx+20], al; ret 4` (a flag on
the manager), not a ctor.

Bootstrap pairs (`FORWARD_TREE` §2 / host `RetailBanks`):

```
GBANK_MAIN / GBANK_MAIN_PC
GBANK_GUI / GBANK_GUI_PC
GBANK_FRONT_END / GBANK_FRONT_END_PC
PARTICLE_MAIN / PARTICLE_MAIN_PC
PARTICLE_FRONTEND / PARTICLE_FRONTEND_PC
```

`MBANK_ALLMESHES` is **not** in this table. It is opened later
by name at `0049E620`.

Host `RegisterRetailBankTable` MATCH.

### 2.2 Mesh bank `0049E620` / `00A09F20` — **PROVEN**

```
0049E620  "Opening Mesh Bank"
  009A4EC0 engine
  00A09F20([engine+116], "MBANK_ALLMESHES")
    hit  → copy existing {object, handle}; inc handle
    miss → 00BFEA1A(0x460) → 00A27030
           vtbl 0129CE94; tables empty
           [bank].vtbl+4(name) = 009D56C0
           link 20-byte node onto [engine+116]+8 list
  world+60 / +64 = {object, handle}   (replace + release old)
  world+68 = [bank+960]
  "Setting Mesh Bank"
  004BBFD0  mov [0x13B8A04], ecx     (global, 2 insns)
```

`00A27030` ctor (**PROVEN**): `009D5F80` base bank, four
`009FC5F0` tables at +368/+492/+616/+740, hash tree at +968,
type vectors at +988/+1008/+1016, helpers `00AA5C80` /
`00AA4710` / `00AA0F60` at +356/+360/+960. Then `009D5230`
which is **`ret 4`** (no-op hook). **No directory read in the
ctor.** Directory is `vtbl+4`.

Object size `0x460` MATCH host `MeshBank.ObjectSize`. One
instance for the process after Init World.

### 2.3 Graphic bank `00416C8A` / `009F83D0` — **PROVEN**

```
00416C8A  "Init Graphics"
  009BE800 / 009BE830 / 009BE870 / 009BE8B0
    (device FourCC probes: DXT1/3/5 CreateTexture, not bank decode)
  "Opening Main Graphic Bank"
  push "GBANK_MAIN"
  009F83D0([0x13B837C], formats, name, flags 1,1)
    same list-walk as 00A09F20
    miss → 00BFEA1A(0x30C) → 009FEA20  vtbl 0129C8CC
           009FD4E0 copy format block; 11 scratch 009FA280
           [bank].vtbl+4 = 009D56C0   (SAME slot as mesh)
  004194BA store at game+0x161C8
  "Setting Main Graphic Bank"
  004BBFC0  mov [0x13B8A08], ecx
```

`009BE8B0` (**PROVEN**): `device.vtbl+40` `CreateTexture`
width/height/mips from `this+92/+96/+452`, FourCC DXT1, usage
3, pool 0 (`D3DPOOL_SCRATCH` on the AVI path comments). Then
`009E3830` record. **Not** a walk of `textures.big`.

`009FD4E0` pre-creates **11** scratch textures from the format
descriptor copied out of `00415B80`. That is device capability
setup, not `TextureLibrary` decode-all.

Host `OpenTextureBank` opens `GBANK_MAIN_PC` directory. Native
string is `GBANK_MAIN`; the PC remap lives in the manager name
pair from `009A8150`. **PARTIAL** host name (`_PC` vs logical).
Directory-only at init is **MATCH**.

### 2.4 Shared vtbl open slot — **PROVEN**

| Slot | Mesh `0129CE94` | Graphic `0129C8CC` |
|---|---|---|
| +4 open | **`009D56C0`** | **`009D56C0`** |
| +8 / +12 / +16 | `009D51D0` / `009D5820` / `009D5200` | same |
| +32 type id | `00A23AC0` | `009FD0B0` |
| +40 bucket | `00A26660` | `009FF8B0` → `00A01820` at +480 |
| +48 load | `00A26D40` | `009FF590` |
| +52 get/reload | `00A243B0` | `009FD910` |
| +56 evict | `00A24330` | `009FD970` |

One open implementation. Two bank objects.

`009D5230` is a stub. **DISPROVEN** that ctor “opens the file”.

---

## 3. Archive lifetime

### 3.1 `009D56C0` Open Bank File Async — **PROVEN**

```
009D56C0(this=bank, name, flags)
  009D06F0(name, flags)          // directory bind
    [+141]=1; [+120]=flags; copy name to +136
    if [0x13CA7B0] && !(flags&4):
      [+140]=1
      009A7F80(manager=0x13CA79C, name, out handle)
        miss → fail
      vtbl+32() must match handle+28     // bank type
      bind archive at bank+124
      00994700(0x4000) stream
      009CFBC0 parse DIRECTORY
      vtbl+44
    else:
      009A7CA0 resolve path or " bank not found!"
      vtbl+12 open by path
  if [+140]:
    009A7F80 again
    009D6FD0 store handle at bank+272    // keep archive
    [+328]=[handle+20]; [+329]=0
  else:
    alloc 28 → 0098DFD0 file object
    009CBF10 / 0098E1E0 sync read
  return 1
```

`009A7F80` (**PROVEN**): look up the **logical name** in
manager+24 (`009AB5D0`). Walk already-opened archives at
manager+16. For each, look up the sub-bank at `archive+24`
(`009AB4F0`). Hit copies a 20-byte record (id/size/offset/…)
and assigns the archive shared handle to the out-pair. Miss
walks the next archive. Empty list → `al=0`.

So: **one BIG file can back many named banks**. Opening
`MBANK_ALLMESHES` binds `graphics.big` once; later
`GBANK_MAIN` binds `textures.big` the same way. The archive
handle at bank+272 keeps the file open for the bank’s life.

`009A7CA0` is the path fallback when the manager has the name
but no open archive yet. Fail string `" bank not found!"`.
**PARTIAL**: who first opens the `.big` path and inserts it
into manager+16 (`009A7CA0` / `vtbl+12` inner) is not fully
walked. Host `BigArchive.Open(path)` is the semantic stand-in.

### 3.2 Directory parse `009CFBC0` — **PROVEN** (shape)

Stream walk, not blob load:

1. Read count, then `count` × `{u32,u32}` stat pairs into an
   8-byte vector (`009D2AF0`). Matches host
   `BankDirectory.ReadEntries` skipping `statsCount*8`.
2. For each entry: id / type / size / offset / extra / name
   (`00996390`) / optional dep blob.
3. `009CE050` inserts into bank tables gated by `[+120]` bits:
   - `0x40` → map at +172 (`009CCDC0` later hits this)
   - `0x100` → sorted hash pairs at +208 (`004014A0` hash;
     this is `009AD410`’s table)
   - `0x80` → name map at +196
4. `[+148]=1`. Optional `009CD740` if `[+141]`.

**No C3D. No DXT. No `Read(entry)` of payload.** Host
`MeshBank.Open` / `TextureLibrary` ctor MATCH.

`009CCDC0` (**PROVEN**): `009D2DD0` on `this+172`; hit returns
`[node+20]`, miss 0. Used by `00B42750` for
`__STATIC_MAP_COMMON_HEADER__` (STB name), not for C3D.

---

## 4. Handles and lazy lookup

### 4.1 `009AD410` def → mesh handle — **PROVEN**

```
009AD410(this=bank, name*)
  if name empty → 0
  hash = 004014A0(name bytes, len)
  009B21F0 binary search [bank+104, bank+108)   // 8-byte {hash, handle}
  hit  → return [entry+4]
  miss → 009E5170([bank+96], name)              // secondary map
         hit → *out; al=1
  else 0
```

`009B21F0` is a lower-bound on a sorted `u32` key, stride 8.
`009E5170` is a tree walk (`009E6910` / `009E6530`) on the
object at bank+96.

Returns a **handle**. Does **not** call `MeshFile` / C3D /
`vtbl+48`. Host `MeshBank.TryGetEntry` MATCH. Host `Get`
parsing on first draw is the **later** slot (`00A243B0`), not
this function. Calling `Get` from `PresentWorld` would DIVERGE.

### 4.2 Mesh cache `00A243B0` / `00A24330` — **PROVEN**

`00A26660` (vtbl+40, after directory): if `[+141]`, count
directory types into three buckets and reserve:

| Types | Dest |
|---|---|
| 1, 2, 4, 5 | bank+908 |
| 3 | bank+920 |
| 6, 7, 8, 9, 10 | bank+932 |

Host drops `Type==3` at `MeshBank.Open`. Native **keeps** type
3 in its own vector. **PARTIAL** (host skip vs native bucket).

`00A243B0` (vtbl+52, get-or-load):

```
if id < ([+900]-[+896])/4 && [+896][id] != 0
  [obj].vtbl+12()
  return this.vtbl+84(out, id, flags)     // cached
else
  return this.vtbl+48(out, id, flags)     // load 00A26D40 UNREAD
```

`00A24330` (vtbl+56, evict id):

```
if id in [+944, +948) 8-byte slots:
  release handle at slot+4
  zero slot
[+896][id] = 0
```

`calls 00A24330` = **0 E8 sites**. Evict is vtbl-only.
**UNREAD** who dispatches slot +56 (memory pressure? explicit
unload?). Region change does **not** `E8` it.

Parsed C3D objects live in bank+896 / +944 until evict or
bank dtor (`00A27350` walks those vectors and releases).
Banks are **not** destroyed on map close.

### 4.3 Texture cache `009FD910` — **PROVEN** (slot), **PARTIAL** (payload)

```
009FD910(this, out, id, flags)          // graphic vtbl+52
  n = ([+484]-[+480]) / 44
  if id < n && record[id]+36 != -1
    this.vtbl+56(id)                    // 009FD970 evict
  return this.vtbl+48(out, id, flags)   // 009FF590 load UNREAD
```

44-byte records at +480. Occupied sentinel is `dword+36 != -1`.
Unlike mesh get, this path **always** ends in load (evict first
if dirty). First-seen create is `009BE8B0` DXT `CreateTexture`,
not host RGBA.

Host `TextureLibrary._cache` is parse-once RGBA. Native is DXT
on device, scratch pool, per id. **DIVERGE** format.
**MATCH** “not at Init Graphics”.

GPU residency / `Unlock` / device-lost rebuild: **UNREAD**
(see F-load-performance §4). Host `SetTextures` destroy+upload
every Present is **DIVERGE** (proven in F, not re-clocked here).

---

## 5. Map / region resources

### 5.1 Region apply `006C2170` — **PROVEN** (this body does not unload)

Job vector at `this+16..+20`, 28-byte records
(`0x92492493` = divide by 28):

| Pass | String / call | What |
|---|---|---|
| 1 | `"Loading topology"` | `[+4]` blob handle; world-map vtbl+24; `00638310` |
| 2 | `"Loading objects"` | `00522720` then `00521AE0` ContainsMap TNG |
| 3 | (no string) | `[+12]` → `00500230` / `0050AF10` |
| 4 | `"Region Level Files: Post Load Initialise"` | `004FD020` |
| 5 | `"Region Level Files: Activate Topology"` | `004FCBB0` if `[+4]`; `004FCFE0` |
| 6 | if `[+28]>0` | world-map vtbl+88; `004FC8A0` SetRegionAsLoaded |

`004FC8A0` is what reaches `00B428E0` (close then open maps).

**DISPROVEN** that `006C2170` itself unloads the previous
ContainsMap TNG list. FORWARD_TREE §13 still lists that unload
as an UNREAD child; it is **not this body**. Previous-region
thing teardown is **UNREAD** (look before enqueue /
`006C27A0` job build, not here).

Host `ApplyLoadJob` MATCH on ContainsMap TNG + Activate
Topology + `SetRegionAsLoaded`. Extra: opens **its own** WAD
and disposes it (`F-load-performance`). `LevelLibrary` already
holds one.

### 5.2 Close `00B40000` — **PROVEN**

```
00B40000(this=map-manager)
  if [+424]==0  return
  for i=1 .. (end-begin)/4 - 1:          // SKIP index 0
    if list[i]  00B3EF40(list[i])
  if [0x1436E54]  00B6DB80                // water
  [+52].vtbl+20
  [+424]=0; [+432]=0
```

`00B3EF40` CloseStaticMap (**PROVEN**):

```
if [+32]==0  return
if [+0]  00BDC4F0     // current patch: 00BDC450 then [+32]=0
if [+4]  00BDDD50     // neighbour: 00BDDC00 then [+4]=0
release [+64] handle
[+64]=0; [+60]=0; [+32]=0
```

`00BDC4F0` / `00BDDD50` inners (`00BDC450` / `00BDDC00`)
**PARTIAL** (wrappers proven; GPU/VB teardown UNREAD).

Index 0 (current) is **not** closed by the list walk. Water +
vtbl+20 cover the rest. Host `CloseStaticMapFile` walks from 1
then **clears everything** including current headers. **PARTIAL**
(start index MATCH; host also drops current and has no patch
objects to destroy).

Banks, `graphics.big`, `textures.big`, WAD, STB directory,
mesh/texture caches: **untouched**.

### 5.3 Open `00B42750` — **PROVEN**

```
00B42750(this, mode)
  if [+424]==mode  return
  00B40070                         // 32 m occupancy, once
  if mode==2:
    for each [+32..+36): 00B42530(slot, 2)
  else:                            // mode 1 = SetStaticMapFileForUse
    [+52].vtbl+12(98, +48)
    00B3E820 bind current handle from [0x1436E98]+2272
    00B6D4D0 sea name
    009CCDC0(+52, 0x1436ECC)       // STB / common header
    hit  → 00B420F0(size); [+424]=mode; return
    miss → 00B42530(slot, mode) each list entry
  [+424]=mode
```

`00B40070` (**PROVEN**): if `[+12]==0`, union map `+96/+98`
origins and `+92/+94` sizes, snap to 32, allocate `2*cols*rows`
words at +12, `00B3EDF0` stamp each slot. Occupancy grid, not
a mesh load.

`00B3E820` (**PROVEN**): replace `{+280,+284}` shared handle
(current STB/archive). Inc new, dec old, free at 0.

`00B420F0` (**PROVEN**): `009CC240` size, alloc, `009CC2A0`
read intern blob, parse name records, then **every**
`[+32..+36)` slot → `00B41E50`.

`00B41E50` (**PROVEN**) — the STB-hit attach:

```
0042B467(+28) resolve
00B3EF40 close this slot
vtbl+4 on the intern stream
00B3EFA0 header → stack dest
copy dest+88.. into slot+36 (24 bytes)
009D58D0 blob handle
assign slot+60/+64 (refcount)
00BE03A0(+68) background-patch ctor input
00BDD0E0([+0], +68, stream)     // current CEngineLandscapePatch
if [+4]  00BDF010([+4], +68, stream)  // neighbour CLandscapeBackgroundPatch
[+32]=1
```

`00B42530` is the **STB-miss fallback** (and the mode-2
driver). New Game Lookout is the hit path (`00B420F0` /
`00B41E50`), not `00B42530`. Host `AttachStaticMap` MATCH.
Host `OpenStaticMap` is the miss path only.

Open is **header + patch objects + blob handle**. Not C3D.
Not `LevHeightField.Parse` of the fine grid. Not
`ToTileTriangles`. Host `PeekMapHeader` + null
`CurrentCompiledLev` / `CurrentHeightField` MATCH at open
(`F-load-performance`, `Install_banks` asserts).

### 5.4 Neighbour preload — **PROVEN** (who), **DIVERGE** (host draw)

Mode 1 opens **every** pointer in `[+32..+36)`. That list is
the WLD Contains ∪ Sees ∪ BWD-touch set (`C-terrain-static-map`
§10, host `StaticMapsAround`). Neighbours are
`CLandscapeBackgroundPatch` (`00BDF010` / ctor `00BE6090`).
They are **terrain**. `006C2170` does **not** load neighbour
TNG. `Activate Topology` `004FCBB0` sets current-only
`record+38=1`.

Native draw: per-patch AABB `00BDC2D0` then per-cell
`00BF4570` (`C-terrain`). Host `SubmitCurrentWorld` →
`TessellateVisible` dumps **all stored tiles** of every opened
map that survives a **map-sized** AABB (`F-load-performance`
30–43 s). That is the hitch. It is **not** required by this
ownership model.

---

## 6. What native keeps vs what it drops

**Initialize once, keep until process teardown**

- Manager name map (`009A8150`)
- `MBANK_ALLMESHES` object + `graphics.big` handle (`0049E620`)
- `GBANK_MAIN` object + `textures.big` handle (`00416C8A`)
- Globals `0x13B8A04` (mesh) / `0x13B8A08` (graphic)
- WLD / region graph / def bins (`FORWARD_TREE` §10)
- Parsed C3D / DXT **slots** until vtbl+56 (almost never on
  New Game)

**Per region**

- ContainsMap TNG (`006C2170` / `00521AE0`)
- Topology flags `+38/+39`
- `004FC8A0` → close maps → open maps → water

**Per map slot (open)**

- 48-byte-class LEV header (`00B3EFA0`)
- STB blob handle at +60/+64
- Current patch (`00BDD0E0`) and/or neighbour (`00BDF010`)
- `[+32]=1`

**Per map slot (close)**

- Destroy current patch, destroy neighbour, release blob handle
- Do **not** close banks

**At draw**

- `009AD410` already done (handle on the thing)
- Mesh vtbl+52 / +48 → blob
- Texture vtbl+52 / `009BE8B0` → DXT texture
- Landscape: stored strips on the **opened patch**, slot
  already bound (`00BF50E0`)

---

## 7. Host vs native (replace convenience loading)

| Resource | Native | Host now | Match |
|---|---|---|---|
| `009A8150` names | Insert only | `RegisterRetailBankTable` | **MATCH** |
| `MBANK_ALLMESHES` | One 0x460; directory; global `0x13B8A04` | one `MeshBank` | **MATCH** |
| `009D56C0` | Shared open; archive handle +272 | `BigArchive.Open` once | **MATCH** |
| `009AD410` | Hash → handle | `TryGetEntry` at open | **MATCH** |
| C3D parse | `00A243B0` / vtbl+48 at draw | `MeshBank.Get` at submit | **MATCH** (timing); host flatten after is DIVERGE |
| Mesh evict `00A24330` | vtbl+56, 0 E8 | never evicts | **PARTIAL** (New Game does not need it) |
| Type 3 entries | own vector +920 | dropped at Open | **PARTIAL** |
| `GBANK_MAIN` | One 0x30C; same open slot; global `0x13B8A08` | `TextureLibrary` | **MATCH** (directory) |
| Texture payload | DXT `CreateTexture` | LZO+DXT→RGBA | **DIVERGE** |
| WAD / STB / WLD | once | `LevelLibrary` + extra WAD in `ApplyLoadJob` / global things | **DIVERGE** (count) |
| LEV/STB at open | header + patch | `PeekMapHeader`; full parse cached later | **MATCH** at open |
| Neighbour maps | background patches | headers at open; **tessellate at submit** | **DIVERGE** (submit) |
| `00B40000` | close list from 1; water; `[+424]=0` | clear lists + current | **PARTIAL** |
| `006C2170` unload previous TNG | **not in this body** | host never unloads `_thingsByMap` | **UNREAD** native / host keeps |
| `PresentWorld` `game.bin` | not here | second `GameBin.Load` | **DIVERGE** (F) |
| Banks across region change | kept | kept | **MATCH** |

**What to implement (not done here)**

1. One owner: manager names + one mesh bank + one graphic bank
   + one WAD + one STB + one WLD. Stop opening WAD in
   `ApplyLoadJob` / `PresentWorld`.
2. Keep `009AD410` as **id/name → handle**. Parse C3D only
   inside submit `Get` (already). Never parse in
   `PresentWorld`.
3. Texture: directory at Init Graphics; DXT (or at least
   decode-once) per submitted id; do not rebuild the set every
   Present (F).
4. Map open = `00B41E50` shape: close slot, header, blob
   handle, current **or** neighbour patch. Do not
   `LoadHeightField` / `ToTileTriangles` at open.
5. Map close = `00B3EF40` on list **from index 1**, then
   water. Do not dispose `MeshBank` / `TextureLibrary`.
6. Neighbour preload = patch objects + headers, **not** TNG,
   **not** world-space soup. Draw with per-patch AABB then
   stored strips (`C-terrain`).
7. Do not invent a “load everything” cache. Native never
   does. `LevelLibrary` full-LEV/`_heights` dicts are a host
   convenience; they are legal **after** first draw of that
   map, not at `00B42750`.

---

## 8. Classification index

| Claim | Status |
|---|---|
| `009A8150` names only; no `.big` | **PROVEN** |
| `009A76D0` constructs the manager | **DISPROVEN** — stores a byte at +20 |
| `00A09F20` hit returns existing 0x460 + inc ref | **PROVEN** |
| `00A09F20` miss alloc 0x460 + `00A27030` + vtbl+4 | **PROVEN** |
| Mesh/graphic vtbl+4 is `009D56C0` | **PROVEN** |
| `009D56C0` → `009A7F80` on `0x13CA79C` then `009CFBC0` directory | **PROVEN** |
| `009CFBC0` reads every C3D | **DISPROVEN** — directory fields + hash insert |
| `004BBFD0` / `004BBFC0` are global stores | **PROVEN** (`0x13B8A04` / `0x13B8A08`) |
| `009AD410` parses C3D | **DISPROVEN** — hash + `009E5170` |
| `009AD410` table is `[bank+104..+108)` stride 8 | **PROVEN** |
| Mesh get-or-load is vtbl+52 `00A243B0`; evict vtbl+56 | **PROVEN** |
| `00A24330` called on region change | **DISPROVEN** as E8 (0 callers); vtbl dispatch **UNREAD** |
| `00416C8A` decodes `textures.big` | **DISPROVEN** — FourCC probes + `009F83D0` directory |
| Graphic bank size 0x30C, same open as mesh | **PROVEN** |
| `009BE8B0` is per-id DXT `CreateTexture` | **PROVEN** |
| `00B40000` walks from index 1, water, `[+424]=0` | **PROVEN** |
| `00B3EF40` destroys +0 current, +4 neighbour, releases +64 | **PROVEN** |
| `00B42750` mode 1 STB hit is `00B420F0` then return | **PROVEN** |
| `00B42530` is miss / mode-2 | **PROVEN** |
| `00B41E50` header + handle + `00BE03A0` / `00BDD0E0` / `00BDF010` | **PROVEN** |
| `00B3E820` refcounted current at +280/+284 | **PROVEN** |
| `009CCDC0` is name→index on bank+172 | **PROVEN** |
| `006C2170` unloads previous ContainsMaps | **DISPROVEN** for this body (load-only). Prior-region TNG teardown **UNREAD** |
| Neighbour TNG at New Game | **DISPROVEN** (`006C2170` is ContainsMap only) |
| Banks die on `00B40000` | **DISPROVEN** |
| Native GPU texture eviction policy | **UNREAD** |
| `00A26D40` / `009FF590` payload parse | **UNREAD** (vtbl+48 load) |
| `009A7CA0` / first `.big` path insert into manager+16 | **PARTIAL** |
| Host `TessellateVisible` of all opened maps | **PROVEN DIVERGE** (F) — not required by ownership |

---

## 9. Call tree (native ownership)

```
00402510 "Setup basic retail banks"
  009A76D0 [0x13CA79C]+20 = flag
  009A8150 × pairs → manager+24 names          // no file

004184BD "Init Graphics" 00416C8A
  009BE8x0 device FourCC probes
  009F83D0 "GBANK_MAIN"
    miss: 009FEA20 0x30C + 009FD4E0 scratch×11
    vtbl+4 009D56C0 → 009A7F80 / 009CFBC0      // textures.big dir
  004BBFC0 [0x13B8A08]

004A6E30 "Init Mesh Bank" 0049E620
  00A09F20 "MBANK_ALLMESHES"
    miss: 00A27030 0x460
    vtbl+4 009D56C0 → 009A7F80 / 009CFBC0      // graphics.big MESH dir
  world+60/+64 handle; +68=[bank+960]
  004BBFD0 [0x13B8A04]

006C2170 ContainsMap TNG + 004FC8A0
  00B428E0
    00B40000 close list[1..] + water           // maps only
    00BDA070 pool
    00B42750(1)
      00B40070 occupancy
      00B3E820 current handle
      009CCDC0 STB name
      hit  00B420F0 → 00B41E50 each slot       // header+patch
      miss 00B42530
    00B41FA0 water

thing construct / 004CA010
  009AD410(bank, def name) → handle            // no parse

draw / submit
  mesh   vtbl+52 00A243B0 → vtbl+48 00A26D40   // first id
  tex    vtbl+52 009FD910 → 009BE8B0           // first id
  land   00BDC2D0 AABB → 00BF4570 cells        // opened patches
```

---

## 10. Sources

- This pass: `fn --exact` / `vtbl` / `calls` listed above.
- `docs/runtime/FORWARD_TREE.md` §9, §14, §15.
- `docs/status/investigations/F-load-performance.md`.
- `docs/status/investigations/C-terrain-static-map.md`.
- Host: `MeshBank.cs`, `TextureLibrary.cs`, `LevelLibrary.cs`.
  `EngineLifecycle.cs` read only.
