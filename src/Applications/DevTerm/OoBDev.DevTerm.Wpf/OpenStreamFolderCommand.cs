using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

internal sealed class OpenStreamFolderCommand(StreamMonitorDialogViewModel owner) : CommandBase
{
    public override bool CanExecute(object? parameter) => owner.CanOpenFolder;

    public override void Execute(object? parameter) => owner.OpenSaveFolder();
}
