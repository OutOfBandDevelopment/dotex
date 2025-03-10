﻿using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

public sealed class Int64ExpressionEvaluator : IExpressionEvaluator<long>
{
    public long Add(long left, long right) => left + right;
    public long Divide(long left, long right) => left / right;

    public long Modulo(long left, long right) => left % right;
    public long Multiply(long left, long right) => left * right;
    public long Negate(long operand) => -operand;
    public long Power(long left, long right) => (long)Math.Pow(left, right);
    public long Subtract(long left, long right) => left - right;

    public long? TryParse(string input) => long.TryParse(input, out var ret) ? ret : null;
    public long GetValue(int value) => value;
    public long GetValue(double value) => (long)value;
}
