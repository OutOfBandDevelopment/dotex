using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Theme selection and its mapping onto WPF resources.</summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class ThemeTests
{
    [TestCleanup]
    public void ResetTheme() => ActiveTheme.Reset();

    [TestMethod]
    public void Selected_DarkChosen_MakesDarkTheCurrentTheme()
    {
        // Stage
        ActiveTheme.Reset();
        var themes = new ThemeViewModel();

        // Test
        themes.Selected = BuiltInThemes.DarkName;

        // Assert
        Assert.AreSame(BuiltInThemes.Dark, ActiveTheme.Current);
        CollectionAssert.Contains(themes.Names.ToList(), BuiltInThemes.DarkName);
    }

    [TestMethod]
    public void Apply_DarkTheme_AddsRoleBrushesAndDarkControlTemplates()
    {
        // Stage
        Color? background = null;
        var hasButtonTemplate = false;
        RunOnSta(() =>
        {
            var window = new Window();

            // Test
            WpfTheme.Apply(window, BuiltInThemes.Dark);

            // Assert
            background = (window.Resources[WpfTheme.Key(ThemeRole.Background)] as SolidColorBrush)?.Color;
            hasButtonTemplate = window.Resources.MergedDictionaries.SelectMany(d => d.MergedDictionaries).Any(d => d.Count > 0);
        });

        Assert.AreEqual(WpfTheme.ToColor(BuiltInThemes.Dark[ThemeRole.Background]), background);
        Assert.IsTrue(hasButtonTemplate);
    }

    [TestMethod]
    public void Attach_ThemeSwitched_ReappliesTheNewTheme()
    {
        // Stage
        ActiveTheme.Reset();
        Color? after = null;
        RunOnSta(() =>
        {
            var window = new Window();
            WpfTheme.Attach(window);

            // Test
            ActiveTheme.Select(BuiltInThemes.DarkName, persist: false);

            // Assert
            after = (window.Resources[WpfTheme.Key(ThemeRole.Background)] as SolidColorBrush)?.Color;
        });

        Assert.AreEqual(WpfTheme.ToColor(BuiltInThemes.Dark[ThemeRole.Background]), after);
    }

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
        {
            throw new AssertFailedException(failure.Message, failure);
        }
    }
}
