using Silk.NET.Windowing;

namespace Fable.Client;

/// <summary>
/// Host OS window. Default is windowed.
/// Alt+Enter toggles fullscreen like
/// most Steam games. Native
/// <c>DeviceWindowed</c> is D3D only.
/// </summary>
public static class SilkHostWindow
{
    public static WindowState DefaultState => WindowState.Normal;

    public static bool AltEnterPressed(
        bool altDown, bool enterDown, bool enterWasDown) =>
        altDown && enterDown && !enterWasDown;

    public static WindowState ToggleFullscreen(WindowState current) =>
        current == WindowState.Fullscreen
            ? WindowState.Normal
            : WindowState.Fullscreen;
}
