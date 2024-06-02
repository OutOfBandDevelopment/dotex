using Antlr4.Runtime;
using OoBDev.System.ExpressionCalculator.Expressions;
using System;

namespace OoBDev.System.ExpressionCalculator.Parser;

public class ExpressionParser<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    public ExpressionBase<T> Parse(string input) =>
        new ExpressionTreeVisitor<T>().Visit(
            new ExpressionTreeParser(
                    new CommonTokenStream(
                        new ExpressionTreeLexer(
                            new AntlrInputStream(input)
                            )
                        )
                    )
            {
                ErrorHandler = new BailErrorStrategy(),
            }.start()
            );
}
