using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Fills the editor's import and export path with a new zip path the user picks.</summary>
/// <param name="editor">The connection editor.</param>
/// <param name="dialogs">The file pickers.</param>
internal sealed class SaveProfileZipAsCommand(ConnectionEditorViewModel editor, IProfileZipDialogs dialogs) : CommandBase
{
    public override void Execute(object? parameter)
    {
        if (dialogs.PickZipToExport() is { } path)
        {
            editor.ImportExportPath = path;
        }
    }
}
