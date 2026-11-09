using System;
using System.Collections;
using System.ComponentModel;

namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>
/// One entry in the manifest editor's outline (Identity; Commands › a command › its parameters;
/// Response patterns › a pattern; Panel › a section › its controls), with the form model that edits
/// it — null for a group heading, which only adds children.
/// </summary>
public sealed class ManifestEditorNode : INotifyPropertyChanged
{
    private readonly Func<string> _label;

    internal ManifestEditorNode(ManifestNodeKind kind, int depth, Func<string> label, EditorForm? form, object? item = null, IList? list = null, string? hint = null)
    {
        Kind = kind;
        Depth = depth;
        _label = label;
        Form = form;
        Item = item;
        List = list;
        Hint = hint;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ManifestNodeKind Kind { get; }

    public int Depth { get; }

    /// <summary>The outline text, indented two spaces per level.</summary>
    public string Display => new string(' ', Depth * 2) + _label();

    /// <summary>The form that edits this entry; null for a heading.</summary>
    public EditorForm? Form { get; }

    /// <summary>What a heading's (form-less) pane says instead.</summary>
    public string? Hint { get; }

    /// <summary>The manifest object this entry is (a command, a section, ...) and the list it sits in — for remove/move.</summary>
    internal object? Item { get; }

    internal IList? List { get; }

    internal void RefreshDisplay() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Display)));

    public override string ToString() => Display;
}
