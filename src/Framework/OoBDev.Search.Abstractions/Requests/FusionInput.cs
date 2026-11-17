using System.Collections.Generic;

namespace OoBDev.Search.Requests;

/// <summary>
/// The ranked items one source returned, handed to an <see cref="IScoreFusion"/>.
/// </summary>
/// <param name="Source">The source name, for example <c>keyword</c> or <c>vector</c>.</param>
/// <param name="Items">The items, best first.</param>
public sealed record FusionInput(string Source, IReadOnlyList<SearchItem> Items);
