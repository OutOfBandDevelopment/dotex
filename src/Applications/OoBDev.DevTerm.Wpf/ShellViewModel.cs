using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The shell: the registered panels, the open one and the command to open another.</summary>
public sealed class ShellViewModel : ViewModelBase, IDisposable
{
    private readonly ISessionViewModelFactory? _sessionFactory;
    private readonly IConnectionEditorDialog? _editorDialog;
    private readonly IRoutingDialog? _routingDialog;
    private PanelViewModel? _selected;
    private SessionViewModel? _selectedSession;
    private int _connectionCount;

    /// <summary>Creates the shell and opens the first panel, if any.</summary>
    /// <param name="factories">The registered panel factories.</param>
    /// <param name="sessionFactory">Creates the sessions the shell opens, or null when it cannot.</param>
    /// <param name="routingDialog">Shows the routing window, or null when the shell has no window to host it.</param>
    /// <param name="editorDialog">Shows the connection editor, or null when the shell has no window to host it.</param>
    public ShellViewModel(IEnumerable<IPanelFactory> factories, ISessionViewModelFactory? sessionFactory = null, IConnectionEditorDialog? editorDialog = null, IRoutingDialog? routingDialog = null)
    {
        ArgumentNullException.ThrowIfNull(factories);
        Factories = [.. factories];
        _sessionFactory = sessionFactory;
        _editorDialog = editorDialog;
        _routingDialog = routingDialog;
        ShowRoutingWindow = new ShowRoutingCommand(this);
        NewConnection = new NewConnectionCommand(this);
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

    /// <summary>Shows the connection editor and opens a session from what it returns.</summary>
    public ICommand NewConnection { get; }

    /// <summary>Shows the routing window for the selected session.</summary>
    public ICommand ShowRoutingWindow { get; }

    internal bool CanShowRouting => _routingDialog is not null && SelectedSession?.Tab is not null;

    internal void ShowRouting() =>
        _routingDialog!.Show(new RoutingDialogViewModel(new RoutingViewModel(SelectedSession!.Tab!, _sessionFactory?.ProfileStore)));

    internal bool CanEditConnections => _sessionFactory is not null && _editorDialog is not null;

    internal void EditConnection()
    {
        var editor = _sessionFactory!.CreateEditor();
        using (editor)
        {
            if (_editorDialog!.Show(editor) is not { } options)
            {
                return;
            }

            var session = _sessionFactory.CreateFromOptions(options, $"{options.Transport} {++_connectionCount}");
            Sessions.Add(session);
            SelectedSession = session;
        }
    }

    /// <summary>The names of the saved connection profiles.</summary>
    public IReadOnlyList<string> ProfileNames => _sessionFactory?.ListProfiles() ?? [];

    /// <summary>The selected session, or null.</summary>
    public SessionViewModel? SelectedSession
    {
        get => _selectedSession;
        set
        {
            if (SetProperty(ref _selectedSession, value))
            {
                ((CommandBase)ShowRoutingWindow).RaiseCanExecuteChanged();
            }
        }
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
