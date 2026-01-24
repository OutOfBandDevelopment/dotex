using OoBDev.System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.SqlServer.DacFx;

/// <summary>
/// Base class for factories that create templates from files.
/// </summary>
/// <typeparam name="T">The type of template to create.</typeparam>
public abstract class TemplateFactoryBase<T> where T : class, new()
{
    /// <summary>
    /// The object converter for deserializing template files.
    /// </summary>
    protected readonly IObjectConverter _converter;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateFactoryBase{T}"/> class.
    /// </summary>
    /// <param name="converter">The object converter for deserializing templates.</param>
    protected TemplateFactoryBase(IObjectConverter converter) => _converter = converter;

    /// <summary>
    /// Reads and deserializes a template from a file.
    /// </summary>
    /// <param name="fileName">The path to the template file, or null to return a default instance.</param>
    /// <returns>The deserialized template instance.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    protected async Task<T> ReadTemplateFileAsync(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return new T();
        if (!File.Exists(fileName))
            throw new FileNotFoundException($"Missing Template File: \"{fileName}\"", fileName);

        var content = await File.ReadAllTextAsync(fileName).ConfigureAwait(false);
        var ext = Path.GetExtension(fileName).ToUpper();

        var template = ext switch
        {
            //TODO: add a way to detect serializer by passing around media type
            //".YML" => ReadAsYaml(content),
            //".YAML" => ReadAsYaml(content),
            _ => _converter.Convert<T>(content)
        } ?? new T();

        return template;
    }
}
