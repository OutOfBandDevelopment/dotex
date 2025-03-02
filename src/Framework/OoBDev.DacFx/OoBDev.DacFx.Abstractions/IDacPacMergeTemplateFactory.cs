using System.Threading.Tasks;

namespace OoBDev.DacFx;

public interface IDacPacMergeTemplateFactory
{
    Task<IDacPacMergeTemplate> Create();
}