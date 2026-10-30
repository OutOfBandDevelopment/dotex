using System;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Wires the destructive profile actions of a <see cref="ConnectionEditorViewModel"/> to a confirmation prompt.</summary>
public static class ConnectionEditorPrompts
{
    /// <summary>Makes deleting several profiles and replacing all of them from a zip ask first.</summary>
    /// <param name="editor">The editor to wire.</param>
    /// <param name="confirm">The prompt to ask with.</param>
    public static void Wire(ConnectionEditorViewModel editor, IConfirmPrompt confirm)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentNullException.ThrowIfNull(confirm);
        editor.ConfirmDeleteProfiles = names => confirm.Confirm(names.Count == 1
            ? $"Delete profile '{names[0]}'? This can't be undone."
            : $"Delete {names.Count} profiles ({string.Join(", ", names)})? This can't be undone.");
        editor.ConfirmReplaceAllProfiles = (existing, incoming) =>
            confirm.Confirm($"Delete all {existing} saved profile(s) and import the {incoming} in the zip? This can't be undone.");
    }
}
