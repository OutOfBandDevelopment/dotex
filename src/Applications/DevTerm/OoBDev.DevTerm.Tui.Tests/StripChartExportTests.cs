using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using System.IO;
using OoBDev.UiDefinitions;
namespace OoBDev.DevTerm.Tui.Tests;

[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class StripChartExportTests
{
    [TestMethod]
    public void SaveHistoryCsv_WritesTheHistoryIntoTheExportFolder()
    {
        using var exportFolder = new TempDirectoryHandle();
        var folder = exportFolder.DirectoryPath;
        var original = ControlPanelMode.ExportFolder;
        ControlPanelMode.ExportFolder = () => folder;
        try
        {
            var state = new StripChartState(new StripChartControl { Id = "chart", Label = "Chart", Channels = [new ChartChannel { Id = "a" }] });
            state.Apply("a", "1.5");

            var path = ControlPanelMode.SaveHistoryCsv(state);

            Assert.AreEqual(folder, Path.GetDirectoryName(path));
            Assert.AreEqual("Age,a\r\n0,1.5\r\n", File.ReadAllText(path));
        }
        finally
        {
            ControlPanelMode.ExportFolder = original;
        }
    }
}
