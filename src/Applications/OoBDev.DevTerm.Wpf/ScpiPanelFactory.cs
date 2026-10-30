using System;
using System.Threading.Tasks;
using OoBDev.Scpi;
using OoBDev.Scpi.Panels;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Contributes a panel generated from a SCPI instrument profile.</summary>
/// <param name="profile">The instrument profile.</param>
/// <param name="execute">Sends a request; returns the reply text for a query, or null.</param>
/// <param name="colorPicker">The colour picker for colour fields, if any.</param>
public sealed class ScpiPanelFactory(ScpiInstrumentProfile profile, Func<ScpiRequest, Task<string?>> execute, IColorPicker? colorPicker = null) : IPanelFactory
{
    private readonly ScpiInstrumentProfile _profile = profile ?? throw new ArgumentNullException(nameof(profile));

    /// <inheritdoc />
    public string Title => _profile.Name;

    /// <inheritdoc />
    public PanelViewModel Create()
    {
        var panel = ScpiPanelBuilder.Build(_profile, execute);
        return new PanelViewModel(Title, new FormViewModel(panel.Definition, panel.Binding, colorPicker));
    }
}
