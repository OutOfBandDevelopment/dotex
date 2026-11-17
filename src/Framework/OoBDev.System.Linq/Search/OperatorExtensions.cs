using OoBDev.Extensions.Reflection;
using System.Linq;
using OoBDev.System.ResponseModel;
using System;

namespace OoBDev.System.Linq.Search;

internal static class OperatorExtensions
{
    public static FilterParameter AsFilter(
        this Operators expressionOperator,
        object? value,
#if DEBUG
        ICaptureResultMessage? capture
#else
        ICaptureResultMessage? capture = null
#endif
        ) =>
        new FilterParameter().And(expressionOperator, value, capture);

    public static FilterParameter And(
        this FilterParameter filter,
        Operators expressionOperator,
        object? value,
#if DEBUG
        ICaptureResultMessage? capture
#else
        ICaptureResultMessage? capture = null
#endif
        ) =>
        expressionOperator switch
        {
            Operators.EqualTo => filter with { EqualTo = value },
            Operators.NotEqualTo => filter with { NotEqualTo = value },

            Operators.LessThan => filter with { LessThan = value },
            Operators.LessThanOrEqualTo => filter with { LessThanOrEqualTo = value },
            Operators.GreaterThan => filter with { GreaterThan = value },
            Operators.GreaterThanOrEqualTo => filter with { GreaterThanOrEqualTo = value },

            Operators.InSet => filter with { InSet = (object[]?)typeof(object).MakeSafeArray(AsArray(value), capture) },

            _ => throw new NotSupportedException($"{expressionOperator} is not supported"),
        };

    private static Array? AsArray(object? value) => value switch
    {
        null => null,
        Array array => array,
        string text => new[] { text },
        global::System.Collections.IEnumerable sequence => sequence.Cast<object?>().ToArray(),
        _ => new[] { value },
    };
}
