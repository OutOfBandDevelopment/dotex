using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using OoBDev.System.PathSegments;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OoBDev.System.Text.Json.JsonPath.Parser;

/// <summary>
/// Implements the visitor pattern for traversing and converting JSON Path parse trees into path segment objects.
/// This class handles the conversion of parsed JSON Path expressions into strongly-typed path segment structures.
/// </summary>
public class JsonPathVisitor : JsonPathBaseVisitor<IPathSegment?>
{
    /// <summary>
    /// Visits the start node of the JSON Path parse tree, which represents the entry point for path parsing.
    /// </summary>
    /// <param name="context">The parse tree context for the start rule.</param>
    /// <returns>The root path segment of the parsed JSON Path expression.</returns>
    /// <exception cref="JsonPathException">Thrown when no path is defined in the context.</exception>
    public override IPathSegment VisitStart([NotNull] JsonPathParser.StartContext context) =>
        Visit(context.path()) ?? throw new JsonPathException("no path defined");

    /// <summary>
    /// Visits a path node, creating either a binary path segment or a function path segment depending on the structure.
    /// </summary>
    /// <param name="context">The parse tree context for the path rule.</param>
    /// <returns>A path segment representing the parsed path expression.</returns>
    /// <exception cref="JsonPathException">Thrown when required path components are missing.</exception>
    public override IPathSegment VisitPath([NotNull] JsonPathParser.PathContext context) =>
        Visit(context.function()) switch
        {
            null => new BinaryPathSegment(
            Visit<PathBaseTypes>(context.pathBase()) ?? throw new JsonPathException("missing pathBase"),
            Visit(context.sequence()) ?? throw new JsonPathException("missing path sequence")
            ),
            IPathSegment function => function
        };
    /// <summary>
    /// Visits a path base node, which represents the root ($) or relative (@) starting point of a JSON Path expression.
    /// </summary>
    /// <param name="context">The parse tree context for the path base rule.</param>
    /// <returns>A path segment representing the path base type (root or relative).</returns>
    public override IPathSegment? VisitPathBase([NotNull] JsonPathParser.PathBaseContext context) =>
        Visit<PathBaseTypes>(context.ROOT(), context.RELATIVE());

    /// <summary>
    /// Visits an identity node, which represents a named identifier in the path.
    /// </summary>
    /// <param name="context">The parse tree context for the identity rule.</param>
    /// <returns>A path segment representing the identity.</returns>
    public override IPathSegment? VisitIdentity([NotNull] JsonPathParser.IdentityContext context) =>
        Visit(context.IDENTITY());

    /// <summary>
    /// Visits an operand node, which can be a path, string, or numeric value used in filter expressions.
    /// </summary>
    /// <param name="context">The parse tree context for the operand rule.</param>
    /// <returns>A path segment representing the operand value.</returns>
    public override IPathSegment? VisitOperand([NotNull] JsonPathParser.OperandContext context) =>
        Visit(
            context.path(),
            context.@string()
            ) ??
            Visit(context.NUMBER()) switch
            {
                null => null,
                NumericPathSegment number => new DecimalPathSegment(number.Value),
                IPathSegment passThough => passThough
            };
    /// <summary>
    /// Visits a string node, creating a path segment from a quoted string literal.
    /// </summary>
    /// <param name="context">The parse tree context for the string rule.</param>
    /// <returns>A path segment representing the quoted string value.</returns>
    public override IPathSegment? VisitString([NotNull] JsonPathParser.StringContext context) =>
        Visit(context.QUOTED_STRING());

