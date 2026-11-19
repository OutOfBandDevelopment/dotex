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

    private sealed record Device(string Display, string Host);

    private sealed class DeviceModel
    {
        public IReadOnlyList<Device> Found { get; set; } = [new("scope  [192.168.0.5]", "192.168.0.5"), new("psu  [192.168.0.6]", "192.168.0.6")];

        public Device? Picked { get; set; }
    }

    [TestMethod]
    public void Select_ObjectValuedChoice_ShowsDisplayTextAndWritesTheItem()
    {
        // Stage
        var model = new DeviceModel();
        var control = new ChoiceControl { Id = nameof(DeviceModel.Picked), Label = "Device", OptionsSource = nameof(DeviceModel.Found), Options = [] };
        using var form = new FormViewModel(new UiDefinition { Name = "t", Sections = [new UiSection { Label = "s", Controls = [control] }] }, new FormBinding(model));
        var row = (ChoiceViewModel)form.Row("Picked")!;

        // Test
        row.Select.Execute("psu  [192.168.0.6]");

        // Assert
        CollectionAssert.AreEqual(new[] { "scope  [192.168.0.5]", "psu  [192.168.0.6]" }, row.Options.ToArray());
        Assert.AreSame(model.Found[1], model.Picked);
        Assert.AreEqual("psu  [192.168.0.6]", row.SelectedOption);
    }

    [TestMethod]
    public void UsesDropdown_DynamicOrLongLists_ButNotShortFixedOnes()
    {
        // Stage
        using var dynamicForm = Form(new Model());
        using var fixedForm = Form(new Model(), source: null);
        var longFixed = new ChoiceControl { Id = nameof(Model.Port), Label = "Port", Options = [.. Enumerable.Range(1, 7).Select(i => $"COM{i}")] };
        using var longForm = new FormViewModel(new UiDefinition { Name = "t", Sections = [new UiSection { Label = "s", Controls = [longFixed] }] }, new FormBinding(new Model()));

        // Test / Assert
        Assert.IsTrue(((ChoiceViewModel)dynamicForm.Row("Port")!).UsesDropdown, "A list that can change is a drop-down.");
        Assert.IsFalse(((ChoiceViewModel)fixedForm.Row("Port")!).UsesDropdown, "A short fixed list stays radio buttons.");
        Assert.IsTrue(((ChoiceViewModel)longForm.Row("Port")!).UsesDropdown, "A fixed list over the threshold is a drop-down.");
    }

    [TestMethod]
    public void PickedOption_SetToAListedOption_WritesTheModel_AndIgnoresOthers()
    {
        // Stage
        var model = new Model { Ports = ["COM1", "COM9"] };
        using var form = Form(model);
        var row = (ChoiceViewModel)form.Row("Port")!;

        // Test
        row.PickedOption = "COM9";
        row.PickedOption = "COM77";
        row.PickedOption = null;

        // Assert
        Assert.AreEqual("COM9", model.Port);
    }
}
