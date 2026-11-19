using System;
using OoBDev.System.IO;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Captures the connection editor window for the user guide.</summary>
[DoNotParallelize]
[TestCategory("DevLocal")]
[TestClass]
public sealed class ConnectionEditorScreenshotTests
{
    private TempDirectoryHandle _folder = null!;
    private string _dir = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _folder = new TempDirectoryHandle();
        _dir = _folder.DirectoryPath;
    }

    [TestCleanup]
    public void Cleanup() => _folder.Dispose();

    [TestMethod]
    [DataRow("tcp", "wpf-device-profiles-tcp.png")]
    [DataRow("serial", "wpf-device-profiles-serial.png")]
    [DataRow("loopback", "wpf-device-profiles-loopback.png")]
    public void Window_Transport_IsCaptured(string transport, string imageName)
    {
        // Stage
        var path = Path.Combine(RepoRoot(), "docs", "applications", "devterm", "user-guide", "images", imageName);

        // Test
        RunOnSta(() =>
        {
            using var editor = new ConnectionEditorViewModel(new ConnectionProfileStore(_dir), new CliOptions { Transport = transport });
            var window = new ConnectionEditorWindow(editor);
            WpfScreenshot.ShowOffScreen(window, 900, 700);
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
            window.UpdateLayout();
            WpfScreenshot.Save(window, path);
            window.Close();
        });

        // Assert
        Assert.IsTrue(File.Exists(path));
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GitVersion.yml")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
        {
            throw new AssertFailedException(failure.Message, failure);
        }
    }
}
