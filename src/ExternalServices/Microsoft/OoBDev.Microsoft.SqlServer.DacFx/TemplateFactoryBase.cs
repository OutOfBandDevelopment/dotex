using OoBDev.System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.SqlServer.DacFx;

public abstract class TemplateFactoryBase<T> where T : class, new()
{
    protected readonly IObjectConverter _converter;

    protected TemplateFactoryBase(IObjectConverter converter)
    {
        _converter = converter;
    }

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
