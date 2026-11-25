namespace OoBDev.System.Sandbox;

/// <summary>Why the broker refused a plugin's context id.</summary>
public enum ContextRefusal
{
    /// <summary>Not refused.</summary>
    None = 0,

    /// <summary>No context with that id is stored (never issued, forged or already released).</summary>
    Unknown = 1,

    /// <summary>The context belongs to another plugin.</summary>
    WrongPlugin = 2,

    /// <summary>The context has expired.</summary>
    Expired = 3,

    /// <summary>The user lacks the right the action needs.</summary>
    MissingRight = 4,
}
