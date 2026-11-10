namespace OoBDev.DevTerm.Wpf;

/// <summary>The prompts of Device &gt; Install plugin: choose the zip, approve what it would run, and show the outcome.</summary>
public interface IPluginInstallDialog
{
    /// <summary>Asks for the plugin .zip; null when the user cancels.</summary>
    string? PickZip();

    /// <summary>Shows what the plugin would run and asks whether to install it, and whether to remember the approval for this content.</summary>
    PluginApprovalChoice Approve(PluginApprovalRequest request);

    /// <summary>Asks whether a plugin that wants to run a program may start (this time, always for this version, or not).</summary>
    PluginApprovalChoice AskPending(PendingPlugin plugin);

    /// <summary>Asks what to do with one remembered approval: keep it, forget it (asked again next start), or stop reviewing.</summary>
    ApprovalReviewChoice AskReview(PluginApproval approval);

    /// <summary>Tells the user there are no remembered plugin approvals.</summary>
    void ReportNoApprovals();

    /// <summary>Tells the user how the install ended.</summary>
    void Report(PluginInstallResult result);
}
