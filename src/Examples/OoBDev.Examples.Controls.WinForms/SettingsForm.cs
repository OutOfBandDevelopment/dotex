using System.Drawing;
using System.Windows.Forms;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;
using OoBDev.UiDefinitions.WinForms;

namespace OoBDev.Examples.Controls.WinForms;

/// <summary>Shows the form generated from <see cref="ControlsSampleSettings"/> in a <see cref="FormView"/>.</summary>
public sealed class SettingsForm : Form
{
    /// <summary>Builds the form for <paramref name="settings"/>.</summary>
    public SettingsForm(ControlsSampleSettings settings)
    {
        Settings = settings;
        Text = "UiDefinitions controls: Windows Forms";
        ClientSize = new Size(480, 620);
        View = new FormView(new FormViewModel(FormDefinitionGenerator.Generate(settings), new FormBinding(settings))) { Dock = DockStyle.Fill };
        Controls.Add(View);
    }

    /// <summary>The edited settings.</summary>
    public ControlsSampleSettings Settings { get; }

    /// <summary>The rendered form.</summary>
    public FormView View { get; }
}
