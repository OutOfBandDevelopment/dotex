using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

public interface IPathResolver
{
    Task<object> ItemSelector(string path);
}
