using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.Panels;

/// <summary>Turns a <see cref="UiDefinition"/> and the <see cref="IControlSurface"/> that drives the device into a form-ready panel, through a generated model as the SCPI panels do.</summary>
public static class SurfacePanelBuilder
{
    /// <summary>Builds the panel. Toggles, sliders, numbers, choices and text fields send their value to the surface when it changes; a button sends its command id with the parameter text. Live displays (indicators, bar graphs, charts, vectors) are left out until the form view model supports them.</summary>
    /// <param name="definition">The definition.</param>
    /// <param name="surface">The surface that sends to the device.</param>
    /// <param name="onError">Receives a failed send, or null to ignore failures.</param>
    /// <returns>The panel.</returns>
    public static SurfacePanel Build(UiDefinition definition, IControlSurface surface, Action<Exception>? onError = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(surface);

        var properties = new List<DynamicProperty>();
        var sending = new Dictionary<string, UiControl>(StringComparer.Ordinal);
        var sections = new List<UiSection>();
        foreach (var section in definition.Sections)
        {
            var kept = new List<UiControl>();
            foreach (var control in section.Controls)
            {
                if (Property(control, surface, onError) is { } property)
                {
                    properties.Add(property);
                    kept.Add(control);
                    if (control is not ButtonControl)
                    {
                        sending[control.Id] = control;
                    }
                }
            }

            sections.Add(new UiSection { Label = section.Label, VisibleWhen = section.VisibleWhen, Controls = kept });
        }

        var binding = new FormBinding(DynamicModelFactory.Create(properties));
        var filtered = new UiDefinition { Name = definition.Name, Description = definition.Description, Sections = sections };
        return new SurfacePanel(filtered, binding, sending, surface, onError);
    }

    private static DynamicProperty? Property(UiControl control, IControlSurface surface, Action<Exception>? onError) => control switch
    {
        ToggleControl toggle => new DynamicProperty(control.Id, typeof(bool), toggle.DefaultValue),
        SliderControl slider => new DynamicProperty(control.Id, typeof(double), slider.DefaultValue),
        NumericControl numeric => new DynamicProperty(control.Id, typeof(double), numeric.DefaultValue),
        ChoiceControl choice => new DynamicProperty(control.Id, typeof(string), choice.DefaultValue ?? choice.Options.FirstOrDefault() ?? string.Empty),
        TextFieldControl text => new DynamicProperty(control.Id, typeof(string), text.DefaultValue ?? string.Empty),
        ButtonControl button => new DynamicProperty(control.Id, typeof(ICommand), new SurfaceCommand(surface, button.CommandId ?? button.Id, onError)),
        _ => null,
    };
}
