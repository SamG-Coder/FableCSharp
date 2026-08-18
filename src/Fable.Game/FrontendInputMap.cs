using Fable.Formats.Defs;

namespace Fable.Game;

/// <summary>
/// Recovered <c>0042E3EE</c> type/action
/// → frontend message. DIK → 0xE5 /
/// 0x126 is UNREAD. Type-10
/// <c>0054E280</c> action 26 posts the
/// stored 0xE5; event type 4 is the
/// <c>0042E3EE</c> producer. Use
/// <see cref="Queue"/> when the poster
/// is unread.
/// </summary>
public sealed class FrontendInputMap
{
    public const uint InputPollFn = 0x0042E3EE;
    public const uint InputSingletonVa = 0x013B8388;
    public const uint EventTypeFn = 0x00A03B40;
    public const uint EventKeyFn = 0x00A03B70;
    public const int EventTypeOffset = 40;
    public const int EventKeyOffset = 0;
    public const uint ActionGetter = 0x0041E5F2;
    public const uint ActionApply = 0x0055CB10;
    public const uint InputVtbl = 0x01230134;
    public const int InputVtblMessage = 56;
    public const uint InputVtblMessageFn = 0x0041E6D3;
    public const uint Type10ActionFn = 0x0054E280;
    public const uint Type10InnerVtbl = 0x012497BC;
    public const uint AttachWriteE5 = 0x00598EE6;
    public const uint Type10StoreMsgFn = 0x0054E4F0;
    public const int Type10StoredMsgOffset = 352;
    public const uint GenericWidgetVtbl = 0x0122F5D4;
    public const uint Type10WidgetVtbl = 0x012497E4;
    public const int WidgetMessageVtbl = 284;
    public const uint WidgetMessageNoopFn = 0x0052F040;
    public const int EditBoxActionA = 33;
    public const int EditBoxActionB = 34;

    public const int TypeKey = 1;
    public const int Type4 = 4;
    public const int Type6 = 6;
    public const int Type7 = 7;
    public const int Type10 = 10;
    public const int TypeMouse = 13;
    public const int Type15 = 15;
    public const int TypeAnalog = 17;

    /// <summary>
    /// <c>0042E3EE</c> type 4 →
    /// <c>push 26</c>.
    /// </summary>
    public const int ActionType4 = 26;
    public const int ActionType10 = 27;
    public const int ActionType6 = 28;
    public const int ActionFromKey = 33;
    public const int ActionType15 = 34;
    public const int ActionType7 = 35;
    public const int ActionMouse = 25;

    /// <summary>
    /// Type-10 <c>0054E280</c> jump
    /// table <c>0x54E32C</c> index
    /// <c>00 01 03 03 03 03 03 02 02</c>
    /// for actions 26–34. Case 0 is
    /// <c>0054E2FA</c> → UI vtbl+32.
    /// </summary>
    public const uint Type10JumpTable = 0x0054E32C;
    public const uint Type10IndexTable = 0x0054E33C;
    public const uint Type10PostSite = 0x0054E2FA;

    /// <summary>
    /// Persist field copied by
    /// <c>0055B040</c> from def
    /// <c>+224</c>. Name UNREAD.
    /// </summary>
    public const uint MessageIdCrc = FrontendUiDef.MessageIdCrc;
    public const uint Type4RecordCtor = 0x00A03C80;
    public const uint Type4TranslateFn = 0x00AB5420;
    public const uint Type11ActionFn = 0x0054DBC0;
    public const uint Type38ActionFn = 0x0055AD60;
    public const uint PersistMessageCopyFn = 0x0055B040;
    public const int PersistMessageDefOffset = 224;
    public const int TypeButton = 11;
    public const int TypeAccept = 38;

    /// <summary>
    /// Type 4 is LMB down
    /// (<c>00A03C80</c>, device 3,
    /// <c>00AB5420</c> code 1). Not a
    /// DIK. Return (28) is type 1
    /// action 33.
    /// </summary>
    public const bool DikPosterUnread = false;
    public const int Type4Device = 3;

    private readonly Queue<int> _messages = new();

    public int PendingCount => _messages.Count;

    /// <summary>
    /// Host / test stand-in for an unread
    /// poster. Native 0x126 has no .text
    /// writer; 0xE5 user post is action 26
    /// not a recovered DIK.
    /// </summary>
    public void Queue(int msg) => _messages.Enqueue(msg);

    public bool TryDequeue(out int msg)
    {
        if (_messages.Count == 0)
        {
            msg = 0;
            return false;
        }

        msg = _messages.Dequeue();
        return true;
    }

    /// <summary>
    /// <c>0042E3EE</c> classify
    /// <c>00A03B40</c> type. Key is unused
    /// for type 4 (action 26 has no DIK
    /// compare).
    /// </summary>
    public static int? ActionFromEvent(int type, int key)
    {
        _ = key;
        return type switch
        {
            TypeKey => ActionFromKey,
            Type4 => ActionType4,
            Type6 => ActionType6,
            Type7 => ActionType7,
            Type10 => ActionType10,
            TypeMouse => ActionMouse,
            Type15 => ActionType15,
            _ => null,
        };
    }

    /// <summary>
    /// Type-10 inner <c>0054E280</c>
    /// action 26 posts widget+352
    /// (attach 0xE5). Type 11/38
    /// <c>0054DBC0</c>/<c>0055AD60</c>
    /// action 26 posts persist
    /// <see cref="MessageIdCrc"/>.
    /// Action 33 is not a frontend
    /// message.
    /// </summary>
    public static int? MessageFromAction(int action, string? screen)
    {
        _ = screen;
        if (action != ActionType4)
            return null;
        return null;
    }

    /// <summary>
    /// First visible stored id: type-10
    /// attach message, else type 11/38
    /// persist <c>+224</c>.
    /// </summary>
    public static int? MessageFromWidgets(
        int action, IReadOnlyList<FrontendWidget> widgets)
    {
        if (action != ActionType4)
            return null;
        ArgumentNullException.ThrowIfNull(widgets);
        foreach (var widget in widgets)
        {
            if (!widget.Visible || widget.Clip || widget.MessageId == 0)
                continue;
            if (widget.Type == FrontendWidgetType.Menu ||
                widget.Type == TypeButton ||
                widget.Type == TypeAccept)
                return widget.MessageId;
        }

        return null;
    }

    public static int? TryMapEvent(int type, int key, string? screen)
    {
        var action = ActionFromEvent(type, key);
        if (action is null)
            return null;
        return MessageFromAction(action.Value, screen);
    }

    public static int? TryMapEvent(
        int type, int key, IReadOnlyList<FrontendWidget> widgets)
    {
        var action = ActionFromEvent(type, key);
        if (action is null)
            return null;
        return MessageFromWidgets(action.Value, widgets);
    }
}
