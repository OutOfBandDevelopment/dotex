using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Data.Vectors.Tests;

/// <summary>
/// Deploys the OoBDev.Data.Vectors.DB dacpac (with the SQL CLR vector types merged in) to the Docker SQL Server
/// and runs the vector functions inside the database.
/// </summary>
/// <remarks>
/// Needs the container from containers/testing (SQLSERVER_CONNECTION_STRING). The test enables CLR and turns off
/// "clr strict security" for its run and restores both afterwards, so only point it at a disposable server.
/// </remarks>
[TestClass]
public class SqlServerDeploymentTests
{
    public required TestContext TestContext { get; set; }

    private string _masterConnection = default!;
    private string _databaseName = default!;
    private int _originalClrEnabled;
    private int _originalClrStrict;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _masterConnection = TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING");
        _databaseName = $"VectorDeploy_{Guid.NewGuid():N}";

        _originalClrEnabled = await ScalarAsync<int>(_masterConnection, "SELECT CAST(value_in_use AS int) FROM sys.configurations WHERE name = 'clr enabled'");
        _originalClrStrict = await ScalarAsync<int>(_masterConnection, "SELECT CAST(value_in_use AS int) FROM sys.configurations WHERE name = 'clr strict security'");

        await ExecuteAsync(_masterConnection, $@"
            EXEC sp_configure 'show advanced options', 1; RECONFIGURE;
            EXEC sp_configure 'clr enabled', 1;
            EXEC sp_configure 'clr strict security', 0; RECONFIGURE;
            CREATE DATABASE [{_databaseName}];");

        var dacpac = FindDacpac();
        var services = new DacServices(_masterConnection);
        using var package = DacPackage.Load(dacpac);
        services.Deploy(package, _databaseName, upgradeExisting: true, new DacDeployOptions { IncludeCompositeObjects = true });
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        if (_masterConnection is null) return;

        SqlConnection.ClearAllPools();
        await ExecuteAsync(_masterConnection, $@"
            IF DB_ID('{_databaseName}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{_databaseName}];
            END
            EXEC sp_configure 'clr strict security', {_originalClrStrict};
            EXEC sp_configure 'clr enabled', {_originalClrEnabled}; RECONFIGURE;");
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Distance_And_Angle_RunInsideSqlServer()
    {
        var db = DatabaseConnection();
        const string vectors = "DECLARE @a embedding.VectorF = embedding.VectorF::Parse('1,0,0'); DECLARE @b embedding.VectorF = embedding.VectorF::Parse('0.6,0.8,0');";

        Assert.AreEqual(0.8944272, await ScalarAsync<double>(db, vectors + "SELECT CAST(@a.Distance(@b, 'euclidean') AS float)"), 1e-5);
        Assert.AreEqual(0.4, await ScalarAsync<double>(db, vectors + "SELECT CAST(@a.Distance(@b, 'cosine') AS float)"), 1e-5);
        Assert.AreEqual(Math.Acos(0.6), await ScalarAsync<double>(db, vectors + "SELECT CAST(@a.Angle(@b) AS float)"), 1e-5);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task UndefinedInputs_ReturnNull_WithoutFailingTheBatch()
    {
        const string sql = @"
            DECLARE @a embedding.VectorF = embedding.VectorF::Parse('1,0,0');
            DECLARE @short embedding.VectorF = embedding.VectorF::Parse('1,0');
            DECLARE @zero embedding.VectorF = embedding.VectorF::Parse('0,0,0');
            SELECT @a.Angle(@short), @a.Angle(@zero), @a.Distance(@short, 'euclidean'), @a.Distance(@a, 'not_a_metric'), @a.Angle(@a)";

        // one batch with four undefined results and no exception
        await using var connection = new SqlConnection(DatabaseConnection());
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        Assert.IsTrue(await reader.ReadAsync());
        Assert.IsTrue(reader.IsDBNull(0), "angle, different lengths");
        Assert.IsTrue(reader.IsDBNull(1), "angle, zero vector");
        Assert.IsTrue(reader.IsDBNull(2), "distance, different lengths");
        Assert.IsTrue(reader.IsDBNull(3), "distance, unsupported metric");
        Assert.AreEqual(0d, Convert.ToDouble(reader.GetValue(4)), 1e-6, "identical vectors");
    }

    private string DatabaseConnection() =>
        new SqlConnectionStringBuilder(_masterConnection) { InitialCatalog = _databaseName }.ConnectionString;

    private string FindDacpac()
    {
        var configured = TestContext.GetPropertyOrDefault("VECTORS_DB_DACPAC", "");
        if (!string.IsNullOrWhiteSpace(configured)) return configured;

        // <src>/Extensions/OoBDev.Data.Vectors.Tests/bin/<config>/net10.0/ -> <src>/Extensions/OoBDev.Data.Vectors.DB/bin/<config>/netstandard2.1/
        var testDir = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        var configuration = testDir.Parent!.Name;
        var extensions = testDir.Parent.Parent!.Parent!.Parent!.FullName;
        var path = Path.Combine(extensions, "OoBDev.Data.Vectors.DB", "bin", configuration, "netstandard2.1", "OoBDev.Data.Vectors.DB.dacpac");
        Assert.IsTrue(File.Exists(path), $"Dacpac not found: {path}. Build OoBDev.Data.Vectors.DB or set VECTORS_DB_DACPAC.");
        return path;
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection) { CommandTimeout = 120 };
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<T> ScalarAsync<T>(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        return (T)Convert.ChangeType((await command.ExecuteScalarAsync())!, typeof(T));
    }
}
