using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
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

