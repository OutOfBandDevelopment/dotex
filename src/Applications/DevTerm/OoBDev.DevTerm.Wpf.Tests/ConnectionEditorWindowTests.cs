using System;
using OoBDev.System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The connection editor window keeps its profile list current.</summary>
[DoNotParallelize]
[TestCategory("Unit")]
[TestClass]
public sealed class ConnectionEditorWindowTests
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
    public void Window_ProfileSavedByAnotherStore_ListsItWithoutPressingRefresh()
    {
        // Stage
        var listed = false;
        RunOnSta(() =>
        {
            using var editor = new ConnectionEditorViewModel(new ConnectionProfileStore(_dir), new CliOptions { Transport = "tcp" });
            var window = new ConnectionEditorWindow(editor);

            // Test
            new ConnectionProfileStore(_dir).Save("added-elsewhere", new CliOptions { Transport = "tcp", Host = "1.1.1.1", Port = "1" });
            var clock = Stopwatch.StartNew();
            while (clock.Elapsed < TimeSpan.FromSeconds(5) && !listed)
            {
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
                listed = editor.Profiles.Contains("added-elsewhere");
                Thread.Sleep(20);
            }

            window.Close();
        });

        // Assert
        Assert.IsTrue(listed);
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
