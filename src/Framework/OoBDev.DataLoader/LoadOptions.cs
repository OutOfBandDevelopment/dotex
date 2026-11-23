using System.Collections.Generic;

namespace OoBDev.DataLoader;

/// <summary>Options for one load.</summary>
public sealed class LoadOptions
{
    /// <summary>Duplicate handling.</summary>
    public DuplicatePolicy Duplicates { get; set; } = DuplicatePolicy.Skip;

    /// <summary>Stop at the first rejected row.</summary>
    public bool StopOnError { get; set; }

    /// <summary>Rows written per sink call when the sink is an <see cref="IBatchRowSink"/>; 1 writes row by row.</summary>
    public int BatchSize { get; set; } = 1;
}
