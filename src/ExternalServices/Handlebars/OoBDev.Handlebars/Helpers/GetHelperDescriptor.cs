using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Represents a descriptor for a Handlebars helper that retrieves a value from a state store.
/// </summary>
public class GetHelperDescriptor : HelperDescriptorBase
{
    private readonly StateStore _store = new();
    private readonly ILogger _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetHelperDescriptor"/> class.
    /// </summary>
    /// <param name="log">The logger used for logging get operations.</param>
    public GetHelperDescriptor(
        ILogger<GetHelperDescriptor> log
        ) => _log = log;

    /// <summary>
    /// Gets the name of the helper.
    /// </summary>
    public override PathInfo Name => "get";

    /// <summary>
    /// Gets the Handlebars helper associated with this descriptor.
    /// </summary>
    /// <remarks>
    /// This method retrieves the value associated with the specified key from the state store
    /// and writes it to the output.
    /// </remarks>
    protected override HandlebarsHelper Helper => (output, context, arguments) =>
    {
        var key = arguments.ElementAtOrDefault(0)?.ToString();
        _log.LogDebug($"get: {{{nameof(key)}}}", key);
        if (!string.IsNullOrWhiteSpace(key))
        {
            output.Write(_store[key]);
        }
    };
}
