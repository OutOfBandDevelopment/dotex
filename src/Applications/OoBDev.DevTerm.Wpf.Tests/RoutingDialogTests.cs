using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The routing window opened from the shell.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class RoutingDialogTests
{
    [TestMethod]
    public async Task ShowRoutingWindow_LoopbackProfileSession_OpensTheDialogAndApplyReportsTheProblem()
    {
        // Stage
        var directory = Path.Combine(Path.GetTempPath(), "devterm-" + Guid.NewGuid().ToString("N"));
        try
        {
            string? message = null;
            var services = new ServiceCollection();
            services.AddSingleton(new ConnectionProfileStore(directory));
            services.AddSingleton<IRoutingDialog>(new FakeRoutingDialog(dialog =>
            {
                dialog.Draft.Host = string.Empty;
                dialog.Apply.Execute(null);
                message = dialog.Message;
            }));
            await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
            var shell = provider.GetRequiredService<ShellViewModel>();
            provider.GetRequiredService<ConnectionProfileStore>().Save("bench", new CliOptions { Transport = "loopback" });
            shell.OpenProfileSession.Execute("bench");

            // Test
            shell.ShowRoutingWindow.Execute(null);

            // Assert
            Assert.IsTrue(shell.ShowRoutingWindow.CanExecute(null));
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [TestMethod]
    public async Task ShowRoutingWindow_LoopbackSessionWithoutTab_CannotExecute()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddSingleton<IRoutingDialog>(new FakeRoutingDialog(_ => { }));
        await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();

        // Test
        shell.NewLoopbackSession.Execute(null);

        // Assert
        Assert.IsFalse(shell.ShowRoutingWindow.CanExecute(null));
    }
}
