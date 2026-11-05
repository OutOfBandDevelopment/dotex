using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The Browse and Save As buttons for the profile zip path.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ProfileZipBrowserTests
{
    private sealed class FakeZipDialogs : IProfileZipDialogs
    {
        public string? Import { get; set; }

        public string? Export { get; set; }

        public string? PickZipToImport() => Import;

        public string? PickZipToExport() => Export;
    }

    private static ConnectionEditorViewModel Editor() => new(new ConnectionProfileStore(global::System.IO.Path.Combine(global::System.IO.Path.GetTempPath(), "devterm-zip-" + global::System.Guid.NewGuid().ToString("N"))), new CliOptions());

    [TestMethod]
    public void Browse_FileChosen_FillsTheImportExportPath()
    {
        // Stage
        var editor = Editor();
        var browser = new ProfileZipBrowser(editor, new FakeZipDialogs { Import = "C:/backup/profiles.zip" });

        // Test
        browser.Browse.Execute(null);

        // Assert
        Assert.AreEqual("C:/backup/profiles.zip", editor.ImportExportPath);
    }

    [TestMethod]
    public void SaveAs_PathChosen_FillsTheImportExportPath()
    {
        // Stage
        var editor = Editor();
        var browser = new ProfileZipBrowser(editor, new FakeZipDialogs { Export = "C:/backup/new.zip" });

        // Test
        browser.SaveAs.Execute(null);

        // Assert
        Assert.AreEqual("C:/backup/new.zip", editor.ImportExportPath);
    }

    [TestMethod]
    public void Browse_Cancelled_KeepsTheTypedPath()
    {
        // Stage
        var editor = Editor();
        editor.ImportExportPath = "typed.zip";
        var browser = new ProfileZipBrowser(editor, new FakeZipDialogs());

        // Test
        browser.Browse.Execute(null);
        browser.SaveAs.Execute(null);

        // Assert
        Assert.AreEqual("typed.zip", editor.ImportExportPath);
    }
}
