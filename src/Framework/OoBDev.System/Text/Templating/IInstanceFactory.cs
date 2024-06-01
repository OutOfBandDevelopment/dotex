using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

public interface IInstanceFactory
{
    Task<IPathResolver> GetPathResolver(object source);
    Task<IFormatter> GetFormatter(object source);
    Task<ITemplateTransform> GetTemplateTransform(string mediaType);
}
