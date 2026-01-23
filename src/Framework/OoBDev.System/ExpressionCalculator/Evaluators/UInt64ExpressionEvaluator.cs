using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="ulong"/> (UInt64) type.
/// Note: Negate operation is not supported for unsigned types.
/// </summary>
public sealed class UInt64ExpressionEvaluator : IExpressionEvaluator<ulong>
{
    /// <summary>
    /// Adds two unsigned 64-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to add.</param>
    /// <param name="right">The second value to add.</param>
    /// <returns>The sum of the two values.</returns>
    public ulong Add(ulong left, ulong right) => left + right;

    /// <summary>
    /// Divides the left unsigned 64-bit integer value by the right unsigned 64-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The quotient of the division.</returns>
    public ulong Divide(ulong left, ulong right) => left / right;

    /// <summary>
    /// Computes the modulo (remainder) of dividing the left unsigned 64-bit integer value by the right unsigned 64-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The remainder of the division.</returns>
    public ulong Modulo(ulong left, ulong right) => left % right;

    /// <summary>
    /// Multiplies two unsigned 64-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to multiply.</param>
    /// <param name="right">The second value to multiply.</param>
    /// <returns>The product of the two values.</returns>
    public ulong Multiply(ulong left, ulong right) => left * right;

    /// <summary>
    /// Negates the specified unsigned 64-bit integer value. This operation is not supported for unsigned types.
    /// </summary>
    /// <param name="operand">The value to negate.</param>
    /// <returns>This method always throws an exception.</returns>
    /// <exception cref="NotSupportedException">Always thrown because negation is not supported for unsigned types.</exception>
    public ulong Negate(ulong operand) => throw new NotSupportedException(nameof(Negate));

    /// <summary>
    /// Raises the left unsigned 64-bit integer value to the power of the right unsigned 64-bit integer value.
    /// </summary>
    /// <param name="left">The base value.</param>
    /// <param name="right">The exponent value.</param>
    /// <returns>The result of left raised to the power of right, cast to ulong.</returns>
    public ulong Power(ulong left, ulong right) => (ulong)global::System.Math.Pow(left, right);

    /// <summary>
    /// Subtracts the right unsigned 64-bit integer value from the left unsigned 64-bit integer value.
    /// </summary>
    /// <param name="left">The value to subtract from.</param>
    /// <param name="right">The value to subtract.</param>
    /// <returns>The difference of the two values.</returns>
    public ulong Subtract(ulong left, ulong right) => left - right;

    /// <summary>
    /// Attempts to parse the specified string as an unsigned 64-bit integer value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed ulong value if successful; otherwise, null.</returns>
    public ulong? TryParse(string input) => ulong.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Converts the specified integer value to an unsigned 64-bit integer.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The value cast to ulong.</returns>
    public ulong GetValue(int value) => (ulong)value;

    /// <summary>
    /// Converts the specified double-precision floating-point value to an unsigned 64-bit integer.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <returns>The value cast to ulong.</returns>
    public ulong GetValue(double value) => (ulong)value;
}
