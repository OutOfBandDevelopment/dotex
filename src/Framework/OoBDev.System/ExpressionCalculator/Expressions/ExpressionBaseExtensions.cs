using OoBDev.System.ExpressionCalculator.Evaluators;
using OoBDev.System.ExpressionCalculator.Optimizers;
using OoBDev.System.ExpressionCalculator.Parser;
using OoBDev.System.ExpressionCalculator.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.ExpressionCalculator.Expressions;

/// <summary>
/// Provides extension methods for working with expression objects, including optimization, evaluation, and variable manipulation.
/// </summary>
public static class ExpressionBaseExtensions
{
    /// <summary>
    /// Optimizes the expression by applying various optimization strategies such as constant folding and algebraic simplification.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The expression to optimize.</param>
    /// <returns>An optimized version of the expression.</returns>
    public static ExpressionBase<T> Optimize<T>(this ExpressionBase<T> expression)
        where T : struct, IComparable<T>, IEquatable<T> =>
            new ExpressionOptimizationProvider<T>().Optimize(expression);

    /// <summary>
    /// Creates an empty variable set for use in expression evaluation.
    /// </summary>
    /// <typeparam name="T">The numeric type for the variable values.</typeparam>
    /// <returns>An empty dictionary for storing variable name-value pairs.</returns>
    public static IDictionary<string, T> EmptySet<T>()
        where T : struct, IComparable<T>, IEquatable<T> =>
            new Dictionary<string, T>();

    /// <summary>
    /// Recursively retrieves all sub-expressions from an expression tree, including the root expression.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The root expression to traverse.</param>
    /// <returns>An enumerable of all expressions in the expression tree.</returns>
    public static IEnumerable<ExpressionBase<T>> GetAllExpressions<T>(this ExpressionBase<T> expression)
        where T : struct, IComparable<T>, IEquatable<T>
    {
        yield return expression;

        var subExpressions = expression switch
        {
            InnerExpression<T> inner => inner.Expression.GetAllExpressions(),
            UnaryOperatorExpression<T> unary => unary.Operand.GetAllExpressions(),
            BinaryOperatorExpression<T> binary => binary.Left.GetAllExpressions().Concat(binary.Right.GetAllExpressions()),
            _ => []
        };

        foreach (var sub in subExpressions)
            yield return sub;
    }

    /// <summary>
    /// Evaluates the expression using variable values provided as an enumerable of name-value tuples.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="variables">An enumerable of variable name-value pairs.</param>
    /// <returns>The result of evaluating the expression with the provided variables.</returns>
    public static T Evaluate<T>(this ExpressionBase<T> expression, IEnumerable<(string name, T value)> variables)
        where T : struct, IComparable<T>, IEquatable<T> =>
        expression.Evaluate(variables.ToDictionary(k => k.name, v => v.value));

    /// <summary>
    /// Evaluates the expression using variable values provided as a parameter array of name-value tuples.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="variables">A parameter array of variable name-value pairs.</param>
    /// <returns>The result of evaluating the expression with the provided variables.</returns>
    public static T Evaluate<T>(this ExpressionBase<T> expression, params (string name, T value)[] variables)
        where T : struct, IComparable<T>, IEquatable<T> => expression.Evaluate(variables.AsEnumerable());

    /// <summary>
    /// Gets all distinct variable names used in the expression tree.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The expression to analyze.</param>
    /// <returns>An enumerable of distinct variable names found in the expression.</returns>
    public static IEnumerable<string> GetDistinctVariableNames<T>(this ExpressionBase<T> expression)
        where T : struct, IComparable<T>, IEquatable<T> =>
        expression.GetAllExpressions()
                  .OfType<VariableExpression<T>>()
                  .Select(s => s.Name)
                  .Distinct();

