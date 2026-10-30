using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the connection editor modally.</summary>
public interface IConnectionEditorDialog
{
    /// <summary>Shows <paramref name="editor"/> and returns the options it produced, or null when the user cancelled.</summary>
    /// <param name="editor">The editor view model.</param>
    CliOptions? Show(ConnectionEditorViewModel editor);
}
