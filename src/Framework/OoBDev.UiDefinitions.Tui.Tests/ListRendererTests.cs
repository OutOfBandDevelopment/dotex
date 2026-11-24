using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace OoBDev.UiDefinitions.Tui.Tests;

/// <summary>The form renderer's <see cref="ListControl"/>: a list view that shows the model's items and writes the selected one.</summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class ListRendererTests
{
    public sealed record Profile(string Name)
    {
        public override string ToString() => Name;
    }

    public sealed class Model
    {
        public List<Profile> Saved { get; } = [new("bench"), new("lab")];

        [FormField(Kind = FormFieldKind.List, OptionsFrom = nameof(Saved))]
        public Profile? Selected { get; set; }
    }

    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void Build_ListControl_ShowsItemsAndWritesTheSelection()
    {
        var model = new Model();
        TuiTestRunner.RunHeadlessApp(app =>
        {
            using var binding = new FormBinding(model);
            var parts = FormRenderer.Build(app, FormDefinitionGenerator.Generate(model, "m"), binding, new TuiFormOptions { AvailableWidth = 76 });
            var window = new Window { Width = Dim.Fill(), Height = Dim.Fill() };
            window.Add(parts.Root);
            var token = app.Begin(window) ?? throw new NotSupportedException();
            app.LayoutAndDraw(true);
            try
            {
                var view = (ListView)parts.ControlViews[nameof(Model.Selected)];
                TestContext.WriteLine("list rows: {0}", view.Source?.Count);
                Assert.AreEqual(2, view.Source?.Count);

                view.SelectedItem = 1;

                Assert.AreSame(model.Saved[1], model.Selected);
            }
            finally
            {
                app.End(token);
                window.Dispose();
            }
        });
    }
}
