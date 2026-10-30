using System;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf;

/// <summary>An on/off setting.</summary>
public sealed class ConfigToggleFieldViewModel(DeviceConfigViewModel config, string id, string label, Action changed) : ConfigFieldViewModel(id, label)
{
    /// <summary>Whether the setting is on; setting it edits the session.</summary>
    public bool IsOn
    {
        get => config.IsOn(Id);
        set
        {
            config.SetOn(Id, value);
            OnPropertyChanged();
            changed();
        }
    }
}
