using OoBDev.System.ExpressionCalculator.Expressions;
using System;
using static OoBDev.System.ExpressionCalculator.Expressions.BinaryOperators;
using static OoBDev.System.ExpressionCalculator.Expressions.UnaryOperators;

namespace OoBDev.System.ExpressionCalculator.Optimizers;

/// <summary>
/// Optimizes expressions by applying identity operation simplifications.
/// Handles algebraic identities like B^1=B, B*1=B, B+0=B, B-0=B, B/1=B, B*-1=-B, etc.
/// </summary>
/// <typeparam name="T">The numeric type of the expression, which must be a value type implementing IComparable&lt;T&gt; and IEquatable&lt;T&gt;.</typeparam>
public sealed class IdentityExpressionOptimizer<T> : IExpressionOptimizer<T> where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Optimizes the given expression by applying identity operation simplifications.
    /// </summary>
    /// <param name="expression">The expression to optimize.</param>
    /// <returns>An optimized expression with simplified identity operations.</returns>
    public ExpressionBase<T> Optimize(ExpressionBase<T> expression) =>
        expression switch
        {
            InnerExpression<T> inner => new InnerExpression<T>(Optimize(inner.Expression)),
            BinaryOperatorExpression<T> binaryOperator => Optimize(binaryOperator),
            _ => expression
        };

    // simplify identity
    // B^1 => B
    // 1*B | B*1 => B
    // B*-1=>-B
    // -1*B=>-B
    // B/1 => B
    // B/-1=>-B
    // 0+B | B+0 => B
    // B-0 => B
    // 0-B=>-B
    // -(1) => -1
    private ExpressionBase<T> Optimize(BinaryOperatorExpression<T> expression)
    {
        var left = Optimize(expression.Left);
        var right = Optimize(expression.Right);

        ExpressionBase<T> result = (expression.Operator, GetValue(left), GetValue(right)) switch
        {
            (Power, _, 1) => left,

            (Multiply, 1, _) => right,
            (Multiply, _, 1) => left,
            (Multiply, -1, _) => new UnaryOperatorExpression<T>(Negate, right),
            (Multiply, _, -1) => new UnaryOperatorExpression<T>(Negate, left),

            (Divide, _, 1) => left,
            (Divide, _, -1) => new UnaryOperatorExpression<T>(Negate, left),

            (Add, _, 0) => left,
            (Add, 0, _) => right,

            (Subtract, _, 0) => left,
            (Subtract, 0, _) => new UnaryOperatorExpression<T>(Negate, right),

            _ => new BinaryOperatorExpression<T>(left, expression.Operator, right)
        };

        return result;
    }

    private int? GetValue(ExpressionBase<T> expression) =>
        expression switch
        {
            NumberExpression<T> num => Convert.ToInt32(num.Value),
            UnaryOperatorExpression<T> unaryOp => 0 - GetValue(unaryOp.Operand),
            _ => null
        };
}
