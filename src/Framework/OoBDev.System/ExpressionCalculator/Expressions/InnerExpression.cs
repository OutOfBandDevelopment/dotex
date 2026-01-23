using System;
using System.Collections.Generic;

namespace OoBDev.System.ExpressionCalculator.Expressions;

/// <summary>
/// Represents a parenthesized expression that wraps another expression to control evaluation order.
/// This expression type is used to represent explicit grouping with parentheses in mathematical expressions.
/// </summary>
/// <typeparam name="T">The numeric type of the expression, which must be a value type implementing IComparable&lt;T&gt; and IEquatable&lt;T&gt;.</typeparam>
/// <param name="expression">The inner expression to wrap with parentheses.</param>
public sealed class InnerExpression<T>(ExpressionBase<T> expression) : ExpressionBase<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Gets the wrapped inner expression.
    /// </summary>
    public ExpressionBase<T> Expression { get; } = expression;

    /// <summary>
    /// Creates a copy of this inner expression with a cloned inner expression.
    /// </summary>
    /// <returns>A new InnerExpression instance with a cloned inner expression.</returns>
    public override ExpressionBase<T> Clone() => new InnerExpression<T>(Expression.Clone());

    /// <summary>
    /// Evaluates the inner expression using the provided variables.
    /// </summary>
    /// <param name="variables">The dictionary of variable names to values for evaluation.</param>
    /// <returns>The result of evaluating the inner expression.</returns>
    public override T Evaluate(IDictionary<string, T> variables) => Expression.Evaluate(variables);

    /// <summary>
    /// Returns the string representation of this expression with parentheses around the inner expression.
    /// </summary>
    /// <returns>A string in the format "(inner_expression)".</returns>
    public override string ToString() => $"({Expression})";
}
