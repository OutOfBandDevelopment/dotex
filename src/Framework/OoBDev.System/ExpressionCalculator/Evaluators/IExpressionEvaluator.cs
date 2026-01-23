using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Defines arithmetic operations and value conversions for expression evaluation with a specific numeric type.
/// </summary>
/// <typeparam name="T">The numeric type that supports comparison and equality (must be a value type).</typeparam>
public interface IExpressionEvaluator<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Raises the left operand to the power of the right operand.
    /// </summary>
    /// <param name="left">The base value.</param>
    /// <param name="right">The exponent value.</param>
    /// <returns>The result of left raised to the power of right.</returns>
    T Power(T left, T right);

    /// <summary>
    /// Multiplies two values.
    /// </summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    /// <returns>The product of left and right.</returns>
    T Multiply(T left, T right);

    /// <summary>
    /// Divides the left value by the right value.
    /// </summary>
    /// <param name="left">The dividend.</param>
    /// <param name="right">The divisor.</param>
    /// <returns>The quotient of left divided by right.</returns>
    T Divide(T left, T right);

    /// <summary>
    /// Calculates the remainder of dividing the left value by the right value.
    /// </summary>
    /// <param name="left">The dividend.</param>
    /// <param name="right">The divisor.</param>
    /// <returns>The remainder of left divided by right.</returns>
    T Modulo(T left, T right);

    /// <summary>
    /// Adds two values.
    /// </summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    /// <returns>The sum of left and right.</returns>
    T Add(T left, T right);

    /// <summary>
    /// Subtracts the right value from the left value.
    /// </summary>
    /// <param name="left">The minuend.</param>
    /// <param name="right">The subtrahend.</param>
    /// <returns>The difference of left minus right.</returns>
    T Subtract(T left, T right);

    /// <summary>
    /// Negates the operand (multiplies by -1).
    /// </summary>
    /// <param name="operand">The value to negate.</param>
    /// <returns>The negated value.</returns>
    T Negate(T operand);

    /// <summary>
    /// Attempts to parse a string into a value of type T.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed value if successful, or null if parsing fails.</returns>
    T? TryParse(string input);

    /// <summary>
    /// Converts an integer value to type T.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The value converted to type T.</returns>
    T GetValue(int value);

    /// <summary>
    /// Converts a double value to type T.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <returns>The value converted to type T.</returns>
    T GetValue(double value);
}
