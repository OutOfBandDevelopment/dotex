using OoBDev.System.Accessors;
using System.Threading;

namespace OoBDev.Extensions.Accessors;

/// <summary>
/// Represents an accessor for a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the value to be accessed.</typeparam>
internal class Accessor<T> : IAccessor<T>
{
    private readonly AsyncLocal<T?> _local = new();

    /// <summary>
    /// Gets or sets the value associated with this accessor.
    /// </summary>
    public T? Value
    {
        get => _local.Value;
        set => _local.Value = value;
    }
}
