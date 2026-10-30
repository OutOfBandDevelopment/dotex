using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The converter tools window: add, edit, remove and reorder the stream monitor's converter tools over a <see cref="ConverterToolsEditor"/>.</summary>
public sealed class ConverterToolsDialogViewModel : ViewModelBase
{
    private readonly ConverterToolsEditor _editor;
    private int _selectedIndex = -1;
    private bool _loading;
    private string _name = string.Empty;
    private string _path = string.Empty;
    private string _arguments = string.Empty;
    private string _formats = string.Empty;
    private string _outputExtension = string.Empty;
    private string _dpiText = string.Empty;
    private string _error = string.Empty;

    /// <summary>Creates the view model over <paramref name="editor"/>.</summary>
    /// <param name="editor">The editor holding a copy of the tools.</param>
    public ConverterToolsDialogViewModel(ConverterToolsEditor editor)
    {
        ArgumentNullException.ThrowIfNull(editor);
        _editor = editor;
        Add = new ConverterToolsCommand(() => true, () =>
        {
            _editor.Add();
            Refresh(_editor.Tools.Count - 1);
        });
        Remove = new ConverterToolsCommand(() => HasSelection, () => Refresh(_editor.Remove(SelectedIndex)));
        MoveUp = new ConverterToolsCommand(() => SelectedIndex > 0, () => Refresh(_editor.MoveUp(SelectedIndex)));
        MoveDown = new ConverterToolsCommand(() => HasSelection && SelectedIndex < _editor.Tools.Count - 1, () => Refresh(_editor.MoveDown(SelectedIndex)));
        Refresh(_editor.Tools.Count > 0 ? 0 : -1);
    }

    /// <summary>The tool names, in the order the stream monitor tries them.</summary>
    public ObservableCollection<string> ToolNames { get; } = [];

    /// <summary>The selected tool, or -1.</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (!_loading && SetProperty(ref _selectedIndex, value))
            {
                LoadFields();
            }
        }
    }

    /// <summary>Whether a tool is selected and its fields can be edited.</summary>
    public bool HasSelection => SelectedIndex >= 0 && SelectedIndex < _editor.Tools.Count;

    /// <summary>The name shown in the conversion list.</summary>
    public string Name
    {
        get => _name;
        set => Edit(ref _name, value, tool => tool.Name = value);
    }

    /// <summary>The converter executable.</summary>
    public string Path
    {
        get => _path;
        set => Edit(ref _path, value, tool => tool.Path = value);
    }

    /// <summary>The arguments, split on spaces, with <c>{input}</c>, <c>{output}</c> and <c>{dpi}</c> substituted.</summary>
    public string Arguments
    {
        get => _arguments;
        set => Edit(ref _arguments, value, tool => tool.Arguments = value);
    }

    /// <summary>The capture formats the tool accepts, comma-separated; empty means any.</summary>
    public string Formats
    {
        get => _formats;
        set => Edit(ref _formats, value, tool => tool.Formats = value);
    }

    /// <summary>The extension, without a dot, of the file the tool writes.</summary>
    public string OutputExtension
    {
        get => _outputExtension;
        set => Edit(ref _outputExtension, value, tool => tool.OutputExtension = value);
    }

    /// <summary>The value substituted for <c>{dpi}</c>.</summary>
    public string DpiText
    {
        get => _dpiText;
        set => Edit(ref _dpiText, value, tool =>
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dpi))
            {
                tool.Dpi = dpi;
            }
        });
    }

    /// <summary>Why the tools cannot be accepted, or empty.</summary>
    public string Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    /// <summary>Adds a tool.</summary>
    public ICommand Add { get; }

    /// <summary>Removes the selected tool.</summary>
    public ICommand Remove { get; }

    /// <summary>Moves the selected tool up.</summary>
    public ICommand MoveUp { get; }

    /// <summary>Moves the selected tool down.</summary>
    public ICommand MoveDown { get; }

    /// <summary>The edited tools; read after <see cref="TryAccept"/> returned true.</summary>
    public IReadOnlyList<StreamConvertToolOptions> Result => _editor.ToList();

    /// <summary>Validates the tools; returns false and sets <see cref="Error"/> when one is incomplete.</summary>
    public bool TryAccept()
    {
        Error = _editor.Validate() ?? string.Empty;
        return Error.Length == 0;
    }

    private void Edit(ref string field, string value, Action<StreamConvertToolOptions> apply, [CallerMemberName] string? name = null)
    {
        if (!SetProperty(ref field, value, name) || _loading || !HasSelection)
        {
            return;
        }

        apply(_editor.Tools[SelectedIndex]);
        Error = string.Empty;
        if (name == nameof(Name))
        {
            ToolNames[SelectedIndex] = Label(_editor.Tools[SelectedIndex]);
        }
    }

    private static string Label(StreamConvertToolOptions tool) => string.IsNullOrWhiteSpace(tool.Name) ? "(unnamed)" : tool.Name;

    private void Refresh(int select)
    {
        _loading = true;
        ToolNames.Clear();
        foreach (var tool in _editor.Tools)
        {
            ToolNames.Add(Label(tool));
        }

        _selectedIndex = Math.Min(select, _editor.Tools.Count - 1);
        _loading = false;
        OnPropertyChanged(nameof(SelectedIndex));
        LoadFields();
    }

    private void LoadFields()
    {
        _loading = true;
        var tool = HasSelection ? _editor.Tools[SelectedIndex] : null;
        Name = tool?.Name ?? string.Empty;
        Path = tool?.Path ?? string.Empty;
        Arguments = tool?.Arguments ?? string.Empty;
        Formats = tool?.Formats ?? string.Empty;
        OutputExtension = tool?.OutputExtension ?? string.Empty;
        DpiText = tool is null ? string.Empty : tool.Dpi.ToString(CultureInfo.InvariantCulture);
        _loading = false;
        OnPropertyChanged(nameof(HasSelection));
        ((CommandBase)Remove).RaiseCanExecuteChanged();
        ((CommandBase)MoveUp).RaiseCanExecuteChanged();
        ((CommandBase)MoveDown).RaiseCanExecuteChanged();
    }
}
