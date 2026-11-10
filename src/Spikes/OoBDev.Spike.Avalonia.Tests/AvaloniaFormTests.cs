using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Skia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Spike.Avalonia;
using OoBDev.UiDefinitions.ViewModels;

[assembly: AvaloniaTestApplication(typeof(OoBDev.Spike.Avalonia.Tests.TestAppBuilder))]

namespace OoBDev.Spike.Avalonia.Tests;

/// <summary>Headless application with the real Skia renderer, so screenshots show what a user sees.</summary>
public static class TestAppBuilder
{
    /// <summary>Builds the test application.</summary>
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<SpikeApp>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

/// <summary>Renders the shared UiDefinition view models in an Avalonia window, headless, and attaches screenshots to the result.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class AvaloniaFormTests
{
    private static HeadlessUnitTestSession _session => HeadlessSession.Instance;

    public TestContext TestContext { get; set; } = null!;

    private void Run(global::System.Action<DemoModel, Window> body) => _session.Dispatch(() =>
    {
        var model = new DemoModel();
        var window = DemoForm.CreateWindow(model);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        body(model, window);
    }, default).GetAwaiter().GetResult();

    private void Shot(Window window, string step) => Screenshots.Save(TestContext, window, step);

    [TestMethod]
    public void Render_Form_ShowsControlsAndHidesConditionalRow()
    {
        Run((_, window) =>
        {
            Assert.IsNotNull(window.GetVisualDescendants().OfType<CheckBox>().SingleOrDefault());
            Assert.IsNotNull(window.GetVisualDescendants().OfType<Slider>().SingleOrDefault());
            Assert.HasCount(2, window.GetVisualDescendants().OfType<RadioButton>().ToList());
            Assert.IsEmpty(window.GetVisualDescendants().OfType<TextBox>().ToList(), "Name is hidden until Advanced is on.");
            Shot(window, "initial");
        });
    }

    [TestMethod]
    public void Toggle_Click_RunsTheCommandAndRevealsTheRow()
    {
        Run((model, window) =>
        {
            var check = window.GetVisualDescendants().OfType<CheckBox>().Single();

            check.Command!.Execute(check.CommandParameter);
            Dispatcher.UIThread.RunJobs();

            Assert.IsTrue(model.Advanced);
            Assert.HasCount(1, window.GetVisualDescendants().OfType<TextBox>().ToList());
            Shot(window, "advanced-on");
        });
    }

    [TestMethod]
    public void Slider_ValueChange_WritesTheModel()
    {
        Run((model, window) =>
        {
            window.GetVisualDescendants().OfType<Slider>().Single().Value = 7;
            Dispatcher.UIThread.RunJobs();

            Assert.AreEqual(7, model.Level);
            Shot(window, "level-7");
        });
    }

    [TestMethod]
    public void Choice_SelectOption_WritesTheModel()
    {
        Run((model, window) =>
        {
            var high = window.GetVisualDescendants().OfType<RadioButton>().Single(r => (string?)r.Content == "High");

            high.Command!.Execute(high.CommandParameter);
            Dispatcher.UIThread.RunJobs();

            Assert.AreEqual("High", model.Mode);
            Shot(window, "mode-high");
        });
    }
}
