namespace OoBDev.System.Accessors;

/// <summary>
/// IAccessor[T] is a type that allows for a instance to be bound to a async context
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAccessor<T>
{
    /// <summary>
    /// accessible value
    /// </summary>
    T? Value { get; set; }
}
