namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Asks the user to confirm an action; a view supplies the dialog.</summary>
public interface IConfirmPrompt
{
    /// <summary>Shows the question.</summary>
    /// <param name="message">The question.</param>
    /// <returns>Whether the user agreed.</returns>
    bool Confirm(string message);
}
