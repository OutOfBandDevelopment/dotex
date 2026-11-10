using System;
using OoBDev.System.IO;
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
        using var directoryFolder = new TempDirectoryHandle();
        var directory = directoryFolder.DirectoryPath;
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
    }

    [TestMethod]
    public void Jump_UnreadableTextThenRecordNumber_ReportsThenListsUpToThatRecord()
    {
        // Stage
        using var directoryFolder = new TempDirectoryHandle();
        var directory = directoryFolder.DirectoryPath;
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
    }

    [TestMethod]
    public void TrimAndNote_MarkedRange_SavesTheTrimNextToTheLogAndTheNoteIntoIt()
    {
        // Stage
        using var directoryFolder = new TempDirectoryHandle();
        var directory = directoryFolder.DirectoryPath;
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
            var services = new ServiceCollection();
            services.AddSingleton<IPlaybackDialog>(new FakePlaybackDialog(path, playback =>
            {
                playback.Step.Execute(null);
                playback.MarkIn.Execute(null);
                playback.Step.Execute(null);
                playback.Step.Execute(null);
                playback.MarkOut.Execute(null);
                playback.SaveTrim.Execute(null);
                playback.NoteText = "checked";
                playback.AddNote.Execute(null);
                playback.NoteText = "rechecked";
                playback.EditNote.Execute(null);
            }));
            using var provider = services.AddDevTermWpfShell().BuildServiceProvider();

            // Test
            provider.GetRequiredService<ShellViewModel>().OpenPlaybackWindow.Execute(null);

            // Assert
            Assert.AreEqual(1, Directory.GetFiles(directory, "*.trim-*").Length);
            var saved = File.ReadAllText(path);
            Assert.IsTrue(saved.Contains("rechecked", StringComparison.Ordinal));
            Assert.IsTrue(saved.Contains("\"history\":[\"checked\"]", StringComparison.Ordinal));
        }
    }

    [TestMethod]
    public void ApplyPresenters_HexChosen_ReplaysTheLinesThroughIt()
    {
        // Stage
        using var directoryFolder = new TempDirectoryHandle();
        var directory = directoryFolder.DirectoryPath;
        {
            var path = Path.Combine(directory, "capture.jsonl");
            var t0 = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
            new SessionLog(
                new SessionLogHeader { Created = t0, Connection = "loopback", Presenters = ["ascii"] },
                [new SessionLogRecord { Kind = SessionLogRecordKind.Rx, Sequence = 1, Timestamp = t0, Data = Encoding.ASCII.GetBytes("abc") }]).Save(path);
            string? line = null;
            var services = new ServiceCollection();
            services.AddSingleton<IPlaybackDialog>(new FakePlaybackDialog(path, playback =>
            {
                playback.SkipToEnd.Execute(null);
                playback.PresenterText = "hex";
                playback.ApplyPresenters.Execute(null);
                line = playback.Lines.SingleOrDefault();
            }));
            using var provider = services.AddDevTermWpfShell().BuildServiceProvider();

            // Test
            provider.GetRequiredService<ShellViewModel>().OpenPlaybackWindow.Execute(null);

            // Assert
            Assert.IsNotNull(line);
            StringAssert.Contains(line, "hex", StringComparison.OrdinalIgnoreCase);
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
