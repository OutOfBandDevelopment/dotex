using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

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

    [TestMethod]
    public async Task OpenProfileSession_SavedLoopbackProfile_AddsASessionTitledByTheProfile()
    {
        // Stage
        var directory = Path.Combine(Path.GetTempPath(), "devterm-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new ConnectionProfileStore(directory);
            store.Save("bench", new CliOptions { Transport = "loopback" });
            var services = new ServiceCollection();
            services.AddSingleton(store);
            await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
            var shell = provider.GetRequiredService<ShellViewModel>();

            // Test
            shell.OpenProfileSession.Execute("bench");

            // Assert
            CollectionAssert.AreEqual(new[] { "bench" }, (global::System.Collections.ICollection)shell.ProfileNames);
            Assert.HasCount(1, shell.Sessions);
            Assert.AreEqual("bench", shell.SelectedSession?.Title);
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
    public void OpenProfileSession_NoParameter_CannotExecute()
    {
        // Stage
        using var shell = new ShellViewModel([]);

        // Assert
        Assert.IsFalse(shell.OpenProfileSession.CanExecute("bench"));
    }

    [TestMethod]
    public async Task NewConnection_EditorConnects_OpensASessionFromTheOptions()
    {
        // Stage
        var directory = Path.Combine(Path.GetTempPath(), "devterm-" + Guid.NewGuid().ToString("N"));
        try
        {
            var services = new ServiceCollection();
            services.AddSingleton(new ConnectionProfileStore(directory));
            services.AddSingleton<IConnectionEditorDialog>(new FakeConnectionEditorDialog(editor =>
            {
                editor.Transport = "loopback";
                editor.ConnectCommand.Execute(null);
            }));
            await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
            var shell = provider.GetRequiredService<ShellViewModel>();

            // Test
            shell.NewConnection.Execute(null);

            // Assert
            Assert.HasCount(1, shell.Sessions);
            Assert.AreEqual("loopback 1", shell.SelectedSession?.Title);
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
    public async Task NewConnection_EditorCancelled_OpensNothing()
    {
        // Stage
        var directory = Path.Combine(Path.GetTempPath(), "devterm-" + Guid.NewGuid().ToString("N"));
        try
        {
            var services = new ServiceCollection();
            services.AddSingleton(new ConnectionProfileStore(directory));
            services.AddSingleton<IConnectionEditorDialog>(new FakeConnectionEditorDialog(_ => { }));
            await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
            var shell = provider.GetRequiredService<ShellViewModel>();

            // Test
            shell.NewConnection.Execute(null);

            // Assert
            Assert.IsEmpty(shell.Sessions);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
