using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.Avalonia;

/// <summary>Renders a <see cref="FormViewModel"/> in Avalonia using only bindings to its properties and commands (no code-behind logic).</summary>
public static class FormView
{
    /// <summary>Builds the visual tree for <paramref name="form"/>.</summary>
    public static Control Create(FormViewModel form)
    {
        var sections = new ItemsControl
        {
            ItemsSource = form.Sections,
            ItemTemplate = new FuncDataTemplate<FormSectionViewModel>((_, _) => Section()),
        };
        return new ScrollViewer { DataContext = form, Content = sections };
    }

    private static StackPanel Section()
    {
        var header = new TextBlock { FontWeight = global::Avalonia.Media.FontWeight.Bold };
        header.Bind(TextBlock.TextProperty, new Binding(nameof(FormSectionViewModel.Label)));
        var rows = new ItemsControl { ItemTemplate = new FuncDataTemplate<ControlViewModel>((_, _) => Row()) };
        rows.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(FormSectionViewModel.Rows)));
        var panel = new StackPanel { Children = { header, rows } };
        panel.Bind(global::Avalonia.Visual.IsVisibleProperty, new Binding(nameof(FormSectionViewModel.HasVisibleRows)));
        return panel;
    }

    private static StackPanel Row()
    {
        var label = new TextBlock { Width = 140, VerticalAlignment = VerticalAlignment.Center };
        label.Bind(TextBlock.TextProperty, new Binding(nameof(ControlViewModel.Label)));
        var host = new ContentControl { ContentTemplate = new FuncDataTemplate<ControlViewModel>((vm, _) => Editor(vm)) };
        host.Bind(ContentControl.ContentProperty, new Binding("."));
        var row = new StackPanel { Orientation = Orientation.Horizontal, Children = { label, host } };
        row.Bind(global::Avalonia.Visual.IsVisibleProperty, new Binding(nameof(ControlViewModel.IsVisible)));
        row.Bind(InputElement_IsEnabled, new Binding(nameof(ControlViewModel.IsEnabled)));
        return row;
    }

    private static readonly global::Avalonia.AvaloniaProperty<bool> InputElement_IsEnabled = global::Avalonia.Input.InputElement.IsEnabledProperty;

    private static Control Editor(ControlViewModel? vm)
    {
        switch (vm)
        {
            case ToggleViewModel:
                var check = new CheckBox();
                check.Bind(ToggleButton_IsChecked, new Binding(nameof(ToggleViewModel.IsChecked)));
                check.Bind(global::Avalonia.Controls.Button.CommandProperty, new Binding(nameof(ToggleViewModel.Toggle)));
                check.Bind(global::Avalonia.Controls.Button.CommandParameterProperty, new Binding(nameof(ToggleViewModel.IsChecked), BindingMode.OneWay) { Converter = global::Avalonia.Data.Converters.BoolConverters.Not });
                return check;
            case ChoiceViewModel:
                var list = new ItemsControl { ItemTemplate = new FuncDataTemplate<ChoiceOptionViewModel>((_, _) => Option()) };
                list.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(ChoiceViewModel.OptionItems)));
                return list;
            case ButtonViewModel:
                var button = new global::Avalonia.Controls.Button();
                button.Bind(ContentControl.ContentProperty, new Binding(nameof(ControlViewModel.Label)));
                button.Bind(global::Avalonia.Controls.Button.CommandProperty, new Binding(nameof(ButtonViewModel.Invoke)));
                return button;
            case TextViewModel { IsSlider: true }:
                var slider = new Slider { Width = 160 };
                slider.Bind(global::Avalonia.Controls.Primitives.RangeBase.MinimumProperty, new Binding(nameof(TextViewModel.Minimum)));
                slider.Bind(global::Avalonia.Controls.Primitives.RangeBase.MaximumProperty, new Binding(nameof(TextViewModel.Maximum)));
                slider.Bind(global::Avalonia.Controls.Primitives.RangeBase.ValueProperty, new Binding(nameof(TextViewModel.Value), BindingMode.TwoWay));
                return slider;
            default:
                var text = new TextBox { Width = 160 };
                text.Bind(TextBox.TextProperty, new Binding(nameof(TextViewModel.Text), BindingMode.TwoWay));
                return text;
        }
    }

    private static readonly global::Avalonia.AvaloniaProperty<bool?> ToggleButton_IsChecked = global::Avalonia.Controls.Primitives.ToggleButton.IsCheckedProperty;

    private static RadioButton Option()
    {
        var radio = new RadioButton { GroupName = "choice" };
        radio.Bind(ContentControl.ContentProperty, new Binding(nameof(ChoiceOptionViewModel.Text)));
        radio.Bind(ToggleButton_IsChecked, new Binding(nameof(ChoiceOptionViewModel.IsSelected)));
        radio.Bind(global::Avalonia.Controls.Button.CommandProperty, new Binding(nameof(ChoiceOptionViewModel.Command)));
        radio.Bind(global::Avalonia.Controls.Button.CommandParameterProperty, new Binding(nameof(ChoiceOptionViewModel.Text)));
        return radio;
    }
}
