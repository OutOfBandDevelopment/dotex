using System;
using System.Windows.Forms;

namespace OoBDev.Examples.Controls.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var controls = new ControlsForm();
        controls.Show();
        new LiveForm().Show();
        Application.Run(new SettingsForm(new ControlsSampleSettings()));
    }
}
