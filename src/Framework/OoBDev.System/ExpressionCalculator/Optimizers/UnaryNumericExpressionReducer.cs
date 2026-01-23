using OoBDev.System.ExpressionCalculator.Expressions;
using System;

namespace OoBDev.System.ExpressionCalculator.Optimizers;

/// <summary>
/// Optimizes expressions by evaluating unary operations on numeric constants.
/// Converts expressions like -(5) or !(3) into their evaluated numeric results.
/// Also simplifies double negation (--x = x) and removes unnecessary parentheses around simple values.
/// </summary>
/// <typeparam name="T">The numeric type of the expression, which must be a value type implementing IComparable&lt;T&gt; and IEquatable&lt;T&gt;.</typeparam>
public sealed class UnaryNumericExpressionReducer<T> : IExpressionOptimizer<T> where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Optimizes the given expression by evaluating unary operations on constants.
    /// </summary>
    /// <param name="expression">The expression to optimize.</param>
    /// <returns>An optimized expression with evaluated unary operations.</returns>
    public ExpressionBase<T> Optimize(ExpressionBase<T> expression) =>
        expression switch
        {
            InnerExpression<T> inner => Optimize(inner),
            BinaryOperatorExpression<T> binaryOperator => new BinaryOperatorExpression<T>(
                Optimize(binaryOperator.Left),
                binaryOperator.Operator,
                Optimize(binaryOperator.Right)
                ),
            UnaryOperatorExpression<T> unary => Optimize(unary),

            _ => expression
        };

    /// <summary>
    /// Optimizes an inner (parenthesized) expression by removing unnecessary parentheses around simple values.
    /// </summary>
    /// <param name="expression">The inner expression to optimize.</param>
    /// <returns>The simplified expression, without parentheses if wrapping a number or variable.</returns>
    public ExpressionBase<T> Optimize(InnerExpression<T> expression) =>
        expression.Expression switch
        {
            NumberExpression<T> number => number,
            VariableExpression<T> variable => variable,
            _ => new InnerExpression<T>(Optimize(expression.Expression)),
        };

    /// <summary>
    /// Optimizes a unary operator expression by evaluating operations on constants or simplifying nested unary operations.
    /// </summary>
    /// <param name="expression">The unary operator expression to optimize.</param>
    /// <returns>The optimized expression, with constants evaluated or nested operations simplified.</returns>
    public ExpressionBase<T> Optimize(UnaryOperatorExpression<T> expression)
    {
        var operand = Optimize(expression.Operand);
        return operand switch
        {
            NumberExpression<T> _ => new NumberExpression<T>(expression.Evaluate(ExpressionBaseExtensions.EmptySet<T>())),
            UnaryOperatorExpression<T> unaryOperator => Reduce(expression, unaryOperator),
            InnerExpression<T> _ => new UnaryOperatorExpression<T>(expression.Operator, Optimize(operand)),
            BinaryOperatorExpression<T> _ => new UnaryOperatorExpression<T>(expression.Operator, Optimize(operand)),
            _ => new UnaryOperatorExpression<T>(expression.Operator, operand)
        };
    }

    private ExpressionBase<T> Reduce(UnaryOperatorExpression<T> expression, UnaryOperatorExpression<T> unaryOperator)
    {
        var unary = Optimize(unaryOperator.Operand);
        return unaryOperator.Operator == UnaryOperators.Negate && unaryOperator.Operator == UnaryOperators.Negate
            ? new InnerExpression<T>(unary)
            : new UnaryOperatorExpression<T>(expression.Operator,
                new UnaryOperatorExpression<T>(unaryOperator.Operator,
                    unary
                )
            );
    }
}
