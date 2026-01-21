using OoBDev.System.ExpressionCalculator.Evaluators;
using System;
using System.Collections.Generic;

namespace OoBDev.System.ExpressionCalculator.Expressions;

/// <summary>
/// Represents a literal numeric value expression in the expression calculator system.
/// This expression type encapsulates a constant numeric value of type T.
/// </summary>
/// <typeparam name="T">The numeric type of the value, which must be a value type implementing IComparable&lt;T&gt; and IEquatable&lt;T&gt;.</typeparam>
/// <param name="value">The constant numeric value of this expression.</param>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
public sealed class NumberExpression<T>(T value) : ExpressionBase<T>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    where T : struct, IComparable<T>, IEquatable<T>
{
    private static readonly IExpressionEvaluator<T> _evaluator = ExpressionEvaluatorFactory.Create<T>();

    /// <summary>
    /// Gets the constant numeric value of this expression.
    /// </summary>
    public T Value { get; } = value;

    /// <summary>
    /// Evaluates the expression by returning its constant value.
    /// </summary>
    /// <param name="variables">The variable dictionary (not used for constant values).</param>
    /// <returns>The constant value of this expression.</returns>
    public override T Evaluate(IDictionary<string, T> variables) => Value;

    /// <summary>
    /// Creates a copy of this number expression with the same value.
    /// </summary>
    /// <returns>A new NumberExpression instance with the same value.</returns>
    public override ExpressionBase<T> Clone() => new NumberExpression<T>(Value);

    /// <summary>
    /// Returns the string representation of the numeric value.
    /// </summary>
    /// <returns>The string representation of the value.</returns>
    public override string? ToString() => Value.ToString();

    /// <summary>
    /// Determines whether this expression is equal to another object.
    /// Supports comparison with other NumberExpression instances and raw T values.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public override bool Equals(object? obj) =>
        this == obj ||
        obj is NumberExpression<T> no && Value.Equals(no.Value) ||
        obj is T && Value.Equals(obj);

    /// <summary>
    /// A predefined expression representing the numeric value 1.
    /// </summary>
    public static readonly ExpressionBase<T> One = new NumberExpression<T>(_evaluator.GetValue(1));

    /// <summary>
    /// A predefined expression representing the numeric value 0.
    /// </summary>
    public static readonly ExpressionBase<T> Zero = new NumberExpression<T>(_evaluator.GetValue(0));

    /// <summary>
    /// A predefined expression representing the numeric value -1.
    /// </summary>
    public static readonly ExpressionBase<T> NegativeOne = new NumberExpression<T>(_evaluator.GetValue(-1));
}
