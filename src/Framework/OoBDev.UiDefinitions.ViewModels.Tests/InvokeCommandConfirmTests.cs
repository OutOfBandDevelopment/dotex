using System;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary>A button with a confirm message asks before it runs.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class InvokeCommandConfirmTests
{
    private sealed class Counter : ICommand
    {
        public int Runs { get; private set; }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => Runs++;
    }

    private sealed class Model
    {
        public Counter Wipe { get; } = new();
    }

    private sealed class Answer(bool agree) : IConfirmPrompt
    {
        public string? Asked { get; private set; }

        public bool Confirm(string message)
        {
            Asked = message;
            return agree;
        }
    }

    private static ButtonControl Button() => new() { Id = "Wipe", Label = "Wipe", ConfirmMessage = "Erase everything?" };

    [TestMethod]
    public void Execute_ConfirmMessageAgreed_RunsTheCommand()
    {
        // Stage
        var model = new Model();
        var answer = new Answer(true);
        var command = new InvokeCommand(new FormBinding(model), Button(), null, answer);

        // Test
        command.Execute(null);

        // Assert
        Assert.AreEqual("Erase everything?", answer.Asked);
        Assert.AreEqual(1, model.Wipe.Runs);
    }

    [TestMethod]
    public void Execute_ConfirmMessageDeclined_DoesNotRun()
    {
        // Stage
        var model = new Model();
        var command = new InvokeCommand(new FormBinding(model), Button(), null, new Answer(false));

        // Test
        command.Execute(null);

        // Assert
        Assert.AreEqual(0, model.Wipe.Runs);
    }

    [TestMethod]
    public void Execute_NoPrompt_RunsWithoutAsking()
    {
        // Stage
        var model = new Model();
        var command = new InvokeCommand(new FormBinding(model), Button());

        // Test
        command.Execute(null);

        // Assert
        Assert.AreEqual(1, model.Wipe.Runs);
    }
}
