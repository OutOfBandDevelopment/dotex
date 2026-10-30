using System;
using System.Collections.Generic;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The theme choice: the names of the available themes and the one in use.</summary>
public sealed class ThemeViewModel : ViewModelBase
{
    private string? _message;

    /// <summary>The names of the built-in and user themes.</summary>
    public IReadOnlyList<string> Names => ActiveTheme.Catalog.SelectionNames;

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
            OnPropertyChanged();
        }
    }

    /// <summary>A warning from the last selection, or null.</summary>
    public string? Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }
}
