using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Amazon.Sqs.MessageQueueing;
using System.Collections.Generic;
using System.Text.Json;

namespace OoBDev.Amazon.Sqs.Tests.MessageQueueing;

[TestClass]
public class SqsAsyncApiContributorTests
{
    private static IConfigurationSection Section(params (string Key, string Value)[] values)
    {
        var data = new Dictionary<string, string?>();
        foreach (var (key, value) in values)
        {
            data["q:" + key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(data).Build().GetSection("q");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_QueueNameAndRegion_BuildsAwsHostAndBinding()
    {
        // Stage
        var contributor = new SqsAsyncApiContributor();

        // Test
        var result = contributor.Describe(Section(("QueueName", "orders"), ("Region", "eu-west-1")));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("sqs", result.ServerName);
        Assert.AreEqual("sqs.eu-west-1.amazonaws.com", result.Server.Host);
        Assert.AreEqual("sqs", result.Server.Protocol);
        Assert.AreEqual("orders", result.Address);
        Assert.AreEqual("orders", (string?)result.Bindings!["sqs"]!["queue"]!["name"]);
        Assert.IsFalse((bool)result.Bindings["sqs"]!["queue"]!["fifoQueue"]!);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_QueueUrlOnly_TakesTheNameFromTheLastSegmentAndDetectsFifo()
    {
        // Test
        var result = new SqsAsyncApiContributor().Describe(Section(("QueueUrl", "https://sqs.us-east-1.amazonaws.com/123456789012/jobs.fifo")));

        // Assert
        Assert.AreEqual("jobs.fifo", result!.Address);
        Assert.IsTrue((bool)result.Bindings!["sqs"]!["queue"]!["fifoQueue"]!);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_ServiceUrl_UsesItsAuthorityAndDefaultsTheRegionHostOtherwise()
    {
        // Test
        var emulator = new SqsAsyncApiContributor().Describe(Section(("QueueName", "q1"), ("ServiceUrl", "http://localhost:5000")));
        var bare = new SqsAsyncApiContributor().Describe(Section(("QueueName", "q1")));

        // Assert
        Assert.AreEqual("localhost:5000", emulator!.Server.Host);
        Assert.AreEqual("sqs.us-east-1.amazonaws.com", bare!.Server.Host);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_NoQueue_HasNoBindingsAndNoAddress()
    {
        // Test
        var result = new SqsAsyncApiContributor().Describe(Section(("Region", "us-west-2")));

        // Assert
        Assert.IsNull(result!.Address);
        Assert.IsNull(result.Bindings);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Describe_Credentials_AreNeverCopiedIntoTheContribution()
    {
        // Test
        var result = new SqsAsyncApiContributor().Describe(Section(("QueueName", "q1"), ("AccessKey", "AKIA-SECRET"), ("SecretKey", "s3cr3t")));

        // Assert
        var text = JsonSerializer.Serialize(result);
        Assert.IsFalse(text.Contains("AKIA-SECRET"));
        Assert.IsFalse(text.Contains("s3cr3t"));
    }
}
