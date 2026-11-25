using System;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The Browse and Save As buttons next to the connection editor's import and export path.</summary>
public sealed class ProfileZipBrowser
{
    /// <summary>Creates the buttons' commands.</summary>
    /// <param name="editor">The connection editor whose path they fill.</param>
    /// <param name="dialogs">The file pickers.</param>
    public ProfileZipBrowser(ConnectionEditorViewModel editor, IProfileZipDialogs dialogs)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentNullException.ThrowIfNull(dialogs);
        Browse = new BrowseProfileZipCommand(editor, dialogs);
        SaveAs = new SaveProfileZipAsCommand(editor, dialogs);
    }

    /// <summary>Picks an existing zip to import.</summary>
    public ICommand Browse { get; }

    /// <summary>Picks a new zip path to export to.</summary>
    public ICommand SaveAs { get; }
}
