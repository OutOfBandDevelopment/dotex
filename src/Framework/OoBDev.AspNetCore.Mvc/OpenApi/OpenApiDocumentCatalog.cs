using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// The OpenAPI documents served by the application: <see cref="AllDocumentName"/> plus one document per assembly that contains controllers.
/// </summary>
/// <param name="Names">The registered document names.</param>
public record OpenApiDocumentCatalog(IReadOnlyList<string> Names)
{
    /// <summary>
    /// Name of the document that contains every endpoint.
    /// </summary>
    public const string AllDocumentName = "all";

    /// <summary>
    /// Finds the assemblies that contain controllers, starting from the entry assembly, its references and the loaded assemblies.
    /// </summary>
    /// <returns>The assemblies that declare at least one concrete controller.</returns>
    public static IReadOnlyList<Assembly> DiscoverControllerAssemblies()
    {
        var candidates = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
        var entry = Assembly.GetEntryAssembly();
        if (entry != null)
        {
            candidates.Add(entry);
            foreach (var reference in entry.GetReferencedAssemblies().Where(r => !IsPlatform(r.Name)))
            {
                try
                {
                    candidates.Add(Assembly.Load(reference));
                }
                catch (Exception)
                {
                    // optional reference that is not deployed
                }
            }
        }

        return candidates
            .Distinct()
            .Where(a => !a.IsDynamic && !IsPlatform(a.GetName().Name) && HasControllers(a))
            .OrderBy(a => a.GetName().Name, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Creates a catalog from the assemblies that contain controllers.
    /// </summary>
    /// <param name="assemblies">The controller assemblies.</param>
    /// <returns>The catalog.</returns>
    public static OpenApiDocumentCatalog From(IEnumerable<Assembly> assemblies) =>
        new([AllDocumentName, .. assemblies.Select(a => a.GetName().Name!).Distinct()]);

    private static bool IsPlatform(string? name) =>
        name == null || name.StartsWith("System", StringComparison.Ordinal) || name.StartsWith("Microsoft", StringComparison.Ordinal) || name == "netstandard" || name == "mscorlib";

    private static bool HasControllers(Assembly assembly)
    {
        try
        {
            return assembly.GetExportedTypes().Any(t => t is { IsClass: true, IsAbstract: false } && typeof(ControllerBase).IsAssignableFrom(t));
        }
        catch (Exception)
        {
            return false;
        }
    }
}
