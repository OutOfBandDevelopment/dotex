using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.System.IO.Messages;
using OoBDev.TestUtilities;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Scpi.Tests;

[TestClass]
public class ScpiTests
{
    private static ScpiInstrumentProfile Profile(string terminator) => new()
    {
        Name = "Test Instrument",
        Terminator = terminator,
        Commands =
        [
            new ScpiCommandDefinition { Id = "meas", Label = "Measure", Template = "MEAS:VOLT:DC? DEF", IsQuery = true },
            new ScpiCommandDefinition
            {
                Id = "vset",
                Label = "Set Voltage",
                Template = "VSET1:{Voltage}",
                Parameters = [new ScpiParameterDefinition { Name = "Voltage", Kind = ScpiParameterKind.Numeric, Minimum = 0, Maximum = 60, DecimalPlaces = 2, IntegerDigits = 2, DefaultValue = "0" }],
            },
            new ScpiCommandDefinition
            {
                Id = "two",
                Label = "Two",
                Template = "A {First} B {Second}",
                Parameters =
                [
                    new ScpiParameterDefinition { Name = "First", DefaultValue = "x" },
                    new ScpiParameterDefinition { Name = "Second" },
                ],
            },
        ],
    };

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_Query_AppendsTerminatorAndFlagsQuery()
    {
        var request = Profile("\n").Build("meas");
        Assert.AreEqual("MEAS:VOLT:DC? DEF\n", request.WireText);
        Assert.IsTrue(request.IsQuery);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("5", "VSET1:05.00")]
    [DataRow("12", "VSET1:12.00")]
    [DataRow("3.456", "VSET1:03.46")]
    [DataRow("99", "VSET1:60.00")]
    [DataRow("-4", "VSET1:00.00")]
    [DataRow("junk", "VSET1:00.00")]
    [DataRow("", "VSET1:00.00")]
    public void Build_FixedWidthNumeric_PadsRoundsAndClamps(string value, string expected) =>
        Assert.AreEqual(expected, Profile("").Build("vset", [value]).WireText);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_NoValues_UsesDefaults() =>
        Assert.AreEqual("VSET1:00.00", Profile("").Build("vset").WireText);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_ValueContainingAnotherPlaceholder_IsNotSubstitutedTwice() =>
        Assert.AreEqual("A {Second} B y\r\n", Profile("\r\n").Build("two", ["{Second}", "y"]).WireText);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_UnknownCommand_Throws() =>
        Assert.ThrowsExactly<ArgumentException>(() => Profile("\n").Build("nope"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void BuildCustom_SendsTextVerbatimWithTerminator() =>
        Assert.AreEqual("*IDN?\r", Profile("\r").BuildCustom("*IDN?", isQuery: true).WireText);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Encoder_WritesAscii()
    {
        var request = Profile("\n").Build("meas");
        Assert.AreEqual("MEAS:VOLT:DC? DEF\n", Encoding.ASCII.GetString(new ScpiMessageEncoder().Encode(ref request).ToArray()));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("\n")]
    [DataRow("\r")]
    [DataRow("\r\n")]
    public async Task Device_Terminated_CutsEachLineWhateverTheEnding(string ending)
    {
        var pump = new SegmentPump<ScpiReply>(new ScpiDevice(Profile("\n")));
        var bytes = Encoding.ASCII.GetBytes("RIGOL,DG1022" + ending + "1.5" + ending);
        await pump.PushAsync(bytes[..5]);
        await pump.PushAsync(bytes[5..]);

        CollectionAssert.AreEqual(new[] { "RIGOL,DG1022", "1.5" }, pump.Messages.Select(m => m.Text).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Device_DefiniteLengthBlock_IsOneReplyEvenWithLineEndingsInTheData()
    {
        // Stage
        var data = new byte[] { 0x42, 0x4D, 0x0D, 0x0A, 0x00, 0x0A, 0x0D, 0xFF, 0x23, 0x31 };
        var block = Encoding.ASCII.GetBytes("#210").Concat(data).Concat(Encoding.ASCII.GetBytes("\n")).ToArray();
        var pump = new SegmentPump<ScpiReply>(new ScpiDevice(Profile("\n")));

        // Test
        await pump.PushAsync(block[..7]);
        await pump.PushAsync(block[7..]);
        await pump.PushAsync(Encoding.ASCII.GetBytes("OK\n"));

        // Assert
        Assert.AreEqual(2, pump.Messages.Count);
        CollectionAssert.AreEqual(data, pump.Messages[0].Block.ToArray());
        Assert.AreEqual("OK", pump.Messages[1].Text);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("#\n")]
    [DataRow("#0abc\n")]
    [DataRow("#x\n")]
    [DataRow("#2ab\n")]
    public async Task Device_HashThatIsNotABlockHeader_IsAnOrdinaryLine(string line)
    {
        // Stage
        var pump = new SegmentPump<ScpiReply>(new ScpiDevice(Profile("\n")));

        // Test
        await pump.PushAsync(Encoding.ASCII.GetBytes(line));

        // Assert
        Assert.AreEqual(1, pump.Messages.Count);
        Assert.AreEqual(line.TrimEnd('\n'), pump.Messages[0].Text);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decoder_TruncatedBlock_FallsBackToText()
    {
        // Test
        var reply = new ScpiMessageDecoder().Decode(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("#15ab")));

        // Assert
        Assert.AreEqual("#15ab", reply.Text);
        Assert.IsTrue(reply.Block.IsEmpty);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Device_Unterminated_TreatsEachArrivalAsOneReply()
    {
        var pump = new SegmentPump<ScpiReply>(new ScpiDevice(Profile("")));
        await pump.PushAsync(Encoding.ASCII.GetBytes("05.00"));
        await pump.PushAsync(Encoding.ASCII.GetBytes("KORAD"));

        CollectionAssert.AreEqual(new[] { "05.00", "KORAD" }, pump.Messages.Select(m => m.Text).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Device_NullProfile_Throws() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new ScpiDevice(null!));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Catalog_Bundled_LoadsAllNineProfilesWithoutErrors()
    {
        var catalog = ScpiProfileCatalog.Create();
        Assert.AreEqual(9, catalog.Profiles.Count);
        Assert.AreEqual(0, catalog.LoadErrors.Count, string.Join("; ", catalog.LoadErrors));
        Assert.IsTrue(catalog.Profiles.All(p => p.Commands.Count > 0));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("KORAD KA3005P V5.8 SN:12345", "Korad KA3005P Power Supply")]
    [DataRow("RIGOL TECHNOLOGIES,DG1022,DG1D1,00.01", null)]
    [DataRow("TEKTRONIX,TDS 2024,0,CF:91.1CT", null)]
    [DataRow("HEWLETT-PACKARD,34401A,0,10-5-2", null)]
    public void Catalog_TryMatchByIdn_FindsTheProfile(string idn, string? name)
    {
        var profile = ScpiProfileCatalog.Create().TryMatchByIdn(idn);
        Assert.IsNotNull(profile);
        if (name is not null) Assert.AreEqual(name, profile.Name);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Catalog_TryMatchByIdn_UnknownReply_ReturnsNull() =>
        Assert.IsNull(ScpiProfileCatalog.Create().TryMatchByIdn("ACME,Widget,1,2"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Catalog_DropInDirectory_AddsProfilesAndReportsBadFiles()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ScpiTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "a.json"), """
                { "Name": "Synthetic", "IdnPattern": "SYN9000", "Terminator": "\r\n",
                  "Commands": [ { "Id": "freq", "Label": "F", "Template": "FREQ {F}",
                    "Parameters": [ { "Name": "F", "Kind": "Numeric", "Minimum": 0, "Maximum": 100 } ] } ] }
                """);
            File.WriteAllText(Path.Combine(directory, "b.json"), "{ not json");
            File.WriteAllText(Path.Combine(directory, "c.json"), """
                { "Name": "Backwards", "Commands": [ { "Id": "x", "Label": "X", "Template": "X {V}",
                    "Parameters": [ { "Name": "V", "Kind": "Numeric", "Minimum": 5, "Maximum": 1 } ] } ] }
                """);

            var catalog = ScpiProfileCatalog.Create([directory, Path.Combine(directory, "missing")]);

            Assert.AreEqual(10, catalog.Profiles.Count);
            Assert.AreEqual("\r\n", catalog.TryMatchByIdn("SYN9000,1")!.Terminator);
            Assert.AreEqual(2, catalog.LoadErrors.Count);
            Assert.IsTrue(catalog.LoadErrors.Any(e => e.StartsWith("b.json", StringComparison.Ordinal)));
            Assert.IsTrue(catalog.LoadErrors.Any(e => e.Contains("Minimum", StringComparison.Ordinal)));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Catalog_BadIdnPattern_IsTreatedAsNoMatch()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ScpiTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "a.json"), """{ "Name": "Bad", "IdnPattern": "(", "Commands": [] }""");
            Assert.IsNull(ScpiProfileCatalog.Create([directory]).TryMatchByIdn("ACME"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Generic_HasTheCommonCommandsAndNoIdnPattern()
    {
        CollectionAssert.AreEqual(new[] { "idn", "rst", "cls", "opc" }, ScpiProfileCatalog.Generic.Commands.Select(c => c.Id).ToArray());
        Assert.IsNull(ScpiProfileCatalog.Generic.IdnPattern);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decoder_TrimsLineEndings() =>
        Assert.AreEqual("1.23E+0", new ScpiMessageDecoder().Decode(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("1.23E+0\r\n"))).Text);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryAddScpi_RegistersCodecsAndCatalog()
    {
        using var provider = new ServiceCollection().TryAddScpi().BuildServiceProvider();
        Assert.IsInstanceOfType<ScpiMessageDecoder>(provider.GetRequiredService<IMessageDecoder<ScpiReply>>());
        Assert.IsInstanceOfType<ScpiMessageEncoder>(provider.GetRequiredService<IMessageEncoder<ScpiRequest>>());
        Assert.AreEqual(9, provider.GetRequiredService<ScpiProfileCatalog>().Profiles.Count);
    }
}
