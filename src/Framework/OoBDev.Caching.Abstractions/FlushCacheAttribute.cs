using System;

namespace OoBDev.Caching;

/// <summary>
/// Attribute to mark methods that should flush cache entries when executed.
/// Can be applied multiple times to flush multiple cache entries.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class FlushCacheAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlushCacheAttribute"/> class with a key formatter.
    /// </summary>
    /// <param name="keyFormatter">The cache key formatter string.</param>
    public FlushCacheAttribute(string keyFormatter) => KeyFormatter = keyFormatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlushCacheAttribute"/> class targeting a specific method.
    /// </summary>
    /// <param name="targetClass">The target class containing the cached method.</param>
    /// <param name="methodName">The name of the method whose cache should be flushed.</param>
    public FlushCacheAttribute(Type targetClass, string methodName) => (TargetClass, MethodName) = (targetClass, methodName);

    /// <summary>
    /// Gets the target class containing the cached method.
    /// </summary>
    public Type? TargetClass { get; }

    /// <summary>
    /// Gets the name of the method whose cache should be flushed.
    /// </summary>
    public string? MethodName { get; }

    /// <summary>
    /// Gets the cache key formatter string.
    /// </summary>
    public string? KeyFormatter { get; }

    /// <summary>
    /// Gets a unique identifier for this attribute instance.
    /// </summary>
    public override object TypeId => this;
}
