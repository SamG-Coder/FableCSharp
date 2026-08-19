namespace Fable.Dx9;

/// <summary>
/// <c>D3DPRIMITIVETYPE</c>. Frontend
/// <c>009DA9F0</c> uses 2 (list) or 4
/// (strip). First-seen queue empty:
/// no Draw.
/// </summary>
public enum Dx9PrimitiveType : int
{
    PointList = 1,
    LineList = 2,
    LineStrip = 3,
    TriangleList = 4,
    TriangleStrip = 5,
    TriangleFan = 6,
}
