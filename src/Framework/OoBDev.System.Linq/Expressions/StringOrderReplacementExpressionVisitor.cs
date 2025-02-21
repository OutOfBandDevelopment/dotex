using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OoBDev.System.Linq.Expressions;

/// <summary>
/// A custom expression visitor that replaces string casing in OrderBy expressions 
/// within LINQ queries based on the specified <see cref="StringCasing"/>.
/// </summary>
/// <param name="stringCasing">Determines whether to convert strings to uppercase or lowercase in sorting operations.</param>
/// <param name="logger">Optional logger to capture debug information.</param>
public class StringOrderReplacementExpressionVisitor(
#if DEBUG
    StringCasing stringCasing,
#else
    StringCasing stringCasing = StringCasing.Upper,
#endif
    ILogger<StringOrderReplacementExpressionVisitor>? logger = null
    ) : ExpressionVisitor, IPostBuildExpressionVisitor
{
    private readonly ILogger _logger = logger ?? new ConsoleLogger<StringOrderReplacementExpressionVisitor>();

    private static readonly string[] TargetMethodNames = [
        nameof(Queryable.OrderBy),
        nameof(Queryable.OrderByDescending),
        nameof(Queryable.ThenBy),
        nameof(Queryable.ThenByDescending),
    ];
    private static readonly MethodInfo[] TargetedMethods =
        typeof(Queryable)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .Where(m => TargetMethodNames.Contains(m.Name))
        .Where(m => m.GetParameters().Length == 2)
        .ToArray()
        ;
    private readonly RewriteChildren Rewriter = new RewriteChildren(logger, stringCasing);

    /// <summary>
    /// Visits method call expressions, rewriting them to adjust string casing 
    /// for order-by operations in LINQ queries.
    /// </summary>
    /// <param name="node">The method call expression being visited.</param>
    /// <returns>The modified expression if applicable, otherwise the original expression.</returns>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (stringCasing != StringCasing.Default && TargetMethodNames.Contains(node.Method.Name) &&
            ((node.Arguments[1] as UnaryExpression)?.Operand as LambdaExpression)?.Body.NodeType != ExpressionType.Call &&
            ((node.Arguments[1] as UnaryExpression)?.Operand as LambdaExpression)?.Body.Type == typeof(string)
            )
        {
            _logger.LogInformation("Rewrite VisitMethodCall: {method}({args}) -> {node} / {nt}", node.Method.Name, node.Arguments.Count, node, node.Arguments[1].Type);

            var methodInfo = TargetedMethods.Single(m => m.Name == node.Method.Name).MakeGenericMethod(node.Method.GetGenericArguments());
            node = Expression.Call(methodInfo, node.Arguments[0], Rewriter.Visit(node.Arguments[1]));
        }
        return base.VisitMethodCall(node);
    }

    internal class RewriteChildren(
        ILogger? logger,
        StringCasing stringCasing
        ) : ExpressionVisitor
    {
        private readonly ILogger _logger = logger ?? new ConsoleLogger<RewriteChildren>();

        private readonly static MethodInfo ToUpperInfo = typeof(string).GetMethod(nameof(string.ToUpper), BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes)!;
        private readonly static MethodInfo ToLowerInfo = typeof(string).GetMethod(nameof(string.ToLower), BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes)!;

        public static MethodInfo GetMethod(StringCasing stringCasing) => stringCasing switch
        {
            StringCasing.Upper => ToUpperInfo,
            _ => ToLowerInfo,
        };

        protected override Expression VisitMember(MemberExpression node)
        {
            if (stringCasing != StringCasing.Default && node.Type == typeof(string))
            {
                _logger.LogInformation("Rewriting VisitMember: {method} [{args}] -> {node}", node.Member, node.Expression, node);

                var newNode = Expression.Call(node, GetMethod(stringCasing));
                return newNode;
            }
            return base.VisitMember(node);
        }
    }
}
