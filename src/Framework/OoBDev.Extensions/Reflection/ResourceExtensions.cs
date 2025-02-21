using OoBDev.System.Reflection;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace OoBDev.Extensions.Reflection;

/// <summary>
/// Set of extension methods for embedded resources
/// </summary>
public static class ResourceExtensions
{
    /// <summary>
    /// Lookup resource stream based on filename relative the scope of context
    /// </summary>
    /// <param name="context"></param>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    public static Stream? GetResourceStream(this object? context, string resourceName) =>
        context switch
        {
            null => null,
            Type type => type.GetResourceStream(resourceName),
            IResolveType resolve => resolve.ResolveType().GetResourceStream(resourceName),
            Assembly assembly => assembly.GetResourceStream(resourceName),
            _ => context.GetType().GetResourceStream(resourceName)
        };

    /// <summary>
    /// Lookup resource stream based on filename relative the scope of Type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    public static Stream? GetResourceStream(this Type? type, string resourceName) =>
        type == null ? null :
        type.Assembly.GetResourceStream($"{type.Namespace}.{resourceName}") ??
        type.Assembly.GetResourceStream($"{resourceName}")
        ;

    /// <summary>
    /// Lookup resource stream based on filename relative the scope of Type
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    public static Stream? GetResourceStream(this Assembly assembly, string resourceName) =>
        assembly.GetManifestResourceStream($"{resourceName}") ??
        assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{resourceName}")
        ;

    /// <summary>
    /// Lookup resource content based on filename relative the scope of context
    /// </summary>
    /// <param name="context"></param>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    public static async Task<string?> GetResourceAsStringAsync(this object? context, string resourceName)
    {
        var resourceStream = context.GetResourceStream(resourceName);
        if (resourceStream == null)
            return null;
        using var sr = new StreamReader(resourceStream);
        return await sr.ReadToEndAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Lookup resource content based on filename relative the scope of context
    /// </summary>
    /// <param name="context"></param>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    public static string? GetResourceAsString(this object context, string resourceName)
    {
        var resourceStream = context.GetResourceStream(resourceName);
        if (resourceStream == null)
            return null;
        using var sr = new StreamReader(resourceStream);
        return sr.ReadToEnd();
    }
}
