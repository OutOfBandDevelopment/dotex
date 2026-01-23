namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="sbyte"/> (Int8) type.
/// </summary>
public sealed class Int8ExpressionEvaluator : IExpressionEvaluator<sbyte>
{
    /// <summary>
    /// Adds two signed 8-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to add.</param>
    /// <param name="right">The second value to add.</param>
    /// <returns>The sum of the two values, cast to sbyte.</returns>
    public sbyte Add(sbyte left, sbyte right) => (sbyte)(left + right);

    /// <summary>
    /// Divides the left signed 8-bit integer value by the right signed 8-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The quotient of the division, cast to sbyte.</returns>
    public sbyte Divide(sbyte left, sbyte right) => (sbyte)(left / right);

    /// <summary>
    /// Computes the modulo (remainder) of dividing the left signed 8-bit integer value by the right signed 8-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The remainder of the division, cast to sbyte.</returns>
    public sbyte Modulo(sbyte left, sbyte right) => (sbyte)(left % right);

    /// <summary>
    /// Multiplies two signed 8-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to multiply.</param>
    /// <param name="right">The second value to multiply.</param>
    /// <returns>The product of the two values, cast to sbyte.</returns>
    public sbyte Multiply(sbyte left, sbyte right) => (sbyte)(left * right);

    /// <summary>
    /// Negates the specified signed 8-bit integer value.
    /// </summary>
    /// <param name="operand">The value to negate.</param>
    /// <returns>The negated value, cast to sbyte.</returns>
    public sbyte Negate(sbyte operand) => (sbyte)-operand;

    /// <summary>
    /// Raises the left signed 8-bit integer value to the power of the right signed 8-bit integer value.
    /// </summary>
    /// <param name="left">The base value.</param>
    /// <param name="right">The exponent value.</param>
    /// <returns>The result of left raised to the power of right, cast to sbyte.</returns>
    public sbyte Power(sbyte left, sbyte right) => (sbyte)global::System.Math.Pow(left, right);

    /// <summary>
    /// Subtracts the right signed 8-bit integer value from the left signed 8-bit integer value.
    /// </summary>
    /// <param name="left">The value to subtract from.</param>
    /// <param name="right">The value to subtract.</param>
    /// <returns>The difference of the two values, cast to sbyte.</returns>
    public sbyte Subtract(sbyte left, sbyte right) => (sbyte)(left - right);

    /// <summary>
    /// Attempts to parse the specified string as a signed 8-bit integer value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed sbyte value if successful; otherwise, null.</returns>
    public sbyte? TryParse(string input) => sbyte.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Converts the specified integer value to a signed 8-bit integer.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The value cast to sbyte.</returns>
    public sbyte GetValue(int value) => (sbyte)value;

    /// <summary>
    /// Converts the specified double-precision floating-point value to a signed 8-bit integer.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <returns>The value cast to sbyte.</returns>
    public sbyte GetValue(double value) => (sbyte)value;
}
