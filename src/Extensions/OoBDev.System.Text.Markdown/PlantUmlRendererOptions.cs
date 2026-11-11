namespace OoBDev.System.Text.Markdown;

/// <summary>Settings for the PlantUML renderer.</summary>
public class PlantUmlRendererOptions
{
    /// <summary>The default server: the local <c>plantuml</c> container of the test stack, so no diagram source leaves the machine unless a remote URL is set on purpose.</summary>
    public const string DefaultRemoteUrl = "http://localhost:8086/";

    /// <summary>Base URL of the PlantUML server that renders the diagrams; set it to a public server (for example <c>https://www.plantuml.com/plantuml/</c>) only by choice.</summary>
    public string RemoteUrl { get; set; } = DefaultRemoteUrl;
}
