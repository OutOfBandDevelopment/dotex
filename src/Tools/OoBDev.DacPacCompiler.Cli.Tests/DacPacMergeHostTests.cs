using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.DacPacCompiler.Cli.Tests;

[TestClass]
public class DacPacMergeHostTests
{
    [TestMethod]
    [TestCategory("Unit")]
    [DataRow(new string[0], DacPackTools.SqlClr)]
    [DataRow(new[] { "--tool", "Merge" }, DacPackTools.Merge)]
    [DataRow(new[] { "--tool", "merge" }, DacPackTools.Merge)]
    [DataRow(new[] { "--tool=SqlClr" }, DacPackTools.SqlClr)]
    [DataRow(new[] { "--tool", "Nope" }, DacPackTools.Unknown)]
    [DataRow(new[] { "--tool" }, DacPackTools.Unknown)]
    public void SelectTool_Arguments_PicksTool(string[] args, DacPackTools expected)
    {
        // Stage

        // Test
        var actual = DacPacMergeHost.SelectTool(args);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Create_TwoSourcePackages_WritesMergedPackage()
    {
        // Stage
        using var folder = new TempFileFactory(null!).GetTempDirectory();
        BuildPackage(Path.Combine(folder.DirectoryPath, "a.dacpac"), "CREATE TABLE [dbo].[A] ([Id] INT NOT NULL PRIMARY KEY)");
        BuildPackage(Path.Combine(folder.DirectoryPath, "b.dacpac"), "CREATE TABLE [dbo].[B] ([Id] INT NOT NULL PRIMARY KEY)");
        var target = Path.Combine(folder.DirectoryPath, "out", "merged.dacpac");
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        string[] args =
        [
            "--tool", "Merge",
            "--source-path", folder.DirectoryPath,
            "--source-patterns", "*.dacpac",
            "--target-path", target,
            "--version", "1.2.3",
            "--setting-modeloptionsource", "First",
        ];

        // Test
        using var host = DacPacMergeHost.Create(args).Build();
        await host.StartAsync();
        await host.StopAsync();

        // Assert
        Assert.IsTrue(File.Exists(target));
        using var merged = TSqlModel.LoadFromDacpac(target, new ModelLoadOptions());
        var tables = merged.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Table);
        Assert.AreEqual(2, tables.Count());
    }

    private static void BuildPackage(string path, string sql)
    {
        using var model = new TSqlModel(SqlServerVersion.Sql160, new TSqlModelOptions());
        model.AddObjects(sql);
        DacPackageExtensions.BuildPackage(path, model, new PackageMetadata { Name = Path.GetFileNameWithoutExtension(path), Version = "1.0.0" });
    }
}
