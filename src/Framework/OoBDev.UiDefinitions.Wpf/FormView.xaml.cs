using System.Windows.Controls;

namespace OoBDev.UiDefinitions.Wpf;

/// <summary>Renders a <see cref="ViewModels.FormViewModel"/> set as the <c>DataContext</c>; the view holds no logic, only bindings.</summary>
public partial class FormView : UserControl
{
    /// <summary>Creates the view.</summary>
    public FormView() => InitializeComponent();
}
