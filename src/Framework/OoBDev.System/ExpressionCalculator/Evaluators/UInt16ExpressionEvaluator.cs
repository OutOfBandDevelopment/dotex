using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="ushort"/> (UInt16) type.
/// Note: Negate operation is not supported for unsigned types.
/// </summary>
public sealed class UInt16ExpressionEvaluator : IExpressionEvaluator<ushort>
{
    /// <summary>
    /// Adds two unsigned 16-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to add.</param>
    /// <param name="right">The second value to add.</param>
    /// <returns>The sum of the two values, cast to ushort.</returns>
    public ushort Add(ushort left, ushort right) => (ushort)(left + right);

    /// <summary>
    /// Divides the left unsigned 16-bit integer value by the right unsigned 16-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The quotient of the division, cast to ushort.</returns>
    public ushort Divide(ushort left, ushort right) => (ushort)(left / right);

    /// <summary>
    /// Computes the modulo (remainder) of dividing the left unsigned 16-bit integer value by the right unsigned 16-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The remainder of the division, cast to ushort.</returns>
    public ushort Modulo(ushort left, ushort right) => (ushort)(left % right);

    /// <summary>
    /// Multiplies two unsigned 16-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to multiply.</param>
    /// <param name="right">The second value to multiply.</param>
    /// <returns>The product of the two values, cast to ushort.</returns>
    public ushort Multiply(ushort left, ushort right) => (ushort)(left * right);

    /// <summary>
    /// Negates the specified unsigned 16-bit integer value. This operation is not supported for unsigned types.
    /// </summary>
    /// <param name="operand">The value to negate.</param>
    /// <returns>This method always throws an exception.</returns>
    /// <exception cref="NotSupportedException">Always thrown because negation is not supported for unsigned types.</exception>
    public ushort Negate(ushort operand) => throw new NotSupportedException(nameof(Negate));

    /// <summary>
    /// Raises the left unsigned 16-bit integer value to the power of the right unsigned 16-bit integer value.
    /// </summary>
    /// <param name="left">The base value.</param>
    /// <param name="right">The exponent value.</param>
    /// <returns>The result of left raised to the power of right, cast to ushort.</returns>
    public ushort Power(ushort left, ushort right) => (ushort)global::System.Math.Pow(left, right);

    /// <summary>
    /// Subtracts the right unsigned 16-bit integer value from the left unsigned 16-bit integer value.
    /// </summary>
    /// <param name="left">The value to subtract from.</param>
    /// <param name="right">The value to subtract.</param>
    /// <returns>The difference of the two values, cast to ushort.</returns>
    public ushort Subtract(ushort left, ushort right) => (ushort)(left - right);

    /// <summary>
    /// Attempts to parse the specified string as an unsigned 16-bit integer value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed ushort value if successful; otherwise, null.</returns>
    public ushort? TryParse(string input) => ushort.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Converts the specified integer value to an unsigned 16-bit integer.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The value cast to ushort.</returns>
    public ushort GetValue(int value) => (ushort)value;

    /// <summary>
    /// Converts the specified double-precision floating-point value to an unsigned 16-bit integer.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <returns>The value cast to ushort.</returns>
    public ushort GetValue(double value) => (ushort)value;
}
