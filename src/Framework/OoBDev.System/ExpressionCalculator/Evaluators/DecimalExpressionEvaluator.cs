namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="decimal"/> type.
/// </summary>
public sealed class DecimalExpressionEvaluator : IExpressionEvaluator<decimal>
{
    /// <inheritdoc/>
    public decimal Add(decimal left, decimal right) => left + right;

    /// <inheritdoc/>
    public decimal Divide(decimal left, decimal right) => left / right;

    /// <inheritdoc/>
    public decimal Modulo(decimal left, decimal right) => left % right;

    /// <inheritdoc/>
    public decimal Multiply(decimal left, decimal right) => left * right;

    /// <inheritdoc/>
    public decimal Negate(decimal operand) => -operand;

    /// <inheritdoc/>
    public decimal Power(decimal left, decimal right) => (decimal)global::System.Math.Pow((double)left, (double)right);

    /// <inheritdoc/>
    public decimal Subtract(decimal left, decimal right) => left - right;

    /// <inheritdoc/>
    public decimal? TryParse(string input) => decimal.TryParse(input, out var ret) ? ret : null;

    /// <inheritdoc/>
    public decimal GetValue(int value) => value;

    /// <inheritdoc/>
    public decimal GetValue(double value) => (decimal)value;
}
