using System.IO;
using OoBDev.System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public sealed class AppliedScriptStoreInstanceTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Record_TwoPrefixedInstances_KeepSeparateHistories()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        var file = Path.Combine(folder.DirectoryPath, "history.db");
        var store = new AdoAppliedScriptStore(t => new SqliteConnection($"Data Source={t.Database};Pooling=False"));
        var a = DatabaseTarget.File(file).ForInstance(tablePrefix: "a_");
        var b = DatabaseTarget.File(file).ForInstance(tablePrefix: "b_");
        try
        {
            // Test
            await store.RecordAsync(a, "010_seed.once.sql", "h1", TestContext.CancellationToken);
            var inA = await store.HasAppliedAsync(a, "010_seed.once.sql", "h1", TestContext.CancellationToken);
            var inB = await store.HasAppliedAsync(b, "010_seed.once.sql", "h1", TestContext.CancellationToken);
            TestContext.WriteLine($"a={inA} b={inB}");

            // Assert
            Assert.IsTrue(inA);
            Assert.IsFalse(inB);
            StringAssert.Contains(AdoAppliedScriptStore.SchemaScriptFor(a), "\"a___desired_state_history\"");
            StringAssert.Contains(AdoAppliedScriptStore.SchemaScriptFor(DatabaseTarget.File(file).ForInstance("s1")), "\"s1\".\"__desired_state_history\"");
        }
        finally
        {
            SqliteConnection.ClearAllPools();
        }
    }
}
