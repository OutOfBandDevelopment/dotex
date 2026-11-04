using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OoBDev.DevTerm.Web.Tests;
namespace OoBDev.DevTerm.Web.Tests;

/// <summary>The web host's Manifest Editor service: Save As by name, an uploaded .ksy, and session-log sample data.</summary>
[TestClass]
[TestCategory("Unit")]
public class WebManifestEditorTests
{
    private const string _ksy = """
        meta:
          id: bench_frame
          endian: be
        seq:
          - id: header
            type: u1
          - id: reading
            type: u2
        """;

    [TestMethod]
    public void SaveAs_WritesAFolderNamedFromTheTypedName_AndRefusesAnEmptyOne()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"devterm-web-editor-{Guid.NewGuid():N}");
        try
        {
            var web = new WebManifestEditor(directory, null);
            web.Editor.New();
            Assert.IsFalse(web.SaveAs("   "));
            Assert.IsTrue(web.SaveAs("../escape"));
            Assert.IsTrue(File.Exists(Path.Combine(directory, "escape", "device.json")));
            Assert.IsTrue(web.SaveAs("Bench Copy"));
            Assert.IsTrue(File.Exists(Path.Combine(directory, "bench-copy", "device.json")));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [TestMethod]
    public void ImportKsy_TakesAnUploadedFileAsTheFrame_AndLeavesNoTempFileBehind()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"devterm-web-editor-{Guid.NewGuid():N}");
        var web = new WebManifestEditor(directory, null);
        web.Editor.New();
        var before = Directory.GetDirectories(Path.GetTempPath(), "devterm-ksy-*").Length;

        Assert.IsTrue(web.ImportKsy("../bench.ksy", _ksy), web.Editor.StatusMessage);
        Assert.IsNotNull(web.Editor.Manifest.Inbound?.Frame);
        Assert.AreEqual(before, Directory.GetDirectories(Path.GetTempPath(), "devterm-ksy-*").Length);
    }

    [TestMethod]
    public void UseRecording_OnlyAcceptsALogFromThePlaybackFolder()
    {
        var logs = WebScreenshotTests.SampleLogs();
        var web = new WebManifestEditor(Path.Combine(Path.GetTempPath(), $"devterm-web-editor-{Guid.NewGuid():N}"), null, new PlaybackLibrary(logs));
        web.Editor.New();

        CollectionAssert.AreEqual(new[] { "20261008-120000_Scope.jsonl" }, web.Recordings().ToArray());
        Assert.IsFalse(web.UseRecording("../../../outside.jsonl"));
        Assert.IsNull(web.Editor.Recording);
    }
}
