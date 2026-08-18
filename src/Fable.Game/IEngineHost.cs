namespace Fable.Game;

/// <summary>
/// Window / input / Present surface the
/// PE header implements. The engine
/// owns modes, load, AVI, camera, and
/// world submit. The host does not
/// decide New Game, region, or expand.
/// </summary>
public interface IEngineHost
{
    int Width { get; }
    int Height { get; }
    string Title { get; set; }

    /// <summary>
    /// <c>009BEEB0</c> Present. Engine
    /// already chose camera / world / AVI.
    /// </summary>
    void Present(EngineFrame frame);

    void Quit();
}

/// <summary>
/// One frame the engine asks the host
/// to Present. Null world is frontend
/// or loading (clear only).
/// </summary>
public readonly record struct EngineFrame(
    ScriptedCamera Camera,
    WorldGeometry? World,
    int AviWidth,
    int AviHeight,
    byte[]? AviRgba,
    int AviSerial,
    bool AviPlaying,
    byte FadeAlpha,
    byte FadeR,
    byte FadeG,
    byte FadeB,
    Fable.Render.MeshVertex[]? Vertices = null,
    Fable.Render.MeshDraw[]? Draws = null,
    Fable.Render.GpuTexture[]? Textures = null);
