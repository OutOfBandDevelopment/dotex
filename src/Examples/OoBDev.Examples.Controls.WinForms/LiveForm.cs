using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OoBDev.UiDefinitions.ViewModels;
using OoBDev.UiDefinitions.WinForms;

namespace OoBDev.Examples.Controls.WinForms;

/// <summary>The indicator, bar graph and strip chart of the shared <see cref="LiveSampleFeed"/>, drawn with the Windows Forms renderer.</summary>
public sealed class LiveForm : Form
{
    private readonly LiveSampleFeed _feed = new();
    private readonly Timer _timer = new() { Interval = (int)LiveSampleFeed.Interval.TotalMilliseconds };

    /// <summary>Creates one view and view model per live control; the timer publishes while the form is open.</summary>
    public LiveForm()
    {
        Text = "OoBDev live displays";
        AutoSize = true;
        Displays = [.. LiveSampleFeed.Controls.Select(control => new LiveDisplayViewModel(control))];
        var stack = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(8) };
        foreach (var display in Displays)
        {
            stack.Controls.Add(new LiveDisplayView(display));
        }

        Controls.Add(stack);
        _timer.Tick += (_, _) => Step();
        Shown += (_, _) => _timer.Start();
        FormClosed += (_, _) => _timer.Stop();
    }

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

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
        }

        base.Dispose(disposing);
    }
}
