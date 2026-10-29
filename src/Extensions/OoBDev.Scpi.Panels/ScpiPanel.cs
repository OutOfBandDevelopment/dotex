using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.Scpi.Panels;

/// <summary>A control panel generated from an instrument profile: the form, and the bound model that drives it.</summary>
/// <param name="Definition">The form for a renderer.</param>
/// <param name="Binding">The binding over <see cref="Model"/>; a renderer uses it, and replies arrive through it so <see cref="FormBinding.Changed"/> fires.</param>
/// <param name="Model">The model built at run time from the profile's commands.</param>
public sealed record ScpiPanel(UiDefinition Definition, FormBinding Binding, object Model);
