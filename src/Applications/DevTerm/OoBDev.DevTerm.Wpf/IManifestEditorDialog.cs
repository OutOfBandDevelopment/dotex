namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the device manifest editor.</summary>
public interface IManifestEditorDialog
{
    /// <summary>Shows the editor modally.</summary>
    /// <param name="editor">The view model it edits.</param>
    void Show(ManifestEditorDialogViewModel editor);
}
