using System;
using System.IO;
using System.Text;
using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Logging;
using OoBDev.DevTerm.Wpf;
using OoBDev.DevTerm.TestUtilities;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The playback window part way through a captured Tektronix session with a note and a trim mark, captured for the user guide.</summary>
[DoNotParallelize]
[TestCategory("DevLocal")]
[TestClass]
public sealed class PlaybackScreenshotTests
{
    private static readonly DateTimeOffset _t0 = new(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void PlaybackWindow_PartWayThroughWithANote_IsCaptured()
    {
        var directory = Directory.CreateTempSubdirectory("devterm-playback-");
        try
        {
            WpfScreenshot.Capture(() => BuildWindow(WriteSampleLog(directory.FullName)), "wpf-playback.png", 1000, 560);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    private static PlaybackWindow BuildWindow(string path)
    {
        var controller = new PlaybackPresenters().Open(path, new ManualTimeProvider());
        var dialog = new PlaybackDialogViewModel(controller);
        dialog.PresenterText = "ascii, hex";
        dialog.ApplyPresenters.Execute(null);
        dialog.JumpText = "5";
        dialog.Jump.Execute(null);
        dialog.NoteText = "IDN reply is correct";
        dialog.AddNote.Execute(null);
        dialog.Step.Execute(null);
        dialog.MarkIn.Execute(null);
        dialog.Speed = PlaybackController.Speeds[3];
        return new PlaybackWindow(dialog);
    }

    private static string WriteSampleLog(string directory)
    {
        var path = Path.Combine(directory, "tek2230.jsonl");
        new SessionLog(
            new SessionLogHeader { Created = _t0, Connection = "tcp://192.168.0.107:23", Profile = "tek2230", Transport = "tcp", Presenters = ["ascii"], Parser = "ascii" },
            [
                new() { Kind = SessionLogRecordKind.Session, Sequence = 1, Timestamp = _t0, Connection = "tcp://192.168.0.107:23", Profile = "tek2230", State = "closed" },
                new() { Kind = SessionLogRecordKind.Open, Sequence = 2, Timestamp = _t0.AddMilliseconds(40) },
                new() { Kind = SessionLogRecordKind.Tx, Sequence = 3, Timestamp = _t0.AddSeconds(1), Data = Encoding.ASCII.GetBytes("ID?\r") },
                new() { Kind = SessionLogRecordKind.Rx, Sequence = 4, Timestamp = _t0.AddMilliseconds(1200), Data = Encoding.ASCII.GetBytes("ID TEK/2230,") },
                new() { Kind = SessionLogRecordKind.Rx, Sequence = 5, Timestamp = _t0.AddMilliseconds(1250), Data = Encoding.ASCII.GetBytes("V81.1,VERS:14\r") },
                new() { Kind = SessionLogRecordKind.Tx, Sequence = 6, Timestamp = _t0.AddSeconds(6), Data = Encoding.ASCII.GetBytes("CH1?\r") },
                new() { Kind = SessionLogRecordKind.Rx, Sequence = 7, Timestamp = _t0.AddMilliseconds(6300), Data = Encoding.ASCII.GetBytes("CH1 VOLTS:1,COUPLING:DC\r") },
                new() { Kind = SessionLogRecordKind.Disconnect, Sequence = 8, Timestamp = _t0.AddSeconds(9) },
            ]).Save(path);
        return path;
    }
}
