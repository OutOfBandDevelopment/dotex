using System;
using OoBDev.DevTerm.Logging;
using Terminal.Gui.Editor;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

/// <summary>The Playback window's controls, for tests: <see cref="Pump"/> is one timer tick; <see cref="Do"/> runs an action and renders its batch, exactly as a button does.</summary>
internal sealed record PlaybackWindowParts(Window Window, PlaybackController Controller, Editor Output, Label PositionLabel, TextField PresentersField, TextField JumpField, Button PlayButton, Action Pump, Action<Func<PlaybackBatch>> Do);
