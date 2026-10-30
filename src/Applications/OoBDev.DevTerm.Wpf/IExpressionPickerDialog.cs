using OoBDev.DevTerm.DeviceManifests.Editing;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the expression picker.</summary>
public interface IExpressionPickerDialog
{
    /// <summary>Shows the picker modally.</summary>
    /// <param name="picker">The picker view model.</param>
    /// <returns>The accepted text, or null when the user cancels.</returns>
    string? Pick(ExpressionPickerViewModel picker);
}
