using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.Tests;

/// <summary>Models built from a property list, bound through <see cref="FormBinding"/>.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class DynamicModelFactoryTests
{
    [TestMethod]
    public void Create_Properties_ReadsInitialValues()
    {
        // Stage
        var model = DynamicModelFactory.Create([new("Volts", typeof(double), 5.0), new("Name", typeof(string), "psu"), new("On", typeof(bool))]);

        // Test
        var binding = new FormBinding(model);

        // Assert
        Assert.AreEqual("5", binding.GetText("Volts"));
        Assert.AreEqual("psu", binding.GetText("Name"));
        Assert.IsFalse(binding.GetBool("On"));
    }

    [TestMethod]
    public void SetText_DynamicNumber_ConvertsAndWrites()
    {
        // Stage
        var model = DynamicModelFactory.Create([new("Volts", typeof(double))]);
        var binding = new FormBinding(model);
        var changed = 0;
        binding.Changed += (_, _) => changed++;

        // Test
        var result = binding.SetText(new TextFieldControl { Id = "Volts", Label = "V", Constraint = new ValueConstraint { Kind = ValueKind.Number } }, "12.5");

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual("12.5", binding.GetText("Volts"));
        Assert.AreEqual(1, changed);
    }

    [TestMethod]
    public void Invoke_DynamicCommand_RunsIt()
    {
        // Stage
        var ran = false;
        var model = DynamicModelFactory.Create([new("Go", typeof(System.Windows.Input.ICommand), new Run(() => ran = true))]);

        // Test
        var invoked = new FormBinding(model).Invoke("Go");

        // Assert
        Assert.IsTrue(invoked);
        Assert.IsTrue(ran);
    }

    [TestMethod]
    public void Create_DuplicateName_Throws()
    {
        // Assert
        Assert.ThrowsExactly<ArgumentException>(() => DynamicModelFactory.Create([new("A", typeof(int)), new("A", typeof(int))]));
    }

    [TestMethod]
    public void Create_TwoCalls_GiveIndependentTypes()
    {
        // Stage
        var first = DynamicModelFactory.Create([new("A", typeof(int), 1)]);
        var second = DynamicModelFactory.Create([new("A", typeof(string), "x")]);

        // Assert
        Assert.AreNotEqual(first.GetType(), second.GetType());
        Assert.AreEqual("1", new FormBinding(first).GetText("A"));
        Assert.AreEqual("x", new FormBinding(second).GetText("A"));
    }

    private sealed class Run(Action action) : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => action();
    }
}
