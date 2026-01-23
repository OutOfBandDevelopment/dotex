using Antlr4.Runtime;
using OoBDev.ExpressionCalculator.Expressions;
using OoBDev.ExpressionCalculator.Visitors;
using System;

namespace OoBDev.ExpressionCalculator.Parser;

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
