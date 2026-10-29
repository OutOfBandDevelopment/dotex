using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.Wpf.Tests;

/// <summary>The XAML form view against real view models; needs an STA thread, no window is shown.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class FormViewTests
{
    public sealed class Settings
    {
        public bool Enabled { get; set; }

        public string Mode { get; set; } = "Low";

        public string Tags { get; set; } = "x";

        public string Name { get; set; } = "a";
    }

    private static UiDefinition Definition() => new()
    {
        Name = "t",
        Sections =
        [
            new UiSection
            {
                Label = "Main",
                Controls =
                [
                    new ToggleControl { Id = "Enabled", Label = "Enabled" },
                    new TextFieldControl { Id = "Name", Label = "Name", VisibleWhen = new UiCondition { Id = "Enabled" } },
                    new ChoiceControl { Id = "Mode", Label = "Mode", Options = ["Low", "High"], Style = ChoiceStyle.RadioGroup },
                    new ChoiceControl { Id = "Tags", Label = "Tags", Options = ["x", "y"], Style = ChoiceStyle.CheckList },
                ],
            },
        ],
    };

    private static FormView Render(FormViewModel form)
    {
        var view = new FormView { DataContext = form };
        view.Measure(new Size(600, 600));
        view.Arrange(new Rect(0, 0, 600, 600));
        view.UpdateLayout();
        return view;
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
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

    [STATestMethod]
    public void Render_Form_ShowsToggleRadiosAndChecks()
    {
        // Stage
        var settings = new Settings();
        using var form = new FormViewModel(Definition(), new FormBinding(settings));

        // Test
        var view = Render(form);

        // Assert
        Assert.AreEqual(2, Descendants<RadioButton>(view).Count());
        Assert.AreEqual(3, Descendants<CheckBox>(view).Count());
    }

    [STATestMethod]
    public void Click_Toggle_WritesModelThroughCommand()
    {
        // Stage
        var settings = new Settings();
        using var form = new FormViewModel(Definition(), new FormBinding(settings));
        var view = Render(form);
        var toggle = Descendants<CheckBox>(view).First();

        // Test
        toggle.Command.Execute(toggle.CommandParameter);

        // Assert
        Assert.IsTrue(settings.Enabled);
    }

    [STATestMethod]
    public void Row_ConditionNotMet_IsCollapsed()
    {
        // Stage
        var settings = new Settings();
        using var form = new FormViewModel(Definition(), new FormBinding(settings));
        var view = Render(form);

        // Test
        var before = Descendants<TextBox>(view).Any(IsShown);
        ((ToggleViewModel)form.Row("Enabled")!).Toggle.Execute(true);
        view.UpdateLayout();

        // Assert
        Assert.IsFalse(before);
        Assert.IsTrue(Descendants<TextBox>(view).Any(IsShown));
    }

    public sealed class LevelModel
    {
        public int Level { get; set; }
    }

    [STATestMethod]
    public void Slide_SliderRow_WritesModel()
    {
        // Stage
        var model = new LevelModel();
        var definition = new UiDefinition
        {
            Name = "s",
            Sections = [new UiSection { Label = "S", Controls = [new SliderControl { Id = "Level", Label = "Level", Minimum = 0, Maximum = 10, Step = 1 }] }],
        };
        using var form = new FormViewModel(definition, new FormBinding(model));
        var view = Render(form);
        var slider = Descendants<Slider>(view).Single();

        // Test
        slider.Value = 6;

        // Assert
        Assert.AreEqual(6, model.Level);
    }

    private static bool IsShown(UIElement element) => IsVisibleInTree(element);

    private static bool IsVisibleInTree(DependencyObject element)
    {
        for (DependencyObject? current = element; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is UIElement { Visibility: not Visibility.Visible })
            {
                return false;
            }
        }

        return true;
    }
}
