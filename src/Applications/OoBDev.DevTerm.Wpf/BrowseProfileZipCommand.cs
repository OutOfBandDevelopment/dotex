using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Fills the editor's import and export path with an existing zip the user picks.</summary>
/// <param name="editor">The connection editor.</param>
/// <param name="dialogs">The file pickers.</param>
internal sealed class BrowseProfileZipCommand(ConnectionEditorViewModel editor, IProfileZipDialogs dialogs) : CommandBase
{
    public override void Execute(object? parameter)
    {
        if (dialogs.PickZipToImport() is { } path)
        {
            editor.ImportExportPath = path;
        }
    }
}
