using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Logging.Tests;

/// <summary>Freezes a version 1 log as written today so a later format change cannot silently break old captures.</summary>
[TestClass]
[TestCategory(TestCategories.Unit)]
[TestCategory(TestCategories.Logging)]
public class SessionLogCompatibilityTests
{
    private static readonly string[] _versionOne =
    [
        "{\"type\":\"header\",\"format\":\"dev-term-session-log\",\"version\":1,\"created\":\"2026-09-25T12:00:00.0000000Z\",\"application\":\"dev-term\",\"connection\":\"bench\",\"transport\":\"serial\",\"presenters\":[\"text\"]}",
        "{\"type\":\"open\",\"seq\":1,\"t\":\"2026-09-25T12:00:00.5000000Z\"}",
        "{\"type\":\"tx\",\"seq\":2,\"t\":\"2026-09-25T12:00:01.0000000Z\",\"data\":\"SUQ/DQ==\"}",
        "{\"type\":\"rx\",\"seq\":3,\"t\":\"2026-09-25T12:00:01.2500000Z\",\"data\":\"S09SQUQ=\"}",
        "{\"type\":\"note\",\"t\":\"2026-09-25T12:00:01.5000000Z\",\"text\":\"looks right\"}",
        "{\"type\":\"close\",\"seq\":4,\"t\":\"2026-09-25T12:00:03.0000000Z\"}",
    ];

    private static Stream Stream(params string[] lines) => new MemoryStream(Encoding.UTF8.GetBytes(string.Join('\n', lines) + "\n"));

    [TestMethod]
    public void VersionOneLog_StillLoads_WithItsRecordsAndPayloads()
    {
        // Stage and Test
        using var stream = Stream(_versionOne);
        var log = SessionLog.Read(stream);

        // Assert
        Assert.AreEqual(1, log.Header.Version);
        Assert.AreEqual("bench", log.Header.Connection);
        Assert.HasCount(5, log.Records);
        CollectionAssert.AreEqual(
            new[] { SessionLogRecordKind.Open, SessionLogRecordKind.Tx, SessionLogRecordKind.Rx, SessionLogRecordKind.Note, SessionLogRecordKind.Close },
            log.Records.Select(r => r.Kind).ToArray());
        Assert.AreEqual("ID?\r", Encoding.ASCII.GetString(log.Records[1].Data.Span));
        Assert.AreEqual("KORAD", Encoding.ASCII.GetString(log.Records[2].Data.Span));
        Assert.AreEqual("looks right", log.Records[3].Text);
    }

    [TestMethod]
    public void VersionOneLog_WritesBackIdentically()
    {
        using var input = Stream(_versionOne);
        var log = SessionLog.Read(input);

        using var output = new MemoryStream();
        log.Write(output);

        var lines = Encoding.UTF8.GetString(output.ToArray()).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        CollectionAssert.AreEqual(_versionOne, lines, "The version 1 layout changed; bump the version and keep a reader for version 1.");
    }
}
