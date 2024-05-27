namespace OoBDev.System.Input;

public interface ICommand : System.Windows.Input.ICommand
{
    void RaiseCanExecuteChanged();
}
