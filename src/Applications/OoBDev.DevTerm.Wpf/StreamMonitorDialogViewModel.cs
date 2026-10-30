using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The stream monitor window: start and stop, the running state and the captures taken so far.</summary>
public sealed class StreamMonitorDialogViewModel : ViewModelBase, IDisposable
{
    private readonly StreamMonitor _monitor;
    private readonly SynchronizationContext? _context = SynchronizationContext.Current;

    /// <summary>Creates the view model over <paramref name="monitor"/> and lists its current captures.</summary>
    /// <param name="monitor">The monitor, with a session already tracked.</param>
    public StreamMonitorDialogViewModel(StreamMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        _monitor = monitor;
        foreach (var capture in monitor.Captures)
        {
            Captures.Add(capture.Describe());
        }

        Start = new StartStreamMonitorCommand(this);
        Stop = new StopStreamMonitorCommand(this);
        _monitor.CaptureAdded += OnCaptureAdded;
        _monitor.StateChanged += OnStateChanged;
    }

    /// <summary>One line per capture, oldest first.</summary>
    public ObservableCollection<string> Captures { get; } = [];

    /// <summary>Whether the monitor is watching its sessions.</summary>
    public bool IsRunning => _monitor.IsRunning;

    /// <summary>Starts watching.</summary>
    public ICommand Start { get; }

    /// <summary>Stops watching and saves captures in progress.</summary>
    public ICommand Stop { get; }

    internal void StartMonitor() => _monitor.Start();

    internal void StopMonitor() => _monitor.Stop();

    /// <inheritdoc />
    public void Dispose()
    {
        _monitor.CaptureAdded -= OnCaptureAdded;
        _monitor.StateChanged -= OnStateChanged;
    }

    private void OnCaptureAdded(object? sender, StreamMonitorCapture capture) => Post(() => Captures.Add(capture.Describe()));

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
