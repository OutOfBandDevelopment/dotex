using System;
using OoBDev.DevTerm.Logging;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>A playback transport action.</summary>
/// <param name="playback">The playback window view model.</param>
/// <param name="action">The controller action to run.</param>
internal sealed class PlaybackCommand(PlaybackDialogViewModel playback, Func<PlaybackController, PlaybackBatch> action) : CommandBase
{
    public override bool CanExecute(object? parameter) => true;

    public override void Execute(object? parameter) => playback.Run(action);
}
