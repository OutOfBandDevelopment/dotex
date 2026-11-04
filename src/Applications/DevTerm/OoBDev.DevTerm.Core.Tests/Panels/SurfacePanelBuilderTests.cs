using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Panels;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Core.Tests.Panels;

/// <summary>Device panels built from a definition and a control surface.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class SurfacePanelBuilderTests
{
    private sealed class RecordingSurface : IControlSurface
    {
        public List<(string Id, string? Value)> Sent { get; } = [];

        public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default)
        {
            Sent.Add((commandId, value));
            return Task.CompletedTask;
        }
    }

    private static UiDefinition Definition() => new()
    {
        Name = "Panel",
        Sections =
        [
            new UiSection
            {
                Controls =
                [
                    new ToggleControl { Id = "d1", Label = "D1" },
                    new ChoiceControl { Id = "mode", Label = "Mode", Options = ["a", "b"] },
                    new ButtonControl { Id = "reset", Label = "Reset", CommandId = "RST" },
                    new IndicatorControl { Id = "led", Label = "Led" },
                ],
            },
        ],
    };

    [TestMethod]
    public void Build_DefinitionWithDisplays_KeepsOnlyFormControls()
    {
        // Stage
        var surface = new RecordingSurface();

        // Test
        using var panel = SurfacePanelBuilder.Build(Definition(), surface);

        // Assert
        CollectionAssert.AreEqual(new[] { "d1", "mode", "reset" }, panel.Definition.Sections[0].Controls.ConvertAll(c => c.Id));
    }

    [TestMethod]
    public void Changed_ToggleAndChoice_SendValuesToSurface()
    {
        // Stage
        var surface = new RecordingSurface();
        using var panel = SurfacePanelBuilder.Build(Definition(), surface);

        // Test
        panel.Binding.SetBool("d1", true);
        panel.Binding.SetSelection("mode", ["b"]);

        // Assert
        CollectionAssert.AreEqual(new (string, string?)[] { ("d1", "1"), ("mode", "b") }, surface.Sent);
    }

    [TestMethod]
    public void Invoke_Button_SendsItsCommandId()
    {
        // Stage
        var surface = new RecordingSurface();
        using var panel = SurfacePanelBuilder.Build(Definition(), surface);

        // Test
        panel.Binding.Invoke("reset");

        // Assert
        CollectionAssert.AreEqual(new (string, string?)[] { ("RST", null) }, surface.Sent);
    }
}
