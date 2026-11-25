using System.Windows;
using OoBDev.UiDefinitions.ViewModels;

namespace UiDefinitionsWpf;

/// <summary>Asks with a Yes/No message box.</summary>
/// <param name="owner">The window the box belongs to, or null.</param>
public sealed class WpfConfirmPrompt(Window? owner = null) : IConfirmPrompt
{
    /// <inheritdoc />
    public bool Confirm(string message) =>
        (owner is null ? MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) : MessageBox.Show(owner, message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question)) == MessageBoxResult.Yes;
}
