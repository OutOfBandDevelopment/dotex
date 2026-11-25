using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Security.OAuthClient.Windows.Tests;

[TestClass]
public sealed class DpapiSecretStoreTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory("Unit")]
    public async Task WriteRead_RoundTripsAndStoresCipherText()
    {
        var folder = Path.Combine(Path.GetTempPath(), "dpapi-" + Guid.NewGuid().ToString("N"));
        try
        {
            // Stage
            var store = new DpapiSecretStore(folder);
            TestContext.WriteLine($"Folder {folder}");

            // Test
            Assert.IsNull(await store.ReadAsync("t"));
            await store.WriteAsync("t", "refresh-secret");
            await store.WriteAsync("t", "refresh-secret-2");

            // Assert
            Assert.AreEqual("refresh-secret-2", await store.ReadAsync("t"));
            Assert.IsFalse((await File.ReadAllTextAsync(Path.Combine(folder, "t.dpapi"))).Contains("refresh-secret", StringComparison.Ordinal));
            await store.DeleteAsync("t");
            await store.DeleteAsync("t");
            Assert.IsNull(await store.ReadAsync("t"));
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => store.ReadAsync("../x"));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Read_CorruptFile_ReturnsNull()
    {
        var folder = Path.Combine(Path.GetTempPath(), "dpapi-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(folder);
            await File.WriteAllBytesAsync(Path.Combine(folder, "t.dpapi"), [1, 2, 3]);
            Assert.IsNull(await new DpapiSecretStore(folder).ReadAsync("t"));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }
    }
}
