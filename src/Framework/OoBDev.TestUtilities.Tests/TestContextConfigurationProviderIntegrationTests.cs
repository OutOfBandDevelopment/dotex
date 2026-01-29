using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace OoBDev.TestUtilities.Tests;

/// <summary>
/// Integration tests for TestContext configuration provider using actual .runsettings values.
/// These tests verify end-to-end functionality with MSTest's TestContext.Properties population.
/// </summary>
[TestClass]
public class TestContextConfigurationProviderIntegrationTests
{
    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the current test run.
    /// </summary>
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public void AddTestContext_WithRunsettingsValues_ReadsFlatValues()
    {
        // Arrange & Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        // Assert - These values come from .runsettings
        var environment = config["TestConfig_Environment"];
        var version = config["TestConfig_Version"];

        Assert.IsNotNull(environment, "TestConfig_Environment should be set in .runsettings");
        Assert.IsNotNull(version, "TestConfig_Version should be set in .runsettings");
        Assert.AreEqual("IntegrationTest", environment);
        Assert.AreEqual("1.0.0", version);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public void AddTestContext_WithRunsettingsValues_ReadsHierarchicalColonValues()
    {
        // Arrange & Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        // Assert - These use colon separator in .runsettings
        var host = config["TestConfig:Database:Host"];
        var port = config["TestConfig:Database:Port"];

        Assert.IsNotNull(host, "TestConfig:Database:Host should be set in .runsettings");
        Assert.IsNotNull(port, "TestConfig:Database:Port should be set in .runsettings");
        Assert.AreEqual("test-db-host", host);
        Assert.AreEqual("5432", port);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public void AddTestContext_WithRunsettingsValues_NormalizesDoubleUnderscores()
    {
        // Arrange & Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        // Assert - These use double underscores in .runsettings
        var baseUrl = config["TestConfig:Api:BaseUrl"];
        var timeout = config["TestConfig:Api:Timeout"];
        var retryCount = config["TestConfig:Api:RetryCount"];

        Assert.IsNotNull(baseUrl, "TestConfig__Api__BaseUrl should be set in .runsettings");
        Assert.IsNotNull(timeout, "TestConfig__Api__Timeout should be set in .runsettings");
        Assert.IsNotNull(retryCount, "TestConfig__Api__RetryCount should be set in .runsettings");
        Assert.AreEqual("https://api.integration.test", baseUrl);
        Assert.AreEqual("60", timeout);
        Assert.AreEqual("3", retryCount);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public void AddTestContext_WithRunsettingsValues_BindsArrays()
    {
        // Arrange & Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        var servers = config.GetSection("TestConfig:Servers").Get<string[]>();

        // Assert
        Assert.IsNotNull(servers, "TestConfig__Servers array should be set in .runsettings");
        Assert.AreEqual(3, servers.Length);
        Assert.AreEqual("server1.integration.test", servers[0]);
        Assert.AreEqual("server2.integration.test", servers[1]);
        Assert.AreEqual("server3.integration.test", servers[2]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public void AddTestContext_WithRunsettingsValues_BindsToStrongTypes()
    {
        // Arrange & Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        var apiConfig = config.GetSection("TestConfig:Api").Get<ApiConfiguration>();

        // Assert
        Assert.IsNotNull(apiConfig, "TestConfig:Api section should bind to ApiConfiguration");
        Assert.AreEqual("https://api.integration.test", apiConfig.BaseUrl);
        Assert.AreEqual(60, apiConfig.Timeout);
        Assert.AreEqual(3, apiConfig.RetryCount);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public void AddTestContext_WithPrefixFilter_LoadsOnlyMatchingParameters()
    {
        // Arrange & Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext, prefix: "TestConfig")
            .Build();

        // Assert - With prefix filter, "TestConfig" should be removed
        var environment = config["Environment"];
        var apiBaseUrl = config["Api:BaseUrl"];

        // These should exist (after prefix removal)
        Assert.IsNotNull(environment, "Environment should exist after removing TestConfig prefix");
        Assert.IsNotNull(apiBaseUrl, "Api:BaseUrl should exist after removing TestConfig prefix");
        Assert.AreEqual("IntegrationTest", environment);
        Assert.AreEqual("https://api.integration.test", apiBaseUrl);

        // MongoDB parameters should NOT exist (different prefix)
        var mongoConnection = config["MONGODB_CONNECTION_STRING"];
        Assert.IsNull(mongoConnection, "MONGODB_CONNECTION_STRING should be filtered out by prefix");
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public void AddTestContext_WithRunsettingsValues_SupportsNestedConfiguration()
    {
        // Arrange & Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        var loggingConfig = config.GetSection("TestConfig:Logging").Get<LoggingConfiguration>();

        // Assert
        Assert.IsNotNull(loggingConfig, "TestConfig:Logging section should bind to LoggingConfiguration");
        Assert.AreEqual("Debug", loggingConfig.Level);
        Assert.AreEqual(true, loggingConfig.EnableConsole);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public void AddTestContext_OverridesOtherConfigurationSources()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string>
        {
            { "TestConfig_Environment", "FromMemory" }
        };

        // Act
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .AddTestContext(TestContext)  // Should override in-memory
            .Build();

        // Assert
        var environment = config["TestConfig_Environment"];
        Assert.AreEqual("IntegrationTest", environment,
            "TestContext should override in-memory configuration");
    }

    #region Helper Classes

    private class ApiConfiguration
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int Timeout { get; set; }
        public int RetryCount { get; set; }
    }

    private class LoggingConfiguration
    {
        public string Level { get; set; } = string.Empty;
        public bool EnableConsole { get; set; }
    }

    #endregion
}
