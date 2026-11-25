using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public sealed class SqldefInstallerTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Ensure_WithMatchingHash_ExtractsOnceAndCaches()
    {
        // Stage
        var zip = Zip("psqldef.exe", "binary");
        var handler = new CountingHandler(zip);
        using var folder = new TempDirectoryHandle();
        var installer = Installer(handler, folder.DirectoryPath, Hash(zip));

        // Test
        var first = await installer.EnsureAsync("psqldef", TestContext.CancellationToken);
        var second = await installer.EnsureAsync("psqldef", TestContext.CancellationToken);
        TestContext.WriteLine($"path={first} requests={handler.Requests}");

        // Assert
        Assert.AreEqual(first, second);
        Assert.AreEqual("binary", await File.ReadAllTextAsync(first, TestContext.CancellationToken));
        Assert.AreEqual(1, handler.Requests);
        Assert.IsFalse(File.Exists(first + ".part"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Ensure_WithWrongHash_ThrowsAndExtractsNothing()
    {
        // Stage
        var handler = new CountingHandler(Zip("psqldef.exe", "tampered"));
        using var folder = new TempDirectoryHandle();
        var installer = Installer(handler, folder.DirectoryPath, new string('0', 64));

        // Test
        var error = await Assert.ThrowsExactlyAsync<InvalidDataException>(() => installer.EnsureAsync("psqldef", TestContext.CancellationToken));
        TestContext.WriteLine(error.Message);

        // Assert
        Assert.IsEmpty(Directory.GetFiles(folder.DirectoryPath, "*", SearchOption.AllDirectories));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Ensure_ForAnUnpinnedTool_Throws()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        var installer = Installer(new CountingHandler([]), folder.DirectoryPath, "00");

        // Test and Assert
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => installer.EnsureAsync("mysqldef", TestContext.CancellationToken));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void PinnedHashes_CoverEveryToolOnTheCurrentPlatform()
    {
        // Test
        var platform = SqldefInstaller.CurrentPlatform();
        TestContext.WriteLine($"platform={platform} version={SqldefInstaller.Version}");

        // Assert
        StringAssert.Matches(platform, new global::System.Text.RegularExpressions.Regex("^(windows|linux|darwin)_(amd64|arm64)$"));
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task Ensure_RealRelease_DownloadsVerifiesAndRuns()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        using var http = new HttpClient();
        var installer = new SqldefInstaller(http, folder.DirectoryPath);

        // Test
        var path = await installer.EnsureAsync("sqlite3def", TestContext.CancellationToken);
        var result = await new ProcessToolRunner().RunAsync(path, ["--version"], string.Empty, new Dictionary<string, string>(), TestContext.CancellationToken);
        TestContext.WriteLine($"path={path} exit={result.ExitCode} out={result.StandardOutput.Trim()}");

        // Assert
        Assert.AreEqual(0, result.ExitCode);
        StringAssert.Contains(result.StandardOutput + result.StandardError, SqldefInstaller.Version);
    }

    private static SqldefInstaller Installer(HttpMessageHandler handler, string cache, string hash) =>
        new(new HttpClient(handler), cache, "windows_amd64", new Dictionary<string, string> { ["psqldef_windows_amd64.zip"] = hash }, "https://example.test/sqldef");

    private static byte[] Zip(string name, string content)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = zip.CreateEntry(name);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(content);
        }

        return stream.ToArray();
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private sealed class CountingHandler(byte[] body) : HttpMessageHandler
    {
        public int Requests { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(body) });
        }
    }
}
