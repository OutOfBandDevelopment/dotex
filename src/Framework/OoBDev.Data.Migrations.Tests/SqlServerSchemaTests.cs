using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.EntityFrameworkCore;
using OoBDev.Communications.EntityFrameworkCore;
using OoBDev.ComplexEvents.EntityFrameworkCore;
using OoBDev.TestUtilities;
using System;
using System.IO;

namespace OoBDev.Data.Migrations.Tests;

[TestClass]
public class SqlServerSchemaTests
{
    private const string SchemaProject = "OoBDev.Data.Schema.SqlServer";

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void RightAudit_SqlProjectMatchesTheModel() => AssertMatches<RightAuditDbContext>("RightAudit", o => new RightAuditDbContext(o));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Suppressions_SqlProjectMatchesTheModel() => AssertMatches<SuppressionsDbContext>("Suppressions", o => new SuppressionsDbContext(o));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Schedule_SqlProjectMatchesTheModel() => AssertMatches<ScheduleDbContext>("Schedule", o => new ScheduleDbContext(o));

    private static void AssertMatches<T>(string name, Func<DbContextOptions<T>, T> create)
        where T : DbContext
    {
        using var context = create(new DbContextOptionsBuilder<T>().UseSqlServer("Server=unused;Database=unused").Options);
        var generated = Normalize(context.Database.GenerateCreateScript());
        var file = Path.Combine(FindSrc(), "Framework", SchemaProject, "Tables", $"{name}.sql");
        if (Environment.GetEnvironmentVariable("REGENERATE_SQL_SCHEMA") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, generated);
        }

        Assert.IsTrue(File.Exists(file), $"Missing {file}; run with REGENERATE_SQL_SCHEMA=1.");
        Assert.AreEqual(generated, Normalize(File.ReadAllText(file)), $"{name}.sql is out of date; run with REGENERATE_SQL_SCHEMA=1.");
    }

    // The SQL project needs GO between statements; only blank-line runs are collapsed.
    private static string Normalize(string sql)
    {
        var text = sql.Replace("\r\n", "\n");
        while (text.Contains("\n\n\n", StringComparison.Ordinal))
            text = text.Replace("\n\n\n", "\n\n", StringComparison.Ordinal);
        return text.Trim() + "\n";
    }

    private static string FindSrc()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Directory.Packages.props")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("src folder not found.");
    }
}
