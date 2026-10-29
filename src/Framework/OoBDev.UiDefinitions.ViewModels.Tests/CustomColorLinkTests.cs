using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary>The choice option linked to a colour picker button.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class CustomColorLinkTests
{
    private sealed class Model
    {
        public string Color { get; set; } = "Red";
    }

    private sealed class FakePicker(string? result) : IColorPicker
    {
        public int Opened { get; private set; }

        public string? Pick(string? current)
        {
            Opened++;
            return result;
        }
    }

    private static readonly UiDefinition Definition = new()
    {
        Name = "Light",
        Sections =
        [
            new UiSection
            {
                Label = "Color",
                Controls =
                [
                    new ChoiceControl { Id = "Color", Label = "Color", Style = ChoiceStyle.RadioGroup, Options = ["Red", "Custom", "Off"] },
                    new ButtonControl { Id = "customColor", Label = "Custom...", ColorPickerTargetCommandId = "Color", ColorPickerChoiceOption = "Custom" },
                ],
            },
        ],
    };

    private static ChoiceOptionViewModel CustomItem(FormViewModel form) => ((ChoiceViewModel)form.Row("Color")!).OptionItems[1];

    [TestMethod]
    public void Select_CustomWithoutPick_OpensPicker()
    {
        // Stage
        var model = new Model();
        var picker = new FakePicker("1,2,3");
        using var form = new FormViewModel(Definition, new FormBinding(model), picker);

        // Test
        CustomItem(form).Command.Execute("Custom");

        // Assert
        Assert.AreEqual(1, picker.Opened);
        Assert.AreEqual("1,2,3", model.Color);
        Assert.IsTrue(CustomItem(form).IsSelected);
    }

    [TestMethod]
    public void Select_CustomAfterPreset_ReappliesLastColourWithoutPicker()
    {
        // Stage
        var model = new Model();
        var picker = new FakePicker("1,2,3");
        using var form = new FormViewModel(Definition, new FormBinding(model), picker);
        var choice = (ChoiceViewModel)form.Row("Color")!;
        CustomItem(form).Command.Execute("Custom");
        choice.Select.Execute("Red");

        // Test
        CustomItem(form).Command.Execute("Custom");

        // Assert
        Assert.AreEqual(1, picker.Opened);
        Assert.AreEqual("1,2,3", model.Color);
    }

    [TestMethod]
    public void Pick_Button_SelectsCustomOption()
    {
        // Stage
        using var form = new FormViewModel(Definition, new FormBinding(new Model()), new FakePicker("9,9,9"));

        // Test
        ((ButtonViewModel)form.Row("customColor")!).Invoke.Execute(null);

        // Assert
        Assert.AreEqual("Custom", ((ChoiceViewModel)form.Row("Color")!).SelectedOption);
    }

    [TestMethod]
    public void Select_PresetOption_StaysSelectedAsPreset()
    {
        // Stage
        using var form = new FormViewModel(Definition, new FormBinding(new Model()), new FakePicker("9,9,9"));

        // Assert
        Assert.AreEqual("Red", ((ChoiceViewModel)form.Row("Color")!).SelectedOption);
        Assert.IsFalse(CustomItem(form).IsSelected);
    }

    [TestMethod]
    public void Select_NoPicker_LeavesDefaultCommand()
    {
        // Stage
        using var form = new FormViewModel(Definition, new FormBinding(new Model()));

        // Assert
        Assert.IsInstanceOfType<SelectOptionCommand>(CustomItem(form).Command);
    }
}
