using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Scpi;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The SCPI panel factory inside the shell.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ScpiPanelFactoryTests
{
    [TestMethod]
    public void Constructor_ScpiFactory_OpensGeneratedPanel()
    {
        // Stage
        var profile = new ScpiInstrumentProfile
        {
            Name = "Bench PSU",
            Terminator = "\n",
            Commands = [new ScpiCommandDefinition { Id = "idn", Label = "Identify", Category = "System", Template = "*IDN?", IsQuery = true }],
        };
        var factory = new ScpiPanelFactory(profile, _ => Task.FromResult<string?>("ACME"));

        // Test
        using var shell = new ShellViewModel([factory]);

        // Assert
        Assert.AreEqual("Bench PSU", shell.SelectedPanel?.Title);
        Assert.IsTrue(shell.SelectedPanel!.Form.Rows.Any());
    }
}
