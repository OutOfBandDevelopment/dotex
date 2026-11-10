using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using OoBDev.System.IO;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>Device &gt; Install plugin: the path prompt, the install (with a stub approver) and the outcome box.</summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class InstallPluginDialogTests
{
    private sealed class Yes : IPluginApprover
    {
        public PluginApprovalChoice Ask(PluginApprovalRequest request) => PluginApprovalChoice.Once;
    }

    private static IEnumerable<View> All(View root) => root.SubViews.SelectMany(s => new[] { s }.Concat(All(s)));

    [TestMethod]
    public void InstallPlugin_PromptsForTheZip_InstallsIt_AndReportsTheOutcome()
    {
        using var folder = new TempDirectoryHandle();
        var zip = Path.Combine(folder.DirectoryPath, "echo.zip");
        using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
        {
            using var writer = new StreamWriter(archive.CreateEntry("plugin.json").Open());
            writer.Write("""{ "name": "Echo", "version": "1.0", "contract": 1, "process": { "command": "echo" } }""");
        }

        var plugins = Path.Combine(folder.DirectoryPath, "plugins");
        string? failure = null;
        string? outcome = null;
        using var done = new ManualResetEventSlim(false);

        TuiTestRunner.RunWithLoopApp(
            app => app.Driver!.SetScreenSize(100, 24),
            app =>
            {
                var root = new Window { Title = "dev-term" };
                var step = 0;
                app.AddTimeout(TimeSpan.FromMilliseconds(50), () =>
                {
                    app.AddTimeout(TimeSpan.FromMilliseconds(30), () =>
                    {
                        if (app.TopRunnableView is not { } dialog || dialog == root)
                        {
                            return true;
                        }

                        try
                        {
                            if (step++ == 0)
                            {
                                All(dialog).OfType<TextField>().First().Text = zip;
                                All(dialog).OfType<Button>().First(b => b.Text.Contains("OK", StringComparison.Ordinal)).InvokeCommand(Command.Accept);
                                return true;
                            }

                            outcome = dialog.Text + " " + string.Join(" ", All(dialog).Select(v => v.Text));
                            All(dialog).OfType<Button>().Concat((dialog as Dialog)?.Buttons ?? []).First().InvokeCommand(Command.Accept);
                        }
                        catch (Exception ex)
                        {
                            failure = ex.ToString();
                            app.RequestStop();
                        }

                        return step < 2;
                    });
                    try
                    {
                        TuiMode.InstallPlugin(app, plugins, new Yes());
                    }
                    catch (Exception ex)
                    {
                        failure ??= ex.ToString();
                    }
                    finally
                    {
                        done.Set();
                    }

                    return false;
                });
                return root;
            },
            (app, root) => Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(20)), "The dialogs never finished."));

        Assert.IsNull(failure, failure);
        Assert.IsTrue(File.Exists(Path.Combine(plugins, "Echo", "plugin.json")));
        StringAssert.Contains(outcome, "installed Echo", "The outcome box should say what was installed.");
    }
}
