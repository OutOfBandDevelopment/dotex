using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Cgi;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Services;
using System;
using System.IO;
using System.Linq;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class CgiProgramCatalogTests
{
    private static CgiProgramCatalog Catalog(string folder) => new(Options.Create(new CgiOptions
    {
        ScriptRoot = folder,
        Handlers = { [".exe"] = string.Empty, [".cgi"] = string.Empty },
    }));

    private static string Folder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "cgi-catalog-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "greet.exe"), "MZ");
        File.WriteAllBytes(Path.Combine(folder, "greet.cgi"), [0x7F, (byte)'E', (byte)'L', (byte)'F', 1]);
        File.WriteAllText(Path.Combine(folder, "greet.openapi.json"), "{}");
        return folder;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void List_OnWindows_SkipsLinuxBinariesAndDescriptions()
    {
        var folder = Folder();
        try
        {
            var programs = Catalog(folder).List(isWindows: true);

            CollectionAssert.AreEqual(new[] { "greet.exe" }, programs.Select(p => p.Name).ToArray());
            Assert.AreEqual("/cgi-bin/greet.exe", programs[0].Path);
        }
        finally { Directory.Delete(folder, true); }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void List_OffWindows_ListsEveryHandledFile()
    {
        var folder = Folder();
        try
        {
            var programs = Catalog(folder).List(isWindows: false);

            CollectionAssert.AreEqual(new[] { "greet.cgi", "greet.exe" }, programs.Select(p => p.Name).ToArray());
        }
        finally { Directory.Delete(folder, true); }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void List_MissingFolder_IsEmpty() =>
        Assert.AreEqual(0, Catalog(Path.Combine(Path.GetTempPath(), "nope-" + Guid.NewGuid().ToString("N"))).List(true).Count);
}
