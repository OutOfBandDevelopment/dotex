using System;
using System.Collections.Generic;

namespace OoBDev.System.Cryptography;

/// <summary>
/// Implements the Playfair cipher, a manual symmetric encryption technique that encrypts pairs of letters (digraphs) using a 5x5 grid.
/// See https://en.wikipedia.org/wiki/Playfair_cipher for more information.
/// </summary>
/// <remarks>
/// WARNING: This is a classic cipher for educational purposes only. It provides no security and should never be used for protecting sensitive data.
/// The cipher uses a 5x5 table and requires reducing the alphabet by combining or omitting one letter (Q, I, or J).
/// </remarks>
public class PlayFair
{
    /*
    [edit] Using Playfair
    The Playfair cipher uses a 5 by 5 table containing a key word or phrase. Memorization of the keyword and 4 simple rules was all that was required to create the 5 by 5 table and use the cipher.

    To generate the key table, one would first fill in the spaces in the table with the letters of the keyword (dropping any duplicate letters), then fill the remaining spaces with the rest of the letters of the alphabet in order (usually omitting "Q" to reduce the alphabet to fit, other versions put both "I" and "J" in the same space). The key can be written in the top rows of the table, from left to right, or in some other pattern, such as a spiral beginning in the upper-left-hand corner and ending in the center. The keyword together with the conventions for filling in the 5 by 5 table constitute the cipher key.

    To encrypt a message, one would break the message into digraphs (groups of 2 letters) such that, for example, "HelloWorld" becomes "HE LL OW OR LD", and map them out on the key table. The two letters of the digraph look like the corners of a rectangle in the key table. Note the relative position of the corners of this rectangle. Then apply the following 4 rules, in order, to each pair of letters in the plaintext:

    If both letters are the same (or only one letter is left), add an "X" after the first letter. Encrypt the new pair and continue. Some variants of Playfair use "Q" instead of "X", but any uncommon monograph will do. 
    If the letters appear on the same row of your table, replace them with the letters to their immediate right respectively (wrapping around to the left side of the row if a letter in the original pair was on the right side of the row). 
    If the letters appear on the same column of your table, replace them with the letters immediately below respectively (wrapping around to the top side of the column if a letter in the original pair was on the bottom side of the column). 
    If the letters are not on the same row or column, replace them with the letters on the same row respectively but at the other pair of corners of the rectangle defined by the original pair. The order is important – the first encrypted letter of the pair is the one that lies on the same row as the first plaintext letter. 
    To decrypt, use the inverse of these 4 rules (dropping any extra "X"s (or "Q"s) that don't make sense in the final message when you finish).
    */

    /// <summary>
    /// Specifies which letter to omit from the 5x5 grid and how to handle it in the plaintext.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Omit 'Q' from the grid and replace it with 'X' in the plaintext.
        /// </summary>
        Q = 1,

        /// <summary>
        /// Omit 'J' from the grid and replace it with 'I' in the plaintext.
        /// </summary>
        J = 2,