    /// <summary>
    /// Generates random test values for all variables in the expression, useful for testing and validation.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The expression for which to generate test values.</param>
    /// <param name="scale">The scale factor for random value generation (default is 4, producing values up to 10^4).</param>
    /// <param name="includeNegatives">Whether to include negative values in the generated test set (default is false).</param>
    /// <param name="places">The number of decimal places for rounding (default is 2).</param>
    /// <returns>A dictionary mapping variable names to randomly generated values.</returns>
    public static IDictionary<string, T> GenerateTestValues<T>(this ExpressionBase<T> expression, int scale = 4, bool includeNegatives = false, int places = 2)
        where T : struct, IComparable<T>, IEquatable<T>
    {
        var evaluator = ExpressionEvaluatorFactory.Create<T>();

        var variableNames = expression.GetDistinctVariableNames();
        var rand = new Random();

        var variables = new Dictionary<string, T>();
        foreach (var variableName in variableNames)
        {
            var randomValue = global::System.Math.Round(rand.NextDouble() * global::System.Math.Pow(10, scale) * (includeNegatives && rand.Next() % 2 == 0 ? -1 : 1), places);
            if (randomValue == 0) randomValue += 0.0000000001d;
            var value = evaluator.GetValue(randomValue);
            if (value is uint ui && ui == 0) value = (T)(object)(uint)2;
            else if (value is ulong ul && ul == 0) value = (T)(object)(ulong)2;
            else if (value is ushort us && us == 0) value = (T)(object)(ushort)2;
            else if (value is byte b && b == 0) value = (T)(object)(byte)2;
            variables.Add(variableName, value);
        }
        return variables;
    }

    /// <summary>
    /// Parses a string into an expression of the specified numeric type.
    /// </summary>
    /// <typeparam name="T">The numeric type for the resulting expression.</typeparam>
    /// <param name="input">The string representation of the expression to parse.</param>
    /// <returns>An expression object representing the parsed input.</returns>
    public static ExpressionBase<T> ParseAsExpression<T>(this string input)
        where T : struct, IComparable<T>, IEquatable<T> =>
        new ExpressionParser<T>().Parse(input);

    /// <summary>
    /// Replaces variable names in an expression with different variable names.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The expression in which to replace variables.</param>
    /// <param name="variables">An enumerable of input-output variable name pairs.</param>
    /// <returns>A new expression with the specified variables renamed.</returns>
    public static ExpressionBase<T> ReplaceVariables<T>(this ExpressionBase<T> expression, IEnumerable<(string input, string output)> variables)
        where T : struct, IComparable<T>, IEquatable<T> =>
        new ExpressionVariableReplacementVistor<T>().Visit(expression, variables);

    /// <summary>
    /// Replaces variable names in an expression with different variable names.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The expression in which to replace variables.</param>
    /// <param name="variables">A parameter array of input-output variable name pairs.</param>
    /// <returns>A new expression with the specified variables renamed.</returns>
    public static ExpressionBase<T> ReplaceVariables<T>(this ExpressionBase<T> expression, params (string input, string output)[] variables)
        where T : struct, IComparable<T>, IEquatable<T> => expression.ReplaceVariables(variables.AsEnumerable());

    /// <summary>
    /// Pre-evaluates an expression by replacing variables with their constant values, producing a simplified expression.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The expression to pre-evaluate.</param>
    /// <param name="variables">An enumerable of variable name-value pairs to substitute.</param>
    /// <returns>A new expression with the specified variables replaced by their values.</returns>
    public static ExpressionBase<T> PreEvaluate<T>(this ExpressionBase<T> expression, IEnumerable<(string name, T value)> variables)
        where T : struct, IComparable<T>, IEquatable<T> =>
        new ExpressionVariableReplacementVistor<T>().Visit(expression, variables);

    /// <summary>
    /// Pre-evaluates an expression by replacing variables with their constant values, producing a simplified expression.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The expression to pre-evaluate.</param>
    /// <param name="variables">A parameter array of variable name-value pairs to substitute.</param>
    /// <returns>A new expression with the specified variables replaced by their values.</returns>
    public static ExpressionBase<T> PreEvaluate<T>(this ExpressionBase<T> expression, params (string name, T value)[] variables)
        where T : struct, IComparable<T>, IEquatable<T> => expression.PreEvaluate(variables.AsEnumerable());

