using System.Reflection;

namespace OoBDev.System.Utilities;

public interface IStringFormatter
{
    string? Format(string keyFormatter, MethodInfo method, object?[]? args);
}
