using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

public sealed class UInt16ExpressionEvaluator : IExpressionEvaluator<ushort>
{
    public ushort Add(ushort left, ushort right) => (ushort)(left + right);
    public ushort Divide(ushort left, ushort right) => (ushort)(left / right);

    public ushort Modulo(ushort left, ushort right) => (ushort)(left % right);
    public ushort Multiply(ushort left, ushort right) => (ushort)(left * right);
    public ushort Negate(ushort operand) => throw new NotSupportedException(nameof(Negate));
    public ushort Power(ushort left, ushort right) => (ushort)Math.Pow(left, right);
    public ushort Subtract(ushort left, ushort right) => (ushort)(left - right);

    public ushort? TryParse(string input) => ushort.TryParse(input, out var ret) ? ret : null;
    public ushort GetValue(int value) => (ushort)value;
    public ushort GetValue(double value) => (ushort)value;
}
