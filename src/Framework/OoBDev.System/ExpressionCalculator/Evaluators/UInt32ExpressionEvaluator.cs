using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="uint"/> (UInt32) type.
/// Note: Negate operation is not supported for unsigned types.
/// </summary>
public sealed class UInt32ExpressionEvaluator : IExpressionEvaluator<uint>
{
    /// <summary>
    /// Adds two unsigned 32-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to add.</param>
    /// <param name="right">The second value to add.</param>
    /// <returns>The sum of the two values.</returns>
    public uint Add(uint left, uint right) => left + right;

    /// <summary>
    /// Divides the left unsigned 32-bit integer value by the right unsigned 32-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The quotient of the division.</returns>
    public uint Divide(uint left, uint right) => left / right;

    /// <summary>
    /// Computes the modulo (remainder) of dividing the left unsigned 32-bit integer value by the right unsigned 32-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The remainder of the division.</returns>
    public uint Modulo(uint left, uint right) => left % right;

    /// <summary>
    /// Multiplies two unsigned 32-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to multiply.</param>
    /// <param name="right">The second value to multiply.</param>
    /// <returns>The product of the two values.</returns>
    public uint Multiply(uint left, uint right) => left * right;

    /// <summary>
    /// Negates the specified unsigned 32-bit integer value. This operation is not supported for unsigned types.
    /// </summary>
    /// <param name="operand">The value to negate.</param>
    /// <returns>This method always throws an exception.</returns>
    /// <exception cref="NotSupportedException">Always thrown because negation is not supported for unsigned types.</exception>
    public uint Negate(uint operand) => throw new NotSupportedException(nameof(Negate));

    /// <summary>
    /// Raises the left unsigned 32-bit integer value to the power of the right unsigned 32-bit integer value.
    /// </summary>
    /// <param name="left">The base value.</param>
    /// <param name="right">The exponent value.</param>
    /// <returns>The result of left raised to the power of right, cast to uint.</returns>
    public uint Power(uint left, uint right) => (uint)global::System.Math.Pow(left, right);

    /// <summary>
    /// Subtracts the right unsigned 32-bit integer value from the left unsigned 32-bit integer value.
    /// </summary>
    /// <param name="left">The value to subtract from.</param>
    /// <param name="right">The value to subtract.</param>
    /// <returns>The difference of the two values.</returns>
    public uint Subtract(uint left, uint right) => left - right;

    /// <summary>
    /// Attempts to parse the specified string as an unsigned 32-bit integer value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed uint value if successful; otherwise, null.</returns>
    public uint? TryParse(string input) => uint.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Converts the specified integer value to an unsigned 32-bit integer.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The value cast to uint.</returns>
    public uint GetValue(int value) => (uint)value;

    /// <summary>
    /// Converts the specified double-precision floating-point value to an unsigned 32-bit integer.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <returns>The value cast to uint.</returns>
    public uint GetValue(double value) => (uint)value;

}
