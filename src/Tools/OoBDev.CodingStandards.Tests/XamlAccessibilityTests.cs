using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OoBDev.CodingStandards.Tests;

/// <summary>
/// Screen readers need a name for every input control. Text boxes, lists and combo boxes must carry
/// <c>AutomationProperties.Name</c> or <c>LabeledBy</c>; buttons, check boxes and radio buttons are named by their content.
/// Every Applications XAML file is held to it with no exceptions.
/// </summary>
[TestClass, TestCategory("Unit")]
public class XamlAccessibilityTests
{
    private static readonly HashSet<string> _inputControls = new(StringComparer.Ordinal)
    {
        "TextBox", "PasswordBox", "ComboBox", "ListBox", "ListView", "DataGrid", "Slider", "TreeView", "TabControl",
    };

    private static readonly HashSet<string> _contentControls = new(StringComparer.Ordinal)
    {
        "Button", "CheckBox", "RadioButton", "ToggleButton",
    };

    private static IEnumerable<string> XamlFiles()
    {
        var root = Path.Combine(RepositoryRoot.Find(), "src", "Applications");
        return Directory.EnumerateFiles(root, "*.xaml", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
    }

    internal static int CountUnnamed(string xaml)
    {
        var document = XDocument.Parse(xaml);
        var count = 0;
        foreach (var element in document.Descendants())
        {
            var kind = element.Name.LocalName;
            var isInput = _inputControls.Contains(kind);
            if (!isInput && !_contentControls.Contains(kind))
            {
                continue;
            }

            if (element.Ancestors().Any(a => a.Name.LocalName is "ControlTemplate" or "Style"))
            {
                continue;
            }

            var named = element.Attributes().Any(a => a.Name.LocalName is "AutomationProperties.Name" or "AutomationProperties.LabeledBy")
                || element.Elements().Any(e => e.Name.LocalName.EndsWith(".AutomationProperties.Name", StringComparison.Ordinal));
            var hasContent = element.Attributes().Any(a => a.Name.LocalName is "Content" or "Header")
                || element.HasElements
                || !string.IsNullOrWhiteSpace(element.Value);
            if (!named && (isInput || !hasContent))
            {
                count++;
            }
        }

        return count;
    }

    [TestMethod]
    public void CountUnnamed_FlagsInputsWithoutAName_AndAcceptsNamedOnes()
    {
        const string xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
              <StackPanel>
                <TextBox x:Name="a" />
                <TextBox AutomationProperties.Name="Host" />
                <Button Content="Save" />
                <Button />
                <Button><Image /></Button>
                <ControlTemplate><TextBox /></ControlTemplate>
              </StackPanel>
            </Window>
            """;

        Assert.AreEqual(2, CountUnnamed(xaml));
    }

    [TestMethod]
    public void XamlFiles_HaveNoUnnamedInputControls()
    {
        var problems = XamlFiles()
            .Select(file => (Name: Path.GetFileName(file), Count: CountUnnamed(File.ReadAllText(file))))
            .Where(f => f.Count > 0)
            .Select(f => $"{f.Name}: {f.Count} unnamed controls. Add AutomationProperties.Name.")
            .ToList();

        Assert.IsEmpty(problems, string.Join(Environment.NewLine, problems));
    }
}
