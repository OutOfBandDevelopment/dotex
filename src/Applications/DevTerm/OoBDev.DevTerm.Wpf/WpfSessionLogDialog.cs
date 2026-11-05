using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OoBDev.DevTerm.Logging;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Asks for the log file with a save file dialog and for notes with a small prompt window.</summary>
public sealed class WpfSessionLogDialog : ISessionLogDialog
{
    /// <inheritdoc />
    public string? PickLogPath(string suggestedPath)
    {
        var directory = Path.GetDirectoryName(suggestedPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var extension = SessionLogFormat.FileExtension;
        var dialog = new SaveFileDialog
        {
            Title = "Start Logging",
            FileName = Path.GetFileName(suggestedPath),
            InitialDirectory = directory,
            DefaultExt = extension,
            Filter = $"Session logs (*{extension})|*{extension}|All files (*.*)|*.*",
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    /// <inheritdoc />
    public string? PromptNote()
    {
        var box = new TextBox { MinWidth = 320, Margin = new Thickness(0, 0, 0, 12) };
        var ok = new Button { Content = "OK", IsDefault = true, MinWidth = 72, Margin = new Thickness(0, 0, 8, 0) };
        var cancel = new Button { Content = "Cancel", IsCancel = true, MinWidth = 72 };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        var panel = new StackPanel { Margin = new Thickness(12) };
        panel.Children.Add(new TextBlock { Text = "Note:", Margin = new Thickness(0, 0, 0, 4) });
        panel.Children.Add(box);
        panel.Children.Add(buttons);
        var dialog = new Window
        {
            Title = "Add Note to Log",
            Content = panel,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };
        WpfTheme.Attach(dialog);
        ok.Click += (_, _) => dialog.DialogResult = true;
        dialog.Loaded += (_, _) => box.Focus();
        return dialog.ShowDialog() == true ? box.Text : null;
    }
}
