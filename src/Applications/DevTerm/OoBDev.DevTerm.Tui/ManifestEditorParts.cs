using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.DeviceManifests;
using OoBDev.DevTerm.DeviceManifests.Editing;
using OoBDev.UiDefinitions.Forms;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using OoBDev.UiDefinitions.Tui;
namespace OoBDev.DevTerm.Tui;

/// <summary>The editor's controls, for tests to drive headlessly.</summary>
internal sealed class ManifestEditorParts
{
    public required ManifestEditorViewModel ViewModel { get; init; }

    public required Window Window { get; init; }

    public required ListView Outline { get; init; }

    /// <summary>The right-hand pane: the selected part's form, a heading's hint, or the preview.</summary>
    public required FrameView Pane { get; init; }

    public required Label StatusLabel { get; init; }

    public required Button NewButton { get; init; }

    public required Button OpenButton { get; init; }

    public required Button SaveButton { get; init; }

    public required Button SaveAsButton { get; init; }

    public required Button ValidateButton { get; init; }

    public required Button PreviewButton { get; init; }

    public required Button CloseButton { get; init; }

    public required Button AddButton { get; init; }

    public required Button RemoveButton { get; init; }

    public required Button UpButton { get; init; }

    public required Button DownButton { get; init; }

    public required Button UndoButton { get; init; }

    public required Button RedoButton { get; init; }

    public required Button ImportKsyButton { get; init; }

    /// <summary>Picks a session log to draw sample data from (a second press, with a log loaded, goes back to generated data).</summary>
    public required Button RecordingButton { get; init; }

    /// <summary>The selected part's rendered form, while one is shown.</summary>
    public TuiFormParts? Form { get; set; }

    /// <summary>The live panel preview, while it's shown.</summary>
    public ControlPanelWindowParts? Preview { get; set; }

    public bool ShowingPreview { get; set; }

    internal Action ShowPreviewAction { get; set; } = () => { };

    internal Action ShowSelectionAction { get; set; } = () => { };

    public void ShowPreview() => ShowPreviewAction();

    public void ShowSelection() => ShowSelectionAction();
}
