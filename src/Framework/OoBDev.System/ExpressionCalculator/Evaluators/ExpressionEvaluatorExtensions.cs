using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides extension methods for <see cref="IExpressionEvaluator{T}"/> to perform advanced operations like sequences, products, sums, and factorials.
/// </summary>
public static class ExpressionEvaluatorExtensions
{
    /// <summary>
    /// Defines a function that generates the next value in a sequence given an evaluator, current value, and index.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="evaluator">The expression evaluator.</param>
    /// <param name="current">The current value in the sequence.</param>
    /// <param name="index">The zero-based index in the sequence.</param>
    /// <returns>The next value in the sequence.</returns>
    public delegate T EvaluationFunction<T>(IExpressionEvaluator<T> evaluator, T current, int index)
         where T : struct, IComparable<T>, IEquatable<T>;

    /// <summary>
    /// Defines a predicate that determines whether to continue generating sequence values.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="evaluator">The expression evaluator.</param>
    /// <param name="current">The current value in the sequence.</param>
    /// <param name="index">The zero-based index in the sequence.</param>
    /// <returns>True to continue the sequence, false to stop.</returns>
    public delegate bool EvaluationPredicate<T>(IExpressionEvaluator<T> evaluator, T current, int index)
        where T : struct, IComparable<T>, IEquatable<T>;

    /// <summary>
    /// Generates a sequence of values starting from a seed value, applying a function to generate each subsequent value.
    /// The sequence continues until the predicate returns false (or indefinitely if no predicate is provided).
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="evaluator">The expression evaluator to use for operations.</param>
    /// <param name="seed">The initial value of the sequence.</param>
    /// <param name="function">The function to generate the next value from the current value and index.</param>
    /// <param name="predicate">An optional predicate to determine when to stop the sequence (defaults to infinite if null).</param>
    /// <returns>An enumerable sequence of values.</returns>
    public static IEnumerable<T> Sequence<T>(
        this IExpressionEvaluator<T> evaluator,
        T seed,
        EvaluationFunction<T> function,
        EvaluationPredicate<T>? predicate = null
        ) where T : struct, IComparable<T>, IEquatable<T>
    {
        var index = 0;
        var current = seed;

        while (predicate?.Invoke(evaluator, current, index) ?? true)
        {
            yield return current;
            current = function(evaluator, current, index);
            index++;
        }
    }

    /// <summary>
    /// Calculates the product of all values in the sequence by multiplying them together.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="evaluator">The expression evaluator to use for multiplication.</param>
    /// <param name="values">The values to multiply together.</param>
    /// <returns>The product of all values (starting from 1).</returns>
    public static T Product<T>(
        this IExpressionEvaluator<T> evaluator,
        IEnumerable<T> values
        ) where T : struct, IComparable<T>, IEquatable<T> =>
        values.Aggregate(evaluator.GetValue(1), evaluator.Multiply);

    /// <summary>
    /// Calculates the sum of all values in the sequence by adding them together.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="evaluator">The expression evaluator to use for addition.</param>
    /// <param name="values">The values to add together.</param>
    /// <returns>The sum of all values (starting from 0).</returns>
    public static T Sum<T>(
        this IExpressionEvaluator<T> evaluator,
        IEnumerable<T> values
        ) where T : struct, IComparable<T>, IEquatable<T> =>
        values.Aggregate(evaluator.GetValue(0), evaluator.Add);

    /// <summary>
    /// Calculates the factorial of a value (base!), which is the product of all positive integers less than or equal to the base.
    /// For example, 5! = 5 × 4 × 3 × 2 × 1 = 120.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="evaluator">The expression evaluator to use for operations.</param>
    /// <param name="base">The value to calculate the factorial of.</param>
    /// <returns>The factorial of the base value.</returns>
    public static T Factorial<T>(
        this IExpressionEvaluator<T> evaluator,
        T @base
        ) where T : struct, IComparable<T>, IEquatable<T>
    {
        var sequence = evaluator.Sequence(
            @base,
            (ev, n, i) => ev.Subtract(n, ev.GetValue(1)),
            (ev, n, i) => n.CompareTo(ev.GetValue(0)) > 0
            );
        var result = evaluator.Product(sequence);
        return result;
    }
}
