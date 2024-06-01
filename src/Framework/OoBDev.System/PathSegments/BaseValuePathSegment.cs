namespace OoBDev.System.PathSegments;

public abstract class BaseValuePathSegment<T> : IPathSegment<T>
{
    protected BaseValuePathSegment(T value)
    {
        Value = value;
    }

    public T Value { get; }
    public override string ToString() => $"{Value}";
}
