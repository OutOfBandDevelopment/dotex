using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The theme builder: edit the colours of a <see cref="ThemeBuilderState"/> with a live preview, then save it as a user theme.</summary>
public sealed class ThemeBuilderDialogViewModel : ViewModelBase
{
    private const string _overriddenMarker = "● ";
    private const string _unchangedMarker = "  ";

    private readonly ThemeBuilderState _state;
    private readonly IColorPicker? _colorPicker;
    private readonly IConfirmPrompt? _confirm;
    private readonly string _directory;
    private readonly ThemeRole[] _roles = Enum.GetValues<ThemeRole>();
    private int _selectedRoleIndex = -1;
    private string _name;
    private string _warnings = string.Empty;
    private string _error = string.Empty;

    /// <summary>Creates the view model.</summary>
    /// <param name="state">The theme being edited.</param>
    /// <param name="colorPicker">Edits one colour; without it editing is not offered.</param>
    /// <param name="confirm">Asks before an existing theme is overwritten; without it the file is overwritten.</param>
    /// <param name="themesDirectory">The user themes directory; null for the default.</param>
    public ThemeBuilderDialogViewModel(ThemeBuilderState state, IColorPicker? colorPicker = null, IConfirmPrompt? confirm = null, string? themesDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        _state = state;
        _colorPicker = colorPicker;
        _confirm = confirm;
        _directory = themesDirectory ?? DevTermUserDataPaths.ThemesDirectory;
        _name = state.Name;
        Edit = new ThemeBuilderCommand(() => _colorPicker is not null && HasSelection, EditSelected);
        Reset = new ThemeBuilderCommand(() => HasSelection && _state.IsOverridden(_roles[SelectedRoleIndex]), ResetSelected);
        Refresh();
    }

    /// <summary>The colour roles with their current values.</summary>
    public ObservableCollection<ThemeRoleRow> Roles { get; } = [];

    /// <summary>The chart palettes to choose from.</summary>
    public string[] Palettes { get; } = ["Light", "Dark"];

    /// <summary>The selected role, or -1.</summary>
    public int SelectedRoleIndex
    {
        get => _selectedRoleIndex;
        set
        {
            if (SetProperty(ref _selectedRoleIndex, value))
            {
                OnPropertyChanged(nameof(HasSelection));
                ((CommandBase)Edit).RaiseCanExecuteChanged();
                ((CommandBase)Reset).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Whether a role is selected.</summary>
    public bool HasSelection => SelectedRoleIndex >= 0 && SelectedRoleIndex < _roles.Length;

    /// <summary>The theme's name.</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value) && !string.IsNullOrWhiteSpace(value))
            {
                _state.Name = value.Trim();
                Preview();
            }
        }
    }

    /// <summary>The chart palette: 0 for light, 1 for dark.</summary>
    public int PaletteIndex
    {
        get => _state.ChartPalette == ChartPaletteVariant.Dark ? 1 : 0;
        set
        {
            _state.ChartPalette = value == 1 ? ChartPaletteVariant.Dark : ChartPaletteVariant.Light;
            OnPropertyChanged();
            Preview();
        }
    }

    /// <summary>The first contrast problems, or a note that there are none.</summary>
    public string Warnings
    {
        get => _warnings;
        private set => SetProperty(ref _warnings, value);
    }

    /// <summary>Why the theme could not be saved, or empty.</summary>
    public string Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    /// <summary>Changes the selected role's colour with the colour picker.</summary>
    public ICommand Edit { get; }

    /// <summary>Puts the selected role back to the seed's colour.</summary>
    public ICommand Reset { get; }

    /// <summary>Validates and saves the theme, selects it and reloads the catalog.</summary>
    /// <returns>True when it was saved.</returns>
    public bool TrySave()
    {
        var name = Name.Trim();
        if (!ProfileName.IsValid(name))
        {
            Error = "Enter a valid theme name.";
            return false;
        }

        if (BuiltInThemes.IsReservedName(name))
        {
            Error = $"'{name}' is reserved for a built-in theme; pick another name.";
            return false;
        }

        _state.Name = name;
        var path = Path.Combine(_directory, $"{name}.json");
        if (File.Exists(path) && _confirm is not null && !_confirm.Confirm($"A theme named '{name}' already exists. Overwrite it?"))
        {
            return false;
        }

        if (_state.Save(path) is { } problem)
        {
            Error = problem;
            return false;
        }

        ActiveTheme.UseCatalog(ThemeCatalog.Load(_directory));
        ActiveTheme.Select(name);
        Error = string.Empty;
        return true;
    }

    private void EditSelected()
    {
        var role = _roles[SelectedRoleIndex];
        var current = _state[role];
        if (_colorPicker!.Pick($"{current.R},{current.G},{current.B}") is { } picked && TryParse(picked) is { } color)
        {
            _state.Set(role, color);
            Refresh();
            Preview();
        }
    }

    private void ResetSelected()
    {
        _state.ResetToSeed(_roles[SelectedRoleIndex]);
        Refresh();
        Preview();
    }

    private static ThemeColor? TryParse(string text)
    {
        var parts = text.Split(',');
        return parts.Length == 3 && byte.TryParse(parts[0], out var r) && byte.TryParse(parts[1], out var g) && byte.TryParse(parts[2], out var b)
            ? new ThemeColor(r, g, b)
            : null;
    }

    private void Preview() => ActiveTheme.Preview(_state.Build());

    private void Refresh()
    {
        var selected = SelectedRoleIndex;
        Roles.Clear();
        foreach (var role in _roles)
        {
            Roles.Add(new ThemeRoleRow(role, (_state.IsOverridden(role) ? _overriddenMarker : _unchangedMarker) + ThemeFile.RoleName(role).PadRight(24) + _state[role].ToHex()));
        }

        SelectedRoleIndex = selected;
        var warnings = _state.ContrastWarnings();
        Warnings = warnings.Count == 0 ? "No contrast problems." : string.Join('\n', warnings.Take(3));
        ((CommandBase)Reset).RaiseCanExecuteChanged();
    }
}
