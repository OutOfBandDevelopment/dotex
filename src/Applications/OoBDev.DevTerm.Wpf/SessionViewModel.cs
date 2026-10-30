using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.System.IO;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>One session in the shell: the decoded output, the send box, the connection state and the connect, disconnect and send commands.</summary>
public sealed class SessionViewModel : ViewModelBase, IAsyncDisposable
{
    private readonly Session _session;
    private readonly SynchronizationContext? _context = SynchronizationContext.Current;
    private string _sendText = string.Empty;
    private string? _errorMessage;

    /// <summary>Creates the view model over <paramref name="session"/> and listens to its output.</summary>
    /// <param name="session">The session; the view model owns it.</param>
    /// <param name="title">The tab title.</param>
    /// <param name="tab">The per-connection state (options, routing), or null when the session was not built from options.</param>
    public SessionViewModel(Session session, string title, SessionTab? tab = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        _session = session;
        Title = title;
        Tab = tab;
        Connect = new ConnectSessionCommand(this);
        Disconnect = new DisconnectSessionCommand(this);
        Send = new SendTextCommand(this);
        _session.Output += OnOutput;
        _session.Transport.StateChanged += OnStateChanged;
    }

    /// <summary>The per-connection state the routing window edits, or null.</summary>
    public SessionTab? Tab { get; }

    internal Session Session => _session;

    /// <summary>The tab title.</summary>
    public string Title { get; }

    /// <summary>The decoded lines, oldest first.</summary>
    public ObservableCollection<string> OutputLines { get; } = [];

    /// <summary>The connection state of the session's adapter.</summary>
    public ConnectionState State => _session.State;

    /// <summary>The text the send command writes, followed by a line feed.</summary>
    public string SendText
    {
        get => _sendText;
        set
        {
            if (SetProperty(ref _sendText, value))
            {
                ((CommandBase)Send).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>The last connect or send failure, or null.</summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>Opens the adapter.</summary>
    public ICommand Connect { get; }

    /// <summary>Closes the adapter.</summary>
    public ICommand Disconnect { get; }

    /// <summary>Sends <see cref="SendText"/>.</summary>
    public ICommand Send { get; }

    internal bool CanConnect => State is ConnectionState.Closed or ConnectionState.Faulted;

    internal bool CanDisconnect => State == ConnectionState.Open;

    internal bool CanSend => State == ConnectionState.Open && SendText.Length > 0;

    internal Task ConnectAsync() => RunAsync(() => _session.OpenAsync());

    internal Task DisconnectAsync() => RunAsync(() => _session.CloseAsync());

    internal Task SendAsync() => RunAsync(async () =>
    {
        await _session.SendAsync(Encoding.UTF8.GetBytes(SendText + "\n")).ConfigureAwait(false);
        Post(() => SendText = string.Empty);
    });

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _session.Output -= OnOutput;
        _session.Transport.StateChanged -= OnStateChanged;
        await _session.DisposeAsync().ConfigureAwait(false);
    }

    private async Task RunAsync(Func<Task> action)
    {
        try
        {
            Post(() => ErrorMessage = null);
            await action().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Post(() => ErrorMessage = ex.Message);
        }
    }

    private void OnOutput(object? sender, PresenterOutput output) => Post(() => OutputLines.Add(output.Text));

    private void OnStateChanged(object? sender, ConnectionStateChangedEventArgs e) => Post(() =>
    {
        OnPropertyChanged(nameof(State));
        ((CommandBase)Connect).RaiseCanExecuteChanged();
        ((CommandBase)Disconnect).RaiseCanExecuteChanged();
        ((CommandBase)Send).RaiseCanExecuteChanged();
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
