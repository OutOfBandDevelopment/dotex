namespace OoBDev.DevTerm;

/// <summary>What the user decided about running an out-of-process plugin.</summary>
public enum PluginApprovalChoice
{
    /// <summary>Don't run it (and ask again next start).</summary>
    Deny,

    /// <summary>Run it for this start only.</summary>
    Once,

    /// <summary>Run it, and remember the approval for exactly this content (its hash), so it isn't asked about again until it changes.</summary>
    Always,
}
