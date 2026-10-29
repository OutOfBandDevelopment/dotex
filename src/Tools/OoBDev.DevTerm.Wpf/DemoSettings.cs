namespace OoBDev.DevTerm.Wpf;

/// <summary>A sample settings object the demo panel is generated from, until device panels are registered.</summary>
public sealed class DemoSettings
{
    /// <summary>Whether the demo is enabled.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>A name.</summary>
    public string Name { get; set; } = "Demo";

    /// <summary>A count.</summary>
    public int Count { get; set; } = 3;
}
