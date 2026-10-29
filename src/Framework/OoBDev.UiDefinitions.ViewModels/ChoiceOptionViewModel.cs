using System.Windows.Input;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>One option of a choice row, with the command that picks or checks it, so a view binds each option with no code.</summary>
public sealed class ChoiceOptionViewModel : ViewModelBase
{
    private bool _isSelected;
    private ICommand _command;

    /// <summary>Creates the option.</summary>
    /// <param name="text">The option text.</param>
    /// <param name="command">The command that picks (or checks) it; its parameter is <paramref name="text"/>.</param>
    public ChoiceOptionViewModel(string text, ICommand command)
    {
        Text = text;
        _command = command;
    }

    /// <summary>The option text.</summary>
    public string Text { get; }

    /// <summary>Picks or checks the option; pass <see cref="Text"/> as the parameter.</summary>
    public ICommand Command
    {
        get => _command;
        internal set => SetProperty(ref _command, value);
    }

    /// <summary>Whether the option is the picked one (dropdown, radio group) or checked (check list).</summary>
    public bool IsSelected
    {
        get => _isSelected;
        internal set => SetProperty(ref _isSelected, value);
    }
}
