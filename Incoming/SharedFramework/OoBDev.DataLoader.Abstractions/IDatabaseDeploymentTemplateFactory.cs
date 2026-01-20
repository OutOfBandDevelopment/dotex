using System.Threading.Tasks;

namespace OoBDev.DataLoader
{
    public interface IDatabaseDeploymentTemplateFactory
    {
        IDatabaseDeploymentTemplate GetTemplate();
        Task<IDatabaseDeploymentTemplate> GetTemplateAsync();
    }
}