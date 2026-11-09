using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The Notes section remembers whether it was collapsed, through the state the Tui shares.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class PanelViewModelNotesTests
{
    private const string Name = "NotesMemoryPanel";

    private sealed class Settings
    {
        public int Value { get; set; }
    }

    [TestInitialize]
    public void Initialize() => SectionExpansionState.Forget(Name);

    [TestCleanup]
    public void Cleanup() => SectionExpansionState.Forget(Name);

    private static FormViewModel NewForm() =>
        new(new UiDefinition { Name = Name, Description = "Some notes.", Sections = [] }, new FormBinding(new Settings()));

    [TestMethod]
    public void Constructor_NeverToggled_NotesAreExpanded()
    {
        // Stage
        using var form = NewForm();

        // Test
        using var panel = new PanelViewModel("Panel", form);

        // Assert
        Assert.IsTrue(form.NotesExpanded);
    }

    [TestMethod]
    public void NotesExpanded_Collapsed_ReopenedPanelStaysCollapsed()
    {
        // Stage
        using var first = NewForm();
        using var firstPanel = new PanelViewModel("Panel", first);

        // Test
        first.NotesExpanded = false;
        using var second = NewForm();
        using var secondPanel = new PanelViewModel("Panel", second);

        // Assert
        Assert.IsFalse(second.NotesExpanded);
        Assert.IsFalse(SectionExpansionState.IsExpanded(Name, PanelViewModel.NotesSectionLabel));
    }
}
