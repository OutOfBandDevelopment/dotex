using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OoBDev.CodingStandards.Tests;

/// <summary>
/// Keyboard navigation as requirements: menu access keys are unique within a menu level, a menu item names one, and the tab
/// order follows the layout (no positive <c>TabIndex</c> outside templates). Held for every Applications XAML file.
/// </summary>
[TestClass, TestCategory("Unit")]
public class XamlKeyboardTests
{
    internal static char? AccessKey(string header)
    {
        for (var i = 0; i < header.Length - 1; i++)
        {
            if (header[i] != '_')
            {
                continue;
            }

            if (header[i + 1] == '_')
            {
                i++;
                continue;
            }

            return char.ToUpperInvariant(header[i + 1]);
        }

        return null;
    }

    internal static IEnumerable<string> ShortcutProblems(string xaml)
    {
        var document = XDocument.Parse(xaml);
        var gestures = document.Descendants().Where(e => e.Name.LocalName == "KeyBinding")
            .Select(e => $"{e.Attribute("Modifiers")?.Value}+{e.Attribute("Key")?.Value}".Trim('+'))
            .ToList();
        foreach (var duplicate in gestures.GroupBy(g => g, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
        {
            yield return $"shortcut {duplicate.Key} is bound more than once";
        }

        var shown = document.Descendants().Where(e => e.Name.LocalName == "MenuItem").Select(e => e.Attribute("InputGestureText")?.Value)
            .Where(g => g is not null).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var gesture in gestures.Where(g => !shown.Contains(g)))
        {
            yield return $"shortcut {gesture} has no menu item showing it (InputGestureText)";
        }
    }

    internal static IEnumerable<string> MenuProblems(string xaml)
    {
        var document = XDocument.Parse(xaml);
        foreach (var level in document.Descendants().Where(e => e.Elements().Any(c => c.Name.LocalName == "MenuItem")))
        {
            var items = level.Elements().Where(c => c.Name.LocalName == "MenuItem" && !IsBound(c)).ToList();
            var keys = new Dictionary<char, string>();
            foreach (var item in items)
            {
                var header = (string?)item.Attribute("Header");
                if (string.IsNullOrEmpty(header) || header.StartsWith('{'))
                {
                    continue;
                }

                var key = AccessKey(header);
                if (key is null)
                {
                    yield return $"menu item \"{header}\" has no access key (put _ before a letter)";
                }
                else if (!keys.TryAdd(key.Value, header))
                {
                    yield return $"menu items \"{keys[key.Value]}\" and \"{header}\" share the access key {key}";
                }
            }
        }
    }

    private static bool IsBound(XElement item) => item.Attribute("ItemsSource") is not null;

    internal static IEnumerable<string> TabIndexProblems(string xaml) =>
        XDocument.Parse(xaml).Descendants()
            .Where(e => !e.Ancestors().Any(a => a.Name.LocalName is "ControlTemplate" or "Style" or "DataTemplate"))
            .SelectMany(e => e.Attributes().Where(a => a.Name.LocalName is "TabIndex" or "KeyboardNavigation.TabIndex")
                .Where(a => int.TryParse(a.Value, out var index) && index > 0)
                .Select(a => $"{e.Name.LocalName} sets {a.Name.LocalName}={a.Value}; let the tab order follow the layout"));

    [TestMethod]
    public void MenuProblems_FindsDuplicateAndMissingKeys()
    {
        const string xaml = """
            <Menu xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
              <MenuItem Header="_File">
                <MenuItem Header="_Plugins" />
                <MenuItem Header="_Plugin approvals" />
                <MenuItem Header="Quit" />
                <MenuItem Header="Snake__case _Ok" />
              </MenuItem>
            </Menu>
            """;

        var problems = MenuProblems(xaml).ToList();

        Assert.HasCount(2, problems);
        StringAssert.Contains(problems[0], "share the access key P");
        StringAssert.Contains(problems[1], "\"Quit\" has no access key");
    }

    [TestMethod]
    public void TabIndexProblems_FlagsPositiveIndexOutsideTemplates()
    {
        const string xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
              <StackPanel>
                <TextBox TabIndex="2" />
                <TextBox TabIndex="0" />
                <ControlTemplate><TextBox TabIndex="3" /></ControlTemplate>
              </StackPanel>
            </Window>
            """;

        Assert.HasCount(1, TabIndexProblems(xaml).ToList());
    }

    [TestMethod]
    public void ShortcutProblems_DuplicateAndUnlabelled_AreReported()
    {
        const string xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
              <Window.InputBindings>
                <KeyBinding Key="N" Modifiers="Ctrl" /><KeyBinding Key="N" Modifiers="Ctrl" /><KeyBinding Key="Q" Modifiers="Ctrl" />
              </Window.InputBindings>
              <Menu><MenuItem Header="_A" InputGestureText="Ctrl+N" /></Menu>
            </Window>
            """;

        var problems = ShortcutProblems(xaml).ToList();

        Assert.HasCount(2, problems);
    }

    [TestMethod]
    public void XamlFiles_ShortcutsAreUniqueAndShownInAMenu()
    {
        var problems = XamlAccessibilityTests.XamlFiles()
            .SelectMany(f => ShortcutProblems(File.ReadAllText(f)).Select(p => $"{Path.GetFileName(f)}: {p}"))
            .ToList();

        Assert.IsEmpty(problems, string.Join(Environment.NewLine, problems));
    }

    [TestMethod]
    public void XamlFiles_MenusHaveUniqueAccessKeys()
    {
        var problems = XamlAccessibilityTests.XamlFiles()
            .SelectMany(f => MenuProblems(File.ReadAllText(f)).Select(p => $"{Path.GetFileName(f)}: {p}"))
            .ToList();

        Assert.IsEmpty(problems, string.Join(Environment.NewLine, problems));
    }

    [TestMethod]
    public void XamlFiles_TabOrderFollowsTheLayout()
    {
        var problems = XamlAccessibilityTests.XamlFiles()
            .SelectMany(f => TabIndexProblems(File.ReadAllText(f)).Select(p => $"{Path.GetFileName(f)}: {p}"))
            .ToList();

        Assert.IsEmpty(problems, string.Join(Environment.NewLine, problems));
    }
}
