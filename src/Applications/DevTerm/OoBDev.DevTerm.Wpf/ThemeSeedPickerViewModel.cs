using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The first step of Build/Edit Theme: pick the theme to start from and name the new theme, or delete a saved theme.</summary>
public sealed class ThemeSeedPickerViewModel : ViewModelBase
{
    private readonly IConfirmPrompt? _confirm;
    private readonly string? _themesDirectory;
    private IReadOnlyList<ThemeSeedOption> _options = [];
    private int _selectedIndex = -1;
    private string _name = string.Empty;
    private string _message = string.Empty;

    /// <summary>Creates the view model over <see cref="ActiveTheme.Catalog"/>.</summary>
    /// <param name="confirm">Asks before a saved theme is deleted; without it deleting is not offered.</param>
    /// <param name="themesDirectory">The user themes directory; null for the default.</param>
    public ThemeSeedPickerViewModel(IConfirmPrompt? confirm = null, string? themesDirectory = null)
    {
        _confirm = confirm;
        _themesDirectory = themesDirectory;
        Delete = new ThemeBuilderCommand(() => _confirm is not null && SelectedIndex >= 0, DeleteSelected);
        Reload();
    }

    /// <summary>The themes to start from.</summary>
    public IReadOnlyList<ThemeSeedOption> Options
    {
        get => _options;
        private set => SetProperty(ref _options, value);
    }

    /// <summary>The selected option, or -1.</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (SetProperty(ref _selectedIndex, value))
            {
                ((CommandBase)Delete).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>The new theme's name.</summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>The outcome of the last delete, or empty.</summary>
    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    /// <summary>Deletes the selected saved theme after confirming.</summary>
    public ICommand Delete { get; }

    /// <summary>The chosen seed theme once <see cref="TryChoose"/> returned true.</summary>
    public DevTermTheme? ChosenTheme { get; private set; }

    /// <summary>The new theme's name once <see cref="TryChoose"/> returned true.</summary>
    public string? ChosenName { get; private set; }

    /// <summary>Resolves the selected seed; false when nothing is selected.</summary>
    public bool TryChoose()
    {
        if (SelectedIndex < 0 || SelectedIndex >= Options.Count)
        {
            return false;
        }

        var option = Options[SelectedIndex];
        ChosenTheme = ActiveTheme.Catalog.Resolve(option.Name, out _, ActiveTheme.PrefersDark);
        ChosenName = string.IsNullOrWhiteSpace(Name) ? DefaultName(option.Label) : Name.Trim();
        return true;
    }

    private void DeleteSelected()
    {
        var option = Options[SelectedIndex];
        if (!ActiveTheme.Catalog.UserThemes.Any(theme => string.Equals(theme.Name, option.Name, StringComparison.OrdinalIgnoreCase)))
        {
            Message = "Built-in themes can't be deleted.";
            return;
        }

        if (!_confirm!.Confirm($"Delete the saved theme \"{option.Name}\"? This can't be undone."))
        {
            return;
        }

        if (ThemeCatalog.Delete(option.Name, _themesDirectory) is { } problem)
        {
            Message = problem;
            return;
        }

        ActiveTheme.UseCatalog(ThemeCatalog.Load(_themesDirectory));
        Message = string.Empty;
        Reload();
    }

    private void Reload()
    {
        Options = [.. ActiveTheme.Catalog.SelectionNames.Select(name => new ThemeSeedOption(name, Label(name)))];
        SelectedIndex = Options.Count > 0 ? 0 : -1;
        if (Options.Count > 0)
        {
            Name = DefaultName(Options[0].Label);
        }
    }

    private static string Label(string name) => name switch
    {
        BuiltInThemes.LightName => "Light",
        BuiltInThemes.DarkName => "Dark",
        BuiltInThemes.SystemName => "System (follow Windows)",
        _ => name,
    };

    private static string DefaultName(string label) => $"{label} copy";
}
