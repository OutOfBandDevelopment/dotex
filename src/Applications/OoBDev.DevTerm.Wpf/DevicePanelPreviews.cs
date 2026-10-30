using System;
using System.Linq;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Adds the "Sends" preview of a surface to the tooltip of each command button.</summary>
internal static class DevicePanelPreviews
{
    internal static void Apply(UiDefinition definition, IControlSurface surface)
    {
        if (surface is not ICommandPreview preview)
        {
            return;
        }

        foreach (var button in definition.Sections.SelectMany(s => s.Controls).OfType<ButtonControl>())
        {
            if (button.CommandId is { } commandId && preview.PreviewCommand(commandId, null) is { } sends)
            {
                button.Description = string.IsNullOrEmpty(button.Description) ? $"Sends: {sends}" : $"{button.Description}{Environment.NewLine}Sends: {sends}";
            }
        }
    }
}
