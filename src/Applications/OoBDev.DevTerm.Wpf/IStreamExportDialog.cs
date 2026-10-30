namespace OoBDev.DevTerm.Wpf;

/// <summary>Asks where to export a stream capture.</summary>
public interface IStreamExportDialog
{
    /// <summary>Asks for a destination file.</summary>
    /// <param name="suggestedName">The file name to offer.</param>
    /// <param name="extension">The capture's extension without the dot.</param>
    /// <param name="kindName">The capture's display name, for the file filter.</param>
    /// <returns>The chosen path, or null when the user cancels.</returns>
    string? PickExportPath(string suggestedName, string extension, string kindName);
}
