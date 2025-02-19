using OoBDev.System.Providers;
using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;
using System.Linq;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Descriptor for a Handlebars helper that provides the current date and time.
/// </summary>
public class DateNowHelperDescriptor : HelperDescriptorBase
{
    private readonly IDateTimeProvider _date;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateNowHelperDescriptor"/> class.
    /// </summary>
    /// <param name="date">The Date provider.</param>
    public DateNowHelperDescriptor(
        IDateTimeProvider date
        ) => _date = date;

    /// <summary>
    /// Gets the name of the helper.
    /// </summary>
    public override PathInfo Name => "date_now";

    /// <summary>
    /// Gets the Handlebars helper associated with this descriptor.
    /// </summary>
    protected override HandlebarsHelper Helper => (output, context, arguments) =>
    {
        var format = arguments.FirstOrDefault() as string;
        if (string.IsNullOrWhiteSpace(format))
            output.WriteSafeString(_date.Now);
        else
            output.WriteSafeString(_date.Now.ToString(format));
    };
}
