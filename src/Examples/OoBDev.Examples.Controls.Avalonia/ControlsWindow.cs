using Avalonia.Controls;
using OoBDev.UiDefinitions.Avalonia;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.Examples.Controls.Avalonia;

/// <summary>Shows the form generated from <see cref="ControlsSampleSettings"/> in a <see cref="FormView"/>, above the <see cref="LiveSample"/> displays.</summary>
public static class ControlsWindow
{
    /// <summary>Builds the window and its form without showing it.</summary>
    public static Window Build(ControlsSampleSettings settings, out FormViewModel form)
    {
        form = new FormViewModel(FormDefinitionGenerator.Generate(settings), new FormBinding(settings));
        return new Window
        {
            Title = "UiDefinitions controls: Avalonia",
            Width = 480,
            Height = 760,
            Content = FormView.Create(form),
        };
    }
}
