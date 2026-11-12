using System.Collections.Generic;

namespace OoBDev.System.Retro.Apple2.ApplesoftBASIC;

/// <summary>
/// One numbered line of an Applesoft program.
/// </summary>
/// <param name="Number">The line number, 0 to 63999.</param>
/// <param name="Statements">The statements separated by colons.</param>
public sealed record ApplesoftLine(int Number, IReadOnlyList<ApplesoftStatement> Statements);
