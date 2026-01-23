namespace OoBDev.DacFx;

/// <summary>
/// Factory for creating <see cref="IDacPacMergeDefinition"/> instances from templates.
/// </summary>
public interface IDacPacMergeDefinitionFactory
{
    /// <summary>
    /// Creates a merge definition from the specified template.
    /// </summary>
    /// <param name="template">The template containing merge configuration settings.</param>
    /// <returns>A configured <see cref="IDacPacMergeDefinition"/> instance ready for compilation.</returns>
    IDacPacMergeDefinition Create(IDacPacMergeTemplate template);
}