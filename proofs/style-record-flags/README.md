# Style persist `00631C60` / `00632E00` vs `0052C7E0` dword0

Authority: `listing-00600000.txt` (`00631C60`
`00632E00` `00625350`); `listing-00400000.txt`
(`00433FE0`); `listing-00500000.txt`
(`0052C7E0` `0052CEB0`).

| Claim | Class |
| --- | --- |
| `00631CF6` persist vector `CUIDef+64` via `00632E00` | **PROVEN** |
| Style stride 124 | **PROVEN** `00632E5E` / `006253BB` |
| `00433FE0` persist starts at style `+60` (f32s) | **PROVEN** |
| `0052CEB0` returns map node `+20` | **PROVEN** |
| `0052C7E0` `test [eax], 0x10/0x20/0x40` | **PROVEN** |
| Persist CRC / file value of that dword0 / `+20` | **UNREAD** |
| Apply style-6 flags on leftover slots | **not shipped** |

`0x56A59976` still ends the sequential first-style
prefix. Extra `States` records are not walked.
Do not invent dword0 from screenshots.
