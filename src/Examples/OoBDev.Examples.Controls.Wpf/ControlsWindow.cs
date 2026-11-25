using System.Windows;
using System.Windows.Controls;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;
using UiDefinitionsWpf;

namespace OoBDev.Examples.Controls.Wpf;

/// <summary>Shows the form generated from <see cref="ControlsSampleSettings"/> in a <see cref="FormView"/>, above the <see cref="LiveSample"/> displays.</summary>
public static class ControlsWindow
{
    /// <summary>Builds the window and its view model without showing it.</summary>
    public static Window Build(ControlsSampleSettings settings, out FormViewModel form) => Build(settings, out form, out _);

    /// <summary>Builds the window, its form and its live displays without showing it; the caller steps <paramref name="live"/> on a timer.</summary>
    public static Window Build(ControlsSampleSettings settings, out FormViewModel form, out LiveSample live)
    {
        form = new FormViewModel(FormDefinitionGenerator.Generate(settings), new FormBinding(settings));
        live = new LiveSample();
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var formView = new FormView { DataContext = form };
        Grid.SetRow(formView, 0);
        Grid.SetRow(live.Element, 1);
        grid.Children.Add(formView);
        grid.Children.Add(live.Element);
        return new Window
        {
            Title = "UiDefinitions controls: WPF",
            Width = 480,
            Height = 760,
            Content = grid,
        };
    }
}
