using System.Windows.Input;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>One entry of the Theme menu: a theme name and whether it is the one in use.</summary>
public sealed class ThemeChoice : ViewModelBase
{
    private bool _isSelected;

    /// <summary>Creates the entry.</summary>
    /// <param name="name">The theme name.</param>
    /// <param name="select">Applies the theme.</param>
    internal ThemeChoice(string name, ICommand select)
    {
        Name = name;
        Select = select;
    }

    /// <summary>The theme name.</summary>
    public string Name { get; }

    /// <summary>Applies this theme.</summary>
    public ICommand Select { get; }

    /// <summary>True when this is the theme in use.</summary>
    public bool IsSelected
    {
        get => _isSelected;
        internal set => SetProperty(ref _isSelected, value);
    }
}
