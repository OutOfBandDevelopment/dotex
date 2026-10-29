using System.Windows.Controls;

namespace OoBDev.UiDefinitions.Wpf;

/// <summary>The body of the colour dialog: a swatch, three sliders and the buttons; set a <see cref="ViewModels.ColorPickerViewModel"/> as the <c>DataContext</c>.</summary>
public partial class ColorPickerView : UserControl
{
    /// <summary>Creates the view.</summary>
    public ColorPickerView() => InitializeComponent();
}
