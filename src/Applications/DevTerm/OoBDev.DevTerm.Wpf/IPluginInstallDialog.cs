namespace OoBDev.DevTerm.Wpf;

/// <summary>The prompts of Device &gt; Install plugin: choose the zip, approve what it would run, and show the outcome.</summary>
public interface IPluginInstallDialog
{
    /// <summary>Asks for the plugin .zip; null when the user cancels.</summary>
    string? PickZip();

    /// <summary>Shows what the plugin would run and asks whether to install it, and whether to remember the approval for this content.</summary>
    PluginApprovalChoice Approve(PluginApprovalRequest request);

    /// <summary>Tells the user how the install ended.</summary>
    void Report(PluginInstallResult result);
}
