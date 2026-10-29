using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.TestUtilities.Tests;

[TestClass]
public class TestInstanceGuardTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("Server=127.0.0.1,1433;User Id=sa;Password=x;")]
    [DataRow("Data Source=localhost\\SQLEXPRESS;Integrated Security=true")]
    [DataRow("Server=tcp:localhost,1433;Database=t")]
    [DataRow("localhost:6379", false)]
    [DataRow("mongodb://localhost:27017")]
    [DataRow("http://127.0.0.1:11435")]
    [DataRow("Host=host.docker.internal;Port=5432")]
    public void IsTestInstance_LocalHost_ReturnsTrue(string connectionString, bool pairs = true)
    {
        // Test
        var result = TestInstanceGuard.IsTestInstance(pairs ? connectionString : "Endpoint=" + connectionString);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("Server=prod-sql.contoso.com,1433;Database=app")]
    [DataRow("Server=127.0.0.1;Data Source=prod.example.com")]
    [DataRow("https://api.example.com/v1")]
    [DataRow("Database=app;User Id=sa")]
    public void IsTestInstance_RemoteOrUnknownHost_ReturnsFalse(string connectionString)
    {
        // Test
        var result = TestInstanceGuard.IsTestInstance(connectionString);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Require_RemoteHost_FailsTheTest()
    {
        // Test
        var error = Assert.ThrowsExactly<AssertFailedException>(() => TestInstanceGuard.Require("Server=prod.example.com"));

        // Assert
        StringAssert.Contains(error.Message, "Refusing to run", global::System.StringComparison.Ordinal);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Require_LocalHost_ReturnsTheConnectionString()
    {
        // Test
        var result = TestInstanceGuard.Require("Server=127.0.0.1,1433");

        // Assert
        Assert.AreEqual("Server=127.0.0.1,1433", result);
    }
}
