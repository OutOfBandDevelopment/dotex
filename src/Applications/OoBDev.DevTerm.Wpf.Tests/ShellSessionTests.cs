using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Opening sessions from the shell.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ShellSessionTests
{
    [TestMethod]
    public void NewLoopbackSession_NoFactory_CannotExecute()
    {
        // Stage
        using var shell = new ShellViewModel([]);

        // Assert
        Assert.IsFalse(shell.NewLoopbackSession.CanExecute(null));
    }

    [TestMethod]
    public async Task NewLoopbackSession_Composed_AddsAndSelectsASession()
    {
        // Stage
        await using var provider = new ServiceCollection().AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();

        // Test
        shell.NewLoopbackSession.Execute(null);
        shell.NewLoopbackSession.Execute(null);

        // Assert
        Assert.HasCount(2, shell.Sessions);
        Assert.AreEqual("Loopback 2", shell.SelectedSession?.Title);
    }
}
