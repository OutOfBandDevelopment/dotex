using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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
    private string _presenterText;
    private string _noteText = string.Empty;
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
        _presenterText = string.Join(", ", controller.Presenters);
        ApplyPresenters = new PlaybackCommand(this, c => c.SetPresenters(PresenterText.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries)));
        PlayPause = new PlaybackCommand(this, c => c.TogglePlayPause());
        Step = new PlaybackCommand(this, c => c.Step());
        Rewind = new PlaybackCommand(this, c => c.Rewind());
        FastForward = new PlaybackCommand(this, c => c.FastForward());
        SkipToEnd = new PlaybackCommand(this, c => c.SkipToEnd());
        Jump = new PlaybackCommand(this, c => c.JumpTo(JumpText));
        AddNote = new PlaybackCommand(this, c => c.AddNote(NoteText));
        MarkIn = new PlaybackCommand(this, c =>
        {
            c.MarkIn();
            return PlaybackBatch.Empty;
        });
        MarkOut = new PlaybackCommand(this, c =>
        {
            c.MarkOut();
            return PlaybackBatch.Empty;
        });
        SaveTrim = new PlaybackCommand(this, c =>
        {
            var path = c.DefaultTrimPath();
            c.SaveSelection(path);
            StatusText = $"Saved {path}";
            return PlaybackBatch.Empty;
        });
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

    /// <summary>The presenters the log is replayed through, separated by commas.</summary>
    public string PresenterText
    {
        get => _presenterText;
        set => SetProperty(ref _presenterText, value);
    }

    /// <summary>The presenter names that can be chosen.</summary>
    public IReadOnlyList<string> AvailablePresenters => _controller.AvailablePresenters;

    /// <summary>Replays up to the current position through <see cref="PresenterText"/>.</summary>
    public ICommand ApplyPresenters { get; }

    /// <summary>The text of the note to add at the current position.</summary>
    public string NoteText
    {
        get => _noteText;
        set => SetProperty(ref _noteText, value);
    }

    /// <summary>Adds <see cref="NoteText"/> at the current position and saves the log.</summary>
    public ICommand AddNote { get; }

    /// <summary>Starts the trim selection at the current position.</summary>
    public ICommand MarkIn { get; }

    /// <summary>Ends the trim selection at the current position.</summary>
    public ICommand MarkOut { get; }

    /// <summary>Saves the selection as a new log next to the original.</summary>
    public ICommand SaveTrim { get; }

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
            StatusText = string.Empty;
            Apply(action(_controller));
        }
        catch (Exception ex) when (ex is FormatException or InvalidOperationException or ArgumentException or IOException)
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
