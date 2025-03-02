using OoBDev.DacFx;
using OoBDev.System.ComponentModel;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.SqlServer.DacFx;

public class DacPacMergeTemplateFactory : TemplateFactoryBase<DacPacMergeTemplate>, IDacPacMergeTemplateFactory
{
    private readonly IDacPacCompilerConfig _config;

    public DacPacMergeTemplateFactory(
        IDacPacCompilerConfig config,
        IObjectConverter converter
        ) : base(converter)
    {
        _config = config;
    }

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
