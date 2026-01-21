// Ignore Spelling: Dac

namespace OoBDev.DacFx;

/// <summary>
/// Provides validation functionality for DacPac files.
/// </summary>
public interface IDacPacValidator
{
    /// <summary>
    /// Validates the structure and content of a DacPac file.
    /// </summary>
    /// <param name="dacpacFile">The path to the DacPac file to validate.</param>
    /// <exception cref="System.IO.FileNotFoundException">Thrown when the DacPac file does not exist.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when the DacPac file is invalid or corrupted.</exception>
    void ValidateDacPac(string dacpacFile);
}
