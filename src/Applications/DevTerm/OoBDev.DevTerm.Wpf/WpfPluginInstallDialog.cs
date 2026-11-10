using System;
using System.Windows;
using Microsoft.Win32;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Picks, approves and reports a plugin install with the Windows dialogs.</summary>
public sealed class WpfPluginInstallDialog : IPluginInstallDialog
{
    private const string _title = "Install plugin";

    /// <inheritdoc />
    public string? PickZip()
    {
        var dialog = new OpenFileDialog { Filter = "Plugins (*.zip)|*.zip|All files (*.*)|*.*", Title = _title };
        return dialog.ShowDialog(Application.Current?.MainWindow) == true ? dialog.FileName : null;
    }

    /// <inheritdoc />
    public PluginApprovalChoice Approve(PluginApprovalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var text = $"Plugin '{request.Name}' {request.Version} will be installed and can then:{Environment.NewLine}{Environment.NewLine}  {request.CommandLine}{Environment.NewLine}  into {request.Folder}{Environment.NewLine}  (content hash {request.Hash[..12]}){Environment.NewLine}{Environment.NewLine}Yes: install and always allow this version. No: install, ask again at each start. Cancel: don't install.";
        return MessageBox.Show(text, _title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question) switch
        {
            MessageBoxResult.Yes => PluginApprovalChoice.Always,
            MessageBoxResult.No => PluginApprovalChoice.Once,
            _ => PluginApprovalChoice.Deny,
        };
    }

    /// <inheritdoc />
    public void Report(PluginInstallResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        MessageBox.Show(result.Installed ? result.Message + Environment.NewLine + "It loads the next time dev-term starts." : "Not installed: " + result.Message, _title, MessageBoxButton.OK, result.Installed ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }
}
