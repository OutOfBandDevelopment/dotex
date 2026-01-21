using System;
using System.Globalization;
using System.Text;
using System.Threading;

namespace OoBDev.System.IO.Iso9660;

/// <summary>
/// Provides extension methods for parsing ISO 9660 file system data structures.
/// </summary>
public static class Tools
{
    /// <summary>
    /// Converts a byte array to a hexadecimal string representation with space-separated bytes.
    /// </summary>
    /// <param name="buffer">The byte array to convert.</param>
    /// <returns>A string of space-separated hexadecimal byte values.</returns>
    public static string ToHexString(this byte[] buffer)
    {
        var sb = new StringBuilder(buffer.Length * 3);
        foreach (var item in buffer)
            sb.AppendFormat("{0:X2} ", item);
        return sb.ToString();
    }

    /// <summary>
    /// Extracts a string from a byte array at the specified offset using the given encoding.
    /// Trims whitespace and null characters from the result.
    /// </summary>
    /// <param name="buffer">The byte array containing the string data.</param>
    /// <param name="offset">The starting offset in the buffer (updated to point after the string).</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="encoding">The text encoding to use for decoding.</param>
    /// <returns>The decoded and trimmed string.</returns>
    public static string GetString(this byte[] buffer,
                                    ref int offset,
                                        int length,
                                        Encoding encoding)
    {
        var ret = encoding.GetString(buffer, offset, length)
                          .Trim(' ', '\0', '\t'); //, '\x01'
        offset += length;
        return ret;
    }

    /// <summary>
    /// Extracts a 32-bit unsigned integer from a byte array at the specified offset.
    /// </summary>
    /// <param name="buffer">The byte array containing the integer data.</param>
    /// <param name="offset">The starting offset in the buffer (updated to point after the integer).</param>
    /// <param name="length">The number of bytes to skip after reading (typically 8 for both-endian storage).</param>
    /// <returns>The extracted 32-bit unsigned integer.</returns>
    public static uint GetUInt32(this byte[] buffer,
                                  ref int offset,
                                      int length)
    {
        var ret = BitConverter.ToUInt32(buffer, offset);
        offset += length;
        return ret;
    }

    /// <summary>
    /// Extracts a 16-bit unsigned integer from a byte array at the specified offset.
    /// </summary>
    /// <param name="buffer">The byte array containing the integer data.</param>
    /// <param name="offset">The starting offset in the buffer (updated to point after the integer).</param>
    /// <param name="length">The number of bytes to skip after reading (typically 4 for both-endian storage).</param>
    /// <returns>The extracted 16-bit unsigned integer.</returns>
    public static ushort GetUInt16(this byte[] buffer,
                                    ref int offset,
                                        int length)
    {
        var ret = BitConverter.ToUInt16(buffer, offset);
        offset += length;
        return ret;
    }

    /// <summary>
    /// Extracts a DateTime from a byte array in ISO 9660 format (17 bytes: yyyyMMddHHmmssff + timezone offset).
    /// The timezone offset is in 15-minute intervals and is applied to the result.
    /// </summary>
    /// <param name="buffer">The byte array containing the datetime data.</param>
    /// <param name="offset">The starting offset in the buffer (updated to point after the datetime).</param>
    /// <param name="length">The number of bytes to skip after reading.</param>
    /// <returns>The extracted DateTime adjusted for timezone offset.</returns>
    public static DateTime GetDateTime(this byte[] buffer,
                                        ref int offset,
                                            int length)
    {
        var temp = Encoding.ASCII.GetString(buffer, offset, 16).Trim();
        var timeOffset = (sbyte)buffer[offset + 16] * 15;
        if (DateTime.TryParseExact(temp,
                                   "yyyyMMddHHmmssff",
                                   Thread.CurrentThread.CurrentCulture,
                                   DateTimeStyles.AdjustToUniversal,
                                   out DateTime ret))
            ret = ret.AddMinutes(timeOffset);
        offset += length;
        return ret;
    }
}
