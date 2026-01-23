namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a relational comparison operation between two operands in JSON Path filter expressions.
/// Used for filter conditions like [?(@.price > 10)] or [?(@.name == "test")].
/// </summary>
/// <param name="left">The left operand of the comparison.</param>
/// <param name="operator">The relational operator (e.g., equals, greater than, less than).</param>
/// <param name="right">The right operand of the comparison.</param>
public class RelationBinaryOperationPathSegment(
    IPathSegment left,
    IPathSegment<RelationalOperationTypes> @operator,
    IPathSegment right
        ) : BinaryOperationPathSegment<RelationalOperationTypes>(left, @operator, right)
{
}