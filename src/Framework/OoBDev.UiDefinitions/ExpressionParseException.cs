using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.UiDefinitions;

/// <summary>
/// Thrown by <see cref="Expression.Parse"/> when an expression's text doesn't parse — never thrown
/// by <see cref="Expression.Evaluate"/> itself, which always returns a number (see that method's
/// own remarks for why evaluation never throws).
/// </summary>
public sealed class ExpressionParseException : Exception
{
    public ExpressionParseException(string message)
        : base(message)
    {
    }
}
