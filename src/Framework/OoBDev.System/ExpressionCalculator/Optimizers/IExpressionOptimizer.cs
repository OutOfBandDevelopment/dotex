using OoBDev.System.ExpressionCalculator.Expressions;
using System;

namespace OoBDev.System.ExpressionCalculator.Optimizers;

public interface IExpressionOptimizer<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    ExpressionBase<T> Optimize(ExpressionBase<T> expression);
}
