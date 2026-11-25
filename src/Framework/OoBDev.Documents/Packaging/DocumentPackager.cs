using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.Documents.Packaging;

/// <summary>Picks the package handler by type and refuses entry names that could escape the package.</summary>
/// <param name="handlers">The registered handlers.</param>
public sealed class DocumentPackager(IEnumerable<IDocumentPackageHandler> handlers) : IDocumentPackager
{
    /// <inheritdoc />
    public async Task PackAsync(string packageType, IReadOnlyCollection<PackageItem> items, Stream destination, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(destination);
        var handler = handlers.FirstOrDefault(h => string.Equals(h.PackageType, packageType, StringComparison.OrdinalIgnoreCase))
            ?? throw new PackagingException($"No package handler for '{packageType}'.");
        Validate(items);
        await handler.PackAsync(items, destination, cancellationToken).ConfigureAwait(false);
    }

    private static void Validate(IReadOnlyCollection<PackageItem> items)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            var name = item.EntryName;
            if (string.IsNullOrWhiteSpace(name) || name.StartsWith('/') || name.Contains('\\') || name.Contains(':')
                || name.Split('/').Any(part => part is ".."))
            {
                throw new PackagingException($"Entry name '{name}' must be a relative path with forward slashes and no '..'.");
            }

            if (!seen.Add(name))
            {
                throw new PackagingException($"Entry name '{name}' appears twice.");
            }
        }
    }
}
