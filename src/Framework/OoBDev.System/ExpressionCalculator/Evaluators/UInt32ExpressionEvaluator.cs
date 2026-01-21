using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Provides arithmetic operations and value conversions for expression evaluation using the <see cref="uint"/> (UInt32) type.
/// Note: Negate operation is not supported for unsigned types.
/// </summary>
public sealed class UInt32ExpressionEvaluator : IExpressionEvaluator<uint>
{
    public uint Add(uint left, uint right) => left + right;
    public uint Divide(uint left, uint right) => left / right;

    public uint Modulo(uint left, uint right) => left % right;
    public uint Multiply(uint left, uint right) => left * right;
    public uint Negate(uint operand) => throw new NotSupportedException(nameof(Negate));

    public uint Power(uint left, uint right) => (uint)global::System.Math.Pow(left, right);
    public uint Subtract(uint left, uint right) => left - right;

    public uint? TryParse(string input) => uint.TryParse(input, out var ret) ? ret : null;
    public uint GetValue(int value) => (uint)value;
    public uint GetValue(double value) => (uint)value;

}
