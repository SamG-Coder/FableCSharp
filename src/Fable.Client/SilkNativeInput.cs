using Fable.Game;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace Fable.Client;

/// <summary>
/// Silk keys/mouse → native-shaped
/// <see cref="EngineInput"/> records.
/// Does not post frontend messages or
/// New Game.
/// </summary>
public static class SilkNativeInput
{
    /// <summary>
    /// WM_CHAR analog: type 15 / action 34.
    /// Not a DIK→char table.
    /// </summary>
    public static void QueueChar(EngineLifecycle life, char ch)
    {
        if (ch == 0)
            return;
        life.QueueInput(EngineInput.Type15, ch);
    }

    public static void QueueKeys(
        EngineLifecycle life, IKeyboard keyboard, bool skipEnter = false)
    {
        if (keyboard.IsKeyPressed(Key.Escape))
            life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipEscape);
        if (keyboard.IsKeyPressed(Key.Space))
            life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipSpace);
        if (!skipEnter && keyboard.IsKeyPressed(Key.Enter))
            life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipReturn);
        if (keyboard.IsKeyPressed(Key.F4))
            life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipF4);
        if (keyboard.IsKeyPressed(Key.A))
            life.QueueInput(EngineInput.TypeKey, EngineInput.KeyDikA);
        if (keyboard.IsKeyPressed(Key.B))
            life.QueueInput(EngineInput.TypeKey, EngineInput.KeyDikB);
    }

    public static void QueuePointer(
        EngineLifecycle life,
        Vector2D<float> pos,
        bool moved,
        bool lmbDown,
        bool lmbWasDown,
        int windowW,
        int windowH)
    {
        SetPointer(life, pos, windowW, windowH);
        if (moved)
            life.QueueInput(EngineInput.TypeMouse, 0);
        if (lmbDown && !lmbWasDown)
            life.QueueInput(EngineInput.Type4, 0);
        if (!lmbDown && lmbWasDown)
            life.QueueInput(EngineInput.Type6, 0);
    }

    /// <summary>
    /// Queue an actual Silk mouse-button callback. Polling alone can miss a
    /// complete click when down and up both occur between two update frames.
    /// </summary>
    public static void QueuePointerButton(
        EngineLifecycle life,
        Vector2D<float> pos,
        bool down,
        int windowW,
        int windowH)
    {
        SetPointer(life, pos, windowW, windowH);
        life.QueueInput(down ? EngineInput.Type4 : EngineInput.Type6, 0);
    }

    private static void SetPointer(
        EngineLifecycle life,
        Vector2D<float> pos,
        int windowW,
        int windowH)
    {
        var destW = life.BackBufferWidth > 0 ? life.BackBufferWidth : 1024;
        var destH = life.BackBufferHeight > 0 ? life.BackBufferHeight : 768;
        var srcW = Math.Max(1, windowW);
        var srcH = Math.Max(1, windowH);
        if (srcW == destW && srcH == destH)
            life.SetFrontendPointer(pos.X, pos.Y);
        else
            life.SetFrontendPointer(pos.X / srcW * destW, pos.Y / srcH * destH);
    }
}
