using OoBDev.System.ExpressionCalculator.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.ExpressionCalculator.Visitors;

/// <summary>
/// Provides functionality to visit and modify expression trees by replacing variables with new names, constant values, or other expressions.
/// Implements the visitor pattern to traverse expression trees and perform variable substitution.
/// </summary>
/// <typeparam name="T">The numeric type used in the expression.</typeparam>
public class ExpressionVariableReplacementVistor<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Visits an expression tree and replaces variable names according to the provided mapping.
    /// </summary>
    /// <param name="expression">The expression to visit and modify.</param>
    /// <param name="variables">An enumerable of input-output variable name pairs specifying which variables to rename.</param>
    /// <returns>A new expression with variables renamed according to the mapping.</returns>
    public ExpressionBase<T> Visit(ExpressionBase<T> expression, IEnumerable<(string input, string output)> variables) =>
        expression switch
        {
            InnerExpression<T> inner => new InnerExpression<T>(
                Visit(inner.Expression, variables)
                ),
            UnaryOperatorExpression<T> unary => new UnaryOperatorExpression<T>(
                unary.Operator,
                Visit(unary.Operand, variables)
                ),
            BinaryOperatorExpression<T> binary => new BinaryOperatorExpression<T>(
                Visit(binary.Left, variables),
                binary.Operator,
                Visit(binary.Right, variables)
                ),

            VariableExpression<T> variable =>
                new VariableExpression<T>(variables.FirstOrDefault(v => v.input == variable.Name).output ?? variable.Name),

            _ => expression.Clone(),
        };

    /// <summary>
    /// Visits an expression tree and replaces variables with constant values, effectively pre-evaluating known variables.
    /// </summary>
    /// <param name="expression">The expression to visit and modify.</param>
    /// <param name="variables">An enumerable of variable name-value pairs specifying which variables to replace with constants.</param>
    /// <returns>A new expression with variables replaced by their constant values where matches are found.</returns>
    public ExpressionBase<T> Visit(ExpressionBase<T> expression, IEnumerable<(string name, T value)> variables) =>
        expression switch
        {
            InnerExpression<T> inner => new InnerExpression<T>(
                Visit(inner.Expression, variables)
                ),
            UnaryOperatorExpression<T> unary => new UnaryOperatorExpression<T>(
                unary.Operator,
                Visit(unary.Operand, variables)
                ),
            BinaryOperatorExpression<T> binary => new BinaryOperatorExpression<T>(
                Visit(binary.Left, variables),
                binary.Operator,
                Visit(binary.Right, variables)
                ),

            VariableExpression<T> variable => CheckVariable(variable, variables),

            _ => expression.Clone(),
        };

    /// <summary>
    /// Visits an expression tree and replaces variables with other expressions, allowing for expression composition and substitution.
    /// </summary>
    /// <param name="expression">The expression to visit and modify.</param>
    /// <param name="variables">An enumerable of variable name-expression pairs specifying which variables to replace with sub-expressions.</param>
    /// <returns>A new expression with variables replaced by the provided expressions where matches are found.</returns>
    public ExpressionBase<T> Visit(ExpressionBase<T> expression, IEnumerable<(string name, ExpressionBase<T> value)> variables) =>
        expression switch
        {
            InnerExpression<T> inner => new InnerExpression<T>(
                Visit(inner.Expression, variables)
                ),
            UnaryOperatorExpression<T> unary => new UnaryOperatorExpression<T>(
                unary.Operator,
                Visit(unary.Operand, variables)
                ),
            BinaryOperatorExpression<T> binary => new BinaryOperatorExpression<T>(
                Visit(binary.Left, variables),
                binary.Operator,
                Visit(binary.Right, variables)
                ),

            VariableExpression<T> variable => CheckVariable(variable, variables),

            _ => expression.Clone(),
        };

    private ExpressionBase<T> CheckVariable(VariableExpression<T> variable, IEnumerable<(string name, T value)> variables)
    {
        var value = (from v in variables
                     where variable.Name == v.name
                     select (T?)v.value).FirstOrDefault();
        return value.HasValue ?
            new NumberExpression<T>(value.Value) :
            variable.Clone();
    }
    private ExpressionBase<T> CheckVariable(VariableExpression<T> variable, IEnumerable<(string name, ExpressionBase<T> value)> variables) =>
         (from v in variables
          where variable.Name == v.name
          select v.value).FirstOrDefault() ?? variable.Clone();
}
