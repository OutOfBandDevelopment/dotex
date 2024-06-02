using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

public sealed class UInt8ExpressionEvaluator : IExpressionEvaluator<byte>
{
    public byte Add(byte left, byte right) => (byte)(left + right);
    public byte Divide(byte left, byte right) => (byte)(left / right);

    public byte Modulo(byte left, byte right) => (byte)(left % right);
    public byte Multiply(byte left, byte right) => (byte)(left * right);
    public byte Negate(byte operand) => throw new NotSupportedException(nameof(Negate));
    public byte Power(byte left, byte right) => (byte)Math.Pow(left, right);
    public byte Subtract(byte left, byte right) => (byte)(left - right);

    public byte? TryParse(string input) => byte.TryParse(input, out var ret) ? ret : null;
    public byte GetValue(int value) => (byte)value;
    public byte GetValue(double value) => (byte)value;
}
