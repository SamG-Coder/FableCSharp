namespace Fable.Game;

/// <summary>
/// Per-submission-unit Present owner.
/// Compatibility: existing renderer draws.
/// Shadow: existing renderer draws; virtual
/// DX9 records. NativeSemantic: virtual DX9
/// draws that unit; compatibility is off.
/// </summary>
public enum Dx9SubmitMode
{
    Compatibility,
    Shadow,
    NativeSemantic,
}

/// <summary>
/// NativeSemantic only when the device
/// can actually submit that unit. Default
/// is every flag false (unproven).
/// </summary>
public readonly struct Dx9SubmitCapabilities
{
    public bool CanRenderFrontendSprites { get; init; }
    public bool CanRenderFrontendGlyphs { get; init; }
    public bool CanRenderFade { get; init; }
    public bool CanRenderLandscape { get; init; }
    public bool CanRenderStaticMeshes { get; init; }
    public bool CanRenderPalskin { get; init; }
    public bool CanRenderSky { get; init; }
    public bool CanRenderWater { get; init; }
    public bool CanRenderParticles { get; init; }
    public bool CanRenderHud { get; init; }
    public bool CanRenderVideo { get; init; }

    public static Dx9SubmitCapabilities None => default;

    public Dx9SubmitMode FrontendMode(bool deviceAttached)
    {
        if (CanRenderFrontendSprites && CanRenderFrontendGlyphs)
            return Dx9SubmitMode.NativeSemantic;
        return deviceAttached ? Dx9SubmitMode.Shadow : Dx9SubmitMode.Compatibility;
    }
}
