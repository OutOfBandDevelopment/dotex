using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The shell: the registered panels, the open one and the command to open another.</summary>
public sealed class ShellViewModel : ViewModelBase, IDisposable
{
    private readonly ISessionViewModelFactory? _sessionFactory;
    private PanelViewModel? _selected;
    private SessionViewModel? _selectedSession;

    /// <summary>Creates the shell and opens the first panel, if any.</summary>
    /// <param name="factories">The registered panel factories.</param>
    /// <param name="sessionFactory">Creates the sessions the shell opens, or null when it cannot.</param>
    public ShellViewModel(IEnumerable<IPanelFactory> factories, ISessionViewModelFactory? sessionFactory = null)
    {
        ArgumentNullException.ThrowIfNull(factories);
        Factories = [.. factories];
        _sessionFactory = sessionFactory;
        Open = new OpenPanelCommand(this);
        NewLoopbackSession = new NewLoopbackSessionCommand(this);
        OpenProfileSession = new OpenProfileSessionCommand(this);
        if (Factories.Count > 0)
        {
            Open.Execute(Factories[0]);
        }
    }

    /// <summary>The panels that can be opened.</summary>
    public IReadOnlyList<IPanelFactory> Factories { get; }

    /// <summary>Opens the panel of the factory passed as the parameter, replacing the open one.</summary>
    public ICommand Open { get; }

    /// <summary>The open sessions.</summary>
    public ObservableCollection<SessionViewModel> Sessions { get; } = [];

    /// <summary>Opens a loopback session.</summary>
    public ICommand NewLoopbackSession { get; }

    /// <summary>Opens a session from the saved profile named by the command parameter.</summary>
    public ICommand OpenProfileSession { get; }

    /// <summary>The names of the saved connection profiles.</summary>
    public IReadOnlyList<string> ProfileNames => _sessionFactory?.ListProfiles() ?? [];

    /// <summary>The selected session, or null.</summary>
    public SessionViewModel? SelectedSession
    {
        get => _selectedSession;
        set => SetProperty(ref _selectedSession, value);
    }

    internal bool CanCreateSessions => _sessionFactory is not null;

    internal void AddLoopbackSession()
    {
        var session = _sessionFactory!.CreateLoopback();
        Sessions.Add(session);
        SelectedSession = session;
    }

    internal void AddProfileSession(string name)
    {
        var session = _sessionFactory!.CreateFromProfile(name);
        Sessions.Add(session);
        SelectedSession = session;
    }

    /// <summary>The open panel, or null.</summary>
    public PanelViewModel? SelectedPanel
    {
        get => _selected;
        internal set
        {
            var previous = _selected;
            if (SetProperty(ref _selected, value))
            {
                previous?.Dispose();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        SelectedPanel = null;
        foreach (var session in Sessions)
        {
            session.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
