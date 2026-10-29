using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Base for the view models: raises <see cref="INotifyPropertyChanged.PropertyChanged"/> only when a value changes.</summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Stores <paramref name="value"/> and notifies when it differs from <paramref name="field"/>.</summary>
    /// <returns>True when the value changed.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(name);
        return true;
    }

    /// <summary>Raises <see cref="PropertyChanged"/>.</summary>
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
