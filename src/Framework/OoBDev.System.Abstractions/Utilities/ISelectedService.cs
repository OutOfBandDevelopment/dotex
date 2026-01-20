namespace OoBDev.System.Utilities;

public interface ISelectedService<TService> where TService : notnull
{
    TService Value { get; }
}
