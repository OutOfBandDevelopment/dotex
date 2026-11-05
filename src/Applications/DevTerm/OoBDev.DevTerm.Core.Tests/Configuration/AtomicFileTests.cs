using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm;

using OoBDev.DevTerm.Configuration;
using OoBDev.System.IO;
using OoBDev.DevTerm.Configuration.Discovery;
using OoBDev.TestUtilities;
namespace OoBDev.DevTerm.Configuration.Tests;

/// <summary>
/// See docs/bugs/resolved/033-non-atomic-writes.md - <see cref="AtomicFile.WriteAllText"/> replaces a
/// bare <c>File.WriteAllText(path, ...)</c> (which overwrites in place, so a crash or power loss
/// mid-write leaves a truncated file at <c>path</c>) with a write to a temporary file followed by
/// an atomic <see cref="File.Move(string, string, bool)"/>.
/// </summary>
[TestCategory("Unit")]
[TestClass]
public sealed class AtomicFileTests
{
    [TestCategory(TestCategories.BugRegression)]
    [TestMethod]
    [TestCategory("Unit")]
    public void WriteAllText_OnSuccess_WritesContentsAndLeavesNoTemporaryFile()
    {
        using var folder = new TempDirectoryHandle();
        var path = Path.Combine(folder.DirectoryPath, "atomic.json");

        AtomicFile.WriteAllText(path, "{\"a\":1}");

        Assert.AreEqual("{\"a\":1}", File.ReadAllText(path));
        Assert.IsFalse(File.Exists($"{path}.tmp"));
    }

    [TestCategory(TestCategories.BugRegression)]
    [TestMethod]
    [TestCategory("Unit")]
    public void WriteAllText_WhenTheTemporaryFileCannotBeWritten_LeavesTheExistingFileUntouched()
    {
        using var folder = new TempDirectoryHandle();
        var path = Path.Combine(folder.DirectoryPath, "atomic.json");
        File.WriteAllText(path, "original");
        var temporary = $"{path}.tmp";

        // Holding the temp path open with no sharing simulates the write-to-temp step failing
        // partway - the real-world equivalent of the crash/power-loss scenario in the bug report,
        // which a unit test can't otherwise trigger. The old, non-atomic code wrote straight to
        // `path`, so a failure here would have already started clobbering "original"; the fix
        // never touches `path` until the temp file is fully written and ready to move.
        using (new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            Assert.ThrowsExactly<IOException>(() => AtomicFile.WriteAllText(path, "new"));
        }

        Assert.AreEqual("original", File.ReadAllText(path));
    }
}
