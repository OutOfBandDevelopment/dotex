using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.Extensions;

/// <summary>
/// Provides utility methods for string manipulation.
/// </summary>
public static class StringTools
{
    /// <summary>
    /// Splits the input string into lines of specified length, breaking at the nearest space character.
    /// </summary>
    /// <param name="input">The input string to split.</param>
    /// <param name="length">The maximum length of each line (default is 80).</param>
    /// <param name="breaker">The character at which to break lines (default is space).</param>
    /// <returns>An enumerable of strings representing the split lines.</returns>
    public static IEnumerable<string> SplitBy(this string input, int length = 80, char breaker = ' ')
    {
        var reader = new StringReader(input);

        while (true)
        {
            var line = reader.ReadLine();
            if (line == null)
                break;

            if (line.Length > length)
            {
                var nextStart = 0;

                while (nextStart < line.Length)
                {
                    if (nextStart + length > line.Length)
                    {
                        var chunk = line[nextStart..];
                        yield return chunk.TrimEnd('\r', '\n', ' ');
                        break;
                    }
                    else
                    {
                        var chunk = line[nextStart..(nextStart + length)];
                        if (chunk == null)
                            break;
                        else if (chunk.Length < length)
                        {
                            yield return chunk.TrimEnd('\r', '\n', ' ');
                            nextStart += length;
                        }

                        var last = chunk.LastIndexOf(breaker);

                        if (last > 0)
                        {
                            chunk = chunk[..last];
                            yield return chunk.TrimEnd('\r', '\n', ' ');
                            nextStart += chunk.Length + 1;
                        }
                        else if (last == 0)
                        {
                            nextStart++;
                        }
                        else
                        {
                            yield return chunk.TrimEnd('\r', '\n', ' ');
                            nextStart += chunk.Length;
                        }
                    }
                }
            }
            else
            {
                yield return line.TrimEnd('\r', '\n', ' ');
            }
        }
    }

    /// <summary>
    /// Concatenates the lines into a single string with newline separators.
    /// </summary>
    /// <param name="lines">The lines to concatenate.</param>
    /// <returns>A single string with newline separators.</returns>
    public static string WriteAsLines(this IEnumerable<string> lines) => string.Join(Environment.NewLine, lines);
}
