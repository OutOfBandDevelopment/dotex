using System.Collections.Generic;

namespace OoBDev.Query.Metadata;

/// <summary>
/// A search string split into explicit operators and the plain words that remain.
/// </summary>
/// <param name="Operators">The <c>name:value</c> terms.</param>
/// <param name="Words">The plain words.</param>
public sealed record ParsedSearch(IReadOnlyList<SearchOperator> Operators, IReadOnlyList<string> Words);
