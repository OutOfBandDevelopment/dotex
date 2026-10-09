using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;
using System;
using System.Linq;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Descriptor for a Handlebars helper that provides the current date and time.
/// </summary>
public class DateNowHelperDescriptor : HelperDescriptorBase
{
    private readonly TimeProvider _time;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateNowHelperDescriptor"/> class.
    /// </summary>
    /// <param name="time">The time provider.</param>
    public DateNowHelperDescriptor(
        TimeProvider time
        ) => _time = time;

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
            output.WriteSafeString(_time.GetLocalNow());
        else
            output.WriteSafeString(_time.GetLocalNow().ToString(format));
    };
}
