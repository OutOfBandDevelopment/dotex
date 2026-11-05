using System;
using System.Collections.Generic;
using System.Linq;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The theme choice: the names of the available themes and the one in use.</summary>
public sealed class ThemeViewModel : ViewModelBase
{
    private string? _message;
    private IReadOnlyList<ThemeChoice>? _choices;

    /// <summary>The names of the built-in and user themes.</summary>
    public IReadOnlyList<string> Names => ActiveTheme.Catalog.SelectionNames;

    /// <summary>One checkable entry per theme, for the Theme menu.</summary>
    public IReadOnlyList<ThemeChoice> Choices => _choices ??= Build();

    /// <summary>The selected theme; setting it applies and saves it.</summary>
    public string Selected
    {
        get => ActiveTheme.Selection;
        set
        {
            if (string.Equals(value, ActiveTheme.Selection, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Message = ActiveTheme.Select(value);
            MarkSelected();
            OnPropertyChanged();
        }
    }

    /// <summary>A warning from the last selection, or null.</summary>
    public string? Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    private List<ThemeChoice> Build()
    {
        var choices = Names.Select(name => new ThemeChoice(name, new SelectThemeCommand(this, name))).ToList();
        MarkSelected(choices);
        return choices;
    }

    private void MarkSelected() => MarkSelected(_choices);

    private static void MarkSelected(IEnumerable<ThemeChoice>? choices)
    {
        foreach (var choice in choices ?? [])
        {
            choice.IsSelected = string.Equals(choice.Name, ActiveTheme.Selection, StringComparison.OrdinalIgnoreCase);
        }
    }
}
