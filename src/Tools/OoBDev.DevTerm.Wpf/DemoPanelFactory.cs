using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>A panel generated from <see cref="DemoSettings"/>; shows the shell end to end before devices register panels.</summary>
public sealed class DemoPanelFactory : IPanelFactory
{
    /// <inheritdoc />
    public string Title => "Demo";

    /// <inheritdoc />
    public PanelViewModel Create()
    {
        var settings = new DemoSettings();
        return new PanelViewModel(Title, new FormViewModel(FormDefinitionGenerator.Generate(settings, Title), new FormBinding(settings)));
    }
}
