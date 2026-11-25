using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OoBDev.UiDefinitions.ViewModels;
using UiDefinitionsWpf;

namespace OoBDev.Examples.Controls.Wpf;

/// <summary>The live displays (indicator, bar graph, strip chart) in <see cref="LiveDisplayView"/>s, fed by <see cref="LiveSampleFeed"/>.</summary>
public sealed class LiveSample
{
    private readonly LiveSampleFeed _feed = new();

    /// <summary>Creates the displays and the panel that holds them.</summary>
    public LiveSample()
    {
        Displays = [.. LiveSampleFeed.Controls.Select(control => new LiveDisplayViewModel(control))];
        var panel = new StackPanel { Margin = new Thickness(8) };
        panel.Children.Add(new TextBlock { Text = "Live displays", FontWeight = FontWeights.Bold });
        foreach (var display in Displays)
        {
            panel.Children.Add(new LiveDisplayView { DataContext = display });
        }

        Element = panel;
    }

    /// <summary>One view model per live control, in display order.</summary>
    public IReadOnlyList<LiveDisplayViewModel> Displays { get; }

    /// <summary>The panel to place in a window.</summary>
    public FrameworkElement Element { get; }

    /// <summary>Publishes the next simulated sample to every display.</summary>
    public void Step()
    {
        var batch = _feed.Next();
        foreach (var display in Displays)
        {
            display.ApplyAll(batch);
        }
    }
}
