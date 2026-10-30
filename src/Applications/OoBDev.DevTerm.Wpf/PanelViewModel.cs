using System;
using System.Collections.Generic;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>One open panel: a title and the form the shell shows for it. Disposing it releases the form's model subscription.</summary>
public sealed class PanelViewModel : IDisposable
{
    private readonly IDisposable? _owned;

    /// <summary>Creates the panel.</summary>
    /// <param name="title">The title.</param>
    /// <param name="form">The form view model.</param>
    /// <param name="displays">The live displays shown beside the form.</param>
    /// <param name="owned">Something disposed with the panel, such as the surface subscription behind the form.</param>
    public PanelViewModel(string title, FormViewModel form, IDisposable? owned = null, IReadOnlyList<LiveDisplayViewModel>? displays = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(form);
        Title = title;
        Form = form;
        _owned = owned;
        Displays = displays ?? [];
    }

    /// <summary>The title.</summary>
    public string Title { get; }

    /// <summary>The live displays (indicators, bars, charts, vectors) fed while the panel is open.</summary>
    public IReadOnlyList<LiveDisplayViewModel> Displays { get; }

    /// <summary>The form.</summary>
    public FormViewModel Form { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Form.Dispose();
        _owned?.Dispose();
    }
}
