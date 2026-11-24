using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.DacFx.Tests;

[TestClass]
public class SelectOnlyGuardTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("SELECT 1")]
    [DataRow("select top (5) * from sys.objects where name like 'a%'")]
    [DataRow("WITH c AS (SELECT 1 AS n) SELECT n FROM c")]
    [DataRow("SELECT a.x FROM a JOIN b ON a.id = b.id UNION ALL SELECT 2")]
    public void Check_PlainSelect_IsAllowed(string sql)
    {
        // Test
        var result = SelectOnlyGuard.Check(sql);
        TestContext.WriteLine($"{sql} -> {result}");

        // Assert
        Assert.IsTrue(result.IsAllowed, result.Reason);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("", "empty")]
    [DataRow("   ", "empty")]
    [DataRow("SELECT 1; SELECT 2", "Exactly one")]
    [DataRow("DELETE FROM t", "Only a SELECT")]
    [DataRow("EXEC sp_who", "Only a SELECT")]
    [DataRow("DROP TABLE t", "Only a SELECT")]
    [DataRow("SELECT * INTO copy FROM t", "INTO")]
    [DataRow("SELECT * FROM OPENROWSET('SQLNCLI', 'x', 'SELECT 1')", "OPENROWSET")]
    [DataRow("SELECT * FROM OPENQUERY(srv, 'SELECT 1')", "OPENQUERY")]
    [DataRow("SELECT * FROM OPENDATASOURCE('SQLNCLI', 'x').db.dbo.t", "OPENDATASOURCE")]
    [DataRow("DECLARE @a int; SELECT @a = 1", "Exactly one")]
    [DataRow("SELECT FROM WHERE", "parse")]
    public void Check_Refused_GivesTheReason(string sql, string expected)
    {
        // Test
        var result = SelectOnlyGuard.Check(sql);
        TestContext.WriteLine($"{sql} -> {result}");

        // Assert
        Assert.IsFalse(result.IsAllowed);
        Assert.Contains(expected, result.Reason!, global::System.StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Check_Null_IsRefused() => Assert.IsFalse(SelectOnlyGuard.Check(null).IsAllowed);
}
