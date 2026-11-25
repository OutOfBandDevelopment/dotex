using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;
using OoBDev.System.Accessors;
using OoBDev.System.Providers;
using System.Globalization;
using System.Linq;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Descriptor for a Handlebars helper that writes the text for a key in the request culture: <c>{{translate "greeting"}}</c>.
/// </summary>
/// <param name="translations">The translation lookup.</param>
/// <param name="culture">The accessor holding the culture of the current request.</param>
public class TranslateHelperDescriptor(
    ITranslationProvider translations,
    IAccessor<CultureInfo> culture
    ) : HelperDescriptorBase
{
    /// <summary>
    /// Gets the name of the helper.
    /// </summary>
    public override PathInfo Name => "translate";

    /// <summary>
    /// Gets the Handlebars helper associated with this descriptor. A key with no entry is written as the key itself.
    /// </summary>
    protected override HandlebarsHelper Helper => (output, context, arguments) =>
    {
        var key = arguments.FirstOrDefault()?.ToString();
        if (string.IsNullOrWhiteSpace(key))
            return;

        var text = translations.Translate(key, culture.Value ?? CultureInfo.CurrentUICulture);
        output.Write(text ?? key);
    };
}
