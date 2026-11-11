using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Examples.Controls.WinForms.Tests;

/// <summary>The generated settings form, created without showing it.</summary>
[TestCategory("Unit")]
[STATestClass]
public sealed class SettingsFormTests
{
    [TestMethod]
    public void Constructor_Settings_RendersAnEditorPerControl()
    {
        // Stage
        var settings = new ControlsSampleSettings();

        // Test
        using var form = new SettingsForm(settings);

        // Assert
        Assert.IsInstanceOfType<CheckBox>(form.View.Editors["Enabled"]);
        Assert.IsInstanceOfType<CheckBox>(form.View.Editors["ShowAdvanced"]);
        Assert.IsInstanceOfType<TextBox>(form.View.Editors["Name"]);
    }
}
