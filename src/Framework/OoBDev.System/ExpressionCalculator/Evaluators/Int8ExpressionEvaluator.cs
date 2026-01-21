namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="sbyte"/> (Int8) type.
/// </summary>
public sealed class Int8ExpressionEvaluator : IExpressionEvaluator<sbyte>
{
    public sbyte Add(sbyte left, sbyte right) => (sbyte)(left + right);
    public sbyte Divide(sbyte left, sbyte right) => (sbyte)(left / right);

    public sbyte Modulo(sbyte left, sbyte right) => (sbyte)(left % right);
    public sbyte Multiply(sbyte left, sbyte right) => (sbyte)(left * right);
    public sbyte Negate(sbyte operand) => (sbyte)-operand;
    public sbyte Power(sbyte left, sbyte right) => (sbyte)global::System.Math.Pow(left, right);
    public sbyte Subtract(sbyte left, sbyte right) => (sbyte)(left - right);

    public sbyte? TryParse(string input) => sbyte.TryParse(input, out var ret) ? ret : null;
    public sbyte GetValue(int value) => (sbyte)value;
    public sbyte GetValue(double value) => (sbyte)value;
}
