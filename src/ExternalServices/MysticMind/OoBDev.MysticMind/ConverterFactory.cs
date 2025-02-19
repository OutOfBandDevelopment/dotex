using ReverseMarkdown;

namespace OoBDev.MysticMind;

/// <summary>
/// Factory for creating converters.
/// </summary>
public class ConverterFactory : IConverterFactory
{
    /// <summary>
    /// Builds a new converter.
    /// </summary>
    /// <returns>A new instance of the converter.</returns>
    public Converter Build() => new();
}
