using System.Runtime.InteropServices;

namespace OoBDev.Scpi;

/// <summary>
/// One reply line from the instrument, line ending removed.
/// </summary>
/// <param name="Text">The reply text.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct ScpiReply(string Text);