    /// <summary>
    /// Visits a sequence item node, which can be a wildcard, identity, bracket expression, filter, function, or descendants operator.
    /// </summary>
    /// <param name="context">The parse tree context for the sequence item rule.</param>
    /// <returns>A path segment representing the sequence item.</returns>
    public override IPathSegment? VisitSequenceItem([NotNull] JsonPathParser.SequenceItemContext context) =>
        Visit(
            context.WILDCARD(),
            context.identity(),
            context.bracket(),
            context.filter(),
            context.function(),
            context.DESCENDANTS()
            );
    /// <summary>
    /// Visits a sequence node, creating a chain of path segments by recursively processing sequence items.
    /// </summary>
    /// <param name="context">The parse tree context for the sequence rule.</param>
    /// <returns>A path segment representing the sequence, possibly as a binary path segment chain.</returns>
    /// <exception cref="JsonPathException">Thrown when no path segment is defined in the sequence.</exception>
    public override IPathSegment VisitSequence([NotNull] JsonPathParser.SequenceContext context) =>
        Visit(context.sequenceItem()) switch
        {
            null => throw new JsonPathException("no path segment defined"),
            IPathSegment left => Visit(context.sequence()) switch
            {
                null => left,
                IPathSegment right => new BinaryPathSegment(left, right)
            }
        };

    /// <summary>
    /// Visits a bracket node, creating an indexer path segment for array/object access expressions like [0], [*], ['key'].
    /// </summary>
    /// <param name="context">The parse tree context for the bracket rule.</param>
    /// <returns>An indexer path segment representing the bracket expression.</returns>
    /// <exception cref="JsonPathException">Thrown when the bracket content is invalid.</exception>
    public override IPathSegment VisitBracket([NotNull] JsonPathParser.BracketContext context) =>
        new IndexerPathSegment(
            Visit(context.WILDCARD(), context.range(), context.function()) ??
            Visit(context.NUMBER()) ??
            Visit(context.@string()) ??
            throw new JsonPathException($"Invalid bracket content: {context.GetText()}")
        );
    /// <summary>
    /// Visits a relational query node, creating a path segment for relational comparisons like ==, !=, &lt;, &gt;.
    /// </summary>
    /// <param name="context">The parse tree context for the relational query rule.</param>
    /// <returns>A relational binary operation path segment.</returns>
    /// <exception cref="JsonPathException">Thrown when operands or operator are missing.</exception>
    public override IPathSegment VisitQueryRelational([NotNull] JsonPathParser.QueryRelationalContext context) =>
        new RelationBinaryOperationPathSegment(
            left: Visit(context.relationLeft) ?? throw new JsonPathException("no left operand defined"),
            @operator: Visit<RelationalOperationTypes>(context.RELATIONAL()) ?? throw new JsonPathException("no relational operator defined"),
            right: Visit(context.relationRight) ?? throw new JsonPathException("no right operand defined")
            );

    /// <summary>
    /// Visits a logical query node, creating a path segment for logical operations like &amp;&amp; (AND) or || (OR).
    /// </summary>
    /// <param name="context">The parse tree context for the logical query rule.</param>
    /// <returns>A logic binary operation path segment.</returns>
    /// <exception cref="JsonPathException">Thrown when operands or operator are missing.</exception>
    public override IPathSegment VisitQueryLogical([NotNull] JsonPathParser.QueryLogicalContext context) =>
        new LogicBinaryOperationPathSegment(
            left: Visit(context.relationLeft) ?? throw new JsonPathException("no left operand defined"),
            @operator: Visit<LogicOperationTypes>(context.LOGICAL()) ?? throw new JsonPathException("no logical operator defined"),
            right: Visit(context.relationRight) ?? throw new JsonPathException("no right operand defined")
            );

    /// <summary>
    /// Visits a path query node, creating a path segment that checks for the existence of a path.
    /// </summary>
    /// <param name="context">The parse tree context for the path query rule.</param>
    /// <returns>A path exists path segment.</returns>
    /// <exception cref="JsonPathException">Thrown when the reference path is invalid.</exception>
    public override IPathSegment VisitQueryPath([NotNull] JsonPathParser.QueryPathContext context) =>
        new PathExistsPathSegment(
                Visit(context.path()) as BinaryPathSegment ?? throw new JsonPathException("Invalid reference path")
            );

