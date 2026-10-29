using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>A panel generated from <see cref="DemoSettings"/>; shows the shell end to end before devices register panels.</summary>
/// <param name="colorPicker">The dialog colour picker buttons open, or null.</param>
public sealed class DemoPanelFactory(IColorPicker? colorPicker = null) : IPanelFactory
{
    /// <inheritdoc />
    public string Title => "Demo";

    /// <inheritdoc />
    public PanelViewModel Create()
    {
        var settings = new DemoSettings();
        return new PanelViewModel(Title, new FormViewModel(FormDefinitionGenerator.Generate(settings, Title), new FormBinding(settings), colorPicker));
    }
}
