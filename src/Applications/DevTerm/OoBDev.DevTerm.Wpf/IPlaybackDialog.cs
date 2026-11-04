namespace OoBDev.DevTerm.Wpf;

/// <summary>Picks a session log and shows the playback window for it.</summary>
public interface IPlaybackDialog
{
    /// <summary>Asks for a session log file.</summary>
    /// <returns>The chosen path, or null when cancelled.</returns>
    string? PickLog();

    /// <summary>Shows the window modally and disposes <paramref name="playback"/> when it closes.</summary>
    /// <param name="playback">The window view model.</param>
    void Show(PlaybackDialogViewModel playback);
}
