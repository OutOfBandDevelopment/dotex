// Ignore Spelling: Dac

using OoBDev.DacFx;
using OoBDev.System.ComponentModel;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.SqlServer.DacFx;

/// <summary>
/// Factory for creating DacPac merge templates from configuration and template files.
/// </summary>
public class DacPacMergeTemplateFactory : TemplateFactoryBase<DacPacMergeTemplate>, IDacPacMergeTemplateFactory
{
    private readonly IDacPacCompilerConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="DacPacMergeTemplateFactory"/> class.
    /// </summary>
    /// <param name="config">The compiler configuration.</param>
    /// <param name="converter">The object converter for deserializing templates.</param>
    public DacPacMergeTemplateFactory(
        IDacPacCompilerConfig config,
        IObjectConverter converter
        ) : base(converter) => _config = config;

    /// <inheritdoc/>
    public async Task<IDacPacMergeTemplate> Create()
    {
        var template = await ReadTemplateFileAsync(_config.TemplatePath);

        if (!string.IsNullOrWhiteSpace(_config.SourcePath)) template.SourcePath = _config.SourcePath;
        if (_config.SourcePatterns?.Length > 0) template.SourcePatterns = _config.SourcePatterns;

        if (!string.IsNullOrWhiteSpace(_config.TargetPath)) template.TargetPath = _config.TargetPath;

        if (!string.IsNullOrWhiteSpace(_config.Version)) template.Version = _config.Version;
        if (!string.IsNullOrWhiteSpace(_config.BuildVersion)) template.BuildVersion = _config.BuildVersion;
        if (!string.IsNullOrWhiteSpace(_config.Name)) template.Name = _config.Name;
        if (!string.IsNullOrWhiteSpace(_config.Description)) template.Description = _config.Description;

        if (_config.ModelOptionSource.HasValue) template.ModelOptionSource = _config.ModelOptionSource.Value;

        return template;
    }
}
