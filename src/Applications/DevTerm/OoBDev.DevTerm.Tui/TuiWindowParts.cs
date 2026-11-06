using OoBDev.System.IO.Devices;
using OoBDev.System.IO;
using OoBDev.System.IO.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Text;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm;
using OoBDev.DevTerm.Presenters.Text;
using OoBDev.DevTerm.Logging;
using Terminal.Gui.App;
using Terminal.Gui.Editor;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

/// <summary>
/// The controls a test needs to drive the TUI headlessly. <see cref="Output"/> is the startup tab's
/// output <see cref="Editor"/> (every pre-multi-tab test drives exactly one tab, so this stays a
/// stable reference rather than a delegate); <see cref="TabsView"/> gives a multi-tab test access to
/// the tab strip itself (tab count, headers, and switching the active tab via its <c>Value</c>
/// setter). Inject keys into <see cref="SendField"/>, read rendered text back from <see cref="Output"/>,
/// drive a live profile switch directly via <see cref="SwitchProfileAsync"/> (the same delegate the
/// "File &gt; Device Profiles..." menu item calls, acting on whichever tab is active), switch the send
/// format via <see cref="SetParser"/> (what a "Send as" menu item calls), or drive
/// <see cref="NewSessionMenuItem"/>/<see cref="CloseSessionMenuItem"/> directly; plus the
/// connection-state status line, the three Device menu items, <see cref="ToggleConnectionAsync"/> -
/// exactly what File &gt; Connect/Disconnect runs, including refreshing everything that follows the
/// connection state - and <see cref="AllSessions"/>, which <see cref="TuiMode.RunAsync"/> uses to
/// close every open tab's session when the loop ends.
/// </summary>
internal sealed record TuiWindowParts(
    Window Window,
    Editor Output,
    TextField SendField,
    MenuItem ConnectMenuItem,
    Func<CliOptions, Task<bool>> SwitchProfileAsync,
    Action<string> SetParser,
    Label StatusLabel,
    MenuItem K8055MenuItem,
    MenuItem BusylightMenuItem,
    MenuItem ScpiMenuItem,
    Func<Task> ToggleConnectionAsync,
    TuiLoggingParts Logging,
    TuiThemeMenu ThemeMenu,
    Func<Session> CurrentSession,
    MenuItem StreamMonitorMenuItem,
    Func<StreamMonitor?> CurrentStreamMonitor,
    Tabs TabsView,
    MenuItem NewSessionMenuItem,
    MenuItem CloseSessionMenuItem,
    Func<IReadOnlyList<Session>> AllSessions,
    Action CleanupAllTabs,
    MenuItem EchoSentCommandsMenuItem,
    MenuItem ClearOutputMenuItem,
    MenuItem SoftwareFlowControlMenuItem,
    Func<IReadOnlyList<MenuItem>> SendAsItems);

