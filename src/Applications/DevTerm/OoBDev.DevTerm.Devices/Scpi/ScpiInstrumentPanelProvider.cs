using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OoBDev.Scpi;

namespace OoBDev.DevTerm.Devices.Scpi;

/// <summary>
/// SCPI as an <see cref="IInstrumentPanelProvider"/>: one Device-menu item, a picker over the <see cref="ScpiProfileCatalog"/> plus the
/// auto-detect and generic choices, and a panel built from the chosen profile.
/// </summary>
public sealed class ScpiInstrumentPanelProvider : IInstrumentPanelProvider
{
    /// <summary>The picker entry that asks the instrument with <c>*IDN?</c>.</summary>
    public const string AutoDetectChoiceName = "Auto-detect (*IDN?)";

    private readonly ScpiProfileCatalog _catalog;

    /// <summary>Creates the provider over the bundled profiles.</summary>
    public ScpiInstrumentPanelProvider()
        : this(ScpiProfileCatalog.Create())
    {
    }

    /// <summary>Creates the provider over a given catalog.</summary>
    /// <param name="catalog">The profiles to offer.</param>
    public ScpiInstrumentPanelProvider(ScpiProfileCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    /// <inheritdoc />
    public string Id => "scpi";

    /// <inheritdoc />
    public string MenuTitle => "_SCPI Instrument...";

    /// <inheritdoc />
    public string PickerTitle => "Select SCPI Instrument";

    /// <inheritdoc />
    public string PresenterName => "scpi";

    /// <inheritdoc />
    public string AutoDetectChoice => AutoDetectChoiceName;

    /// <inheritdoc />
    public string GenericChoice => ScpiProfileCatalog.Generic.Name;

    /// <inheritdoc />
    public IReadOnlyList<string> ProfileNames => [.. _catalog.Profiles.Select(p => p.Name)];

    /// <inheritdoc />
    /// <remarks>SCPI is text over a byte stream, so any transport but HID (fixed-size binary reports) qualifies.</remarks>
    public bool IsAvailable(string transport, int vendorId, int productId) =>
        !string.Equals(transport, "hid", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool ConfigureForProfile(string profileName, IPresenter? presenter)
    {
        if (Find(profileName) is not { } profile)
        {
            return false;
        }

        (presenter as LineReplyPresenter)?.ConfigureTerminator(profile.Terminator);
        return true;
    }

    /// <inheritdoc />
    public InstrumentPanel Open(string choice, Session session, IPresenter? structuredSource)
    {
        ArgumentNullException.ThrowIfNull(session);

        var profile = Find(choice) ?? ScpiProfileCatalog.Generic;
        (structuredSource as LineReplyPresenter)?.ConfigureTerminator(profile.Terminator);

        return new InstrumentPanel(
            ScpiUiDefinitionBuilder.Build(profile),
            new ScpiControlSurface(session, profile, structuredSource as IReplyTracker),
            $"dev-term — {profile.Name}");
    }

    /// <inheritdoc />
    public string DetectProgressMessage(TimeSpan timeout) => ScpiAutoDetect.ProgressMessage(timeout);

    /// <inheritdoc />
    public async Task<InstrumentDetection> DetectAsync(Session session, IPresenter? structuredSource, TimeSpan timeout)
    {
        var result = await ScpiAutoDetect.DetectAsync(session, structuredSource, _catalog, timeout).ConfigureAwait(false);
        return new InstrumentDetection(result.Profile?.Name ?? GenericChoice, result.Describe(timeout));
    }

    private ScpiInstrumentProfile? Find(string name) =>
        string.Equals(name, GenericChoice, StringComparison.OrdinalIgnoreCase)
            ? ScpiProfileCatalog.Generic
            : _catalog.Profiles.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
}
