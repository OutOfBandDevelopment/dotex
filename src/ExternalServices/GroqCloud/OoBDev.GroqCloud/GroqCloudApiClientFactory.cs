using GroqNet;
using GroqNet.ChatCompletions;
using Microsoft.Extensions.Options;
using System;

namespace OoBDev.GroqCloud;

/// <summary>
/// Factory class for creating instances of the <see cref="GroqClient"/>.
/// </summary>
public class GroqCloudApiClientFactory : IGroqCloudApiClientFactory
{
    private readonly IOptions<GroqCloudApiClientOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroqCloudApiClientFactory"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options for configuring the GroqCloud API client.</param>
    public GroqCloudApiClientFactory(
        IOptions<GroqCloudApiClientOptions> options
      ) => _options = options;

    /// <summary>
    /// Builds a new instance of the <see cref="GroqClient"/> with the specified host.
    /// </summary>
    /// <returns>A new instance of the <see cref="GroqClient"/>.</returns>
    public GroqClient Build() => new(
        _options.Value.ApiKey ?? Environment.GetEnvironmentVariable("API_Key_Groq", EnvironmentVariableTarget.User) ?? throw new NotSupportedException("Missing API Key for Groq"),
        new GroqModel(_options.Value.Model)
        );
}
