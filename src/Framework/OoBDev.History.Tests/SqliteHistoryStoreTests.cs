using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.History.Sqlite;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.History.Tests;

[TestClass]
public class SqliteHistoryStoreTests : HistoryStoreContract
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "history-" + Guid.NewGuid().ToString("N"));
    private SqliteHistoryStore? _store;

    [TestInitialize]
    public void Init() => Directory.CreateDirectory(_directory);

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_store is not null)
        {
            await _store.DisposeAsync();
        }

        SqliteConnection.ClearAllPools();
        Directory.Delete(_directory, recursive: true);
    }

    protected override async Task<IHistoryStore> CreateAsync(HistoryOptions options)
    {
        _store = await SqliteHistoryStore.OpenAsync($"Data Source={Path.Combine(_directory, "h.db")};Pooling=False", options);
        return _store;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Snapshot_CopiesRunningDatabase_AndVerifies()
    {
        var options = new HistoryOptions();
        var source = Path.Combine(_directory, "live.db");
        await using var store = await SqliteHistoryStore.OpenAsync($"Data Source={source};Pooling=False", options);
        await store.AppendAsync("a", new HistoryPoint(T0, 3));
        var snapshot = Path.Combine(_directory, "snap.db");
        await using (var connection = new SqliteConnection($"Data Source={source};Pooling=False"))
        {
            await connection.OpenAsync();
            await SqliteSnapshot.CreateAsync(connection, snapshot);
            await Assert.ThrowsExactlyAsync<IOException>(() => SqliteSnapshot.CreateAsync(connection, snapshot));
        }

        Assert.IsTrue(await SqliteSnapshot.VerifyAsync(snapshot));
        await using var copy = await SqliteHistoryStore.OpenAsync($"Data Source={snapshot};Pooling=False", options);
        Assert.AreEqual(3d, (await copy.ReadRawAsync("a", T0, T0.AddSeconds(1))).Single().Value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Verify_CorruptFile_IsNotOk()
    {
        var path = Path.Combine(_directory, "bad.db");
        await File.WriteAllBytesAsync(path, Enumerable.Repeat((byte)0x41, 4096).ToArray());

        var ok = false;
        try
        {
            ok = await SqliteSnapshot.VerifyAsync(path);
        }
        catch (SqliteException)
        {
        }

        Assert.IsFalse(ok);
    }
}
