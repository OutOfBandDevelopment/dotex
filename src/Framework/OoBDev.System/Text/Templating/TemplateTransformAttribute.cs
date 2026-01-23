using System;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Attribute that marks a class as a template transform implementation, specifying target media types and priority.
/// </summary>
/// <param name="priority">The priority level for selecting this transform when multiple transforms support the same media type.</param>
/// <param name="targetMediaTypes">The media types that this template transform supports.</param>
[AttributeUsage(AttributeTargets.Class)]
public class TemplateTransformAttribute(int priority, params string[] targetMediaTypes) : Attribute
{
    /// <summary>
    /// Initializes a new instance of the TemplateTransformAttribute class with default priority (0).
    /// </summary>
    /// <param name="targetMediaTypes">The media types that this template transform supports.</param>
    public TemplateTransformAttribute(params string[] targetMediaTypes) : this(0, targetMediaTypes) { }

    /// <summary>
    /// Gets the media types that this template transform supports.
    /// </summary>
    public string[] TargetMediaTypes { get; } = targetMediaTypes;

    /// <summary>
    /// Gets the priority level for selecting this transform when multiple transforms support the same media type.
    /// </summary>
    public int Priority { get; } = priority;
}
