using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

public sealed class DoubleExpressionEvaluator : IExpressionEvaluator<double>
{
    public double Add(double left, double right) => left + right;
    public double Divide(double left, double right) => left / right;

    public double Modulo(double left, double right) => left % right;
    public double Multiply(double left, double right) => left * right;
    public double Negate(double operand) => -operand;
    public double Power(double left, double right) => (double)Math.Pow((double)left, (double)right);
    public double Subtract(double left, double right) => left - right;

    public double? TryParse(string input) => double.TryParse(input, out var ret) ? ret : null;
    public double GetValue(int value) => value;
    public double GetValue(double value) => value;
}
