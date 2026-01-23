using OoBDev.Data.Vectors.Hosting;

namespace OoBDev.Common.Hosting;

/// <summary>
/// Represents a builder for configuring hosting extensions.
/// </summary>
public record HostingBuilder
{
    /// <summary>
    /// Gets or sets a value indicating whether MailKit functionality should be disabled.
    /// </summary>
    /// <remarks>
    /// Set to <c>true</c> to disable MailKit functionality; otherwise, set to <c>false</c>.
    /// The default value is <c>false</c>.
    /// </remarks>
    public bool DisableMailKit { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether message queueing should be disabled.
    /// </summary>
    /// <remarks>
    /// Set to <c>true</c> to disable message queueing; otherwise, set to <c>false</c>.
    /// The default value is <c>false</c>.
    /// </remarks>
    public bool DisableMessageQueueing { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether vector hosting should be disabled.
    /// </summary>
    public bool DisableVectorHosting { get; init; } = false;

    /// <summary>
    /// Gets or sets the configuration section name for embedding sentence transformer queue reader options.
    /// </summary>
    public string EmbeddingSentenceTransformerQueueReaderConfigurationSection { get; init; } = nameof(EmbeddingSentenceTransformerQueueReaderOptions);
}
