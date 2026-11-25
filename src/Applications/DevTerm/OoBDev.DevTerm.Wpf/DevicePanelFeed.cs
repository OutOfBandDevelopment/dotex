using System;
using System.Collections.Generic;
using System.Threading;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Carries the values a structured presenter decodes to the live displays of an open device panel, on the thread that opened it.</summary>
public sealed class DevicePanelFeed : IDisposable
{
    private readonly IStructuredPresenter? _source;
    private readonly SynchronizationContext? _context = SynchronizationContext.Current;
    private readonly IDisposable _surface;

    /// <summary>Creates the feed.</summary>
    /// <param name="definition">The panel definition; its live display controls get view models.</param>
    /// <param name="source">The presenter that decodes the values, or null.</param>
    /// <param name="surface">Disposed with the feed.</param>
    public DevicePanelFeed(UiDefinition definition, IStructuredPresenter? source, IDisposable surface)
    {
        _surface = surface;
        var displays = new List<LiveDisplayViewModel>();
        foreach (var section in definition.Sections)
        {
            foreach (var control in section.Controls)
            {
                if (LiveDisplayState.For(control) is not null)
                {
                    displays.Add(new LiveDisplayViewModel(control));
                }
            }
        }

        Displays = displays;
        _source = source;
        if (_source is not null)
        {
            _source.ValuesChanged += OnValues;
        }
    }

    /// <summary>The live displays.</summary>
    public IReadOnlyList<LiveDisplayViewModel> Displays { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_source is not null)
        {
            _source.ValuesChanged -= OnValues;
        }

        _surface.Dispose();
    }

    private void OnValues(object? sender, IReadOnlyDictionary<string, string> values)
    {
        if (_context is null)
        {
            Apply(values);
        }
        else
        {
            _context.Post(_ => Apply(values), null);
        }
    }

    private void Apply(IReadOnlyDictionary<string, string> values)
    {
        foreach (var display in Displays)
        {
            display.ApplyAll(values);
        }
    }
}
