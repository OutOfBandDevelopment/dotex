
namespace OoBDev.Communications.Abstractions.Services
{
    public interface IAttributeResolver
    {
        string GetMessageType<T>();
        RequestPriorities GetPriority<T>();
    }
}