    /// <summary>
    /// Visits a range node, creating a path segment for array slice expressions like [start:end:step].
    /// </summary>
    /// <param name="context">The parse tree context for the range rule.</param>
    /// <returns>A range path segment with start, end, and step values.</returns>
    public override IPathSegment VisitRange([NotNull] JsonPathParser.RangeContext context) =>
        new RangePathSegment(
            start: Visit<int>(context.rangeStart),
            end: Visit<int>(context.rangeEnd),
            step: Visit<int>(context.rangeStep)
            );
    /// <summary>
    /// Visits a filter node, creating a predicate path segment for filtering arrays with conditional expressions like [?(@.price &lt; 10)].
    /// </summary>
    /// <param name="context">The parse tree context for the filter rule.</param>
    /// <returns>A predicate path segment containing the filter query.</returns>
    /// <exception cref="JsonPathException">Thrown when the query is invalid.</exception>
    public override IPathSegment VisitFilter([NotNull] JsonPathParser.FilterContext context) =>
        new PredicatePathSegment(
            Visit(context.query()) ?? throw new JsonPathException("invalid query")
            );

    /// <summary>
    /// Visits a function node, creating a function path segment for JSON Path function calls like length() or min().
    /// </summary>
    /// <param name="context">The parse tree context for the function rule.</param>
    /// <returns>A function path segment with the function name and parameters.</returns>
    /// <exception cref="JsonPathException">Thrown when the function is unnamed.</exception>
    public override IPathSegment VisitFunction([NotNull] JsonPathParser.FunctionContext context) =>
        new FunctionPathSegment(
            Visit(context.identity()) ?? throw new JsonPathException($"Unnamed functions are not allowed: {context.GetText()}"),
            Visit(context.functionParameter()) ?? SetPathSegment.Empty
            );

    /// <summary>
    /// Visits a function parameter node, extracting the parameter value which can be an operand, path base, or decimal.
    /// </summary>
    /// <param name="context">The parse tree context for the function parameter rule.</param>
    /// <returns>A path segment representing the function parameter.</returns>
    /// <exception cref="JsonPathException">Thrown when the parameter type is invalid.</exception>
    public override IPathSegment VisitFunctionParameter([NotNull] JsonPathParser.FunctionParameterContext context) =>
        Visit(
            context.operand(),
            context.pathBase(),
            context.DECIMAL()
            ) ?? throw new JsonPathException($"Invalid function parameter type");

    /// <summary>
    /// Visits a parse tree node, delegating to the base visitor or returning null for null nodes.
    /// </summary>
    /// <param name="tree">The parse tree node to visit.</param>
    /// <returns>The resulting path segment, or null if the input is null.</returns>
    public override IPathSegment? Visit(IParseTree tree) => tree switch { null => null, _ => base.Visit(tree) };

    /// <summary>
    /// Visits multiple parse tree nodes in sequence, returning the first non-null result.
    /// </summary>
    /// <param name="first">The first parse tree to try.</param>
    /// <param name="second">The second parse tree to try if first is null.</param>
    /// <param name="more">Additional parse trees to try.</param>
    /// <returns>The first non-null path segment result, or null if all are null.</returns>
    public virtual IPathSegment? Visit(IParseTree first, IParseTree second, params IParseTree[] more) =>
        Visit(first) ?? Visit(second) ?? more.Select(Visit).Where(l => l != null).FirstOrDefault();

    /// <summary>
    /// Visits multiple parse tree nodes in sequence, returning the first non-null result cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the result to.</typeparam>
    /// <param name="first">The first parse tree to try.</param>
    /// <param name="second">The second parse tree to try if first is null.</param>
    /// <param name="more">Additional parse trees to try.</param>
    /// <returns>The first non-null path segment of type T, or null if all are null or cannot be cast.</returns>
    public virtual IPathSegment<T>? Visit<T>(IParseTree first, IParseTree second, params IParseTree[] more) =>
        Visit(first) as IPathSegment<T> ??
        Visit(second) as IPathSegment<T> ??
        more.Select(i => Visit(i) as IPathSegment<T>)
            .Where(i => i != null)
            .FirstOrDefault();

    /// <summary>
    /// Visits a terminal node, converting it to a path segment based on its text value.
    /// </summary>
    /// <param name="node">The terminal node to visit.</param>
    /// <returns>A path segment representing the terminal's value.</returns>
    /// <exception cref="JsonPathException">Thrown when the terminal node value is invalid.</exception>
    public override IPathSegment VisitTerminal(ITerminalNode node) =>
        Visit(node?.GetText()) ?? throw new JsonPathException($"invalid terminal node \"{node?.GetText()}\"");

