using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Opens the instrument panel of the provider passed as the parameter on the selected session, when the connection suits it.</summary>
/// <param name="shell">The shell.</param>
internal sealed class OpenInstrumentCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => parameter is IInstrumentPanelProvider provider && shell.CanOpenInstrument(provider);

    public override async void Execute(object? parameter)
    {
        if (parameter is IInstrumentPanelProvider provider && CanExecute(provider))
        {
            await shell.OpenInstrumentAsync(provider);
        }
    }
}
