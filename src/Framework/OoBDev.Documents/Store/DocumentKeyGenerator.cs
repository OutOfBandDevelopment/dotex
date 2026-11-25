using System;
using System.Globalization;
using System.IO;
namespace OoBDev.Documents.Store;

/// <summary>Keys are <c>yyyy/MM/{guid}{extension}</c> in UTC, so a container folder never grows without bound.</summary>
/// <param name="time">The clock.</param>
public sealed class DocumentKeyGenerator(TimeProvider time) : IDocumentKeyGenerator
{
    /// <inheritdoc />
    public string Create(string fileName) =>
        string.Create(CultureInfo.InvariantCulture, $"{time.GetUtcNow():yyyy/MM}/{Guid.NewGuid():N}{Path.GetExtension(fileName).ToLowerInvariant()}");
}
