using System.Linq.Expressions;

namespace OoBDev.System.Linq.Expressions;

internal class ParameterReplacerExpressionVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _parameter;

    internal ParameterReplacerExpressionVisitor(ParameterExpression parameter) =>
        _parameter = parameter;

    protected override Expression VisitParameter(ParameterExpression node) =>
        _parameter.Type == node.Type ? _parameter : node;
}
