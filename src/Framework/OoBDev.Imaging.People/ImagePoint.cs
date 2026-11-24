namespace OoBDev.Imaging.People;

/// <summary>
/// A landmark (an eye, a nose tip) in image pixels.
/// </summary>
/// <param name="X">The horizontal position.</param>
/// <param name="Y">The vertical position.</param>
public readonly record struct ImagePoint(float X, float Y);
