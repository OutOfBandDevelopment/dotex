using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.WinForms.Tests;

/// <summary>The Windows Forms form view against real view models, shown off screen so visibility is real.</summary>
[TestCategory("Unit")]
[STATestClass]
public sealed class FormViewTests
{
    public sealed class Settings
    {
        public bool Enabled { get; set; }

        public string Name { get; set; } = "a";

        public double Level { get; set; } = 2;

        public string Mode { get; set; } = "Low";

        public string Tags { get; set; } = "x";
    }

    private static UiDefinition Definition() => new()
    {
        Name = "t",
        Sections =
        [
            new UiSection
            {
                Label = "Main",
                Controls =
                [
                    new ToggleControl { Id = "Enabled", Label = "Enabled" },
                    new TextFieldControl { Id = "Name", Label = "Name", VisibleWhen = new UiCondition { Id = "Enabled" } },
                    new SliderControl { Id = "Level", Label = "Level", Minimum = 0, Maximum = 10 },
                    new ChoiceControl { Id = "Mode", Label = "Mode", Options = ["Low", "High"], Style = ChoiceStyle.RadioGroup },
                    new ChoiceControl { Id = "Tags", Label = "Tags", Options = ["x", "y"], Style = ChoiceStyle.CheckList },
                ],
            },
        ],
    };

    private static (Form Host, FormView View, Settings Model) Show()
    {
        var model = new Settings();
        var view = new FormView(new FormViewModel(Definition(), new FormBinding(model))) { Dock = DockStyle.Fill };
        var host = new Form { Width = 500, Height = 500, StartPosition = FormStartPosition.Manual, Location = new global::System.Drawing.Point(-3000, -3000), ShowInTaskbar = false };
        host.Controls.Add(view);
        host.Show();
        Application.DoEvents();
        return (host, view, model);
    }

    private static void Click(ButtonBase button) =>
        typeof(Control).GetMethod("OnClick", global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.NonPublic)!.Invoke(button, [global::System.EventArgs.Empty]);

    private static T Descendant<T>(Control root, global::System.Func<T, bool> match)
        where T : Control => All<T>(root).Single(match);

    private static global::System.Collections.Generic.IEnumerable<T> All<T>(Control root)
        where T : Control => root.Controls.Cast<Control>().SelectMany(c => All<T>(c)).Concat(root.Controls.OfType<T>());

    [TestMethod]
    public void Render_Form_HidesTheConditionalRowUntilToggled()
    {
        // Stage
        var (host, view, model) = Show();

        // Test
        var hidden = !view.Editors["Name"].Visible;
        ((CheckBox)view.Editors["Enabled"]).Checked = true;
        Application.DoEvents();
        var shown = view.Editors["Name"].Visible;
        host.Dispose();

        // Assert
        Assert.IsTrue(hidden);
        Assert.IsTrue(shown);
        Assert.IsTrue(model.Enabled);
    }

    [TestMethod]
    public void Slider_ValueChange_WritesTheModel()
    {
        // Stage
        var (host, view, model) = Show();

        // Test
        ((TrackBar)view.Editors["Level"]).Value = 7;
        host.Dispose();

        // Assert
        Assert.AreEqual(7, model.Level);
    }

    [TestMethod]
    public void Choice_ClickOption_WritesTheModelAndMovesTheSelection()
    {
        // Stage
        var (host, view, model) = Show();
        var high = Descendant<RadioButton>(view.Editors["Mode"], r => r.Text == "High");
        var low = Descendant<RadioButton>(view.Editors["Mode"], r => r.Text == "Low");

        // Test
        Click(high);
        Application.DoEvents();
        var lowChecked = low.Checked;
        host.Dispose();

        // Assert
        Assert.AreEqual("High", model.Mode);
        Assert.IsTrue(high.Checked);
        Assert.IsFalse(lowChecked);
    }

    [TestMethod]
    public void CheckList_ClickOption_AddsItToTheSelection()
    {
        // Stage
        var (host, view, model) = Show();
        var y = Descendant<CheckBox>(view.Editors["Tags"], c => c.Text == "y");

        // Test
        Click(y);
        Application.DoEvents();
        host.Dispose();

        // Assert
        Assert.Contains("y", model.Tags, global::System.StringComparison.Ordinal);
        Assert.Contains("x", model.Tags, global::System.StringComparison.Ordinal);
    }

    [TestMethod]
    public void Text_Typing_WritesTheModel()
    {
        // Stage
        var (host, view, model) = Show();
        ((CheckBox)view.Editors["Enabled"]).Checked = true;

        // Test
        ((TextBox)view.Editors["Name"]).Text = "typed";
        host.Dispose();

        // Assert
        Assert.AreEqual("typed", model.Name);
    }

    [TestMethod]
    public void Model_ChangedElsewhere_UpdatesTheEditors()
    {
        // Stage
        var (host, view, model) = Show();
        var form = view.ViewModel;

        // Test
        model.Enabled = true;
        form.Refresh();
        var check = ((CheckBox)view.Editors["Enabled"]).Checked;
        host.Dispose();

        // Assert
        Assert.IsTrue(check);
    }
}
