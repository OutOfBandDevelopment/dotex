using System;
using System.Windows.Forms;

namespace OoBDev.Examples.Controls.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new ControlsForm());
    }
}
