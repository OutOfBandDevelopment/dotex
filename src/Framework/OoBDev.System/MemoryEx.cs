using OoBDev.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System;

/// <summary>
/// Extension methods for System.Memory&lt;T&gt; and related types, providing conversion, parsing, and manipulation operations.
/// </summary>
public static class MemoryEx
{
    /// <summary>
    /// Converts an enumerable sequence of characters into a Memory&lt;char&gt; instance.
    /// </summary>
    /// <param name="input">The enumerable sequence of characters to convert.</param>
    /// <returns>A Memory&lt;char&gt; containing all characters from the input sequence.</returns>
    public static Memory<char> AsMemory(this IEnumerable<char> input) =>
        new([.. input]);

    /// <summary>
    /// Returns distinct memory segments from a sequence, comparing segments by their content rather than reference equality.
    /// </summary>
    /// <typeparam name="T">The type of elements in the memory segments, which must implement IEquatable&lt;T&gt;.</typeparam>
    /// <param name="segments">The sequence of memory segments to filter for distinct values.</param>
    /// <returns>A sequence containing only distinct memory segments based on content comparison.</returns>
    public static IEnumerable<Memory<T>> Distinct<T>(this IEnumerable<Memory<T>> segments) where T : IEquatable<T> =>
        segments.Distinct(new MemoryCompare<T>());

    /// <summary>
    /// Converts a hexadecimal string representation into a byte array memory segment.
    /// Each pair of hexadecimal characters (0-9, A-F, a-f) is converted to a single byte.
    /// </summary>
    /// <param name="input">The memory segment containing hexadecimal characters to parse. Must have an even length.</param>
    /// <returns>A Memory&lt;byte&gt; containing the parsed byte values.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the input contains invalid hexadecimal characters.</exception>
    public static Memory<byte> BytesFromHexString(this Memory<char> input)
    {
        static byte charToNibble(char input)
        {
            unchecked
            {
                if (input >= '0' && input <= '9') return (byte)(input - '0');
                else return input >= 'A' && input <= 'F'
                    ? (byte)(input - 'A' + 10)
                    : input >= 'a' && input <= 'f' ? (byte)(input - 'a' + 10) : throw new InvalidOperationException();
            }
        }

        var memory = new Memory<byte>(new byte[input.Length / 2]);
        for (var i = 0; i < input.Length; i += 2)
        {
            var highNibble = charToNibble(input.Span[i]);
            var lowNibble = charToNibble(input.Span[i + 1]);

            var memoryIndex = i >> 1;
            var newValue = (byte)(highNibble << 4 | lowNibble);

            memory.Span[memoryIndex] = newValue;
        }
        return memory;
    }

    /// <summary>
    /// Splits a byte memory segment into multiple segments based on a delimiter byte.
    /// </summary>
    /// <param name="memory">The memory segment to split.</param>
    /// <param name="delimiter">The delimiter byte to split on.</param>
    /// <param name="option">Specifies how to handle the delimiter (Exclude, Return, or Carry). Defaults to Exclude.</param>
    /// <returns>An enumerable sequence of memory segments resulting from the split operation.</returns>
    public static IEnumerable<Memory<byte>> Split(this Memory<byte> memory, byte delimiter, DelimiterOptions option = DelimiterOptions.Exclude) =>
        memory.Split<byte>(delimiter, option);

    /// <summary>
    /// Splits a character memory segment into multiple segments based on a delimiter character.
    /// </summary>
    /// <param name="memory">The memory segment to split.</param>
    /// <param name="delimiter">The delimiter character to split on.</param>
    /// <param name="option">Specifies how to handle the delimiter (Exclude, Return, or Carry). Defaults to Exclude.</param>
    /// <returns>An enumerable sequence of memory segments resulting from the split operation.</returns>
    public static IEnumerable<Memory<char>> Split(this Memory<char> memory, char delimiter, DelimiterOptions option = DelimiterOptions.Exclude) =>
        memory.Split<char>(delimiter, option);

    /// <summary>
    /// Splits a memory segment into multiple segments based on a delimiter value.
    /// The delimiter handling behavior is controlled by the option parameter:
    /// - Exclude: Delimiter is removed from results
    /// - Return: Delimiter is included at the end of each segment
    /// - Carry: Delimiter is included at the beginning of the next segment
    /// </summary>
    /// <typeparam name="T">The type of elements in the memory segment, which must implement IEquatable&lt;T&gt;.</typeparam>
    /// <param name="memory">The memory segment to split.</param>
    /// <param name="delimiter">The delimiter value to split on.</param>
    /// <param name="option">Specifies how to handle the delimiter. Defaults to Exclude.</param>
    /// <returns>An enumerable sequence of memory segments resulting from the split operation.</returns>
    public static IEnumerable<Memory<T>> Split<T>(this Memory<T> memory, T delimiter, DelimiterOptions option = DelimiterOptions.Exclude) where T : IEquatable<T> =>
        option switch
        {
            DelimiterOptions.Return => memory.SplitWithReturn(delimiter),
            DelimiterOptions.Carry => memory.SplitWithCarry(delimiter),
            _ => memory.SplitWithExclude(delimiter),
        };

    private static IEnumerable<Memory<T>> SplitWithExclude<T>(this Memory<T> memory, T delimiter) where T : IEquatable<T>
    {
        var pointer = 0;
        while (memory.Length > pointer)
        {
            var segment = memory.Span[pointer..];
            var next = segment.IndexOf(delimiter) + 1;

            if (next <= 0)
            {
                yield return memory[pointer..];
                yield break;
            }
            else
            {
                yield return memory.Slice(pointer, next - 1);
                pointer += next;
            }
        }
    }

    private static IEnumerable<Memory<T>> SplitWithReturn<T>(this Memory<T> memory, T delimiter) where T : IEquatable<T>
    {
        var pointer = 0;
        while (memory.Length > pointer)
        {
            var segment = memory.Span[pointer..];
            var next = segment.IndexOf(delimiter) + 1;

            if (next <= 0)
            {
                yield return memory[pointer..];
                yield break;
            }
            else
            {
                yield return memory.Slice(pointer, next);
                pointer += next;
            }
        }
    }

    private static IEnumerable<Memory<T>> SplitWithCarry<T>(this Memory<T> memory, T delimiter) where T : IEquatable<T>
    {
        var pointer = 0;
        while (memory.Length > pointer)
        {
            var bump = delimiter.Equals(memory.Span[pointer]);
            var segmentPointer = bump ? pointer + 1 : pointer;
            var segment = memory.Span[segmentPointer..];
            var next = segment.IndexOf(delimiter) + (bump ? 1 : 0);

            if (next <= 0)
            {
                yield return memory[pointer..];
                yield break;
            }
            else
            {
                yield return memory.Slice(pointer, next);
                pointer += next;
            }
        }
    }
}
