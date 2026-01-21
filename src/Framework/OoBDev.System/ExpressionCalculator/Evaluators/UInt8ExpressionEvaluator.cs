using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="byte"/> (UInt8) type.
/// Note: Negate operation is not supported for unsigned types.
/// </summary>
public sealed class UInt8ExpressionEvaluator : IExpressionEvaluator<byte>
{
    public byte Add(byte left, byte right) => (byte)(left + right);
    public byte Divide(byte left, byte right) => (byte)(left / right);

    public byte Modulo(byte left, byte right) => (byte)(left % right);
    public byte Multiply(byte left, byte right) => (byte)(left * right);
    public byte Negate(byte operand) => throw new NotSupportedException(nameof(Negate));
    public byte Power(byte left, byte right) => (byte)global::System.Math.Pow(left, right);
    public byte Subtract(byte left, byte right) => (byte)(left - right);

    public byte? TryParse(string input) => byte.TryParse(input, out var ret) ? ret : null;
    public byte GetValue(int value) => (byte)value;
    public byte GetValue(double value) => (byte)value;
}
