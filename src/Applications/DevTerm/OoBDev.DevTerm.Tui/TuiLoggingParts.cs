using System;
using OoBDev.DevTerm.Logging;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

/// <summary>The logging controls a test drives: <see cref="Start"/>/<see cref="Stop"/> are what File &gt; Start/Stop Logging do after the prompt, acting on whichever tab is active.</summary>
internal sealed record TuiLoggingParts(MenuItem MenuItem, Func<string, bool> Start, Action Stop, Func<SessionLogger?> Logger, MenuItem? AddNoteMenuItem = null);
