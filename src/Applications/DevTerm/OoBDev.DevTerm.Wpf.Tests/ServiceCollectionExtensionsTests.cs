using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The shell composition through DI.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
    [TestMethod]
    public void AddDevTermWpfShell_Built_ResolvesShellWithDemoPanel()
    {
        // Stage
        using var provider = new ServiceCollection().AddDevTermWpfShell().BuildServiceProvider();

        // Test
        var shell = provider.GetRequiredService<ShellViewModel>();

        // Assert
        Assert.AreEqual("Demo", shell.SelectedPanel?.Title);
    }

    [TestMethod]
    public void AddDevTermWpfShell_Built_RegistersDevicePanelContributions()
    {
        // Stage
        using var provider = new ServiceCollection().AddDevTermWpfShell().BuildServiceProvider();

        // Test
        var ids = provider.GetServices<IDevicePanelContribution>().Select(c => c.Id).ToList();

        // Assert
        CollectionAssert.Contains(ids, "k8055");
        CollectionAssert.Contains(ids, "busylight");
    }
}
