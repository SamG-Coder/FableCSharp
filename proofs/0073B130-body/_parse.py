from pathlib import Path
from collections import Counter

p = Path(r"C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00700000.txt")
start, end = 0x0073B130, 0x0073CB40
e8 = []
strs = []
stores = []
imms_esp8 = []
imms_esp12 = []
imms_ecx = []
insns = []
in_fn = False
for line in p.read_text(encoding="utf-8", errors="replace").splitlines():
    if len(line) < 8:
        continue
    try:
        va = int(line[:8], 16)
    except ValueError:
        continue
    rest = line[10:] if len(line) > 10 else ""
    if va == start:
        in_fn = True
    if not in_fn:
        continue
    if va > end:
        break
    insns.append((va, rest.strip()))
    if "call " in rest:
        dest = rest.split("call ", 1)[1].strip().split()[0]
        e8.append((va, dest))
    if 'push "' in rest:
        strs.append((va, rest.strip()))
    if rest.strip().startswith("mov ["):
        stores.append((va, rest.strip()))
    s = rest.strip()
    if s.startswith("mov [esp+8],"):
        imms_esp8.append((va, s.split(",", 1)[1].strip()))
    if s.startswith("mov [esp+12],"):
        imms_esp12.append((va, s.split(",", 1)[1].strip()))
    if s.startswith("mov ecx, 0x") or s.startswith("mov ecx, 0X"):
        imms_ecx.append((va, s.split(",", 1)[1].strip()))

out = Path(r"C:\FableCSharp\proofs\0073B130-body\_parse.txt")
lines = []
lines.append(f"insn_count {len(insns)}")
lines.append(f"first {hex(insns[0][0])} {insns[0][1]}")
lines.append(f"last {hex(insns[-1][0])} {insns[-1][1]}")
lines.append(f"byte_span {insns[-1][0]-insns[0][0]+1}")
lines.append(f"E8 count {len(e8)}")
c = Counter(d for _, d in e8)
lines.append(f"E8 dests {dict(c)}")
lines.append(f"first E8 {hex(e8[0][0])} {e8[0][1]}")
lines.append(f"last E8 {hex(e8[-1][0])} {e8[-1][1]}")
lines.append(f"strings {strs}")
lines.append(f"esp8 count {len(imms_esp8)}")
lines.append(f"esp12 count {len(imms_esp12)}")
lines.append(f"ecx imm count {len(imms_ecx)}")

# reconstruct pairs: each block sets id then fn
# Pattern A (inlined): mov ecx, FN; ... mov [esp+8], ID; mov [esp+12], ecx
# Pattern B (00743B30): mov [esp+12], ID; mov [esp+16], FN  -- wait listing uses [esp+12]/[esp+16] before call
# I'll walk insns in order and pick ID from [esp+8] or [esp+12] just before a write/call

pairs = []
pending_fn = None
pending_id = None
pending_id_kind = None
for va, s in insns:
    if s.startswith("mov ecx, 0x"):
        pending_fn = s.split(",", 1)[1].strip()
    if s.startswith("mov [esp+8],"):
        pending_id = s.split(",", 1)[1].strip()
        pending_id_kind = "esp8"
    if s.startswith("mov [esp+12],") and "0x" in s and "ecx" not in s:
        # could be id (00743B30 path uses [esp+12] as id after lea/push)
        val = s.split(",", 1)[1].strip()
        if val.startswith("0x") or val == "ebx":
            # distinguish fn vs id: if previous was lea/push this is id for 00743B30
            pending_id = val
            pending_id_kind = "esp12"
    if s.startswith("mov [esp+16],"):
        pending_fn = s.split(",", 1)[1].strip()
    if s.startswith("call 00743270") or s.startswith("call 00743B30") or s.startswith("mov [eax],"):
        if pending_id is not None or pending_fn is not None:
            pairs.append((hex(va), pending_id, pending_fn, s[:24], pending_id_kind))
            # don't clear fn for inlined path that uses ecx across

lines.append(f"pair events {len(pairs)}")
for row in pairs:
    lines.append(f"{row[0]}\t{row[1]}\t{row[2]}\t{row[4]}\t{row[3]}")

# unique stores to globals
glob_stores = [s for s in stores if '"' in s[1] or "0x13" in s[1]]
lines.append("GLOBAL_STORES")
for va, s in glob_stores:
    lines.append(f"{hex(va)} {s}")

out.write_text("\n".join(lines), encoding="utf-8")
print(f"wrote {out} lines {len(lines)}")
print("E8", dict(c))
print("insn", len(insns), "pairs", len(pairs))
