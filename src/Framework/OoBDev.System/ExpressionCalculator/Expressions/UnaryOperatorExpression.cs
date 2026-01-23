using OoBDev.System.ExpressionCalculator.Evaluators;
using System;
using System.Collections.Generic;
using static OoBDev.System.ExpressionCalculator.Expressions.UnaryOperators;

namespace OoBDev.System.ExpressionCalculator.Expressions;

/// <summary>
/// Represents a unary operation expression (e.g., negation, factorial) applied to a single operand.
/// </summary>
/// <typeparam name="T">The numeric type of the expression, which must be a value type implementing IComparable&lt;T&gt; and IEquatable&lt;T&gt;.</typeparam>
/// <param name="operator">The unary operator to apply (e.g., Negate, Factorial).</param>
/// <param name="operand">The expression that serves as the operand for the unary operation.</param>
public sealed class UnaryOperatorExpression<T>(
    UnaryOperators @operator,
    ExpressionBase<T> operand
        ) : ExpressionBase<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    private static readonly IExpressionEvaluator<T> _evaluator = ExpressionEvaluatorFactory.Create<T>();

    /// <summary>
    /// Gets the unary operator applied in this expression.
    /// </summary>
    public UnaryOperators Operator { get; } = @operator;

    /// <summary>
    /// Gets the operand expression that the unary operator is applied to.
    /// </summary>
    public ExpressionBase<T> Operand { get; } = operand;

    /// <summary>
    /// Creates a copy of this unary operator expression with a cloned operand.
    /// </summary>
    /// <returns>A new UnaryOperatorExpression instance with the same operator and a cloned operand.</returns>
    public override ExpressionBase<T> Clone() => new UnaryOperatorExpression<T>(Operator, Operand.Clone());

    /// <summary>
    /// Evaluates the unary operation by applying the operator to the evaluated operand.
    /// </summary>
    /// <param name="variables">The dictionary of variable names to values for evaluation.</param>
    /// <returns>The result of applying the unary operator to the operand's value.</returns>
    /// <exception cref="NotSupportedException">Thrown when the operator is not supported.</exception>
    public override T Evaluate(IDictionary<string, T> variables) =>
        Operator switch
        {
            Negate => _evaluator.Negate(Operand.Evaluate(variables)),
            Factorial => _evaluator.Factorial(Operand.Evaluate(variables)),

            _ => throw new NotSupportedException($"Operator {Operator} not supported")
        };

    private string OperandString =>
            Operand switch
            {
                BinaryOperatorExpression<T> _ => $"({Operand})",
                _ => $"{Operand}",
            };

    private string OperatorString => Operator.AsString();

    /// <summary>
    /// Returns the string representation of this unary operator expression.
    /// For right-associative operators (e.g., factorial), the format is "operand!" (e.g., "5!").
    /// For left-associative operators (e.g., negate), the format is "-operand" (e.g., "-5").
    /// </summary>
    /// <returns>A string representing the unary operation in appropriate operator notation.</returns>
    public override string ToString() =>
        Operator.IsRight() ?
            $"{OperandString}{OperatorString}" :
            $"{OperatorString}{OperandString}";
}
