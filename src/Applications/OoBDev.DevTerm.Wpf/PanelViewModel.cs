using System;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>One open panel: a title and the form the shell shows for it. Disposing it releases the form's model subscription.</summary>
public sealed class PanelViewModel : IDisposable
{
    private readonly IDisposable? _owned;

    /// <summary>Creates the panel.</summary>
    /// <param name="title">The title.</param>
    /// <param name="form">The form view model.</param>
    /// <param name="owned">Something disposed with the panel, such as the surface subscription behind the form.</param>
    public PanelViewModel(string title, FormViewModel form, IDisposable? owned = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(form);
        Title = title;
        Form = form;
        _owned = owned;
    }

    /// <summary>The title.</summary>
    public string Title { get; }

    /// <summary>The form.</summary>
    public FormViewModel Form { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Form.Dispose();
        _owned?.Dispose();
    }
}
