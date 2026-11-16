using System.ComponentModel.DataAnnotations;

namespace OoBDev.Embeddings.Http;

/// <summary>
/// Settings of an OpenAI-compatible embedding server (<c>POST {BaseUrl}/v1/embeddings</c>).
/// </summary>
public sealed class HttpEmbeddingOptions
{
    /// <summary>The configuration section the options bind from.</summary>
    public const string SectionName = "Embeddings:Http";

    /// <summary>The server root, for example <c>http://localhost:11435</c>.</summary>
    [Required]
    public string BaseUrl { get; set; } = "";

    /// <summary>The model name sent with every request.</summary>
    [Required]
    public string Model { get; set; } = "";

    /// <summary>The vector length the model returns; a response of another length is rejected.</summary>
    [Range(1, 65536)]
    public int Dimensions { get; set; }

    /// <summary>The request timeout in seconds.</summary>
    [Range(1, 600)]
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>The largest number of inputs sent in one request.</summary>
    [Range(1, 2048)]
    public int BatchSize { get; set; } = 32;

    /// <summary>The bearer token, when the server needs one.</summary>
    public string? ApiKey { get; set; }
}
