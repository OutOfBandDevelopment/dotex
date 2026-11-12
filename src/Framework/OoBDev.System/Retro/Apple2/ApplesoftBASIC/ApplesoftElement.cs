namespace OoBDev.System.Retro.Apple2.ApplesoftBASIC;

/// <summary>
/// One piece of a statement: a keyword token or a run of literal text.
/// </summary>
/// <param name="Text">The keyword as typed (for example <c>PRINT</c>) or the literal text (names, numbers, quoted strings, REM and DATA bodies).</param>
/// <param name="TokenCode">The token byte for a keyword; <see langword="null"/> for literal text.</param>
public sealed record ApplesoftElement(string Text, byte? TokenCode = null)
{
    /// <summary>Gets a value indicating whether this element is a keyword token.</summary>
    public bool IsKeyword => TokenCode.HasValue;
}
