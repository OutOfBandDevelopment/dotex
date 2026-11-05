using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary>A button's "Sends" preview follows the field values its parameter reads.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ButtonPreviewTests
{
    private sealed class Noop : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
        }
    }

    private sealed class Model
    {
        public string Volts { get; set; } = "5";

        public Noop Apply { get; } = new();
    }

    private sealed class Preview : ICommandPreview
    {
        public string? PreviewCommand(string commandId, string? value) => $"{commandId} {value}";
    }

    private static FormViewModel Build(Model model, string? description = null)
    {
        var volts = new TextFieldControl { Id = "Volts", Label = "Volts" };
        var apply = new ButtonControl { Id = "Apply", Label = "Apply", CommandId = "Apply", Description = description, ParameterFieldIds = ["Volts"] };
        var definition = new UiDefinition { Name = "p", Sections = [new UiSection { Label = "Main", Controls = [volts, apply] }] };
        return new FormViewModel(definition, new FormBinding(model), preview: new Preview());
    }

    [TestMethod]
    public void ToolTip_FieldEdited_FollowsTheNewValue()
    {
        // Stage
        var model = new Model();
        using var form = Build(model);
        var button = (ButtonViewModel)form.Row("Apply")!;
        var field = (TextViewModel)form.Row("Volts")!;
        var changed = new List<string?>();
        button.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        // Test
        field.Text = "12";

        // Assert
        Assert.AreEqual("Sends: Apply 12", button.ToolTip);
        CollectionAssert.Contains(changed, nameof(ButtonViewModel.ToolTip));
    }

    [TestMethod]
    public void ToolTip_ButtonWithDescription_PutsTheSendsLineAfterIt()
    {
        // Stage
        using var form = Build(new Model(), "Applies the voltage");
        var button = (ButtonViewModel)form.Row("Apply")!;

        // Test
        var tip = button.ToolTip;

        // Assert
        Assert.AreEqual($"Applies the voltage{Environment.NewLine}Sends: Apply 5", tip);
    }

    [TestMethod]
    public void ToolTip_NoPreview_IsTheDescription()
    {
        // Stage
        var apply = new ButtonControl { Id = "Apply", Label = "Apply", Description = "Applies" };
        var button = new ButtonViewModel(apply, new FormBinding(new Model()));

        // Test
        var tip = button.ToolTip;

        // Assert
        Assert.AreEqual("Applies", tip);
        Assert.IsNull(button.Sends);
    }
}
