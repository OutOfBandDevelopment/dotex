using System.Reflection;

namespace OoBDev.System.Utilities;

/// <summary>
/// Provides functionality to format strings based on method signatures and arguments.
/// </summary>
public interface IStringFormatter
{
    /// <summary>
    /// Formats a string using the specified key formatter, method information, and arguments.
    /// </summary>
    /// <param name="keyFormatter">The key formatter pattern to use.</param>
    /// <param name="method">The method information to include in formatting.</param>
    /// <param name="args">The method arguments to include in formatting.</param>
    /// <returns>The formatted string, or null if formatting fails.</returns>
    string? Format(string keyFormatter, MethodInfo method, object?[]? args);
}
