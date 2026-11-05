using System;
using OoBDev.System.IO;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Rule list editing in the routing window.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class RoutingRuleEditingTests
{
    [TestMethod]
    public async Task AddRuleCommand_Executed_AddsARowAndADraftRule()
    {
        await RunAsync(dialog =>
        {
            // Stage
            var before = dialog.Rules.Count;

            // Test
            dialog.AddRuleCommand.Execute(null);

            // Assert
            Assert.AreEqual(before + 1, dialog.Rules.Count);
            Assert.AreEqual(before + 1, dialog.Draft.Rules.Count);
        });
    }

    [TestMethod]
    public async Task RemoveRuleCommand_RowGiven_RemovesThatRule()
    {
        await RunAsync(dialog =>
        {
            // Stage
            dialog.AddRuleCommand.Execute(null);
            dialog.AddRuleCommand.Execute(null);
            var count = dialog.Rules.Count;

            // Test
            dialog.RemoveRuleCommand.Execute(dialog.Rules[0]);

            // Assert
            Assert.AreEqual(count - 1, dialog.Rules.Count);
            Assert.AreEqual(count - 1, dialog.Draft.Rules.Count);
        });
    }

    [TestMethod]
    public async Task MoveRuleDown_FirstRow_SwapsTheOrder()
    {
        await RunAsync(dialog =>
        {
            // Stage
            dialog.AddRuleCommand.Execute(null);
            dialog.AddRuleCommand.Execute(null);
            dialog.Rules[0].Topic = "first";
            dialog.Rules[1].Topic = "second";

            // Test
            dialog.MoveRuleDown.Execute(dialog.Rules[0]);

            // Assert
            Assert.AreEqual("second", dialog.Rules[0].Topic);
            Assert.AreEqual("first", dialog.Rules[1].Topic);
            Assert.AreEqual("second", dialog.Draft.Rules[0].Topic);
        });
    }

    [TestMethod]
    public async Task MoveRuleUp_SecondRow_SwapsTheOrder()
    {
        await RunAsync(dialog =>
        {
            // Stage
            dialog.AddRuleCommand.Execute(null);
            dialog.AddRuleCommand.Execute(null);
            dialog.Rules[0].Topic = "first";
            dialog.Rules[1].Topic = "second";

            // Test
            dialog.MoveRuleUp.Execute(dialog.Rules[1]);

            // Assert
            Assert.AreEqual("second", dialog.Rules[0].Topic);
        });
    }

    [TestMethod]
    public async Task TestRulesCommand_SampleGiven_FillsOneResultPerRule()
    {
        await RunAsync(dialog =>
        {
            // Stage
            dialog.AddRuleCommand.Execute(null);
            dialog.SampleText = "hello";

            // Test
            dialog.TestRulesCommand.Execute(null);

            // Assert
            Assert.AreEqual(dialog.Rules.Count, dialog.TestResults.Count);
        });
    }

    [TestMethod]
    public async Task RefreshCommand_Executed_KeepsRowsAndHistoryConsistent()
    {
        await RunAsync(dialog =>
        {
            // Stage
            dialog.AddRuleCommand.Execute(null);

            // Test
            dialog.RefreshCommand.Execute(null);

            // Assert
            Assert.IsTrue(dialog.Rules.All(row => row.Hits == 0));
            Assert.AreEqual(0, dialog.History.Count);
        });
    }

    private static async Task RunAsync(Action<RoutingDialogViewModel> body)
    {
        using var directoryFolder = new TempDirectoryHandle();
        var directory = directoryFolder.DirectoryPath;
        {
            var services = new ServiceCollection();
            services.AddSingleton(new ConnectionProfileStore(directory));
            services.AddSingleton<IRoutingDialog>(new FakeRoutingDialog(body));
            await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
            var shell = provider.GetRequiredService<ShellViewModel>();
            provider.GetRequiredService<ConnectionProfileStore>().Save("bench", new CliOptions { Transport = "loopback" });
            shell.OpenProfileSession.Execute("bench");
            shell.ShowRoutingWindow.Execute(null);
        }
    }
}
