using System;
using System.Threading.Tasks;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

internal sealed record RoutingWindowParts(
    Window Window,
    Label Status,
    Label Message,
    Button Protocol,
    TextField Host,
    ListView Rules,
    TextField Match,
    TextField Topic,
    TextField Payload,
    TextField Sample,
    Label TestResult,
    ListView History,
    Button Apply,
    Button StartStop,
    Button Close,
    Action Refresh,
    Action RunTest,
    Func<Task> ApplyAsync);

