using System;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Device &gt; Install plugin in the WPF shell, over a fake dialog.</summary>
[TestCategory("Unit")]
[DoNotParallelize]
[TestClass]
public sealed class InstallPluginTests
{
    private TempDirectoryHandle? _home;
    private string? _previousHome;

    [TestInitialize]
    public void UseTempHome()
    {
        _previousHome = Environment.GetEnvironmentVariable(DevTermHome.EnvironmentVariable);
        _home = new TempDirectoryHandle();
        Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, _home.DirectoryPath);
    }

    [TestCleanup]
    public void RestoreHome()
    {
        Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, _previousHome);
        _home?.Dispose();
    }

    private sealed class FakeDialog(string? zip, PluginApprovalChoice choice) : IPluginInstallDialog
    {
        public PluginInstallResult? Reported { get; private set; }

        public ApprovalReviewChoice Review { get; init; } = ApprovalReviewChoice.Keep;

        public int Reviewed { get; private set; }

        public bool SaidNone { get; private set; }

        public PluginApprovalChoice AskPending(PendingPlugin plugin) => PluginApprovalChoice.Deny;

        public ApprovalReviewChoice AskReview(PluginApproval approval)
        {
            Reviewed++;
            return Review;
        }

        public void ReportNoApprovals() => SaidNone = true;

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

    [TestMethod]
    public void PromptPendingPlugins_NothingWaiting_AsksNothing()
    {
        var dialog = new FakeDialog(null, PluginApprovalChoice.Once);

        new ShellViewModel([], pluginInstallDialog: dialog).PromptPendingPlugins();
        new ShellViewModel([]).PromptPendingPlugins();

        Assert.AreEqual(0, dialog.Reviewed);
    }

    [TestMethod]
    public void ReviewPluginApprovals_NothingRemembered_SaysSo()
    {
        var dialog = new FakeDialog(null, PluginApprovalChoice.Once);
        var shell = new ShellViewModel([], pluginInstallDialog: dialog);

        shell.ReviewPluginApprovals.Execute(null);

        Assert.IsTrue(dialog.SaidNone);
        Assert.AreEqual(0, dialog.Reviewed);
    }

    [TestMethod]
    public void ReviewPluginApprovals_Forget_RemovesTheApproval()
    {
        PluginTrust.Remember("Echo", "abc123");
        var dialog = new FakeDialog(null, PluginApprovalChoice.Once) { Review = ApprovalReviewChoice.Forget };
        var shell = new ShellViewModel([], pluginInstallDialog: dialog);

        shell.ReviewPluginApprovals.Execute(null);

        Assert.AreEqual(1, dialog.Reviewed);
        Assert.IsEmpty(PluginTrust.Approvals());
    }

    [TestMethod]
    public void ReviewPluginApprovals_Keep_LeavesTheApproval()
    {
        PluginTrust.Remember("Echo", "abc123");
        var shell = new ShellViewModel([], pluginInstallDialog: new FakeDialog(null, PluginApprovalChoice.Once));

        shell.ReviewPluginApprovals.Execute(null);

        Assert.HasCount(1, PluginTrust.Approvals());
    }
}
