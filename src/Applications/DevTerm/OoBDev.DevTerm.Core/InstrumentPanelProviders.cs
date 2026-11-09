using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.DevTerm;

/// <summary>
/// The instrument providers the loaded plugins registered, for the places that run before or outside a built host (the
/// Connection Editor lists their profile names). Filled by <c>AddPlugins</c>; empty until then.
/// </summary>
public static class InstrumentPanelProviders
{
    private static IReadOnlyList<IInstrumentPanelProvider> _all = [];

    public static IReadOnlyList<IInstrumentPanelProvider> All => _all;

    public static void Set(IEnumerable<IInstrumentPanelProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        _all = [.. providers];
    }

    /// <summary>The picker rows for every provider's profiles, with the synthetic choices once, for a "which instrument" setting.</summary>
    public static IReadOnlyList<string> ProfileChoices() =>
        [.. _all.SelectMany(p => p.PickerChoices()).Distinct(StringComparer.Ordinal)];
}
