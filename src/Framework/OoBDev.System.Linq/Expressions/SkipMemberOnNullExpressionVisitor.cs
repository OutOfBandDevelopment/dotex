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
