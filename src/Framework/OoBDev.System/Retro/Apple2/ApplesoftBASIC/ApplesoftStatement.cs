using System.Collections.Generic;

namespace OoBDev.System.Retro.Apple2.ApplesoftBASIC;

/// <summary>
/// One statement of a line (the text between colons).
/// </summary>
/// <param name="Elements">The keywords and literal runs in source order.</param>
public sealed record ApplesoftStatement(IReadOnlyList<ApplesoftElement> Elements);
