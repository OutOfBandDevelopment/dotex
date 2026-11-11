using System.IO;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Examples.Controls.Avalonia.Tests;

/// <summary>Writes a rendered window to the test results directory and attaches it to the test result.</summary>
public static class Screenshots
{
    /// <summary>Saves <paramref name="window"/> as <c>{TestName}-{step}.png</c> under <see cref="TestContext.TestResultsDirectory"/> and attaches it with <see cref="TestContext.AddResultFile"/>.</summary>
    public static string Save(TestContext context, Window window, string step)
    {
        Dispatcher.UIThread.RunJobs();
        var directory = context.TestResultsDirectory ?? Path.GetTempPath();
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{context.TestName}-{step}.png");
        window.CaptureRenderedFrame()!.Save(path);
        context.AddResultFile(path);
        return path;
    }
}
