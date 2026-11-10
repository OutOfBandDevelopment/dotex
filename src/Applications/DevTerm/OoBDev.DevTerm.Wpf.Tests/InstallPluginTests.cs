using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Device &gt; Install plugin in the WPF shell, over a fake dialog.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class InstallPluginTests
{
    private sealed class FakeDialog(string? zip, PluginApprovalChoice choice) : IPluginInstallDialog
    {
        public PluginInstallResult? Reported { get; private set; }

        public PluginApprovalRequest? Asked { get; private set; }

        public string? PickZip() => zip;

        public PluginApprovalChoice Approve(PluginApprovalRequest request)
        {
            Asked = request;
            return choice;
        }

        public void Report(PluginInstallResult result) => Reported = result;
    }

    private static string MakeZip(string folder)
    {
        var zip = Path.Combine(folder, "echo.zip");
        using var archive = ZipFile.Open(zip, ZipArchiveMode.Create);
        using var writer = new StreamWriter(archive.CreateEntry("plugin.json").Open());
        writer.Write("""{ "name": "Echo", "version": "1.0", "contract": 1, "process": { "command": "echo" } }""");
        return zip;
    }

    [TestMethod]
    public void InstallPlugin_NoDialog_IsDisabled()
    {
        var shell = new ShellViewModel([]);

        Assert.IsFalse(shell.InstallPluginFromZip.CanExecute(null));
    }

    [TestMethod]
    public void InstallPlugin_ApprovedZip_IsInstalledAndReported()
    {
        using var folder = new TempDirectoryHandle();
        var dialog = new FakeDialog(MakeZip(folder.DirectoryPath), PluginApprovalChoice.Once);
        var plugins = Path.Combine(folder.DirectoryPath, "plugins");
        var shell = new ShellViewModel([], pluginInstallDialog: dialog, pluginsDirectory: plugins);

        shell.InstallPluginFromZip.Execute(null);

        Assert.AreEqual("Echo", dialog.Asked!.Name);
        Assert.IsTrue(dialog.Reported!.Installed, dialog.Reported.Message);
        Assert.IsTrue(File.Exists(Path.Combine(plugins, "Echo", "plugin.json")));
    }

    [TestMethod]
    public void InstallPlugin_DeclinedOrCancelled_InstallsNothing()
    {
        using var folder = new TempDirectoryHandle();
        var plugins = Path.Combine(folder.DirectoryPath, "plugins");
        var declined = new FakeDialog(MakeZip(folder.DirectoryPath), PluginApprovalChoice.Deny);
        var cancelled = new FakeDialog(null, PluginApprovalChoice.Once);

        new ShellViewModel([], pluginInstallDialog: declined, pluginsDirectory: plugins).InstallPluginFromZip.Execute(null);
        new ShellViewModel([], pluginInstallDialog: cancelled, pluginsDirectory: plugins).InstallPluginFromZip.Execute(null);

        Assert.IsFalse(declined.Reported!.Installed);
        Assert.IsNull(cancelled.Reported, "Cancelling the file picker reports nothing.");
        Assert.IsFalse(Directory.Exists(Path.Combine(plugins, "Echo")));
    }
}
