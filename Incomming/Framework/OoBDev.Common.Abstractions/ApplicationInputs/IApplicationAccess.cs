using System.Threading.Tasks;

namespace OoBDev.Common.ApplicationInputs;

public interface IApplicationAccess
{
    Task<string> GetApplicationApiKey(string applicationName);
}
