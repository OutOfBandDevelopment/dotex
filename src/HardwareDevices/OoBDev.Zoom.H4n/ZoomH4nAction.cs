namespace OoBDev.Zoom.H4n;

/// <summary>
/// Which half of a button event to send. A physical remote sends the press, then the shared release.
/// </summary>
public enum ZoomH4nAction
{
    /// <summary>The two-byte press code of a button.</summary>
    Press,

    /// <summary>The shared two-byte release code (<c>80 00</c>), which ignores the button.</summary>
    Release,
}
