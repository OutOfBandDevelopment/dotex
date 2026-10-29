using System;

namespace OoBDev.UiDefinitions.Forms;

/// <summary>One property of a model built at run time by <see cref="DynamicModelFactory"/>.</summary>
/// <param name="Name">The property name, which a control's <see cref="UiControl.Id"/> refers to.</param>
/// <param name="Type">The property type (string, a number, bool, an enum or <see cref="System.Windows.Input.ICommand"/>).</param>
/// <param name="Initial">The value the property starts with, or null.</param>
public sealed record DynamicProperty(string Name, Type Type, object? Initial = null);
