using System;
using System.Globalization;
using System.Threading.Tasks;
using OoBDev.Communications.Abstractions;
using OoBDev.System.Text.Templating;

namespace OoBDev.Communications.Orchestration;

/// <summary>Renders a named template for a request in the target's culture; a missing template gives <c>null</c>.</summary>
internal static class TemplateRenderer
{
    public static async Task<string?> RenderAsync(ITemplateEngine engine, string templateName, CommunicationRequest request, TargetPreference preference)
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = preference.Culture;
        try
        {
            if (engine.Get(templateName) is null)
            {
                return null;
            }

            return await engine.ApplyAsync(templateName, request.Data).ConfigureAwait(false);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    public static string? Address(CommunicationRequest request, string key) =>
        request.Data.TryGetValue(key, out var value) && value is string text && !string.IsNullOrWhiteSpace(text) ? text : null;
}
