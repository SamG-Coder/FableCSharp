# Subject: verify open GitHub issues vs HEAD

One explore subagent per open issue.
Index: https://github.com/SamG-Coder/FableCSharp/issues

HEAD at verify: `ee08490` (later `3a7b594`
adds `CInventoryItemDef` only).
Closed issues (#6, #8, #11, #13, #18, #37)
are out of this subject.

| # | Title | Status vs HEAD | Proof |
| --- | --- | --- | --- |
| 42 | `0044C72B` as `[01232C24+8]` without rdata dword | **STILL OPEN** | `proofs/issue-42-verify` |
| 36 | Frontend invents dest; claims `009DB700` DIP | **PARTIAL** | `proofs/issue-36-verify` |
| 20 | PlayAVI still runs 3D Draw | **PARTIAL** | `proofs/issue-20-verify` |
| 19 | TryWalk / F2 / Title frozen `gameCam` | **FIXED** (close) | `proofs/issue-19-verify` |
| 17 | Init GUI `0043A380` Note-only | **PARTIAL** | `proofs/issue-17-verify` |
| 15 | Init Sound / PlayMusic record-only | **STILL OPEN** | `proofs/issue-15-verify` |
| 14 | Frontend draw Note-only; New Game N/Enter | **PARTIAL** | `proofs/issue-14-verify` |
| 12 | `EnterRegion` invents 64,-40,95 | **FIXED** (close) | `proofs/issue-12-verify` |
| 9 | `WmvPlayer` never QIs `IBasicAudio` | **STILL OPEN** | `proofs/issue-9-verify` |
| 5 | Tests write to hard-coded grok-goal path | **STILL OPEN** | `proofs/issue-5-verify` |
| 4 | First-scene ledgers still say Oakvale | **STILL OPEN** | `proofs/issue-4-verify` |

Do not close an issue from a Note-only host
stand-in. Close #19 and #12 only: the filed
client paths are gone.

## Next work (do not invent)

1. #42 — quote rdata dword `[01232C24+8]` or demote dest to PARTIAL.
2. #5 — delete `grok-goal-*` writes from tests.
3. #9 — QI `PlayAviBasicAudioIid` after `RenderFile`.
4. #15 — stop treating Init Sound as applied, or host the proven register only.
5. #4 — retitle FIRST_SCENE ledgers: no-save Lookout vs intro view.
6. #36 / #14 — stop claiming `009DA9F0` DIP; keep type-4/`0xE5`.
7. #20 — skip 3D `Draw` while `006286F0` owns the pump.
8. #17 — do not set `PlayerGuiReady` until a dest exists.
