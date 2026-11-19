using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary>A choice row re-reads the option list the model names in <see cref="ChoiceControl.OptionsSource"/>.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ChoiceDynamicOptionsTests
{
    private sealed class Model : INotifyPropertyChanged
    {
        private IReadOnlyList<string> _ports = ["COM1"];

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Port { get; set; } = "COM1";

        public IReadOnlyList<string> Ports
        {
            get => _ports;
            set
            {
                _ports = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ports)));
            }
        }
    }

    private static FormViewModel Form(Model model, string? source = nameof(Model.Ports)) => new(
        new UiDefinition
        {
            Name = "t",
            Sections = [new UiSection { Label = "s", Controls = [new ChoiceControl { Id = nameof(Model.Port), Label = "Port", Options = ["COM1"], OptionsSource = source }] }],
        },
        new FormBinding(model));

    [TestMethod]
    public void Refresh_OptionsSourceChanged_ReplacesTheOptionsAndRaisesCollectionChanged()
    {
        // Stage
        var model = new Model();
        using var form = Form(model);
        var row = (ChoiceViewModel)form.Row("Port")!;
        var raised = 0;
        ((INotifyCollectionChanged)row.OptionItems).CollectionChanged += (_, _) => raised++;

        // Test
        model.Ports = ["COM1", "COM6", "COM7"];

        // Assert
        CollectionAssert.AreEqual(new[] { "COM1", "COM6", "COM7" }, row.Options.ToArray());
        CollectionAssert.AreEqual(new[] { "COM1", "COM6", "COM7" }, row.OptionItems.Select(o => o.Text).ToArray());
        Assert.IsTrue(raised > 0);
        Assert.IsTrue(row.OptionItems.Single(o => o.Text == "COM1").IsSelected);
    }

    [TestMethod]
    public void Select_AnOptionThatAppearedLater_WritesTheModel()
    {
        // Stage
        var model = new Model();
        using var form = Form(model);
        var row = (ChoiceViewModel)form.Row("Port")!;
        model.Ports = ["COM1", "COM6"];

        // Test
        row.Select.Execute("COM6");

        // Assert
        Assert.AreEqual("COM6", model.Port);
    }

    [TestMethod]
    public void Refresh_NoOptionsSource_KeepsTheFixedList()
    {
        // Stage
        var model = new Model();
        using var form = Form(model, source: null);
        var row = (ChoiceViewModel)form.Row("Port")!;

        // Test
        model.Ports = ["COM9"];

        // Assert
        CollectionAssert.AreEqual(new[] { "COM1" }, row.Options.ToArray());
    }

    [TestMethod]
    public void GetOptions_MissingProperty_ReturnsNull()
    {
        // Stage / Test / Assert
        using var binding = new FormBinding(new Model());
        Assert.IsNull(binding.GetOptions("Nope"));
        CollectionAssert.AreEqual(new[] { "COM1" }, binding.GetOptions(nameof(Model.Ports))!.ToArray());
    }
}
