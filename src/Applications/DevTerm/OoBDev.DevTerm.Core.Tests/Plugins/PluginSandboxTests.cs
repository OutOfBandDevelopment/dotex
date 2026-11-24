using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.Sandbox;

namespace OoBDev.DevTerm.Tests;

/// <summary>Process plugins with a <c>permissions</c> block run through a sandbox launcher, or are refused.</summary>
[DoNotParallelize]
[TestClass]
public sealed class PluginSandboxTests
{
    public required TestContext TestContext { get; set; }

    [TestCleanup]
    public void Cleanup() => PluginSandboxes.Reset();

    [TestMethod]
    [TestCategory("Unit")]
    public void TryRead_WithPermissions_ReadsTheBlock()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        var path = Path.Combine(folder.DirectoryPath, PluginManifest.FileName);
        File.WriteAllText(path, """{ "name": "p", "process": { "command": "tool" }, "permissions": { "read": ["{folder}/data"], "write": ["{folder}/out"], "memoryMb": 64, "wallSeconds": 5 } }""");

        // Test
        var manifest = PluginManifest.TryRead(path, out var error);
        TestContext.WriteLine($"error='{error}' permissions={manifest?.Permissions is not null}");

        // Assert
        Assert.IsNotNull(manifest?.Permissions);
        Assert.AreEqual(64, manifest.Permissions.MemoryMb);
        Assert.AreEqual(5, manifest.Permissions.WallSeconds);
        CollectionAssert.AreEqual(new[] { "{folder}/data" }, manifest.Permissions.Read.ToArray());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryRead_WithoutPermissions_LeavesThemNull()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        var path = Path.Combine(folder.DirectoryPath, PluginManifest.FileName);
        File.WriteAllText(path, """{ "name": "p", "process": { "command": "tool" } }""");

        // Test
        var manifest = PluginManifest.TryRead(path, out _);

        // Assert
        Assert.IsNotNull(manifest);
        Assert.IsNull(manifest.Permissions);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ToSandbox_ExpandsFolderAndAlwaysReadsThePluginFolder()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        var permissions = new PluginPermissions { Read = ["{folder}/data"], Write = ["{folder}/out"], MemoryMb = 64, WallSeconds = 5 };

        // Test
        var sandbox = permissions.ToSandbox(new FakeLauncher(), folder.DirectoryPath);
        TestContext.WriteLine($"read={string.Join(';', sandbox.ReadDirectories)} write={string.Join(';', sandbox.WriteDirectories)}");

