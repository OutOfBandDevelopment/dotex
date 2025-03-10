﻿using Antlr4.Runtime;
using OoBDev.System.PathSegments;

namespace OoBDev.System.Text.Json.JsonPath.Parser;

public static class JsonPathFactory
{
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
