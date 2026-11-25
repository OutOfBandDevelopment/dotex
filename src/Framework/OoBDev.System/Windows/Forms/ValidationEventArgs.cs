using System;

namespace OoBDev.System.Windows.Forms;

/// <summary>The outcome of one validation of a text box: whether the value is valid, and the value. Host-free, so it lives in Framework with the Windows Forms control in <c>OoBDev.Extensions.Windows.Forms</c>.</summary>
public class ValidationEventArgs : EventArgs
{
    /// <summary>Whether <see cref="Value"/> passed validation.</summary>
    public bool IsValid { get; init; }

    /// <summary>The validated text.</summary>
    public string? Value { get; init; }
}
