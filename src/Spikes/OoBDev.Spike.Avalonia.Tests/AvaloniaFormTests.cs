using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Spike.Avalonia;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.Spike.Avalonia.Tests;

/// <summary>Renders the shared UiDefinition view models in an Avalonia window, headless.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class AvaloniaFormTests
{
    private sealed class Model : INotifyPropertyChanged
    {
        private bool _advanced;
        private string _name = "a";
        private string _mode = "Low";
        private double _level = 2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool Advanced { get => _advanced; set => Set(ref _advanced, value); }

        public string Name { get => _name; set => Set(ref _name, value); }

        public string Mode { get => _mode; set => Set(ref _mode, value); }

        public double Level { get => _level; set => Set(ref _level, value); }

        private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    private static readonly HeadlessUnitTestSession _session = HeadlessUnitTestSession.StartNew(typeof(TestApp));

    private sealed class TestApp : Application
    {
        public override void Initialize() => Styles.Add(new global::Avalonia.Themes.Fluent.FluentTheme());
    }

    private static UiDefinition Definition() => new()
    {
        Name = "spike",
        Sections =
        [
            new UiSection
            {
                Label = "Main",
                Controls =
                [
                    new ToggleControl { Id = "Advanced", Label = "Advanced" },
                    new TextFieldControl { Id = "Name", Label = "Name", VisibleWhen = new UiCondition { Id = "Advanced" } },
                    new SliderControl { Id = "Level", Label = "Level", Minimum = 0, Maximum = 10 },
                    new ChoiceControl { Id = "Mode", Label = "Mode", Options = ["Low", "High"] },
                ],
            },
        ],
    };

    private static void Run(global::System.Action<Model, FormViewModel, Window> body) => _session.Dispatch(() =>
    {
        var model = new Model();
        var form = new FormViewModel(Definition(), new FormBinding(model));
        var window = new Window { Width = 400, Height = 300, Content = FormView.Create(form) };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        body(model, form, window);
    }, default).GetAwaiter().GetResult();

    [TestMethod]
    public void Render_Form_ShowsControlsAndHidesConditionalRow()
    {
        Run((_, _, window) =>
        {
            Assert.IsNotNull(window.GetVisualDescendants().OfType<CheckBox>().SingleOrDefault());
            Assert.IsNotNull(window.GetVisualDescendants().OfType<Slider>().SingleOrDefault());
            Assert.HasCount(2, window.GetVisualDescendants().OfType<RadioButton>().ToList());
            Assert.IsEmpty(window.GetVisualDescendants().OfType<TextBox>().ToList(), "Name is hidden until Advanced is on.");
        });
    }

    [TestMethod]
    public void Toggle_Click_RunsTheCommandAndRevealsTheRow()
    {
        Run((model, _, window) =>
        {
            var check = window.GetVisualDescendants().OfType<CheckBox>().Single();

            check.Command!.Execute(check.CommandParameter);
            Dispatcher.UIThread.RunJobs();

            Assert.IsTrue(model.Advanced);
            Assert.HasCount(1, window.GetVisualDescendants().OfType<TextBox>().ToList());
        });
    }

    [TestMethod]
    public void Slider_ValueChange_WritesTheModel()
    {
        Run((model, _, window) =>
        {
            window.GetVisualDescendants().OfType<Slider>().Single().Value = 7;
            Dispatcher.UIThread.RunJobs();

            Assert.AreEqual(7, model.Level);
        });
    }

    [TestMethod]
    public void Choice_SelectOption_WritesTheModel()
    {
        Run((model, _, window) =>
        {
            var high = window.GetVisualDescendants().OfType<RadioButton>().Single(r => (string?)r.Content == "High");

            high.Command!.Execute(high.CommandParameter);
            Dispatcher.UIThread.RunJobs();

            Assert.AreEqual("High", model.Mode);
        });
    }
}
