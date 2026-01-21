using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="ulong"/> (UInt64) type.
/// Note: Negate operation is not supported for unsigned types.
/// </summary>
public sealed class UInt64ExpressionEvaluator : IExpressionEvaluator<ulong>
{
    public ulong Add(ulong left, ulong right) => left + right;
    public ulong Divide(ulong left, ulong right) => left / right;

    public ulong Modulo(ulong left, ulong right) => left % right;
    public ulong Multiply(ulong left, ulong right) => left * right;
    public ulong Negate(ulong operand) => throw new NotSupportedException(nameof(Negate));
    public ulong Power(ulong left, ulong right) => (ulong)global::System.Math.Pow(left, right);
    public ulong Subtract(ulong left, ulong right) => left - right;

    public ulong? TryParse(string input) => ulong.TryParse(input, out var ret) ? ret : null;
    public ulong GetValue(int value) => (ulong)value;
    public ulong GetValue(double value) => (ulong)value;
}
