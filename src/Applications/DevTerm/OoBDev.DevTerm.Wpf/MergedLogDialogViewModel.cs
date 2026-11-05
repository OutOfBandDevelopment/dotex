using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The merged log window: every tracked session's traffic in time order, and a command to clear it.</summary>
public sealed class MergedLogDialogViewModel : ViewModelBase, IDisposable
{
    private readonly MergedSessionLog _log;
    private readonly SynchronizationContext? _context = SynchronizationContext.Current;

    /// <summary>Creates the view model over <paramref name="log"/> and lists its current entries.</summary>
    /// <param name="log">The merged log.</param>
    public MergedLogDialogViewModel(MergedSessionLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        _log = log;
        foreach (var entry in log.Entries)
        {
            Lines.Add(entry.ToString());
        }

        Clear = new ClearMergedLogCommand(this);
        _log.EntryAdded += OnEntryAdded;
    }

    /// <summary>One line per entry, oldest first.</summary>
    public ObservableCollection<string> Lines { get; } = [];

    /// <summary>Clears the log and the list.</summary>
    public ICommand Clear { get; }

    internal void ClearLog()
    {
        _log.Clear();
        Lines.Clear();
    }

    /// <inheritdoc />
    public void Dispose() => _log.EntryAdded -= OnEntryAdded;

    private void OnEntryAdded(object? sender, MergedLogEntry entry)
    {
        if (_context is null)
        {
            Lines.Add(entry.ToString());
        }
        else
        {
            _context.Post(_ => Lines.Add(entry.ToString()), null);
        }
    }
}
