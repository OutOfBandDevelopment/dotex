using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.GroqCloud.Tests;

[TestClass]
public class GroqCloudApiKeyTests
{
    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void FromEnvironment_GroqApiKeySet_ReturnsIt()
    {
        var previous = Environment.GetEnvironmentVariable(GroqCloudApiKey.EnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(GroqCloudApiKey.EnvironmentVariable, "test-key");
            Assert.AreEqual("test-key", GroqCloudApiKey.FromEnvironment());
        }
        finally
        {
            Environment.SetEnvironmentVariable(GroqCloudApiKey.EnvironmentVariable, previous);
        }
    }
}
