using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Asks for the listen port with a small prompt window.</summary>
public sealed class WpfListenPeersDialog : IListenPeersDialog
{
    /// <inheritdoc />
    public int? AskPort()
    {
        var box = new TextBox { MinWidth = 120, Text = "9000", Margin = new Thickness(0, 0, 0, 12) };
        var ok = new Button { Content = "OK", IsDefault = true, MinWidth = 72, Margin = new Thickness(0, 0, 8, 0) };
        var cancel = new Button { Content = "Cancel", IsCancel = true, MinWidth = 72 };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        var panel = new StackPanel { Margin = new Thickness(12) };
        panel.Children.Add(new TextBlock { Text = "TCP port to listen on (each peer gets its own tab):", Margin = new Thickness(0, 0, 0, 4) });
        panel.Children.Add(box);
        panel.Children.Add(buttons);
        var dialog = new Window
        {
            Title = "Listen for Peers",
            Content = panel,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };
        WpfTheme.Attach(dialog);
        ok.Click += (_, _) =>
        {
            if (int.TryParse(box.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var port) && port is >= 0 and <= 65535)
            {
                dialog.DialogResult = true;
            }
        };
        dialog.Loaded += (_, _) => box.Focus();
        return dialog.ShowDialog() == true ? int.Parse(box.Text, NumberStyles.None, CultureInfo.InvariantCulture) : null;
    }
}
