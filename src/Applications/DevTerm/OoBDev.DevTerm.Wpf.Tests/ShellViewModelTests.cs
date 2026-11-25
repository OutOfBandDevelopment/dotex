using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The shell view model with the demo panel.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ShellViewModelTests
{
    [TestMethod]
    public void Constructor_OneFactory_OpensItsPanel()
    {
        // Stage
        var factory = new DemoPanelFactory();

        // Test
        using var shell = new ShellViewModel([factory]);

        // Assert
        Assert.AreEqual("Demo", shell.SelectedPanel?.Title);
        Assert.IsTrue(shell.SelectedPanel!.Form.Rows.Any());
    }

    [TestMethod]
    public void Open_UnknownFactory_DoesNothing()
    {
        // Stage
        using var shell = new ShellViewModel([]);

        // Test
        var can = shell.Open.CanExecute(new DemoPanelFactory());
        shell.Open.Execute(new DemoPanelFactory());

        // Assert
        Assert.IsFalse(can);
        Assert.IsNull(shell.SelectedPanel);
    }

    [TestMethod]
    public void Open_KnownFactory_ReplacesPanelAndDisposesPrevious()
    {
        // Stage
        var factory = new DemoPanelFactory();
        using var shell = new ShellViewModel([factory]);
        var first = shell.SelectedPanel;
        var raised = 0;
        shell.PropertyChanged += (_, _) => raised++;

        // Test
        shell.Open.Execute(factory);

        // Assert
        Assert.AreNotSame(first, shell.SelectedPanel);
        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void Constructor_Null_Throws()
    {
        // Assert
        Assert.ThrowsExactly<global::System.ArgumentNullException>(() => new ShellViewModel(null!));
    }
}
