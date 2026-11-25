using System.Collections.Generic;

namespace OoBDev.DataLoader;

/// <summary>What to do when a row's key already exists in the sink.</summary>
public enum DuplicatePolicy
{
    /// <summary>Keep the existing row.</summary>
    Skip,

    /// <summary>Overwrite the existing row.</summary>
    Replace,

    /// <summary>Report the row as an error.</summary>
    Fail,
}
