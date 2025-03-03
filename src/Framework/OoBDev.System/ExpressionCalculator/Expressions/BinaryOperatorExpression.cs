using OoBDev.System.ExpressionCalculator.Evaluators;
using System;
using System.Collections.Generic;
using static OoBDev.System.ExpressionCalculator.Expressions.BinaryOperators;

namespace OoBDev.System.ExpressionCalculator.Expressions;

/// <summary>
/// Represents a binary operator expression with two operands.
/// </summary>
/// <typeparam name="T">The type of the value in the expression (must be a value type that is comparable and equatable).</typeparam>
public sealed class BinaryOperatorExpression<T>(
    ExpressionBase<T> left,
    BinaryOperators @operator,
    ExpressionBase<T> right
        ) : ExpressionBase<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    private static readonly IExpressionEvaluator<T> _evaluator = ExpressionEvaluatorFactory.Create<T>();

    /// <summary>
    /// Gets the left operand of the binary operator expression.
    /// </summary>
    public ExpressionBase<T> Left { get; } = left;

    /// <summary>
    /// Gets the binary operator used in the expression.
    /// </summary>
    public BinaryOperators Operator { get; } = @operator;

    /// <summary>
    /// Gets the right operand of the binary operator expression.
    /// </summary>
    public ExpressionBase<T> Right { get; } = right;

    /// <summary>
    /// Creates a new clone of the current binary operator expression.
    /// </summary>
    /// <returns>A new instance of the same type with cloned operands.</returns>
    public override ExpressionBase<T> Clone() => new BinaryOperatorExpression<T>(Left.Clone(), Operator, Right.Clone());

    /// <summary>
    /// Evaluates the binary operator expression given a dictionary of variable values.
    /// </summary>
    /// <param name="variables">The dictionary containing variable values for evaluation.</param>
    /// <returns>The result of the binary operation.</returns>
    public override T Evaluate(IDictionary<string, T> variables) =>
        Operator switch
        {
            Power => _evaluator.Power(Left.Evaluate(variables), Right.Evaluate(variables)),

            Multiply => _evaluator.Multiply(Left.Evaluate(variables), Right.Evaluate(variables)),
            Divide => _evaluator.Divide(Left.Evaluate(variables), Right.Evaluate(variables)),
            Modulo => _evaluator.Modulo(Left.Evaluate(variables), Right.Evaluate(variables)),

            Add => _evaluator.Add(Left.Evaluate(variables), Right.Evaluate(variables)),
            Subtract => _evaluator.Subtract(Left.Evaluate(variables), Right.Evaluate(variables)),

            _ => throw new NotSupportedException($"Operator {Operator} not supported")
        };

    /// <summary>
    /// Returns a string representation of the binary operator expression.
    /// </summary>
    /// <returns>A string that represents the binary operator expression in the format "LeftOperand Operator RightOperand".</returns>
    public override string ToString() => $"{Left} {Operator.AsString()} {Right}";
}
