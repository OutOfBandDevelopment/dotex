using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using OoBDev.DevTerm.Logging;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The playback window: a session log replayed through its presenters with transport controls.</summary>
public sealed class PlaybackDialogViewModel : ViewModelBase
{
    private readonly PlaybackController _controller;
    private string _position;

    /// <summary>Creates the view model over <paramref name="controller"/>.</summary>
    /// <param name="controller">The playback controller.</param>
    public PlaybackDialogViewModel(PlaybackController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        _controller = controller;
        _position = controller.PositionText;
        PlayPause = new PlaybackCommand(this, c => c.TogglePlayPause());
        Step = new PlaybackCommand(this, c => c.Step());
        Rewind = new PlaybackCommand(this, c => c.Rewind());
        FastForward = new PlaybackCommand(this, c => c.FastForward());
        SkipToEnd = new PlaybackCommand(this, c => c.SkipToEnd());
    }

    /// <summary>What is being played.</summary>
    public string Description => _controller.Description;

    /// <summary>The current position, for example <c>3/6</c>.</summary>
    public string Position
    {
        get => _position;
        private set => SetProperty(ref _position, value);
    }

    /// <summary>The replayed lines, oldest first.</summary>
    public ObservableCollection<string> Lines { get; } = [];

    /// <summary>Starts or pauses playback.</summary>
    public ICommand PlayPause { get; }

    /// <summary>Plays one record.</summary>
    public ICommand Step { get; }

    /// <summary>Returns to the start.</summary>
    public ICommand Rewind { get; }

    /// <summary>Skips ahead by the fast-forward amount.</summary>
    public ICommand FastForward { get; }

    /// <summary>Plays everything that is left.</summary>
    public ICommand SkipToEnd { get; }

    /// <summary>Advances playback by what the clock allows; called from the window timer.</summary>
    public void Tick() => Apply(_controller.Tick());

    internal void Run(Func<PlaybackController, PlaybackBatch> action) => Apply(action(_controller));

    private void Apply(PlaybackBatch batch)
    {
        if (batch.Reset)
        {
            Lines.Clear();
        }

        foreach (var item in batch.Items)
        {
            foreach (var output in item.Outputs)
            {
                Lines.Add($"{item.Offset.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture)} {output.PresenterName}: {output.Text}");
            }
        }

        Position = _controller.PositionText;
    }
}
