namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a relational operation type in JSON Path filter expressions.
/// Used to indicate comparison operators like equals, greater than, less than, etc.
/// </summary>
/// <param name="type">The type of relational operation.</param>
public sealed class RelationalOperationTypePathSegment(RelationalOperationTypes type) : BaseValuePathSegment<RelationalOperationTypes>(type)
{
}
