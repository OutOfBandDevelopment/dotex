using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Logging;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The playback window opened from the shell.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class PlaybackDialogTests
{
    private sealed class FakePlaybackDialog(string? path, Action<PlaybackDialogViewModel> interact) : IPlaybackDialog
    {
        public string? PickLog() => path;

        public void Show(PlaybackDialogViewModel playback) => interact(playback);
    }

    [TestMethod]
    public void OpenPlaybackWindow_LogChosen_SkipToEndListsEveryRecordAndRewindClearsThem()
    {
        // Stage
        var directory = Path.Combine(Path.GetTempPath(), $"devterm-wpf-playback-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "capture.jsonl");
            var t0 = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
            new SessionLog(
                new SessionLogHeader { Created = t0, Connection = "loopback", Presenters = ["ascii"] },
                [.. Enumerable.Range(0, 3).Select(i => new SessionLogRecord
                {
                    Kind = SessionLogRecordKind.Rx,
                    Sequence = i + 1,
                    Timestamp = t0.AddSeconds(i),
                    Data = Encoding.ASCII.GetBytes($"line{i}\r"),
                })]).Save(path);
            var afterSkip = 0;
            var afterRewind = -1;
            var services = new ServiceCollection();
            services.AddSingleton<IPlaybackDialog>(new FakePlaybackDialog(path, playback =>
            {
                playback.SkipToEnd.Execute(null);
                afterSkip = playback.Lines.Count;
                playback.Rewind.Execute(null);
                afterRewind = playback.Lines.Count;
            }));
            using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
            var shell = provider.GetRequiredService<ShellViewModel>();

            // Test
            shell.OpenPlaybackWindow.Execute(null);

            // Assert
            Assert.AreEqual(3, afterSkip);
            Assert.AreEqual(0, afterRewind);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void Jump_UnreadableTextThenRecordNumber_ReportsThenListsUpToThatRecord()
    {
        // Stage
        var directory = Path.Combine(Path.GetTempPath(), $"devterm-wpf-jump-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "capture.jsonl");
            var t0 = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
            new SessionLog(
                new SessionLogHeader { Created = t0, Connection = "loopback", Presenters = ["ascii"] },
                [.. Enumerable.Range(0, 4).Select(i => new SessionLogRecord
                {
                    Kind = SessionLogRecordKind.Rx,
                    Sequence = i + 1,
                    Timestamp = t0.AddSeconds(i),
                    Data = Encoding.ASCII.GetBytes($"line{i}\r"),
                })]).Save(path);
            string? status = null;
            var lines = -1;
            var services = new ServiceCollection();
            services.AddSingleton<IPlaybackDialog>(new FakePlaybackDialog(path, playback =>
            {
                playback.JumpText = "nonsense";
                playback.Jump.Execute(null);
                status = playback.StatusText;
                playback.JumpText = "2";
                playback.Jump.Execute(null);
                lines = playback.Lines.Count;
            }));
            using var provider = services.AddDevTermWpfShell().BuildServiceProvider();

            // Test
            provider.GetRequiredService<ShellViewModel>().OpenPlaybackWindow.Execute(null);

            // Assert
            Assert.IsNotNull(status);
            Assert.AreEqual(2, lines);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void OpenPlaybackWindow_PickCancelled_ShowsNothing()
    {
        // Stage
        var shown = false;
        var services = new ServiceCollection();
        services.AddSingleton<IPlaybackDialog>(new FakePlaybackDialog(null, _ => shown = true));
        using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();

        // Test
        shell.OpenPlaybackWindow.Execute(null);

        // Assert
        Assert.IsFalse(shown);
    }

    [TestMethod]
    public void OpenPlaybackWindow_NoDialog_CannotExecute()
    {
        // Stage
        using var shell = new ShellViewModel([]);

        // Assert
        Assert.IsFalse(shell.OpenPlaybackWindow.CanExecute(null));
    }
}