        /// <summary>
        /// Omit 'I' from the grid and replace it with 'J' in the plaintext.
        /// </summary>
        I = 3
    }

    /// <summary>
    /// Specifies which character to use for padding when duplicate letters appear in a digraph.
    /// </summary>
    public enum Swap
    {
        /// <summary>
        /// Use 'X' as the padding character.
        /// </summary>
        X = 1,

        /// <summary>
        /// Use 'Z' as the padding character.
        /// </summary>
        Z = 2
    }

    /// <summary>
    /// Builds a 5x5 Playfair cipher key grid from the specified keyword.
    /// The grid is filled with the keyword (removing duplicates), followed by the remaining letters of the alphabet.
    /// </summary>
    /// <param name="key">The keyword to use for building the cipher grid (must not be null or empty).</param>
    /// <param name="mode">The mode specifying which letter to omit from the grid.</param>
    /// <param name="swap">The character to use for padding duplicate letters.</param>
    /// <returns>A 25-element character array representing the 5x5 cipher grid (row-major order).</returns>
    /// <exception cref="ArgumentNullException">Thrown when the key is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when mode or swap has an invalid value.</exception>
    public static char[] BuildKey(string key, Mode mode, Swap swap)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        var seed = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        var cipherKey = new char[5 * 5];
        key = key.ToUpper();
        char cMode;
        var cSwap = swap switch
        {
            Swap.X => 'X',
            Swap.Z => 'Z',
            _ => throw new ArgumentOutOfRangeException(nameof(swap)),
        };
        switch (mode)
        {
            case Mode.Q:
                cMode = 'Q';
                key = key.Replace('Q', cSwap);
                break;
            case Mode.J:
                cMode = 'J';
                key = key.Replace('J', 'I');
                break;
            case Mode.I:
                cMode = 'I';
                key = key.Replace('I', 'J');
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
        seed = seed.Replace(cMode.ToString(), "");
        var pos = 0;

        foreach (var currentChar in key.ToCharArray())
        {
            if (seed.Contains(currentChar))
            {
                seed = seed.Replace(currentChar.ToString(), "");
                cipherKey[pos++] = currentChar;
            }

            if (pos >= cipherKey.Length)
                break;
        }

        foreach (var currentChar in seed.ToCharArray())
        {
            if (pos >= cipherKey.Length)
                break;
            cipherKey[pos++] = currentChar;
        }

        return cipherKey;
    }

    /// <summary>
    /// Encrypts a message using the Playfair cipher with the specified key grid.
    /// The message is broken into digraphs (pairs of letters), with padding added for duplicate letters and odd-length messages.
    /// Non-alphabetic characters are removed from the message.
    /// </summary>
    /// <param name="cryptic">The 25-element cipher key grid (must be exactly 25 characters).</param>
    /// <param name="message">The message to encrypt (must not be null or empty).</param>
    /// <param name="mode">The mode specifying which letter was omitted from the grid.</param>
    /// <param name="swap">The character used for padding duplicate letters.</param>
    /// <returns>The encrypted ciphertext.</returns>
    /// <exception cref="ArgumentNullException">Thrown when cryptic or message is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when cryptic is not exactly 25 characters, or when mode or swap has an invalid value.</exception>
    public static string Cipher(char[] cryptic, string message, Mode mode, Swap swap)
    {
        ArgumentNullException.ThrowIfNull(cryptic);

        if (cryptic.Length != 25)
            throw new ArgumentOutOfRangeException(nameof(cryptic));

        if (string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(message));

        message = message.ToUpper();

        var cSwap = swap switch
        {
            Swap.X => 'X',
            Swap.Z => 'Z',
            _ => throw new ArgumentOutOfRangeException(nameof(swap)),
        };
        var check = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        switch (mode)
        {
            case Mode.Q:
                check = check.Replace("Q", "");
                message = message.Replace('Q', cSwap);
                break;
            case Mode.J:
                check = check.Replace("J", "");
                message = message.Replace('J', 'I');
                break;
            case Mode.I:
                check = check.Replace("I", "");
                message = message.Replace('I', 'J');
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }

        List<char> newMessage = [];
        foreach (var currentChar in message.ToCharArray())
            if (check.Contains(currentChar))
                newMessage.Add(currentChar);
        message = new string([.. newMessage]);

        foreach (var currentChar in check.ToCharArray())
        {
            if (currentChar != cSwap)
                message = message.Replace(
                    new string(new char[] { currentChar, currentChar }),
                    new string(new char[] { currentChar, cSwap, currentChar }));
        }

        message = message.PadRight(message.Length + message.Length % 2, cSwap);

        string sCryptic = new(cryptic);

        List<char> cipherText = [];

        for (var i = 0; i < message.Length; i += 2)
        {
            var p1 = sCryptic.IndexOf(message[i]);
            var p2 = sCryptic.IndexOf(message[i + 1]);

            var mn = global::System.Math.Min(p1, p2);
            var mx = global::System.Math.Max(p1, p2);

            var column = mn % 5 == mx % 5;
            var row = mx - mn < 5;

            if (row)
            {
                p1 = p1++ % 5 == 0 ? p1 - 5 : p1;
                p2 = p2++ % 5 == 0 ? p2 - 5 : p2;
            }
            else if (column)
            {
                p1 = p1 += 5 >= 25 ? p1 - 25 : p1;
                p2 = p2 += 5 >= 25 ? p2 - 25 : p2;
            }
            else
            {
                var x1 = p1 % 5;
                var y1 = p1 / 5;

                var x2 = p2 % 5;
                var y2 = p2 / 5;
            }

            cipherText.Add(sCryptic[p1]);
            cipherText.Add(sCryptic[p2]);
        }

        return new string([.. cipherText]);
    }
}
