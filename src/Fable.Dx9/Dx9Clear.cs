namespace Fable.Dx9;

/// <summary>
/// <c>D3DCLEAR_*</c>. <c>009D8CF0</c>
/// <c>test ebx; jne; mov ebx, 7</c>
/// when the caller's flags arg is 0.
/// </summary>
[Flags]
public enum Dx9Clear : int
{
    Target = 1,
    ZBuffer = 2,
    Stencil = 4,
    /// <summary>
    /// Native default when
    /// <c>0042E063 push ebx</c> is 0.
    /// </summary>
    WhenArgZero = Target | ZBuffer | Stencil,
}
