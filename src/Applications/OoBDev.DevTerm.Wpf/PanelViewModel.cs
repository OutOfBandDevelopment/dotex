using System;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>One open panel: a title and the form the shell shows for it. Disposing it releases the form's model subscription.</summary>
public sealed class PanelViewModel : IDisposable
{
    /// <summary>Creates the panel.</summary>
    /// <param name="title">The title.</param>
    /// <param name="form">The form view model.</param>
    public PanelViewModel(string title, FormViewModel form)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(form);
        Title = title;
        Form = form;
    }

    /// <summary>The title.</summary>
    public string Title { get; }

    /// <summary>The form.</summary>
    public FormViewModel Form { get; }

    /// <inheritdoc />
    public void Dispose() => Form.Dispose();
}
