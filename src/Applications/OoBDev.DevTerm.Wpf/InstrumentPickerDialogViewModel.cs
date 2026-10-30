using System;
using System.Collections.Generic;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Picks the instrument of an <see cref="IInstrumentPanelProvider"/>: auto-detect, the generic panel or a profile.</summary>
public sealed class InstrumentPickerDialogViewModel : ViewModelBase
{
    private int _selectedIndex;

    /// <summary>Creates the picker over <paramref name="provider"/>'s choices.</summary>
    /// <param name="provider">The provider.</param>
    public InstrumentPickerDialogViewModel(IInstrumentPanelProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        Title = provider.PickerTitle;
        Choices = provider.PickerChoices();
    }

    /// <summary>The picker title.</summary>
    public string Title { get; }

    /// <summary>The choices: auto-detect, generic, then each profile.</summary>
    public IReadOnlyList<string> Choices { get; }

    /// <summary>The selected choice, or -1.</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    /// <summary>The choice once <see cref="TryChoose"/> returned true.</summary>
    public string? Chosen { get; private set; }

    /// <summary>Takes the selected choice; false when nothing is selected.</summary>
    public bool TryChoose()
    {
        if (SelectedIndex < 0 || SelectedIndex >= Choices.Count)
        {
            return false;
        }

        Chosen = Choices[SelectedIndex];
        return true;
    }
}
