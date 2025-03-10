﻿using OoBDev.System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.Linq;

public static class ShuffleExtensions
{
    private static Random RandomGenerator { get; } = new Random();

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random? randomGenerator = null)
    {
        randomGenerator ??= RandomGenerator;

        //http://stackoverflow.com/questions/1287567/is-using-random-and-orderby-a-good-shuffle -algorithm
        var elements = source.ToArray();
        for (int i = elements.Length - 1; i >= 0; i--)
        {
            // Swap element "i" with a random earlier element it (or itself)
            // ... except we don't really need to swap it fully, as we can
            // return it immediately, and afterwards it's irrelevant.
            var swapIndex = randomGenerator.Next(i + 1);
            yield return elements[swapIndex];
            elements[swapIndex] = elements[i];
        }
    }
}
