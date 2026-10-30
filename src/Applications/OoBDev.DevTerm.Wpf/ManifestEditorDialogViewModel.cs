using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.DeviceManifests.Editing;
using OoBDev.DevTerm.Panels;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The device manifest editor window: an outline, the selected part's form and a live preview that sends nothing.</summary>
public sealed class ManifestEditorDialogViewModel : ViewModelBase, IDisposable
{
    private readonly ManifestEditorViewModel _editor;
    private readonly IManifestFileDialogs _files;
    private readonly IColorPicker? _colorPicker;
    private readonly IConfirmPrompt? _confirm;
    private readonly ManifestEditorCommand[] _stateful;
    private FormViewModel? _form;
    private FormViewModel? _preview;
    private PanelViewModel? _previewPanel;
    private string _previewSent = string.Empty;
    private int _selectedIndex = -1;
    private FormBinding? _formBinding;
    private IReadOnlyList<(string Label, string Id)> _readouts = [];

    /// <summary>Creates the view model.</summary>
    /// <param name="editor">The editing logic shared with the terminal front end.</param>
    /// <param name="files">The file pickers.</param>
    /// <param name="colorPicker">The colour picker for the forms, or null.</param>
    /// <param name="confirm">The prompt for destructive actions, or null.</param>
    public ManifestEditorDialogViewModel(ManifestEditorViewModel editor, IManifestFileDialogs files, IColorPicker? colorPicker = null, IConfirmPrompt? confirm = null)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentNullException.ThrowIfNull(files);
        _editor = editor;
        _files = files;
        _colorPicker = colorPicker;
        _confirm = confirm;
        var undo = new ManifestEditorCommand(() => _editor.CanUndo, _editor.Undo);
        var redo = new ManifestEditorCommand(() => _editor.CanRedo, _editor.Redo);
        var add = new ManifestEditorCommand(() => _editor.AddLabel is not null, _editor.AddChild);
        var remove = new ManifestEditorCommand(() => _editor.CanRemove, _editor.Remove);
        var up = new ManifestEditorCommand(() => _editor.CanMove, _editor.MoveUp);
        var down = new ManifestEditorCommand(() => _editor.CanMove, _editor.MoveDown);
        var createPanel = new ManifestEditorCommand(() => _editor.SelectedNode is { Kind: ManifestNodeKind.Panel, Form: null }, _editor.CreatePanelFromCommands);
        _stateful = [undo, redo, add, remove, up, down, createPanel];
        Undo = undo;
        Redo = redo;
        Add = add;
        Remove = remove;
        MoveUp = up;
        MoveDown = down;
        CreatePanel = createPanel;
        New = new ManifestEditorCommand(() => true, () => _editor.New());
        Open = new ManifestEditorCommand(() => true, OpenManifest);
        Save = new ManifestEditorCommand(() => true, () => _editor.Save());
        SaveAs = new ManifestEditorCommand(() => true, SaveManifestAs);
        Check = new ManifestEditorCommand(() => true, () => _editor.Validate());
        ImportKsy = new ManifestEditorCommand(() => true, ImportKsyFile);
        ToggleRecording = new ManifestEditorCommand(() => true, ToggleRecordingFile);
        _editor.ConfirmDiscardChanges = () => _confirm?.Confirm("The manifest has unsaved changes. Discard them?") ?? true;
        _editor.ConfirmOverwrite = target => _confirm?.Confirm($"'{target}' already exists. Overwrite it?") ?? true;
        _editor.SelectionChanged += (_, _) => ShowSelection();
        _editor.StructureChanged += (_, _) => ShowSelection();
        _editor.StatusChanged += (_, _) => OnPropertyChanged(nameof(Status));
        _editor.Edited += (_, _) =>
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Readouts));
            RebuildPreview();
            RefreshCommands();
        };
        ShowSelection();
        RebuildPreview();
    }

    /// <summary>The window title, with a star when there are unsaved edits.</summary>
    public string Title => _editor.Title;

    /// <summary>The last outcome in words.</summary>
    public string Status => _editor.StatusMessage;

    /// <summary>Every part of the manifest.</summary>
    public ObservableCollection<ManifestEditorNode> Nodes => _editor.Nodes;

    /// <summary>The selected outline row, or -1.</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (SetProperty(ref _selectedIndex, value))
            {
                _editor.Select(value);
            }
        }
    }

    /// <summary>The selected part's heading.</summary>
    public string PaneTitle => _editor.SelectedNode?.Display.Trim() ?? string.Empty;

    /// <summary>Guidance shown when the selected part has no form.</summary>
    public string Hint => _editor.SelectedNode is { Form: null } node ? node.Hint ?? string.Empty : string.Empty;

    /// <summary>The label of the Add button.</summary>
    public string AddLabel => _editor.AddLabel ?? "Add";

    /// <summary>The selected part's form, or null.</summary>
    public FormViewModel? Form
    {
        get => _form;
        private set => SetProperty(ref _form, value);
    }

    /// <summary>The read-only values of the selected part, one per line.</summary>
    public string Readouts => _formBinding is null ? string.Empty : string.Join(Environment.NewLine, _readouts.Select(r => $"{r.Label}: {_formBinding.GetText(r.Id)}"));

    /// <summary>The live preview of the panel, or null.</summary>
    public FormViewModel? Preview
    {
        get => _preview;
        private set => SetProperty(ref _preview, value);
    }

    /// <summary>What the last preview control would have sent.</summary>
    public string PreviewSent
    {
        get => _previewSent;
        private set => SetProperty(ref _previewSent, value);
    }

    /// <summary>Whether sample values come from a recorded session log.</summary>
    public bool UsesRecording => _editor.Recording is not null;

    /// <summary>The label of the recording button.</summary>
    public string RecordingLabel => UsesRecording ? "Stop using recording" : "Use recording...";

    /// <summary>Whether there are unsaved edits.</summary>
    public bool IsDirty => _editor.IsDirty;

    /// <summary>Starts a new manifest.</summary>
    public ICommand New { get; }

    /// <summary>Opens a manifest to edit.</summary>
    public ICommand Open { get; }

    /// <summary>Saves the manifest.</summary>
    public ICommand Save { get; }

    /// <summary>Saves the manifest to a chosen path.</summary>
    public ICommand SaveAs { get; }

    /// <summary>Runs the checks the loader runs.</summary>
    public ICommand Check { get; }

    /// <summary>Undoes the last edit.</summary>
    public ICommand Undo { get; }

    /// <summary>Redoes the last undone edit.</summary>
    public ICommand Redo { get; }

    /// <summary>Imports a Kaitai Struct file as the binary frame.</summary>
    public ICommand ImportKsy { get; }

    /// <summary>Starts or stops drawing sample values from a recorded session.</summary>
    public ICommand ToggleRecording { get; }

    /// <summary>Adds a child to the selected part.</summary>
    public ICommand Add { get; }

    /// <summary>Removes the selected part.</summary>
    public ICommand Remove { get; }

    /// <summary>Moves the selected part up.</summary>
    public ICommand MoveUp { get; }

    /// <summary>Moves the selected part down.</summary>
    public ICommand MoveDown { get; }

    /// <summary>Creates the panel from the manifest's commands.</summary>
    public ICommand CreatePanel { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        _form?.Dispose();
        _previewPanel?.Dispose();
    }

    private void OpenManifest()
    {
        if (_files.PickManifestToOpen() is { } path)
        {
            _editor.Open(path);
        }
    }

    private void SaveManifestAs()
    {
        var start = Directory.Exists(_editor.SaveTarget) ? _editor.SaveTarget : DevTermUserDataPaths.UserManifestsDirectory;
        if (_files.PickSavePath(start) is { } path)
        {
            _editor.SaveAs(path);
        }
    }

    private void ImportKsyFile()
    {
        if (_files.PickKsy() is { } path)
        {
            _editor.ImportKsy(path);
        }
    }

    private void ToggleRecordingFile()
    {
        if (_editor.Recording is not null)
        {
            _editor.ClearRecording();
        }
        else if (_files.PickRecording() is { } path)
        {
            _editor.LoadRecording(path);
        }

        OnPropertyChanged(nameof(UsesRecording));
        OnPropertyChanged(nameof(RecordingLabel));
        RebuildPreview();
    }

    private void ShowSelection()
    {
        _form?.Dispose();
        Form = null;
        _formBinding = null;
        _readouts = [];
        var node = _editor.SelectedNode;
        var index = node is null ? -1 : _editor.Nodes.IndexOf(node);
        if (index != _selectedIndex)
        {
            _selectedIndex = index;
            OnPropertyChanged(nameof(SelectedIndex));
        }

        if (node?.Form is { } form)
        {
            var definition = FormDefinitionGenerator.Generate(form.GetType(), form);
            _readouts = [.. definition.Sections.SelectMany(section => section.Controls).OfType<IndicatorControl>().Select(c => (c.Label ?? c.Id, c.Id))];
            foreach (var section in definition.Sections)
            {
                section.Controls.RemoveAll(c => c is IndicatorControl);
            }

            _formBinding = new FormBinding(form);
            Form = new FormViewModel(definition, _formBinding, _colorPicker, _confirm);
        }

        OnPropertyChanged(nameof(Readouts));
        OnPropertyChanged(nameof(PaneTitle));
        OnPropertyChanged(nameof(Hint));
        OnPropertyChanged(nameof(AddLabel));
        OnPropertyChanged(nameof(Status));
        RefreshCommands();
    }

    private void RebuildPreview()
    {
        _previewPanel?.Dispose();
        var definition = _editor.BuildPreviewDefinition();
        var surface = _editor.CreatePreviewSurface(definition);
        surface.PreviewInvoked += (_, sent) => PreviewSent = $"Would send: {sent}";
        var presenter = _editor.CreatePreviewPresenter();
        var built = SurfacePanelBuilder.Build(definition, surface);
        DevicePanelPreviews.Apply(built.Definition, surface);
        var feed = new DevicePanelFeed(definition, presenter, built);
        _editor.PublishSampleData(presenter);
        var form = new FormViewModel(built.Definition, built.Binding, _colorPicker, _confirm);
        _previewPanel = new PanelViewModel(_editor.Manifest.Name, form, feed, feed.Displays);
        Preview = form;
    }

    private void RefreshCommands()
    {
        foreach (var command in _stateful)
        {
            command.Refresh();
        }
    }
}
