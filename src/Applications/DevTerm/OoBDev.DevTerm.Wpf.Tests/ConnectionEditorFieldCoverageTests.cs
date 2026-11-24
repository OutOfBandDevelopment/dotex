using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Every field the connection editor view model declares for the form reaches the generated form the WPF window shows.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ConnectionEditorFieldCoverageTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void GeneratedForm_HasAControlForEveryFormFieldOfTheViewModel()
    {
        // Stage
        var fields = typeof(ConnectionEditorViewModel)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttributes().Any(a => a.GetType().Name == "FormFieldAttribute"))
            .Select(p => p.Name)
            .ToList();
        TestContext.WriteLine($"{fields.Count} annotated properties");

        // Test
        using var folder = new OoBDev.System.IO.TempDirectoryHandle();
        using var editor = new ConnectionEditorViewModel(new ConnectionProfileStore(folder.DirectoryPath), new CliOptions { Transport = "tcp" });
        var ids = editor.FormDefinition.Sections.SelectMany(s => s.Controls).Select(c => c.Id).ToHashSet(StringComparer.Ordinal);
        var missing = fields.Where(f => !ids.Contains(f)).ToList();

        // Assert
        Assert.IsTrue(fields.Count > 20);
        Assert.AreEqual(0, missing.Count, "No control in the generated form for: " + string.Join(", ", missing));
    }
}
