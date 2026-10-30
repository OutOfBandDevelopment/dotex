using System.Windows.Input;

namespace OoBDev.DevTerm.Wpf;

/// <summary>A button beside the form that opens the expression picker for one field.</summary>
/// <param name="Label">The field's label.</param>
/// <param name="Pick">Opens the picker and writes the accepted text back.</param>
public sealed record FieldPickerViewModel(string Label, ICommand Pick);
