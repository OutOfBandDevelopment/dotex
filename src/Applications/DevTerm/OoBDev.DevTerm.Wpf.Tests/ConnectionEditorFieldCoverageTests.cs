using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Every field the connection editor view model declares for the form is bound by the WPF window.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ConnectionEditorFieldCoverageTests
{
    [TestMethod]
    public void Xaml_BindsEveryFormFieldOfTheViewModel()
    {
        // Stage
        var xaml = File.ReadAllText(Path.Combine(RepoRoot(), "src", "Applications", "DevTerm", "OoBDev.DevTerm.Wpf", "ConnectionEditorWindow.xaml"));
        var fields = typeof(ConnectionEditorViewModel)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttributes().Any(a => a.GetType().Name == "FormFieldAttribute"))
            .Select(p => p.Name)
            .Where(name => name != nameof(ConnectionEditorViewModel.PresentersText)) // the checkbox list binds PresenterChoices instead
            .ToList();

        // Test
        var missing = fields.Where(name => !xaml.Contains("Binding " + name + ",", StringComparison.Ordinal)
            && !xaml.Contains("Binding " + name + "}", StringComparison.Ordinal)).ToList();

        // Assert
        Assert.IsTrue(fields.Count > 20);
        Assert.AreEqual(0, missing.Count, "Not bound in ConnectionEditorWindow.xaml: " + string.Join(", ", missing));
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GitVersion.yml")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
