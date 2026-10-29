using System;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>One control of a form as the view sees it: label, help, visibility, enabled state and a validation message.</summary>
public abstract class ControlViewModel : ViewModelBase
{
    private bool _isVisible = true;
    private bool _isEnabled = true;
    private string? _error;

    /// <summary>Creates the view model; call <see cref="Refresh"/> once the derived state is ready.</summary>
    /// <param name="control">The control definition.</param>
    /// <param name="binding">The bound model.</param>
    protected ControlViewModel(UiControl control, FormBinding binding)
    {
        Control = control ?? throw new ArgumentNullException(nameof(control));
        Binding = binding ?? throw new ArgumentNullException(nameof(binding));
    }

    /// <summary>The control definition.</summary>
    public UiControl Control { get; }

    /// <summary>The bound model.</summary>
    protected FormBinding Binding { get; }

    /// <summary>The control id (the model property it binds to).</summary>
    public string Id => Control.Id;

    /// <summary>The label text.</summary>
    public string Label => Control.Label;

    /// <summary>Help text, or null.</summary>
    public string? Description => Control.Description;

    /// <summary>False while <see cref="UiControl.VisibleWhen"/> does not hold; a view collapses the row.</summary>
    public bool IsVisible
    {
        get => _isVisible;
        private set => SetProperty(ref _isVisible, value);
    }

    /// <summary>False while <see cref="UiControl.EnabledWhen"/> does not hold, or the model property is read only.</summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        private set => SetProperty(ref _isEnabled, value);
    }

    /// <summary>The reason shown while the control is disabled by its condition, otherwise null.</summary>
    public string? DisabledReason => IsEnabled ? null : Control.DisabledReason;

    /// <summary>The validation message for the last value written, or null when it was valid.</summary>
    public string? Error
    {
        get => _error;
        protected set => SetProperty(ref _error, value);
    }

    /// <summary>Reads the model again: visibility, enabled state and the control's own value.</summary>
    public void Refresh()
    {
        IsVisible = Binding.IsVisible(Control.VisibleWhen);
        IsEnabled = Binding.IsVisible(Control.EnabledWhen) && !IsLocked();
        OnPropertyChanged(nameof(DisabledReason));
        RefreshValue();
    }

    /// <summary>Reads the control's own value from the model and raises the changes.</summary>
    protected abstract void RefreshValue();

    /// <summary>Whether the model can't take a value for this control (a read-only property); a button has none to lock.</summary>
    protected virtual bool IsLocked() => Binding.IsReadOnly(Control.Id);
}
