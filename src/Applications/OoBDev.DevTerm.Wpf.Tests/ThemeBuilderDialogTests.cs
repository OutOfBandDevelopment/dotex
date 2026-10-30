using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The theme seed picker and builder opened from the shell.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ThemeBuilderDialogTests
{
    private sealed class FakeThemeBuilderDialog(Func<ThemeSeedPickerViewModel, bool> pick, Func<ThemeBuilderDialogViewModel, bool> edit) : IThemeBuilderDialog
    {
        public bool PickSeed(ThemeSeedPickerViewModel picker) => pick(picker);

        public bool Edit(ThemeBuilderDialogViewModel builder) => edit(builder);
    }

    private sealed class FixedColorPicker(string? result) : IColorPicker
    {
        public string? Pick(string? current) => result;
    }

    private sealed class FixedConfirm(bool answer) : IConfirmPrompt
    {
        public bool Confirm(string message) => answer;
    }

    private static string TempDirectory() => Directory.CreateTempSubdirectory().FullName;

    [TestMethod]
    public void TryChoose_FirstOption_NamesTheCopyAfterIt()
    {
        // Stage
        var picker = new ThemeSeedPickerViewModel(null, TempDirectory());

        // Test
        var chosen = picker.TryChoose();

        // Assert
        Assert.IsTrue(chosen);
        Assert.IsNotNull(picker.ChosenTheme);
        Assert.EndsWith(" copy", picker.ChosenName);
    }

    [TestMethod]
    public void TryChoose_NothingSelected_ReturnsFalse()
    {
        // Stage
        var picker = new ThemeSeedPickerViewModel(null, TempDirectory()) { SelectedIndex = -1 };

        // Test
        var chosen = picker.TryChoose();

        // Assert
        Assert.IsFalse(chosen);
    }

    [TestMethod]
    public void Delete_BuiltInTheme_Refuses()
    {
        // Stage
        var picker = new ThemeSeedPickerViewModel(new FixedConfirm(true), TempDirectory());

        // Test
        picker.Delete.Execute(null);

        // Assert
        Assert.AreEqual("Built-in themes can't be deleted.", picker.Message);
    }

    [TestMethod]
    public void Edit_PickedColor_MarksTheRoleOverridden()
    {
        // Stage
        var directory = TempDirectory();
        try
        {
            var seed = ActiveTheme.Catalog.Resolve(BuiltInThemes.DarkName, out _);
            var builder = new ThemeBuilderDialogViewModel(new ThemeBuilderState(seed, "Mine"), new FixedColorPicker("1,2,3"), null, directory)
            {
                SelectedRoleIndex = 0,
            };

            // Test
            builder.Edit.Execute(null);

            // Assert
            StringAssert.StartsWith(builder.Roles[0].Row, "●");
            StringAssert.Contains(builder.Roles[0].Row, "#010203");
        }
        finally
        {
            ActiveTheme.CancelPreview();
            Directory.Delete(directory, true);
        }
    }

    [TestMethod]
    public void TrySave_ReservedName_ReportsAnError()
    {
        // Stage
        var directory = TempDirectory();
        try
        {
            var seed = ActiveTheme.Catalog.Resolve(BuiltInThemes.DarkName, out _);
            var builder = new ThemeBuilderDialogViewModel(new ThemeBuilderState(seed, "Mine"), null, null, directory) { Name = BuiltInThemes.DarkName };

            // Test
            var saved = builder.TrySave();

            // Assert
            Assert.IsFalse(saved);
            StringAssert.Contains(builder.Error, "reserved");
        }
        finally
        {
            ActiveTheme.CancelPreview();
            Directory.Delete(directory, true);
        }
    }

    [TestMethod]
    public void TrySave_NewName_WritesTheFileAndSelectsIt()
    {
        // Stage
        var directory = TempDirectory();
        try
        {
            var seed = ActiveTheme.Catalog.Resolve(BuiltInThemes.DarkName, out _);
            var builder = new ThemeBuilderDialogViewModel(new ThemeBuilderState(seed, "Mine"), null, null, directory);

            // Test
            var saved = builder.TrySave();

            // Assert
            Assert.IsTrue(saved);
            Assert.IsTrue(File.Exists(Path.Combine(directory, "Mine.json")));
            Assert.IsTrue(ActiveTheme.Catalog.UserThemes.Any(theme => theme.Name == "Mine"));
        }
        finally
        {
            ActiveTheme.Reset();
            Directory.Delete(directory, true);
        }
    }

    [TestMethod]
    public void BuildThemeFromSeed_DialogCancelled_ReturnsWithoutSaving()
    {
        // Stage
        var directory = TempDirectory();
        try
        {
            var edited = false;
            var shell = new ShellViewModel([], themeBuilderDialog: new FakeThemeBuilderDialog(_ => false, _ => edited = true), themesDirectory: directory);

            // Test
            shell.BuildTheme.Execute(null);

            // Assert
            Assert.IsFalse(edited);
            Assert.AreEqual(0, Directory.GetFiles(directory).Length);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [TestMethod]
    public void BuildThemeFromSeed_SeedPicked_OpensTheBuilderOnACopy()
    {
        // Stage
        var directory = TempDirectory();
        try
        {
            string? builderName = null;
            var shell = new ShellViewModel([], themeBuilderDialog: new FakeThemeBuilderDialog(p => p.TryChoose(), b =>
            {
                builderName = b.Name;
                return false;
            }), themesDirectory: directory);

            // Test
            shell.BuildTheme.Execute(null);

            // Assert
            Assert.IsNotNull(builderName);
            Assert.EndsWith(" copy", builderName);
        }
        finally
        {
            ActiveTheme.CancelPreview();
            Directory.Delete(directory, true);
        }
    }
}
