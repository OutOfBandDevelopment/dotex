using System.Linq.Expressions;

namespace OoBDev.System.Linq.Expressions;

/// <summary>
/// Represents an interface for a visitor that processes an expression tree after it has been built.
/// </summary>
public interface IPostBuildExpressionVisitor
{
    /// <summary>
    /// Visits the provided expression node after the expression tree has been built.
    /// </summary>
    /// <param name="node">The expression node to visit.</param>
    /// <returns>The modified expression node.</returns>
    Expression? Visit(Expression? node);
}
