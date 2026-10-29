using System.Linq;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.Wpf.Tests;

/// <summary>The colour dialog bindings.</summary>
[TestClass]
public sealed class ColorPickerWindowTests
{
    [STATestMethod]
    public void Accept_Command_ClosesAccepted()
    {
        // Stage
        var model = new ColorPickerViewModel("1,2,3");
        var window = new ColorPickerWindow(model);

        // Test
        model.Accept.Execute(null);

        // Assert
        Assert.IsTrue(window.Accepted);
    }

    [STATestMethod]
    public void Cancel_Command_ClosesNotAccepted()
    {
        // Stage
        var model = new ColorPickerViewModel("1,2,3");
        var window = new ColorPickerWindow(model);

        // Test
        model.Cancel.Execute(null);

        // Assert
        Assert.IsFalse(window.Accepted);
    }

    [STATestMethod]
    public void Sliders_Bound_WriteChannels()
    {
        // Stage
        var model = new ColorPickerViewModel(null);
        var window = new ColorPickerView { DataContext = model };
        window.Measure(new System.Windows.Size(400, 400));
        window.Arrange(new System.Windows.Rect(0, 0, 400, 400));
        window.UpdateLayout();
        var sliders = Descendants<Slider>(window).ToList();

        // Test
        sliders[0].Value = 200;

        // Assert
        Assert.HasCount(3, sliders);
        Assert.AreEqual(200, model.Red);
    }

    private static System.Collections.Generic.IEnumerable<T> Descendants<T>(System.Windows.DependencyObject root)
        where T : System.Windows.DependencyObject
    {
        foreach (var child in System.Windows.LogicalTreeHelper.GetChildren(root).OfType<System.Windows.DependencyObject>())
        {
            if (child is T match)
            {
                yield return match;
            }

            foreach (var inner in Descendants<T>(child))
            {
                yield return inner;
            }
        }
    }
}
