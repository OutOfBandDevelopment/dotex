using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm;
using OoBDev.DevTerm.Logging;
using Terminal.Gui.App;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

/// <summary>The logging controls a test drives: <see cref="Start"/>/<see cref="Stop"/> are what File &gt; Start/Stop Logging do after the prompt, acting on whichever tab is active.</summary>
internal sealed record TuiLoggingParts(MenuItem MenuItem, Func<string, bool> Start, Action Stop, Func<SessionLogger?> Logger, MenuItem? AddNoteMenuItem = null);
