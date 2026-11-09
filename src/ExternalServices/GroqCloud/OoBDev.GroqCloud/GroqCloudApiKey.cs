using System;

namespace OoBDev.GroqCloud;

/// <summary>
/// Finds the Groq Cloud API key outside the options.
/// </summary>
public static class GroqCloudApiKey
{
    /// <summary>
    /// The environment variable Groq's own tools use.
    /// </summary>
    public const string EnvironmentVariable = "GROQ_API_KEY";

    /// <summary>
    /// The earlier variable name, still read from the user scope.
    /// </summary>
    public const string LegacyEnvironmentVariable = "API_Key_Groq";

    /// <summary>
    /// Returns the key from <see cref="EnvironmentVariable"/> (process, then user scope) or <see cref="LegacyEnvironmentVariable"/>, or <see langword="null"/>.
    /// </summary>
    public static string? FromEnvironment() =>
        NonEmpty(Environment.GetEnvironmentVariable(EnvironmentVariable))
        ?? NonEmpty(Environment.GetEnvironmentVariable(EnvironmentVariable, EnvironmentVariableTarget.User))
        ?? NonEmpty(Environment.GetEnvironmentVariable(LegacyEnvironmentVariable, EnvironmentVariableTarget.User));

    private static string? NonEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
