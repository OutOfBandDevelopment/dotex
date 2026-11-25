using System;
using OoBDev.System.IO;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The converter tools window opened from the shell.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ConverterToolsDialogTests
{
    private sealed class FakeConverterToolsDialog(Func<ConverterToolsDialogViewModel, bool> interact) : IConverterToolsDialog
    {
        public bool Edit(ConverterToolsDialogViewModel tools) => interact(tools);
    }

    [TestMethod]
    public void ShowConverterTools_ToolAddedAndAccepted_SavesTheTool()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        var path = Path.Combine(folder.DirectoryPath, "tools.json");
        {
            var store = new ConverterToolsStore(path);
            var services = new ServiceCollection();
            services.AddSingleton<IConverterToolsDialog>(new FakeConverterToolsDialog(tools =>
            {
                tools.Add.Execute(null);
                tools.Name = "magick";
                tools.Path = "magick.exe";
                tools.Arguments = "{input} {output}";
                return tools.TryAccept();
            }));
            services.AddSingleton(store);
            using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
            var shell = provider.GetRequiredService<ShellViewModel>();

            // Test
            shell.ShowConverterTools.Execute(null);

            // Assert
            var saved = store.Load();
            Assert.HasCount(1, saved);
            Assert.AreEqual("magick", saved[0].Name);
        }
    }

    [TestMethod]
    public void ShowConverterTools_Cancelled_SavesNothing()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        var path = Path.Combine(folder.DirectoryPath, "tools.json");
        var store = new ConverterToolsStore(path);
        var services = new ServiceCollection();
        services.AddSingleton<IConverterToolsDialog>(new FakeConverterToolsDialog(tools =>
        {
            tools.Add.Execute(null);
            return false;
        }));
        services.AddSingleton(store);
        using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();

        // Test
        shell.ShowConverterTools.Execute(null);

        // Assert
        Assert.IsFalse(File.Exists(path));
    }

    [TestMethod]
    public void TryAccept_ToolWithoutPath_SetsErrorAndRejects()
    {
        // Stage
        var tools = new ConverterToolsDialogViewModel(new ConverterToolsEditor([]));
        tools.Add.Execute(null);
        tools.Name = "magick";

        // Test
        var accepted = tools.TryAccept();

        // Assert
        Assert.IsFalse(accepted);
        Assert.AreNotEqual(string.Empty, tools.Error);
    }

    [TestMethod]
    public void MoveDownAndRemove_TwoTools_ReordersAndRemoves()
    {
        // Stage
        var tools = new ConverterToolsDialogViewModel(new ConverterToolsEditor([]));
        tools.Add.Execute(null);
        tools.Name = "first";
        tools.Add.Execute(null);
        tools.Name = "second";
        tools.SelectedIndex = 0;

        // Test
        tools.MoveDown.Execute(null);

        // Assert
        CollectionAssert.AreEqual(new[] { "second", "first" }, tools.ToolNames);
        Assert.AreEqual(1, tools.SelectedIndex);
        tools.Remove.Execute(null);
        CollectionAssert.AreEqual(new[] { "second" }, tools.ToolNames);
    }

    [TestMethod]
    public void ShowConverterTools_NoDialog_CannotExecute()
    {
        // Stage
        using var shell = new ShellViewModel([]);

        // Assert
        Assert.IsFalse(shell.ShowConverterTools.CanExecute(null));
    }
}
