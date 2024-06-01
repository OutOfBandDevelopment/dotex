using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

public interface ITemplateTransform
{
    Task<string> Transform(object source, string template);
}
