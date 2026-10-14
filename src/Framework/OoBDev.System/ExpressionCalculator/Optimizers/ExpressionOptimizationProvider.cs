using OoBDev.System.ExpressionCalculator.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.ExpressionCalculator.Optimizers;

/// <summary>
/// Provides expression optimization services by applying a series of optimization strategies to simplify and improve expression performance.
/// Optimizations include constant folding, identity reduction, inner expression simplification, and variable shifting.
/// </summary>
/// <typeparam name="T">The numeric type used in the expression.</typeparam>
public class ExpressionOptimizationProvider<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    private static readonly IEnumerable<IExpressionOptimizer<T>> _optimizations =
    [
        new InnerExpressionReducer<T>(),
        new UnaryNumericExpressionReducer<T>(),
        new IdentityExpressionOptimizer<T>(),
        new DeterminedExpressionReducer<T>(),
        new ShiftCommutativeVariablesRight<T>(),
    ];

    /// <summary>
    /// Optimizes an expression by repeatedly applying optimization strategies until no further optimizations are possible.
    /// </summary>
    /// <param name="expression">The expression to optimize.</param>
    /// <returns>An optimized version of the expression with reduced complexity.</returns>
    public ExpressionBase<T> Optimize(ExpressionBase<T> expression)
    {
        var optimized = _optimizations.Aggregate(expression.Clone(), (exp, operation) => operation.Optimize(exp));
        return string.Equals(optimized.ToString(), expression.ToString(), StringComparison.Ordinal) ? optimized : Optimize(optimized);
    }
}
