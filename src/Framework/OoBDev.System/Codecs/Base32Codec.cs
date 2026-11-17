using System;
using System.Text;

namespace OoBDev.System.Codecs;

/// <summary>
/// 
/// </summary>
/// <remarks>
///  https://tools.ietf.org/html/rfc4648
///
/// The case for base 32 is shown in the following figure, borrowed from
/// [7].  Each successive character in a base-32 value represents 5
/// successive bits of the underlying octet sequence.  Thus, each group
/// of 8 characters represents a sequence of 5 octets (40 bits).
///             1          2          3
///  01234567 89012345 67890123 45678901 23456789
/// +--------+--------+--------+--------+--------+
/// |&lt; 1 &gt;&lt; 2| &gt;&lt; 3 &gt;&lt;|.4 &gt;&lt; 5.|&gt;&lt; 6 &gt;&lt;.|7 &gt;&lt; 8 &gt;|
/// +--------+--------+--------+--------+--------+
///                                         &lt;===&gt; 8th character
///                                   &lt;====&gt; 7th character
///                              &lt;===&gt; 6th character
///                        &lt;====&gt; 5th character
///                  &lt;====&gt; 4th character
///             &lt;===&gt; 3rd character
///       &lt;====&gt; 2nd character
///  &lt;===&gt; 1st character
/// 
///  01234567 01234567 01234567 01234567 01234567
///  00000000 11111111 22222222 33333333 44444444
///  AAAAABBB BBCCCCCD DDDDEEEE EFFFFFGG GGGHHHHH
/// </remarks>
public class Base32Codec
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    /// <summary>
    /// Encoding 8bit binary data as base32 string
    /// </summary>
    /// <param name="data">8bit data</param>
    /// <returns>base32 string, padded with '=' to a multiple of 8 characters</returns>
    public string Encode(ReadOnlySpan<byte> data)
    {
        var sb = new StringBuilder((data.Length + 4) / 5 * 8);
        Span<byte> vi = stackalloc byte[5];

        for (var i = 0; i < data.Length; i += 5)
        {
            var cl = global::System.Math.Min(5, data.Length - i);
            vi.Clear();
            data.Slice(i, cl).CopyTo(vi);

            Span<int> vo =
            [
                (vi[0] & 0xf8) / 8,
                (vi[0] & 0x7) * 4 + (vi[1] & 0xc0) / 64,
                (vi[1] & 0x3E) / 2,
                (vi[1] & 0x1) * 16 + (vi[2] & 0xF0) / 16,
                (vi[2] & 0xF) * 2 + (vi[3] & 0x80) / 128,
                (vi[3] & 0x7E) / 4,
                (vi[3] & 0x3) * 8 + (vi[4] & 0xE0) / 32,
                vi[4] & 0x1F,
            ];

            var characters = cl < 5 ? (int)global::System.Math.Ceiling(cl * 8d / 5d) : 8;
            for (var c = 0; c < 8; c++)
                sb.Append(c < characters ? Base32Alphabet[vo[c]] : '=');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Decode base32 string into 8bit binary
    /// </summary>
    /// <param name="input">base32 string</param>
    /// <returns>8bit data</returns>
    public byte[] Decode(ReadOnlySpan<char> input)
    {
        var trimmed = input.TrimEnd('=');
        var oData = new byte[trimmed.Length * 5 / 8];
        Span<byte> vi = stackalloc byte[8];

        for (int i = 0, o = 0; i < trimmed.Length; i += 8, o += 5)
        {
            vi.Clear();
            var group = trimmed.Slice(i, global::System.Math.Min(8, trimmed.Length - i));
            for (var c = 0; c < group.Length; c++)
            {
                var index = Base32Alphabet.IndexOf(group[c], StringComparison.Ordinal);
                vi[c] = index < 0 ? (byte)0 : (byte)index;
            }

            Span<byte> vo =
            [
                (byte)(vi[0] * 8 + (vi[1] & 0x1C) / 4),
                (byte)((vi[1] & 0x3) * 64 + vi[2] * 2 + (vi[3] & 0x10) / 16),
                (byte)((vi[3] & 0xF) * 16 + (vi[4] & 0x1E) / 2),
                (byte)((vi[4] & 0x1) * 128 + vi[5] * 4 + (vi[6] & 0x18) / 8),
                (byte)((vi[6] & 0x7) * 32 + vi[7]),
            ];

            vo[..global::System.Math.Min(5, oData.Length - o)].CopyTo(oData.AsSpan(o));
        }

        return oData;
    }
}

//public class Base32Encoding
//{
//    public static byte[] ToBytes(string input)
//    {
//        if (string.IsNullOrEmpty(input))
//        {
//            throw new ArgumentNullException("input");
//        }

//        input = input.TrimEnd('='); //remove padding characters
//        int byteCount = input.Length * 5 / 8; //this must be TRUNCATED
//        byte[] returnArray = new byte[byteCount];

//        byte curByte = 0, bitsRemaining = 8;
//        int mask = 0, arrayIndex = 0;

//        foreach (char c in input)
//        {
//            int cValue = CharToValue(c);

//            if (bitsRemaining > 5)
//            {
//                mask = cValue << (bitsRemaining - 5);
//                curByte = (byte)(curByte | mask);
//                bitsRemaining -= 5;
//            }
//            else
//            {
//                mask = cValue >> (5 - bitsRemaining);
//                curByte = (byte)(curByte | mask);
//                returnArray[arrayIndex++] = curByte;
//                curByte = (byte)(cValue << (3 + bitsRemaining));
//                bitsRemaining += 3;
//            }
//        }

//        //if we didn't end with a full byte
//        if (arrayIndex != byteCount)
//        {
//            returnArray[arrayIndex] = curByte;
//        }

//        return returnArray;
//    }

//    public static string ToString(byte[] input)
//    {
//        if (input == null || input.Length == 0)
//        {
//            throw new ArgumentNullException("input");
//        }

//        int charCount = (int)Math.Ceiling(input.Length / 5d) * 8;
//        char[] returnArray = new char[charCount];

//        byte nextChar = 0, bitsRemaining = 5;
//        int arrayIndex = 0;

//        foreach (byte b in input)
//        {
//            nextChar = (byte)(nextChar | (b >> (8 - bitsRemaining)));
//            returnArray[arrayIndex++] = ValueToChar(nextChar);

//            if (bitsRemaining < 4)
//            {
//                nextChar = (byte)((b >> (3 - bitsRemaining)) & 31);
//                returnArray[arrayIndex++] = ValueToChar(nextChar);
//                bitsRemaining += 5;
//            }

//            bitsRemaining -= 3;
//            nextChar = (byte)((b << bitsRemaining) & 31);
//        }

//        //if we didn't end with a full char
//        if (arrayIndex != charCount)
//        {
//            returnArray[arrayIndex++] = ValueToChar(nextChar);
//            while (arrayIndex != charCount) returnArray[arrayIndex++] = '='; //padding
//        }

//        return new string(returnArray);
//    }

//    private static int CharToValue(char c)
//    {
//        int value = (int)c;

//        //65-90 == uppercase letters
//        if (value < 91 && value > 64)
//        {
//            return value - 65;
//        }
//        //50-55 == numbers 2-7
//        if (value < 56 && value > 49)
//        {
//            return value - 24;
//        }
//        //97-122 == lowercase letters
//        if (value < 123 && value > 96)
//        {
//            return value - 97;
//        }

//        throw new ArgumentException("Character is not a Base32 character.", "c");
//    }

//    private static char ValueToChar(byte b)
//    {
//        if (b < 26)
//        {
//            return (char)(b + 65);
//        }

//        if (b < 32)
//        {
//            return (char)(b + 24);
//        }

//        throw new ArgumentException("Byte is not a value Base32 value.", "b");
//    }

//}
