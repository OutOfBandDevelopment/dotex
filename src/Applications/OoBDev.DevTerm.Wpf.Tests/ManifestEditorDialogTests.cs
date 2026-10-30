using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.DeviceManifests.Editing;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The manifest editor window's view model over the shared editing logic.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ManifestEditorDialogTests
{
    private sealed class FakeFiles : IManifestFileDialogs
    {
        public string? SavePath { get; set; }

        public string? PickManifestToOpen() => null;

        public string? PickSavePath(string initialDirectory) => SavePath;

        public string? PickKsy() => null;

        public string? PickRecording() => null;
    }

    private sealed class FakeDialog : IManifestEditorDialog
    {
        public ManifestEditorDialogViewModel? Shown { get; private set; }

        public void Show(ManifestEditorDialogViewModel editor) => Shown = editor;
    }

    private static string TempDirectory() =>
        global::System.IO.Directory.CreateTempSubdirectory("manifest-editor-tests").FullName;

    private static ManifestEditorDialogViewModel Create(FakeFiles files, string directory) =>
        new(new ManifestEditorViewModel(directory), files);

    [TestMethod]
    public void Constructor_NewManifest_ShowsOutlineAndPreview()
    {
        // Stage
        var directory = TempDirectory();

        // Test
        using var editor = Create(new FakeFiles(), directory);

        // Assert
        Assert.IsTrue(editor.Nodes.Count > 0);
        Assert.IsNotNull(editor.Preview);
        Assert.IsFalse(editor.IsDirty);
    }

    [TestMethod]
    public void SelectedIndex_IdentityNode_ShowsAForm()
    {
        // Stage
        using var editor = Create(new FakeFiles(), TempDirectory());
        var index = editor.Nodes.ToList().FindIndex(n => n.Kind == ManifestNodeKind.Identity);

        // Test
        editor.SelectedIndex = index;

        // Assert
        Assert.IsNotNull(editor.Form);
        Assert.AreEqual(index, editor.SelectedIndex);
    }

    [TestMethod]
    public void Add_CommandsNode_AddsAChildAndEnablesUndo()
    {
        // Stage
        using var editor = Create(new FakeFiles(), TempDirectory());
        editor.SelectedIndex = editor.Nodes.ToList().FindIndex(n => n.Kind == ManifestNodeKind.Commands);
        var before = editor.Nodes.Count;
        Assert.IsTrue(editor.Add.CanExecute(null));
        Assert.IsFalse(editor.Undo.CanExecute(null));

        // Test
        editor.Add.Execute(null);

        // Assert
        Assert.IsTrue(editor.Nodes.Count > before);
        Assert.IsTrue(editor.Undo.CanExecute(null));
        Assert.IsTrue(editor.IsDirty);
    }

    [TestMethod]
    public void Undo_AfterAdd_RestoresTheOutline()
    {
        // Stage
        using var editor = Create(new FakeFiles(), TempDirectory());
        editor.SelectedIndex = editor.Nodes.ToList().FindIndex(n => n.Kind == ManifestNodeKind.Commands);
        var before = editor.Nodes.Count;
        editor.Add.Execute(null);

        // Test
        editor.Undo.Execute(null);

        // Assert
        Assert.AreEqual(before, editor.Nodes.Count);
        Assert.IsTrue(editor.Redo.CanExecute(null));
    }

    [TestMethod]
    public void SaveAs_PickedPath_WritesTheManifest()
    {
        // Stage
        var directory = TempDirectory();
        var target = global::System.IO.Path.Combine(directory, "saved.json");
        using var editor = Create(new FakeFiles { SavePath = target }, directory);

        // Test
        editor.SaveAs.Execute(null);

        // Assert
        Assert.IsTrue(global::System.IO.File.Exists(target) || global::System.IO.Directory.EnumerateFiles(directory, "*.json", global::System.IO.SearchOption.AllDirectories).Any(), editor.Status);
    }

    [TestMethod]
    public void SaveAs_Cancelled_WritesNothing()
    {
        // Stage
        var directory = TempDirectory();
        using var editor = Create(new FakeFiles(), directory);

        // Test
        editor.SaveAs.Execute(null);

        // Assert
        Assert.IsFalse(global::System.IO.Directory.EnumerateFiles(directory, "*.json", global::System.IO.SearchOption.AllDirectories).Any());
    }

    [TestMethod]
    public void EditManifest_ShellWithDialog_ShowsTheEditor()
    {
        // Stage
        var dialog = new FakeDialog();
        var shell = new ShellViewModel([], manifestEditorDialog: dialog, manifestFiles: new FakeFiles());

        // Test
        shell.ShowManifestEditor.Execute(null);

        // Assert
        Assert.IsNotNull(dialog.Shown);
        dialog.Shown.Dispose();
    }

    [TestMethod]
    public void CanEditManifest_NoDialog_IsFalse()
    {
        // Stage / Test
        var shell = new ShellViewModel([]);

        // Assert
        Assert.IsFalse(shell.ShowManifestEditor.CanExecute(null));
    }
}
