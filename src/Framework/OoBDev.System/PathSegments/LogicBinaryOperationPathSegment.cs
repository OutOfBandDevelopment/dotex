namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a binary logical operation path segment for JSON Path filter expressions (e.g., AND, OR).
/// </summary>
/// <param name="left">The left operand of the logical operation.</param>
/// <param name="operator">The logical operator (AND or OR) to apply.</param>
/// <param name="right">The right operand of the logical operation.</param>
public class LogicBinaryOperationPathSegment(
    IPathSegment left,
    IPathSegment<LogicOperationTypes> @operator,
    IPathSegment right
        ) : BinaryOperationPathSegment<LogicOperationTypes>(left, @operator, right)
{
}