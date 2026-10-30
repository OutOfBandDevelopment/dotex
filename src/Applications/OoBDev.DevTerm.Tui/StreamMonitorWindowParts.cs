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
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

/// <summary>The Stream Monitor window's controls, for tests to drive/inspect headlessly — <see cref="Refresh"/> is what the window runs whenever the monitor changes.</summary>
internal sealed record StreamMonitorWindowParts(Window Window, Label StatusLabel, ListView CaptureList, Label DetailLabel, Button ToggleButton, Button ConvertButton, Button CloseButton, Action Refresh, Button ModeButton, StreamCaptureConverterOptions ConverterOptions, TextField SearchField, Button SortButton, Button DeviceButton);
