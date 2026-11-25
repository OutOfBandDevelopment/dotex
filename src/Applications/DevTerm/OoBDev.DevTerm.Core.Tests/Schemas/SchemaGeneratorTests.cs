using System;
using System.IO;
using OoBDev.System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Schemas.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class SchemaGeneratorTests
{
    [TestMethod]
    public void WriteTo_NewFolder_WritesEverySchemaFile()
    {
        // Stage
        using var temp = new TempDirectoryHandle();
        var folder = Path.Combine(temp.DirectoryPath, "schemas");
        {
            // Test
            var written = SchemaGenerator.WriteTo(folder);

            // Assert
            CollectionAssert.AreEquivalent(SchemaGenerator.Generate().Keys.ToArray(), written.Select(Path.GetFileName).ToArray());
            foreach (var (name, text) in SchemaGenerator.Generate())
            {
                Assert.AreEqual(text, File.ReadAllText(Path.Combine(folder, name)));
            }
        }
    }

    [TestMethod]
    public void WriteTo_BlankFolder_Throws() =>
        Assert.ThrowsExactly<ArgumentException>(() => SchemaGenerator.WriteTo(" "));
}
