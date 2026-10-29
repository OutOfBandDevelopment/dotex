using System.IO;
using System.Xml.XPath;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Xml.XPath;

namespace OoBDev.Extensions.CodeAnalysis.DacFx.Tests;

[TestClass]
public class DacPacNavigatorTests
{
    private const string Schema = """
        CREATE TABLE [dbo].[Customer] ([Id] INT NOT NULL PRIMARY KEY, [Name] NVARCHAR(100) NOT NULL);
        """;

    private static TSqlModel SampleModel()
    {
        var model = new TSqlModel(SqlServerVersion.Sql160, new TSqlModelOptions());
        model.AddObjects(Schema);
        return model;
    }

    private static int Count(XPathNavigator navigator, string xpath) => navigator.Select(xpath).Count;

    [TestMethod]
    [TestCategory("Unit")]
    public void Build_Model_ListsTopLevelObjectsByType()
    {
        // Stage
        var navigator = new DacPacElementNodeBuilder(SampleModel()).Build().ToNavigable().CreateNavigator()!;

        // Test
        var tables = Count(navigator, "//Table");

        // Assert
        Assert.IsTrue(tables >= 1);
        Assert.AreEqual(1, Count(navigator, "//Table[@Name='[dbo].[Customer]']"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Build_Model_IncludesColumnsAndScript()
    {
        var navigator = new DacPacElementNodeBuilder(SampleModel()).Build().ToNavigable().CreateNavigator()!;

        Assert.IsTrue(Count(navigator, "//Table[@Name='[dbo].[Customer]']/Column") >= 1);
        Assert.AreEqual(1, Count(navigator, "//Table[@Name='[dbo].[Customer]']/SCRIPT"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ToNavigable_File_ReadsPackage()
    {
        var path = Path.Combine(Path.GetTempPath(), $"Test_{(global::System.Guid.NewGuid()):N}.dacpac");
        try
        {
            using (var model = SampleModel())
                DacPackageExtensions.BuildPackage(path, model, new PackageMetadata { Name = "Sample", Version = "1.0.0.0" });

            var navigator = new DacPacNavigator().ToNavigable(path)!.CreateNavigator()!;

            Assert.AreEqual(1, Count(navigator, "//Table[@Name='[dbo].[Customer]']"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ToNavigable_Stream_ReadsPackage()
    {
        var path = Path.Combine(Path.GetTempPath(), $"Test_{(global::System.Guid.NewGuid()):N}.dacpac");
        try
        {
            using (var model = SampleModel())
                DacPackageExtensions.BuildPackage(path, model, new PackageMetadata { Name = "Sample", Version = "1.0.0.0" });

            using var stream = File.OpenRead(path);
            var navigator = new DacPacNavigator().ToNavigable(stream)!.CreateNavigator()!;

            Assert.AreEqual(1, Count(navigator, "//Table[@Name='[dbo].[Customer]']"));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
