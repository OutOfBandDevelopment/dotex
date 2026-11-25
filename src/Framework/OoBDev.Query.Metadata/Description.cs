using System.Collections.Generic;

namespace OoBDev.Query.Metadata;

/// <summary>What a client may know about the registered kinds.</summary>
/// <param name="Kinds">The kinds.</param>
public sealed record Description(IReadOnlyList<KindDescription> Kinds);
