using System.Windows;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;
using UiDefinitionsWpf;

namespace OoBDev.Examples.Controls.Wpf;

/// <summary>Shows the form generated from <see cref="ControlsSampleSettings"/> in a <see cref="FormView"/>.</summary>
public static class ControlsWindow
{
    /// <summary>Builds the window and its view model without showing it.</summary>
    public static Window Build(ControlsSampleSettings settings, out FormViewModel form)
    {
        form = new FormViewModel(FormDefinitionGenerator.Generate(settings), new FormBinding(settings));
        return new Window
        {
            Title = "UiDefinitions controls: WPF",
            Width = 480,
            Height = 520,
            Content = new FormView { DataContext = form },
        };
    }
}
