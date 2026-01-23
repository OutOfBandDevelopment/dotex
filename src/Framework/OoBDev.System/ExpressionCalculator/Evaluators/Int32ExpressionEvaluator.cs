namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="int"/> (Int32) type.
/// </summary>
public sealed class Int32ExpressionEvaluator : IExpressionEvaluator<int>
{
    /// <summary>
    /// Adds two 32-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to add.</param>
    /// <param name="right">The second value to add.</param>
    /// <returns>The sum of the two values.</returns>
    public int Add(int left, int right) => left + right;

    /// <summary>
    /// Divides the left 32-bit integer value by the right 32-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The quotient of the division.</returns>
    public int Divide(int left, int right) => left / right;

    /// <summary>
    /// Computes the modulo (remainder) of dividing the left 32-bit integer value by the right 32-bit integer value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The remainder of the division.</returns>
    public int Modulo(int left, int right) => left % right;

    /// <summary>
    /// Multiplies two 32-bit integer values together.
    /// </summary>
    /// <param name="left">The first value to multiply.</param>
    /// <param name="right">The second value to multiply.</param>
    /// <returns>The product of the two values.</returns>
    public int Multiply(int left, int right) => left * right;

    /// <summary>
    /// Negates the specified 32-bit integer value.
    /// </summary>
    /// <param name="operand">The value to negate.</param>
    /// <returns>The negated value.</returns>
    public int Negate(int operand) => -operand;

    /// <summary>
    /// Raises the left 32-bit integer value to the power of the right 32-bit integer value.
    /// </summary>
    /// <param name="left">The base value.</param>
    /// <param name="right">The exponent value.</param>
    /// <returns>The result of left raised to the power of right, cast to int.</returns>
    public int Power(int left, int right) => (int)global::System.Math.Pow(left, right);

    /// <summary>
    /// Subtracts the right 32-bit integer value from the left 32-bit integer value.
    /// </summary>
    /// <param name="left">The value to subtract from.</param>
    /// <param name="right">The value to subtract.</param>
    /// <returns>The difference of the two values.</returns>
    public int Subtract(int left, int right) => left - right;

    /// <summary>
    /// Attempts to parse the specified string as a 32-bit integer value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed int value if successful; otherwise, null.</returns>
    public int? TryParse(string input) => int.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Returns the specified integer value as-is.
    /// </summary>
    /// <param name="value">The integer value to return.</param>
    /// <returns>The value without conversion.</returns>
    public int GetValue(int value) => value;

    /// <summary>
    /// Converts the specified double-precision floating-point value to a 32-bit integer.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <returns>The value cast to int.</returns>
    public int GetValue(double value) => (int)value;
}
