using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.Tui;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace OoBDev.Examples.Controls.Tui;

/// <summary>Shows the form generated from <see cref="ControlsSampleSettings"/> through <see cref="FormRenderer"/>.</summary>
public static class ControlsWindow
{
    /// <summary>Builds the window and its form without running it; dispose <paramref name="binding"/> with the window.</summary>
    public static Window Build(IApplication app, ControlsSampleSettings settings, out FormBinding binding, out TuiFormParts parts)
    {
        binding = new FormBinding(settings);
        parts = FormRenderer.Build(app, FormDefinitionGenerator.Generate(settings), binding, new TuiFormOptions { CollapsibleSections = false });
        var window = new Window { Title = "UiDefinitions controls: Tui (Esc to quit)", Width = Dim.Fill(), Height = Dim.Fill() };
        window.Add(parts.Root);
        return window;
    }
}
