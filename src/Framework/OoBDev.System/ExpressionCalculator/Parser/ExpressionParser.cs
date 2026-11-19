using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using OoBDev.System.ExpressionCalculator.Expressions;
using System;
using System.IO;

namespace OoBDev.System.ExpressionCalculator.Parser;

/// <summary>
/// Parses string representations of mathematical expressions into expression tree objects using ANTLR-based parsing.
/// </summary>
/// <typeparam name="T">The numeric type for the parsed expression values.</typeparam>
public class ExpressionParser<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Parses a string expression into an expression tree.
    /// </summary>
    /// <param name="input">The string representation of the expression to parse.</param>
    /// <returns>An expression tree representing the parsed input.</returns>
    /// <exception cref="Exception">Thrown when the input contains syntax errors or cannot be parsed.</exception>
    public ExpressionBase<T> Parse(string input)
    {
        var lexer = new ExpressionTreeLexer(new AntlrInputStream(input));
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new ThrowingLexerErrorListener());
        return new ExpressionTreeVisitor<T>().Visit(
            new ExpressionTreeParser(new CommonTokenStream(lexer))
            {
                ErrorHandler = new BailErrorStrategy(),
            }.start());
    }

    private sealed class ThrowingLexerErrorListener : IAntlrErrorListener<int>
    {
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) =>
            throw new ParseCanceledException($"line {line}:{charPositionInLine} {msg}");
    }
}
