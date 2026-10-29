using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.IO;

namespace OoBDev.DacFx.Tests;

[TestClass]
public class DacPacBuilderTests
{
    public required TestContext TestContext { get; set; }

    private static string? FindVectorsClr()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var clr = Path.Combine(dir.FullName, "src", "Extensions", "OoBDev.Data.Vectors.Net481", "bin", "Debug", "net48", "OoBDev.Data.Vectors.dll");
            if (File.Exists(clr))
            {
                return clr;
            }
        }

        return null;
    }

    [TestMethod, TestCategory(TestCategories.DevLocal)]
    public void BuildPackageTest()
    {
        // Stage
        var assembly = FindVectorsClr();
        if (assembly is null)
        {
            Assert.Inconclusive("Build src/Extensions/OoBDev.Data.Vectors.Net481 (Debug) first.");
            return;
        }

        var output = Path.Combine(Path.GetTempPath(), "dacpac-" + Guid.NewGuid().ToString("N") + ".dacpac");
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        var builder = new DacPacBuilder(
            loggerFactory.CreateLogger<DacPacBuilder>(),
            new DacPacValidator(loggerFactory.CreateLogger<DacPacValidator>()));

        try
        {
            // Test
            builder.BuildDacPac(assemblyFileFramework: assembly, dacpacFile: output);

            // Assert
            Assert.IsTrue(File.Exists(output));
            Assert.IsTrue(new FileInfo(output).Length > 0);
        }
        finally
        {
            File.Delete(output);
        }
    }
}
