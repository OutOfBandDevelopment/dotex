namespace OoBDev.System.Input;

/// <summary>
/// Extends the standard ICommand interface with the ability to manually trigger CanExecuteChanged notifications.
/// This interface is useful for MVVM command implementations that need to notify the UI when the command's executability changes.
/// </summary>
public interface ICommand : global::System.Windows.Input.ICommand
{
    /// <summary>
    /// Raises the CanExecuteChanged event to notify listeners that the result of CanExecute may have changed.
    /// This should be called when factors affecting command executability change.
    /// </summary>
    void RaiseCanExecuteChanged();
}
