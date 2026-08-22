# S_QNOVI father/good-deed loop map

This file is a grep-first map of the retail block entered after the first
Oakvale cutscene. It records only mechanically recovered control flow. Names
such as `unknown_slot_1480` intentionally remain unknown until the callee body
proves a stronger meaning.

## Boundary and ownership

```text
PARENT_FUNCTION 00DB8680..00DB9794 S_QNOVI/NOVI_LiveFather watcher
CHILD_RETURN    00DB88FD CCutsceneDef CS_OAKVALE_INTRO_FATHER returned
FADE_TAIL       00DB8925 CGameScriptInterface+1504(1.0), 15 WorldFrames
CAMERA_RELEASE  00DB8935 +1668(0.0), 00DB8946 +1664()
INSTRUCTION     00DB89F7 +460 TEXT_QST_048_INSTRUCTION_HIGHLIGHTING_PC
EVENT_GATE      00DB8A1A +160/00894370, type=0x12 CHEERING, +28 yield
HUD_CREATE      00DB8A83 +1308 HUD_DEED_GOOD_ICON, quest+92=handle
HUD_ENABLE      00DB8AB8 +1284(handle,1)
LEASE_MODE_3    00DB8B00 CGameScriptInterface slot 8 / 0089B5B0
HERO_NAME_TEST  00DB8B6A SCRIPT_NAME_HERO through wrapper vtbl+108
LEASE_MODE_4    00DB8BBE CGameScriptInterface slot 8 / 0089B5B0
LOOP_START      00DB8C0C
LOOP_YIELD      00DB9720 context+28
LOOP_BACK       00DB9731 -> 00DB8B00
FUNCTION_EXIT   00DB974A or cleanup 00DB9785..00DB9794
```

Modes 3 and 4 belong to `CScriptGameResourceObjectScriptedThing` (RTTI for
vtable `0x0128D86C`). They are not game modes, player-control flags, or AI
actions.

## Loop state and branch inputs

```text
QUEST_FIELD +84   read at 00DB8C56; compared with watcher+28 at 00DB8D39
QUEST_FIELD +88   read at 00DB8C61, 00DB8D70; incremented by 00DAEA70
WATCHER_FIELD +28 tracks the consumed +84 count at 00DB8D51..00DB8D5E
SCRIPT_NAME_HERO  repeated predicate path starts at 00DB9499
CONTEXT +28       yield/pump used by dialogue handles and loop back
```

`00DAEA70` is not a generic state transition. It increments quest `+88`, calls
context slot `+624`, and on its first branch submits
`TEXT_QST_048_SCRMSG_DID_FIRST_BAD_DEED`, polls context slot `+160`, and writes
the corresponding quest/log state. Keep it out of engine lifecycle code.

## Repeated interface operations

```text
SLOT +280   returns an object used to construct dialogue/session handles
007E7390    forwards handle.inner vtbl+52 with 24 bytes of arguments
007E7450    forwards handle.inner vtbl+104; zero inner returns false
007E7490    forwards handle.inner vtbl+48 into a CString output
007E74D0    releases/refcounts the handle object
SLOT +504   consumes the delta between quest+84 and watcher+28
SLOT +508   returns a count compared with 3
SLOT +736   object/inventory predicate using OBJECT_CHOCOLATE_BOX_UNGIVEABLE
SLOT +1056  float query repeatedly compared with [0x122DEDC]
SLOT +1444  receives watcher+8 on reward branches
SLOT +1480  called with an empty CString and a temporary handle
SLOT +1516  called with 1 on entry and 0 on every cleanup path
SLOT +2396  called twice on the SCRIPT_NAME_HERO branch
```

The slot labels above are storage/call-shape facts, not semantic names.

## Retail text and reward branches

```text
00DB8CC9 TEXT_QST_048_DAD_DONE_NOTHING_YET
00DB8DDA TEXT_QST_048_DAD_GIVE_REWARD_JUST_GOOD
00DB8E95 TEXT_QST_048_DAD_GIVE_REWARD_PART_BAD
00DB8EEC OBJECT_CHOCOLATE_BOX_UNGIVEABLE
00DB8F86 TEXT_QST_048_DAD_GIVE_PRESENT
00DB9062 TEXT_QST_048_DAD_YOU_HAVE_ENOUGH
00DB9128 TEXT_QST_048_DAD_IS_ENOUGH
00DB9282 TEXT_QST_048_DAD_ANTISOCIAL
00DB932E TEXT_QST_048_DAD_DO_MORE
00DB9385 OBJECT_CHOCOLATE_BOX_UNGIVEABLE
00DB942E TEXT_QST_048_DAD_GIVE_PRESENT_ALT
00DB96BA TEXT_QST_048_DAD_TEMPER
```

Each dialogue handle is polled through `007E7450`; while it reports active,
the parent yields through context slot `+28`. Therefore these branches cannot
be represented by a global presentation queue or by fixed wall-clock delays.

## Managed comparison

```text
MATCH    child return, fade-tail WorldFrame wait, camera release
MATCH    instruction owner and timestamp-window query
PARTIAL  CHEERING event producer and journal expiry
MATCH    HUD handle create/enable order (graphics submission remains partial)
MATCH    scripted-Thing mode 3/name-test/mode 4 acquisition spine
PARTIAL  00DB8C0C..00DB972F dialogue/reward/inventory branch bodies
UNREAD   later PostAttack player-control owner
```

The managed player object is constructed with its action-ready flag before
this cutscene. No recovered instruction in this block flips a separate
"gameplay mode" flag. Do not add one merely to make the scene advance.
