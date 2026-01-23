using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.System;

/// <summary>
/// Provides extended console input/output functionality including asynchronous and secure prompts.
/// </summary>
public static class ConsoleEx
{
    /// <summary>
    /// Asynchronously reads a line of text from the console input.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the line read from the console or null if end of stream.</returns>
    public static Task<string?> ReadLineAsync() =>
        Task.FromResult(Console.ReadLine());

    /// <summary>
    /// Prompts the user for input with an optional default value and supports editing.
    /// </summary>
    /// <param name="prompt">The prompt text to display (optional).</param>
    /// <param name="defaultValue">The default value to pre-populate (optional).</param>
    /// <returns>The user's input text, or null if the user pressed Escape.</returns>
    public static string? Prompt(string? prompt = null, string? defaultValue = null)
    {
        if (!string.IsNullOrWhiteSpace(prompt))
            Console.Write("{0} ", prompt);
        if (!string.IsNullOrWhiteSpace(defaultValue))
            Console.Write("{0}", defaultValue);

        var chars = new List<char>(defaultValue ?? "");
        while (true)
        {
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Escape)
                return null;
            else if (key.Key == ConsoleKey.Enter)
                break;
            else if (key.Key == ConsoleKey.Backspace || key.Key == ConsoleKey.Delete)
            {
                if (chars.Count > 0)
                {
                    chars.RemoveAt(chars.Count - 1);
                    Console.Write((char)8);
                    Console.Write(" ");
                    Console.Write((char)8);
                }
            }
            else
            {
                chars.Add(key.KeyChar);
                Console.Write(key.KeyChar);
            }
        }
        Console.WriteLine();
        var result = new string([.. chars]);
        return result;
    }

    /// <summary>
    /// Prompts the user for secure input (like passwords) with characters hidden by a specified character.
    /// </summary>
    /// <param name="prompt">The prompt text to display (optional).</param>
    /// <param name="defaultValue">The default value to pre-populate (optional, hidden when displayed).</param>
    /// <param name="hideWith">The character to display instead of the actual input (default is '*').</param>
    /// <returns>The user's input text, or null if the user pressed Escape.</returns>
    public static string? PromptSecure(string? prompt = null, string? defaultValue = null, char hideWith = '*')
    {
        if (!string.IsNullOrWhiteSpace(prompt))
            Console.Write($"{prompt} ");
        if (!string.IsNullOrWhiteSpace(defaultValue))
            Console.Write($"{new string(hideWith, defaultValue.Length)}");

        var chars = new List<char>(defaultValue ?? "");
        while (true)
        {
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Escape)
                return null;
            else if (key.Key == ConsoleKey.Enter)
                break;
            else if (key.Key == ConsoleKey.Backspace || key.Key == ConsoleKey.Delete)
            {
                if (chars.Count > 0)
                {
                    chars.RemoveAt(chars.Count - 1);
                    Console.Write((char)8);
                    Console.Write(" ");
                    Console.Write((char)8);
                }
            }
            else
            {
                chars.Add(key.KeyChar);
                Console.Write(hideWith);
            }
        }
        Console.WriteLine();
        var result = new string([.. chars]);
        return result;
    }
}
