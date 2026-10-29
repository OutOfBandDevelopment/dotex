using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The shell: the registered panels, the open one and the command to open another.</summary>
public sealed class ShellViewModel : ViewModelBase, IDisposable
{
    private PanelViewModel? _selected;

    /// <summary>Creates the shell and opens the first panel, if any.</summary>
    /// <param name="factories">The registered panel factories.</param>
    public ShellViewModel(IEnumerable<IPanelFactory> factories)
    {
        ArgumentNullException.ThrowIfNull(factories);
        Factories = [.. factories];
        Open = new OpenPanelCommand(this);
        if (Factories.Count > 0)
        {
            Open.Execute(Factories[0]);
        }
    }

    /// <summary>The panels that can be opened.</summary>
    public IReadOnlyList<IPanelFactory> Factories { get; }

    /// <summary>Opens the panel of the factory passed as the parameter, replacing the open one.</summary>
    public ICommand Open { get; }

    /// <summary>The open panel, or null.</summary>
    public PanelViewModel? SelectedPanel
    {
        get => _selected;
        internal set
        {
            var previous = _selected;
            if (SetProperty(ref _selected, value))
            {
                previous?.Dispose();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose() => SelectedPanel = null;
}
