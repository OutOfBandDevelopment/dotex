using System.Runtime.InteropServices;

namespace OoBDev.Scpi;

/// <summary>
/// A command ready to send: the wire text with its terminator.
/// </summary>
/// <param name="WireText">The ASCII text to send, terminator included.</param>
/// <param name="IsQuery">Whether the instrument answers it.</param>
/// <param name="ResponseFormat">What the answer contains.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct ScpiRequest(string WireText, bool IsQuery, ScpiResponseFormat ResponseFormat);
