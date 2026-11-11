using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OoBDev.UiDefinitions.Avalonia.Tests;

/// <summary>The object the demo form edits.</summary>
public sealed class DemoModel : INotifyPropertyChanged
{
    private bool _advanced;
    private string _name = "a";
    private string _mode = "Low";
    private double _level = 2;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Shows the advanced rows.</summary>
    public bool Advanced { get => _advanced; set => Set(ref _advanced, value); }

    /// <summary>A text value, visible only when <see cref="Advanced"/> is on.</summary>
    public string Name { get => _name; set => Set(ref _name, value); }

    /// <summary>One of Low or High.</summary>
    public string Mode { get => _mode; set => Set(ref _mode, value); }

    /// <summary>A number from 0 to 10.</summary>
    public double Level { get => _level; set => Set(ref _level, value); }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
