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
    private readonly IStreamMonitorDialog? _streamMonitorDialog;
    private readonly IConverterToolsDialog? _converterToolsDialog;
    private readonly IManifestPickerDialog? _manifestPickerDialog;
    private readonly IManifestEditorDialog? _manifestEditorDialog;
    private readonly IManifestFileDialogs? _manifestFiles;
    private readonly IExpressionPickerDialog? _expressionPicker;
    private readonly IStreamExportDialog? _streamExport;
    private readonly IThemeBuilderDialog? _themeBuilderDialog;
    private readonly IConfigureDeviceDialog? _configureDeviceDialog;
    private readonly IInstrumentPickerDialog? _instrumentPickerDialog;
    private readonly IReadOnlyList<OoBDev.System.IO.Devices.IDeviceConfigEditor> _configEditors;
    private readonly string? _themesDirectory;
    private readonly ConverterToolsStore? _converterToolsStore;
    private readonly StreamMonitor? _streamMonitor;
    private readonly IMergedLogDialog? _mergedLogDialog;
    private readonly IPlaybackDialog? _playbackDialog;
    private readonly PlaybackPresenters? _playbackPresenters;
    private readonly MergedSessionLog? _mergedLog;
    private PanelViewModel? _selected;
    private readonly IConfirmPrompt? _confirm;
    private readonly IColorPicker? _colorPicker;
    private SessionViewModel? _selectedSession;
    private int _connectionCount;

    /// <summary>Creates the shell and opens the first panel, if any.</summary>
    /// <param name="factories">The registered panel factories.</param>
    /// <param name="sessionFactory">Creates the sessions the shell opens, or null when it cannot.</param>
    /// <param name="streamMonitorDialog">Shows the stream monitor window, or null when the shell has no window to host it.</param>
    /// <param name="streamMonitor">The stream monitor shared by the shell, or null.</param>
    /// <param name="playbackDialog">Picks and shows session log playback, or null when the shell has no window to host it.</param>
    /// <param name="playbackPresenters">Opens session logs for playback, or null.</param>
    /// <param name="mergedLogDialog">Shows the merged log window, or null when the shell has no window to host it.</param>
    /// <param name="mergedLog">The log every opened session is recorded in, or null.</param>
    /// <param name="routingDialog">Shows the routing window, or null when the shell has no window to host it.</param>
    /// <param name="editorDialog">Shows the connection editor, or null when the shell has no window to host it.</param>
    public ShellViewModel(IEnumerable<IPanelFactory> factories, ISessionViewModelFactory? sessionFactory = null, IConnectionEditorDialog? editorDialog = null, IRoutingDialog? routingDialog = null, IStreamMonitorDialog? streamMonitorDialog = null, StreamMonitor? streamMonitor = null, IMergedLogDialog? mergedLogDialog = null, MergedSessionLog? mergedLog = null, IPlaybackDialog? playbackDialog = null, PlaybackPresenters? playbackPresenters = null, IEnumerable<IDevicePanelContribution>? devicePanels = null, IConfirmPrompt? confirm = null, IColorPicker? colorPicker = null, IConverterToolsDialog? converterToolsDialog = null, ConverterToolsStore? converterToolsStore = null, IManifestPickerDialog? manifestPickerDialog = null, IThemeBuilderDialog? themeBuilderDialog = null, string? themesDirectory = null, IConfigureDeviceDialog? configureDeviceDialog = null, IEnumerable<OoBDev.System.IO.Devices.IDeviceConfigEditor>? configEditors = null, IInstrumentPickerDialog? instrumentPickerDialog = null, IEnumerable<IInstrumentPanelProvider>? instruments = null, IManifestEditorDialog? manifestEditorDialog = null, IManifestFileDialogs? manifestFiles = null, IExpressionPickerDialog? expressionPicker = null, IStreamExportDialog? streamExport = null)
    {
        ArgumentNullException.ThrowIfNull(factories);
        Factories = [.. factories];
        _sessionFactory = sessionFactory;
        _editorDialog = editorDialog;
        _routingDialog = routingDialog;
        _streamMonitorDialog = streamMonitorDialog;
        _converterToolsDialog = converterToolsDialog;
        _manifestPickerDialog = manifestPickerDialog;
        _manifestEditorDialog = manifestEditorDialog;
        _manifestFiles = manifestFiles;
        _expressionPicker = expressionPicker;
        _streamExport = streamExport;
        _themeBuilderDialog = themeBuilderDialog;
        _configureDeviceDialog = configureDeviceDialog;
        _instrumentPickerDialog = instrumentPickerDialog;
        Instruments = [.. instruments ?? []];
        _configEditors = [.. configEditors ?? []];
        _themesDirectory = themesDirectory;
        _converterToolsStore = converterToolsStore;
        _streamMonitor = streamMonitor;
        _mergedLogDialog = mergedLogDialog;
        _mergedLog = mergedLog;
        _playbackDialog = playbackDialog;
        _playbackPresenters = playbackPresenters;
        DevicePanels = [.. devicePanels ?? []];
        _confirm = confirm;
        _colorPicker = colorPicker;
        OpenDevicePanelCommand = new OpenDevicePanelCommand(this);
        OpenPlaybackWindow = new OpenPlaybackCommand(this);
        ShowMergedLogWindow = new ShowMergedLogCommand(this);
        ShowStreamMonitorWindow = new ShowStreamMonitorCommand(this);
        ShowConverterTools = new ShowConverterToolsCommand(this);
        OpenManifestPanel = new OpenManifestCommand(this);
        ShowManifestEditor = new ShowManifestEditorCommand(this);
        BuildTheme = new BuildThemeCommand(this);
        ShowConfigureDevice = new ShowConfigureDeviceCommand(this);
        OpenInstrumentPanel = new OpenInstrumentCommand(this);
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

    /// <summary>Shows the stream monitor for the selected session.</summary>
    public ICommand ShowStreamMonitorWindow { get; }

    /// <summary>Edits the stream monitor's converter tools.</summary>
    public ICommand ShowConverterTools { get; }

    /// <summary>The instrument families plugins contribute (SCPI).</summary>
    public IReadOnlyList<IInstrumentPanelProvider> Instruments { get; }

    /// <summary>Opens the instrument panel of the provider passed as the parameter for the selected session.</summary>
    public ICommand OpenInstrumentPanel { get; }

    /// <summary>Shows the Configure device window.</summary>
    public ICommand ShowConfigureDevice { get; }

    /// <summary>Picks a seed theme and edits a new theme from it.</summary>
    public ICommand BuildTheme { get; }

    /// <summary>Picks a device manifest and opens its panel on the selected session.</summary>
    public ICommand OpenManifestPanel { get; }

    /// <summary>Opens the device manifest editor.</summary>
    public ICommand ShowManifestEditor { get; }

    /// <summary>Shows the log of every open session, merged by time.</summary>
    public ICommand ShowMergedLogWindow { get; }

    /// <summary>Picks a session log and replays it in the playback window.</summary>
    public ICommand OpenPlaybackWindow { get; }

    /// <summary>The device panels plugins contribute.</summary>
    public IReadOnlyList<IDevicePanelContribution> DevicePanels { get; }

    /// <summary>The theme choice.</summary>
    public ThemeViewModel Themes { get; } = new();

    /// <summary>Opens the device panel passed as the parameter for the selected session.</summary>
    public ICommand OpenDevicePanelCommand { get; }

    internal string? DevicePanelUnavailableReason(IDevicePanelContribution panel) =>
        SelectedSession is not { } session
            ? "Open a session first."
            : OoBDev.DevTerm.Configuration.DevicePanels.UnavailableReason(panel, session.Tab?.CliOptions ?? new CliOptions(), session.State == OoBDev.System.IO.ConnectionState.Open);

    internal void OpenDevicePanel(IDevicePanelContribution panel)
    {
        var surface = panel.CreateSurface(SelectedSession!.Session);
        var definition = panel.BuildDefinition();
        var built = OoBDev.DevTerm.Panels.SurfacePanelBuilder.Build(definition, surface);
        OoBDev.UiDefinitions.IStructuredPresenter? source = null;
        if (panel.PresenterName is { } name && SelectedSession.Tab?.Catalog.TryGet(name, out var presenter) == true)
        {
            source = presenter as OoBDev.UiDefinitions.IStructuredPresenter;
        }

        var feed = new DevicePanelFeed(definition, source, built);
        SelectedPanel = new PanelViewModel(panel.MenuTitle.Replace("_", string.Empty, StringComparison.Ordinal).TrimEnd('.'), new FormViewModel(built.Definition, built.Binding, _colorPicker, _confirm, surface as OoBDev.UiDefinitions.ICommandPreview), feed, feed.Displays);
    }

    internal bool CanOpenInstrument(IInstrumentPanelProvider provider) =>
        _instrumentPickerDialog is not null
        && SelectedSession is { Tab: { } tab } session
        && session.State == OoBDev.System.IO.ConnectionState.Open
        && provider.IsAvailable(tab.CliOptions.Transport, tab.CliOptions.VendorId, tab.CliOptions.ProductId);

    internal async global::System.Threading.Tasks.Task OpenInstrumentAsync(IInstrumentPanelProvider provider)
    {
        var selected = SelectedSession!;
        var picker = new InstrumentPickerDialogViewModel(provider);
        if (!_instrumentPickerDialog!.Pick(picker) || picker.Chosen is not { } choice)
        {
            return;
        }

        try
        {
            IPresenter? source = null;
            if (selected.Tab!.Catalog.TryGet(provider.PresenterName, out var presenter))
            {
                selected.Session.AddPresenter(presenter);
                source = presenter;
            }

            if (choice == provider.AutoDetectChoice)
            {
                var timeout = TimeSpan.FromMilliseconds(selected.Tab.CliOptions.ScpiAutoDetectTimeoutMs);
                selected.OutputLines.Add(provider.DetectProgressMessage(timeout));
                var detection = await provider.DetectAsync(selected.Session, source, timeout);
                selected.OutputLines.Add(detection.Message);
                choice = detection.Choice;
            }

            var panel = provider.Open(choice, selected.Session, source);
            var built = OoBDev.DevTerm.Panels.SurfacePanelBuilder.Build(panel.Definition, panel.Surface);
            var feed = new DevicePanelFeed(panel.Definition, source as OoBDev.UiDefinitions.IStructuredPresenter, built);
            SelectedPanel = new PanelViewModel(panel.Title, new FormViewModel(built.Definition, built.Binding, _colorPicker, _confirm, panel.Surface as OoBDev.UiDefinitions.ICommandPreview), feed, feed.Displays);
        }
        catch (Exception ex)
        {
            selected.OutputLines.Add($"Could not open the instrument panel: {ex.Message}");
        }
    }

    internal bool CanConfigureDevice => _configureDeviceDialog is not null && _configEditors.Count > 0;

    internal void ConfigureDevice() => _configureDeviceDialog!.Show(new ConfigureDeviceDialogViewModel(_configEditors));

    internal bool CanBuildTheme => _themeBuilderDialog is not null;

    internal void BuildThemeFromSeed()
    {
        var picker = new ThemeSeedPickerViewModel(_confirm, _themesDirectory);
        if (!_themeBuilderDialog!.PickSeed(picker) || picker.ChosenTheme is not { } seed || picker.ChosenName is not { } name)
        {
            return;
        }

        var builder = new ThemeBuilderDialogViewModel(new ThemeBuilderState(seed, name), _colorPicker, _confirm, _themesDirectory);
        ActiveTheme.Preview(seed);
        try
        {
            _themeBuilderDialog.Edit(builder);
        }
        finally
        {
            ActiveTheme.CancelPreview();
        }
    }

    internal bool CanEditManifest => _manifestEditorDialog is not null && _manifestFiles is not null;

    internal void EditManifest()
    {
        var editor = new OoBDev.DevTerm.DeviceManifests.Editing.ManifestEditorViewModel(OoBDev.DevTerm.Configuration.DevTermUserDataPaths.UserManifestsDirectory, OoBDev.DevTerm.Configuration.DevTermUserDataPaths.AppManifestsDirectory);
        _manifestEditorDialog!.Show(new ManifestEditorDialogViewModel(editor, _manifestFiles!, _colorPicker, _confirm, _expressionPicker));
    }

    internal bool CanOpenManifest => _manifestPickerDialog is not null && SelectedSession is not null;

    internal void OpenManifest()
    {
        var picker = new ManifestPickerDialogViewModel(OoBDev.DevTerm.Configuration.InstalledManifests.Discover());
        if (!_manifestPickerDialog!.Pick(picker) || picker.Chosen is not { } manifest)
        {
            return;
        }

        var panel = OoBDev.DevTerm.DeviceManifests.ManifestPanel.Attach(SelectedSession!.Session, manifest);
        var built = OoBDev.DevTerm.Panels.SurfacePanelBuilder.Build(panel.Definition, panel.Surface);
        var feed = new DevicePanelFeed(panel.Definition, panel.Presenter, new PanelDisposal(built, panel));
        SelectedPanel = new PanelViewModel(manifest.Name, new FormViewModel(built.Definition, built.Binding, _colorPicker, _confirm, panel.Surface as OoBDev.UiDefinitions.ICommandPreview), feed, feed.Displays);
    }

    internal bool CanOpenPlayback => _playbackDialog is not null && _playbackPresenters is not null;

    internal void OpenPlayback()
    {
        if (_playbackDialog!.PickLog() is { } path)
        {
            _playbackDialog.Show(new PlaybackDialogViewModel(_playbackPresenters!.Open(path)));
        }
    }

    internal bool CanShowMergedLog => _mergedLogDialog is not null && _mergedLog is not null;

    internal void ShowMergedLog() => _mergedLogDialog!.Show(new MergedLogDialogViewModel(_mergedLog!));

    private void AddSession(SessionViewModel session)
    {
        Sessions.Add(session);
        session.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SessionViewModel.State))
            {
                ((CommandBase)OpenDevicePanelCommand).RaiseCanExecuteChanged();
                ((CommandBase)OpenInstrumentPanel).RaiseCanExecuteChanged();
            }
        };
        _mergedLog?.Track(session, session.Session, session.Title);
        SelectedSession = session;
    }

    internal bool CanShowStreamMonitor => _streamMonitorDialog is not null && _streamMonitor is not null && SelectedSession is not null;

    internal void ShowStreamMonitor()
    {
        var selected = SelectedSession!;
        _streamMonitor!.Track(selected, selected.Session, selected.Title, DevTermUserDataPaths.ExportsDirectory);
        using var window = new StreamMonitorDialogViewModel(_streamMonitor, _streamExport, StreamCaptureConverterOptions.FromCliOptions(null, _converterToolsStore?.Load()));
        _streamMonitorDialog!.Show(window);
    }

    internal bool CanEditConverterTools => _converterToolsDialog is not null && _converterToolsStore is not null;

    internal void EditConverterTools()
    {
        var dialog = new ConverterToolsDialogViewModel(new ConverterToolsEditor(_converterToolsStore!.Load()));
        if (_converterToolsDialog!.Edit(dialog))
        {
            _converterToolsStore.Save(dialog.Result);
        }
    }

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
            AddSession(session);
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
                ((CommandBase)OpenDevicePanelCommand).RaiseCanExecuteChanged();
                ((CommandBase)OpenInstrumentPanel).RaiseCanExecuteChanged();
                ((CommandBase)ShowStreamMonitorWindow).RaiseCanExecuteChanged();
            }
        }
    }

    internal bool CanCreateSessions => _sessionFactory is not null;

    internal void AddLoopbackSession()
    {
        var session = _sessionFactory!.CreateLoopback();
        AddSession(session);
    }

    internal void AddProfileSession(string name)
    {
        var session = _sessionFactory!.CreateFromProfile(name);
        AddSession(session);
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
