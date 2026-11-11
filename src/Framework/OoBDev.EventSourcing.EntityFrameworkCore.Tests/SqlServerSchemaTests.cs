using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.EventSourcing.SqlServer;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.EventSourcing.EntityFrameworkCore.Tests;

[TestClass]
public class SqlServerSchemaTests
{
    private static string FindDacpac()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var project = Path.Combine(directory.FullName, "OoBDev.EventSourcing.SqlServer.Database", "bin");
            if (Directory.Exists(project))
                return Directory.EnumerateFiles(project, "*.dacpac", SearchOption.AllDirectories)
                    .OrderByDescending(File.GetLastWriteTimeUtc).First();
            directory = directory.Parent;
        }
        throw new FileNotFoundException("The SQL Server dacpac was not built.");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Dacpac_TablesAndColumnsMatchTheModel()
    {
        // Stage: the EF model for SQL Server and the model.xml of the built dacpac
        var builder = new DbContextOptionsBuilder<EventSourcingDbContext>().UseSqlServer("Server=unused;Database=unused");
        using var db = new EventSourcingDbContext(builder.Options);
        string xml;
        using (var archive = ZipFile.OpenRead(FindDacpac()))
        using (var reader = new StreamReader(archive.GetEntry("model.xml")!.Open()))
            xml = reader.ReadToEnd();

        // Test / Assert: every mapped table has the same column set in the dacpac
        foreach (var entity in db.Model.GetEntityTypes())
        {
            var table = entity.GetTableName()!;
            var expected = entity.GetProperties().Select(p => p.GetColumnName()).Order().ToArray();
            var actual = Regex.Matches(xml, $@"Name=""\[dbo\]\.\[{table}\]\.\[(?<c>[^\]]+)\]""")
                .Select(m => m.Groups["c"].Value)
                .Where(c => expected.Contains(c))
                .Distinct().Order().ToArray();
            Assert.IsTrue(xml.Contains($"[dbo].[{table}]", StringComparison.Ordinal), $"Table {table} missing from the dacpac.");
            CollectionAssert.AreEqual(expected, actual, $"Columns of {table}");
        }
    }
}
