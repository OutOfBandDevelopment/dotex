using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Microsoft.SqlServer.ServiceBroker.MessageQueueing;
using System.Collections.Generic;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker.Tests.MessageQueueing;

[TestClass]
public class BrokerScriptsTests
{
    private static ServiceBrokerOptions Options(string target = "//svc/orders") =>
        ServiceBrokerOptions.From(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["q:ConnectionString"] = "x",
            ["q:QueueName"] = "orders",
            ["q:ServiceName"] = "//svc/orders",
            ["q:TargetServiceName"] = target,
        }).Build().GetSection("q"));

    [TestMethod]
    [TestCategory("Unit")]
    public void Receive_Options_WaitsOnTheQuotedQueue()
    {
        // Test
        var sql = BrokerScripts.Receive(Options());

        // Assert
        StringAssert.Contains(sql, "FROM [orders]");
        StringAssert.Contains(sql, "TIMEOUT @timeout");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Ensure_SameServiceAndTarget_CreatesEachObjectOnceGuarded()
    {
        // Test
        var sql = BrokerScripts.Ensure(Options());

        // Assert
        StringAssert.Contains(sql, "CREATE MESSAGE TYPE [//OoBDev/MessageQueueing/Message] VALIDATION = NONE");
        StringAssert.Contains(sql, "CREATE CONTRACT [//OoBDev/MessageQueueing/Contract]");
        StringAssert.Contains(sql, "CREATE QUEUE [orders]");
        Assert.AreEqual(1, sql.Split("CREATE SERVICE").Length - 1);
        Assert.AreEqual(4, sql.Split("IF ").Length - 1);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Ensure_DifferentTarget_CreatesBothServices()
    {
        // Test
        var sql = BrokerScripts.Ensure(Options("//svc/other"));

        // Assert
        Assert.AreEqual(2, sql.Split("CREATE SERVICE").Length - 1);
        StringAssert.Contains(sql, "[//svc/other]");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Escaping_QuotesInNames_AreDoubled()
    {
        // Test / Assert
        Assert.AreEqual("[a]]b]", BrokerScripts.Bracket("a]b"));
        Assert.AreEqual("N'it''s'", BrokerScripts.Literal("it's"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Send_Script_UsesParametersForServicesAndBody()
    {
        // Assert
        StringAssert.Contains(BrokerScripts.Send, "FROM SERVICE @from TO SERVICE @to ON CONTRACT @contract");
        StringAssert.Contains(BrokerScripts.Send, "(@body)");
    }
}
