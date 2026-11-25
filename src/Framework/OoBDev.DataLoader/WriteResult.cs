using System.Collections.Generic;

namespace OoBDev.DataLoader;

/// <summary>What a sink did with a row.</summary>
public enum WriteResult
{
    /// <summary>A new row was stored.</summary>
    Written,

    /// <summary>An existing row was overwritten.</summary>
    Replaced,

    /// <summary>The key existed and the row was left alone.</summary>
    Skipped,

    /// <summary>The key existed and the policy is <see cref="DuplicatePolicy.Fail"/>.</summary>
    Duplicate,
}
