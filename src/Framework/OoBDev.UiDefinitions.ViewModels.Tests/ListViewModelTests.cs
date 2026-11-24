using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class ListViewModelTests
{
    public sealed record Profile(string Name)
    {
        public override string ToString() => Name;
    }

    public sealed class Model
    {
        public List<Profile> Saved { get; } = [new("bench"), new("lab")];

        [FormField(Kind = FormFieldKind.List, OptionsFrom = nameof(Saved))]
        public Profile? Selected { get; set; }
    }

    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void SelectedItem_Set_WritesTheModelAndRefreshPicksUpAddedItems()
    {
        var model = new Model();
        var binding = new FormBinding(model);
        var control = FormDefinitionGenerator.Generate(model, "m").Sections.SelectMany(s => s.Controls).OfType<ListControl>().Single();
        var vm = (ListViewModel)ControlViewModelFactory.Create(control, binding);

        vm.SelectedItem = "lab";
        model.Saved.Add(new Profile("new"));
        vm.Refresh();

        TestContext.WriteLine("items {0}; selected {1}", string.Join(",", vm.Items), vm.SelectedItem);
        Assert.AreSame(model.Saved[1], model.Selected);
        Assert.AreEqual("lab", vm.SelectedItem);
        Assert.AreSequenceEqual(["bench", "lab", "new"], [.. vm.Items]);
    }
}
