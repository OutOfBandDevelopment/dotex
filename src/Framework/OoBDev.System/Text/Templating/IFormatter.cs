using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

public interface IFormatter
{
    bool CanFormat(object source);
    Task<string?> Format(object source, string format);
}
