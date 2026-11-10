using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Tests;

/// <summary>A plugin shared as a .zip is unpacked, checked and approved before it lands in the plugins directory.</summary>
[TestCategory("Unit")]
[DoNotParallelize]
[TestClass]
public sealed class PluginInstallerTests
{
    private const string _processManifest = """{ "name": "Echo", "version": "1.2.0", "contract": 1, "process": { "command": "{folder}/run.exe", "arguments": [ "--go" ] } }""";

    private TempDirectoryHandle _folder = null!;
    private string _plugins = "";
    private string? _previousHome;

    [TestInitialize]
    public void Init()
    {
        _previousHome = Environment.GetEnvironmentVariable(DevTermHome.EnvironmentVariable);
        _folder = new TempDirectoryHandle();
        _plugins = Path.Combine(_folder.DirectoryPath, "plugins");
        Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, _folder.DirectoryPath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, _previousHome);
        _folder.Dispose();
    }

    private string Zip(params (string Name, string Content)[] entries)
    {
        var path = Path.Combine(_folder.DirectoryPath, Guid.NewGuid().ToString("N") + ".zip");
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (var (name, content) in entries)
        {
            using var writer = new StreamWriter(archive.CreateEntry(name).Open());
            writer.Write(content);
        }

        return path;
    }

    private sealed class Approver(PluginApprovalChoice choice) : IPluginApprover
    {
        public PluginApprovalRequest? Asked { get; private set; }

        public PluginApprovalChoice Ask(PluginApprovalRequest request)
        {
            Asked = request;
            return choice;
        }
    }

    [TestMethod]
    public void Install_AnApprovedArchive_CopiesItIntoThePluginsFolder_AfterAskingWithTheFinalCommandLine()
    {
        var approver = new Approver(PluginApprovalChoice.Once);

        var result = PluginInstaller.Install(Zip(("plugin.json", _processManifest), ("run.exe", "x"), ("lib/data.txt", "d")), _plugins, approver);

        Assert.IsTrue(result.Installed, result.Message);
        Assert.IsTrue(File.Exists(Path.Combine(_plugins, "Echo", "plugin.json")));
        Assert.IsTrue(File.Exists(Path.Combine(_plugins, "Echo", "lib", "data.txt")));
        Assert.AreEqual("Echo", approver.Asked!.Name);
        StringAssert.Contains(approver.Asked.CommandLine, Path.Combine(Path.GetFullPath(_plugins), "Echo"));
        Assert.IsFalse(PluginTrust.IsApproved("Echo", approver.Asked.Hash), "A one-time approval isn't remembered.");
    }

    [TestMethod]
    public void Install_Always_RemembersTheHashOfTheInstalledFolder()
    {
        var approver = new Approver(PluginApprovalChoice.Always);

        var result = PluginInstaller.Install(Zip(("plugin.json", _processManifest), ("run.exe", "x")), _plugins, approver);

        Assert.IsTrue(PluginTrust.IsApproved("Echo", PluginHash.Compute(result.Folder!)));
    }

    [TestMethod]
    public void Install_Denied_LeavesNothingBehind()
    {
        var result = PluginInstaller.Install(Zip(("plugin.json", _processManifest), ("run.exe", "x")), _plugins, new Approver(PluginApprovalChoice.Deny));

        Assert.IsFalse(result.Installed);
        Assert.IsFalse(Directory.Exists(Path.Combine(_plugins, "Echo")));
    }

    [TestMethod]
    public void Install_AnExistingPlugin_IsReplacedNotMerged()
    {
        var approver = new Approver(PluginApprovalChoice.Once);
        PluginInstaller.Install(Zip(("plugin.json", _processManifest), ("old.txt", "x")), _plugins, approver);

        PluginInstaller.Install(Zip(("plugin.json", _processManifest), ("new.txt", "y")), _plugins, approver);

        Assert.IsFalse(File.Exists(Path.Combine(_plugins, "Echo", "old.txt")));
        Assert.IsTrue(File.Exists(Path.Combine(_plugins, "Echo", "new.txt")));
    }

    [TestMethod]
    public void Install_AnEntryEscapingTheFolder_IsRefusedWithoutAsking()
    {
        var approver = new Approver(PluginApprovalChoice.Always);

        var result = PluginInstaller.Install(Zip(("plugin.json", _processManifest), ("../evil.txt", "x")), _plugins, approver);

        Assert.IsFalse(result.Installed);
        StringAssert.Contains(result.Message, "outside");
        Assert.IsNull(approver.Asked);
        Assert.IsFalse(File.Exists(Path.Combine(_folder.DirectoryPath, "evil.txt")));
    }

    [TestMethod]
    public void Install_WithoutAManifestAtTheRoot_OrTheWrongContract_IsRefused()
    {
        var approver = new Approver(PluginApprovalChoice.Always);

        var nested = PluginInstaller.Install(Zip(("Echo/plugin.json", _processManifest)), _plugins, approver);
        var contract = PluginInstaller.Install(Zip(("plugin.json", _processManifest.Replace("\"contract\": 1", "\"contract\": 99", StringComparison.Ordinal))), _plugins, approver);
        var notZip = Path.Combine(_folder.DirectoryPath, "bad.zip");
        File.WriteAllText(notZip, "not a zip");

        Assert.IsFalse(nested.Installed);
        StringAssert.Contains(nested.Message, "plugin.json");
        Assert.IsFalse(contract.Installed);
        StringAssert.Contains(contract.Message, "contract 99");
        Assert.IsFalse(PluginInstaller.Install(notZip, _plugins, approver).Installed);
        Assert.IsFalse(PluginInstaller.Install(Path.Combine(_folder.DirectoryPath, "missing.zip"), _plugins, approver).Installed);
        Assert.IsNull(approver.Asked);
    }

    [TestMethod]
    public void Install_AnInProcessPlugin_IsAskedAboutToo()
    {
        var approver = new Approver(PluginApprovalChoice.Once);

        PluginInstaller.Install(Zip(("plugin.json", """{ "name": "Inproc", "version": "1", "contract": 1, "assembly": "My.dll" }"""), ("My.dll", "x")), _plugins, approver);

        StringAssert.Contains(approver.Asked!.CommandLine, "My.dll");
    }
}
