namespace OoBDev.System.Input;

public interface ICommand : global::System.Windows.Input.ICommand
{
    void RaiseCanExecuteChanged();
}
