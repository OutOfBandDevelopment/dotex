using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace OoBDev.TestUtilities.Tests;

/// <summary>
/// Unit tests for TestContext configuration provider.
/// </summary>
[TestClass]
public class TestContextConfigurationProviderTests
{
    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the current test run.
    /// </summary>
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IConfigurationBuilder? builder = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder!.AddTestContext(TestContext));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_WithNullTestContext_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.AddTestContext(null!));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_WithFlatValues_ReadsCorrectly()
    {
        // Arrange
        TestContext.Properties["Environment"] = "Integration";
        TestContext.Properties["ServerName"] = "localhost";

        // Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        // Assert
        Assert.AreEqual("Integration", config["Environment"]);
        Assert.AreEqual("localhost", config["ServerName"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_WithColonHierarchy_ReadsCorrectly()
    {
        // Arrange
        TestContext.Properties["Database:Server"] = "localhost";
        TestContext.Properties["Database:Port"] = "5432";

        // Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        // Assert
        Assert.AreEqual("localhost", config["Database:Server"]);
        Assert.AreEqual("5432", config["Database:Port"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_WithDoubleUnderscoreHierarchy_NormalizesToColons()
    {
        // Arrange
        TestContext.Properties["Database__Server"] = "localhost";
        TestContext.Properties["Database__Port"] = "5432";
        TestContext.Properties["Api__BaseUrl"] = "https://api.test.com";

        // Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        // Assert
        Assert.AreEqual("localhost", config["Database:Server"]);
        Assert.AreEqual("5432", config["Database:Port"]);
        Assert.AreEqual("https://api.test.com", config["Api:BaseUrl"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_WithArrays_BindsCorrectly()
    {
        // Arrange
        TestContext.Properties["Servers__0"] = "server1.test.local";
        TestContext.Properties["Servers__1"] = "server2.test.local";
        TestContext.Properties["Servers__2"] = "server3.test.local";

        // Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        var servers = config.GetSection("Servers").Get<string[]>();

        // Assert
        Assert.IsNotNull(servers);
        Assert.HasCount(3, servers);
        Assert.AreEqual("server1.test.local", servers[0]);
        Assert.AreEqual("server2.test.local", servers[1]);
        Assert.AreEqual("server3.test.local", servers[2]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_WithPrefix_FiltersAndRemovesPrefix()
    {
        // Arrange
        TestContext.Properties["MyApp:Database:Server"] = "localhost";
        TestContext.Properties["MyApp:Database:Port"] = "5432";
        TestContext.Properties["OtherApp:Setting"] = "ignored";

        // Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext, prefix: "MyApp")
            .Build();

        // Assert
        Assert.AreEqual("localhost", config["Database:Server"]);
        Assert.AreEqual("5432", config["Database:Port"]);
        Assert.IsNull(config["OtherApp:Setting"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_WithPrefixAndDoubleUnderscore_FiltersAndNormalizes()
    {
        // Arrange
        TestContext.Properties["MyApp__Database__Server"] = "localhost";
        TestContext.Properties["MyApp__Database__Port"] = "5432";
        TestContext.Properties["OtherApp__Setting"] = "ignored";

        // Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext, prefix: "MyApp")
            .Build();

        // Assert
        Assert.AreEqual("localhost", config["Database:Server"]);
        Assert.AreEqual("5432", config["Database:Port"]);
        Assert.IsNull(config["OtherApp:Setting"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_WithStrongTypedBinding_BindsCorrectly()
    {
        // Arrange
        TestContext.Properties["Database__Server"] = "localhost";
        TestContext.Properties["Database__Port"] = "5432";
        TestContext.Properties["Database__Username"] = "testuser";

        // Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        var dbConfig = config.GetSection("Database").Get<DatabaseConfig>();

        // Assert
        Assert.IsNotNull(dbConfig);
        Assert.AreEqual("localhost", dbConfig.Server);
        Assert.AreEqual(5432, dbConfig.Port);
        Assert.AreEqual("testuser", dbConfig.Username);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_IsCaseInsensitive()
    {
        // Arrange
        TestContext.Properties["database__SERVER"] = "localhost";

        // Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        // Assert
        Assert.AreEqual("localhost", config["Database:Server"]);
        Assert.AreEqual("localhost", config["database:server"]);
        Assert.AreEqual("localhost", config["DATABASE:SERVER"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_OverridesEarlierSources()
    {
        // Arrange
        TestContext.Properties["Setting"] = "FromTestContext";

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Setting", "FromMemory" }
        };

        // Act
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .AddTestContext(TestContext)  // Should override in-memory
            .Build();

        // Assert
        Assert.AreEqual("FromTestContext", config["Setting"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddTestContext_WithEmptyKeys_IgnoresThem()
    {
        // Arrange
        TestContext.Properties["ValidKey"] = "value";
        TestContext.Properties[""] = "ignored";

        // Act
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        // Assert
        Assert.AreEqual("value", config["ValidKey"]);
    }

    #region Helper Classes

    private class DatabaseConfig
    {
        public string Server { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    #endregion
}
