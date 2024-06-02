using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

public sealed class Int16ExpressionEvaluator : IExpressionEvaluator<short>
{
    public short Add(short left, short right) => (short)(left + right);
    public short Divide(short left, short right) => (short)(left / right);

    public short Modulo(short left, short right) => (short)(left % right);
    public short Multiply(short left, short right) => (short)(left * right);
    public short Negate(short operand) => (short)-operand;
    public short Power(short left, short right) => (short)Math.Pow(left, right);
    public short Subtract(short left, short right) => (short)(left - right);

    public short? TryParse(string input) => short.TryParse(input, out var ret) ? ret : null;
    public short GetValue(int value) => (short)value;
    public short GetValue(double value) => (short)value;
}
