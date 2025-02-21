using OoBDev.System.Security.Cryptography;
using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Represents a descriptor for a Handlebars helper that calculates the hash of a given string.
/// </summary>
public class HashHelperDescriptor : HelperDescriptorBase
{
    private readonly IHash _hash;

    /// <summary>
    /// Initializes a new instance of the <see cref="HashHelperDescriptor"/> class.
    /// </summary>
    /// <param name="hash">The hash provider.</param>
    public HashHelperDescriptor(
        IHash hash
        ) => _hash = hash;

    /// <summary>
    /// Gets the name of the helper.
    /// </summary>
    /// <remarks>
    /// This property returns the name of the Handlebars helper, which is "hash".
    /// </remarks>
    public override PathInfo Name => "hash";

    /// <summary>
    /// Gets the Handlebars helper associated with this descriptor.
    /// </summary>
    /// <remarks>
    /// This property returns a Handlebars helper function that calculates the hash of a given string
    /// using the provided hash provider.
    /// </remarks>
    protected override HandlebarsHelper Helper => (output, context, arguments) =>
    {
        var arg = arguments[0];
        var result = _hash.GetHash(arg?.ToString() ?? "");
        output.WriteSafeString(result);
    };
}
