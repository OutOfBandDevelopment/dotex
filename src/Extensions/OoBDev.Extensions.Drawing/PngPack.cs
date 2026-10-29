using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OoBDev.Extensions.Drawing;

/// <summary>Stores arbitrary bytes as the pixels of a lossless PNG image, and reads them back.</summary>
[SupportedOSPlatform("windows")]
public static class PngPack
{
    private const int HeaderLength = sizeof(int);
    private const PixelFormat Format = PixelFormat.Format32bppArgb;

    /// <summary>Packs bytes into a roughly square PNG; the first four bytes of pixel data hold the length.</summary>
    /// <param name="input">The bytes to store.</param>
    /// <returns>The PNG file.</returns>
    public static byte[] Pack(byte[] input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var pixels = ((input.Length + HeaderLength + 3) / 4);
        var width = (int)Math.Ceiling(Math.Sqrt(pixels));
        var height = (int)Math.Ceiling(pixels / (double)width);

        using var bitmap = new Bitmap(width, height, Format);
        var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, Format);
        try
        {
            Marshal.Copy(BitConverter.GetBytes(input.Length), 0, data.Scan0, HeaderLength);
            Marshal.Copy(input, 0, data.Scan0 + HeaderLength, input.Length);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        using var output = new MemoryStream();
        bitmap.Save(output, ImageFormat.Png);
        return output.ToArray();
    }

    /// <summary>Reads back the bytes stored by <see cref="Pack"/>.</summary>
    /// <param name="input">A PNG file made by <see cref="Pack"/>.</param>
    /// <exception cref="InvalidDataException">The stored length does not fit the image.</exception>
    public static byte[] Unpack(byte[] input)
    {
        ArgumentNullException.ThrowIfNull(input);

        using var stream = new MemoryStream(input);
        using var bitmap = new Bitmap(stream);
        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, Format);
        try
        {
            var header = new byte[HeaderLength];
            Marshal.Copy(data.Scan0, header, 0, HeaderLength);
            var length = BitConverter.ToInt32(header, 0);
            var capacity = (long)bitmap.Width * bitmap.Height * 4 - HeaderLength;
            if (length < 0 || length > capacity)
                throw new InvalidDataException("The image does not hold packed data.");

            var output = new byte[length];
            Marshal.Copy(data.Scan0 + HeaderLength, output, 0, length);
            return output;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }
}
