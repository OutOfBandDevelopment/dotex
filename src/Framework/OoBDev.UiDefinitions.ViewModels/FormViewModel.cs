using System;
using System.Collections.Generic;
using System.Linq;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>
/// A form definition bound to a model: sections of control view models that refresh whenever the model
/// changes, so a row with a condition on another value updates after any edit.
/// </summary>
public sealed class FormViewModel : ViewModelBase, IDisposable
{
    private readonly FormBinding _binding;
    private bool _notesExpanded = true;

    /// <summary>Builds the view models for <paramref name="definition"/>.</summary>
    /// <param name="definition">The form definition, typically from <see cref="FormDefinitionGenerator"/>.</param>
    /// <param name="binding">The bound model; this view model listens to it but does not own it.</param>
    /// <param name="colorPicker">The dialog colour picker buttons open, or null.</param>
    /// <param name="confirm">Asks a button's confirm message before it runs, or null.</param>
    /// <param name="preview">Shows what a button would send for the current field values, or null.</param>
    public FormViewModel(UiDefinition definition, FormBinding binding, IColorPicker? colorPicker = null, IConfirmPrompt? confirm = null, ICommandPreview? preview = null)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _binding = binding ?? throw new ArgumentNullException(nameof(binding));
        Sections = [.. definition.Sections.Select(s => new FormSectionViewModel(s, [.. s.Controls.Select(c => ControlViewModelFactory.Create(c, binding, colorPicker, s.Controls, confirm, preview))], binding))];
        LinkColorPickers(definition);
        _binding.Changed += OnModelChanged;
    }

    /// <summary>The form definition.</summary>
    public UiDefinition Definition { get; }

    /// <summary>The definition's description, which a view shows as a final collapsible "Notes" section, or null.</summary>
    public string? Notes => string.IsNullOrWhiteSpace(Definition.Description) ? null : Definition.Description;

    /// <summary>Whether the Notes section is expanded (the view binds two-way; a host can seed and persist it).</summary>
    public bool NotesExpanded
    {
        get => _notesExpanded;
        set => SetProperty(ref _notesExpanded, value);
    }

    /// <summary>Whether there are <see cref="Notes"/> to show.</summary>
    public bool HasNotes => Notes is not null;

    /// <summary>The sections in definition order.</summary>
    public IReadOnlyList<FormSectionViewModel> Sections { get; }

    /// <summary>Every row of every section.</summary>
    public IEnumerable<ControlViewModel> Rows => Sections.SelectMany(s => s.Rows);

    /// <summary>The row for a control id, or null.</summary>
    public ControlViewModel? Row(string id) => Rows.FirstOrDefault(r => string.Equals(r.Id, id, StringComparison.Ordinal));

    /// <summary>Reads the model again into every row.</summary>
    public void Refresh()
    {
        foreach (var row in Rows)
        {
            row.Refresh();
        }

        OnPropertyChanged(nameof(Sections));
    }

    /// <inheritdoc />
    public void Dispose() => _binding.Changed -= OnModelChanged;

    private void LinkColorPickers(UiDefinition definition)
    {
        foreach (var (choiceId, (button, option)) in CustomColorChoices.Find(definition))
        {
            if (Row(choiceId) is ChoiceViewModel choice && Row(button.Id) is ButtonViewModel { Invoke: PickColorCommand pick })
            {
                choice.LinkColorPicker(option, pick);
            }
        }
    }

    private void OnModelChanged(object? sender, string? name) => Refresh();
}
