Create a readme file describing the general functionality over the follow files.

Generate a summary that includes a description of the related functionality.
In a technical summary define any design patterns or achitectural patterns described by the files.
Generate a component diagrams using plantuml.
PlantUML blocks must all be identified with "```plantuml" and closed with "```"

## Source Files

```SkipInstanceMethodOnNullExpressionVisitor.cs
using System.Linq.Expressions;

namespace OoBDev.System.Linq.Expressions;

/// <summary>
/// This visitor will modify expressions to add `x.Property != null` before instance method calls for query rewrite
/// </summary>
public class SkipInstanceMethodOnNullExpressionVisitor : ExpressionVisitor, IPostBuildExpressionVisitor
{
    /// <summary>
    /// If expression is an instance method then modify the expression to ensure a 
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        return node.Object == null || node.Method.IsStatic
            ? base.VisitMethodCall(node)
            : Expression.AndAlso(
            Expression.NotEqual(node.Object, Expression.Constant(null, node.Object.Type)),
            node
            );
    }
}

```

```SkipMemberOnNullExpressionVisitor.cs
using System.Linq;
using System.Linq.Expressions;

namespace OoBDev.System.Linq.Expressions;

/// <summary>
/// Represents a visitor for expression trees that skips member access when encountering null values.
/// </summary>
public class SkipMemberOnNullExpressionVisitor : ExpressionVisitor, IPostBuildExpressionVisitor
{
    /// <summary>
    /// Visits the method call expression node and modifies it to skip member access if the target object is null.
    /// </summary>
    /// <param name="node">The method call expression to visit.</param>
    /// <returns>The modified expression.</returns>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method?.ReturnType == typeof(bool))
        {
            if (!node.Method.IsStatic && node.Object != null)
            {
                var obj = base.Visit(node.Object);
                var exp = Expression.AndAlso(
                     Expression.NotEqual(obj, Expression.Constant(null, obj.Type)),
                     node);
                return exp;
            }
            else if (node.Method.IsStatic && node.Object == null && node.Arguments.Count > 0)
            {
                var arg = base.Visit(node.Arguments[0]);
                var exp = Expression.AndAlso(
                     Expression.NotEqual(arg, Expression.Constant(null, arg.Type)),
                     node);
                return exp;
            }
            else
            {

            }
        }
        return base.VisitMethodCall(node);
    }

    /// <summary>
    /// Visits the method call expression node and modifies it to skip member access if the target object is null.
    /// </summary>
    /// <param name="node">The method call expression to visit.</param>
    /// <returns>The modified expression.</returns>
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        if (node.ReturnType != typeof(bool)) return base.VisitLambda<T>(node);

        var body = base.Visit(node.Body);

        if (body is MethodCallExpression method)
        {
            if (!method.Method.IsStatic && method.Object != null)
            {
                return Expression.Lambda(
                    Expression.AndAlso(
                        Expression.NotEqual(method.Object, Expression.Constant(null, method.Object.Type)),
                        body),
                    node.Parameters);
            }
        }
        else if (node.Parameters.FirstOrDefault() is not null)
        {
            return Expression.Lambda(
                Expression.AndAlso(
                    Expression.NotEqual(node.Parameters.First(), Expression.Constant(null, node.Parameters.First().Type)),
                    body),
                node.Parameters);
        }
        return base.VisitLambda(node);
    }
}

```

```StringComparisonReplacementExpressionVisitor.cs
using OoBDev.System.ComponentModel.Search;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace OoBDev.System.Linq.Expressions;

/// <summary>
/// Expression visitor to replace string functions with the matching 
/// functions that end with a StringComparison parameter
/// </summary>
public class StringComparisonReplacementExpressionVisitor(
    ILogger<StringComparisonReplacementExpressionVisitor>? logger = null,
    StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase
        ) : ExpressionVisitor, IPostBuildExpressionVisitor
{
    private readonly ILogger _logger = logger ?? new ConsoleLogger<StringComparisonReplacementExpressionVisitor>();

    /// <summary>
    /// Replace `string == string` with `string.Equals(string, StringComparison)`
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        var declaringType = node.Method?.DeclaringType;
        if (declaringType != typeof(string))
            goto done;

        if (!(node.Method?.IsSpecialName ?? false) || node.Method?.Name != "op_Equality")
            goto done;

        var typeArgs = new[]
        {
            typeof(string),
            typeof(StringComparison),
        };

        var method = declaringType.GetMethod(nameof(String.Equals), typeArgs);
        if (method == null)
            goto done;

        var replacement = Expression.Call(node.Left, method, node.Right, Expression.Constant(stringComparison));

        return replacement;

    done:
        return base.VisitBinary(node);
    }

    /// <summary>
    /// Replace `string.Xyz(string)` with `string.Xyz(string, StringComparison)`
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    protected override Expression VisitMethodCall(MethodCallExpression input)
    {
        var declaringType = input.Method.DeclaringType;
        if (declaringType != typeof(string))
            goto done;

        var parameters = input.Method.GetParameters();
        if (parameters.LastOrDefault()?.ParameterType == typeof(StringComparison))
            goto done;

        var typeArgs = new Type[parameters.Length + 1];
        for (var idx = 0; idx < parameters.Length; idx++)
            typeArgs[idx] = parameters[idx].ParameterType;
        typeArgs[parameters.Length] = typeof(StringComparison);

        var method = declaringType.GetMethod(input.Method.Name, typeArgs);
        if (method == null)
            goto done;

        if (input.Object?.GetAttributes().OfType<IgnoreStringComparisonReplacementAttribute>().Any() ?? false) goto done;

        var args = input.Arguments.Concat(new[] { Expression.Constant(stringComparison) });
        var replacement = Expression.Call(input.Object, method, args);

        _logger.LogDebug($"Replace: {{{nameof(input)}}} with {{{nameof(replacement)}}}", input, replacement);

        return replacement;

    done:
        return base.VisitMethodCall(input);
    }
}

```

