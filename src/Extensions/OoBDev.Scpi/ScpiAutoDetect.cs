using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Scpi;

/// <summary>Finds the profile of an instrument that is already connected, by asking it <c>*IDN?</c>.</summary>
public static class ScpiAutoDetect
{
    /// <summary>Sends <c>*IDN?</c> and matches the reply against the catalog.</summary>
    /// <param name="tracker">The tracker wired to the device.</param>
    /// <param name="catalog">The profiles to match.</param>
    /// <param name="terminator">The line ending the instrument expects; the generic profile's when omitted.</param>
    /// <param name="cancellationToken">Gives up the wait.</param>
    /// <returns>The matching profile and the reply text; the profile is <see cref="ScpiProfileCatalog.Generic"/> when nothing matches.</returns>
    /// <exception cref="TimeoutException">The instrument did not answer.</exception>
    public static async Task<(ScpiInstrumentProfile Profile, string Idn)> IdentifyAsync(
        ScpiReplyTracker tracker,
        ScpiProfileCatalog catalog,
        string? terminator = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tracker);
        ArgumentNullException.ThrowIfNull(catalog);

        var asker = terminator is null ? ScpiProfileCatalog.Generic : new ScpiInstrumentProfile { Name = "Identify", Terminator = terminator };
        var reply = await tracker.QueryAsync(asker.BuildCustom("*IDN?", isQuery: true), cancellationToken).ConfigureAwait(false);
        return (catalog.TryMatchByIdn(reply.Text) ?? ScpiProfileCatalog.Generic, reply.Text);
    }
}
