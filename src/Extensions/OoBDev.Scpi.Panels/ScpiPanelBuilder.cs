using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.Scpi.Panels;

/// <summary>Turns an instrument profile's commands into a control panel: a section per category, a field per parameter, a button per command and a reply field per query.</summary>
public static class ScpiPanelBuilder
{
    /// <summary>Builds the panel.</summary>
    /// <param name="profile">The instrument profile.</param>
    /// <param name="execute">Sends a request; returns the reply text for a query, or null (for example <c>tracker.QueryAsync</c> mapped to its text).</param>
    /// <returns>The form, its binding and the model behind it.</returns>
    public static ScpiPanel Build(ScpiInstrumentProfile profile, Func<ScpiRequest, Task<string?>> execute)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(execute);

        var properties = new List<DynamicProperty>();
        var commands = new List<ScpiPanelCommand>();
        var sections = new List<UiSection>();
        foreach (var group in profile.Commands.GroupBy(c => c.Category, StringComparer.Ordinal))
        {
            var section = new UiSection { Label = group.Key };
            foreach (var command in group)
            {
                AddCommand(profile, command, execute, section, properties, commands);
            }

            sections.Add(section);
        }

        var model = DynamicModelFactory.Create(properties);
        var binding = new FormBinding(model);
        foreach (var command in commands)
        {
            command.Binding = binding;
        }

        return new ScpiPanel(new UiDefinition { Name = profile.Name, Description = profile.Notes, Sections = sections }, binding, model);
    }

    private static void AddCommand(
        ScpiInstrumentProfile profile,
        ScpiCommandDefinition command,
        Func<ScpiRequest, Task<string?>> execute,
        UiSection section,
        List<DynamicProperty> properties,
        List<ScpiPanelCommand> commands)
    {
        var fieldIds = new List<string>();
        foreach (var parameter in command.Parameters)
        {
            var id = ScpiPanelIds.Field(command.Id, parameter.Name);
            fieldIds.Add(id);
            var (control, type, initial) = Field(id, parameter);
            section.Controls.Add(control);
            properties.Add(new DynamicProperty(id, type, initial));
        }

        var run = new ScpiPanelCommand(profile, command, execute);
        commands.Add(run);
        var buttonId = ScpiPanelIds.Button(command.Id);
        properties.Add(new DynamicProperty(buttonId, typeof(ICommand), run));
        section.Controls.Add(new ButtonControl
        {
            Id = buttonId,
            Label = command.Label,
            Description = command.Template,
            ParameterFieldIds = fieldIds.Count > 0 ? fieldIds : null,
        });

        if (command.IsQuery)
        {
            var replyId = ScpiPanelIds.Reply(command.Id);
            section.Controls.Add(new TextFieldControl { Id = replyId, Label = command.Label + " reply" });
            properties.Add(new DynamicProperty(replyId, typeof(string), string.Empty));
        }
    }

    private static (UiControl Control, Type Type, object? Initial) Field(string id, ScpiParameterDefinition parameter)
    {
        var label = parameter.Unit is { Length: > 0 } unit ? $"{parameter.Name} ({unit})" : parameter.Name;
        var kind = parameter.Control ?? parameter.Kind switch
        {
            ScpiParameterKind.Numeric => ScpiParameterControl.Numeric,
            ScpiParameterKind.Choice => ScpiParameterControl.Choice,
            _ => ScpiParameterControl.Text,
        };
        var number = double.TryParse(parameter.DefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : parameter.Minimum ?? 0;
        var minimum = parameter.Minimum ?? 0;
        var maximum = parameter.Maximum ?? Math.Max(minimum, number);

        return kind switch
        {
            ScpiParameterControl.Slider => (
                new SliderControl
                {
                    Id = id,
                    Label = label,
                    Minimum = minimum,
                    Maximum = maximum,
                    Step = parameter.DecimalPlaces is { } places ? Math.Pow(10, -places) : 1,
                    DefaultValue = number,
                    Unit = parameter.Unit,
                },
                typeof(double),
                number),
            ScpiParameterControl.Numeric => (
                new NumericControl { Id = id, Label = label, Minimum = minimum, Maximum = maximum, DefaultValue = number, Unit = parameter.Unit },
                typeof(double),
                number),
            ScpiParameterControl.Choice => (
                new ChoiceControl { Id = id, Label = label, Options = [.. parameter.Options], DefaultValue = parameter.DefaultValue ?? parameter.Options.FirstOrDefault() },
                typeof(string),
                parameter.DefaultValue ?? parameter.Options.FirstOrDefault() ?? string.Empty),
            _ => (
                new TextFieldControl { Id = id, Label = label, DefaultValue = parameter.DefaultValue },
                typeof(string),
                parameter.DefaultValue ?? string.Empty),
        };
    }
}
