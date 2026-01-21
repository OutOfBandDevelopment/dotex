using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="byte"/> (UInt8) type.
/// Note: Negate operation is not supported for unsigned types.
/// </summary>
public sealed class UInt8ExpressionEvaluator : IExpressionEvaluator<byte>
{
    /// <summary>
    /// Adds two byte values together.
    /// </summary>
    /// <param name="left">The first value to add.</param>
    /// <param name="right">The second value to add.</param>
    /// <returns>The sum of the two values, cast to byte.</returns>
    public byte Add(byte left, byte right) => (byte)(left + right);

    /// <summary>
    /// Divides the left byte value by the right byte value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The quotient of the division, cast to byte.</returns>
    public byte Divide(byte left, byte right) => (byte)(left / right);

    /// <summary>
    /// Computes the modulo (remainder) of dividing the left byte value by the right byte value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The remainder of the division, cast to byte.</returns>
    public byte Modulo(byte left, byte right) => (byte)(left % right);

    /// <summary>
    /// Multiplies two byte values together.
    /// </summary>
    /// <param name="left">The first value to multiply.</param>
    /// <param name="right">The second value to multiply.</param>
    /// <returns>The product of the two values, cast to byte.</returns>
    public byte Multiply(byte left, byte right) => (byte)(left * right);

    /// <summary>
    /// Negates the specified byte value. This operation is not supported for unsigned types.
    /// </summary>
    /// <param name="operand">The value to negate.</param>
    /// <returns>This method always throws an exception.</returns>
    /// <exception cref="NotSupportedException">Always thrown because negation is not supported for unsigned types.</exception>
    public byte Negate(byte operand) => throw new NotSupportedException(nameof(Negate));

    /// <summary>
    /// Raises the left byte value to the power of the right byte value.
    /// </summary>
    /// <param name="left">The base value.</param>
    /// <param name="right">The exponent value.</param>
    /// <returns>The result of left raised to the power of right, cast to byte.</returns>
    public byte Power(byte left, byte right) => (byte)global::System.Math.Pow(left, right);

    /// <summary>
    /// Subtracts the right byte value from the left byte value.
    /// </summary>
    /// <param name="left">The value to subtract from.</param>
    /// <param name="right">The value to subtract.</param>
    /// <returns>The difference of the two values, cast to byte.</returns>
    public byte Subtract(byte left, byte right) => (byte)(left - right);

    /// <summary>
    /// Attempts to parse the specified string as a byte value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed byte value if successful; otherwise, null.</returns>
    public byte? TryParse(string input) => byte.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Converts the specified integer value to a byte.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The value cast to byte.</returns>
    public byte GetValue(int value) => (byte)value;

    /// <summary>
    /// Converts the specified double-precision floating-point value to a byte.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <returns>The value cast to byte.</returns>
    public byte GetValue(double value) => (byte)value;
}
