namespace OoBDev.System.Sandbox;

/// <summary>Which limit ended a plugin, as reported by the host.</summary>
public enum LimitHit
{
    /// <summary>No limit was reached.</summary>
    None = 0,

    /// <summary>The per-process memory cap.</summary>
    Memory = 1,

    /// <summary>The processor-time cap.</summary>
    Cpu = 2,

    /// <summary>The wall-clock cap.</summary>
    Wall = 3,

    /// <summary>The process-count cap.</summary>
    Processes = 4,
}
