using System.Text.Json;

namespace OoBDev.AsyncApi;

/// <summary>Serializes AsyncAPI documents.</summary>
public static class AsyncApiJsonWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    /// <summary>Serializes the document to JSON.</summary>
    public static string Write(AsyncApiDocument document) => JsonSerializer.Serialize(document, Options);
}
