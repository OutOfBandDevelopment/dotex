namespace OoBDev.Caching;

/// <summary>
/// Factory for creating cacheable proxy instances of interfaces.
/// </summary>
public interface ICacheableFactory
{
    /// <summary>
    /// Creates a cacheable proxy instance for the specified interface and implementation.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to proxy.</typeparam>
    /// <typeparam name="TImplemention">The implementation type to wrap with caching behavior.</typeparam>
    /// <returns>A proxy instance that intercepts method calls and applies caching logic.</returns>
    TInterface Create<TInterface, TImplemention>() where TImplemention : class, TInterface;
}
