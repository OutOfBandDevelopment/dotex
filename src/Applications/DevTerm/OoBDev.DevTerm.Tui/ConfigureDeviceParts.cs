using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

/// <summary>The dialog and the actions a test can drive without key injection.</summary>
internal sealed record ConfigureDeviceParts(Dialog Dialog, TextField HostField, TextField PortField, Action Read, Action Write, Label Status, View Form, Button WriteButton);
