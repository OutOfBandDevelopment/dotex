using System;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf;

/// <summary>A free-text setting.</summary>
public sealed class ConfigTextFieldViewModel(DeviceConfigViewModel config, string id, string label, Action changed) : ConfigFieldViewModel(id, label)
{
    /// <summary>The value; setting it edits the session.</summary>
    public string Text
    {
        get => config.Value(Id);
        set
        {
            config.Set(Id, value);
            OnPropertyChanged();
            changed();
        }
    }
}
