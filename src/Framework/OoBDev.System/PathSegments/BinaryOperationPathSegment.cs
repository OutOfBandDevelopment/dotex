namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a binary operation in a path segment that operates on two operands.
/// </summary>
/// <typeparam name="T">The type of the value in the path segment.</typeparam>
public abstract class BinaryOperationPathSegment<T> : BinaryPathSegment
{
    /// <summary>
    /// Gets the operator used in the binary operation.
    /// </summary>
    public IPathSegment<T> Operator { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryOperationPathSegment{T}"/> class.
    /// </summary>
    /// <param name="left">The left operand of the binary operation.</param>
    /// <param name="operator">The operator to apply to the operands.</param>
    /// <param name="right">The right operand of the binary operation.</param>
    protected BinaryOperationPathSegment(
        IPathSegment left,
        IPathSegment<T> @operator,
        IPathSegment right
        ) : base(left, right)
    {
        Operator = @operator;
    }

    /// <summary>
    /// Returns a string representation of the binary operation in the format "LeftOperand Operator RightOperand".
    /// </summary>
    /// <returns>A string that represents the binary operation.</returns>
    public override string ToString() => $"{Left} {Operator} {Right}";
}
