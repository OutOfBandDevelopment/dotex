using OoBDev.System.ExpressionCalculator.Expressions;
using System;
using static OoBDev.System.ExpressionCalculator.Expressions.BinaryOperators;

namespace OoBDev.System.ExpressionCalculator.Optimizers;

/// <summary>
/// Optimizes expressions by simplifying operations with determined values (constants).
/// Handles algebraic identities like B/B=1, B%B=0, B^0=1, 0^B=0, B*0=0, etc.
/// Also pre-evaluates expressions where both operands are numeric constants.
/// </summary>
/// <typeparam name="T">The numeric type of the expression, which must be a value type implementing IComparable&lt;T&gt; and IEquatable&lt;T&gt;.</typeparam>
public sealed class DeterminedExpressionReducer<T> : IExpressionOptimizer<T> where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Optimizes the given expression by reducing operations with determined constant values.
    /// </summary>
    /// <param name="expression">The expression to optimize.</param>
    /// <returns>An optimized expression with simplified constant operations.</returns>
    public ExpressionBase<T> Optimize(ExpressionBase<T> expression) =>
        expression switch
        {
            InnerExpression<T> inner => new InnerExpression<T>(Optimize(inner.Expression)),
            BinaryOperatorExpression<T> binaryOperator => Optimize(binaryOperator),
            _ => expression
        };

    /// simplify determined
    // #`?#`` => #```
    // B/B => 1
    // B%B => 0
    // B^0 => 1
    // 0^B => 0
    // B*0 | 0*B => 0
    // B/0 => exception!
    // 0/B => 0
    // B%0 => exception!
    // 0%B => 0
    // B%1 => 0
    // B%-1 => 0	
    private ExpressionBase<T> Optimize(BinaryOperatorExpression<T> expression)
    {
        var left = Optimize(expression.Left);
        var right = Optimize(expression.Right);

        if (left is NumberExpression<T> && right is NumberExpression<T>)
        {
            return new NumberExpression<T>(
                new BinaryOperatorExpression<T>(left, expression.Operator, right).Evaluate(
                    ExpressionBaseExtensions.EmptySet<T>()
                    )
                );
        }

        ExpressionBase<T> result = (expression.Operator, GetValue(left), GetValue(right)) switch
        {
            (Power, _, 0) => NumberExpression<T>.One,
            (Power, 0, _) => NumberExpression<T>.Zero,

            (Multiply, 0, _) => NumberExpression<T>.Zero,
            (Multiply, _, 0) => NumberExpression<T>.Zero,

            (Divide, _, 0) => throw new DivideByZeroException(),
            (Divide, 0, _) => NumberExpression<T>.Zero,
            (Divide, _, _) when left.Equals(right) => NumberExpression<T>.One,

            (Modulo, _, 0) => throw new DivideByZeroException(),
            (Modulo, 0, _) => NumberExpression<T>.Zero,
            (Modulo, _, 1) => NumberExpression<T>.Zero,
            (Modulo, _, -1) => NumberExpression<T>.Zero,
            (Modulo, _, _) when left.Equals(right) => NumberExpression<T>.Zero,

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
