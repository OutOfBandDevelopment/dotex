namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Marker interface for segment build definitions that configure how data streams are segmented.
/// </summary>
/// <remarks>
/// This interface serves as a marker for the fluent builder pattern used to define segment parsing rules.
/// Use the <see cref="Segment"/> static class to create and configure segment definitions.
/// </remarks>
public interface ISegmentBuildDefinition
{
}