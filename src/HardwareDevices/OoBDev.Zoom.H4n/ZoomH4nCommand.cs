using System.Runtime.InteropServices;

namespace OoBDev.Zoom.H4n;

/// <summary>
/// One half of a remote button event.
/// </summary>
/// <param name="Button">The button; ignored for <see cref="ZoomH4nAction.Release"/>.</param>
/// <param name="Action">Press or release.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct ZoomH4nCommand(ZoomH4nButton Button, ZoomH4nAction Action = ZoomH4nAction.Press)
{
    /// <summary>The release that ends any press.</summary>
    public static ZoomH4nCommand Release { get; } = new(default, ZoomH4nAction.Release);
}
