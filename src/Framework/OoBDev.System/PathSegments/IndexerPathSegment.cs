﻿namespace OoBDev.System.PathSegments;

public class IndexerPathSegment(
    IPathSegment child
        ) : IPathSegment
{
    public IPathSegment Child { get; } = child;

    public override string ToString() => $"[{Child}]";
}