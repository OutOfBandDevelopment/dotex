namespace OoBDev.DevTerm.Wpf;

/// <summary>Asks where a session log goes and what a log note says.</summary>
public interface ISessionLogDialog
{
    /// <summary>Asks for the log file.</summary>
    /// <param name="suggestedPath">The default log path to offer.</param>
    /// <returns>The chosen path, or null when the user cancels.</returns>
    string? PickLogPath(string suggestedPath);

    /// <summary>Asks for the text of a note.</summary>
    /// <returns>The note, or null when the user cancels.</returns>
    string? PromptNote();
}
