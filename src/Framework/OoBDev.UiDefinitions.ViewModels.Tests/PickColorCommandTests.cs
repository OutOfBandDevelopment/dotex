using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary>The colour picker button: dialog result to model text.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class PickColorCommandTests
{
    private sealed class Model
    {
        public string Colour { get; set; } = "1,2,3";
    }

    private sealed class FakePicker(string? result) : IColorPicker
    {
        public string? Current { get; private set; }

        public string? Pick(string? current)
        {
            Current = current;
            return result;
        }
    }

    private static readonly TextFieldControl Target = new() { Id = "Colour", Label = "Colour" };

    [TestMethod]
    public void Execute_Picked_WritesColourToTarget()
    {
        // Stage
        var model = new Model();
        var picker = new FakePicker("10,20,30");
        var command = new PickColorCommand(new FormBinding(model), Target, picker);

        // Test
        command.Execute(null);

        // Assert
        Assert.AreEqual("10,20,30", model.Colour);
        Assert.AreEqual("10,20,30", command.LastColor);
        Assert.AreEqual("1,2,3", picker.Current);
    }

    [TestMethod]
    public void Execute_Cancelled_LeavesTarget()
    {
        // Stage
        var model = new Model();
        var command = new PickColorCommand(new FormBinding(model), Target, new FakePicker(null));

        // Test
        command.Execute(null);

        // Assert
        Assert.AreEqual("1,2,3", model.Colour);
        Assert.IsNull(command.LastColor);
    }

    [TestMethod]
    public void Execute_Twice_OffersLastPickedColour()
    {
        // Stage
        var picker = new FakePicker("9,9,9");
        var command = new PickColorCommand(new FormBinding(new Model()), Target, picker);

        // Test
        command.Execute(null);
        command.Execute(null);

        // Assert
        Assert.AreEqual("9,9,9", picker.Current);
    }

    [TestMethod]
    public void Button_ColourPickerTarget_UsesPickCommand()
    {
        // Stage
        var button = new ButtonControl { Id = "pick", Label = "Pick", ColorPickerTargetCommandId = "Colour" };
        var model = new Model();
        var siblings = new List<UiControl> { Target, button };

        // Test
        var withPicker = new ButtonViewModel(button, new FormBinding(model), new FakePicker("5,6,7"), siblings);
        var without = new ButtonViewModel(button, new FormBinding(model), null, siblings);
        withPicker.Invoke.Execute(null);

        // Assert
        Assert.AreEqual("5,6,7", model.Colour);
        Assert.IsInstanceOfType<PickColorCommand>(withPicker.Invoke);
        Assert.IsInstanceOfType<InvokeCommand>(without.Invoke);
    }
}
