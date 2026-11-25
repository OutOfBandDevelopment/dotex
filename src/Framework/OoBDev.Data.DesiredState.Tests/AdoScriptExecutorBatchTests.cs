using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public sealed class AdoScriptExecutorBatchTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void SplitBatches_GoLines_SplitAndDropEmptyBatches()
    {
        var batches = AdoScriptExecutor.SplitBatches("CREATE TABLE a (id int)\r\nGO\r\n\r\ngo\r\nINSERT INTO a VALUES (1)\nGO");

        Assert.HasCount(2, batches);
        Assert.AreEqual("CREATE TABLE a (id int)", batches[0].Trim());
        Assert.AreEqual("INSERT INTO a VALUES (1)", batches[1].Trim());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SplitBatches_GoInsideAWord_IsNotASeparator()
    {
        var batches = AdoScriptExecutor.SplitBatches("SELECT 'GO'\nSELECT 1 AS GOAL");

        Assert.HasCount(1, batches);
    }
}
