using OoBDev.System.Providers;
using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Descriptor for a Handlebars helper that generates a new GUID.
/// </summary>
public class GuidNewHelperDescriptor : HelperDescriptorBase
{
    private readonly IGuidProvider _guid;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuidNewHelperDescriptor"/> class.
    /// </summary>
    /// <param name="guid">The GUID provider.</param>
    public GuidNewHelperDescriptor(
        IGuidProvider guid
        ) => _guid = guid;

    /// <summary>
    /// Gets the name of the helper.
    /// </summary>
    public override PathInfo Name => "guid_new";

    /// <summary>
    /// Gets the Handlebars helper associated with this descriptor.
    /// </summary>
    protected override HandlebarsHelper Helper => (output, context, arguments) =>
        output.WriteSafeString(_guid.NewGuid());
}
