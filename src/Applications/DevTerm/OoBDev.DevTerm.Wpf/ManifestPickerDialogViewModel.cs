using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OoBDev.DevTerm.DeviceManifests;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The device manifest picker: the discovered manifests, or a typed path to a manifest file, folder or zip, loaded on accept.</summary>
public sealed class ManifestPickerDialogViewModel : ViewModelBase
{
    private int _selectedIndex;
    private string _pathText = string.Empty;
    private string _error = string.Empty;

    /// <summary>Creates the view model.</summary>
    /// <param name="entries">The discovered manifests to list.</param>
    public ManifestPickerDialogViewModel(IReadOnlyList<ManifestEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Entries = entries;
        _selectedIndex = entries.Count > 0 ? 0 : -1;
    }

    /// <summary>The discovered manifests.</summary>
    public IReadOnlyList<ManifestEntry> Entries { get; }

    /// <summary>The selected entry, or -1.</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    /// <summary>A manifest file, folder or zip typed or browsed to; used instead of the selected entry when not empty.</summary>
    public string PathText
    {
        get => _pathText;
        set => SetProperty(ref _pathText, value);
    }

    /// <summary>Why the choice could not be loaded, or empty.</summary>
    public string Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    /// <summary>The loaded manifest once <see cref="TryLoad"/> returned true.</summary>
    public DeviceManifest? Chosen { get; private set; }

    /// <summary>Loads the typed path, else the selected entry; reports a failure in <see cref="Error"/>.</summary>
    /// <returns>True when a manifest loaded.</returns>
    public bool TryLoad()
    {
        var path = !string.IsNullOrWhiteSpace(PathText)
            ? PathText.Trim().Trim('"')
            : SelectedIndex >= 0 && SelectedIndex < Entries.Count ? Entries[SelectedIndex].Path : null;
        if (path is null)
        {
            Error = "Pick a manifest, or enter a path to one.";
            return false;
        }

        try
        {
            var manifest = DeviceManifestLoader.Load(path);
            _ = new ManifestReplyPresenter(manifest);
            Chosen = manifest;
            Error = string.Empty;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException or JsonException or InvalidDataException)
        {
            Error = $"Could not load '{path}': {ex.Message}";
            return false;
        }
    }
}
