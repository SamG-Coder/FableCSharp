# MessageId is persist `+228`, not `+224`

Authority: `listing-00600000.txt` `00631C60` /
`00632500`; `0055B040`; FOREST sequential walk.

## Mapping

| Def | Helper | File CRC | `0055B040` |
|---|---|---|---|
| `+224` | `00632500` | `0x230364D6` | first copy, vtbl+284 |
| `+228` | `00632500` | `0x53C644E4` | second copy, vtbl+320 |

`0x53C644E4` payload is `0x126` on
`UI_ACCEPT_NEW_PROFILE` and `15` on
`UI_FRONTEND_BUTTON_NEW_GAME`. That CRC is
**not** `FableCrc("Message")` / `"MessageId"`.
Name **UNREAD**.

`+224` as MessageId is **DISPROVEN**.
Helper `00431102` for this pair is **DISPROVEN**.

Host: `MessageIdCrc` / `MessageIdDefOffset=228` /
`Plus224Crc` / `PersistTailDwordFn=0x00632500`.
