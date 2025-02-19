using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;
using System.Linq;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Represents a descriptor for a Handlebars helper that performs string replacement.
/// </summary>
public class StringReplaceHelperDescriptor : HelperDescriptorBase
{
    /// <summary>
    /// Gets the name of the helper.
    /// </summary>
    public override PathInfo Name => "str-replace";

    /// <summary>
    /// Gets the Handlebars helper associated with this descriptor.
    /// </summary>
    /// <remarks>
    /// This helper performs string replacement on the input string using the specified find and replacement strings.
    /// </remarks>
    protected override HandlebarsHelper Helper => (output, context, arguments) =>
    {
        var input = arguments.ElementAtOrDefault(0)?.ToString();
        var find = arguments.ElementAtOrDefault(1)?.ToString();
        var replacement = arguments.ElementAtOrDefault(2)?.ToString();
        if (!string.IsNullOrWhiteSpace(input) && !string.IsNullOrWhiteSpace(find))
        {
            output.Write(input.Replace(find, replacement ?? ""));
        }
    };
}
