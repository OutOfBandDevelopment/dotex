using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The stream monitor window: start and stop, the running state, the captures taken so far and a preview of the selected one.</summary>
public sealed class StreamMonitorDialogViewModel : ViewModelBase, IDisposable
{
    private const string EmptyMessage = "Nothing captured yet. Send a command that returns an image, plot or print job, or wait for the device to send one.";

    private readonly StreamMonitor _monitor;
    private readonly IStreamExportDialog? _exportDialog;
    private readonly List<StreamMonitorCapture> _items = [];
    private readonly SynchronizationContext? _context = SynchronizationContext.Current;
    private int _selectedIndex = -1;

    /// <summary>Creates the view model over <paramref name="monitor"/> and lists its current captures.</summary>
    /// <param name="monitor">The monitor, with a session already tracked.</param>
    /// <param name="exportDialog">Asks where to export a capture; without it export is unavailable.</param>
    public StreamMonitorDialogViewModel(StreamMonitor monitor, IStreamExportDialog? exportDialog = null)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        _monitor = monitor;
        _exportDialog = exportDialog;
        foreach (var capture in monitor.Captures)
        {
            AddCapture(capture);
        }

        Start = new StartStreamMonitorCommand(this);
        Stop = new StopStreamMonitorCommand(this);
        Export = new ExportStreamCaptureCommand(this);
        _monitor.CaptureAdded += OnCaptureAdded;
        _monitor.StateChanged += OnStateChanged;
    }

    /// <summary>One line per capture, oldest first.</summary>
    public ObservableCollection<string> Captures { get; } = [];

    /// <summary>The selected capture, or -1.</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (SetProperty(ref _selectedIndex, value))
            {
                ShowSelected();
            }
        }
    }

    /// <summary>What the selected capture is.</summary>
    public string Detail { get; private set; } = string.Empty;

    /// <summary>Where the selected capture was saved.</summary>
    public string SavedPath { get; private set; } = string.Empty;

    /// <summary>The selected capture drawn, when it can be.</summary>
    public ImageSource? Preview { get; private set; }

    /// <summary>Why there is no preview, when there is none.</summary>
    public string PreviewMessage { get; private set; } = EmptyMessage;

    /// <summary>The outcome of the last export.</summary>
    public string ExportStatus { get; private set; } = string.Empty;

    /// <summary>Whether the monitor is watching its sessions.</summary>
    public bool IsRunning => _monitor.IsRunning;

    /// <summary>Starts watching.</summary>
    public ICommand Start { get; }

    /// <summary>Stops watching and saves captures in progress.</summary>
    public ICommand Stop { get; }

    /// <summary>Saves a copy of the selected capture where the user chooses.</summary>
    public ICommand Export { get; }

    internal bool CanExport => _exportDialog is not null && SelectedIndex >= 0 && SelectedIndex < _items.Count;

    internal void StartMonitor() => _monitor.Start();

    internal void StopMonitor() => _monitor.Stop();

    internal void ExportSelected()
    {
        var capture = _items[SelectedIndex];
        var kind = capture.Capture.Kind;
        var suggested = capture.SavedPath is { } saved ? global::System.IO.Path.GetFileName(saved) : $"capture.{kind.Extension}";
        if (_exportDialog!.PickExportPath(suggested, kind.Extension, kind.DisplayName) is not { } path)
        {
            return;
        }

        try
        {
            global::System.IO.File.WriteAllBytes(path, capture.Capture.Data);
            ExportStatus = $"Exported to {path}";
        }
        catch (Exception ex) when (ex is global::System.IO.IOException or UnauthorizedAccessException)
        {
            ExportStatus = $"Could not save '{path}': {ex.Message}";
        }

        OnPropertyChanged(nameof(ExportStatus));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _monitor.CaptureAdded -= OnCaptureAdded;
        _monitor.StateChanged -= OnStateChanged;
    }

    private void AddCapture(StreamMonitorCapture capture)
    {
        _items.Add(capture);
        Captures.Add(capture.Describe());
    }

    private void ShowSelected()
    {
        if (SelectedIndex < 0 || SelectedIndex >= _items.Count)
        {
            Preview = null;
            Detail = string.Empty;
            SavedPath = string.Empty;
            PreviewMessage = EmptyMessage;
        }
        else
        {
            var capture = _items[SelectedIndex];
            Detail = capture.Summary;
            SavedPath = capture.SavedPath is { } path ? $"Saved to {StreamMonitor.DisplayPath(path)}" : $"Not saved: {capture.SaveError}";
            Preview = StreamCapturePreview.Render(capture, out var message);
            PreviewMessage = message;
        }

        OnPropertyChanged(nameof(Preview));
        OnPropertyChanged(nameof(PreviewMessage));
        OnPropertyChanged(nameof(Detail));
        OnPropertyChanged(nameof(SavedPath));
        ((CommandBase)Export).RaiseCanExecuteChanged();
    }

    private void OnCaptureAdded(object? sender, StreamMonitorCapture capture) => Post(() => AddCapture(capture));

    private void OnStateChanged(object? sender, EventArgs e) => Post(() =>
    {
        OnPropertyChanged(nameof(IsRunning));
        ((CommandBase)Start).RaiseCanExecuteChanged();
        ((CommandBase)Stop).RaiseCanExecuteChanged();
    });

    private void Post(Action action)
    {
        if (_context is null)
        {
            action();
        }
        else
        {
            _context.Post(_ => action(), null);
        }
    }
}
