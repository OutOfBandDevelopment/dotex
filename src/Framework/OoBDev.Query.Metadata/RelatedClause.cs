namespace OoBDev.Query.Metadata;

/// <summary>Rows related to an anchor row through a named relation of the kind.</summary>
/// <param name="Via">The relation name.</param>
/// <param name="Of">The anchor row's key.</param>
public sealed record RelatedClause(string Via, string Of);
