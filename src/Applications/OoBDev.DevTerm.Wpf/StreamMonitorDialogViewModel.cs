using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly StreamCaptureConverterOptions? _converterOptions;
    private readonly StreamCaptureConverter? _converter;
    private readonly IReadOnlyList<StreamConversionChoice> _choices = [];
    private int _convertIndex;
    private const string AllKinds = "All types";

    private readonly List<StreamMonitorCapture> _all = [];
    private List<StreamMonitorCapture> _items = [];
    private string _search = string.Empty;
    private int _kindIndex;
    private int _sortIndex;
    private readonly SynchronizationContext? _context = SynchronizationContext.Current;
    private int _selectedIndex = -1;

    /// <summary>Creates the view model over <paramref name="monitor"/> and lists its current captures.</summary>
    /// <param name="monitor">The monitor, with a session already tracked.</param>
    /// <param name="exportDialog">Asks where to export a capture; without it export is unavailable.</param>
    /// <param name="converterOptions">How captures may be converted; without it Convert is unavailable.</param>
    public StreamMonitorDialogViewModel(StreamMonitor monitor, IStreamExportDialog? exportDialog = null, StreamCaptureConverterOptions? converterOptions = null)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        _monitor = monitor;
        _exportDialog = exportDialog;
        if (converterOptions is not null)
        {
            _converterOptions = converterOptions;
            _converter = new StreamCaptureConverter(Microsoft.Extensions.Options.Options.Create(converterOptions));
            _choices = StreamConversionChoice.For(converterOptions);
            _convertIndex = StreamConversionChoice.IndexOf(_choices, converterOptions);
            foreach (var choice in _choices)
            {
                ConvertChoices.Add(choice.DisplayName);
            }
        }

        KindChoices.Add(AllKinds);
        Start = new StartStreamMonitorCommand(this);
        Stop = new StopStreamMonitorCommand(this);
        Export = new ExportStreamCaptureCommand(this);
        OpenFolder = new OpenStreamFolderCommand(this);
        Convert = new ConvertStreamCaptureCommand(this);
        foreach (var capture in monitor.Captures)
        {
            _all.Add(capture);
        }

        Rebuild();
        _monitor.CaptureAdded += OnCaptureAdded;
        _monitor.StateChanged += OnStateChanged;
    }

    /// <summary>One line per capture, oldest first.</summary>
    public ObservableCollection<string> Captures { get; } = [];

    /// <summary>Text the device, type, end state, file name or time must contain.</summary>
    public string Search
    {
        get => _search;
        set
        {
            if (SetProperty(ref _search, value ?? string.Empty))
            {
                Rebuild();
            }
        }
    }

    /// <summary>"All types" and each content type captured so far.</summary>
    public ObservableCollection<string> KindChoices { get; } = [];

    /// <summary>The chosen content type; 0 is every type.</summary>
    public int KindIndex
    {
        get => _kindIndex;
        set
        {
            if (SetProperty(ref _kindIndex, value))
            {
                Rebuild();
            }
        }
    }

    /// <summary>The orders the list can take.</summary>
    public IReadOnlyList<StreamCaptureSort> SortChoices { get; } = Enum.GetValues<StreamCaptureSort>();

    /// <summary>The chosen order.</summary>
    public int SortIndex
    {
        get => _sortIndex;
        set
        {
            if (SetProperty(ref _sortIndex, value))
            {
                Rebuild();
            }
        }
    }

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

    /// <summary>Where captures are saved, with a count of any other folders.</summary>
    public string FolderText
    {
        get
        {
            var others = _monitor.ExportDirectories.Count - 1;
            return $"Saving to {StreamMonitor.DisplayPath(_monitor.ExportDirectory)}{(others > 0 ? $" (+{others} more)" : string.Empty)}";
        }
    }

    /// <summary>The outcome of the last export or folder open.</summary>
    public string ExportStatus { get; private set; } = string.Empty;

    /// <summary>Whether the monitor is watching its sessions.</summary>
    public bool IsRunning => _monitor.IsRunning;

    /// <summary>Starts watching.</summary>
    public ICommand Start { get; }

    /// <summary>Stops watching and saves captures in progress.</summary>
    public ICommand Stop { get; }

    /// <summary>Saves a copy of the selected capture where the user chooses.</summary>
    public ICommand Export { get; }

    /// <summary>The ways a capture can be converted.</summary>
    public ObservableCollection<string> ConvertChoices { get; } = [];

    /// <summary>The chosen way; applies to this window only.</summary>
    public int ConvertIndex
    {
        get => _convertIndex;
        set
        {
            if (SetProperty(ref _convertIndex, value) && value >= 0 && value < _choices.Count)
            {
                _choices[value].ApplyTo(_converterOptions!);
            }
        }
    }

    /// <summary>Converts the selected capture the chosen way.</summary>
    public ICommand Convert { get; }

    internal bool CanConvert => _converter is not null && SelectedIndex >= 0 && SelectedIndex < _items.Count;

    /// <summary>Converts the selected capture and lists the result.</summary>
    public async Task ConvertSelectedAsync()
    {
        if (!CanConvert)
        {
            return;
        }

        var capture = _items[SelectedIndex];
        var result = await _converter!.ConvertAsync(capture).ConfigureAwait(true);
        if (result.Success)
        {
            ExportStatus = $"Converted to {(global::System.IO.Path.GetFileName(result.OutputPath))}.";
            if (result.OutputPath is not null)
            {
                _monitor.AddConverted(capture, result.OutputPath);
            }
        }
        else
        {
            ExportStatus = result.Error ?? "The conversion failed.";
        }

        OnPropertyChanged(nameof(ExportStatus));
    }

    /// <summary>Shows the folder captures are saved to.</summary>
    public ICommand OpenFolder { get; }

    internal bool CanOpenFolder => _exportDialog is not null;

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

    internal void OpenSaveFolder()
    {
        var directory = _monitor.ExportDirectory;
        try
        {
            global::System.IO.Directory.CreateDirectory(directory);
            _exportDialog!.OpenFolder(directory);
            ExportStatus = string.Empty;
        }
        catch (Exception ex) when (ex is global::System.IO.IOException or UnauthorizedAccessException or global::System.ComponentModel.Win32Exception)
        {
            ExportStatus = $"Could not open '{directory}': {ex.Message}";
        }

        OnPropertyChanged(nameof(ExportStatus));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _monitor.CaptureAdded -= OnCaptureAdded;
        _monitor.StateChanged -= OnStateChanged;
    }

    /// <summary>Lists a capture the monitor reported, applying the current search, filter and sort.</summary>
    /// <param name="capture">The capture.</param>
    public void AddCapture(StreamMonitorCapture capture)
    {
        _all.Add(capture);
        Rebuild();
    }

    private void Rebuild()
    {
        var kinds = StreamCaptureView.Kinds(_all);
        if (!KindChoices.Skip(1).SequenceEqual(kinds, StringComparer.Ordinal))
        {
            var keep = _kindIndex > 0 && _kindIndex < KindChoices.Count ? KindChoices[_kindIndex] : null;
            KindChoices.Clear();
            KindChoices.Add(AllKinds);
            foreach (var kind in kinds)
            {
                KindChoices.Add(kind);
            }

            _kindIndex = keep is null ? 0 : Math.Max(0, KindChoices.IndexOf(keep));
            OnPropertyChanged(nameof(KindIndex));
        }

        var selected = SelectedIndex >= 0 && SelectedIndex < _items.Count ? _items[SelectedIndex] : null;
        var sort = _sortIndex >= 0 && _sortIndex < SortChoices.Count ? SortChoices[_sortIndex] : StreamCaptureSort.Oldest;
        _items = [.. StreamCaptureView.Apply(_all, _search, _kindIndex > 0 ? KindChoices[_kindIndex] : null, null, sort)];
        Captures.Clear();
        foreach (var capture in _items)
        {
            Captures.Add(capture.Describe());
        }

        _selectedIndex = selected is null ? -1 : _items.IndexOf(selected);
        OnPropertyChanged(nameof(SelectedIndex));
        ShowSelected();
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
        ((CommandBase)Convert).RaiseCanExecuteChanged();
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
