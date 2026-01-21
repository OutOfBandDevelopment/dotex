using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using OoBDev.System.ExpressionCalculator.Evaluators;
using OoBDev.System.ExpressionCalculator.Expressions;
using System;
using System.Linq;

namespace OoBDev.System.ExpressionCalculator.Parser;

/// <summary>
/// Implements the visitor pattern for traversing and converting ANTLR parse trees into expression objects.
/// This class handles the conversion of parsed tokens into strongly-typed expression tree nodes.
/// </summary>
/// <typeparam name="T">The numeric type for expression values.</typeparam>
public class ExpressionTreeVisitor<T> : ExpressionTreeBaseVisitor<ExpressionBase<T>>
    where T : struct, IComparable<T>, IEquatable<T>
{
    private static readonly IExpressionEvaluator<T> _evaluator = ExpressionEvaluatorFactory.Create<T>();

    /// <summary>
    /// Visits the start node of the parse tree, which represents the entry point for expression parsing.
    /// </summary>
    /// <param name="context">The parse tree context for the start rule.</param>
    /// <returns>The root expression node of the parsed expression tree.</returns>
    /// <exception cref="NotSupportedException">Thrown when no expression can be parsed from the context.</exception>
    public override ExpressionBase<T> VisitStart([NotNull] ExpressionTreeParser.StartContext context)
    {
        var entryPoint = Visit(context.expression()) ??
            throw new NotSupportedException($"No expression parsed: \"{context.GetText()}\"");
        EnsureChildCount(context, expected: "Expected <EOF>");
        return entryPoint;
    }

    /// <summary>
    /// Visits error nodes in the parse tree, throwing an exception for any syntax errors encountered.
    /// </summary>
    /// <param name="node">The error node encountered during parsing.</param>
    /// <returns>Never returns; always throws an exception.</returns>
    /// <exception cref="NotSupportedException">Always thrown to indicate a parse error.</exception>
    public override ExpressionBase<T> VisitErrorNode(IErrorNode node) =>
        throw new NotSupportedException(node.ToString());

    /// <summary>
    /// Visits expression nodes, creating binary operator expressions for operations like addition, subtraction, multiplication, etc.
    /// </summary>
    /// <param name="context">The parse tree context for the expression rule.</param>
    /// <returns>A binary operator expression if an operator is present; otherwise delegates to the base implementation.</returns>
    /// <exception cref="NotSupportedException">Thrown when left or right expressions are missing.</exception>
    public override ExpressionBase<T> VisitExpression([NotNull] ExpressionTreeParser.ExpressionContext context)
    {
        var op = context.@operator?.Text.AsBinaryOperators();
        return op.HasValue && op.Value != BinaryOperators.Unknown
            ? new BinaryOperatorExpression<T>(
                Visit(context.left) ?? throw new NotSupportedException($"Missing Left Expression: {context.GetText()}"),
                op.Value,
                Visit(context.right) ?? throw new NotSupportedException($"Missing Right Expression: {context.GetText()}")
                )
            : base.VisitExpression(context);
    }

    /// <summary>
    /// Visits inner expression nodes (parenthesized expressions), creating an inner expression wrapper.
    /// </summary>
    /// <param name="context">The parse tree context for the inner expression rule.</param>
    /// <returns>An inner expression containing the nested expression.</returns>
    public override ExpressionBase<T> VisitInnerExpression([NotNull] ExpressionTreeParser.InnerExpressionContext context) =>
        new InnerExpression<T>(Visit(context.inner));

    /// <summary>
    /// Visits value nodes, which represent either numeric constants or variable references.
    /// </summary>
    /// <param name="context">The parse tree context for the value rule.</param>
    /// <returns>Either a number expression or a variable expression depending on the value type.</returns>
    /// <exception cref="NotSupportedException">Thrown when the value cannot be parsed or has an unexpected child count.</exception>
    public override ExpressionBase<T> VisitValue([NotNull] ExpressionTreeParser.ValueContext context)
    {
        var result = VisitNumber(context.NUMBER()) ??
         VisitVariable(context.VARIABLE()) ??
             throw new NotSupportedException($"Unable to parse \"{context.GetText()}\"")
         ;
        EnsureChildCount(context, expected: $"Expected {nameof(context.NUMBER)}|{nameof(context.VARIABLE)}", childCount: 1);

        return result;
    }

    private ExpressionBase<T>? VisitVariable(ITerminalNode node) =>
        node != null ? new VariableExpression<T>(node.GetText()) : null;

    private ExpressionBase<T>? VisitNumber(ITerminalNode node) =>
        node != null ? new NumberExpression<T>(
            _evaluator.TryParse(node.GetText()) ??
            throw new NotSupportedException($"Unable to parse \"{node.GetText()}\" to type \"{typeof(T)}\"")
            ) : null;

    /// <summary>
    /// Visits unary operator expressions where the operator appears on the left side (e.g., -x, +x).
    /// </summary>
    /// <param name="context">The parse tree context for the left unary operator expression rule.</param>
    /// <returns>A unary operator expression with the operator applied to its operand.</returns>
    /// <exception cref="NotSupportedException">Thrown when the operand is missing or the child count is incorrect.</exception>
    public override ExpressionBase<T> VisitUnaryOperatorLeftExpression([NotNull] ExpressionTreeParser.UnaryOperatorLeftExpressionContext context)
    {
        var result = new UnaryOperatorExpression<T>(
            context.@operator.Text.AsUnaryOperator(),
            ChainVisit(context.value(), context.innerExpression(), context.unaryOperatorLeftExpression())
            );
        EnsureChildCount(context, expected: $"Expected {nameof(context.value)}|{nameof(context.innerExpression)}|{nameof(context.unaryOperatorLeftExpression)}");
        return result;
    }

    /// <summary>
    /// Visits unary operator expressions where the operator appears on the right side (e.g., x!).
    /// </summary>
    /// <param name="context">The parse tree context for the right unary operator expression rule.</param>
    /// <returns>A unary operator expression with the operator applied to its operand.</returns>
    /// <exception cref="NotSupportedException">Thrown when the operand is missing or the child count is incorrect.</exception>
    public override ExpressionBase<T> VisitUnaryOperatorRightExpression([NotNull] ExpressionTreeParser.UnaryOperatorRightExpressionContext context)
    {
        var result = new UnaryOperatorExpression<T>(
               context.@operator.Text.AsUnaryOperator(),
               ChainVisit(context.value(), context.innerExpression(), context.unaryOperatorRightExpression())
               );
        EnsureChildCount(context, expected: $"Expected {nameof(context.value)}|{nameof(context.innerExpression)}|{nameof(context.unaryOperatorRightExpression)}");
        return result;
    }

    private ExpressionBase<T> ChainVisit(params IParseTree[] nodes) =>
        Visit(nodes.FirstOrDefault(n => n != null) ?? throw new NotSupportedException($"No non-null node provided"));

    private TParserRuleContext EnsureChildCount<TParserRuleContext>(TParserRuleContext context, string? expected = null, int childCount = 2)
        where TParserRuleContext : ParserRuleContext
    {
        if (context.children.Count != childCount)
        {
            var extraChildren = context.children.Skip(1).Take(context.children.Count - childCount);
            var extras = string.Join(";", extraChildren.Select(c => c.GetText()));

            if (string.IsNullOrWhiteSpace(extras))
            {
                throw new NotSupportedException($"Missing Expression");
            }
            else
            {
                throw new NotSupportedException(string.Join(", ", new[] { expected, $"Found: {extras}" }.Where(s => !string.IsNullOrWhiteSpace(s))));
            }
        }
        return context;
    }
}