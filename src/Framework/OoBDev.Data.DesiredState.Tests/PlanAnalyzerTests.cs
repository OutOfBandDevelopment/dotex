using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public sealed class PlanAnalyzerTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_DryRunOutput_SplitsStatementsAndSkippedDrops()
    {
        // Stage
        const string output = "-- dry run --\nCREATE TABLE a (\n  id int\n);\nALTER TABLE a ADD COLUMN n text;\n-- Skipped: ALTER TABLE a DROP COLUMN old;\n";

        // Test
        var (statements, skipped) = PlanAnalyzer.Parse(output);
        TestContext.WriteLine($"statements={statements.Count} skipped={skipped.Count}");

        // Assert
        Assert.HasCount(2, statements);
        StringAssert.StartsWith(statements[0], "CREATE TABLE a");
        Assert.AreEqual("ALTER TABLE a DROP COLUMN old;", skipped.Single());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_EmptyOutput_IsAnEmptyPlan()
    {
        var (statements, skipped) = PlanAnalyzer.Parse("-- Nothing is modified --\n");

        Assert.IsEmpty(statements);
        Assert.IsEmpty(skipped);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("DROP TABLE users;", HazardKind.DataLoss)]
    [DataRow("ALTER TABLE users DROP COLUMN name;", HazardKind.DataLoss)]
    [DataRow("TRUNCATE TABLE users;", HazardKind.DataLoss)]
    [DataRow("DROP INDEX ix_users;", HazardKind.Drop)]
    [DataRow("ALTER TABLE users DROP CONSTRAINT fk;", HazardKind.Drop)]
    [DataRow("ALTER TABLE users ALTER COLUMN name TYPE varchar(5);", HazardKind.TypeChange)]
    [DataRow("ALTER TABLE users ALTER COLUMN name SET DATA TYPE text;", HazardKind.TypeChange)]
    [DataRow("ALTER TABLE users ALTER COLUMN [name] nvarchar(5);", HazardKind.TypeChange)]
    [DataRow("ALTER TABLE users MODIFY name varchar2(5);", HazardKind.TypeChange)]
    public void Analyze_HazardousStatement_IsFlagged(string statement, HazardKind expected)
    {
        var hazards = PlanAnalyzer.Analyze([statement]);
        TestContext.WriteLine(string.Join(", ", hazards.Select(h => h.Kind)));

        CollectionAssert.Contains(hazards.Select(h => h.Kind).ToList(), expected);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("CREATE TABLE users (id int);")]
    [DataRow("ALTER TABLE users ADD COLUMN name text;")]
    [DataRow("ALTER TABLE users ALTER COLUMN name SET NOT NULL;")]
    [DataRow("ALTER TABLE users ALTER COLUMN name DROP DEFAULT;")]
    [DataRow("CREATE INDEX ix ON users (name);")]
    public void Analyze_SafeStatement_HasNoHazard(string statement) =>
        Assert.IsEmpty(PlanAnalyzer.Analyze([statement]));
}
