using Avalonia.Controls;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.Avalonia.Tests;

/// <summary>Builds the demo definition, view model and window content.</summary>
public static class DemoForm
{
    /// <summary>The shared, UI-neutral definition of the demo form.</summary>
    public static UiDefinition Definition() => new()
    {
        Name = "spike",
        Sections =
        [
            new UiSection
            {
                Label = "Main",
                Controls =
                [
                    new ToggleControl { Id = "Advanced", Label = "Advanced" },
                    new TextFieldControl { Id = "Name", Label = "Name", VisibleWhen = new UiCondition { Id = "Advanced" } },
                    new SliderControl { Id = "Level", Label = "Level", Minimum = 0, Maximum = 10 },
                    new ChoiceControl { Id = "Mode", Label = "Mode", Options = ["Low", "High"] },
                ],
            },
        ],
    };

    /// <summary>A window showing the form bound to <paramref name="model"/>.</summary>
    public static Window CreateWindow(DemoModel model) => new()
    {
        Title = "OoBDev UiDefinition spike",
        Width = 400,
        Height = 300,
        Content = FormView.Create(new FormViewModel(Definition(), new FormBinding(model))),
    };
}
