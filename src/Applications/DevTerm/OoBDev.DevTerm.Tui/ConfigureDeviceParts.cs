using System;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

/// <summary>The dialog and the actions a test can drive without key injection.</summary>
internal sealed record ConfigureDeviceParts(Dialog Dialog, TextField HostField, TextField PortField, Action Read, Action Write, Label Status, View Form, Button WriteButton);
