using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>A dropdown, radio group or check list row.</summary>
public sealed class ChoiceViewModel : ControlViewModel
{
    private string _selected = string.Empty;
    private IReadOnlyList<string> _checked = [];

    /// <summary>Creates the view model and reads the model.</summary>
    /// <param name="control">The choice.</param>
    /// <param name="binding">The bound model.</param>
    public ChoiceViewModel(ChoiceControl control, FormBinding binding)
        : base(control, binding)
    {
        Choice = control;
        Select = new SelectOptionCommand(binding, control);
        ToggleOption = new ToggleOptionCommand(binding, control);
        var pick = control.Style == ChoiceStyle.CheckList ? ToggleOption : Select;
        OptionItems = [.. control.Options.Select(o => new ChoiceOptionViewModel(o, pick))];
        Refresh();
    }

    /// <summary>The choice definition.</summary>
    public ChoiceControl Choice { get; }

    /// <summary>The options in definition order.</summary>
    public IReadOnlyList<string> Options => Choice.Options;

    /// <summary>How a view presents the options.</summary>
    public ChoiceStyle Style => Choice.Style;

    /// <summary>Picks one option; the parameter is the option text (dropdown and radio group).</summary>
    public ICommand Select { get; }

    /// <summary>Checks or unchecks one option; the parameter is the option text (check list).</summary>
    public ICommand ToggleOption { get; }

    /// <summary>The options as bindable items; each carries its command and whether it is picked or checked.</summary>
    public IReadOnlyList<ChoiceOptionViewModel> OptionItems { get; }

    /// <summary>The picked option (dropdown and radio group), as the model spells it.</summary>
    public string SelectedOption
    {
        get => _selected;
        private set => SetProperty(ref _selected, value);
    }

    /// <summary>The checked options (check list).</summary>
    public IReadOnlyList<string> CheckedOptions
    {
        get => _checked;
        private set
        {
            if (!_checked.SequenceEqual(value, StringComparer.Ordinal))
            {
                _checked = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Whether <paramref name="option"/> is checked (check list).</summary>
    public bool IsChecked(string option) => CheckedOptions.Contains(option, StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    protected override void RefreshValue()
    {
        if (Style == ChoiceStyle.CheckList)
        {
            CheckedOptions = Binding.GetSelection(Id);
            MarkItems(o => IsChecked(o));
            return;
        }

        var current = Binding.GetText(Id);
        SelectedOption = Options.FirstOrDefault(o => string.Equals(o, current, StringComparison.OrdinalIgnoreCase)) ?? current;
        MarkItems(o => string.Equals(o, SelectedOption, StringComparison.Ordinal));
    }

    private void MarkItems(Func<string, bool> isSelected)
    {
        foreach (var item in OptionItems)
        {
            item.IsSelected = isSelected(item.Text);
        }
    }
}
