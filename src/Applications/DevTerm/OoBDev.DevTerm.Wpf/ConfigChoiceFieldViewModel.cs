using System;
using System.Collections.Generic;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf;

/// <summary>A setting with a fixed set of options.</summary>
public sealed class ConfigChoiceFieldViewModel(DeviceConfigViewModel config, string id, string label, IReadOnlyList<string> options, Action changed) : ConfigFieldViewModel(id, label)
{
    /// <summary>The options.</summary>
    public IReadOnlyList<string> Options { get; } = options;

    /// <summary>The chosen option; setting it edits the session.</summary>
    public string? Selected
    {
        get => config.Value(Id);
        set
        {
            if (value is not null)
            {
                config.Set(Id, value);
                OnPropertyChanged();
                changed();
            }
        }
    }
}
