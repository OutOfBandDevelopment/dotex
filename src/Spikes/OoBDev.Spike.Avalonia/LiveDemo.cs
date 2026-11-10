using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using OoBDev.Examples.Controls;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.Spike.Avalonia;

/// <summary>The indicator, bar graph and strip chart of the shared <see cref="LiveSampleFeed"/>, drawn in Avalonia.</summary>
public sealed class LiveDemo
{
    private readonly LiveSampleFeed _feed = new();

    /// <summary>Creates one view model per live control.</summary>
    public LiveDemo() => Displays = [.. LiveSampleFeed.Controls.Select(control => new LiveDisplayViewModel(control))];

    /// <summary>One view model per live control, in display order.</summary>
    public IReadOnlyList<LiveDisplayViewModel> Displays { get; }

    /// <summary>Publishes the next simulated sample to every display.</summary>
    public void Step()
    {
        var batch = _feed.Next();
        foreach (var display in Displays)
        {
            display.ApplyAll(batch);
        }
    }

    /// <summary>A window with every display; with <paramref name="run"/> a timer publishes a sample every <see cref="LiveSampleFeed.Interval"/>.</summary>
    public Window CreateWindow(bool run)
    {
        var panel = new StackPanel { Margin = new global::Avalonia.Thickness(8), Spacing = 8 };
        foreach (var display in Displays)
        {
            panel.Children.Add(LiveDisplayView.Create(display));
        }

        var window = new Window { Title = "OoBDev live displays", Width = 420, Height = 400, Content = panel };
        if (run)
        {
            var timer = new DispatcherTimer { Interval = LiveSampleFeed.Interval };
            timer.Tick += (_, _) => Step();
            window.Opened += (_, _) => timer.Start();
            window.Closed += (_, _) => timer.Stop();
        }

        return window;
    }
}
