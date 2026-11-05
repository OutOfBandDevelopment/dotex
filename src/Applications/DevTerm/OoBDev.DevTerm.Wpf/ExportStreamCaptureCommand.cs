using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

internal sealed class ExportStreamCaptureCommand(StreamMonitorDialogViewModel owner) : CommandBase
{
    public override bool CanExecute(object? parameter) => owner.CanExport;

    public override void Execute(object? parameter) => owner.ExportSelected();
}
