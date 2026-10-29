using System;
using System.Windows;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.Wpf;

/// <summary>A red, green and blue colour dialog; the view holds only bindings and closes when the view model asks.</summary>
public partial class ColorPickerWindow : Window
{
    /// <summary>Creates the dialog.</summary>
    /// <param name="model">The dialog state.</param>
    public ColorPickerWindow(ColorPickerViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        InitializeComponent();
        DataContext = model;
        model.CloseRequested += (_, accepted) =>
        {
            Accepted = accepted;
            Close();
        };
    }

    /// <summary>Whether the user accepted the colour.</summary>
    public bool Accepted { get; private set; }
}
