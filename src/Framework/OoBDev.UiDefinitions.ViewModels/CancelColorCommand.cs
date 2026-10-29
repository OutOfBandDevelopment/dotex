namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Closes a <see cref="ColorPickerViewModel"/> without a colour.</summary>
/// <param name="picker">The dialog state.</param>
public sealed class CancelColorCommand(ColorPickerViewModel picker) : CommandBase
{
    /// <inheritdoc />
    public override void Execute(object? parameter) => picker.Close(false);
}
