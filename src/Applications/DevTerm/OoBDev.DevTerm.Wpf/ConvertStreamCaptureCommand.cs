using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

internal sealed class ConvertStreamCaptureCommand(StreamMonitorDialogViewModel owner) : CommandBase
{
    public override bool CanExecute(object? parameter) => owner.CanConvert;

    public override async void Execute(object? parameter) => await owner.ConvertSelectedAsync();
}
