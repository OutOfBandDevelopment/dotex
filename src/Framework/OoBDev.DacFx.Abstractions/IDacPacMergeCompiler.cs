namespace OoBDev.DacFx;

/// <summary>
/// Provides functionality for compiling multiple DacPac files into a single merged package.
/// </summary>
public interface IDacpacMergeCompiler
{
    /// <summary>
    /// Creates a merged DacPac package from the specified definition.
    /// </summary>
    /// <param name="def">The merge definition containing source files, options, and target settings.</param>
    void CreatePackage(IDacPacMergeDefinition def);
}