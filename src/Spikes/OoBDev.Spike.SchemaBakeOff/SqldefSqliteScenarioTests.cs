using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Spike.SchemaBakeOff;

/// <summary>
/// The sqldef-on-SQLite column of the bake-off (docs/design/DesiredStateDatabase/bake-off-test-plan.md), replayable: S1 to S3, S5, S6, S8, S9, S11 and S12.
/// A file database is the throwaway, so no container is needed; the tests end Inconclusive when sqlite3def is not on PATH.
/// </summary>
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public sealed class SqldefSqliteScenarioTests
{
    public TestContext TestContext { get; set; } = null!;

    private const string Base = "CREATE TABLE customer (id INTEGER PRIMARY KEY, name TEXT NOT NULL);\nCREATE TABLE orders (id INTEGER PRIMARY KEY, customer_id INTEGER NOT NULL REFERENCES customer (id), total NUMERIC NOT NULL DEFAULT 0);\n";

    private SqliteScenarioHarness Stage()
    {
        if (!SqliteScenarioHarness.ToolAvailable("sqlite3def"))
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH: go install github.com/sqldef/sqldef/v3/cmd/sqlite3def@latest");
        var harness = new SqliteScenarioHarness();
        TestContext.WriteLine("database " + harness.DatabasePath);
        return harness;
    }

    [TestMethod]
    public async Task S1S2_ApplyToEmptyThenAgain_CreatesTablesThenChangesNothing()
    {
        // Stage
        using var h = Stage();

        // Test
        var first = await h.ApplyAsync(Base, false, TestContext.CancellationToken);
        var tables = await h.ObjectsAsync("table");
        var check = await h.CheckAsync(Base, TestContext.CancellationToken);
        TestContext.WriteLine($"tables={string.Join(",", tables)} check-exit={check.ExitCode}");

        // Assert
        Assert.AreEqual(0, first.ExitCode, first.StandardError);
        CollectionAssert.AreEqual(new[] { "customer", "orders" }, tables.ToArray());
        Assert.AreEqual(0, check.ExitCode, "S2: a synced database makes --check exit 0");
    }

    [TestMethod]
    public async Task S3_AddColumnIndexAndView_PlansExactlyThoseSteps()
    {
        // Stage
        using var h = Stage();
        await h.ApplyAsync(Base, false, TestContext.CancellationToken);
        var changed = Base.Replace("name TEXT NOT NULL", "name TEXT NOT NULL, email TEXT")
            + "CREATE INDEX ix_orders_customer ON orders (customer_id);\nCREATE VIEW v_totals AS SELECT customer_id, SUM(total) AS total FROM orders GROUP BY customer_id;\n";

        // Test
        var plan = await h.DryRunAsync(changed, TestContext.CancellationToken);
        TestContext.WriteLine(plan.StandardOutput);

        // Assert
        Assert.AreEqual(0, plan.ExitCode, plan.StandardError);
        StringAssert.Contains(plan.StandardOutput, "ADD COLUMN", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(plan.StandardOutput, "CREATE INDEX", StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(plan.StandardOutput, "CREATE VIEW", StringComparison.OrdinalIgnoreCase);
        Assert.IsFalse(plan.StandardOutput.Contains("DROP TABLE", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task S5_DropTableWithoutTheFlag_IsSkippedAndDroppedWithIt()
    {
        // Stage
        using var h = Stage();
        await h.ApplyAsync(Base, false, TestContext.CancellationToken);
        const string withoutOrders = "CREATE TABLE customer (id INTEGER PRIMARY KEY, name TEXT NOT NULL);\n";

        // Test
        await h.ApplyAsync(withoutOrders, false, TestContext.CancellationToken);
        var kept = await h.ObjectsAsync("table");
        await h.ApplyAsync(withoutOrders, true, TestContext.CancellationToken);
        var dropped = await h.ObjectsAsync("table");
        TestContext.WriteLine($"without flag: {string.Join(",", kept)}; with flag: {string.Join(",", dropped)}");

        // Assert
        CollectionAssert.Contains(kept.ToArray(), "orders");
        CollectionAssert.DoesNotContain(dropped.ToArray(), "orders");
    }

    [TestMethod]
    public async Task S6_NarrowAType_NeverNamesTheHazard()
    {
        // Stage
        using var h = Stage();
        await h.ApplyAsync("CREATE TABLE t (id INTEGER PRIMARY KEY, code VARCHAR(50));\n", false, TestContext.CancellationToken);

        // Test
        var plan = await h.DryRunAsync("CREATE TABLE t (id INTEGER PRIMARY KEY, code VARCHAR(5));\n", TestContext.CancellationToken);
        TestContext.WriteLine($"exit={plan.ExitCode} plan=[{plan.StandardOutput.Trim()}]");

        // Assert: the gap the layer-2 PlanAnalyzer exists for
        Assert.IsFalse(plan.StandardOutput.Contains("hazard", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(plan.StandardOutput.Contains("data loss", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task S8_ChildScriptBeforeParent_IsReordered()
    {
        // Stage
        using var h = Stage();
        const string outOfOrder = "CREATE TABLE child (id INTEGER PRIMARY KEY, parent_id INTEGER REFERENCES parent (id));\nCREATE TABLE parent (id INTEGER PRIMARY KEY);\n";

        // Test
        var result = await h.ApplyAsync(outOfOrder, false, TestContext.CancellationToken);
        var tables = await h.ObjectsAsync("table");

        // Assert
        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        CollectionAssert.AreEquivalent(new[] { "child", "parent" }, tables.ToArray());
    }

    [TestMethod]
    public async Task S9_ViewChange_IsPlannedAndAnUnchangedSchemaPlansNothing()
    {
        // Stage
        using var h = Stage();
        var v1 = Base + "CREATE VIEW v_orders AS SELECT id FROM orders;\nCREATE TRIGGER trg_orders AFTER INSERT ON orders BEGIN UPDATE orders SET total = 1 WHERE id = NEW.id; END;\n";
        var v2 = v1.Replace("SELECT id FROM orders", "SELECT id, total FROM orders");
        await h.ApplyAsync(v1, false, TestContext.CancellationToken);

        // Test
        var unchanged = await h.DryRunAsync(v1, TestContext.CancellationToken);
        var changed = await h.DryRunAsync(v2, TestContext.CancellationToken);
        TestContext.WriteLine($"unchanged=[{unchanged.StandardOutput.Trim()}] changed=[{changed.StandardOutput.Trim()}]");

        // Assert
        Assert.IsFalse(unchanged.StandardOutput.Contains("CREATE", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(changed.StandardOutput, "v_orders", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public async Task S11_HandAddedColumn_IsNotDroppedWithoutTheFlag()
    {
        // Stage
        using var h = Stage();
        await h.ApplyAsync(Base, false, TestContext.CancellationToken);
        await h.ExecuteAsync("ALTER TABLE customer ADD COLUMN drift TEXT");

        // Test
        var check = await h.CheckAsync(Base, TestContext.CancellationToken);
        await h.ApplyAsync(Base, false, TestContext.CancellationToken);
        var columns = await h.ColumnsAsync("customer");
        TestContext.WriteLine($"check-exit={check.ExitCode} columns={string.Join(",", columns)}");

        // Assert
        Assert.AreEqual(2, check.ExitCode, "drift is pending, so --check exits 2");
        CollectionAssert.Contains(columns.ToArray(), "drift");
    }

    [TestMethod]
    public async Task S12_ExitCodes_AreUsableFromDotNet()
    {
        // Stage
        using var h = Stage();

        // Test
        var syntax = await h.DryRunAsync("CREATE TABLEE nope (;\n", TestContext.CancellationToken);
        var pending = await h.CheckAsync(Base, TestContext.CancellationToken);
        TestContext.WriteLine($"syntax-error exit={syntax.ExitCode}; pending exit={pending.ExitCode}");

        // Assert
        Assert.AreNotEqual(0, syntax.ExitCode);
        Assert.AreEqual(2, pending.ExitCode);
        Assert.IsFalse(pending.StandardOutput.TrimStart().StartsWith('{'), "the plan is SQL text, not JSON");
    }
}
