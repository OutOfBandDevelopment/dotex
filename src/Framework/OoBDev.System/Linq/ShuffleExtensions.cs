using OoBDev.System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.Linq;

/// <summary>
/// Provides extension methods for randomizing the order of elements in sequences.
/// </summary>
public static class ShuffleExtensions
{
    private static Random RandomGenerator { get; } = new Random();

    /// <summary>
    /// Randomly shuffles the elements of a sequence using the Fisher-Yates shuffle algorithm.
    /// This method provides a uniform random permutation of the input sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to shuffle.</param>
    /// <param name="randomGenerator">Optional random number generator to use. If null, uses a shared static Random instance.</param>
    /// <returns>A new sequence with elements in randomized order.</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random? randomGenerator = null)
    {
        randomGenerator ??= RandomGenerator;

        //http://stackoverflow.com/questions/1287567/is-using-random-and-orderby-a-good-shuffle -algorithm
        var elements = source.ToArray();
        for (var i = elements.Length - 1; i >= 0; i--)
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
