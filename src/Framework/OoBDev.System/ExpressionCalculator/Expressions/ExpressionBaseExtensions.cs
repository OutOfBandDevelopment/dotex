﻿using OoBDev.System.ExpressionCalculator.Evaluators;
using OoBDev.System.ExpressionCalculator.Optimizers;
using OoBDev.System.ExpressionCalculator.Parser;
using OoBDev.System.ExpressionCalculator.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.ExpressionCalculator.Expressions;

public static class ExpressionBaseExtensions
{
    public static ExpressionBase<T> Optimize<T>(this ExpressionBase<T> expression)
        where T : struct, IComparable<T>, IEquatable<T> =>
            new ExpressionOptimizationProvider<T>().Optimize(expression);

    public static IDictionary<string, T> EmptySet<T>()
        where T : struct, IComparable<T>, IEquatable<T> =>
            new Dictionary<string, T>();

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

    public static T Evaluate<T>(this ExpressionBase<T> expression, IEnumerable<(string name, T value)> variables)
        where T : struct, IComparable<T>, IEquatable<T> =>
        expression.Evaluate(variables.ToDictionary(k => k.name, v => v.value));
    public static T Evaluate<T>(this ExpressionBase<T> expression, params (string name, T value)[] variables)
        where T : struct, IComparable<T>, IEquatable<T> => expression.Evaluate(variables.AsEnumerable());

    public static IEnumerable<string> GetDistinctVariableNames<T>(this ExpressionBase<T> expression)
        where T : struct, IComparable<T>, IEquatable<T> =>
        expression.GetAllExpressions()
                  .OfType<VariableExpression<T>>()
                  .Select(s => s.Name)
                  .Distinct();

    public static IDictionary<string, T> GenerateTestValues<T>(this ExpressionBase<T> expression, int scale = 4, bool includeNegatives = false, int places = 2)
        where T : struct, IComparable<T>, IEquatable<T>
    {
        var evaluator = ExpressionEvaluatorFactory.Create<T>();

        var variableNames = expression.GetDistinctVariableNames();
        var rand = new Random();

        var variables = new Dictionary<string, T>();
        foreach (var variableName in variableNames)
        {
            var randomValue = Math.Round(rand.NextDouble() * Math.Pow(10, scale) * (includeNegatives && rand.Next() % 2 == 0 ? -1 : 1), places);
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

    public static ExpressionBase<T> ParseAsExpression<T>(this string input)
        where T : struct, IComparable<T>, IEquatable<T> =>
        new ExpressionParser<T>().Parse(input);

    public static ExpressionBase<T> ReplaceVariables<T>(this ExpressionBase<T> expression, IEnumerable<(string input, string output)> variables)
        where T : struct, IComparable<T>, IEquatable<T> =>
        new ExpressionVariableReplacementVistor<T>().Visit(expression, variables);

    public static ExpressionBase<T> ReplaceVariables<T>(this ExpressionBase<T> expression, params (string input, string output)[] variables)
        where T : struct, IComparable<T>, IEquatable<T> => expression.ReplaceVariables(variables.AsEnumerable());

    public static ExpressionBase<T> PreEvaluate<T>(this ExpressionBase<T> expression, IEnumerable<(string name, T value)> variables)
        where T : struct, IComparable<T>, IEquatable<T> =>
        new ExpressionVariableReplacementVistor<T>().Visit(expression, variables);
    public static ExpressionBase<T> PreEvaluate<T>(this ExpressionBase<T> expression, params (string name, T value)[] variables)
        where T : struct, IComparable<T>, IEquatable<T> => expression.PreEvaluate(variables.AsEnumerable());

    public static ExpressionBase<T> PreEvaluate<T>(this ExpressionBase<T> expression, IEnumerable<(string name, ExpressionBase<T> value)> variables)
        where T : struct, IComparable<T>, IEquatable<T> =>
        variables.Aggregate(expression, (exp, v) => new ExpressionVariableReplacementVistor<T>().Visit(exp, new[] { v }));
    public static ExpressionBase<T> PreEvaluate<T>(this ExpressionBase<T> expression, params (string name, ExpressionBase<T> value)[] variables)
        where T : struct, IComparable<T>, IEquatable<T> => expression.PreEvaluate(variables.AsEnumerable());

    public static ExpressionBase<T> PreEvaluate<T>(this string expression, IEnumerable<(string name, ExpressionBase<T> value)> variables)
        where T : struct, IComparable<T>, IEquatable<T> => ((ExpressionBase<T>)expression).PreEvaluate(variables);
    public static ExpressionBase<T> PreEvaluate<T>(this string expression, params (string name, ExpressionBase<T> value)[] variables)
        where T : struct, IComparable<T>, IEquatable<T> => ((ExpressionBase<T>)expression).PreEvaluate(variables);

    public static ExpressionBase<decimal> PreEvaluate(this string expression, IEnumerable<(string name, ExpressionBase<decimal> value)> variables) =>
        ((ExpressionBase<decimal>)expression).PreEvaluate(variables);
    public static ExpressionBase<decimal> PreEvaluate(this string expression, params (string name, ExpressionBase<decimal> value)[] variables) =>
        ((ExpressionBase<decimal>)expression).PreEvaluate(variables);
}
