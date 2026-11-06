using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Blazor;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.Blazor.Tests;

/// <summary><see cref="FormView"/> renders each control kind from a generated form and writes changes back to the model.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class FormViewTests
{
    [FormSection("Main", Order = 0)]
    [FormSection("Extra", Order = 1, VisibleWhen = nameof(ShowExtra))]
    public sealed class Model
    {
        public IReadOnlyList<string> Sizes { get; } = ["small", "medium", "large"];

        [Category("Main")]
        [FormField(Kind = FormFieldKind.Toggle, Order = 0)]
        public bool ShowExtra { get; set; }

        [Category("Main")]
        [FormField(Kind = FormFieldKind.TextField, Order = 1)]
        public string Name { get; set; } = "first";

        [Category("Main")]
        [FormField(Kind = FormFieldKind.Choice, OptionsFrom = nameof(Sizes), ChoiceStyle = ChoiceStyle.RadioGroup, Order = 2)]
        public string Size { get; set; } = "medium";

        [Category("Main")]
        [FormField(Kind = FormFieldKind.Slider, Minimum = 0, Maximum = 10, Order = 3)]
        public double Level { get; set; } = 5;

        [Category("Extra")]
        [FormField(Kind = FormFieldKind.TextField)]
        public string Secret { get; set; } = "hidden";
    }

    private static (IRenderedComponent<FormView> Cut, Model Model, FormViewModel Form, Bunit.TestContext Context) Render()
    {
        var model = new Model();
        var binding = new FormBinding(model);
        var form = new FormViewModel(FormDefinitionGenerator.Generate(model), binding);
        var context = new Bunit.TestContext();
        var cut = context.RenderComponent<FormView>(p => p.Add(c => c.Form, form));
        return (cut, model, form, context);
    }

    [TestMethod]
    public void Render_EachControlKind_ShowsItsElement_AndHidesTheConditionalSection()
    {
        // Test
        var (cut, _, _, context) = Render();
        using var _ = context;

        // Assert
        Assert.IsNotNull(cut.Find("[data-id=ShowExtra] input[type=checkbox]"));
        Assert.AreEqual("first", cut.Find("[data-id=Name] input[type=text]").GetAttribute("value"));
        Assert.AreEqual(3, cut.FindAll("[data-id=Size] input[type=radio]").Count);
        Assert.IsNotNull(cut.Find("[data-id=Level] input[type=range]"));
        Assert.AreEqual(0, cut.FindAll("[data-id=Secret]").Count);
    }

    [TestMethod]
    public void Change_TextRadioAndToggle_WriteTheModel_AndToggleRevealsTheSection()
    {
        // Stage
        var (cut, model, _, context) = Render();
        using var _ = context;

        // Test
        cut.Find("[data-id=Name] input[type=text]").Change("second");
        cut.FindAll("[data-id=Size] input[type=radio]").Last().Change(true);
        cut.Find("[data-id=ShowExtra] input[type=checkbox]").Change(true);

        // Assert
        Assert.AreEqual("second", model.Name);
        Assert.AreEqual("large", model.Size);
        Assert.IsTrue(model.ShowExtra);
        Assert.AreEqual(1, cut.FindAll("[data-id=Secret]").Count);
    }

    [TestMethod]
    public void ModelChange_ThroughTheBinding_RerendersTheText()
    {
        // Stage
        var (cut, _, form, context) = Render();
        using var _ = context;

        // Test
        ((TextViewModel)form.Row("Name")!).Text = "from code";

        // Assert
        cut.WaitForAssertion(() => Assert.AreEqual("from code", cut.Find("[data-id=Name] input[type=text]").GetAttribute("value")));
    }
}
