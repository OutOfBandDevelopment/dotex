using System.Reflection;

namespace OoBDev.System.Utilities;

public interface IStringFormatter
{
    //TODO: need to rebuild this function
    string? Format(string keyFormatter, MethodInfo method, object[] args);
}
public interface ISelectedService<TService>
{
    //TODO: maybe used key services instead
    TService Value { get; }
}
