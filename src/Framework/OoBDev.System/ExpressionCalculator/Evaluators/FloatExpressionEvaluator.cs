namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="float"/> type.
/// </summary>
public sealed class FloatExpressionEvaluator : IExpressionEvaluator<float>
{
    /// <summary>
    /// Adds two single-precision floating-point values together.
    /// </summary>
    /// <param name="left">The first value to add.</param>
    /// <param name="right">The second value to add.</param>
    /// <returns>The sum of the two values.</returns>
    public float Add(float left, float right) => left + right;

    /// <summary>
    /// Divides the left single-precision floating-point value by the right single-precision floating-point value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The quotient of the division.</returns>
    public float Divide(float left, float right) => left / right;

    /// <summary>
    /// Computes the modulo (remainder) of dividing the left single-precision floating-point value by the right single-precision floating-point value.
    /// </summary>
    /// <param name="left">The dividend value.</param>
    /// <param name="right">The divisor value.</param>
    /// <returns>The remainder of the division.</returns>
    public float Modulo(float left, float right) => left % right;

    /// <summary>
    /// Multiplies two single-precision floating-point values together.
    /// </summary>
    /// <param name="left">The first value to multiply.</param>
    /// <param name="right">The second value to multiply.</param>
    /// <returns>The product of the two values.</returns>
    public float Multiply(float left, float right) => left * right;

    /// <summary>
    /// Negates the specified single-precision floating-point value.
    /// </summary>
    /// <param name="operand">The value to negate.</param>
    /// <returns>The negated value.</returns>
    public float Negate(float operand) => -operand;

    /// <summary>
    /// Raises the left single-precision floating-point value to the power of the right single-precision floating-point value.
    /// </summary>
    /// <param name="left">The base value.</param>
    /// <param name="right">The exponent value.</param>
    /// <returns>The result of left raised to the power of right, cast to float.</returns>
    public float Power(float left, float right) => (float)global::System.Math.Pow((double)left, (double)right);

    /// <summary>
    /// Subtracts the right single-precision floating-point value from the left single-precision floating-point value.
    /// </summary>
    /// <param name="left">The value to subtract from.</param>
    /// <param name="right">The value to subtract.</param>
    /// <returns>The difference of the two values.</returns>
    public float Subtract(float left, float right) => left - right;

    /// <summary>
    /// Attempts to parse the specified string as a single-precision floating-point value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed float value if successful; otherwise, null.</returns>
    public float? TryParse(string input) => float.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Converts the specified integer value to a single-precision floating-point value.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The value without conversion.</returns>
    public float GetValue(int value) => value;

    /// <summary>
    /// Converts the specified double-precision floating-point value to a single-precision floating-point value.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <returns>The value cast to float.</returns>
    public float GetValue(double value) => (float)value;
}
