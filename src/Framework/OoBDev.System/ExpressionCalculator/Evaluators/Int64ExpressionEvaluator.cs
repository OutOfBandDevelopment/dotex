namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="long"/> (Int64) type.
/// </summary>
public sealed class Int64ExpressionEvaluator : IExpressionEvaluator<long>
{
    /// <summary>
    /// Adds two 64-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to add.</param>
    /// <param name="right">The second value to add.</param>
    /// <returns>The sum of the two values.</returns>
    public long Add(long left, long right) => left + right;

    /// <summary>
    /// Divides the left 64-bit integer value by the right 64-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The quotient of the division.</returns>
    public long Divide(long left, long right) => left / right;

    /// <summary>
    /// Computes the modulo (remainder) of dividing the left 64-bit integer value by the right 64-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The remainder of the division.</returns>
    public long Modulo(long left, long right) => left % right;

    /// <summary>
    /// Multiplies two 64-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to multiply.</param>
    /// <param name="right">The second value to multiply.</param>
    /// <returns>The product of the two values.</returns>
    public long Multiply(long left, long right) => left * right;

    /// <summary>
    /// Negates the specified 64-bit integer value.
    /// </summary>
    /// <param name="operand">The value to negate.</param>
    /// <returns>The negated value.</returns>
    public long Negate(long operand) => -operand;

    /// <summary>
    /// Raises the left 64-bit integer value to the power of the right 64-bit integer value.
    /// </summary>
    /// <param name="left">The base value.</param>
    /// <param name="right">The exponent value.</param>
    /// <returns>The result of left raised to the power of right, cast to long.</returns>
    public long Power(long left, long right) => (long)global::System.Math.Pow(left, right);

    /// <summary>
    /// Subtracts the right 64-bit integer value from the left 64-bit integer value.
    /// </summary>
    /// <param name="left">The value to subtract from.</param>
    /// <param name="right">The value to subtract.</param>
    /// <returns>The difference of the two values.</returns>
    public long Subtract(long left, long right) => left - right;

    /// <summary>
    /// Attempts to parse the specified string as a 64-bit integer value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed long value if successful; otherwise, null.</returns>
    public long? TryParse(string input) => long.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Converts the specified integer value to a 64-bit integer.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The value without conversion.</returns>
    public long GetValue(int value) => value;

    /// <summary>
    /// Converts the specified double-precision floating-point value to a 64-bit integer.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <returns>The value cast to long.</returns>
    public long GetValue(double value) => (long)value;
}
