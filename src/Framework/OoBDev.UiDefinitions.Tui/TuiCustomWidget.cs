using Terminal.Gui.ViewBase;
namespace OoBDev.UiDefinitions.Tui;

/// <summary>A host-built widget and how many lines it takes.</summary>
public sealed record TuiCustomWidget(View View, int Rows);
