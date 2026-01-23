namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="decimal"/> type.
/// </summary>
public sealed class DecimalExpressionEvaluator : IExpressionEvaluator<decimal>
{
    /// <summary>
    /// Adds two decimal values together.
    /// </summary>
    /// <param name="left">The first value to add.</param>
    /// <param name="right">The second value to add.</param>
    /// <returns>The sum of the two values.</returns>
    public decimal Add(decimal left, decimal right) => left + right;

    /// <summary>
    /// Divides the left decimal value by the right decimal value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The quotient of the division.</returns>
    public decimal Divide(decimal left, decimal right) => left / right;

    /// <summary>
    /// Computes the modulo (remainder) of dividing the left decimal value by the right decimal value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The remainder of the division.</returns>
    public decimal Modulo(decimal left, decimal right) => left % right;

    /// <summary>
    /// Multiplies two decimal values together.
    /// </summary>
    /// <param name="left">The first value to multiply.</param>
    /// <param name="right">The second value to multiply.</param>
    /// <returns>The product of the two values.</returns>
    public decimal Multiply(decimal left, decimal right) => left * right;

    /// <summary>
    /// Negates the specified decimal value.
    /// </summary>
    /// <param name="operand">The value to negate.</param>
    /// <returns>The negated value.</returns>
    public decimal Negate(decimal operand) => -operand;

    /// <summary>
    /// Raises the left decimal value to the power of the right decimal value.
    /// </summary>
    /// <param name="left">The base value.</param>
    /// <param name="right">The exponent value.</param>
    /// <returns>The result of left raised to the power of right, cast to decimal.</returns>
    public decimal Power(decimal left, decimal right) => (decimal)global::System.Math.Pow((double)left, (double)right);

    /// <summary>
    /// Subtracts the right decimal value from the left decimal value.
    /// </summary>
    /// <param name="left">The value to subtract from.</param>
    /// <param name="right">The value to subtract.</param>
    /// <returns>The difference of the two values.</returns>
    public decimal Subtract(decimal left, decimal right) => left - right;

    /// <summary>
    /// Attempts to parse the specified string as a decimal value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed decimal value if successful; otherwise, null.</returns>
    public decimal? TryParse(string input) => decimal.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Converts the specified integer value to a decimal.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The value without conversion.</returns>
    public decimal GetValue(int value) => value;

    /// <summary>
    /// Converts the specified double-precision floating-point value to a decimal.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <returns>The value cast to decimal.</returns>
    public decimal GetValue(double value) => (decimal)value;
}
