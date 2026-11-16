namespace OoBDev.System.Text.Templating;

/// <summary>One template rendered against sample data, with the output a person approved.</summary>
/// <param name="TemplateName">The template to render.</param>
/// <param name="Data">The sample data.</param>
/// <param name="Expected">The approved output.</param>
public record TemplateRenderCase(string TemplateName, object Data, string Expected);
