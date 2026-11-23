using System.Text.Json.Serialization;
namespace OoBDev.DevTerm.Web.Docking;

/// <summary>A node of the dock tree: a split or a pane.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(DockSplit), "split")]
[JsonDerivedType(typeof(DockPane), "pane")]
public abstract record DockNode;
