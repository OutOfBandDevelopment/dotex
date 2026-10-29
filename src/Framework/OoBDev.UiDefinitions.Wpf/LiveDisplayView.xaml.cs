using System.Windows.Controls;

namespace OoBDev.UiDefinitions.Wpf;

/// <summary>Renders a <see cref="ViewModels.LiveDisplayViewModel"/> set as the <c>DataContext</c>: an indicator's text or a bar graph's bars.</summary>
public partial class LiveDisplayView : UserControl
{
    /// <summary>Creates the view.</summary>
    public LiveDisplayView() => InitializeComponent();
}
