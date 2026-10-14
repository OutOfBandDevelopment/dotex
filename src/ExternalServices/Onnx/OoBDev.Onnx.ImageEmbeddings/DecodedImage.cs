using System;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>
/// Decoded pixels in row-major order, three bytes (R, G, B) per pixel.
/// </summary>
/// <param name="Width">Width in pixels.</param>
/// <param name="Height">Height in pixels.</param>
/// <param name="Rgb">Pixel bytes, length <c>Width * Height * 3</c>.</param>
public readonly record struct DecodedImage(int Width, int Height, byte[] Rgb);
