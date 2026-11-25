namespace OoBDev.DevTerm.Wpf;

/// <summary>The file pickers the manifest editor needs; each returns null when the user cancels.</summary>
public interface IManifestFileDialogs
{
    /// <summary>Asks for a manifest to edit.</summary>
    string? PickManifestToOpen();

    /// <summary>Asks where to save the manifest.</summary>
    /// <param name="initialDirectory">The folder to start in.</param>
    string? PickSavePath(string initialDirectory);

    /// <summary>Asks for a Kaitai Struct file to import as the binary frame.</summary>
    string? PickKsy();

    /// <summary>Asks for a session log to draw sample values from.</summary>
    string? PickRecording();
}
