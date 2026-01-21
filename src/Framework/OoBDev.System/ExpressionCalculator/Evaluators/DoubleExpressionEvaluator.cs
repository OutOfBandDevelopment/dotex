namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="double"/> type.
/// </summary>
public sealed class DoubleExpressionEvaluator : IExpressionEvaluator<double>
{
    /// <summary>
    /// Adds two double-precision floating-point values together.
    /// </summary>
    /// <param name="left">The first value to add.</param>
    /// <param name="right">The second value to add.</param>
    /// <returns>The sum of the two values.</returns>
    public double Add(double left, double right) => left + right;

    /// <summary>
    /// Divides the left double-precision floating-point value by the right double-precision floating-point value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The quotient of the division.</returns>
    public double Divide(double left, double right) => left / right;

    /// <summary>
    /// Computes the modulo (remainder) of dividing the left double-precision floating-point value by the right double-precision floating-point value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The remainder of the division.</returns>
    public double Modulo(double left, double right) => left % right;

    /// <summary>
    /// Multiplies two double-precision floating-point values together.
    /// </summary>
    /// <param name="left">The first value to multiply.</param>
    /// <param name="right">The second value to multiply.</param>
    /// <returns>The product of the two values.</returns>
    public double Multiply(double left, double right) => left * right;

    /// <summary>
    /// Negates the specified double-precision floating-point value.
    /// </summary>
    /// <param name="operand">The value to negate.</param>
    /// <returns>The negated value.</returns>
    public double Negate(double operand) => -operand;

    /// <summary>
    /// Raises the left double-precision floating-point value to the power of the right double-precision floating-point value.
    /// </summary>
    /// <param name="left">The base value.</param>
    /// <param name="right">The exponent value.</param>
    /// <returns>The result of left raised to the power of right.</returns>
    public double Power(double left, double right) => (double)global::System.Math.Pow((double)left, (double)right);

    /// <summary>
    /// Subtracts the right double-precision floating-point value from the left double-precision floating-point value.
    /// </summary>
    /// <param name="left">The value to subtract from.</param>
    /// <param name="right">The value to subtract.</param>
    /// <returns>The difference of the two values.</returns>
    public double Subtract(double left, double right) => left - right;

    /// <summary>
    /// Attempts to parse the specified string as a double-precision floating-point value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed double value if successful; otherwise, null.</returns>
    public double? TryParse(string input) => double.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Converts the specified integer value to a double-precision floating-point value.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The value without conversion.</returns>
    public double GetValue(int value) => value;

    /// <summary>
    /// Returns the specified double-precision floating-point value as-is.
    /// </summary>
    /// <param name="value">The double value to return.</param>
    /// <returns>The value without conversion.</returns>
    public double GetValue(double value) => value;
}
