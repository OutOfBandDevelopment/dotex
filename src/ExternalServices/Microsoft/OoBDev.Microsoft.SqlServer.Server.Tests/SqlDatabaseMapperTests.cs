using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.ComponentModel;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.TestUtilities;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace OoBDev.Microsoft.SqlServer.Server.Tests;

[TestClass]
public class SqlDatabaseMapperTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetCommandParametersTest()
    {
        var mock = new MockRepository(MockBehavior.Strict);
        var mockConfiguration = mock.Create<IConfiguration>();
        var mockJsonSerializer = mock.Create<IJsonSerializer>();

        var mapper = new SqlDatabaseMapper(mockConfiguration.Object, mockJsonSerializer.Object);

        var parameters = mapper.GetCommandParameters(new TestTarget()
        {
            Name = "test",
        }).ToArray();

        Assert.AreEqual(2, parameters.Length);
        Assert.AreEqual("@Name", parameters[0].ParameterName);
        Assert.AreEqual("@Props", parameters[1].ParameterName);
        Assert.AreEqual("test", parameters[0].Value);
        Assert.AreEqual(DBNull.Value, parameters[1].Value);

        mock.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetConnectionStringTest()
    {
        var fakeConnectionString = "connection string";

        var mock = new MockRepository(MockBehavior.Strict);
        var mockConfiguration = mock.Create<IConfiguration>();
        var mockJsonSerializer = mock.Create<IJsonSerializer>();

        var mockConfigurationSection = mock.Create<IConfigurationSection>();
        mockConfiguration.Setup(s => s.GetSection("ConnectionStrings"))
                         .Returns(mockConfigurationSection.Object);

        mockConfigurationSection.SetupGet(c => c["TestConnection"]).Returns(fakeConnectionString);
        var mapper = new SqlDatabaseMapper(mockConfiguration.Object, mockJsonSerializer.Object);

        var connectionString = mapper.GetConnectionString<TestTarget>();

        Assert.AreEqual(fakeConnectionString, connectionString);

        mock.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetConnectionTest()
    {
        var fakeConnectionString = "Database=value;User=other";

        var mock = new MockRepository(MockBehavior.Strict);
        var mockConfiguration = mock.Create<IConfiguration>();
        var mockJsonSerializer = mock.Create<IJsonSerializer>();

        var mockConfigurationSection = mock.Create<IConfigurationSection>();
        mockConfiguration.Setup(s => s.GetSection("ConnectionStrings"))
                         .Returns(mockConfigurationSection.Object);

        mockConfigurationSection.SetupGet(c => c["TestConnection"]).Returns(fakeConnectionString);
        var mapper = new SqlDatabaseMapper(mockConfiguration.Object, mockJsonSerializer.Object);

        var connection = mapper.GetConnection<TestTarget>();

        Assert.AreEqual(fakeConnectionString, connection.ConnectionString);

        mock.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetStoredProcedureNameTest()
    {
        var mock = new MockRepository(MockBehavior.Strict);
        var mockConfiguration = mock.Create<IConfiguration>();
        var mockJsonSerializer = mock.Create<IJsonSerializer>();

        var mapper = new SqlDatabaseMapper(mockConfiguration.Object, mockJsonSerializer.Object);

        var procedure = mapper.GetStoredProcedureName<TestTarget>(); ;

        Assert.AreEqual("test.name", procedure);

        mock.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetReaderMapperTest()
    {
        var mock = new MockRepository(MockBehavior.Strict);
        var mockConfiguration = mock.Create<IConfiguration>();
        var mockJsonSerializer = mock.Create<IJsonSerializer>();

        var mapper = new SqlDatabaseMapper(mockConfiguration.Object, mockJsonSerializer.Object);

        var mockDbDataReader = mock.Create<DbDataReader>();

        var schemaTable = new DataTable();
        schemaTable.Columns.Add("ColumnName", typeof(string));
        schemaTable.Columns.Add("ColumnOrdinal", typeof(int));

        var row = schemaTable.NewRow();
        row.ItemArray = [nameof(TestTarget.Name), 0];
        schemaTable.Rows.Add(row);
        row = schemaTable.NewRow();
        row.ItemArray = [nameof(TestTarget.Ignored), 1];
        schemaTable.Rows.Add(row);

        mockDbDataReader.Setup(s => s.GetSchemaTable()).Returns(schemaTable);
        mockDbDataReader.Setup(s => s[0]).Returns("name value");
        mockDbDataReader.Setup(s => s[1]).Returns("ignored value");

        var readerMapper = mapper.GetReaderMapper<TestTarget>(mockDbDataReader.Object);

        var mapped = readerMapper(mockDbDataReader.Object);

        Assert.AreEqual("name value", mapped.Name);
        Assert.AreEqual("ignored value", mapped.Ignored);
        Assert.IsNull(mapped.Props);

        mock.VerifyAll();
    }

    [StoredProcedure("test.name")]
    [ConnectionStringName("TestConnection")]
    public class TestTarget
    {
        [QueryParameter]
        public string? Name { get; set; }

        [QueryParameter]
        public string[]? Props { get; set; }

        public string? Ignored { get; set; }
    }
}
