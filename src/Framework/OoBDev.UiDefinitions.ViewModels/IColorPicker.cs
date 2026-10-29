namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Asks the user for a colour; a view supplies the dialog.</summary>
public interface IColorPicker
{
    /// <summary>Shows the picker.</summary>
    /// <param name="current">The current colour as <c>r,g,b</c> (each 0 to 255), or null or blank when there is none.</param>
    /// <returns>The picked colour as <c>r,g,b</c>, or null when the user cancelled.</returns>
    string? Pick(string? current);
}
