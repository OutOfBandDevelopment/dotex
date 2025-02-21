using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OoBDev.System.Linq.Expressions;

/// <summary>
/// Extensions for System.Linq.Expressions.Expression
/// <seealso cref="Expression"/>
/// </summary>
public static class ExpressionExtensions
{

    /// <summary>
    /// Build an or'd expression chain
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="ors"></param>
    /// <returns></returns>
    public static Expression<Func<TModel, bool>>? OrElse<TModel>(
        this IEnumerable<Expression<Func<TModel, bool>>> ors
    ) => Chain(ors, ChainTypes.OrElse);

    /// <summary>
    /// Build an and'd expression chain
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="ands"></param>
    /// <returns></returns>
    public static Expression<Func<TModel, bool>>? AndAlso<TModel>(
        this IEnumerable<Expression<Func<TModel, bool>>?> ands
    ) => Chain(ands, ChainTypes.AndAlso);

    private static Expression<Func<TModel, bool>>? Chain<TModel>(
        IEnumerable<Expression<Func<TModel, bool>>?> expressions, ChainTypes type
    )
    {
        Expression? chain = null;

        foreach (var expression in expressions.Where(o => o != null))
        {
            chain = (chain, expression) switch
            {
                (_, null) => null,
                (null, _) => expression.Body,
                (_, _) => type switch
                {
                    ChainTypes.AndAlso => Expression.AndAlso(chain, expression.Body),
                    ChainTypes.OrElse => Expression.OrElse(chain, expression.Body),
                    _ => throw new NotSupportedException($"{type}")
                }
            };
        }

        if (chain == null) return null;

        var parameter = Expression.Parameter(typeof(TModel), "n");
        var replaced = new ParameterReplacerExpressionVisitor(parameter).Visit(chain);
        var lambda = Expression.Lambda<Func<TModel, bool>>(replaced, parameter);
        return lambda;
    }

    internal static IEnumerable<Attribute> GetAttributes(this Expression expression)
    {
        if (expression is MethodCallExpression methodCallExpression)
        {
            foreach (var argument in methodCallExpression.Arguments)
            {
                foreach (var attribute in argument.GetAttributes())
                {
                    yield return attribute;
                }
            }
        }
        else if (expression is BinaryExpression binaryExpression)
        {
            foreach (var attribute in binaryExpression.Left.GetAttributes())
            {
                yield return attribute;
            }
            foreach (var attribute in binaryExpression.Right.GetAttributes())
            {
                yield return attribute;
            }
        }
        else if (expression is MemberExpression memberExpression)
        {
            var memberInfo = memberExpression.Member;
            foreach (var attribute in Attribute.GetCustomAttributes(memberInfo))
            {
                yield return attribute;
            }
        }
        else if (expression is LambdaExpression lambdaExpression)
        {
            var children = lambdaExpression.Body.GetAttributes();
            foreach (var attribute in children)
            {
                yield return attribute;
            }
        }
    }
}
