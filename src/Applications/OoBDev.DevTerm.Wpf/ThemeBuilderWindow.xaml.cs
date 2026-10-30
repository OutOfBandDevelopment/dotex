using System;
using System.Windows;
using System.Windows.Input;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>Hosts a <see cref="ThemeBuilderDialogViewModel"/>.</summary>
public partial class ThemeBuilderWindow : Window
{
    private readonly ThemeBuilderDialogViewModel _builder;

    /// <summary>Creates the window over <paramref name="builder"/>.</summary>
    /// <param name="builder">The theme builder view model.</param>
    public ThemeBuilderWindow(ThemeBuilderDialogViewModel builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        _builder = builder;
        InitializeComponent();
        WpfTheme.Attach(this);
        DataContext = builder;
    }

    private void Roles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_builder.Edit.CanExecute(null))
        {
            _builder.Edit.Execute(null);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_builder.TrySave())
        {
            DialogResult = true;
        }
    }
}