    /// <summary>
    /// Visits a token, converting it to a path segment based on its text value.
    /// </summary>
    /// <param name="token">The token to visit.</param>
    /// <returns>A path segment representing the token's value, or null if the token is null.</returns>
    public virtual IPathSegment? Visit(IToken token) => Visit(token?.Text);

    /// <summary>
    /// Converts a string value into the appropriate path segment type based on the value's content.
    /// Handles special operators ($, @, .., *), relational operators (==, !=, &lt;, &gt;), logical operators (&amp;&amp;, ||), and literal values.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>A path segment representing the string value, or null if the input is null.</returns>
    public virtual IPathSegment? Visit(string? value) =>
        value switch
        {
            null => null,

            ".." => new DescendantsPathSegment(),
            "*" => new WildcardPathSegment(),
            //Note: hidden terminal "." => new PathSeperatorPathSegment(),

            "$" => new PathBaseTypePathSegment(PathBaseTypes.Root),
            "@" => new PathBaseTypePathSegment(PathBaseTypes.Relative),

            "==" => new RelationalOperationTypePathSegment(RelationalOperationTypes.Equal),
            "!=" => new RelationalOperationTypePathSegment(RelationalOperationTypes.NotEqual),
            "<" => new RelationalOperationTypePathSegment(RelationalOperationTypes.LessThan),
            "<=" => new RelationalOperationTypePathSegment(RelationalOperationTypes.LessThanOrEqual),
            ">" => new RelationalOperationTypePathSegment(RelationalOperationTypes.GreaterThan),
            ">=" => new RelationalOperationTypePathSegment(RelationalOperationTypes.GreaterThanOrEqual),

            "&&" => new LogicOperationTypePathSegment(LogicOperationTypes.And),
            "||" => new LogicOperationTypePathSegment(LogicOperationTypes.Or),

            _ when value.Length == 0 => new StringPathSegment(""),
            _ when value[0] == '\'' => new QuotedStringPathSegment(value.Trim('\'')),
            _ when int.TryParse(value, out var number) => new NumericPathSegment(number),
            _ when decimal.TryParse(value, out var number) => new DecimalPathSegment(number),
            _ => new StringPathSegment(value)
        };
    /// <summary>
    /// Visits a terminal node and casts the result to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the result to.</typeparam>
    /// <param name="node">The terminal node to visit.</param>
    /// <returns>The path segment cast to type T, or null if the cast fails or the node is null.</returns>
    public virtual IPathSegment<T>? Visit<T>(ITerminalNode node) => Visit(node) as IPathSegment<T>;

    /// <summary>
    /// Visits a token and casts the result to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the result to.</typeparam>
    /// <param name="token">The token to visit.</param>
    /// <returns>The path segment cast to type T, or null if the cast fails or the token is null.</returns>
    public virtual IPathSegment<T>? Visit<T>(IToken token) => Visit(token) as IPathSegment<T>;

    /// <summary>
    /// Visits a parse tree node and casts the result to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the result to.</typeparam>
    /// <param name="tree">The parse tree to visit.</param>
    /// <returns>The path segment cast to type T, or null if the cast fails or the tree is null.</returns>
    public virtual IPathSegment<T>? Visit<T>(IParseTree tree) => Visit(tree) as IPathSegment<T>;

    /// <summary>
    /// Visits a collection of parse trees, combining multiple results into a set path segment.
    /// </summary>
    /// <param name="trees">The collection of parse trees to visit.</param>
    /// <returns>A single path segment if only one result, a set path segment if multiple results, or null if no results.</returns>
    public virtual IPathSegment? Visit(IEnumerable<IParseTree> trees) =>
        trees?.Select(Visit).Where(i => i != null).Cast<IPathSegment>() switch
        {
            null => null,
            IEnumerable<IPathSegment> path => path.Count() switch
            {
                0 => null,
                1 => path.First(),
                _ => new SetPathSegment(path)
            }
        };
}
