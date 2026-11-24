using System;

namespace OoBDev.Imaging.People;

/// <summary>
/// The kinds a run looks for.
/// </summary>
[Flags]
public enum MatchKinds
{
    /// <summary>Nothing.</summary>
    None = 0,

    /// <summary>Faces.</summary>
    Faces = 1,

    /// <summary>Pets.</summary>
    Pets = 2,

    /// <summary>Faces and pets.</summary>
    Both = Faces | Pets,
}
