using System.Windows.Input;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>A check box row.</summary>
public sealed class ToggleViewModel : ControlViewModel
{
    private bool _isChecked;

    /// <summary>Creates the view model and reads the model.</summary>
    /// <param name="control">The toggle.</param>
    /// <param name="binding">The bound model.</param>
    public ToggleViewModel(ToggleControl control, FormBinding binding)
        : base(control, binding)
    {
        Toggle = new ToggleCommand(binding, control);
        Refresh();
    }

    /// <summary>Flips the value, or sets it from a <see cref="bool"/> parameter.</summary>
    public ICommand Toggle { get; }

    /// <summary>The current value.</summary>
    public bool IsChecked
    {
        get => _isChecked;
        private set => SetProperty(ref _isChecked, value);
    }

    /// <inheritdoc />
    protected override void RefreshValue() => IsChecked = Binding.GetBool(Id);
}
