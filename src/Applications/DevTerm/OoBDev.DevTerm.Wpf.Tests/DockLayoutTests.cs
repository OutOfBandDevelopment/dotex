using System.IO;
using AvalonDock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Wpf;

namespace OoBDev.DevTerm.Wpf.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class DockLayoutTests
{
    [TestMethod]
    public void SaveThenLoad_RoundTripsTheLayout_AndAMissingFileKeepsTheDefault()
    {
        // Stage
        var path = Path.Combine(Directory.CreateTempSubdirectory("devterm-dock-").FullName, "sub", "layout.xml");
        WpfScreenshot.RunOnSta(() =>
        {
            var window = new DevTermApp.MainWindow();
            var dock = (DockingManager)window.FindName("Dock");

            // Test / Assert
            Assert.IsFalse(DockLayout.Load(dock, path));
            DockLayout.Save(dock, path);
            Assert.IsTrue(File.Exists(path));
            Assert.IsTrue(DockLayout.Load(dock, path));
            File.WriteAllText(path, "not xml");
            Assert.IsFalse(DockLayout.Load(dock, path));
        });
    }
}
