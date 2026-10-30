using System;
using System.Collections.Generic;
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
    private string _jumpText = string.Empty;
    private string _statusText = string.Empty;
    private PlaybackSpeed _speed;

    /// <summary>Creates the view model over <paramref name="controller"/>.</summary>
    /// <param name="controller">The playback controller.</param>
    public PlaybackDialogViewModel(PlaybackController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        _controller = controller;
        _position = controller.PositionText;
        _speed = controller.Speed;
        PlayPause = new PlaybackCommand(this, c => c.TogglePlayPause());
        Step = new PlaybackCommand(this, c => c.Step());
        Rewind = new PlaybackCommand(this, c => c.Rewind());
        FastForward = new PlaybackCommand(this, c => c.FastForward());
        SkipToEnd = new PlaybackCommand(this, c => c.SkipToEnd());
        Jump = new PlaybackCommand(this, c => c.JumpTo(JumpText));
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

    /// <summary>The speeds to choose from.</summary>
    public IReadOnlyList<PlaybackSpeed> Speeds => PlaybackController.Speeds;

    /// <summary>The playback speed.</summary>
    public PlaybackSpeed Speed
    {
        get => _speed;
        set
        {
            if (SetProperty(ref _speed, value))
            {
                _controller.SetSpeed(value);
            }
        }
    }

    /// <summary>A record number or log time to jump to.</summary>
    public string JumpText
    {
        get => _jumpText;
        set => SetProperty(ref _jumpText, value);
    }

    /// <summary>The outcome of the last action, such as a jump that could not be read.</summary>
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    /// <summary>Jumps to <see cref="JumpText"/>.</summary>
    public ICommand Jump { get; }

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

    internal void Run(Func<PlaybackController, PlaybackBatch> action)
    {
        try
        {
            Apply(action(_controller));
            StatusText = string.Empty;
        }
        catch (FormatException ex)
        {
            StatusText = ex.Message;
        }
    }

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
