using System;
using System.Collections.Generic;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.Panels;

/// <summary>A device panel ready for a form view: the supported controls of a definition, a binding over a generated model, and sends to the surface as values change.</summary>
public sealed class SurfacePanel : IDisposable
{
    private readonly IReadOnlyDictionary<string, UiControl> _sending;
    private readonly IControlSurface _surface;
    private readonly Action<Exception>? _onError;

    internal SurfacePanel(UiDefinition definition, FormBinding binding, IReadOnlyDictionary<string, UiControl> sending, IControlSurface surface, Action<Exception>? onError)
    {
        Definition = definition;
        Binding = binding;
        _sending = sending;
        _surface = surface;
        _onError = onError;
        binding.Changed += OnChanged;
    }

    /// <summary>The definition limited to the controls a form can show.</summary>
    public UiDefinition Definition { get; }

    /// <summary>The binding over the generated model.</summary>
    public FormBinding Binding { get; }

    /// <inheritdoc />
    public void Dispose() => Binding.Changed -= OnChanged;

    private void OnChanged(object? sender, string? id)
    {
        if (id is null || !_sending.TryGetValue(id, out var control))
        {
            return;
        }

        var value = control is ToggleControl ? (Binding.GetBool(id) ? "1" : "0") : Binding.GetText(id);
        SurfaceSender.Send(_surface, id, value, _onError);
    }
}
