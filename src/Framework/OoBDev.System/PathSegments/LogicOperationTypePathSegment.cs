namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a path segment containing a logical operation type value (AND or OR).
/// </summary>
/// <param name="type">The logical operation type.</param>
public sealed class LogicOperationTypePathSegment(LogicOperationTypes type) : BaseValuePathSegment<LogicOperationTypes>(type)
{
}
