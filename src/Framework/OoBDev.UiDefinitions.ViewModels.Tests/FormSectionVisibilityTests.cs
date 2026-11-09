using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary>A section whose visibility depends on a model value tells its view when that value changes.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class FormSectionVisibilityTests
{
    private sealed class Model
    {
        public bool ShowAdvanced { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    [TestMethod]
    public void ToggleCommand_ConditionalSection_RaisesHasVisibleRowsAndShowsIt()
    {
        // Stage
        var definition = new UiDefinition
        {
            Name = "n",
            Sections =
            [
                new UiSection { Label = "Main", Controls = [new ToggleControl { Id = "ShowAdvanced", Label = "Show advanced" }] },
                new UiSection
                {
                    Label = "Advanced",
                    VisibleWhen = new UiCondition { Id = "ShowAdvanced" },
                    Controls = [new TextFieldControl { Id = "Name", Label = "Name" }],
                },
            ],
        };
        using var form = new FormViewModel(definition, new FormBinding(new Model()));
        var advanced = form.Sections[1];
        var raised = new List<string?>();
        advanced.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        Assert.IsFalse(advanced.HasVisibleRows);

        // Test
        ((ToggleViewModel)form.Row("ShowAdvanced")!).Toggle.Execute(null);

        // Assert
        CollectionAssert.Contains(raised, nameof(FormSectionViewModel.HasVisibleRows));
        Assert.IsTrue(advanced.HasVisibleRows);
    }
}
