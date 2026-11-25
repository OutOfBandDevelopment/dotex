using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OoBDev.PackMan.Cli.Tests;

[TestClass]
public class PackagePropsTests
{
    private const string Props = """
        <Project>
          <PropertyGroup>
            <Alpha>1.0.0</Alpha>
            <Empty></Empty>
          </PropertyGroup>
          <ItemGroup>
            <PackageVersion Include="Plain" Version="1.0.0" />
            <PackageVersion Include="Locked" Version="1.0.0"><_LockVersion>true</_LockVersion></PackageVersion>
            <PackageVersion Include="NotAVersion" Version="$(Alpha)" />
          </ItemGroup>
          <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
            <PackageVersion Include="Conditioned" Version="8.0.1" />
            <PackageVersion Include="ConditionedPre" Version="8.0.1-beta" />
          </ItemGroup>
        </Project>
        """;

    private readonly PackageProps _props = new(NullLogger<PackageProps>.Instance);

    private sealed class FakeSource(Dictionary<string, string[]> versions) : IPackageVersionSource
    {
        public List<string> Asked { get; } = [];

        public Task<IReadOnlyList<NuGetVersion>> GetVersionsAsync(string packageName, CancellationToken cancellationToken)
        {
            Asked.Add(packageName);
            return Task.FromResult<IReadOnlyList<NuGetVersion>>([.. versions[packageName].Select(NuGetVersion.Parse).Order()]);
        }
    }

    private static string VersionOf(XDocument doc, string package) =>
        (string)doc.Descendants("PackageVersion").Single(p => (string?)p.Attribute("Include") == package).Attribute("Version")!;

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow(null, 0)]
    [DataRow("", 0)]
    [DataRow("A", 1)]
    [DataRow("A,1.0;B, 2.0 ;;,3", 2)]
    public void SplitVersions_Text_ReturnsNamedPairs(string? text, int count)
    {
        Assert.AreEqual(count, PackageProps.SplitVersions(text).Count());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SplitVersions_NameAndVersion_AreTrimmed()
    {
        var pair = PackageProps.SplitVersions(" A , 1.2.3 ").Single();

        Assert.AreEqual("A", pair.Name);
        Assert.AreEqual("1.2.3", pair.Version);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void UpdateTargetVersions_NamedProperty_IsSet()
    {
        var doc = XDocument.Parse(Props);

        _props.UpdateTargetVersions(doc, [("Alpha", "2.0.0"), ("Missing", "1")]);

        Assert.AreEqual("2.0.0", (string)doc.Descendants("Alpha").Single());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetProperties_PropertyGroups_SkipsEmptyValues()
    {
        var properties = PackageProps.GetProperties(XDocument.Parse(Props)).ToArray();

        Assert.AreEqual(1, properties.Length);
        Assert.AreEqual(("Alpha", "1.0.0"), properties[0]);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task UpdateNugetVersionsAsync_Items_FollowTheRules()
    {
        // Stage
        var doc = XDocument.Parse(Props);
        var source = new FakeSource(new()
        {
            ["Plain"] = ["1.0.0", "1.5.0", "2.0.0", "3.0.0-beta"],
            ["Locked"] = ["9.0.0"],
            ["Conditioned"] = ["8.0.1", "8.0.9", "8.1.0-beta", "9.0.0"],
            ["ConditionedPre"] = ["8.0.1-beta", "8.0.1-rc", "8.0.2", "9.0.0"],
        });

        // Test
        await _props.UpdateNugetVersionsAsync(doc, source, CancellationToken.None);

        // Assert
        Assert.AreEqual("2.0.0", VersionOf(doc, "Plain"));
        Assert.AreEqual("1.0.0", VersionOf(doc, "Locked"));
        Assert.AreEqual("$(Alpha)", VersionOf(doc, "NotAVersion"));
        Assert.AreEqual("8.0.9", VersionOf(doc, "Conditioned"));
        Assert.AreEqual("8.0.2", VersionOf(doc, "ConditionedPre"));
        CollectionAssert.DoesNotContain(source.Asked, "Locked");
        CollectionAssert.DoesNotContain(source.Asked, "NotAVersion");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task UpdateNugetVersionsAsync_Cancelled_AsksNothing()
    {
        var doc = XDocument.Parse(Props);
        var source = new FakeSource([]);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await _props.UpdateNugetVersionsAsync(doc, source, cts.Token);

        Assert.AreEqual(0, source.Asked.Count);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Open_MissingFile_GivesEmptyProject()
    {
        var doc = PackageProps.Open(Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}.props"));

        Assert.AreEqual("Project", doc.Root?.Name.LocalName);
        Assert.IsFalse(doc.Root!.HasElements);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void OpenAndSave_ImportsFollowed_RoundTrips()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllText(Path.Combine(folder, "child.props"), "<Project><PropertyGroup><Child>c</Child></PropertyGroup></Project>");
            File.WriteAllText(Path.Combine(folder, "root.props"), "<Project><Import Project=\"child.props\" /></Project>");

            var doc = PackageProps.Open(Path.Combine(folder, "root.props"), followImports: true);
            var output = Path.Combine(folder, "sub", "out.props");
            _props.Save(doc, output);

            Assert.IsFalse(doc.Descendants("Import").Any());
            Assert.AreEqual("c", PackageProps.GetProperties(XDocument.Load(output)).Single().Value);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
