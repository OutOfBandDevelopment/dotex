namespace OoBDev.Imaging.People;

/// <summary>
/// A rectangle in image pixels.
/// </summary>
/// <param name="X">The left edge.</param>
/// <param name="Y">The top edge.</param>
/// <param name="Width">The width.</param>
/// <param name="Height">The height.</param>
public readonly record struct ImageBox(float X, float Y, float Width, float Height);
