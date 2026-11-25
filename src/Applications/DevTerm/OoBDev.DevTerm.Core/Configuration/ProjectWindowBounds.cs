namespace OoBDev.DevTerm.Configuration;

/// <summary>The main window's position and size when a project was saved (device-independent pixels), and whether it was maximized. Only the WPF front end uses it.</summary>
public sealed record ProjectWindowBounds(double Left, double Top, double Width, double Height, bool Maximized);
