using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace OoBDev.TestUtilities.Tests;

[TestClass]
public class ProjectTools
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.DevLocal)]
    public void FixReadmes()
    {
        var solutionDir = @"C:\repo\oobdev\dotex\";
        var projFilePaths = Directory.EnumerateFiles(solutionDir, "*.csproj", SearchOption.AllDirectories);

        foreach (var projFilePath in projFilePaths)
        {
            var projDir = Path.GetDirectoryName(projFilePath);
            var projFile = Path.GetFileNameWithoutExtension(projFilePath);
            if (projFile.StartsWith("oobdev.", StringComparison.InvariantCultureIgnoreCase) && projFile.Length > 7)
                projFile = projFile.Substring(7);
            var simpleReadmeFile = Path.Combine(projDir, "README.md");
            var realReadmeFile = Path.Combine(projDir, $"ReadMe.{projFile}.md");

            if (File.Exists(simpleReadmeFile) && !File.Exists(realReadmeFile))
            {
                File.Move(simpleReadmeFile, realReadmeFile);
                this.TestContext.WriteLine(projFile);
            }
        }
    }
}
