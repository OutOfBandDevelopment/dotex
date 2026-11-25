namespace OoBDev.System.Text.Templating;

/// <summary>A render case whose output was not the approved one.</summary>
/// <param name="TemplateName">The template that failed.</param>
/// <param name="Reason">Why: the template is missing, it rendered nothing, it threw, or the first line that differs.</param>
public record TemplateRenderFailure(string TemplateName, string Reason);
