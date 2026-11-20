namespace OoBDev.System.Sandbox;

/// <summary>How a plugin instance relates to its callers (REQ-011).</summary>
public enum PluginLifetime
{
    /// <summary>One instance per lease; it ends with the lease and, through the job's kill-on-close, with the caller.</summary>
    Bound = 0,

    /// <summary>One instance per plugin id and grant set, shared by leases and ended after an idle period.</summary>
    Shared = 1,
}
