using System;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>
/// Turns encoded image bytes (JPEG, PNG, WebP, GIF, BMP, ...) into 8-bit RGB pixels. The runner has no imaging
/// dependency of its own, so the decoder library can be swapped.
/// </summary>
public interface IImageDecoder
{
    /// <summary>
    /// Decodes an image. Alpha is dropped without compositing (the stored colour is kept), grayscale and palette
    /// images are expanded to RGB, and animated images decode their first frame, like Pillow's <c>convert("RGB")</c>.
    /// </summary>
    /// <param name="encoded">Encoded image bytes.</param>
    /// <param name="maxPixels">Largest accepted <c>width * height</c>; the decoder must reject larger images before allocating pixels.</param>
    /// <returns>The decoded pixels.</returns>
    /// <exception cref="System.IO.InvalidDataException">The bytes are not a supported image, or the image is larger than <paramref name="maxPixels"/>.</exception>
    DecodedImage Decode(ReadOnlySpan<byte> encoded, long maxPixels);
}

/// <summary>
/// Decoded pixels in row-major order, three bytes (R, G, B) per pixel.
/// </summary>
/// <param name="Width">Width in pixels.</param>
/// <param name="Height">Height in pixels.</param>
/// <param name="Rgb">Pixel bytes, length <c>Width * Height * 3</c>.</param>
public readonly record struct DecodedImage(int Width, int Height, byte[] Rgb);