        // Assert
        var root = Path.GetFullPath(folder.DirectoryPath);
        Assert.Contains(root, sandbox.ReadDirectories);
        Assert.Contains(Path.GetFullPath(Path.Combine(root, "data")), sandbox.ReadDirectories);
        CollectionAssert.AreEqual(new[] { Path.GetFullPath(Path.Combine(root, "out")) }, sandbox.WriteDirectories.ToArray());
        Assert.AreEqual(64, sandbox.Limits.MemoryMb);
        Assert.AreEqual(5, sandbox.Limits.WallSeconds);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void LoadAll_PermissionsWithoutALauncher_IsRefusedNotRunPlain()
    {
        // Stage
        PluginSandboxes.Use(null);
        using var home = new TempDirectoryHandle();
        var folder = Path.Combine(home.DirectoryPath, "p");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, PluginManifest.FileName), """{ "name": "p", "version": "1", "contract": 1, "process": { "command": "tool" }, "permissions": { } }""");
        var services = new ServiceCollection();

        // Test
        var result = PluginLoader.LoadAll(home.DirectoryPath, services).Single();
        TestContext.WriteLine($"loaded={result.Loaded} message={result.Message}");

        // Assert
        Assert.IsFalse(result.Loaded);
        StringAssert.Contains(result.Message, "no sandbox launcher");
        Assert.IsEmpty(services);
        Assert.IsEmpty(LivePlugins.Pending.Where(p => p.Request.Name == "p").ToList());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task StartAsync_WithASandbox_HandshakesAndRendersThroughTheLauncher()
    {
        // Stage
        var launcher = new FakeLauncher();
        var sandbox = new PluginSandbox(launcher, ["/read"], ["/write"], new JobLimits(MemoryMb: 32));

        // Test
        await using var plugin = await ExternalProcessPresenter.StartAsync("tool", ["--x"], TimeSpan.FromSeconds(5), TestContext.CancellationToken, sandbox);
        var lines = plugin.Render(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("hi")));
        TestContext.WriteLine($"name={plugin.Name} lines={string.Join('|', lines)} started={launcher.FileName} {string.Join(' ', launcher.Arguments)}");

        // Assert
        Assert.AreEqual("fake", plugin.Name);
        CollectionAssert.AreEqual(new[] { "6869" }, lines.ToArray());
        Assert.AreEqual("tool", launcher.FileName);
        CollectionAssert.AreEqual(new[] { "--x" }, launcher.Arguments.ToArray());
        CollectionAssert.AreEqual(new[] { "/write" }, launcher.Write.ToArray());
        Assert.IsFalse(plugin.Faulted);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task StartAsync_WithASandboxThatEndsTheProgram_FailsTheHandshakeInsteadOfHanging()
    {
        // Stage
        var sandbox = new PluginSandbox(new FakeLauncher { SilentAfterStart = true }, [], [], new JobLimits(MemoryMb: 32, WallSeconds: 1));

        // Test
        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => ExternalProcessPresenter.StartAsync("tool", [], TimeSpan.FromMilliseconds(300), TestContext.CancellationToken, sandbox));
        TestContext.WriteLine(ex.Message);

        // Assert
        StringAssert.Contains(ex.Message, "failed its handshake");
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task Windows_AProgramThatWaitsPastItsWallTime_FailsTheHandshakeAndTheHostCarriesOn()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Windows launcher (AppContainer).");
        }

        // Stage
        var launcher = PluginSandboxes.Launcher ?? throw new AssertFailedException("no Windows launcher");
        var cmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
        var sandbox = new PluginSandbox(launcher, [], [], new JobLimits(MemoryMb: 256, WallSeconds: 1));

        // Test
        var started = DateTime.UtcNow;
        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => ExternalProcessPresenter.StartAsync(cmd, ["/c", "pause"], TimeSpan.FromSeconds(10), TestContext.CancellationToken, sandbox));
        var seconds = (DateTime.UtcNow - started).TotalSeconds;
        TestContext.WriteLine($"{ex.Message} after {seconds:0.0}s");

        // Assert
        Assert.IsLessThan(10, seconds);
    }

    private sealed class FakeLauncher : ISandboxCommandLauncher
    {
        public string Mechanism => "fake";

        public bool IsAvailable => true;

        public bool SilentAfterStart { get; init; }

        public string FileName { get; private set; } = string.Empty;

        public IReadOnlyList<string> Arguments { get; private set; } = [];

        public IReadOnlyCollection<string> Write { get; private set; } = [];

        public ISandboxedProcess StartCommand(string fileName, IReadOnlyList<string> arguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null, JobLimits? limits = null)
        {
            FileName = fileName;
            Arguments = arguments;
            Write = writeDirectories ?? [];
            return new FakeProcess(SilentAfterStart);
        }
    }

    /// <summary>Answers hello and render lines like a well-behaved plugin, or closes its output at once.</summary>
    private sealed class FakeProcess : ISandboxedProcess
    {
        private readonly AnonymousPipeServerStream _replies = new(PipeDirection.Out);
        private readonly AnonymousPipeClientStream _output;

        public FakeProcess(bool silent)
        {
            _output = new AnonymousPipeClientStream(PipeDirection.In, _replies.ClientSafePipeHandle);
            Input = new ResponderStream(_replies);
            if (silent)
            {
                _replies.Dispose();
            }
        }

        public Stream Input { get; }

        public Stream Output => _output;

        public bool HasExited { get; private set; }

        public int ExitCode => 0;

        public LimitHit LimitHit => LimitHit.None;

        public void Kill() => HasExited = true;

        public bool WaitForExit(int milliseconds) => HasExited;

        public void Dispose()
        {
            Kill();
            _output.Dispose();
            _replies.Dispose();
        }
    }

    private sealed class ResponderStream(Stream replies) : Stream
    {
        private readonly StringBuilder _line = new();

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            foreach (var c in Encoding.UTF8.GetString(buffer, offset, count))
            {
                if (c != '\n')
                {
                    _line.Append(c);
                    continue;
                }

                var request = _line.ToString();
                _line.Clear();
                var reply = request.Contains("\"hello\"", StringComparison.Ordinal)
                    ? """{"type":"hello","name":"fake","protocol":1}"""
                    : """{"type":"output","lines":["6869"]}""";
                var bytes = Encoding.UTF8.GetBytes(reply + "\n");
                replies.Write(bytes, 0, bytes.Length);
                replies.Flush();
            }
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
