using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm;

/// <summary>Helpers over <see cref="IInstrumentPanelProvider"/> that every front end shares.</summary>
public static class InstrumentPanelProviderExtensions
{
    /// <summary>Whether <paramref name="choice"/> is something the provider's picker offers (the two synthetic choices or a profile name).</summary>
    public static bool Offers(this IInstrumentPanelProvider provider, string? choice)
    {
        ArgumentNullException.ThrowIfNull(provider);
        return !string.IsNullOrWhiteSpace(choice)
            && (choice == provider.AutoDetectChoice || choice == provider.GenericChoice || provider.ProfileNames.Contains(choice, StringComparer.Ordinal));
    }

    /// <summary>The picker's rows: the auto-detect choice, the generic one, then each profile.</summary>
    public static IReadOnlyList<string> PickerChoices(this IInstrumentPanelProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        return [provider.AutoDetectChoice, provider.GenericChoice, .. provider.ProfileNames];
    }
}
