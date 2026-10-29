using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Data.Vectors.DB.Tests;

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
    public async Task SqlVectorStore_StoreAndFindNeighbors_RanksByCosineAndKeepsMetadata()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new OoBDev.Data.Vectors.SqlStore.SqlVectorStoreOptions { ConnectionString = DatabaseConnection(), Limit = 10 });
        var store = new OoBDev.Data.Vectors.SqlStore.SqlVectorStoreProvider(options, "notes");
        var common = new global::System.Collections.Generic.Dictionary<string, object> { ["kind"] = "note" };

        var ids = await store.StoreVectorsAsync(
        [
            (new float[] { 1, 0, 0 }, new global::System.Collections.Generic.Dictionary<string, object> { ["title"] = "x axis" }),
            (new float[] { 0, 1, 0 }, new global::System.Collections.Generic.Dictionary<string, object> { ["title"] = "y axis" }),
            (new float[] { 0.9f, 0.1f, 0 }, new global::System.Collections.Generic.Dictionary<string, object> { ["title"] = "near x" }),
        ], common);

        Assert.AreEqual(3, ids.Length);
        var hits = new global::System.Collections.Generic.List<OoBDev.Search.Models.SearchResultModel>();
        await foreach (var hit in store.FindNeighborsAsync(new float[] { 1, 0, 0 })) hits.Add(hit);
        Assert.AreEqual(3, hits.Count);
        Assert.AreEqual("x axis", hits[0].MetaData!["title"]);
        Assert.AreEqual("near x", hits[1].MetaData!["title"]);
        Assert.AreEqual("note", hits[0].MetaData!["kind"]);
        Assert.IsTrue(hits[0].Score <= hits[1].Score);

        var listed = 0;
        await foreach (var _ in store.ListAsync()) listed++;
        Assert.AreEqual(3, listed);
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
    public async Task Aggregates_RunInsideSqlServer()
    {
        var db = DatabaseConnection();
        const string rows = "FROM (VALUES (embedding.VectorF::Parse('1,8,3')), (embedding.VectorF::Parse('4,2,9')), (embedding.VectorF::Parse('2,5,6'))) AS t(v)";

        Assert.AreEqual(1d, await ScalarAsync<double>(db, $"SELECT CAST(embedding.MinimumF(v).Element(0) AS float) {rows}"), 1e-5);
        Assert.AreEqual(2d, await ScalarAsync<double>(db, $"SELECT CAST(embedding.MinimumF(v).Element(1) AS float) {rows}"), 1e-5);
        Assert.AreEqual(4d, await ScalarAsync<double>(db, $"SELECT CAST(embedding.MaximumF(v).Element(0) AS float) {rows}"), 1e-5);
        Assert.AreEqual(9d, await ScalarAsync<double>(db, $"SELECT CAST(embedding.MaximumF(v).Element(2) AS float) {rows}"), 1e-5);
        Assert.AreEqual(5d, await ScalarAsync<double>(db, $"SELECT CAST(embedding.CentroidF(v).Element(1) AS float) {rows}"), 1e-5);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task MatrixAndDoubleVector_RunInsideSqlServer()
    {
        var db = DatabaseConnection();
        const string matrix = "DECLARE @m embedding.MatrixF = embedding.MatrixF::Parse('1,2,3|4,5,6'); DECLARE @md embedding.Matrix = embedding.Matrix::Parse('1,2,3|4,5,6');";

        Assert.AreEqual(5d, await ScalarAsync<double>(db, matrix + "SELECT CAST(@m.Row(1).Element(1) AS float)"), 1e-5);
        Assert.AreEqual(6d, await ScalarAsync<double>(db, matrix + "SELECT CAST(@m.[Column](2).Element(1) AS float)"), 1e-5);
        Assert.AreEqual(4d, await ScalarAsync<double>(db, matrix + "SELECT CAST(@md.Row(1).Element(0) AS float)"), 1e-9);
        Assert.IsTrue(await ScalarAsync<bool>(db, matrix + "SELECT CASE WHEN @m.Row(5) IS NULL THEN 1 ELSE 0 END"), "row out of range is NULL");

        const string vectors = "DECLARE @a embedding.Vector = embedding.Vector::Parse('3,4'); DECLARE @b embedding.Vector = embedding.Vector::Parse('0,0');";
        Assert.AreEqual(5d, await ScalarAsync<double>(db, vectors + "SELECT CAST(@a.Magnitude() AS float)"), 1e-9);
        Assert.AreEqual(5d, await ScalarAsync<double>(db, vectors + "SELECT CAST(@a.Distance(@b, 'euclidean') AS float)"), 1e-9);
        Assert.AreEqual(2, await ScalarAsync<int>(db, vectors + "SELECT @a.Length()"));
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
