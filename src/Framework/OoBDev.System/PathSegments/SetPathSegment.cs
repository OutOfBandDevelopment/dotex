using System.Collections.Generic;

namespace OoBDev.System.PathSegments;

public class SetPathSegment(
    IEnumerable<IPathSegment> set
        ) : IPathSegment
{
    public IEnumerable<IPathSegment> Set { get; } = set;

    public override string ToString() => string.Join(",", Set);

    public static readonly IPathSegment Empty = new SetPathSegment([]);
}