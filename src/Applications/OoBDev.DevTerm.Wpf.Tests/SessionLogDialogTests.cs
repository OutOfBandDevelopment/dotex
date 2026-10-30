using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Loopback;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Choosing the log path and adding notes to a running log.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class SessionLogDialogTests
{
    [TestMethod]
    public async Task ToggleLogging_PathChosen_LogsToThatPath()
    {
        // Stage
        var dir = NewDirectory();
        try
        {
            var chosen = Path.Combine(dir, "mine.jsonl");
            var dialog = new FakeSessionLogDialog { PathToReturn = chosen };
            await using var viewModel = Create(dialog);

            // Test
            viewModel.ToggleLogging.Execute(null);

            // Assert
            Assert.IsTrue(viewModel.IsLogging);
            Assert.AreEqual(chosen, viewModel.LogPath);
            Assert.IsNotNull(dialog.SuggestedPath);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [TestMethod]
    public async Task ToggleLogging_DialogCancelled_DoesNotStart()
    {
        // Stage
        await using var viewModel = Create(new FakeSessionLogDialog { PathToReturn = null });

        // Test
        viewModel.ToggleLogging.Execute(null);

        // Assert
        Assert.IsFalse(viewModel.IsLogging);
    }

    [TestMethod]
    public async Task AddLogNote_NotLogging_CannotExecute()
    {
        // Stage
        await using var viewModel = Create(new FakeSessionLogDialog { NoteToReturn = "x" });

        // Assert
        Assert.IsFalse(viewModel.AddLogNote.CanExecute(null));
    }

    [TestMethod]
    public async Task AddLogNote_Logging_WritesNoteToTheLog()
    {
        // Stage
        var dir = NewDirectory();
        try
        {
            var chosen = Path.Combine(dir, "notes.jsonl");
            var dialog = new FakeSessionLogDialog { PathToReturn = chosen, NoteToReturn = "marker-note-42" };
            var viewModel = Create(dialog);
            viewModel.ToggleLogging.Execute(null);

            // Test
            var canAdd = viewModel.AddLogNote.CanExecute(null);
            viewModel.AddLogNote.Execute(null);
            await viewModel.DisposeAsync();

            // Assert
            Assert.IsTrue(canAdd);
            Assert.IsTrue(File.ReadAllLines(chosen).Any(line => line.Contains("marker-note-42", StringComparison.Ordinal)));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [TestMethod]
    public async Task AddLogNote_BlankNote_WritesNothing()
    {
        // Stage
        var dir = NewDirectory();
        try
        {
            var chosen = Path.Combine(dir, "blank.jsonl");
            var dialog = new FakeSessionLogDialog { PathToReturn = chosen, NoteToReturn = "   " };
            var viewModel = Create(dialog);
            viewModel.ToggleLogging.Execute(null);

            // Test
            viewModel.AddLogNote.Execute(null);
            await viewModel.DisposeAsync();

            // Assert
            Assert.IsFalse(File.ReadAllLines(chosen).Any(line => line.Contains("\"note\"", StringComparison.Ordinal)));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    private static SessionViewModel Create(ISessionLogDialog dialog)
    {
        var transport = new LoopbackTransport(Options.Create(new LoopbackTransportOptions()));
        var session = new Session(transport, new Pipeline([]));
        var tab = new SessionTab(session, new PresenterCatalog([]), new CliOptions());
        return new SessionViewModel(session, "Loopback", tab, dialog);
    }

    private static string NewDirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), "devterm-wpf-note-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}
