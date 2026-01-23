using OoBDev.System.ExpressionCalculator.Expressions;
using System;

namespace OoBDev.System.ExpressionCalculator.Optimizers;

/// <summary>
/// Defines the contract for expression optimizers that transform expressions into more efficient equivalent forms.
/// Optimizers can simplify, reduce, or restructure expressions while preserving their mathematical meaning.
/// </summary>
/// <typeparam name="T">The numeric type of the expression, which must be a value type implementing IComparable&lt;T&gt; and IEquatable&lt;T&gt;.</typeparam>
public interface IExpressionOptimizer<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Optimizes the given expression by applying transformation rules to produce an equivalent but more efficient expression.
    /// </summary>
    /// <param name="expression">The expression to optimize.</param>
    /// <returns>An optimized expression that is mathematically equivalent to the input.</returns>
    ExpressionBase<T> Optimize(ExpressionBase<T> expression);
}
