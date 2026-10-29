using System.Collections.Generic;

namespace OoBDev.DerEE.De5000;

/// <summary>
/// What one packet means: a line of text, plus named values for indicators.
/// </summary>
/// <param name="Text">The readable line.</param>
/// <param name="Values">The named values (<c>primary</c>, <c>secondary</c>, <c>frequency</c> and the mode flags as <c>1</c> or <c>0</c>).</param>
public sealed record De5000Reading(string Text, IReadOnlyDictionary<string, string> Values);
