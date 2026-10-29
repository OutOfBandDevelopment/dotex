namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Accepts the colour a <see cref="ColorPickerViewModel"/> shows.</summary>
/// <param name="picker">The dialog state.</param>
public sealed class AcceptColorCommand(ColorPickerViewModel picker) : CommandBase
{
    /// <inheritdoc />
    public override void Execute(object? parameter) => picker.Close(true);
}
