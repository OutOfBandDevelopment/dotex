using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary>A button that packs sibling field values into the command parameter.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class InvokeCommandParameterTests
{
    private sealed class Capture : ICommand
    {
        public object? Last { get; private set; }

        public bool Allow { get; set; } = true;

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => Allow;

        public void Execute(object? parameter) => Last = parameter;
    }

    private sealed class Model
    {
        public string Volts { get; set; } = "5";

        public int Amps { get; set; } = 2;

        public Capture Apply { get; } = new();
    }

    private static readonly List<UiControl> Siblings =
    [
        new TextFieldControl { Id = "Volts", Label = "Volts" },
        new TextFieldControl { Id = "Amps", Label = "Amps" },
    ];

    [TestMethod]
    public void Execute_ParameterFieldIds_PassesJoinedValues()
    {
        // Stage
        var model = new Model();
        var button = new ButtonControl { Id = "Apply", Label = "Apply", ParameterFieldIds = ["Volts", "Amps"] };
        var command = new InvokeCommand(new FormBinding(model), button, Siblings);

        // Test
        command.Execute(null);

        // Assert
        Assert.AreEqual("5,2", model.Apply.Last);
    }

    [TestMethod]
    public void Execute_NoParameterFieldIds_PassesNull()
    {
        // Stage
        var model = new Model();
        var command = new InvokeCommand(new FormBinding(model), new ButtonControl { Id = "Apply", Label = "Apply" }, Siblings);

        // Test
        command.Execute(null);

        // Assert
        Assert.IsNull(model.Apply.Last);
    }

    [TestMethod]
    public void Execute_UnknownSibling_PassesEmptySlot()
    {
        // Stage
        var model = new Model();
        var button = new ButtonControl { Id = "Apply", Label = "Apply", ParameterFieldIds = ["Volts", "Missing"] };
        var command = new InvokeCommand(new FormBinding(model), button, Siblings);

        // Test
        command.Execute(null);

        // Assert
        Assert.AreEqual("5,", model.Apply.Last);
    }

    [TestMethod]
    public void Execute_ParameterExpression_TransformsValue()
    {
        // Stage
        var model = new Model();
        var button = new ButtonControl { Id = "Apply", Label = "Apply", ParameterFieldIds = ["Volts", "Amps"], ParameterExpressions = ["{Volts} * 2", null] };
        var command = new InvokeCommand(new FormBinding(model), button, Siblings);

        // Test
        command.Execute(null);

        // Assert
        Assert.AreEqual("10,2", model.Apply.Last);
    }

    [TestMethod]
    public void Execute_BadExpression_KeepsBareValue()
    {
        // Stage
        var model = new Model();
        var button = new ButtonControl { Id = "Apply", Label = "Apply", ParameterFieldIds = ["Volts"], ParameterExpressions = ["{Volts"] };
        var command = new InvokeCommand(new FormBinding(model), button, Siblings);

        // Test
        command.Execute(null);

        // Assert
        Assert.AreEqual("5", model.Apply.Last);
    }

    [TestMethod]
    public void CanExecute_ModelRefuses_ReturnsFalse()
    {
        // Stage
        var model = new Model();
        model.Apply.Allow = false;
        var button = new ButtonControl { Id = "Apply", Label = "Apply", ParameterFieldIds = ["Volts"] };
        var command = new InvokeCommand(new FormBinding(model), button, Siblings);

        // Test
        var result = command.CanExecute(null);

        // Assert
        Assert.IsFalse(result);
    }
}
