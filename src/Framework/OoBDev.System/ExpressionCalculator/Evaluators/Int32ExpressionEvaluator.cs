using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

public sealed class Int32ExpressionEvaluator : IExpressionEvaluator<int>
{
    public int Add(int left, int right) => left + right;
    public int Divide(int left, int right) => left / right;

    public int Modulo(int left, int right) => left % right;
    public int Multiply(int left, int right) => left * right;
    public int Negate(int operand) => -operand;
    public int Power(int left, int right) => (int)Math.Pow(left, right);
    public int Subtract(int left, int right) => left - right;

    public int? TryParse(string input) => int.TryParse(input, out var ret) ? ret : null;
    public int GetValue(int value) => value;
    public int GetValue(double value) => (int)value;
}
