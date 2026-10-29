using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.TestUtilities;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Kuando.Busylight.Tests;

[TestClass]
public class BusylightStreamTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_IdentificationReport_DecodesPrintableText()
    {
        var pump = new SegmentPump<BusylightIdentification>(new BusylightDevice());
        var report = new byte[] { 0x00, 0x01 };
        var bytes = new byte[report.Length + 6];
        report.CopyTo(bytes, 0);
        Encoding.ASCII.GetBytes("PLENOM").CopyTo(bytes, 2);

        await pump.PushAsync(bytes).ConfigureAwait(false);

        Assert.AreEqual(1, pump.Messages.Count);
        Assert.AreEqual("..PLENOM", pump.Messages[0].Text);
    }
}
