using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.System.IO.Devices;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Device, Configure device: pick an editor, point it at a host and port, read, edit the form it declares, review the changes, write.</summary>
public sealed class ConfigureDeviceDialogViewModel : ViewModelBase
{
    private readonly DeviceConfigViewModel _config;
    private int _selectedEditorIndex;
    private string _host;
    private string _port;
    private string _summary = string.Empty;
    private string _status = string.Empty;

    /// <summary>Creates the view model.</summary>
    /// <param name="editors">The device configuration editors.</param>
    /// <param name="host">The host to start with.</param>
    /// <param name="port">The port to start with.</param>
    public ConfigureDeviceDialogViewModel(IReadOnlyList<IDeviceConfigEditor> editors, string host = "", string port = "")
    {
        ArgumentNullException.ThrowIfNull(editors);
        Editors = editors;
        _config = new DeviceConfigViewModel(editors, host, port);
        _host = host;
        _port = port;
        _selectedEditorIndex = _config.Editor is { } only ? IndexOf(only.Id) : -1;
        Read = new ConfigureDeviceCommand(() => _config.Editor is not null, ReadAsync);
        Write = new ConfigureDeviceCommand(() => _config.CanWrite, WriteAsync);
    }

    /// <summary>The editors to choose from.</summary>
    public IReadOnlyList<IDeviceConfigEditor> Editors { get; }

    /// <summary>The settings of the editor, filled by a read.</summary>
    public ObservableCollection<ConfigFieldViewModel> Fields { get; } = [];

    /// <summary>The selected editor, or -1.</summary>
    public int SelectedEditorIndex
    {
        get => _selectedEditorIndex;
        set
        {
            if (SetProperty(ref _selectedEditorIndex, value))
            {
                _config.PickEditor(value >= 0 && value < Editors.Count ? Editors[value].Id : null);
                Refresh();
            }
        }
    }

    /// <summary>The device's host.</summary>
    public string Host
    {
        get => _host;
        set
        {
            if (SetProperty(ref _host, value))
            {
                _config.Host = value;
            }
        }
    }

    /// <summary>The device's port.</summary>
    public string Port
    {
        get => _port;
        set
        {
            if (SetProperty(ref _port, value))
            {
                _config.Port = value;
            }
        }
    }

    /// <summary>The changes, problems and a warning when the write drops the connection.</summary>
    public string Summary
    {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }

    /// <summary>The last outcome in words.</summary>
    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    /// <summary>Reads the device's settings.</summary>
    public ICommand Read { get; }

    /// <summary>Writes the edited settings.</summary>
    public ICommand Write { get; }

    /// <summary>Reads the device's settings and builds the form.</summary>
    public async Task ReadAsync()
    {
        _config.Host = Host;
        _config.Port = Port;
        await _config.ReadAsync();
        BuildFields();
        Refresh();
    }

    /// <summary>Writes the edited settings and rebuilds the form from the read-back.</summary>
    public async Task WriteAsync()
    {
        await _config.WriteAsync();
        BuildFields();
        Refresh();
    }

    private int IndexOf(string id) => Editors.ToList().FindIndex(editor => editor.Id == id);

    private void BuildFields()
    {
        Fields.Clear();
        if (_config.Session is null)
        {
            return;
        }

        foreach (var control in _config.Fields)
        {
            Fields.Add(control switch
            {
                ToggleControl => new ConfigToggleFieldViewModel(_config, control.Id, control.Label, Refresh),
                ChoiceControl choice => new ConfigChoiceFieldViewModel(_config, control.Id, control.Label, choice.Options, Refresh),
                _ => new ConfigTextFieldViewModel(_config, control.Id, control.Label, Refresh),
            });
        }
    }

    private void Refresh()
    {
        var lines = new List<string>();
        lines.AddRange(_config.ChangeLines().Select(line => "change: " + line));
        lines.AddRange(_config.IssueLines().Select(line => "problem: " + line));
        if (_config.WillDropConnection)
        {
            lines.Add("Writing these values changes the address, so the connection will drop.");
        }

        Summary = string.Join(Environment.NewLine, lines);
        Status = _config.Message ?? string.Empty;
        ((CommandBase)Read).RaiseCanExecuteChanged();
        ((CommandBase)Write).RaiseCanExecuteChanged();
    }
}
