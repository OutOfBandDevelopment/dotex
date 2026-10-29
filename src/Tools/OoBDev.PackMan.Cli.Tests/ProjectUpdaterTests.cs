using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OoBDev.PackMan.Cli.Tests;

[TestClass]
public class ProjectUpdaterTests
{
    private const string Props = """
        <Project>
          <ItemGroup>
            <PackageVersion Include="Pkg" Version="2.0.0" />
          </ItemGroup>
          <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
            <PackageVersion Include="Old" Version="8.0.0" />
          </ItemGroup>
        </Project>
        """;

    private string _folder = string.Empty;

    [TestInitialize]
    public void Init()
    {
        _folder = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_folder);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_folder, recursive: true);

    private string Write(string name, string content)
    {
        var path = Path.Combine(_folder, name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Update_MatchingProject_SetsVersionsForItsFramework()
    {
        // Stage
        var project = Write("a/a.csproj", """
            <Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            <ItemGroup><PackageReference Include="Pkg" Version="1.0.0" /><PackageReference Include="Old" Version="1.0.0" /><PackageReference Include="Bare" /></ItemGroup></Project>
            """);

        // Test
        var saved = ProjectUpdater.Update(XDocument.Parse(Props), _folder, "**/*.csproj");

        // Assert
        Assert.AreEqual(1, saved.Count);
        var references = XElement.Load(project).Descendants("PackageReference").ToDictionary(r => (string)r.Attribute("Include")!, r => (string?)r.Attribute("Version"));
        Assert.AreEqual("2.0.0", references["Pkg"]);
        Assert.AreEqual("1.0.0", references["Old"]);
        Assert.IsNull(references["Bare"]);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Update_ConditionMatchesFramework_UsesConditionedVersion()
    {
        var project = Write("b.csproj", """
            <Project><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup>
            <ItemGroup><PackageReference Include="Old" Version="1.0.0" /></ItemGroup></Project>
            """);

        ProjectUpdater.Update(XDocument.Parse(Props), _folder, "*.csproj");

        Assert.AreEqual("8.0.0", (string?)XElement.Load(project).Descendants("PackageReference").Single().Attribute("Version"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Update_ExcludedAndUnchangedProjects_AreNotSaved()
    {
        Write("keep/k.csproj", """
            <Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            <ItemGroup><PackageReference Include="Pkg" Version="2.0.0" /></ItemGroup></Project>
            """);
        Write("skip/s.csproj", """
            <Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            <ItemGroup><PackageReference Include="Pkg" Version="1.0.0" /></ItemGroup></Project>
            """);

        var saved = ProjectUpdater.Update(XDocument.Parse(Props), _folder, "**/*.csproj;!skip/**");

        Assert.AreEqual(0, saved.Count);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Update_ProjectWithoutTargetFramework_Throws()
    {
        Write("c.csproj", "<Project><ItemGroup><PackageReference Include=\"Pkg\" Version=\"1\" /></ItemGroup></Project>");

        Assert.ThrowsExactly<NotSupportedException>(() => ProjectUpdater.Update(XDocument.Parse(Props), _folder, "*.csproj"));
    }
}
