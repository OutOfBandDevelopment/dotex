namespace OoBDev.DevTerm.Wpf;

/// <summary>The file pickers for the profile import and export zip; each returns null when the user cancels.</summary>
public interface IProfileZipDialogs
{
    /// <summary>Asks for an existing zip to import profiles from.</summary>
    string? PickZipToImport();

    /// <summary>Asks where to save the exported profiles zip; the file need not exist.</summary>
    string? PickZipToExport();
}
