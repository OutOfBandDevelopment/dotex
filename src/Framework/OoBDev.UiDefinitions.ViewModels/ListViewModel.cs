using System;
using System.Collections.ObjectModel;
using System.Linq;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>A list with one selected item; the add and delete buttons are the sibling button view models named by <see cref="ListControl.AddCommand"/> and <see cref="ListControl.DeleteCommand"/>.</summary>
public sealed class ListViewModel : ControlViewModel
{
    private string? _selected;
    private readonly ObservableCollection<string> _items;

    /// <summary>Creates the view model and reads the model.</summary>
    /// <param name="control">The list.</param>
    /// <param name="binding">The bound model.</param>
    public ListViewModel(ListControl control, FormBinding binding)
        : base(control, binding)
    {
        List = control;
        _items = new ObservableCollection<string>(control.Items);
        Refresh();
    }

    /// <summary>The list definition.</summary>
    public ListControl List { get; }

    /// <summary>The item texts, re-read from the model on every refresh.</summary>
    public ObservableCollection<string> Items => _items;

    /// <summary>The selected item's text, or null when nothing is selected. Setting a listed item selects it; anything else is ignored.</summary>
    public string? SelectedItem
    {
        get => _selected;
        set
        {
            if (value is null || string.Equals(value, _selected, StringComparison.Ordinal) || !_items.Contains(value, StringComparer.Ordinal))
            {
                return;
            }

            Binding.SetText(List, value);
            Refresh();
        }
    }

    /// <inheritdoc />
    protected override void RefreshValue()
    {
        if (List.ItemsSource is { } source && Binding.GetOptions(source) is { } current && !current.SequenceEqual(_items, StringComparer.Ordinal))
        {
            _items.Clear();
            foreach (var item in current)
            {
                _items.Add(item);
            }
        }

        var text = Binding.GetText(Id);
        var selected = _items.FirstOrDefault(i => string.Equals(i, text, StringComparison.Ordinal));
        if (!string.Equals(_selected, selected, StringComparison.Ordinal))
        {
            _selected = selected;
            OnPropertyChanged(nameof(SelectedItem));
        }
    }
}
