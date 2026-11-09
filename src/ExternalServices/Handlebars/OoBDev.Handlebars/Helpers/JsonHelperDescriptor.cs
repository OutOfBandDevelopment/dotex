using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;
using System;
using System.Linq;
using System.Text.Json;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Serializes a value as JSON: <c>{{json value}}</c>, indented with <c>{{json value true}}</c>.
/// </summary>
public class JsonHelperDescriptor : HelperDescriptorBase
{
    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    /// <inheritdoc />
    public override PathInfo Name => "json";

    /// <inheritdoc />
    protected override HandlebarsHelper Helper => (output, context, arguments) =>
    {
        if (arguments.Length == 0)
            return;
        var indent = arguments.ElementAtOrDefault(1) is true || string.Equals(arguments.ElementAtOrDefault(1)?.ToString(), "true", StringComparison.OrdinalIgnoreCase);
        output.WriteSafeString(JsonSerializer.Serialize(arguments[0], indent ? Indented : null));
    };
}
