using Antlr4.Runtime;
using OoBDev.System.PathSegments;

namespace OoBDev.System.Text.Json.JsonPath.Parser;

/// <summary>
/// Provides factory methods for parsing JSON path expressions.
/// </summary>
public static class JsonPathFactory
{
    /// <summary>
    /// Parses a JSON path expression string into a path segment structure.
    /// </summary>
    /// <param name="input">The JSON path expression to parse.</param>
    /// <returns>A path segment representing the parsed JSON path.</returns>
    /// <exception cref="JsonPathException">Thrown when the input is not a valid JSON path expression.</exception>
    public static IPathSegment Parse(string input) =>
        new JsonPathVisitor().Visit(
            new JsonPathParser(
            new CommonTokenStream(
                new JsonPathLexer(
                    new AntlrInputStream(input)
                    )
                )
            )
            {
                ErrorHandler = new BailErrorStrategy(),
            }.start()
        ) ?? throw new JsonPathException($"Invalid JSONPath \"{input}\"");
}
