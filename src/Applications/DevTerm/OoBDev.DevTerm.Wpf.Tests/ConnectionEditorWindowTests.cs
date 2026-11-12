using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

    [TestMethod]
    public void Window_ShowsOnePresenterCheckBoxPerPresenter_AndTheSendFormatAndLineEndingChoices()
    {
        // Stage
        var checkBoxes = 0;
        var combos = 0;
        var expected = 0;
        RunOnSta(() =>
        {
            using var editor = new ConnectionEditorViewModel(new ConnectionProfileStore(_dir), new CliOptions { Transport = "tcp" });
            expected = editor.PresenterChoices.Count;
            var window = new ConnectionEditorWindow(editor);

            // Test
            WpfScreenshot.ShowOffScreen(window, 900, 700);
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
            window.UpdateLayout();
            checkBoxes = Descendants<CheckBox>(window).Count(c => editor.PresenterChoices.Any(p => ReferenceEquals(c.DataContext, p)));
            combos = Descendants<ComboBox>(window).Count(c => ReferenceEquals(c.ItemsSource, editor.PresenterOptions) || ReferenceEquals(c.ItemsSource, editor.LineEndingOptions));
            window.Close();
        });

        // Assert
        Assert.IsTrue(expected > 0);
        Assert.AreEqual(expected, checkBoxes);
        Assert.AreEqual(2, combos);
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var nested in Descendants<T>(child))
            {
                yield return nested;
            }
        }
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