    /// <summary>
    /// Pre-evaluates an expression by replacing variables with other expressions, allowing for expression composition.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The expression to pre-evaluate.</param>
    /// <param name="variables">An enumerable of variable name-expression pairs to substitute.</param>
    /// <returns>A new expression with the specified variables replaced by the provided expressions.</returns>
    public static ExpressionBase<T> PreEvaluate<T>(this ExpressionBase<T> expression, IEnumerable<(string name, ExpressionBase<T> value)> variables)
        where T : struct, IComparable<T>, IEquatable<T> =>
        variables.Aggregate(expression, (exp, v) => new ExpressionVariableReplacementVistor<T>().Visit(exp, new[] { v }));

    /// <summary>
    /// Pre-evaluates an expression by replacing variables with other expressions, allowing for expression composition.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The expression to pre-evaluate.</param>
    /// <param name="variables">A parameter array of variable name-expression pairs to substitute.</param>
    /// <returns>A new expression with the specified variables replaced by the provided expressions.</returns>
    public static ExpressionBase<T> PreEvaluate<T>(this ExpressionBase<T> expression, params (string name, ExpressionBase<T> value)[] variables)
        where T : struct, IComparable<T>, IEquatable<T> => expression.PreEvaluate(variables.AsEnumerable());

    /// <summary>
    /// Parses a string as an expression and pre-evaluates it by replacing variables with other expressions.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The string representation of the expression to parse and pre-evaluate.</param>
    /// <param name="variables">An enumerable of variable name-expression pairs to substitute.</param>
    /// <returns>A pre-evaluated expression with the specified variables replaced.</returns>
    public static ExpressionBase<T> PreEvaluate<T>(this string expression, IEnumerable<(string name, ExpressionBase<T> value)> variables)
        where T : struct, IComparable<T>, IEquatable<T> => ((ExpressionBase<T>)expression).PreEvaluate(variables);

    /// <summary>
    /// Parses a string as an expression and pre-evaluates it by replacing variables with other expressions.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the expression.</typeparam>
    /// <param name="expression">The string representation of the expression to parse and pre-evaluate.</param>
    /// <param name="variables">A parameter array of variable name-expression pairs to substitute.</param>
    /// <returns>A pre-evaluated expression with the specified variables replaced.</returns>
    public static ExpressionBase<T> PreEvaluate<T>(this string expression, params (string name, ExpressionBase<T> value)[] variables)
        where T : struct, IComparable<T>, IEquatable<T> => ((ExpressionBase<T>)expression).PreEvaluate(variables);

    /// <summary>
    /// Parses a string as a decimal expression and pre-evaluates it by replacing variables with other expressions.
    /// </summary>
    /// <param name="expression">The string representation of the expression to parse and pre-evaluate.</param>
    /// <param name="variables">An enumerable of variable name-expression pairs to substitute.</param>
    /// <returns>A pre-evaluated decimal expression with the specified variables replaced.</returns>
    public static ExpressionBase<decimal> PreEvaluate(this string expression, IEnumerable<(string name, ExpressionBase<decimal> value)> variables) =>
        ((ExpressionBase<decimal>)expression).PreEvaluate(variables);

    /// <summary>
    /// Parses a string as a decimal expression and pre-evaluates it by replacing variables with other expressions.
    /// </summary>
    /// <param name="expression">The string representation of the expression to parse and pre-evaluate.</param>
    /// <param name="variables">A parameter array of variable name-expression pairs to substitute.</param>
    /// <returns>A pre-evaluated decimal expression with the specified variables replaced.</returns>
    public static ExpressionBase<decimal> PreEvaluate(this string expression, params (string name, ExpressionBase<decimal> value)[] variables) =>
        ((ExpressionBase<decimal>)expression).PreEvaluate(variables);
}
