﻿using System;
using System.Collections.Generic;

namespace OoBDev.System.Codecs;

internal static class RomanExtensions
{
    internal static IEnumerable<int> FirstPass(this IEnumerable<char> value)
    {
        var enumerator = value.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current == '/'
                ? !enumerator.MoveNext()
                    ? throw new ApplicationException("invalid. unexpected end of input")
                    : enumerator.Current.GetValue() * 1000
                : enumerator.Current.GetValue();
        }
    }

    internal static IEnumerable<int> SecondPass(this IEnumerable<int> inputs)
    {
        var enumerator = inputs.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;

        var last = enumerator.Current;

        while (enumerator.MoveNext())
        {
            yield return last < enumerator.Current ? -last : last;
            last = enumerator.Current;
        }

        yield return last;
    }

    internal static int GetValue(this char v) =>
        v switch
        {
            'I' => 1,
            'V' => 5,
            'X' => 10,
            'L' => 50,
            'C' => 100,
            'D' => 500,
            'M' => 1000,
            _ => throw new NotSupportedException()
        };

}
