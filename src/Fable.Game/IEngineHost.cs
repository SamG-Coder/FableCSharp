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
    Fable.Render.GpuTexture[]? Textures = null,
    Fable.Render.MeshVertex[]? ObjectVertices = null,
    Fable.Render.MeshDraw[]? ObjectDraws = null,
    ushort[]? Indices = null,
    uint AviClearArgb = 0xFF000000,
    byte[]? FrontendRgba = null,
    int FrontendWidth = 0,
    int FrontendHeight = 0,
    float PresentX0 = 0,
    float PresentY0 = 0,
    float PresentX1 = 1,
    float PresentY1 = 1,
    Fable.Render.FrontendSubmitBatch? FrontendBatch = null);

public readonly record struct FrontendWidget(
    string Name,
    int Type,
    float DestX0,
    float DestY0,
    float DestX1,
    float DestY1,
    string? TextTag,
    string? Text,
    string? ParentName = null,
    string? TextureName = null,
    int GraphicId = 0,
    float PersistWidth = 0,
    float PersistHeight = 0,
    float PersistX = 0,
    float PersistY = 0,
    float PersistScaleX = 1,
    float PersistScaleY = 1,
    bool Center = false,
    bool Absolute = false,
    bool ScaleOriginToViewport = false,
    bool ScaleSizeToViewport = false,
    bool Visible = true,
    bool Enabled = true,
    bool Clip = false,
    int ActiveChild = 0,
    int Font = 0,
    string? FontFace = null,
    float U0 = 0,
    float V0 = 0,
    float U1 = 0,
    float V1 = 0,
    uint Colour = 0xFFFFFFFFu,
    int GlyphCount = 0,
    int DrawOrder = 0);
