using System.Runtime.InteropServices;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// One command for the board.
/// </summary>
/// <param name="Kind">What to do.</param>
/// <param name="Outputs">The outputs to set; only used by <see cref="K8055CommandKind.SetOutputs"/>.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct K8055Command(K8055CommandKind Kind, K8055Outputs Outputs = default)
{
    /// <summary>A command that resets pulse counter 1.</summary>
    public static K8055Command ResetCounter1 { get; } = new(K8055CommandKind.ResetCounter1);

    /// <summary>A command that resets pulse counter 2.</summary>
    public static K8055Command ResetCounter2 { get; } = new(K8055CommandKind.ResetCounter2);

    /// <summary>A command that sets every output.</summary>
    /// <param name="outputs">The new output state.</param>
    public static K8055Command SetOutputs(K8055Outputs outputs) => new(K8055CommandKind.SetOutputs, outputs);
}
