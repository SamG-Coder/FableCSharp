namespace Fable.Dx9;

/// <summary>
/// <c>D3DVIEWPORT9</c>. Native
/// <c>009BEF80</c> MinZ 0 MaxZ 1.
/// </summary>
public readonly record struct Dx9Viewport(
    int X,
    int Y,
    int Width,
    int Height,
    float MinZ,
    float MaxZ);
