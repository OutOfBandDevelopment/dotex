namespace OoBDev.DevTerm;

/// <summary>What the user decided about one remembered approval while reviewing them (<see cref="PluginTrust.Review"/>).</summary>
public enum ApprovalReviewChoice
{
    /// <summary>Keep the approval.</summary>
    Keep,

    /// <summary>Forget it: the plugin asks again next start.</summary>
    Forget,

    /// <summary>Stop reviewing; the rest stay as they are.</summary>
    Stop,
}
