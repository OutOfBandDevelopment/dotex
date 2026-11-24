using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.Tests;

/// <summary>The <see cref="ListControl"/> kind: generated from a model, selected through the binding, and kept through serialization.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ListControlTests
{
    public sealed record Profile(string Name)
    {
        public override string ToString() => Name;
    }

    public sealed class Profiles
    {
        public List<Profile> Saved { get; } = [new("bench"), new("lab")];

        [FormField(Kind = FormFieldKind.List, OptionsFrom = nameof(Saved), AddCommand = nameof(Add), DeleteCommand = nameof(Delete))]
        public Profile? Selected { get; set; }

        [FormField]
        public ICommand? Add { get; set; }

        [FormField]
        public ICommand? Delete { get; set; }
    }

    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void Generate_ListField_BecomesAListControlWithItemsAndCommands()
    {
        var definition = FormDefinitionGenerator.Generate(new Profiles(), "Profiles");

        var list = definition.Sections.SelectMany(s => s.Controls).OfType<ListControl>().Single();
        TestContext.WriteLine("list {0}: items {1}, add {2}, delete {3}", list.Id, string.Join(",", list.Items), list.AddCommand, list.DeleteCommand);
        Assert.AreEqual("Selected", list.Id);
        Assert.AreEqual("Saved", list.ItemsSource);
        Assert.AreSequenceEqual(["bench", "lab"], list.Items);
        Assert.AreEqual("Add", list.AddCommand);
        Assert.AreEqual("Delete", list.DeleteCommand);
        Assert.IsTrue(definition.Sections.SelectMany(s => s.Controls).OfType<ButtonControl>().Any(b => b.Id == "Add"), "the add command is a button");
    }

    [TestMethod]
    public void SetText_ListSelection_WritesTheItemItself()
    {
        var model = new Profiles();
        var binding = new FormBinding(model);
        var list = FormDefinitionGenerator.Generate(model, "Profiles").Sections.SelectMany(s => s.Controls).OfType<ListControl>().Single();

        binding.SetText(list, "lab");

        Assert.AreSame(model.Saved[1], model.Selected);
        model.Saved.Add(new Profile("new"));
        Assert.AreSequenceEqual(["bench", "lab", "new"], binding.GetOptions("Saved")!);
    }

    [TestMethod]
    public void Serializer_ListControl_RoundTripsAsList()
    {
        var definition = FormDefinitionGenerator.Generate(new Profiles(), "Profiles");

        var json = UiDefinitionSerializer.ToJson(definition);
        var back = UiDefinitionSerializer.FromJson(json);

        StringAssert.Contains(json, "\"kind\": \"list\"");
        var list = back.Sections.SelectMany(s => s.Controls).OfType<ListControl>().Single();
        Assert.AreEqual("Delete", list.DeleteCommand);
    }
}
