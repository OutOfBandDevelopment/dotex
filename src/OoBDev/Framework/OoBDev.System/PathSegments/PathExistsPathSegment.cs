namespace OoBDev.System.PathSegments;

public class PathExistsPathSegment(BinaryPathSegment path) : BaseValuePathSegment<BinaryPathSegment>(path)
{
    public override string ToString() => $"[{Value}]";
}