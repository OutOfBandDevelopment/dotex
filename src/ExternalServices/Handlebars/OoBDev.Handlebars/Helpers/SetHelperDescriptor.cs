using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Represents a descriptor for a Handlebars helper that sets a value in a state store.
/// </summary>
public class SetHelperDescriptor : HelperDescriptorBase
{
    private readonly StateStore _store = new();
    private readonly ILogger _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetHelperDescriptor"/> class.
    /// </summary>
    /// <param name="log">The logger for logging set operations.</param>
    public SetHelperDescriptor(
        ILogger<SetHelperDescriptor> log
        ) => _log = log;

    /// <summary>
    /// Gets the name of the helper.
    /// </summary>
    public override PathInfo Name => "set";

    /// <summary>
    /// Gets the Handlebars helper associated with this descriptor.
    /// </summary>
    protected override HandlebarsHelper Helper => (output, context, arguments) =>
    {
        var key = arguments.ElementAtOrDefault(0)?.ToString();
        var value = arguments.ElementAtOrDefault(1);
        _log.LogDebug($"set: {{{nameof(key)}}}", key);
        if (!string.IsNullOrWhiteSpace(key))
        {
            _store[key] = value;
        }
    };
}
